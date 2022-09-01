using KanonBot.Drivers;
using KanonBot.Message;
using KanonBot.API;
using Polly;
using System.Security.Cryptography;
using K4os.Compression.LZ4.Internal;
using static KanonBot.API.OSU.Enums;
using KanonBot.functions.osu.rosupp;
using RosuPP;

namespace KanonBot.functions.osubot
{
    public class BestPerformance
    {
        async public static Task Execute(Target target, string cmd)
        {
            var is_bounded = false;
            OSU.Models.User? OnlineOsuInfo;
            Database.Model.Users_osu DBOsuInfo;

            // 解析指令
            var command = BotCmdHelper.CmdParser(cmd, BotCmdHelper.Func_type.BestPerformance);
            if (command.selfquery)
            {
                // 验证账户
                var AccInfo = Accounts.GetAccInfo(target);
                if (Accounts.GetAccount(AccInfo.uid, AccInfo.platform)!.uid == -1)
                { target.reply("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。"); return; }

                // 验证osu信息
                DBOsuInfo = Accounts.CheckOsuAccount(Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform)!.uid)!;
                if (DBOsuInfo == null)
                { target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名来绑定您的osu账户。"); return; }

                // 验证osu信息
                command.osu_mode ??= OSU.Enums.ParseMode(DBOsuInfo.osu_mode);

                // 验证osu信息
                OnlineOsuInfo = await OSU.GetUser(DBOsuInfo.osu_uid, command.osu_mode!.Value);
                is_bounded = true;
            }
            else
            {
                // 验证osu信息
                OnlineOsuInfo = await OSU.GetUser(command.osu_username, command.osu_mode ?? OSU.Enums.Mode.OSU);
                is_bounded = false;
            }

            // 验证osu信息
            if (OnlineOsuInfo == null)
            {
                if (is_bounded) { target.reply("被办了。"); return; }
                target.reply("猫猫没有找到此用户。"); return;
            }

            if (!is_bounded) // 未绑定用户回数据库查询找模式
            {
                var temp_uid = Database.Client.GetOSUUsers(OnlineOsuInfo.Id);
                DBOsuInfo = Accounts.CheckOsuAccount(temp_uid == null ? -1 : temp_uid.uid)!;
                if (DBOsuInfo != null)
                {
                    is_bounded = true;
                    command.osu_mode ??= OSU.Enums.ParseMode(DBOsuInfo.osu_mode);
                }
            }

            var scorePanelData = new LegacyImage.Draw.ScorePanelData();
            var scores = await OSU.GetUserScores(OnlineOsuInfo.Id, OSU.Enums.UserScoreType.Best, command.osu_mode ?? OSU.Enums.Mode.OSU, 1, command.order_number - 1);
            if (scores == null) { target.reply("查询成绩时出错。"); return; }
            if (scores!.Length > 0) scorePanelData.scoreInfo = scores![0];
            else { target.reply("猫猫找不到该BP。"); return; }

            //检查谱面文件下载状态
            OSU.BeatmapFileChecker(scorePanelData.scoreInfo.Beatmap!.BeatmapId);

            // 获取绘制数据
            try
            {

                var ppInfo = PerformanceCalculator.Calculate(
                    $"./work/beatmap/{scorePanelData.scoreInfo.Beatmap!.BeatmapId}.osu", 
                    (int)scorePanelData.scoreInfo.Mode, 
                    null,//scorePanelData.scoreInfo.Accuracy, 
                    scorePanelData.scoreInfo.Statistics.CountGreat,
                    scorePanelData.scoreInfo.Statistics.CountOk,
                    scorePanelData.scoreInfo.Statistics.CountMeh,
                    scorePanelData.scoreInfo.Statistics.CountMiss,
                    null,//scorePanelData.scoreInfo.Statistics.CountKatu,
                    scorePanelData.scoreInfo.MaxCombo,null);

                //scorePanelData.ppInfo = OSU.LegacyPPInfoParser(mainPP);
                scorePanelData.ppInfo.accuracy = scorePanelData.scoreInfo.Accuracy;
                scorePanelData.ppInfo.star = ppInfo.stars;
                scorePanelData.ppInfo.approachRate = ppInfo.ar;
                scorePanelData.ppInfo.circleSize = ppInfo.cs;
                scorePanelData.ppInfo.HPDrainRate = ppInfo.hp;
                scorePanelData.ppInfo.hitWindow = ppInfo.od;
                scorePanelData.ppInfo.ppStat.acc = ppInfo.ppAcc.ToNullable().Value;
                scorePanelData.ppInfo.ppStat.aim = ppInfo.ppAim.ToNullable().Value;
                scorePanelData.ppInfo.ppStat.speed = ppInfo.ppSpeed.ToNullable().Value;
                scorePanelData.ppInfo.ppStat.total = ppInfo.pp;






                //return;
            }
            catch (Exception e)
            {
                //var retry = KanonAPIExceptionHandel(tar, e, scorePanelData.scoreInfo.beatmapInfo.beatmapId, retrytime);
                //if (retry == -1) return;
                //else if (retry == 1) ScoreBest(tar, overstarban, osbanduration, retrytime + 1);
                scorePanelData.ppInfo.star = -1;
                scorePanelData.ppInfo.maxCombo = -1;
                scorePanelData.ppInfo.approachRate = -1;
                scorePanelData.ppInfo.accuracy = -1;
                scorePanelData.ppInfo.circleSize = -1;
                scorePanelData.ppInfo.HPDrainRate = -1;
                scorePanelData.ppInfo.ppStat.total = -1;
                scorePanelData.ppInfo.ppStat.acc = -1;
                scorePanelData.ppInfo.ppStat.aim = -1;
                scorePanelData.ppInfo.ppStat.speed = -1;
                scorePanelData.ppInfo.ppStat.flashlight = -1;
                scorePanelData.ppInfo.ppStat.effective_miss_count = -1;
            }

            if (scorePanelData.scoreInfo.Mode is not OSU.Enums.Mode.Mania)
            {
                // 5个acc的成绩+fc
                try
                {
                    //Policy
                    //.Handle<Exception>()
                    //.Retry(2, (ex, count) => { Thread.Sleep(3000); })
                    //.Execute(() =>
                    //{
                    //    scorePanelData.ppStats = Kanon.OsuCalcPP(
                    //    scorePanelData.scoreInfo.beatmapInfo.beatmapId,
                    //    scorePanelData.scoreInfo.beatmapInfo.beatmapStatus,
                    //    scorePanelData.scoreInfo.rank == "F" ? false : true,
                    //    scorePanelData.scoreInfo.acc,
                    //    scorePanelData.ppInfo.maxCombo,
                    //    scorePanelData.scoreInfo.score,
                    //    scorePanelData.scoreInfo.great,
                    //    scorePanelData.scoreInfo.ok,
                    //    scorePanelData.scoreInfo.meh,
                    //    scorePanelData.scoreInfo.geki,
                    //    scorePanelData.scoreInfo.katu,
                    //    0,
                    //    scorePanelData.scoreInfo.mods,
                    //    scorePanelData.scoreInfo.mode,
                    //    true
                    //    ).ppStats;
                    //});
                    throw new Exception();
                }
                catch
                {
                    // SendMessage(tar, "从KanonAPI获取数据时失败..");
                    // return;
                    scorePanelData.ppStats = new();
                    for (var i = 0; i < 6; i++)
                    {
                        OSU.Legacy.PPInfo.PPStat ppStat = new();
                        ppStat.total = -1;
                        scorePanelData.ppStats.Add(ppStat);
                    }
                }
            }

            // 绘制
            MemoryStream img = LegacyImage.Draw.DrawScore(scorePanelData, command.res);
            img.TryGetBuffer(out ArraySegment<byte> buffer);
            target.reply(new Chain().image(Convert.ToBase64String(buffer.Array!, 0, (int)img.Length), ImageSegment.Type.Base64));
        }
    }
}
