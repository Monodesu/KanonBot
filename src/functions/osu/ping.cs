using KanonBot.Drivers;
using KanonBot.Message;
using KanonBot.API;

namespace KanonBot.functions.osubot
{
    public class Ping
    {
        public static void Execute(Target target, string cmd)
        {
            target.reply("meow~");
        }
    }
}
