using System.CommandLine;
using System.IO;
using KanonBot.API;
using KanonBot.Command;
using KanonBot.Drivers;
using KanonBot.Functions.OSU;

using KanonBot.Message;
using LanguageExt.UnsafeValueAccess;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using static LinqToDB.Common.Configuration;
using static KanonBot.BindService;
using static System.Runtime.CompilerServices.RuntimeHelpers;
using static KanonBot.API.OSU.DataStructure;
using SixLabors.ImageSharp;
using KanonBot.API.OSU;

namespace KanonBot.OSU
{
    public static partial class Basic
    {
        [Command("bpht")]
        [Params("m", "mode", "u", "user", "username")]
        public async static Task bpht(CommandContext args, Target target)
        {
            var osu_username = "";
            bool isSelfQuery = false;
            API.OSU.Enums.Mode? mode = API.OSU.Enums.Mode.OSU;

            args.GetParameters<string>(["u", "user", "username"]).Match
                (
                Some: try_username =>
                {
                    osu_username = try_username;
                },
                None: () => { }
                );
            args.GetDefault<string>().Match
                (
                Some: try_name =>
                {
                    osu_username = try_name;
                },
                None: () =>
                {
                    if (osu_username == "") isSelfQuery = true;
                }
                );
            args.GetParameters<string>(["m", "mode"]).Match
                (
                Some: try_mode =>
                {
                    mode = API.OSU.Enums.String2Mode(try_mode) ?? API.OSU.Enums.Mode.OSU;
                },
                None: () => { }
                );


            var (DBUser, DBOsuInfo, OnlineOSUUserInfo) = await GetOSUOperationInfo(target, isSelfQuery, osu_username, mode); // 查詢用戶是否有效（是否綁定，是否存在，osu!用戶是否可用），并返回所有信息
            bool IsBound = DBOsuInfo != null;
            if (OnlineOSUUserInfo == null) return; // 查询失败

            var allBP = await API.OSU.V2.GetUserScores(
                OnlineOSUUserInfo!.Id,
                API.OSU.Enums.UserScoreType.Best,
                mode!.Value,
                100,
                0
                );
            if (allBP == null)
            {
                await target.reply("查询成绩时出错。");
                return;
            }
            double totalPP = 0;
            // 如果bp数量小于10则取消
            if (allBP!.Length < 10)
            {
                if (osu_username == "")
                    await target.reply("你的bp太少啦，多打些吧");
                else
                    await target.reply($"{OnlineOSUUserInfo.Username}的bp太少啦，请让ta多打些吧");
                return;
            }
            foreach (var item in allBP)
            {
                totalPP += item.PP;
            }
            var last = allBP.Length;
            var str =
                $"{OnlineOSUUserInfo.Username} 在 {OnlineOSUUserInfo.PlayMode.ToStr()} 模式中:"
                + $"\n你的 bp1 有 {allBP[0].PP:0.##}pp"
                + $"\n你的 bp2 有 {allBP[1].PP:0.##}pp"
                + $"\n..."
                + $"\n你的 bp{last - 1} 有 {allBP[last - 2].PP:0.##}pp"
                + $"\n你的 bp{last} 有 {allBP[last - 1].PP:0.##}pp"
                + $"\n你 bp1 与 bp{last} 相差了有 {allBP[0].PP - allBP[last - 1].PP:0.##}pp"
                + $"\n你的 bp 榜上所有成绩的平均值为 {totalPP / allBP.Length:0.##}pp";
            await target.reply(str);


        }
    }
}
