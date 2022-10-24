using KanonBot.Drivers;
using KanonBot.Message;
using KanonBot.API;

namespace KanonBot.functions.osubot
{
    public class Help
    {
        public static void Execute(Target target, string cmd)
        {
            target.reply("用户查询：\n!info/recent/bp/get\n绑定/用户设置：\n!reg/set\n更多细节请移步 https://info.desu.life/?p=383 查阅");
        }
    }
}
