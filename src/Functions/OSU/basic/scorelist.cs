using System.CommandLine;
using System.IO;
using KanonBot.API;
using KanonBot.API.OSU;
using KanonBot.Command;
using KanonBot.Drivers;
using KanonBot.Functions.OSU;
using KanonBot.Image.OSU;
using KanonBot.Message;
using LanguageExt;
using LanguageExt.ClassInstances;
using LanguageExt.UnsafeValueAccess;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using static KanonBot.API.OSU.DataStructure;
using static KanonBot.BindService;
using static LinqToDB.Common.Configuration;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace KanonBot.OSU
{
    public static partial class Basic
    {
        [Command("bplist")]
        [Params("m", "mode", "u", "user", "username")]
        public static async Task bplist(CommandContext args, Target target)
        {
            var osu_username = "";
            bool isSelfQuery = false;
            API.OSU.Enums.Mode? mode = API.OSU.Enums.Mode.OSU;
            int startAt = 1,
                endAt = 10;
            bool shouldContinue = true;

            args.GetParameters<string>([ "u", "user", "username" ])
                .Match(
                    Some: try_username =>
                    {
                        osu_username = try_username;
                    },
                    None: () => { }
                );
            await args.GetDefault<string>()
                .Match(
                    Some: async try_range =>
                    {
                        // 尝试解析范围
                        if (try_range.Contains('-'))
                        {
                            var range = try_range.Split('-');
                            if (range.Length == 2)
                            {
                                if (!IsNumber(range[0]) || !IsNumber(range[1]))
                                {
                                    await target.reply("请指定范围。");
                                    shouldContinue = false;
                                    return;
                                }
                                startAt = int.Parse(range[0]);
                                endAt = int.Parse(range[1]);
                                if (
                                    startAt > 100
                                    || startAt < 1
                                    || endAt < 1
                                    || endAt > 100
                                    || startAt > endAt
                                )
                                {
                                    await target.reply("范围不正确，请重新检查。");
                                    shouldContinue = false;
                                    return;
                                }
                            }
                            else
                            {
                                await target.reply("范围不正确，请重新检查。");
                                shouldContinue = false;
                                return;
                            }
                        }
                        else
                        {
                            if (!IsNumber(try_range))
                            {
                                await target.reply("范围不正确，请重新检查。");
                                shouldContinue = false;
                                return;
                            }
                            endAt = int.Parse(try_range);
                            if (endAt > 100 || endAt < 1)
                            {
                                await target.reply("范围不正确，请重新检查。");
                                shouldContinue = false;
                                return;
                            }
                        }
                    },
                    None: async () =>
                    {
                        await target.reply("请指定范围。");
                        shouldContinue = false;
                        return;
                    }
                );
            args.GetParameters<string>([ "m", "mode" ])
                .Match(
                    Some: try_mode =>
                    {
                        mode = API.OSU.Enums.String2Mode(try_mode) ?? API.OSU.Enums.Mode.OSU;
                    },
                    None: () => { }
                );

            if (!shouldContinue)
                return;
            var (DBUser, DBOsuInfo, OnlineOSUUserInfo) = await GetOSUOperationInfo(
                target,
                isSelfQuery,
                osu_username,
                mode
            ); // 查詢用戶是否有效（是否綁定，是否存在，osu!用戶是否可用），并返回所有信息
            bool IsBound = DBOsuInfo != null;
            if (OnlineOSUUserInfo == null)
                return; // 查询失败

            var allBP = await API.OSU
                .V2
                .GetUserScores(
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

            List<API.OSU.Models.Score> TBP = new();
            List<int> Rank = new();
            for (int i = startAt - 1; i < (allBP.Length > endAt ? endAt : allBP.Length); ++i)
                TBP.Add(allBP[i]);
            for (int i = startAt - 1; i < (allBP.Length > endAt ? endAt : allBP.Length); ++i)
                Rank.Add(i + 1);
            if (TBP.Count == 0)
            {
                if (osu_username == "")
                    await target.reply($"你在 {OnlineOSUUserInfo.PlayMode.ToStr()} 模式上还没有bp呢。。");
                else
                    await target.reply(
                        $"{OnlineOSUUserInfo.Username} 在 {OnlineOSUUserInfo.PlayMode} 模式上还没有bp呢。。"
                    );
            }
            else
            {
                using var image = await OsuScoreList.Draw(
                    OsuScoreList.Type.BPLIST,
                    TBP,
                    Rank,
                    OnlineOSUUserInfo
                );
                using var stream = new MemoryStream();
                await image.SaveAsync(stream, new PngEncoder());
                await target.reply(
                    new Chain().image(
                        Convert.ToBase64String(stream.ToArray(), 0, (int)stream.Length),
                        ImageSegment.Type.Base64
                    )
                );
            }
        }
    }
}
