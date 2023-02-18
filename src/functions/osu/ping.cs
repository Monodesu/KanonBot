using KanonBot.Drivers;
using KanonBot.Message;
using KanonBot.API;

namespace KanonBot.functions.osubot
{
    public class Ping
    {
        public async static Task Execute(Target target)
        {
            await target.reply("meow~");
        }
    }
}
