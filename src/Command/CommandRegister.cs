using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KanonBot.Command
{
    public static class CommandRegister
    {
        public static void Register()
        {
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

            Type type = typeof(OSU.Function);

            // 注册所有主指令
            foreach (string Command in CommandList)
            {
                CommandSystem.RegisterCommand(new string[] { Command }, async commandArgs =>
                {
                    await Task.Run(() =>
                    {
                        // 使用反射
                        MethodInfo methodInfo = type.GetMethod(Command, BindingFlags.Static | BindingFlags.Public)!;
                        methodInfo.Invoke(null, null);
                    });
                });
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
                        MethodInfo methodInfo = type.GetMethod(Command, BindingFlags.Static | BindingFlags.Public)!;
                        methodInfo.Invoke(null, null);
                    });
                });
            }


            // 注册三级次要指令
            // badge三级指令


        }
    }
}
