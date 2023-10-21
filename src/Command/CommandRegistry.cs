using KanonBot.Drivers;
using System.CommandLine;
using System.Data;
using System.Reflection;

namespace KanonBot.Command
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CommandAttribute : Attribute
    {
        public string CommandName { get; }

        public CommandAttribute(string commandName)
        {
            CommandName = commandName;
        }
    }

    public class CommandRegistry
    {
        private readonly Dictionary<string, Func<CommandContext, Target, Task>> _commandHandlers = new();

        // 通过反射扫描带有 Command 特性的方法，并将它们注册为命令处理程序
        public void RegisterCommandsInAssembly(Assembly assembly)
        {
            // 获取所有 public 类型
            var types = assembly.GetTypes().Where(t => t.IsClass && t.IsPublic);

            foreach (var type in types)
            {
                // 获取这些类型中所有 public 方法
                var methods = type.GetMethods(
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static
                );

                foreach (var method in methods)
                {
                    // 如果此方法有 Command 特性，则注册它
                    var commandAttr = method.GetCustomAttribute<CommandAttribute>();
                    if (commandAttr != null)
                    {
                        var instance = method.IsStatic
                        ? null
                        : Activator.CreateInstance(method.DeclaringType!);
                        _commandHandlers[commandAttr.CommandName] = (Func<CommandContext, Target, Task>)Delegate.CreateDelegate(typeof(Func<CommandContext, Target, Task>), instance, method);
                        Console.Error.WriteLine(commandAttr.CommandName);
                    }
                }
            }
        }

        // 调用对应的命令处理程序
        public async Task HandleCommand(string commandName, CommandContext context, Target target)
        {
            if (_commandHandlers.TryGetValue(commandName, out var method))
            {
                // 这里简化了实例创建的过程，真正的场景可能更复杂
                await method.Invoke(context, target);
            }
            else
            {
                Console.WriteLine($"No command handler registered for: {commandName}");
            }
        }

        public Dictionary<string, Func<CommandContext, Target, Task>> commandHandlers => _commandHandlers;
    }
}
