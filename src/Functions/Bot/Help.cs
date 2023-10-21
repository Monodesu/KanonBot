using KanonBot.Drivers;

namespace KanonBot.Bot
{
    public static class Help
    {
        public async static Task Execute(Target target)
        {
            await target.reply(
                """
                用户查询：
                !info/recent/bp
                绑定/用户设置：
                !reg/set
                更多细节请移步 https://desu.life/ 查阅
                """
            );
        }
    }
}