using KanonBot.Drivers;
using KanonBot.Message;
using KanonBot.API;

namespace KanonBot.functions.osubot
{
    public class Score
    {
        async public static void Execute(Target target, string cmd)
        {
            var is_bounded = false;
            Osu.UserInfo OnlineOsuInfo;
            Database.Model.Users_osu DBOsuInfo;

            // 解析指令
            var command = BotCmdHelper.CmdParser(cmd, BotCmdHelper.Func_type.Score);
            if (command.selfquery)
            {
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
                is_bounded = true;
            }
            else
            {
                // 验证osu信息
                try { OnlineOsuInfo = await Osu.GetUser(command.osu_username); }
                catch { OnlineOsuInfo = new Osu.UserInfo(); }
                is_bounded = false;
            }

            // 验证osu信息
            if (OnlineOsuInfo.userName == null)
            {
                if (is_bounded) { target.reply("被办了。"); return; }
                target.reply("猫猫没有找到此用户。"); return;
            }


            // 查询
            string mode;


            // 解析模式
            try { mode = Osu.Modes[int.Parse(command.osu_mode)]; } catch { mode = "osu"; }

            // 解析Mod
            List<string> mods = new();
            try
            {
                mods = Enumerable.Range(0, command.osu_mods.Length / 2)
                    .Select(p =>
                        new string(
                            command.osu_mods
                            .AsSpan()
                            .Slice(p * 2, 2)
                        ).ToUpper()
                    ).ToList<string>();
            }
            catch { }

            // 判断是否给定了bid
            if (command.order_number == -1) { target.reply("猫猫找不到该谱面。"); return; }
            var scorePanelData = new LegacyImage.Draw.ScorePanelData();

            try
            {
                var scoreData = await Osu.GetUserBeatmapScore(OnlineOsuInfo.userId, command.order_number, mods, mode);
                if (!scoreData.HasValue) { target.reply("猫猫没有找到你的成绩"); return; }
                scorePanelData.scoreInfo = scoreData.Value;
            }
            catch (Exception e) { if (e.Message == "{\"error\":null}") { target.reply("猫猫没有找到该谱面。"); return; } else throw; }

            // 获取绘制数据
            try
            {
                //var mainPP = Kanon.OsuCalcPP(
                //    scorePanelData.scoreInfo.beatmapInfo.beatmapId,
                //    scorePanelData.scoreInfo.beatmapInfo.beatmapStatus,
                //    scorePanelData.scoreInfo.rank == "F" ? false : true,
                //    scorePanelData.scoreInfo.acc,
                //    scorePanelData.scoreInfo.combo,
                //    scorePanelData.scoreInfo.score,
                //    scorePanelData.scoreInfo.great,
                //    scorePanelData.scoreInfo.ok,
                //    scorePanelData.scoreInfo.meh,
                //    scorePanelData.scoreInfo.geki,
                //    scorePanelData.scoreInfo.katu,
                //    scorePanelData.scoreInfo.miss,
                //    scorePanelData.scoreInfo.mods,
                //    scorePanelData.scoreInfo.mode
                //);
                //scorePanelData.ppInfo = mainPP;
            }
            //catch (Exception e)
            //{
            //var retry = KanonAPIExceptionHandel(tar, e, scorePanelData.scoreInfo.beatmapInfo.beatmapId, retrytime);
            //if (retry == -1) return;
            //else if (retry == 1) ScoreBest(tar, overstarban, osbanduration, retrytime + 1);
            //scorePanelData.ppInfo.star = -1;
            //scorePanelData.ppInfo.maxCombo = -1;
            //scorePanelData.ppInfo.approachRate = -1;
            //scorePanelData.ppInfo.accuracy = -1;
            //scorePanelData.ppInfo.circleSize = -1;
            //scorePanelData.ppInfo.HPDrainRate = -1;
            //scorePanelData.ppInfo.ppStat.total = -1;
            //scorePanelData.ppInfo.ppStat.acc = -1;
            //scorePanelData.ppInfo.ppStat.aim = -1;
            //scorePanelData.ppInfo.ppStat.speed = -1;
            //scorePanelData.ppInfo.ppStat.flashlight = -1;
            //scorePanelData.ppInfo.ppStat.effective_miss_count = -1;
            //}
            catch (AggregateException ae)
            {
                var isKnownException = false;
                var msg = $"在从KanonAPI获取PP数据时出现错误\n铺面bid: {scorePanelData.scoreInfo.beatmapInfo.beatmapId}";
                ae.Handle((x) =>
                {
                    if (x is HttpRequestException)
                    {
                        target.reply("获取pp数据时超时，等会儿再试试吧..");
                        isKnownException = true;
                        return true;
                    }
                    msg += $"\n异常类型: {x.GetType()}\n异常信息: '{x.Message}'";
                    return true;
                });
                if (isKnownException) return;
                target.reply("获取pp数据时超时，等会儿再试试吧..");
                // TODO  ADMIN MESSAGE  SendAdminMessage(msg);
                return;
            }

            if (scorePanelData.scoreInfo.mode != "mania")
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
                        Osu.PPInfo.PPStat ppStat = new();
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

