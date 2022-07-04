using KanonBot.Drivers;
using KanonBot.Message;
using KanonBot.API;
using System.Net;

namespace KanonBot.functions.osubot
{
    public class Leeway
    {
        async public static void Execute(Target target, string cmd)
        {
            Osu.UserInfo OnlineOsuInfo;
            Database.Model.Users_osu DBOsuInfo;

            // 解析指令
            var command = BotCmdHelper.CmdParser(cmd, BotCmdHelper.Func_type.Leeway);
            //if (command.selfquery)
            //{

            // 验证账户
            var AccInfo = Accounts.GetAccInfo(target);
            if (Accounts.GetAccount(AccInfo.uid, AccInfo.platform)!.uid == -1)
            { target.reply("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。"); return; }

            // 验证osu信息
            DBOsuInfo = Accounts.CheckOsuAccount(Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform)!.uid)!;
            if (DBOsuInfo == null)
            { target.reply("您还没有绑定osu账户，请使用!set osu 您的osu用户名来绑定您的osu账户。"); return; }

            // 验证osu信息
            try { OnlineOsuInfo = await Osu.GetUser(DBOsuInfo.osu_uid); }
            catch { OnlineOsuInfo = new Osu.UserInfo(); }
            //}
            //else
            //{
            // 验证osu信息
            //    try { OnlineOsuInfo = Osu.GetUser(command.osu_username); }
            //    catch { OnlineOsuInfo = new Osu.UserInfo(); }
            //    is_bounded = false;
            //}

            // 验证osu信息
            if (OnlineOsuInfo.userName == null)
            {
                //if (is_bounded) { target.reply("被办了。"); return; }
                //target.reply("猫猫没有找到此用户。"); return;
                target.reply("被办了。"); return;
            }


            // 查询
            string mode;


            // 解析模式
            try { mode = Osu.Modes[int.Parse(command.osu_mode)]; } catch { mode = "osu"; }
            if (mode != "osu") { target.reply("Leeway仅支持osu!std模式。"); return; }

            long bid;
            if (command.order_number == 0) // 检查玩家是否指定bid
            {
                List<Osu.ScoreInfo> scoreInfos;
                try
                {
                    scoreInfos = await Osu.GetUserScores(OnlineOsuInfo.userId, "recent", mode, 1, command.order_number - 1, true); // 查询玩家recent
                    if (scoreInfos.ToArray()[0].mode != "osu") { target.reply("Leeway仅支持osu!std模式。"); return; } // 检查谱面是否是std
                    bid = scoreInfos.ToArray()[0].beatmapId; // 从recent获取bid
                }
                catch { target.reply("猫猫找不到该最近游玩的成绩。"); return; }
            }
            else
            {
                bid = command.order_number;
            }

            // 尝试寻找玩家在该谱面的最高成绩
            long score;
            Osu.ScoreInfo? scoreData;
            List<string> empty_mods = new(); // 要的是最高分，直接给传一个空集合得了
            try
            {
                scoreData = await Osu.GetUserBeatmapScore(OnlineOsuInfo.userId, command.order_number, empty_mods, mode);
                if (scoreData.HasValue)
                {
                    target.reply("猫猫没有找到你在这张谱面上的成绩。"); return;
                }
                else
                {
                    score = scoreData.Value.score;
                }
                if (scoreData.Value.mode != "osu") { target.reply("Leeway仅支持osu!std模式。"); return; } // 检查谱面是否是std
            }
            catch (Exception e) { if (e.Message == "{\"error\":null}") { target.reply("猫猫没有找到该谱面。"); return; } else throw; }


            // LeewayCalculator
            string beatmap;

            // 下载谱面还没有写，之后整合
            try
            {
                var beatmapPath = Http.DownloadFile($"https://old.ppy.sh/osu/{bid}", @$".\work\beatmaps\{bid}.osu");
                beatmap = File.ReadAllText(beatmapPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                target.reply("读取谱面文件失败，请稍后再试"); return;
            }

            Leeway_Calculator lc = new(); // 实例化

            string[] mods = lc.GetMods(command.osu_mods.ToUpper()); // 获取mods
            int maxScore = lc.CalculateMaxScore(beatmap, mods); // 计算理论值
            string modsString = lc.GetModsString(mods); // 获取模式字符串

            string str = "";
            str += string.Concat(new string[]
            {
                    bid.ToString(),
                    " ",
                    lc.GetArtist(beatmap),
                    " - ",
                    lc.GetTitle(beatmap),
                    " (",
                    lc.GetDifficultyName(beatmap),
                    ")"
            });

            if (scoreData.HasValue)
            {
                str += string.Format("\n你的成绩：{0:n0} (+{1})", (int)score, lc.GetModsString(scoreData.Value.mods.ToArray()));
            }
            else
            {
                str += "\n你的成绩：从未玩过";
            }

            str += string.Format("\n理论值：{0:n0} (+{1})", maxScore, modsString);

            List<int[]> spinners = lc.GetSpinners(beatmap); // 获取转盘
            float adjustTime = lc.GetAdjustTime(mods); // 获取 adjustTime(?) | idk what the fuck is adjustTime lol
            float od = lc.GetOD(beatmap); // 获取 OD
            int difficultyModifier = lc.GetDifficultyModifier(mods); // 计算分数增益

            if (spinners.Count > 0)
            {
                for (int i = 0; i < spinners.Count; i++)
                {
                    int length = spinners[i][1];
                    int combo = spinners[i][0];
                    float rotations = lc.CalcRotations(length, adjustTime);
                    int rotReq = lc.CalcRotReq(length, (double)od, difficultyModifier);
                    string amount = lc.CalcAmount((int)rotations, rotReq);
                    double leeway = lc.CalcLeeway(length, adjustTime, (double)od, difficultyModifier);
                    string spinner = string.Format("\n#{0} | 长度：{1} | Combo：{2} | 分数：{3} | 圈数：{4} | Leeway：{5}",
                            i + 1,
                            length,
                            combo,
                            amount,
                            string.Format("{0:0.00000}", rotations),
                            string.Format("{0:0.00000}", leeway));
                    str += spinner;
                }
            }
            else
            {
                str += "\n该难度没有转盘";
            }
            target.reply(new Chain().msg(str));
        }
    }
}