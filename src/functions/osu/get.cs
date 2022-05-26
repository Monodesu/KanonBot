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

namespace KanonBot.functions.osubot
{
    public class Get
    {
        public static void Execute(Target target, string cmd)
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
                    Bonuspp(target, childCmd);
                    break;
                case "elo":
                    Elo(target, childCmd);
                    break;
                case "rolecost":
                    Rolecost(target, childCmd);
                    break;
                case "bpht":
                    Bpht(target, childCmd);
                    break;
                case "todaybp":
                    TodayBP(target, childCmd);
                    break;
                case "annualpass":
                    AnnualPass(target, childCmd);
                    break;
                default:
                    return;
            }
        }

        private static void Bonuspp(Target target, string cmd)
        {
            #region 验证
            Osu.UserInfo userInfo = new();
            bool is_bounded = false;
            Database.Model.Users DBUser = new();
            Database.Model.Users_osu DBOsuInfo;
            Osu.UserInfo OnlineOsuInfo;

            // 解析指令
            var command = BotCmdHelper.CmdParser(cmd, BotCmdHelper.Func_type.Info);

            // 取mode信息
            if (command.osu_mode != "") userInfo.mode = command.osu_mode;

            // 解析指令
            if (command.selfquery)
            {
                // 验证账户
                var AccInfo = Accounts.GetAccInfo(target);
                DBUser = Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
                if (DBUser == null)
                { target.reply(new Chain().msg("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。")); return; }

                // 验证osu信息
                DBOsuInfo = Accounts.CheckOsuAccount(Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform)!.uid)!;
                if (DBOsuInfo == null)
                { target.reply(new Chain().msg("您还没有绑定osu账户，请使用!set osu 您的osu用户名来绑定您的osu账户。")); return; }

                // 取mode信息
                if (command.osu_mode == "") userInfo.mode = DBOsuInfo.osu_mode ?? "osu";

                // 验证osu信息
                try { OnlineOsuInfo = Osu.GetUser(DBOsuInfo.osu_uid, userInfo.mode!); }
                catch { OnlineOsuInfo = new Osu.UserInfo(); }
                is_bounded = true;
            }
            else
            {
                // 取mode信息
                var temp_mode_has_value = false;
                if (command.osu_mode == "") userInfo.mode = "osu"; else temp_mode_has_value = true;
                // 验证osu信息
                try { OnlineOsuInfo = Osu.GetUser(command.osu_username, userInfo.mode!); }
                catch { OnlineOsuInfo = new Osu.UserInfo(); }
                var temp_uid = Database.Client.GetOSUUsers(OnlineOsuInfo.userId);
                DBOsuInfo = Accounts.CheckOsuAccount(temp_uid == null ? -1 : temp_uid.uid)!;
                if (DBOsuInfo != null)
                {
                    is_bounded = true;
                    DBUser = Accounts.GetAccount(OnlineOsuInfo.userId);
                    if (!temp_mode_has_value)
                    {
                        userInfo.mode = DBOsuInfo.osu_mode ?? "osu";
                        try { OnlineOsuInfo = Osu.GetUser(command.osu_username, userInfo.mode); }
                        catch { OnlineOsuInfo = new Osu.UserInfo(); }
                    }
                }
            }

            // 验证osu信息
            if (OnlineOsuInfo.userName == null)
            {
                if (is_bounded) { target.reply(new Chain().msg("被办了。")); return; }
                target.reply(new Chain().msg("猫猫没有找到此用户。")); return;
            }
            #endregion

            // 计算bonuspp
            if (OnlineOsuInfo.pp == 0)
            {
                target.reply(new Chain().msg($"你最近还没有玩过{OnlineOsuInfo.mode}模式呢。。"));
                return;
            }
            List<Osu.ScoreInfo> allBP;
            allBP = Osu.GetUserScores(OnlineOsuInfo.userId, "best", OnlineOsuInfo.mode!, 100, 0);
            double scorePP = 0.0, bounsPP = 0.0, pp = 0.0, sumOxy = 0.0, sumOx2 = 0.0, avgX = 0.0, avgY = 0.0, sumX = 0.0;
            List<double> ys = new();
            for (int i = 0; i < allBP.Count; ++i)
            {
                var tmp = allBP[i].pp * Math.Pow(0.95, i);
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
            for (double i = 100; i <= userInfo.playCount; ++i)
            {
                double val = Math.Pow(100.0, b[0] + b[1] * i);
                if (val <= 0.0)
                {
                    break;
                }
                pp += val;
            }
            scorePP += pp;
            bounsPP = userInfo.pp - scorePP;
            int totalscores = userInfo.A + userInfo.S + userInfo.SH + userInfo.SS + userInfo.SSH;
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
            var str = $"{userInfo.userName} ({OnlineOsuInfo.mode})\n" +
                $"总PP：{userInfo.pp.ToString("0.##")}pp\n" +
                $"原始PP：{scorePP.ToString("0.##")}pp\n" +
                $"Bonus PP：{bounsPP.ToString("0.##")}pp\n" +
                $"共计算出 {rankedScores} 个被记录的ranked谱面成绩。";
            target.reply(new Chain().msg(str));
        }

        private static void Elo(Target target, string cmd)
        {
            #region 验证
            Osu.UserInfo userInfo = new();
            bool is_bounded = false;
            Database.Model.Users DBUser = new();
            Database.Model.Users_osu DBOsuInfo;
            Osu.UserInfo OnlineOsuInfo;

            // 解析指令
            var command = BotCmdHelper.CmdParser(cmd, BotCmdHelper.Func_type.Info);

            // 取mode信息
            if (command.osu_mode != "") userInfo.mode = command.osu_mode;

            // 解析指令
            if (command.selfquery)
            {
                // 验证账户
                var AccInfo = Accounts.GetAccInfo(target);
                DBUser = Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
                if (DBUser == null)
                { target.reply(new Chain().msg("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。")); return; }

                // 验证osu信息
                DBOsuInfo = Accounts.CheckOsuAccount(Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform)!.uid)!;
                if (DBOsuInfo == null)
                { target.reply(new Chain().msg("您还没有绑定osu账户，请使用!set osu 您的osu用户名来绑定您的osu账户。")); return; }

                // 取mode信息
                if (command.osu_mode == "") userInfo.mode = DBOsuInfo.osu_mode ?? "osu";

                // 验证osu信息
                try { OnlineOsuInfo = Osu.GetUser(DBOsuInfo.osu_uid, userInfo.mode!); }
                catch { OnlineOsuInfo = new Osu.UserInfo(); }
                is_bounded = true;
            }
            else
            {
                // 取mode信息
                var temp_mode_has_value = false;
                if (command.osu_mode == "") userInfo.mode = "osu"; else temp_mode_has_value = true;
                // 验证osu信息
                try { OnlineOsuInfo = Osu.GetUser(command.osu_username, userInfo.mode!); }
                catch { OnlineOsuInfo = new Osu.UserInfo(); }
                var temp_uid = Database.Client.GetOSUUsers(OnlineOsuInfo.userId);
                DBOsuInfo = Accounts.CheckOsuAccount(temp_uid == null ? -1 : temp_uid.uid)!;
                if (DBOsuInfo != null)
                {
                    is_bounded = true;
                    DBUser = Accounts.GetAccount(OnlineOsuInfo.userId);
                    if (!temp_mode_has_value)
                    {
                        userInfo.mode = DBOsuInfo.osu_mode ?? "osu";
                        try { OnlineOsuInfo = Osu.GetUser(command.osu_username, userInfo.mode); }
                        catch { OnlineOsuInfo = new Osu.UserInfo(); }
                    }
                }
            }

            // 验证osu信息
            if (OnlineOsuInfo.userName == null)
            {
                if (is_bounded) { target.reply(new Chain().msg("被办了。")); return; }
                target.reply(new Chain().msg("猫猫没有找到此用户。")); return;
            }
            #endregion

            try
            {
                JObject eloInfo = Osu.GetUserEloInfo(OnlineOsuInfo.userId)!;
                foreach (var key in eloInfo!)
                {
                    switch (key.Key)
                    {
                        case "code":
                            switch ((int)eloInfo["code"]!)
                            {
                                case 40009:
                                    target.reply(new Chain().msg(eloInfo["message"]!.ToString()));
                                    break;
                                case 40004:
                                    target.reply(new Chain().msg($"{(string)eloInfo["message"]!}\n{OnlineOsuInfo.userName}的初始ELO为: {eloInfo["elo"]}"));
                                    break;
                            }
                            return;
                        case "elo":
                            target.reply(new Chain().msg($"{OnlineOsuInfo.userName}的ELO为: {eloInfo["elo"]}"));
                            return;
                    }
                }
            }
            catch (Exception ex)
            {
                target.reply(new Chain().msg($"查询失败, 失败信息: {ex.Message}"));
            }
        }

        private static void Rolecost(Target target, string cmd)
        {
            cmd = cmd.Trim();
            Func<Osu.UserInfo, Osu.PPlusInfo, double> occost = (userInfo, pppData) =>
            {
                double a, c, z, p;
                p = userInfo.pp;
                z = 1.92 * Math.Pow(pppData.jump, 0.953) + 69.7 * Math.Pow(pppData.flow, 0.596)
                    + 0.588 * Math.Pow(pppData.spd, 1.175) + 3.06 * Math.Pow(pppData.sta, 0.993);
                a = Math.Pow(pppData.acc, 1.2768) * Math.Pow(p, 0.88213);
                c = Math.Min(0.00930973 * Math.Pow(p / 1000, 2.64192) * Math.Pow(z / 4000, 1.48422), 7) + Math.Min(a / 7554280, 3);
                return Math.Round(c, 2);
            };
            Func<Osu.UserInfo, double> oncost = (userInfo) =>
            {
                double fx, pp;
                pp = userInfo.pp;
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
            Osu.PPlusInfo pppData = new();

            #region 验证
            Osu.UserInfo userInfo = new();
            bool is_bounded = false;
            Database.Model.Users DBUser = new();
            Database.Model.Users_osu DBOsuInfo;
            Osu.UserInfo OnlineOsuInfo;

            // 解析指令
            var command = BotCmdHelper.CmdParser(cmd, BotCmdHelper.Func_type.Info);

            // 取mode信息
            if (command.osu_mode != "") userInfo.mode = command.osu_mode;

            // 解析指令
            if (command.selfquery)
            {
                // 验证账户
                var AccInfo = Accounts.GetAccInfo(target);
                DBUser = Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
                if (DBUser == null)
                { target.reply(new Chain().msg("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。")); return; }

                // 验证osu信息
                DBOsuInfo = Accounts.CheckOsuAccount(Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform)!.uid)!;
                if (DBOsuInfo == null)
                { target.reply(new Chain().msg("您还没有绑定osu账户，请使用!set osu 您的osu用户名来绑定您的osu账户。")); return; }

                // 取mode信息
                if (command.osu_mode == "") userInfo.mode = DBOsuInfo.osu_mode ?? "osu";

                // 验证osu信息
                try { OnlineOsuInfo = Osu.GetUser(DBOsuInfo.osu_uid, userInfo.mode!); }
                catch { OnlineOsuInfo = new Osu.UserInfo(); }
                is_bounded = true;
            }
            else
            {
                // 取mode信息
                var temp_mode_has_value = false;
                if (command.osu_mode == "") userInfo.mode = "osu"; else temp_mode_has_value = true;
                // 验证osu信息
                try { OnlineOsuInfo = Osu.GetUser(command.osu_username, userInfo.mode!); }
                catch { OnlineOsuInfo = new Osu.UserInfo(); }
                var temp_uid = Database.Client.GetOSUUsers(OnlineOsuInfo.userId);
                DBOsuInfo = Accounts.CheckOsuAccount(temp_uid == null ? -1 : temp_uid.uid)!;
                if (DBOsuInfo != null)
                {
                    is_bounded = true;
                    DBUser = Accounts.GetAccount(OnlineOsuInfo.userId);
                    if (!temp_mode_has_value)
                    {
                        userInfo.mode = DBOsuInfo.osu_mode ?? "osu";
                        try { OnlineOsuInfo = Osu.GetUser(command.osu_username, userInfo.mode); }
                        catch { OnlineOsuInfo = new Osu.UserInfo(); }
                    }
                }
            }

            // 验证osu信息
            if (OnlineOsuInfo.userName == null)
            {
                if (is_bounded) { target.reply(new Chain().msg("被办了。")); return; }
                target.reply(new Chain().msg("猫猫没有找到此用户。")); return;
            }
            #endregion

            switch (cmd)
            {
                case "occ":
                    try { OnlineOsuInfo = Osu.GetUser(OnlineOsuInfo.userId, "osu"); }
                    catch { target.reply(new Chain().msg($"获取用户信息失败了，请稍后再试。")); }
                    target.reply(new Chain().msg($"在猫猫杯S1中，{OnlineOsuInfo.userName} 的cost为：{occost(OnlineOsuInfo, pppData)}"));
                    break;
                case "onc":
                    try { OnlineOsuInfo = Osu.GetUser(OnlineOsuInfo.userId, "osu"); }
                    catch { target.reply(new Chain().msg($"获取用户信息失败了，请稍后再试。")); }
                    var onc = occost(OnlineOsuInfo, pppData);
                    if (onc == -1)
                        target.reply(new Chain().msg($"{OnlineOsuInfo.userName} 不在参赛范围内。"));
                    else
                        target.reply(new Chain().msg($"在ONC中，{userInfo.userName} 的cost为：{onc}"));
                    break;
                case "ost":
                    try { OnlineOsuInfo = Osu.GetUser(OnlineOsuInfo.userId, "osu"); }
                    catch { target.reply(new Chain().msg($"获取用户信息失败了，请稍后再试。")); }
                    var eloInfo = Osu.GetUserEloInfo(userInfo.userId);
                    int elo = 0;
                    foreach (var key in eloInfo!)
                    {
                        switch (key.Key)
                        {
                            case "code":
                                switch ((int)eloInfo["code"]!)
                                {
                                    case 40009:
                                        target.reply(new Chain().msg(eloInfo["message"]!.ToString()));
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
                        var matchId = Osu.GetUserEloRecentPlay(userInfo.userId);
                        var body = Osu.GetMatchInfo(matchId)!["result"]!.ToObject<JObject>();
                        TimeSpan ts = new();
                        foreach (var item in body!)
                        {
                            var dt = DateTime.Parse(item.Value!["start_time"]!.ToString());
                            ts = DateTime.Now - dt;
                            break;
                        }
                        if (ts.Days > 365)
                        {
                            elo = 0;
                        }
                    }
                    target.reply(new Chain().msg($"在OST中，{userInfo.userName} 的cost为：{ostcost(userInfo.globalRank, elo)}"));
                    break;
                default:
                    target.reply(new Chain().msg($"请输入要查询cost的比赛名称的缩写。"));
                    break;
            }
        }

        private static void Bpht(Target target, string cmd)
        {
            #region 验证
            Osu.UserInfo userInfo = new();
            bool is_bounded = false;
            Database.Model.Users DBUser = new();
            Database.Model.Users_osu DBOsuInfo;
            Osu.UserInfo OnlineOsuInfo;

            // 解析指令
            var command = BotCmdHelper.CmdParser(cmd, BotCmdHelper.Func_type.Info);

            // 取mode信息
            if (command.osu_mode != "") userInfo.mode = command.osu_mode;

            // 解析指令
            if (command.selfquery)
            {
                // 验证账户
                var AccInfo = Accounts.GetAccInfo(target);
                DBUser = Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
                if (DBUser == null)
                { target.reply(new Chain().msg("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。")); return; }

                // 验证osu信息
                DBOsuInfo = Accounts.CheckOsuAccount(Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform)!.uid)!;
                if (DBOsuInfo == null)
                { target.reply(new Chain().msg("您还没有绑定osu账户，请使用!set osu 您的osu用户名来绑定您的osu账户。")); return; }

                // 取mode信息
                if (command.osu_mode == "") userInfo.mode = DBOsuInfo.osu_mode ?? "osu";

                // 验证osu信息
                try { OnlineOsuInfo = Osu.GetUser(DBOsuInfo.osu_uid, userInfo.mode!); }
                catch { OnlineOsuInfo = new Osu.UserInfo(); }
                is_bounded = true;
            }
            else
            {
                // 取mode信息
                var temp_mode_has_value = false;
                if (command.osu_mode == "") userInfo.mode = "osu"; else temp_mode_has_value = true;
                // 验证osu信息
                try { OnlineOsuInfo = Osu.GetUser(command.osu_username, userInfo.mode!); }
                catch { OnlineOsuInfo = new Osu.UserInfo(); }
                var temp_uid = Database.Client.GetOSUUsers(OnlineOsuInfo.userId);
                DBOsuInfo = Accounts.CheckOsuAccount(temp_uid == null ? -1 : temp_uid.uid)!;
                if (DBOsuInfo != null)
                {
                    is_bounded = true;
                    DBUser = Accounts.GetAccount(OnlineOsuInfo.userId);
                    if (!temp_mode_has_value)
                    {
                        userInfo.mode = DBOsuInfo.osu_mode ?? "osu";
                        try { OnlineOsuInfo = Osu.GetUser(command.osu_username, userInfo.mode); }
                        catch { OnlineOsuInfo = new Osu.UserInfo(); }
                    }
                }
            }

            // 验证osu信息
            if (OnlineOsuInfo.userName == null)
            {
                if (is_bounded) { target.reply(new Chain().msg("被办了。")); return; }
                target.reply(new Chain().msg("猫猫没有找到此用户。")); return;
            }
            #endregion

            List<Osu.ScoreInfo> allBP;
            allBP = Osu.GetUserScores(OnlineOsuInfo.userId, "best", OnlineOsuInfo.mode!, 100, 0);
            float totalPP = 0;
            // 如果bp数量小于10则取消
            if (allBP.Count < 10)
            {
                if (cmd == "")
                    target.reply(new Chain().msg("你的bp太少啦，多打些吧"));
                else
                    target.reply(new Chain().msg($"{OnlineOsuInfo.userName}的bp太少啦，请让ta多打些吧"));
                return;
            }
            foreach (var item in allBP)
            {
                totalPP += item.pp;
            }
            var last = allBP.Count;
            var str = $"{OnlineOsuInfo.userName} 在 {OnlineOsuInfo.mode} 模式中:"
            + $"\n你的 bp1 有 {allBP[0].pp:0.##}pp"
            + $"\n你的 bp2 有 {allBP[1].pp:0.##}pp"
            + $"\n..."
            + $"\n你的 bp{last - 1} 有 {allBP[last - 2].pp:0.##}pp"
            + $"\n你的 bp{last} 有 {allBP[last - 1].pp:0.##}pp"
            + $"\n你 bp1 与 bp{last} 相差了有 {allBP[0].pp - allBP[last - 1].pp:0.##}pp"
            + $"\n你的 bp 榜上所有成绩的平均值为 {totalPP / allBP.Count:0.##}pp";
            target.reply(new Chain().msg(str));
        }

        private static void TodayBP(Target target, string cmd)
        {
            #region 验证
            Osu.UserInfo userInfo = new();
            bool is_bounded = false;
            Database.Model.Users DBUser = new();
            Database.Model.Users_osu DBOsuInfo;
            Osu.UserInfo OnlineOsuInfo;

            // 解析指令
            var command = BotCmdHelper.CmdParser(cmd, BotCmdHelper.Func_type.Info);

            // 取mode信息
            if (command.osu_mode != "") userInfo.mode = command.osu_mode;

            // 解析指令
            if (command.selfquery)
            {
                // 验证账户
                var AccInfo = Accounts.GetAccInfo(target);
                DBUser = Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
                if (DBUser == null)
                { target.reply(new Chain().msg("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。")); return; }

                // 验证osu信息
                DBOsuInfo = Accounts.CheckOsuAccount(Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform)!.uid)!;
                if (DBOsuInfo == null)
                { target.reply(new Chain().msg("您还没有绑定osu账户，请使用!set osu 您的osu用户名来绑定您的osu账户。")); return; }

                // 取mode信息
                if (command.osu_mode == "") userInfo.mode = DBOsuInfo.osu_mode ?? "osu";

                // 验证osu信息
                try { OnlineOsuInfo = Osu.GetUser(DBOsuInfo.osu_uid, userInfo.mode!); }
                catch { OnlineOsuInfo = new Osu.UserInfo(); }
                is_bounded = true;
            }
            else
            {
                // 取mode信息
                var temp_mode_has_value = false;
                if (command.osu_mode == "") userInfo.mode = "osu"; else temp_mode_has_value = true;
                // 验证osu信息
                try { OnlineOsuInfo = Osu.GetUser(command.osu_username, userInfo.mode!); }
                catch { OnlineOsuInfo = new Osu.UserInfo(); }
                var temp_uid = Database.Client.GetOSUUsers(OnlineOsuInfo.userId);
                DBOsuInfo = Accounts.CheckOsuAccount(temp_uid == null ? -1 : temp_uid.uid)!;
                if (DBOsuInfo != null)
                {
                    is_bounded = true;
                    DBUser = Accounts.GetAccount(OnlineOsuInfo.userId);
                    if (!temp_mode_has_value)
                    {
                        userInfo.mode = DBOsuInfo.osu_mode ?? "osu";
                        try { OnlineOsuInfo = Osu.GetUser(command.osu_username, userInfo.mode); }
                        catch { OnlineOsuInfo = new Osu.UserInfo(); }
                    }
                }
            }

            // 验证osu信息
            if (OnlineOsuInfo.userName == null)
            {
                if (is_bounded) { target.reply(new Chain().msg("被办了。")); return; }
                target.reply(new Chain().msg("猫猫没有找到此用户。")); return;
            }
            #endregion

            List<Osu.ScoreInfo> allBP;
            allBP = Osu.GetUserScores(OnlineOsuInfo.userId, "best", OnlineOsuInfo.mode!, 100, 0);
            var str = $"";
            var t = DateTime.Now.Hour < 4 ? DateTime.Now.Date.AddDays(-1).AddHours(4) : DateTime.Now.Date.AddHours(4);
            for (int i = 0; i < allBP.Count; i++)
            {
                var item = allBP[i];
                var ts = (item.achievedTime - t).Days;
                if (0 <= ts && ts < 1)
                {
                    str += $"\n#{i + 1} {item.rank} {item.acc * 100:0.##}% {item.pp.ToString("0.##")}pp";
                    if (item.mods.Count > 0) str += $" +{string.Join(',', item.mods)}";
                }
            }
            if (str == "")
            {
                if (cmd == "")
                    target.reply(new Chain().msg($"你今天在 {OnlineOsuInfo.mode} 模式上还没有新bp呢。。"));
                else
                    target.reply(new Chain().msg($"{OnlineOsuInfo.userName} 今天在 {OnlineOsuInfo.mode} 模式上还没有新bp呢。。"));
            }
            else
            {
                target.reply(new Chain().msg($"{OnlineOsuInfo.userName} 今天在 {OnlineOsuInfo.mode} 模式上新增的BP:" + str));
            }
        }

        private static void AnnualPass(Target target, string cmd) //may need to change describe
        {

        }
    }
}
