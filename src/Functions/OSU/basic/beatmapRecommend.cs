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
using KanonBot.API.OSU;

namespace KanonBot.OSU
{
    public static partial class Basic
    {
        [Command("recommend")]
        [Params("m", "mode", "u", "user", "username", "mods", "md")]
        public async static Task beatmapRecommend(CommandContext args, Target target)
        {
            var osu_username = "";
            bool isSelfQuery = false;
            int normal_range = 20;
            int NFEZHT_range = 60;
            string mods_string = "";

            API.OSU.Enums.Mode? mode = API.OSU.Enums.Mode.OSU;

            args.GetParameters<string>(["u", "user", "username"]).Match
                (
                Some: try_username =>
                {
                    osu_username = try_username;
                },
                None: () => { }
                );
            args.GetParameters<string>(["mods", "md"]).Match
                (
                Some: try_mods =>
                {
                    mods_string = try_mods;
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
            // only support osu!std for now

            //args.GetParameters<string>(["m", "mode"]).Match
            //    (
            //    Some: try_mode =>
            //    {
            //        mode = API.OSU.Enums.String2Mode(try_mode) ?? API.OSU.Enums.Mode.OSU;
            //    },
            //    None: () => { }
            //    );


            var (DBUser, DBOsuInfo, OnlineOSUUserInfo) = await GetOSUOperationInfo(target, isSelfQuery, osu_username, mode); // 查詢用戶是否有效（是否綁定，是否存在，osu!用戶是否可用），并返回所有信息
            bool IsBound = DBOsuInfo != null;
            if (OnlineOSUUserInfo == null) return; // 查询失败


            //获取前50bp
            var allBP = await API.OSU.V2.GetUserScores(
                OnlineOSUUserInfo.Id,
                API.OSU.Enums.UserScoreType.Best,
                API.OSU.Enums.Mode.OSU,
                20,
                0
            );
            if (allBP == null)
            {
                await target.reply("打过的图太少了，多玩一玩再来寻求推荐吧~");
                return;
            }
            if (allBP.Length < 20)
            {
                await target.reply("打过的图太少了，多玩一玩再来寻求推荐吧~");
                return;
            }

            //从数据库获取相似的谱面
            var randBP = allBP![new Random().Next(0, 19)];
            //get stars from rosupp
            var ppinfo = await PerformanceCalculator.CalculatePanelData(randBP);

            var data = new List<Database.Models.OsuStandardBeatmapTechData>();

            //解析mod
            List<string> mods = new();
            try
            {
                mods_string = mods_string.ToLower().Trim();
                mods = Enumerable
                    .Range(0, mods_string.Length / 2)
                    .Select(p => new string(mods_string.AsSpan().Slice(p * 2, 2)).ToUpper())
                    .ToList<string>();
            }
            catch { }

            if (mods.Count == 0)
            {
                //使用bp mod
                mods = randBP.Mods!.ToList();

                bool isDiffReductionMod = false,
                    ez = false,
                    ht = false,
                    nf = false,
                    td = false,
                    so = false,
                    dt = false;
                foreach (var x in mods)
                {
                    var xx = x.ToLower().Trim();
                    if (xx == "nf")
                    {
                        isDiffReductionMod = true;
                        nf = true;
                    }
                    if (xx == "ht")
                    {
                        isDiffReductionMod = true;
                        ht = true;
                    }
                    if (xx == "ez")
                    {
                        isDiffReductionMod = true;
                        ez = true;
                    }
                    if (xx == "td")
                    {
                        isDiffReductionMod = true;
                        td = true;
                    }
                    if (xx == "so")
                    {
                        isDiffReductionMod = true;
                        so = true;
                    }
                    if (xx == "dt" || xx == "nc")
                    {
                        dt = true;
                    }
                }
                data = await Database.Client.GetOsuStandardBeatmapTechData(
                    (int)ppinfo.ppInfo.ppStat.aim!,
                    (int)ppinfo.ppInfo.ppStat.speed!,
                    (int)ppinfo.ppInfo.ppStat.acc!,
                    isDiffReductionMod ? NFEZHT_range : normal_range,
                    dt
                );
                if (data.Count > 0)
                {
                    if (mods.Count == 0)
                    {
                        data.RemoveAll(x => x.mod != "");
                    }
                    else
                    {
                        for (int i = 0; i < mods.Count; i++)
                            if (mods[i].ToUpper() == "NC")
                                mods[i] = "DT";
                        foreach (var xx in mods)
                            data.RemoveAll(x => !x.mod!.Contains(xx));
                        if (!ez)
                            data.RemoveAll(x => x.mod!.IndexOf("EZ") != -1);
                        if (!nf)
                            data.RemoveAll(x => x.mod!.IndexOf("NF") != -1);
                        if (!ht)
                            data.RemoveAll(x => x.mod!.IndexOf("HT") != -1);
                        if (!td)
                            data.RemoveAll(x => x.mod!.IndexOf("TD") != -1);
                        if (!so)
                            data.RemoveAll(x => x.mod!.IndexOf("SO") != -1);
                    }
                }
                else
                {
                    await target.reply("猫猫没办法给你推荐谱面了，当前存入数据库的已经找不到合适的谱面推荐给你了...");
                    return;
                }
            }
            else
            {
                bool isDiffReductionMod = false,
                    ez = false,
                    ht = false,
                    nf = false,
                    td = false,
                    so = false,
                    dt = false;
                foreach (var x in mods)
                {
                    var xx = x.ToLower().Trim();
                    if (xx == "nf")
                    {
                        isDiffReductionMod = true;
                        nf = true;
                    }
                    if (xx == "ht")
                    {
                        isDiffReductionMod = true;
                        ht = true;
                    }
                    if (xx == "ez")
                    {
                        isDiffReductionMod = true;
                        ez = true;
                    }
                    if (xx == "td")
                    {
                        isDiffReductionMod = true;
                        td = true;
                    }
                    if (xx == "so")
                    {
                        isDiffReductionMod = true;
                        so = true;
                    }
                    if (xx == "dt" || xx == "nc")
                    {
                        dt = true;
                    }
                }
                //使用解析到的mod 如果是EZ/HT 需要适当把pprange放宽
                data = await Database.Client.GetOsuStandardBeatmapTechData(
                    (int)ppinfo.ppInfo.ppStat.aim!,
                    (int)ppinfo.ppInfo.ppStat.speed!,
                    (int)ppinfo.ppInfo.ppStat.acc!,
                    isDiffReductionMod ? NFEZHT_range : normal_range,
                    dt
                );

                if (data.Count > 0)
                {
                    for (int i = 0; i < mods.Count; i++)
                        if (mods[i] == "NC")
                            mods[i] = "DT";
                    foreach (var xx in mods)
                        data.RemoveAll(x => !x.mod!.Contains(xx));
                    if (!ez)
                        data.RemoveAll(x => x.mod!.IndexOf("EZ") != -1);
                    if (!nf)
                        data.RemoveAll(x => x.mod!.IndexOf("NF") != -1);
                    if (!ht)
                        data.RemoveAll(x => x.mod!.IndexOf("HT") != -1);
                    if (!td)
                        data.RemoveAll(x => x.mod!.IndexOf("TD") != -1);
                    if (!so)
                        data.RemoveAll(x => x.mod!.IndexOf("SO") != -1);
                }
                else
                {
                    await target.reply("猫猫没办法给你推荐谱面了，当前存入数据库的已经找不到合适的谱面推荐给你了...");
                    return;
                }
            }

            //检查谱面列表长度
            if (data.Count == 0)
            {
                await target.reply("猫猫没办法给你推荐谱面了，当前存入数据库的已经找不到合适的谱面推荐给你了...");
                return;
            }

            //返回
            string msg = $"以下是猫猫给你推荐的谱面：\n";
            int beatmapindex = new Random().Next(0, data.Count - 1);
            string mod = "";
            if (data[beatmapindex].mod != "")
            {
                if (data[beatmapindex].mod!.Contains(','))
                    foreach (var xx in data[beatmapindex].mod!.Split(","))
                        mod += xx;
                else
                    mod += data[beatmapindex].mod!;
            }
            else
                mod += "None";
            msg += $"""
                https://osu.ppy.sh/b/{data[beatmapindex].bid}
                Stars: {data[beatmapindex].stars:0.##*}  Mod: {mod}
                PP Statistics:
                100%: {data[beatmapindex].total}pp  99%: {data[beatmapindex].pp_99acc}pp
                98%: {data[beatmapindex].pp_98acc}pp  97%: {data[beatmapindex].pp_97acc}pp  95%: {data[beatmapindex].pp_95acc}pp
                """;
            await target.reply(msg);
        }
    }
}
