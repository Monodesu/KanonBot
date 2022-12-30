using KanonBot.Drivers;
using KanonBot.Message;
using KanonBot.API;
using System.Net;
using Flurl;
using Flurl.Http;
using RosuPP;
using System.IO;

namespace KanonBot.functions.osubot
{
    public class Leeway
    {
        async public static Task Execute(Target target, string cmd)
        {
            OSU.Models.User? OnlineOsuInfo;
            Database.Model.UserOSU DBOsuInfo;

            // 解析指令
            var command = BotCmdHelper.CmdParser(cmd, BotCmdHelper.FuncType.Leeway);

            // 解析模式
            command.osu_mode ??= OSU.Enums.Mode.OSU;
            if (command.osu_mode is not OSU.Enums.Mode.OSU) { await target.reply("Leeway仅支持osu!std模式。"); return; }

            // 验证账户
            var AccInfo = Accounts.GetAccInfo(target);
            Database.Model.User? DBUser;
            DBUser = await Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
            if (DBUser == null)
            // { await target.reply("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。"); return; }    // 这里引导到绑定osu
            { await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }

            // 验证osu信息
            var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
            DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
            if (DBOsuInfo == null)
            { await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }

            // 验证osu信息
            OnlineOsuInfo = await OSU.GetUser(DBOsuInfo.osu_uid);
            //}
            //else
            //{
            // 验证osu信息
            //    try { OnlineOsuInfo = Osu.GetUser(command.osu_username); }
            //    catch { OnlineOsuInfo = new Osu.UserInfo(); }
            //    is_bounded = false;
            //}

            // 验证osu信息
            if (OnlineOsuInfo == null)
            {
                //if (is_bounded) { await target.reply("被办了。"); return; }
                //await target.reply("猫猫没有找到此用户。"); return;
                await target.reply("被办了。"); return;
            }

            long bid;
            if (command.order_number == 0) // 检查玩家是否指定bid
            {
                var scoreInfos = await OSU.GetUserScores(OnlineOsuInfo.Id, OSU.Enums.UserScoreType.Recent, command.osu_mode ?? OSU.Enums.Mode.OSU, 1, command.order_number - 1, true);
                if (scoreInfos == null) {await target.reply("获取成绩时出错。"); return;};    // 正常是找不到玩家，但是上面有验证，这里做保险
                if (scoreInfos!.Length > 0) { bid = scoreInfos[0].Beatmap!.BeatmapId; }
                else { await target.reply("猫猫找不到你最近游玩的成绩。"); return; }
            }
            else
            {
                bid = command.order_number;
            }

            // 尝试寻找玩家在该谱面的最高成绩
            long score;
            var empty_mods = new string[]{}; // 要的是最高分，直接给传一个空集合得了
            var scoreData = await OSU.GetUserBeatmapScore(OnlineOsuInfo.Id, bid, empty_mods, command.osu_mode ?? OSU.Enums.Mode.OSU);
            if (scoreData == null)
            {
                await target.reply("猫猫找不到你的成绩。"); return;
            }
            else
            {
                score = scoreData.Score.Scores;
            }
            if (scoreData.Score.Mode is not OSU.Enums.Mode.OSU) { await target.reply("Leeway仅支持osu!std模式。"); return; } // 检查谱面是否是std


            // LeewayCalculator
            string beatmap;

            try
            {
                // 下载谱面
                await OSU.BeatmapFileChecker(bid);
                beatmap = File.ReadAllText($"./work/beatmap/{bid}.osu");
            }
            catch (Exception)
            {
                // 加载失败
                File.Delete($"./work/beatmap/{bid}.osu");
                await target.reply("无法获取铺面信息。"); return;
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

            if (scoreData != null)
            {
                string scoreModsString = lc.GetModsString(scoreData.Score.Mods);
                int scoreAdvantage = (int)score - maxScore;
                str += string.Format("\n你的成绩：{0:n0} ({1}) {2}", (int)score, scoreModsString != "" ? $"+{scoreModsString}" : "None", scoreAdvantage < 0 ? scoreAdvantage : $"+{scoreAdvantage}");
            }
            else
            {
                str += "\n你的成绩：从未玩过";
            }

            str += string.Format("\n理论值：{0:n0} (+{1})", maxScore, modsString);

            List<int[]> spinners = lc.GetSpinners(beatmap); // 获取转盘
            double adjustTime = lc.GetAdjustTime(mods); // 获取 adjustTime(?) | idk what the fuck is adjustTime lol
            double od = lc.GetOD(beatmap); // 获取 OD
            int difficultyModifier = lc.GetDifficultyModifier(mods); // 计算分数增益

            if (spinners.Count > 0)
            {
                for (int i = 0; i < spinners.Count; i++)
                {
                    int length = spinners[i][1];
                    int combo = spinners[i][0];
                    double rotations = lc.CalcRotations(length, adjustTime);
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
            await target.reply(new Chain().msg(str));
        }
    }
}
