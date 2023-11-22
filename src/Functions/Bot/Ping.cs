using KanonBot.Drivers;

namespace KanonBot.Bot
{
    public static class Ping
    {
        public static async Task Execute(Target target)
        {
            await target.reply("meow~");
        }
    }
}
