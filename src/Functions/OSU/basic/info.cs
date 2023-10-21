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
            // 指令解析部分
            foreach (var arg in args)
            {
                Log.Information($"Key:{arg.Key} Value:{arg.Value}");
            }
        }
    }
} 
