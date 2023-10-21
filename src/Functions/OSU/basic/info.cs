using System.IO;
using KanonBot.API;
using KanonBot.Drivers;
using KanonBot.Functions.OSU;
using KanonBot.Functions.OSU.RosuPP;
using KanonBot.Message;
using LanguageExt.UnsafeValueAccess;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using static LinqToDB.Common.Configuration;

namespace KanonBot.OSU
{
    public static partial class Basic
    {
        public async static Task info(Dictionary<string, string> args, Target target)
        {
            Log.Information("you just called info");
        }
    }
}
