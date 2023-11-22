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
        [Command("bonuspp")]
        [Params("m", "mode", "u", "user", "username")]
        public async static Task bonuspp(CommandContext args, Target target)
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

            // 计算bonuspp
            if (OnlineOSUUserInfo!.Statistics!.PP == 0)
            {
                await target.reply($"你最近还没有玩过{OnlineOSUUserInfo.PlayMode.ToStr()}模式呢。。");
                return;
            }
            // 因为上面确定过模式，这里就直接用userdata里的mode了
            var allBP = await API.OSU.V2.GetUserScores(
                OnlineOSUUserInfo.Id,
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
            if (allBP!.Length == 0)
            {
                await target.reply("这个模式你还没有成绩呢..");
                return;
            }
            double scorePP = 0.0,
                bounsPP = 0.0,
                pp = 0.0,
                sumOxy = 0.0,
                sumOx2 = 0.0,
                avgX = 0.0,
                avgY = 0.0,
                sumX = 0.0;
            List<double> ys = new();
            for (int i = 0; i < allBP.Length; ++i)
            {
                var tmp = allBP[i].PP * Math.Pow(0.95, i);
                scorePP += tmp;
                ys.Add(Math.Log10(tmp) / Math.Log10(100));
            }
            // calculateLinearRegression
            for (int i = 1; i <= ys.Count; ++i)
            {
                double weight = Utils.log1p(i + 1.0);
                sumX += weight;
                avgX += i * weight;
                avgY += ys[i - 1] * weight;
            }
            avgX /= sumX;
            avgY /= sumX;
            for (int i = 1; i <= ys.Count; ++i)
            {
                sumOxy += (i - avgX) * (ys[i - 1] - avgY) * Utils.log1p(i + 1.0);
                sumOx2 += Math.Pow(i - avgX, 2.0) * Utils.log1p(i + 1.0);
            }
            double Oxy = sumOxy / sumX;
            double Ox2 = sumOx2 / sumX;
            // end
            var b = new double[] { avgY - (Oxy / Ox2) * avgX, Oxy / Ox2 };
            for (double i = 100; i <= OnlineOSUUserInfo.Statistics.PlayCount; ++i)
            {
                double val = Math.Pow(100.0, b[0] + b[1] * i);
                if (val <= 0.0)
                {
                    break;
                }
                pp += val;
            }
            scorePP += pp;
            bounsPP = OnlineOSUUserInfo.Statistics.PP - scorePP;
            int totalscores =
                OnlineOSUUserInfo.Statistics.GradeCounts!.A
                + OnlineOSUUserInfo.Statistics.GradeCounts.S
                + OnlineOSUUserInfo.Statistics.GradeCounts.SH
                + OnlineOSUUserInfo.Statistics.GradeCounts.SS
                + OnlineOSUUserInfo.Statistics.GradeCounts.SSH;
            bool max;
            if (totalscores >= 25397 || bounsPP >= 416.6667)
                max = true;
            else
                max = false;
            int rankedScores = max
                ? Math.Max(totalscores, 25397)
                : (int)Math.Round(Math.Log10(-(bounsPP / 416.6667) + 1.0) / Math.Log10(0.9994));
            if (double.IsNaN(scorePP) || double.IsNaN(bounsPP))
            {
                scorePP = 0.0;
                bounsPP = 0.0;
                rankedScores = 0;
            }
            var str =
                $"{OnlineOSUUserInfo.Username} ({OnlineOSUUserInfo.PlayMode.ToStr()})\n"
                + $"总PP：{OnlineOSUUserInfo.Statistics.PP:0.##}pp\n"
                + $"原始PP：{scorePP:0.##}pp\n"
                + $"Bonus PP：{bounsPP:0.##}pp\n"
                + $"共计算出 {rankedScores} 个被记录的ranked谱面成绩。";
            await target.reply(str);
        }
    }
}
