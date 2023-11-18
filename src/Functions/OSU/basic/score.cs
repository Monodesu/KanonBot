using System.CommandLine;
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
using static KanonBot.BindService;
using static System.Runtime.CompilerServices.RuntimeHelpers;
using static KanonBot.API.OSU.DataStructure;
using SixLabors.ImageSharp;
using KanonBot.Image.OSU;

namespace KanonBot.OSU
{
    public static partial class Basic
    {

        [Command("score", "best", "bp", "recent", "re", "passed", "pr")]
        [Params("m", "mode", "q", "quality", "md", "mods", "u", "username", "i", "index")]
        public async static Task score(CommandContext args, Target target)
        {
            var osu_username = "";
            bool isSelfQuery = false;
            bool quality = false;
            API.OSU.Enums.Mode? mode = API.OSU.Enums.Mode.OSU;
            string mods = "";
            int index = 0;

            args.GetDefault<string>().Match
                (
                Some: try_name =>
                {
                    osu_username = try_name;
                },
                None: () =>
                {
                    isSelfQuery = true;
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

            args.GetParameters<string>(["md", "mods"]).Match
                (
                Some: try_mods =>
                {
                    mods = try_mods;
                },
                None: () => { }
                );
            args.GetParameters<string>(["q", "quality"]).Match
                (
                Some: try_quality =>
                {
                    if (try_quality == "high" || try_quality == "h")
                        quality = true;
                },
                None: () => { });
            args.GetParameters<string>(["i", "index"]).Match
                (
                Some: try_index =>
                {
                    index = int.Parse(try_index) - 1;
                },
                None: () => { }
                );

            var (DBUser, DBOsuInfo, OnlineOSUUserInfo) = await GetOSUOperationInfo(target, isSelfQuery, osu_username, mode); // 查詢用戶是否有效（是否綁定，是否存在，osu!用戶是否可用），并返回所有信息
            bool IsBound = DBOsuInfo != null;
            if (OnlineOSUUserInfo == null) return; // 查询失败


            // 操作部分

            Log.Information(
                $"""

                osu!status
                username: {OnlineOSUUserInfo!.Username}
                osu_uid: {OnlineOSUUserInfo.Id}
                osu_mode: {OnlineOSUUserInfo.PlayMode}
                osu_pp: {OnlineOSUUserInfo!.Statistics!.PP}
                """
                );


            UserPanelData data = new() { userInfo = OnlineOSUUserInfo };
            data.userInfo.PlayMode = OnlineOSUUserInfo.PlayMode;


            // scores

            var scores = await API.OSU.V2.GetUserScores(
                OnlineOSUUserInfo.Id,
                API.OSU.Enums.UserScoreType.Best,
                mode!.Value,
                1,
                index
            );
            if (scores == null)
            {
                await target.reply("查询成绩时出错。");
                return;
            }

            if (scores!.Length > 0)
            {
                var performance_data = await PerformanceCalculator.CalculatePanelData(scores![0]);
                using var stream = new MemoryStream();

                //using var img = (Config.inner != null && Config.inner.debug) ? await DrawV3.OsuScorePanelV3.Draw(performance_data) : await LegacyImage.Draw.DrawScore(performance_data);
                using var img = await OsuScorePanelV2.Draw(performance_data);

                await img.SaveAsync(stream, quality ? new PngEncoder() : new JpegEncoder());
                //await target.reply(
                //    new Chain().image(
                //        Convert.ToBase64String(stream.ToArray(), 0, (int)stream.Length),
                //        ImageSegment.Type.Base64
                //    )
                //);


                await img.SaveAsPngAsync("./temp/2.png", new PngEncoder());
                // 关闭流
                img.Dispose();


                //if (scores![0].Mode == API.OSU.Enums.Mode.OSU)
                //{
                //    if (
                //        scores[0].Beatmap!.Status == API.OSU.Enums.Status.ranked
                //        || scores[0].Beatmap!.Status == API.OSU.Enums.Status.approved
                //    )
                //    {
                //        await Database.Client.InsertOsuStandardBeatmapTechData(
                //            scores[0].Beatmap!.BeatmapId,
                //            performance_data.ppInfo.star,
                //            (int)performance_data.ppInfo.ppStats![0].total,
                //            (int)performance_data.ppInfo.ppStats![0].acc!,
                //            (int)performance_data.ppInfo.ppStats![0].speed!,
                //            (int)performance_data.ppInfo.ppStats![0].aim!,
                //            (int)performance_data.ppInfo.ppStats![1].total,
                //            (int)performance_data.ppInfo.ppStats![2].total,
                //            (int)performance_data.ppInfo.ppStats![3].total,
                //            (int)performance_data.ppInfo.ppStats![4].total,
                //            scores[0].Mods!
                //        );
                //    }
                //}
            }
            else
            {
                await target.reply("猫猫找不到该BP。");
                return;
            }







            //await target.reply(
            //    new Chain().image(
            //        Convert.ToBase64String(stream.ToArray(), 0, (int)stream.Length),
            //        ImageSegment.Type.Base64
            //    )
            //);
            //try
            //{
            //    if (data.userInfo.PlayMode == API.OSU.Enums.Mode.OSU) //只存std的
            //        if (allBP!.Length > 0)
            //            await InsertBeatmapTechInfo(allBP);
            //        else
            //        {
            //            allBP = await API.OSU.GetUserScores(
            //            data.userInfo.Id,
            //            API.OSU.Enums.UserScoreType.Best,
            //            API.OSU.Enums.Mode.OSU,
            //            100,
            //            0
            //        );
            //            if (allBP!.Length > 0)
            //                await InsertBeatmapTechInfo(allBP);
            //        }
            //}
            //catch { }

            await Task.CompletedTask;
        }
    }
}
