using KanonBot.Drivers;
using LanguageExt.Common;
using LinqToDB.Extensions;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;

namespace KanonBot.Command
{
    public class CommandNode(string name, Func<CommandContext, Target, Task>? asyncAction = null)
    {
        public string Name { get; } = name;
        public Dictionary<string, CommandNode> SubCommands { get; } =
            new Dictionary<string, CommandNode>(StringComparer.OrdinalIgnoreCase);
        public Func<CommandContext, Target, Task>? AsyncAction { get; set; } = asyncAction;

        public void AddSubCommand(CommandNode subCommand)
        {
            SubCommands[subCommand.Name] = subCommand;
        }
    }

    public class CommandContext
    {
        public Dictionary<string, object> Parameters { get; }

        public CommandContext(Dictionary<string, object> parameters)
        {
            Parameters = parameters;
        }

        public CommandContext()
        {
            Parameters = new();
        }

        public Option<T> GetParameter<T>(string name)
        {
            if (Parameters.TryGetValue(name, out var value))
            {
                if (value is T v)
                {
                    return v;
                }

                if (typeof(T).IsIntegerType())
                {
                    if (value is string s)
                    {
                        if (int.TryParse(s, out int result))
                        {
                            return (T)Convert.ChangeType(result, typeof(T));
                        }
                    }
                }
            }
            return None;
        }

        public Option<T> GetDefault<T>() => GetParameter<T>("default");
    }

    public static class CommandSystem
    {
        public class ReduplicateTargetChecker
        {
            private ConcurrentDictionary<(string sender, string msg), Target> CommandList = new();

            public bool TryLock(Target target)
            {
                return CommandList.TryAdd((target.sender!, target.msg.ToString()), target);
            }

            public bool Contains(Target target)
            {
                return CommandList.ContainsKey((target.sender!, target.msg.ToString()));
            }

            public void Unlock(Target target)
            {
                CommandList.TryRemove((target.sender!, target.msg.ToString()), out _);
            }
        }

        public static ReduplicateTargetChecker reduplicateTargetChecker = new();

        private static Dictionary<string, CommandNode> commands =
            new(StringComparer.OrdinalIgnoreCase);

        public static void RegisterCommandFromRegistry(CommandRegistry reg)
        {
            foreach (
                KeyValuePair<string, Func<CommandContext, Target, Task>> s in reg.commandHandlers
            )
            {
                var hierarchy = s.Key.Split(' ');
                var method = s.Value;
                var commandName = hierarchy[^1];
                var currentLevel = commands;
                CommandNode? currentNode = null;

                foreach (var level in hierarchy)
                {
                    if (!currentLevel.TryGetValue(level, out currentNode))
                    {
                        currentNode = new CommandNode(level);
                        currentLevel[level] = currentNode;
                    }

                    currentLevel = currentNode.SubCommands;
                }

                currentNode!.AsyncAction = s.Value;
            }
        }

        public static void RegisterCommand(
            string hierarchy,
            Func<CommandContext, Target, Task> action
        )
        {
            RegisterCommand(new string[] { hierarchy }, action);
        }

        public static void RegisterCommand(
            string[] hierarchy,
            Func<CommandContext, Target, Task> action
        )
        {
            if (hierarchy.Length == 0)
            {
                throw new ArgumentException("Hierarchy cannot be empty.", nameof(hierarchy));
            }

            var currentLevel = commands;
            CommandNode? currentNode = null;

            foreach (var level in hierarchy)
            {
                if (!currentLevel.TryGetValue(level, out currentNode))
                {
                    currentNode = new CommandNode(level);
                    currentLevel[level] = currentNode;
                }

                currentLevel = currentNode.SubCommands;
            }

            currentNode!.AsyncAction = action;
        }

        public static async Task ProcessCommand(Target target)
        {
            // 标记消息中可能参数的前后空格
            var sb = target.msg.ToString().ToCharArray();
            for (int i = 0; i < sb.Length; i++)
            {
                if (sb[i] == '=')
                {
                    // 标记前端空格
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (sb[j] == ' ')
                        {
                            sb[j] = '÷';
                        }
                        else
                        {
                            break;
                        }
                    }
                    // 标记后端空格
                    for (int j = i + 1; j < sb.Length; j++)
                    {
                        if (sb[j] == ' ')
                        {
                            sb[j] = '÷';
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            // 处理空格
            var msg = new string(sb).Replace("÷", "");

            // 分割消息
            var parts = msg.Split(' ');

            if (parts.Length == 0) //防止报错
            {
                // 不要将此消息返回给用户
                Log.Warning("Unknown command.");
                return;
            }

            var commandPrefix = parts[0].TrimStart('/', '!', '@');

            // 如果找不到主命令，立即返回
            if (!commands.TryGetValue(commandPrefix, out var currentCommand))
            {
                // 不要将此消息返回给用户
                Log.Warning("Unknown command.");
                return;
            }

            // 上锁
            if (!reduplicateTargetChecker.TryLock(target))
            {
                // 如果已经有相同的命令在处理，忽略
                return;
            }

            try
            {
                var context = new CommandContext();
                int currentPartIndex = 1; // 跳过命令前缀

                // 尝试找到可能的子命令或参数
                for (; currentPartIndex < parts.Length; currentPartIndex++)
                {
                    var currentPart = parts[currentPartIndex];

                    if (
                        currentCommand.SubCommands.Count > 0
                        && currentCommand.SubCommands.TryGetValue(currentPart, out var subCommand)
                    )
                    {
                        // 如果找到了一个子命令，沿着子命令继续
                        currentCommand = subCommand;
                    }
                    else
                    {
                        // 如果没有找到，则剩下的部分是参数
                        break;
                    }
                }

                // 解析参数
                string? currentKey = null;
                string currentValue = "";

                // 解析参数
                for (; currentPartIndex < parts.Length; currentPartIndex++)
                {
                    var part = parts[currentPartIndex];
                    if (part.Contains('='))
                    {
                        if (currentKey != null)
                        {
                            context.Parameters[currentKey] = currentValue.Trim();
                            currentValue = "";
                        }

                        var keyValuePair = part.Split(new[] { '=' }, 2);
                        currentKey = keyValuePair[0];
                        currentValue = keyValuePair.Length > 1 ? keyValuePair[1] : part;
                    }
                    else
                    {
                        // 如果没有键，使用默认键
                        currentKey ??= "default";
                        currentValue += (string.IsNullOrEmpty(currentValue) ? "" : " ") + part;
                    }
                }

                if (currentKey != null)
                {
                    context.Parameters[currentKey] = currentValue.Trim();
                }

                // 执行
                if (currentCommand.AsyncAction != null)
                {
                    await currentCommand.AsyncAction(context, target);
                }
                else
                {
                    // 没有与此命令关联的动作，记录，一般不会出现这个情况
                    Log.Warning($"No action associated with the command: {currentCommand.Name}");
                }
            }
            finally
            {
                // 执行完毕，解锁
                reduplicateTargetChecker.Unlock(target);
            }
        }
    }
}
