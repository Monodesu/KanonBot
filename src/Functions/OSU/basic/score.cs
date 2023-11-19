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
using System;
using static KanonBot.Functions.OSU.RosuPP.PerformanceCalculator;
using LanguageExt;
using System.Reflection;

namespace KanonBot.OSU
{
    public static partial class Basic
    {

        private enum ScoreEnum
        {
            Recent,
            PassedRecent,
            Best,
            Score
        }

        [Command("best", "bp")]
        [Params("m", "mode", "q", "quality", "u", "user", "username", "i", "index")]
        public async static Task score_best(CommandContext args, Target target)
        {
            var osu_username = "";
            bool isSelfQuery = false;
            bool quality = false;
            API.OSU.Enums.Mode? mode = API.OSU.Enums.Mode.OSU;
            int index = 0;

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
                Some: try_default =>
                {
                    if (IsNumber(try_default))
                    {
                        var x = int.Parse(try_default) - 1;
                        if (x > 100 || x < 0)
                            osu_username = try_default;
                        else
                        {
                            isSelfQuery = true;
                            index = x;
                        }
                    }
                    else
                        osu_username = try_default;
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

            UserPanelData data = new() { userInfo = OnlineOSUUserInfo };
            data.userInfo.PlayMode = OnlineOSUUserInfo.PlayMode;

            await ProcessScore(data, target, ScoreEnum.Best, index, new List<string>(), quality, isSelfQuery);
        }

        [Command("recent", "re", "passed", "pr")]
        [Params("m", "mode", "q", "quality", "u", "user", "username", "i", "index")]
        public async static Task score_recent(CommandContext args, Target target)
        {
            var osu_username = "";
            bool isSelfQuery = false;
            bool quality = false;
            API.OSU.Enums.Mode? mode = API.OSU.Enums.Mode.OSU;
            int index = 0;

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
                Some: try_default =>
                {
                    if (IsNumber(try_default))
                    {
                        var x = int.Parse(try_default) - 1;
                        if (x > 100 || x < 0)
                            osu_username = try_default;
                        else
                        {
                            isSelfQuery = true;
                            index = x;
                        }
                    }
                    else
                        osu_username = try_default;
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

            UserPanelData data = new() { userInfo = OnlineOSUUserInfo };
            data.userInfo.PlayMode = OnlineOSUUserInfo.PlayMode;

            await ProcessScore(data, target, ScoreEnum.Recent, index, new List<string>(), quality, isSelfQuery);
        }

        [Command("passed", "pr")]
        [Params("m", "mode", "q", "quality", "u", "user", "username", "i", "index")]
        public async static Task score_passrecent(CommandContext args, Target target)
        {
            var osu_username = "";
            bool isSelfQuery = false;
            bool quality = false;
            API.OSU.Enums.Mode? mode = API.OSU.Enums.Mode.OSU;
            int index = 0;

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
                Some: try_default =>
                {
                    if (IsNumber(try_default))
                    {
                        var x = int.Parse(try_default) - 1;
                        if (x > 100 || x < 0)
                            osu_username = try_default;
                        else
                        {
                            isSelfQuery = true;
                            index = x;
                        }
                    }
                    else
                        osu_username = try_default;
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

            UserPanelData data = new() { userInfo = OnlineOSUUserInfo };
            data.userInfo.PlayMode = OnlineOSUUserInfo.PlayMode;

            await ProcessScore(data, target, ScoreEnum.PassedRecent, index, new List<string>(), quality, isSelfQuery);
        }

        [Command("score")]
        [Params("m", "mode", "q", "quality", "md", "mods", "u", "user", "username", "b", "beatmap")]
        public async static Task score_specific(CommandContext args, Target target)
        {
            var osu_username = "";
            bool isSelfQuery = false;
            bool quality = false;
            API.OSU.Enums.Mode? mode = API.OSU.Enums.Mode.OSU;
            string mods = "";
            int beatmap = 0;

            // score在查询指定用户成绩时，必须单独指定用户名，不能使用默认值处理
            await args.GetDefault<string>().Match
                (
                Some: async try_default =>
                {
                    if (IsNumber(try_default))
                        beatmap = int.Parse(try_default);
                    else
                    {
                        await target.reply("谱面id有误，请重新检查。");
                        return;
                    }
                },
                None: async () =>
                {
                    await target.reply("没有指定谱面id，请重新检查。");
                    return;
                }
                );
            args.GetParameters<string>(["u", "user", "username"]).Match
                (
                Some: try_username =>
                {
                    osu_username = try_username;
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
            args.GetParameters<string>(["b", "beatmap"]).Match
                (
                Some: async try_beatmap =>
                {
                    if (IsNumber(try_beatmap))
                        beatmap = int.Parse(try_beatmap);
                    else
                    {
                        await target.reply("谱面id有误，请重新检查。");
                        return;
                    }
                },
                None: () => { }
                );

            var (DBUser, DBOsuInfo, OnlineOSUUserInfo) = await GetOSUOperationInfo(target, isSelfQuery, osu_username, mode); // 查詢用戶是否有效（是否綁定，是否存在，osu!用戶是否可用），并返回所有信息
            bool IsBound = DBOsuInfo != null;
            if (OnlineOSUUserInfo == null) return; // 查询失败

            UserPanelData data = new() { userInfo = OnlineOSUUserInfo };
            data.userInfo.PlayMode = OnlineOSUUserInfo.PlayMode;

            // 解析Mod
            List<string> mods_list = new();
            try
            {
                mods_list = Enumerable
                    .Range(0, mods.Length / 2)
                    .Select(p => new string(mods.AsSpan().Slice(p * 2, 2)).ToUpper())
                    .ToList();
            }
            catch { }

            await ProcessScore(data, target, ScoreEnum.Score, beatmap, mods_list, quality, isSelfQuery);
        }

        private async static Task ProcessScore(UserPanelData data, Target target, ScoreEnum se, int index, List<string> mods_list, bool quality, bool isSelfQuery)
        {

            if (se == ScoreEnum.Score)
            {
                var scoreData = await API.OSU.V2.GetUserBeatmapScore(
                    data.userInfo!.Id,
                    index,
                    mods_list.ToArray(),
                    data.userInfo.PlayMode
                    );
                if (scoreData == null)
                {
                    if (isSelfQuery)
                        await target.reply("猫猫没有找到你的成绩");
                    else
                        await target.reply("猫猫没有找到TA的成绩");
                    return;
                }
                //ppy的getscore api不会返回beatmapsets信息，需要手动获取
                var beatmapSetInfo = await API.OSU.V2.GetBeatmapAsync(scoreData!.Score!.Beatmap!.BeatmapId);
                scoreData.Score.Beatmapset = beatmapSetInfo!.Beatmapset;

                var c_data = await PerformanceCalculator.CalculatePanelData(scoreData.Score);
                using var stream = new MemoryStream();
                using var img = await OsuScorePanelV2.Draw(c_data);
                await img.SaveAsync(stream, quality ? new PngEncoder() : new JpegEncoder());
                await target.reply(
                    new Chain().image(
                        Convert.ToBase64String(stream.ToArray(), 0, (int)stream.Length),
                        ImageSegment.Type.Base64
                    )
                );
                img.Dispose();
            }
            else
            {
                bool includeFails = true;
                API.OSU.Enums.UserScoreType op_type;
                switch (se)
                {
                    case ScoreEnum.Recent:
                        op_type = API.OSU.Enums.UserScoreType.Recent;
                        break;
                    case ScoreEnum.PassedRecent:
                        op_type = API.OSU.Enums.UserScoreType.Recent;
                        includeFails = false;
                        break;
                    case ScoreEnum.Best:
                        op_type = API.OSU.Enums.UserScoreType.Best;
                        break;
                    default:
                        return;
                }
                var scores = await API.OSU.V2.GetUserScores(
                    data.userInfo!.Id,
                    op_type,
                    data.userInfo.PlayMode,
                    1,
                    index,
                    includeFails
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
                    using var img = await OsuScorePanelV2.Draw(performance_data);

                    await img.SaveAsync(stream, quality ? new PngEncoder() : new JpegEncoder());
                    await target.reply(
                        new Chain().image(
                            Convert.ToBase64String(stream.ToArray(), 0, (int)stream.Length),
                            ImageSegment.Type.Base64
                        )
                    );
                    img.Dispose();
                }
                else
                {
                    if (isSelfQuery)
                        await target.reply("猫猫没有找到你的成绩");
                    else
                        await target.reply("猫猫没有找到TA的成绩");
                    return;
                }
            }

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

        }
    }
}
