using KanonBot.Drivers;
using KanonBot.Message;
using KanonBot.API;

namespace KanonBot.functions.osubot
{
    public class Help
    {
        public static void Execute(Target target, string cmd)
        {
            target.reply("请移步 https://info.desu.life/?p=5 查阅");
        }
    }
}
