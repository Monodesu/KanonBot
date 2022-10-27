using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KanonBot.Drivers;
using KanonBot.Message;
using KanonBot.API;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq.Expressions;
using static KanonBot.API.OSU.Models;

namespace KanonBot.functions.osubot
{
    public class Get
    {
        async public static Task Execute(Target target, string cmd)
        {
            string rootCmd, childCmd = "";
            try
            {
                rootCmd = cmd[..cmd.IndexOf(" ")].Trim();
                childCmd = cmd[(cmd.IndexOf(" ") + 1)..].Trim();
            }
            catch { rootCmd = cmd; }
            switch (rootCmd)
            {
                case "bonuspp":
                    await Bonuspp(target, childCmd);
                    break;
                case "elo":
                    await Elo(target, childCmd);
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
                case "annualpass":
                    // await AnnualPass(target, childCmd);
                    break;
                default:
                    target.reply("!get bonuspp/elo/rolecost/bpht/todaybp");
                    return;
            }
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
                // { target.reply("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。"); return; }    // 这里引导到绑定osu
                { target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }
                // 验证账号信息
                var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
                DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
                if (DBOsuInfo == null)
                { target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }

                mode ??= OSU.Enums.ParseMode(DBOsuInfo.osu_mode)!.Value;    // 从数据库解析，理论上不可能错
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
                        mode ??= OSU.Enums.ParseMode(DBOsuInfo.osu_mode)!.Value;
                    }
                    mode ??= tempOsuInfo.PlayMode;
                    osuID = tempOsuInfo.Id;
                }
                else
                {
                    // 直接取消查询，简化流程
                    target.reply("猫猫没有找到此用户。");
                    return;
                }
            }

            // 验证osu信息
            var OnlineOsuInfo = await OSU.GetUser(osuID!.Value, mode!.Value);
            if (OnlineOsuInfo == null)
            {
                if (DBOsuInfo != null)
                    target.reply("被办了。");
                else
                    target.reply("猫猫没有找到此用户。");
                // 中断查询
                return;
            }
            OnlineOsuInfo.PlayMode = mode!.Value;
            #endregion

            // 计算bonuspp
            if (OnlineOsuInfo!.Statistics.PP == 0)
            {
                target.reply($"你最近还没有玩过{OnlineOsuInfo.PlayMode.ToModeStr()}模式呢。。");
                return;
            }
            // 因为上面确定过模式，这里就直接用userdata里的mode了
            var allBP = await OSU.GetUserScores(OnlineOsuInfo.Id, OSU.Enums.UserScoreType.Best, mode!.Value, 100, 0);
            if (allBP == null) { target.reply("查询成绩时出错。"); return; }
            if (allBP!.Length == 0) { target.reply("这个模式你还没有成绩呢.."); return; }
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
            var str = $"{OnlineOsuInfo.Username} ({OnlineOsuInfo.PlayMode.ToModeStr()})\n" +
                $"总PP：{OnlineOsuInfo.Statistics.PP.ToString("0.##")}pp\n" +
                $"原始PP：{scorePP.ToString("0.##")}pp\n" +
                $"Bonus PP：{bounsPP.ToString("0.##")}pp\n" +
                $"共计算出 {rankedScores} 个被记录的ranked谱面成绩。";
            target.reply(str);
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
                // { target.reply("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。"); return; }    // 这里引导到绑定osu
                { target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }
                // 验证账号信息
                var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
                DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
                if (DBOsuInfo == null)
                { target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }

                mode ??= OSU.Enums.ParseMode(DBOsuInfo.osu_mode)!.Value;    // 从数据库解析，理论上不可能错
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
                        mode ??= OSU.Enums.ParseMode(DBOsuInfo.osu_mode)!.Value;
                    }
                    mode ??= tempOsuInfo.PlayMode;
                    osuID = tempOsuInfo.Id;
                }
                else
                {
                    // 直接取消查询，简化流程
                    target.reply("猫猫没有找到此用户。");
                    return;
                }
            }

            // 验证osu信息
            var OnlineOsuInfo = await OSU.GetUser(osuID!.Value, mode!.Value);
            if (OnlineOsuInfo == null)
            {
                if (DBOsuInfo != null)
                    target.reply("被办了。");
                else
                    target.reply("猫猫没有找到此用户。");
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
                                    target.reply(eloInfo["message"]!.ToString());
                                    break;
                                case 40004:
                                    target.reply($"{(string)eloInfo["message"]!}\n{OnlineOsuInfo.Username}的初始ELO为: {eloInfo["elo"]}");
                                    break;
                            }
                            return;
                        case "elo":
                            target.reply($"{OnlineOsuInfo.Username}的ELO为: {eloInfo["elo"]}");
                            return;
                    }
                }
            }
            catch (Exception ex)
            {
                target.reply($"查询失败, 失败信息: {ex.Message}");
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
                // { target.reply("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。"); return; }    // 这里引导到绑定osu
                { target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }
                // 验证账号信息
                var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
                DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
                if (DBOsuInfo == null)
                { target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }

                mode ??= OSU.Enums.ParseMode(DBOsuInfo.osu_mode)!.Value;    // 从数据库解析，理论上不可能错
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
                        mode ??= OSU.Enums.ParseMode(DBOsuInfo.osu_mode)!.Value;
                    }
                    mode ??= tempOsuInfo.PlayMode;
                    osuID = tempOsuInfo.Id;
                }
                else
                {
                    // 直接取消查询，简化流程
                    target.reply("猫猫没有找到此用户。");
                    return;
                }
            }

            // 验证osu信息
            var OnlineOsuInfo = await OSU.GetUser(osuID!.Value, mode!.Value);
            if (OnlineOsuInfo == null)
            {
                if (DBOsuInfo != null)
                    target.reply("被办了。");
                else
                    target.reply("猫猫没有找到此用户。");
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
                        target.reply($"在猫猫杯S1中，{OnlineOsuInfo.Username} 的cost为：{occost(OnlineOsuInfo, pppData.User)}");
                    }
                    catch { target.reply($"获取pp+失败"); return; }
                    break;
                case "onc":
                    var onc = oncost(OnlineOsuInfo);
                    if (onc == -1)
                        target.reply($"{OnlineOsuInfo.Username} 不在参赛范围内。");
                    else
                        target.reply($"在ONC中，{OnlineOsuInfo.Username} 的cost为：{onc}");
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
                                            target.reply(eloInfo["message"]!.ToString());
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
                        target.reply($"在OST中，{OnlineOsuInfo.Username} 的cost为：{ostcost(OnlineOsuInfo.Statistics.GlobalRank, elo)}");
                    }
                    catch { target.reply($"获取elo失败"); return; }
                    break;
                default:
                    target.reply($"请输入要查询cost的比赛名称的缩写。");
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
                // { target.reply("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。"); return; }    // 这里引导到绑定osu
                { target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }
                // 验证账号信息
                var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
                DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
                if (DBOsuInfo == null)
                { target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }

                mode ??= OSU.Enums.ParseMode(DBOsuInfo.osu_mode)!.Value;    // 从数据库解析，理论上不可能错
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
                        mode ??= OSU.Enums.ParseMode(DBOsuInfo.osu_mode)!.Value;
                    }
                    mode ??= tempOsuInfo.PlayMode;
                    osuID = tempOsuInfo.Id;
                }
                else
                {
                    // 直接取消查询，简化流程
                    target.reply("猫猫没有找到此用户。");
                    return;
                }
            }

            // 验证osu信息
            var OnlineOsuInfo = await OSU.GetUser(osuID!.Value, mode!.Value);
            if (OnlineOsuInfo == null)
            {
                if (DBOsuInfo != null)
                    target.reply("被办了。");
                else
                    target.reply("猫猫没有找到此用户。");
                // 中断查询
                return;
            }
            OnlineOsuInfo.PlayMode = mode!.Value;
            #endregion

            var allBP = await OSU.GetUserScores(OnlineOsuInfo!.Id, OSU.Enums.UserScoreType.Best, command.osu_mode ?? OSU.Enums.Mode.OSU, 100, 0);
            if (allBP == null) { target.reply("查询成绩时出错。"); return; }
            double totalPP = 0;
            // 如果bp数量小于10则取消
            if (allBP!.Length < 10)
            {
                if (cmd == "")
                    target.reply("你的bp太少啦，多打些吧");
                else
                    target.reply($"{OnlineOsuInfo.Username}的bp太少啦，请让ta多打些吧");
                return;
            }
            foreach (var item in allBP)
            {
                totalPP += item.PP;
            }
            var last = allBP.Length;
            var str = $"{OnlineOsuInfo.Username} 在 {OnlineOsuInfo.PlayMode.ToModeStr()} 模式中:"
            + $"\n你的 bp1 有 {allBP[0].PP:0.##}pp"
            + $"\n你的 bp2 有 {allBP[1].PP:0.##}pp"
            + $"\n..."
            + $"\n你的 bp{last - 1} 有 {allBP[last - 2].PP:0.##}pp"
            + $"\n你的 bp{last} 有 {allBP[last - 1].PP:0.##}pp"
            + $"\n你 bp1 与 bp{last} 相差了有 {allBP[0].PP - allBP[last - 1].PP:0.##}pp"
            + $"\n你的 bp 榜上所有成绩的平均值为 {totalPP / allBP.Length:0.##}pp";
            target.reply(str);
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
                // { target.reply("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。"); return; }    // 这里引导到绑定osu
                { target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }
                // 验证账号信息
                var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
                DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
                if (DBOsuInfo == null)
                { target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }

                mode ??= OSU.Enums.ParseMode(DBOsuInfo.osu_mode)!.Value;    // 从数据库解析，理论上不可能错
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
                        mode ??= OSU.Enums.ParseMode(DBOsuInfo.osu_mode)!.Value;
                    }
                    mode ??= tempOsuInfo.PlayMode;
                    osuID = tempOsuInfo.Id;
                }
                else
                {
                    // 直接取消查询，简化流程
                    target.reply("猫猫没有找到此用户。");
                    return;
                }
            }

            // 验证osu信息
            var OnlineOsuInfo = await OSU.GetUser(osuID!.Value, mode!.Value);
            if (OnlineOsuInfo == null)
            {
                if (DBOsuInfo != null)
                    target.reply("被办了。");
                else
                    target.reply("猫猫没有找到此用户。");
                // 中断查询
                return;
            }
            OnlineOsuInfo.PlayMode = mode!.Value;
            #endregion

            var allBP = await OSU.GetUserScores(OnlineOsuInfo!.Id, OSU.Enums.UserScoreType.Best, command.osu_mode ?? OSU.Enums.Mode.OSU, 100, 0);
            if (allBP == null) { target.reply("查询成绩时出错。"); return; }
            var str = $"";
            var t = DateTime.Now.Hour < 4 ? DateTime.Now.Date.AddDays(-1).AddHours(4) : DateTime.Now.Date.AddHours(4);
            for (int i = 0; i < allBP.Length; i++)
            {
                var item = allBP[i];
                var ts = (item.CreatedAt - t).Days;
                if (0 <= ts && ts < 1)
                {
                    str += $"\n#{i + 1} {item.Rank} {item.Accuracy * 100:0.##}% {item.PP.ToString("0.##")}pp";
                    if (item.Mods.Length > 0) str += $" +{string.Join(',', item.Mods)}";
                }
            }
            if (str == "")
            {
                if (cmd == "")
                    target.reply($"你今天在 {OnlineOsuInfo.PlayMode.ToModeStr()} 模式上还没有新bp呢。。");
                else
                    target.reply($"{OnlineOsuInfo.Username} 今天在 {OnlineOsuInfo.PlayMode.ToModeStr()} 模式上还没有新bp呢。。");
            }
            else
            {
                target.reply($"{OnlineOsuInfo.Username} 今天在 {OnlineOsuInfo.PlayMode.ToModeStr()} 模式上新增的BP:" + str);
            }
        }

        private static void AnnualPass(Target target, string cmd) //may need to change describe
        {

        }
    }
}
