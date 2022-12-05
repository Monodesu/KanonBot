using KanonBot.Drivers;
using KanonBot.Message;
using KanonBot.API;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Jpeg;
using KanonBot.functions.osu.rosupp;

namespace KanonBot.functions.osubot
{
    public class Recent
    {
        async public static Task Execute(Target target, string cmd, bool includeFails = false)
        {
            var is_bounded = false;
            OSU.Models.User? OnlineOsuInfo;
            Database.Model.UserOSU DBOsuInfo;

            // 解析指令
            var command = BotCmdHelper.CmdParser(cmd, BotCmdHelper.FuncType.Recent);
            if (command.self_query)
            {
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
                command.osu_mode ??= OSU.Enums.String2Mode(DBOsuInfo.osu_mode);

                // 验证osu信息
                OnlineOsuInfo = await OSU.GetUser(DBOsuInfo.osu_uid, command.osu_mode!.Value);
                is_bounded = true;
            }
            else
            {
                // 验证osu信息
                OnlineOsuInfo = await OSU.GetUser(command.osu_username);
                is_bounded = false;
            }

            // 验证osu信息
            if (OnlineOsuInfo == null)
            {
                if (is_bounded) { await target.reply("被办了。"); return; }
                await target.reply("猫猫没有找到此用户。"); return;
            }

            if (!is_bounded) // 未绑定用户回数据库查询找模式
            {
                var temp_uid = await Database.Client.GetOsuUser(OnlineOsuInfo.Id);
                DBOsuInfo = (await Accounts.CheckOsuAccount(temp_uid == null ? -1 : temp_uid.uid))!;
                if (DBOsuInfo != null)
                {
                    //is_bounded = true;
                    command.osu_mode ??= OSU.Enums.String2Mode(DBOsuInfo.osu_mode);
                }
            }

            // 判断给定的序号是否在合法的范围内
            // if (command.order_number == -1) { await target.reply("猫猫找不到该最近游玩的成绩。"); return; }

            //var scorePanelData = new LegacyImage.Draw.ScorePanelData();
            var scoreInfos = await OSU.GetUserScores(OnlineOsuInfo.Id, OSU.Enums.UserScoreType.Recent, command.osu_mode ?? OSU.Enums.Mode.OSU, 1, command.order_number - 1, includeFails);
            if (scoreInfos == null) { await target.reply("查询成绩时出错。"); return; };    // 正常是找不到玩家，但是上面有验证，这里做保险
            if (scoreInfos!.Length > 0)
            {
                    if (scoreInfos[0].Mode == OSU.Enums.Mode.OSU)
                    {
                        //osu-tools
                        var data = await PerformanceCalculator.CalculatePanelData(scoreInfos[0]);
                        if (scoreInfos[0].Beatmap!.Status == OSU.Enums.Status.ranked || scoreInfos[0].Beatmap!.Status == OSU.Enums.Status.approved)
                            await Database.Client.InsertOsuStandardBeatmapTechData(scoreInfos[0].Beatmap!.BeatmapId, (int)data.ppInfo.ppStats![0].total,
                                (int)data.ppInfo.ppStats![0].acc!, (int)data.ppInfo.ppStats![0].speed!, (int)data.ppInfo.ppStats![0].aim!,
                                scoreInfos[0].Mods);
                        // 绘制
                        var stream = new MemoryStream();
                        var img = await LegacyImage.Draw.DrawScore(data);
                        await img.SaveAsync(stream, command.res ? new PngEncoder() : new JpegEncoder());
                        stream.TryGetBuffer(out ArraySegment<byte> buffer);
                        await target.reply(new Chain().image(Convert.ToBase64String(buffer.Array!, 0, (int)stream.Length), ImageSegment.Type.Base64));
                    }
                    else
                    {
                        //rosupp
                        var data = await PerformanceCalculator.CalculatePanelData(scoreInfos[0]);
                        // 绘制
                        var stream = new MemoryStream();
                        var img = await LegacyImage.Draw.DrawScore(data);
                        await img.SaveAsync(stream, command.res ? new PngEncoder() : new JpegEncoder());
                        stream.TryGetBuffer(out ArraySegment<byte> buffer);
                        await target.reply(new Chain().image(Convert.ToBase64String(buffer.Array!, 0, (int)stream.Length), ImageSegment.Type.Base64));
                    }

            }
            else { await target.reply("猫猫找不到该玩家最近游玩的成绩。"); return; }


        }
    }
}
