using KanonBot.Drivers;
using System.CommandLine;
using System.Data;
using System.Reflection;

namespace KanonBot.Command
{
    public static class CommandRegister
    {
        public static void Register()
        {
            string[] Test = new string[]
            {
                "info"
            };

            string[] CommandList = new string[]
            {
                "reg","bind",
                "info","information",
                "bp","bestperformance",
                "pr","passedrecent",
                "re","recent",
                "score","set",
                "bonuspp","rolecost",
                "bpht","todaybp",
                "seasonalpass","recommend",
                "mu","profile","badge","bplist",
                "leeway","lc"
            };

            string[] BadgeSubCommandList = new string[]
            {
                "sudo",
                "create",
                "redeem",
            };

            Type PrimaryCommandtype = typeof(OSU.Basic);
            //Type BadgeSecondaryCommandtype = typeof(OSU.Basic);
            //Type BadgeTertiaryCommandtype = typeof(OSU.Basic);

            // 注册所有主指令
            foreach (string Command in Test)
            {
                async Task adapterAction((Dictionary<string, string> args, Target target) parameters)
                {
                    try
                    {
                        // 使用反射
                        MethodInfo methodInfo = PrimaryCommandtype.GetMethod(Command, BindingFlags.Static | BindingFlags.Public)!;
                        if (methodInfo != null)
                        {
                            object[] methodParameters = new object[] { parameters.args, parameters.target };

                            // 如果仅需要 Target 对象，改为以下方式
                            // object[] methodParameters = new object[] { parameters.target };
                            object result = methodInfo.Invoke(null, methodParameters)!;
                            if (result is Task taskResult)
                            {
                                await taskResult;
                            }
                        }
                        else
                        {
                            Log.Error($"Command method not found: {Command}");
                        }
                    }
                    catch (TargetParameterCountException ex)
                    {
                        // 特定于参数计数错误的异常处理
                        Log.Error($"Parameter count mismatch: {ex.StackTrace}");
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"An error occurred: {ex.Message}");
                    }
                }

                // 注册命令时使用新的适配器动作
                CommandSystem.RegisterCommand(new string[] { Command }, adapterAction);
            }

            // 注册二级次要指令
            // badge二级指令
            foreach (string Command in BadgeSubCommandList)
            {
                CommandSystem.RegisterCommand(new string[] { "badge", Command }, async commandArgs =>
                {
                    await Task.Run(() =>
                    {
                        // 使用反射
                        MethodInfo methodInfo = PrimaryCommandtype.GetMethod(Command, BindingFlags.Static | BindingFlags.Public)!;
                        methodInfo.Invoke(null, null);
                    });
                });
            }


            // 注册三级次要指令
            // badge三级指令


        }
    }
}
