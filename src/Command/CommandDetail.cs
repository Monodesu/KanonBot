using KanonBot.Drivers;
using LanguageExt.Common;
using LinqToDB.Extensions;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;

namespace KanonBot.Command
{
    public class CommandNode(
        string name,
        Func<CommandContext, Target, Task>? asyncAction = null,
        ParamsAttribute? paramsAttribute = null
    )
    {
        public string Name { get; } = name;
        public Dictionary<string, CommandNode> SubCommands { get; } =
            new(StringComparer.OrdinalIgnoreCase);
        public Func<CommandContext, Target, Task>? AsyncAction { get; set; } = asyncAction;
        public ParamsAttribute? ParamsAttribute { get; set; } = paramsAttribute;

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
                        if (long.TryParse(s, out var result))
                        {
                            return (T)Convert.ChangeType(result, typeof(T));
                        }
                    }
                }

                if (typeof(T).IsFloatType())
                {
                    if (value is string s)
                    {
                        if (double.TryParse(s, out var result))
                        {
                            return (T)Convert.ChangeType(result, typeof(T));
                        }
                    }
                }
            }
            return None;
        }

        public Option<T> GetParameters<T>(string[] names)
        {
            foreach (var name in names)
            {
                var parm = GetParameter<T>(name);
                if (parm.IsSome)
                {
                    return parm;
                }
            }
            return None;
        }

        public Option<T> GetDefault<T>() => GetParameter<T>("default");
    }
}
