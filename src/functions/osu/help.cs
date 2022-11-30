using KanonBot.Drivers;
using KanonBot.Message;
using KanonBot.API;

namespace KanonBot.functions.osubot
{
    public class Help
    {
        public async static Task Execute(Target target, string cmd)
        {
            await target.reply(
                """
                用户查询：
                !info/recent/bp/get
                绑定/用户设置：
                !reg/set
                更多细节请移步 https://info.desu.life/?p=383 查阅
                """
            );
        }
    }
}
