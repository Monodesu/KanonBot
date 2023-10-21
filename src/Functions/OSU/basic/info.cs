using System.IO;
using KanonBot.API;
using KanonBot.Command;
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
        [Command("info")]
        public async static Task info(CommandContext args, Target target)
        {
            var name = args.GetDefault<string>();
            var mode = args.GetParameter<int>("m");
            Log.Information($"name:{name} mode:{mode}");

            // // 指令解析部分
            // foreach (var arg in args.Parameters)
            // {
            //     Log.Information($"Key:{arg.Key} Value:{arg.Value}");
            // }

            await Task.CompletedTask;
        }
    }
} 
