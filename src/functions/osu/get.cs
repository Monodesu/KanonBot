using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Flurl.Util;
using KanonBot.API;
using KanonBot.Drivers;
using KanonBot.functions.osu;
using KanonBot.functions.osu.rosupp;
using KanonBot.image;
using KanonBot.Message;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp.Formats.Png;
using static KanonBot.API.OSU.Models;
using static KanonBot.API.OSU.Models.PPlusData;

namespace KanonBot.functions.osubot
{
    public class Get
    {
        async public static Task Execute(Target target, string cmd)
        {
            string rootCmd, childCmd = "";
            try
            {
                var tmp = cmd.SplitOnFirstOccurence(" ");
                rootCmd = tmp[0].Trim();
                childCmd = tmp[1].Trim().ToLower();
            }
            catch { rootCmd = cmd; }
            switch (rootCmd.ToLower())
            {
                case "bonuspp":
                    await Bonuspp(target, childCmd);
                    break;
                case "bplist":
                    await BPList(target, childCmd);
                    break;
                case "rolecost":
                    await Rolecost(target, childCmd);
                    break;
                case "bpht":
                    await Bpht(target, childCmd);
                    break;
                case "todaybp":
                    await TodayBP(target, childCmd);
                    break;
                case "seasonalpass":
                    await SeasonalPass(target, childCmd);
                    break;
                case "recommend":
                    await BeatmapRecommend(target, childCmd);
                    break;
                case "mu":
                    await SendProfileLink(target, childCmd);
                    break;
                case "profile":
                    await SendProfileLink(target, childCmd);
                    break;
                default:
                    await target.reply(
                                       """
                                       !get bonuspp
                                            rolecost
                                            bpht
                                            bplist
                                            todaybp
                                            seasonalpass
                                            recommend
                                            mu/profile
                                       """);
                    return;
            }
        }

        async private static Task SendProfileLink(Target target, string cmd)
        {
            #region 验证
            long? osuID = null;
            OSU.Enums.Mode? mode;
            Database.Model.User? DBUser = null;
            Database.Model.UserOSU? DBOsuInfo = null;

            // 解析指令
            var command = BotCmdHelper.CmdParser(cmd, BotCmdHelper.FuncType.Info);
            mode = command.osu_mode;

            // 验证账户
            var AccInfo = Accounts.GetAccInfo(target);
            DBUser = await Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
            if (DBUser == null)
            // { await target.reply("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。"); return; }    // 这里引导到绑定osu
            { await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }
            // 验证账号信息
            var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
            DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
            if (DBOsuInfo == null)
            { await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }

            mode ??= OSU.Enums.String2Mode(DBOsuInfo.osu_mode)!.Value;    // 从数据库解析，理论上不可能错
            osuID = DBOsuInfo.osu_uid;



            // 验证osu信息
            var OnlineOsuInfo = await OSU.GetUser(osuID!.Value, OSU.Enums.Mode.OSU); //取osu模式的值
            if (OnlineOsuInfo == null)
            {
                if (DBOsuInfo != null)
                    await target.reply("被办了。");
                else
                    await target.reply("猫猫没有找到此用户。");
                // 中断查询
                return;
            }
            OnlineOsuInfo.PlayMode = mode!.Value;
            #endregion
            await target.reply($"{OnlineOsuInfo.Username}\nhttps://osu.ppy.sh/u/{OnlineOsuInfo.Id}");
        }

        async private static Task BeatmapRecommend(Target target, string cmd)
        {
            int normal_range = 20;
            int NFEZHT_range = 60;
            //only osu!standard
            #region 验证
            long? osuID = null;
            OSU.Enums.Mode? mode;
            Database.Model.User? DBUser = null;
            Database.Model.UserOSU? DBOsuInfo = null;

            // 解析指令
            var command = BotCmdHelper.CmdParser(cmd, BotCmdHelper.FuncType.Info);
            mode = command.osu_mode;

            // 验证账户
            var AccInfo = Accounts.GetAccInfo(target);
            DBUser = await Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
            if (DBUser == null)
            // { await target.reply("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。"); return; }    // 这里引导到绑定osu
            { await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }
            // 验证账号信息
            var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
            DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
            if (DBOsuInfo == null)
            { await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }

            mode ??= OSU.Enums.String2Mode(DBOsuInfo.osu_mode)!.Value;    // 从数据库解析，理论上不可能错
            osuID = DBOsuInfo.osu_uid;



            // 验证osu信息
            var OnlineOsuInfo = await OSU.GetUser(osuID!.Value, OSU.Enums.Mode.OSU); //取osu模式的值
            if (OnlineOsuInfo == null)
            {
                if (DBOsuInfo != null)
                    await target.reply("被办了。");
                else
                    await target.reply("猫猫没有找到此用户。");
                // 中断查询
                return;
            }
            OnlineOsuInfo.PlayMode = mode!.Value;
            #endregion


            //获取前50bp
            var allBP = await OSU.GetUserScores(
                        OnlineOsuInfo.Id,
                        OSU.Enums.UserScoreType.Best,
                        OSU.Enums.Mode.OSU,
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

            var data = new List<Database.Model.OsuStandardBeatmapTechData>();


            //解析mod
            List<string> mods = new();
            try
            {
                cmd = cmd.ToLower().Trim();
                mods = Enumerable
                    .Range(0, cmd.Length / 2)
                    .Select(p => new string(cmd.AsSpan().Slice(p * 2, 2)).ToUpper())
                    .ToList<string>();
            }
            catch { }

            if (mods.Count == 0)
            {
                //使用bp mod
                mods = randBP.Mods.ToList();

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
                            if (mods[i].ToUpper() == "NC") mods[i] = "DT";
                        foreach (var xx in mods)
                            data.RemoveAll(x => x.mod!.IndexOf(xx) == -1);
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
                        if (mods[i] == "NC") mods[i] = "DT";
                    foreach (var xx in mods)
                        data.RemoveAll(x => x.mod!.IndexOf(xx) == -1);
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
                else mod += data[beatmapindex].mod!;
            }
            else mod += "None";
            msg +=
                $"""
                https://osu.ppy.sh/b/{data[beatmapindex].bid}
                Stars: {data[beatmapindex].stars.ToString("0.##*")}  Mod: {mod}
                PP Statistics:
                100%: {data[beatmapindex].total}pp  99%: {data[beatmapindex].pp_99acc}pp
                98%: {data[beatmapindex].pp_98acc}pp  97%: {data[beatmapindex].pp_97acc}pp  95%: {data[beatmapindex].pp_95acc}pp
                """;
            await target.reply(msg);
        }

        async private static Task Bonuspp(Target target, string cmd)
        {
            #region 验证
            long? osuID = null;
            OSU.Enums.Mode? mode;
            Database.Model.User? DBUser = null;
            Database.Model.UserOSU? DBOsuInfo = null;

            // 解析指令
            var command = BotCmdHelper.CmdParser(cmd, BotCmdHelper.FuncType.Info);
            mode = command.osu_mode;

            // 解析指令
            if (command.self_query)
            {
                // 验证账户
                var AccInfo = Accounts.GetAccInfo(target);
                DBUser = await Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
                if (DBUser == null)
                // { await target.reply("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。"); return; }    // 这里引导到绑定osu
                { await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }
                // 验证账号信息
                var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
                DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
                if (DBOsuInfo == null)
                { await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }

                mode ??= OSU.Enums.String2Mode(DBOsuInfo.osu_mode)!.Value;    // 从数据库解析，理论上不可能错
                osuID = DBOsuInfo.osu_uid;
            }
            else
            {
                // 查询用户是否绑定
                var tempOsuInfo = await OSU.GetUser(command.osu_username, command.osu_mode ?? OSU.Enums.Mode.OSU);
                if (tempOsuInfo != null)
                {
                    DBOsuInfo = await Database.Client.GetOsuUser(tempOsuInfo.Id);
                    if (DBOsuInfo != null)
                    {
                        DBUser = await Accounts.GetAccountByOsuUid(tempOsuInfo.Id);
                        mode ??= OSU.Enums.String2Mode(DBOsuInfo.osu_mode)!.Value;
                    }
                    mode ??= tempOsuInfo.PlayMode;
                    osuID = tempOsuInfo.Id;
                }
                else
                {
                    // 直接取消查询，简化流程
                    await target.reply("猫猫没有找到此用户。");
                    return;
                }
            }

            // 验证osu信息
            var OnlineOsuInfo = await OSU.GetUser(osuID!.Value, mode!.Value);
            if (OnlineOsuInfo == null)
            {
                if (DBOsuInfo != null)
                    await target.reply("被办了。");
                else
                    await target.reply("猫猫没有找到此用户。");
                // 中断查询
                return;
            }
            OnlineOsuInfo.PlayMode = mode!.Value;
            #endregion

            // 计算bonuspp
            if (OnlineOsuInfo!.Statistics.PP == 0)
            {
                await target.reply($"你最近还没有玩过{OnlineOsuInfo.PlayMode.ToStr()}模式呢。。");
                return;
            }
            // 因为上面确定过模式，这里就直接用userdata里的mode了
            var allBP = await OSU.GetUserScores(OnlineOsuInfo.Id, OSU.Enums.UserScoreType.Best, mode!.Value, 100, 0);
            if (allBP == null) { await target.reply("查询成绩时出错。"); return; }
            if (allBP!.Length == 0) { await target.reply("这个模式你还没有成绩呢.."); return; }
            double scorePP = 0.0, bounsPP = 0.0, pp = 0.0, sumOxy = 0.0, sumOx2 = 0.0, avgX = 0.0, avgY = 0.0, sumX = 0.0;
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
            for (double i = 100; i <= OnlineOsuInfo.Statistics.PlayCount; ++i)
            {
                double val = Math.Pow(100.0, b[0] + b[1] * i);
                if (val <= 0.0)
                {
                    break;
                }
                pp += val;
            }
            scorePP += pp;
            bounsPP = OnlineOsuInfo.Statistics.PP - scorePP;
            int totalscores = OnlineOsuInfo.Statistics.GradeCounts.A + OnlineOsuInfo.Statistics.GradeCounts.S + OnlineOsuInfo.Statistics.GradeCounts.SH + OnlineOsuInfo.Statistics.GradeCounts.SS + OnlineOsuInfo.Statistics.GradeCounts.SSH;
            bool max;
            if (totalscores >= 25397 || bounsPP >= 416.6667)
                max = true;
            else
                max = false;
            int rankedScores = max ? Math.Max(totalscores, 25397) : (int)Math.Round(Math.Log10(-(bounsPP / 416.6667) + 1.0) / Math.Log10(0.9994));
            if (double.IsNaN(scorePP) || double.IsNaN(bounsPP))
            {
                scorePP = 0.0;
                bounsPP = 0.0;
                rankedScores = 0;
            }
            var str = $"{OnlineOsuInfo.Username} ({OnlineOsuInfo.PlayMode.ToStr()})\n" +
                $"总PP：{OnlineOsuInfo.Statistics.PP.ToString("0.##")}pp\n" +
                $"原始PP：{scorePP.ToString("0.##")}pp\n" +
                $"Bonus PP：{bounsPP.ToString("0.##")}pp\n" +
                $"共计算出 {rankedScores} 个被记录的ranked谱面成绩。";
            await target.reply(str);
        }

        async private static Task Elo(Target target, string cmd)
        {
            #region 验证
            long? osuID = null;
            OSU.Enums.Mode? mode;
            Database.Model.User? DBUser = null;
            Database.Model.UserOSU? DBOsuInfo = null;

            // 解析指令
            var command = BotCmdHelper.CmdParser(cmd, BotCmdHelper.FuncType.Info);
            mode = command.osu_mode;

            // 解析指令
            if (command.self_query)
            {
                // 验证账户
                var AccInfo = Accounts.GetAccInfo(target);
                DBUser = await Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
                if (DBUser == null)
                // { await target.reply("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。"); return; }    // 这里引导到绑定osu
                { await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }
                // 验证账号信息
                var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
                DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
                if (DBOsuInfo == null)
                { await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }

                mode ??= OSU.Enums.String2Mode(DBOsuInfo.osu_mode)!.Value;    // 从数据库解析，理论上不可能错
                osuID = DBOsuInfo.osu_uid;
            }
            else
            {
                // 查询用户是否绑定
                var tempOsuInfo = await OSU.GetUser(command.osu_username, command.osu_mode ?? OSU.Enums.Mode.OSU);
                if (tempOsuInfo != null)
                {
                    DBOsuInfo = await Database.Client.GetOsuUser(tempOsuInfo.Id);
                    if (DBOsuInfo != null)
                    {
                        DBUser = await Accounts.GetAccountByOsuUid(tempOsuInfo.Id);
                        mode ??= OSU.Enums.String2Mode(DBOsuInfo.osu_mode)!.Value;
                    }
                    mode ??= tempOsuInfo.PlayMode;
                    osuID = tempOsuInfo.Id;
                }
                else
                {
                    // 直接取消查询，简化流程
                    await target.reply("猫猫没有找到此用户。");
                    return;
                }
            }

            // 验证osu信息
            var OnlineOsuInfo = await OSU.GetUser(osuID!.Value, mode!.Value);
            if (OnlineOsuInfo == null)
            {
                if (DBOsuInfo != null)
                    await target.reply("被办了。");
                else
                    await target.reply("猫猫没有找到此用户。");
                // 中断查询
                return;
            }
            OnlineOsuInfo.PlayMode = mode!.Value;
            #endregion

            try
            {
                JObject? eloInfo = await OSU.GetUserEloInfo(OnlineOsuInfo!.Id);
                foreach (var key in eloInfo!)
                {
                    switch (key.Key)
                    {
                        case "code":
                            switch ((int)eloInfo["code"]!)
                            {
                                case 40009:
                                    await target.reply(eloInfo["message"]!.ToString());
                                    break;
                                case 40004:
                                    await target.reply($"{(string)eloInfo["message"]!}\n{OnlineOsuInfo.Username}的初始ELO为: {eloInfo["elo"]}");
                                    break;
                            }
                            return;
                        case "elo":
                            await target.reply($"{OnlineOsuInfo.Username}的ELO为: {eloInfo["elo"]}");
                            return;
                    }
                }
            }
            catch (Exception ex)
            {
                await target.reply($"查询失败, 失败信息: {ex.Message}");
            }
        }

        async private static Task Rolecost(Target target, string cmd)
        {
            cmd = cmd.Trim();
            Func<OSU.Models.User, OSU.Models.PPlusData.UserData, double> occost = (userInfo, pppData) =>
            {
                double a, c, z, p;
                p = userInfo.Statistics.PP;
                z = 1.92 * Math.Pow(pppData.JumpAimTotal, 0.953) + 69.7 * Math.Pow(pppData.FlowAimTotal, 0.596)
                    + 0.588 * Math.Pow(pppData.SpeedTotal, 1.175) + 3.06 * Math.Pow(pppData.StaminaTotal, 0.993);
                a = Math.Pow(pppData.AccuracyTotal, 1.2768) * Math.Pow(p, 0.88213);
                c = Math.Min(0.00930973 * Math.Pow(p / 1000, 2.64192) * Math.Pow(z / 4000, 1.48422), 7) + Math.Min(a / 7554280, 3);
                return Math.Round(c, 2);
            };
            Func<OSU.Models.User, double> oncost = (userInfo) =>
            {
                double fx, pp;
                pp = userInfo.Statistics.PP;
                if (pp <= 4000 && pp >= 2000)
                {
                    fx = Math.Round(Math.Pow(1.00053, pp) - 2.88, 2);
                    return fx;
                }
                else
                {
                    return -1;
                }
            };
            Func<long, int, double> ostcost = (rank, elo) =>
            {
                double rankelo, cost;
                if (elo == 0)
                {
                    elo = (int)(1500 - 600 * (Math.Log((rank + 500) / 8500.0) / Math.Log(4.0)));
                }
                else
                {
                    rankelo = 1500 - 600 * (Math.Log((rank + 500) / 8500.0) / Math.Log(4.0));
                    if (elo > rankelo)
                    {
                        rankelo = elo;
                    }
                    else
                    {
                        elo = (int)(0.8 * rankelo + 0.2 * elo);
                    }
                }
                if (elo > 850)
                {
                    cost = 27 * (elo - 700) / 3200.0;
                }
                else
                {
                    cost = 3 * Math.Pow(((elo - 400) / 600.0), 3);
                    if (cost <= 0)
                    {
                        cost = 0;
                    }
                }
                return Math.Round(cost, 2);
            };

            #region 验证
            long? osuID = null;
            OSU.Enums.Mode? mode;
            Database.Model.User? DBUser = null;
            Database.Model.UserOSU? DBOsuInfo = null;

            // 解析指令
            var command = BotCmdHelper.CmdParser(cmd, BotCmdHelper.FuncType.Info);
            mode = command.osu_mode;

            // 解析指令
            if (command.self_query)
            {
                // 验证账户
                var AccInfo = Accounts.GetAccInfo(target);
                DBUser = await Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
                if (DBUser == null)
                // { await target.reply("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。"); return; }    // 这里引导到绑定osu
                { await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }
                // 验证账号信息
                var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
                DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
                if (DBOsuInfo == null)
                { await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }

                mode ??= OSU.Enums.String2Mode(DBOsuInfo.osu_mode)!.Value;    // 从数据库解析，理论上不可能错
                osuID = DBOsuInfo.osu_uid;
            }
            else
            {
                // 查询用户是否绑定
                var tempOsuInfo = await OSU.GetUser(command.osu_username, command.osu_mode ?? OSU.Enums.Mode.OSU);
                if (tempOsuInfo != null)
                {
                    DBOsuInfo = await Database.Client.GetOsuUser(tempOsuInfo.Id);
                    if (DBOsuInfo != null)
                    {
                        DBUser = await Accounts.GetAccountByOsuUid(tempOsuInfo.Id);
                        mode ??= OSU.Enums.String2Mode(DBOsuInfo.osu_mode)!.Value;
                    }
                    mode ??= tempOsuInfo.PlayMode;
                    osuID = tempOsuInfo.Id;
                }
                else
                {
                    // 直接取消查询，简化流程
                    await target.reply("猫猫没有找到此用户。");
                    return;
                }
            }

            // 验证osu信息
            var OnlineOsuInfo = await OSU.GetUser(osuID!.Value, mode!.Value);
            if (OnlineOsuInfo == null)
            {
                if (DBOsuInfo != null)
                    await target.reply("被办了。");
                else
                    await target.reply("猫猫没有找到此用户。");
                // 中断查询
                return;
            }
            OnlineOsuInfo.PlayMode = mode!.Value;
            #endregion

            switch (cmd)
            {
                case "occ":
                    try
                    {
                        var pppData = await OSU.GetUserPlusData(OnlineOsuInfo.Id);
                        await target.reply($"在猫猫杯S1中，{OnlineOsuInfo.Username} 的cost为：{occost(OnlineOsuInfo, pppData.User)}");
                    }
                    catch { await target.reply($"获取pp+失败"); return; }
                    break;
                case "onc":
                    var onc = oncost(OnlineOsuInfo);
                    if (onc == -1)
                        await target.reply($"{OnlineOsuInfo.Username} 不在参赛范围内。");
                    else
                        await target.reply($"在ONC中，{OnlineOsuInfo.Username} 的cost为：{onc}");
                    break;
                case "ost":
                    try
                    {
                        var eloInfo = await OSU.GetUserEloInfo(OnlineOsuInfo.Id);
                        int elo = 0;
                        foreach (var key in eloInfo!)
                        {
                            switch (key.Key)
                            {
                                case "code":
                                    switch ((int)eloInfo["code"]!)
                                    {
                                        case 40009:
                                            await target.reply(eloInfo["message"]!.ToString());
                                            break;
                                        case 40004:
                                            elo = 0;
                                            break;
                                    }
                                    break;
                                case "elo":
                                    elo = int.Parse(eloInfo["elo"]!.ToString());
                                    break;
                            }
                        }
                        if (elo != 0)
                        {
                            var matchId = await OSU.GetUserEloRecentPlay(OnlineOsuInfo.Id);
                            var body = (await OSU.GetMatchInfo(matchId.Value))!["result"]!.ToObject<JObject>();
                            TimeSpan ts = new();
                            foreach (var item in body!)
                            {
                                var dt = DateTimeOffset.Parse(item.Value!["start_time"]!.ToString());
                                ts = DateTime.Now - dt;
                                break;
                            }
                            if (ts.Days > 365)
                            {
                                elo = 0;
                            }
                        }
                        await target.reply($"在OST中，{OnlineOsuInfo.Username} 的cost为：{ostcost(OnlineOsuInfo.Statistics.GlobalRank, elo)}");
                    }
                    catch { await target.reply($"获取elo失败"); return; }
                    break;
                default:
                    await target.reply($"请输入要查询cost的比赛名称的缩写。");
                    break;
            }
        }

        async private static Task Bpht(Target target, string cmd)
        {
            #region 验证
            long? osuID = null;
            OSU.Enums.Mode? mode;
            Database.Model.User? DBUser = null;
            Database.Model.UserOSU? DBOsuInfo = null;

            // 解析指令
            var command = BotCmdHelper.CmdParser(cmd, BotCmdHelper.FuncType.Info);
            mode = command.osu_mode;

            // 解析指令
            if (command.self_query)
            {
                // 验证账户
                var AccInfo = Accounts.GetAccInfo(target);
                DBUser = await Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
                if (DBUser == null)
                // { await target.reply("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。"); return; }    // 这里引导到绑定osu
                { await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }
                // 验证账号信息
                var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
                DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
                if (DBOsuInfo == null)
                { await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }

                mode ??= OSU.Enums.String2Mode(DBOsuInfo.osu_mode)!.Value;    // 从数据库解析，理论上不可能错
                osuID = DBOsuInfo.osu_uid;
            }
            else
            {
                // 查询用户是否绑定
                var tempOsuInfo = await OSU.GetUser(command.osu_username, command.osu_mode ?? OSU.Enums.Mode.OSU);
                if (tempOsuInfo != null)
                {
                    DBOsuInfo = await Database.Client.GetOsuUser(tempOsuInfo.Id);
                    if (DBOsuInfo != null)
                    {
                        DBUser = await Accounts.GetAccountByOsuUid(tempOsuInfo.Id);
                        mode ??= OSU.Enums.String2Mode(DBOsuInfo.osu_mode)!.Value;
                    }
                    mode ??= tempOsuInfo.PlayMode;
                    osuID = tempOsuInfo.Id;
                }
                else
                {
                    // 直接取消查询，简化流程
                    await target.reply("猫猫没有找到此用户。");
                    return;
                }
            }

            // 验证osu信息
            var OnlineOsuInfo = await OSU.GetUser(osuID!.Value, mode!.Value);
            if (OnlineOsuInfo == null)
            {
                if (DBOsuInfo != null)
                    await target.reply("被办了。");
                else
                    await target.reply("猫猫没有找到此用户。");
                // 中断查询
                return;
            }
            OnlineOsuInfo.PlayMode = mode!.Value;
            #endregion

            var allBP = await OSU.GetUserScores(OnlineOsuInfo!.Id, OSU.Enums.UserScoreType.Best, mode!.Value, 100, 0);
            if (allBP == null) { await target.reply("查询成绩时出错。"); return; }
            double totalPP = 0;
            // 如果bp数量小于10则取消
            if (allBP!.Length < 10)
            {
                if (cmd == "")
                    await target.reply("你的bp太少啦，多打些吧");
                else
                    await target.reply($"{OnlineOsuInfo.Username}的bp太少啦，请让ta多打些吧");
                return;
            }
            foreach (var item in allBP)
            {
                totalPP += item.PP;
            }
            var last = allBP.Length;
            var str = $"{OnlineOsuInfo.Username} 在 {OnlineOsuInfo.PlayMode.ToStr()} 模式中:"
            + $"\n你的 bp1 有 {allBP[0].PP:0.##}pp"
            + $"\n你的 bp2 有 {allBP[1].PP:0.##}pp"
            + $"\n..."
            + $"\n你的 bp{last - 1} 有 {allBP[last - 2].PP:0.##}pp"
            + $"\n你的 bp{last} 有 {allBP[last - 1].PP:0.##}pp"
            + $"\n你 bp1 与 bp{last} 相差了有 {allBP[0].PP - allBP[last - 1].PP:0.##}pp"
            + $"\n你的 bp 榜上所有成绩的平均值为 {totalPP / allBP.Length:0.##}pp";
            await target.reply(str);
        }

        async private static Task TodayBP(Target target, string cmd)
        {
            #region 验证
            long? osuID = null;
            OSU.Enums.Mode? mode;
            Database.Model.User? DBUser = null;
            Database.Model.UserOSU? DBOsuInfo = null;

            // 解析指令
            var command = BotCmdHelper.CmdParser(cmd, BotCmdHelper.FuncType.Info);
            mode = command.osu_mode;

            // 解析指令
            if (command.self_query)
            {
                // 验证账户
                var AccInfo = Accounts.GetAccInfo(target);
                DBUser = await Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
                if (DBUser == null)
                // { await target.reply("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。"); return; }    // 这里引导到绑定osu
                { await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }
                // 验证账号信息
                var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
                DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
                if (DBOsuInfo == null)
                { await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }

                mode ??= OSU.Enums.String2Mode(DBOsuInfo.osu_mode)!.Value;    // 从数据库解析，理论上不可能错
                osuID = DBOsuInfo.osu_uid;
            }
            else
            {
                // 查询用户是否绑定
                var tempOsuInfo = await OSU.GetUser(command.osu_username, command.osu_mode ?? OSU.Enums.Mode.OSU);
                if (tempOsuInfo != null)
                {
                    DBOsuInfo = await Database.Client.GetOsuUser(tempOsuInfo.Id);
                    if (DBOsuInfo != null)
                    {
                        DBUser = await Accounts.GetAccountByOsuUid(tempOsuInfo.Id);
                        mode ??= OSU.Enums.String2Mode(DBOsuInfo.osu_mode)!.Value;
                    }
                    mode ??= tempOsuInfo.PlayMode;
                    osuID = tempOsuInfo.Id;
                }
                else
                {
                    // 直接取消查询，简化流程
                    await target.reply("猫猫没有找到此用户。");
                    return;
                }
            }

            // 验证osu信息
            var OnlineOsuInfo = await OSU.GetUser(osuID!.Value, mode!.Value);
            if (OnlineOsuInfo == null)
            {
                if (DBOsuInfo != null)
                    await target.reply("被办了。");
                else
                    await target.reply("猫猫没有找到此用户。");
                // 中断查询
                return;
            }
            OnlineOsuInfo.PlayMode = mode!.Value;
            #endregion

            var allBP = await OSU.GetUserScores(OnlineOsuInfo!.Id, OSU.Enums.UserScoreType.Best, mode!.Value, 100, 0);
            if (allBP == null) { await target.reply("查询成绩时出错。"); return; }
            List<OSU.Models.Score> TBP = new();
            List<int> Rank = new();

            //test codes
            //foreach (var x in allBP) TBP.Add(x);
            //for (int i = 0; i < allBP.Length; ++i) Rank.Add(i + 1);
            //var image1 = await KanonBot.image.TodaysBP.Draw(TBP.Take(10).ToList(), Rank, OnlineOsuInfo);
            //using var stream1 = new MemoryStream();
            //await image1.SaveAsync(stream1, new PngEncoder());
            //await target.reply(
            //    new Chain().image(
            //    Convert.ToBase64String(stream1.ToArray(), 0, (int)stream1.Length),
            //        ImageSegment.Type.Base64
            //));
            //return;
            //test end


            var t = DateTime.Now.Hour < 4 ? DateTime.Now.Date.AddDays(-1).AddHours(4) : DateTime.Now.Date.AddHours(4);
            for (int i = 0; i < allBP.Length; i++)
            {
                var item = allBP[i];
                var ts = (item.CreatedAt - t).Days;
                if (0 <= ts && ts < 1)
                {
                    TBP.Add(item);
                    Rank.Add(i + 1);
                    //str += $"\n#{i + 1} {item.Rank} {item.Accuracy * 100:0.##}% {item.PP:0.##}pp";
                    //if (item.Mods.Length > 0) str += $" +{string.Join(',', item.Mods)}";
                }
            }
            if (TBP.Count == 0)
            {

                if (cmd == "")
                    await target.reply($"你今天在 {OnlineOsuInfo.PlayMode.ToStr()} 模式上还没有新bp呢。。");
                else
                    await target.reply($"{OnlineOsuInfo.Username} 今天在 {OnlineOsuInfo.PlayMode.ToStr()} 模式上还没有新bp呢。。");
            }
            else
            {
                var image = await KanonBot.image.ScoreList.Draw(ScoreList.Type.TODAYBP, TBP, Rank, OnlineOsuInfo);
                using var stream = new MemoryStream();
                await image.SaveAsync(stream, new PngEncoder());
                await target.reply(
                    new Chain().image(
                    Convert.ToBase64String(stream.ToArray(), 0, (int)stream.Length),
                        ImageSegment.Type.Base64
                ));
                //await target.reply($"{OnlineOsuInfo.Username} 今天在 {OnlineOsuInfo.PlayMode.ToStr()} 模式上新增的BP:" + str);
            }
        }

        async private static Task SeasonalPass(Target target, string cmd)
        {
            #region 验证
            long? osuID = null;
            OSU.Enums.Mode? mode;
            Database.Model.User? DBUser = null;
            Database.Model.UserOSU? DBOsuInfo = null;

            // 解析指令
            var command = BotCmdHelper.CmdParser(cmd, BotCmdHelper.FuncType.Info);
            mode = command.osu_mode;

            // 解析指令

            // 验证账户
            var AccInfo = Accounts.GetAccInfo(target);
            DBUser = await Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
            if (DBUser == null)
            // { await target.reply("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。"); return; }    // 这里引导到绑定osu
            { await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }
            // 验证账号信息
            var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
            DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
            if (DBOsuInfo == null)
            { await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }

            mode ??= OSU.Enums.String2Mode(DBOsuInfo.osu_mode)!.Value;    // 从数据库解析，理论上不可能错
            osuID = DBOsuInfo.osu_uid;


            // 验证osu信息
            var OnlineOsuInfo = await OSU.GetUser(osuID!.Value, mode!.Value);
            if (OnlineOsuInfo == null)
            {
                if (DBOsuInfo != null)
                    await target.reply("被办了。");
                else
                    await target.reply("猫猫没有找到此用户。");
                // 中断查询
                return;
            }
            OnlineOsuInfo.PlayMode = mode!.Value;
            #endregion

            //查询前先更新
            if (DBOsuInfo != null)
                await Seasonalpass.Update(
                    OnlineOsuInfo!.Id,
                    OnlineOsuInfo!.PlayMode!.ToStr(),
                    OnlineOsuInfo.Statistics.TotalHits
                );

            //旧版，将于2023年1月1日弃用
            var seasonalpassinfo = await Database.Client.GetSeasonalPassInfo(OnlineOsuInfo!.Id, OnlineOsuInfo!.PlayMode!.ToStr())!;

            if (seasonalpassinfo == null)
            {
                await target.reply("数据库中无此用户的季票信息，请稍后再试。");
                return;
            }
            //10000tth一级，每升1级所需tth+2000
            long temptth = seasonalpassinfo.tth - seasonalpassinfo.inittth;
            int levelcount = 0;
            while (true)
            {
                temptth = temptth - (10000 + levelcount * 2000);
                if (temptth > 0)
                    levelcount = levelcount + 1;
                else break;
            }
            int tt = 0;
            for (int i = 0; i < levelcount; ++i)
            {
                tt += 10000 + i * 2000;
            }
            double t = Math.Round(
                Math.Round(
                    ((double)((seasonalpassinfo.tth - seasonalpassinfo.inittth - tt) * 100) / (double)(10000 + levelcount * 2000)), 4), 4
            );
            string str;
            str = $"{OnlineOsuInfo.Username}\n自2022年11月29日以来\n您在{OnlineOsuInfo!.PlayMode!.ToStr()}模式下的等级为{levelcount}级 " +
            $"({t}%)" +
            $"\n共击打了{seasonalpassinfo.tth - seasonalpassinfo.inittth}次\n距离升级还需要{Math.Abs(temptth)}tth";
            await target.reply(str);
        }

        async private static Task BPList(Target target, string cmd)
        {
            #region 验证
            long? osuID = null;
            OSU.Enums.Mode? mode;
            Database.Model.User? DBUser = null;
            Database.Model.UserOSU? DBOsuInfo = null;

            // 解析指令
            var command = BotCmdHelper.CmdParser(cmd, BotCmdHelper.FuncType.Info);
            mode = command.osu_mode;

            // 验证账户
            var AccInfo = Accounts.GetAccInfo(target);
            DBUser = await Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
            if (DBUser == null)
            // { await target.reply("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。"); return; }    // 这里引导到绑定osu
            { await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }
            // 验证账号信息
            var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
            DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
            if (DBOsuInfo == null)
            { await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }

            mode ??= OSU.Enums.String2Mode(DBOsuInfo.osu_mode)!.Value;    // 从数据库解析，理论上不可能错
            osuID = DBOsuInfo.osu_uid;


            // 验证osu信息
            var OnlineOsuInfo = await OSU.GetUser(osuID!.Value, mode!.Value);
            if (OnlineOsuInfo == null)
            {
                if (DBOsuInfo != null)
                    await target.reply("被办了。");
                else
                    await target.reply("猫猫没有找到此用户。");
                // 中断查询
                return;
            }
            OnlineOsuInfo.PlayMode = mode!.Value;
            #endregion

            var allBP = await OSU.GetUserScores(OnlineOsuInfo!.Id, OSU.Enums.UserScoreType.Best, mode!.Value, 100, 0);
            if (allBP == null) { await target.reply("查询成绩时出错。"); return; }

            //开始解析命令
            int StartAt = 1;
            int EndAt = 10;
            var tempcmd = cmd.Trim().ToLower();
            if (tempcmd != "")
            {
                if (tempcmd.IndexOf('-') != -1)
                {
                    //指定了范围
                    var t = tempcmd.Split('-');
                    if (!int.TryParse(t[0].Trim(), out StartAt))
                    {
                        await target.reply("指定的范围不正确"); return;
                    }
                    if (!int.TryParse(t[1].Trim(), out EndAt))
                    {
                        await target.reply("指定的范围不正确"); return;
                    }
                }
                else
                {
                    //只指定了最大值
                    if (!int.TryParse(tempcmd, out EndAt))
                    {
                        await target.reply("指定的范围不正确"); return;
                    }
                }

                if (StartAt < 1 || StartAt > 99)
                {
                    await target.reply("指定的范围不正确"); return;
                }
                if (EndAt < 2 || StartAt > 100)
                {
                    await target.reply("指定的范围不正确"); return;
                }
                if (EndAt > StartAt)
                {
                    await target.reply("指定的范围不正确"); return;
                }
            }
            List<OSU.Models.Score> TBP = new();
            List<int> Rank = new();
            for (int i = StartAt - 1; i < (allBP.Length > EndAt ? EndAt : allBP.Length); ++i) TBP.Add(allBP[i]);
            for (int i = StartAt - 1; i < (allBP.Length > EndAt ? EndAt : allBP.Length); ++i) Rank.Add(i + 1);

            if (TBP.Count == 0)
            {

                if (cmd == "")
                    await target.reply($"你在 {OnlineOsuInfo.PlayMode.ToStr()} 模式上还没有bp呢。。");
                else
                    await target.reply($"{OnlineOsuInfo.Username} 在 {OnlineOsuInfo.PlayMode.ToStr()} 模式上还没有bp呢。。");
            }
            else
            {
                var image = await KanonBot.image.ScoreList.Draw(ScoreList.Type.BPLIST, TBP, Rank, OnlineOsuInfo);
                using var stream = new MemoryStream();
                await image.SaveAsync(stream, new PngEncoder());
                await target.reply(
                    new Chain().image(
                    Convert.ToBase64String(stream.ToArray(), 0, (int)stream.Length),
                        ImageSegment.Type.Base64
                ));
            }
        }
    }
}
