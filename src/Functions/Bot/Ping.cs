using KanonBot.Drivers;

namespace KanonBot.Bot
{
    public static class Ping
    {
        public async static Task Execute(Target target)
        {
            await target.reply("meow~");
        }
    }
}