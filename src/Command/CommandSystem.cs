using Flurl.Util;
using KanonBot.Drivers;
using KanonBot.Message;
using LanguageExt;
using LanguageExt.ClassInstances;
using Serilog;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace KanonBot.Command
{
    public class CommandNode(string name, Func<Dictionary<string, string>, Task>? asyncAction = null)
    {
        public string Name { get; } = name;
        public Dictionary<string, CommandNode> SubCommands { get; } = new Dictionary<string, CommandNode>(StringComparer.OrdinalIgnoreCase);
        public Func<Dictionary<string, string>, Task>? AsyncAction { get; set; } = asyncAction;

        public void AddSubCommand(CommandNode subCommand)
        {
            SubCommands[subCommand.Name] = subCommand;
        }
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

        private static Dictionary<string, CommandNode> commands = new(StringComparer.OrdinalIgnoreCase);

        public static void RegisterCommand(string[] hierarchy, Func<Dictionary<string, string>, Task> action)
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
            // 分割消息
            var parts = target.msg.ToString().Split(' ');

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
                var args = new Dictionary<string, string>();
                int currentPartIndex = 1; // 跳过命令前缀

                // 尝试找到可能的子命令或参数
                for (; currentPartIndex < parts.Length; currentPartIndex++)
                {
                    var currentPart = parts[currentPartIndex];

                    if (currentCommand.SubCommands.Count > 0 &&
                        currentCommand.SubCommands.TryGetValue(currentPart, out var subCommand))
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

                for (; currentPartIndex < parts.Length; currentPartIndex++)
                {
                    var part = parts[currentPartIndex];

                    if (part.Contains('=')) // 猫猫接下来的参数预计使用等号分割，例：mode=osu
                    {
                        if (currentKey != null)
                        {
                            args[currentKey] = currentValue.Trim();
                            currentValue = "";
                        }

                        var keyValuePair = part.Split(new[] { '=' }, 2);
                        currentKey = keyValuePair[0];
                        currentValue = keyValuePair.Length > 1 ? keyValuePair[1] : "";
                    }
                    else
                    {
                        currentValue += (currentValue == "" ? "" : " ") + part;
                    }
                }

                if (currentKey != null)
                {
                    args[currentKey] = currentValue.Trim();
                }

                // 执行
                if (currentCommand.AsyncAction != null)
                {
                    await currentCommand.AsyncAction(args);
                }
                else
                {
                    // 没有与此命令关联的动作，记录
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


