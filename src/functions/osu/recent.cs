using KanonBot.Drivers;
using KanonBot.Message;
using KanonBot.API;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace KanonBot.functions.osubot
{
    public class Recent
    {
        async public static Task Execute(Target target, string cmd, bool includeFails = false)
        {
            var is_bounded = false;
            OSU.Models.User? OnlineOsuInfo;
            Database.Model.Users_osu DBOsuInfo;

            // 解析指令
            var command = BotCmdHelper.CmdParser(cmd, BotCmdHelper.Func_type.Recent);
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
                OnlineOsuInfo = await OSU.GetUser(command.osu_username);
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

            // 判断给定的序号是否在合法的范围内
            // if (command.order_number == -1) { target.reply("猫猫找不到该最近游玩的成绩。"); return; }

            var scorePanelData = new LegacyImage.Draw.ScorePanelData();
            var scoreInfos = await OSU.GetUserScores(OnlineOsuInfo.Id, OSU.Enums.UserScoreType.Recent, command.osu_mode ?? OSU.Enums.Mode.OSU, 1, command.order_number - 1, includeFails);
            if (scoreInfos == null) {target.reply("查询成绩时出错。"); return;};    // 正常是找不到玩家，但是上面有验证，这里做保险
            if (scoreInfos!.Length > 0) { scorePanelData.scoreInfo = scoreInfos[0]; }
            else { target.reply("猫猫找不到该玩家最近游玩的成绩。"); return; }

            //检查谱面文件下载状态
            OSU.BeatmapFileChecker(scorePanelData.scoreInfo.Beatmap!.BeatmapId);

            // 绘制
            var stream = new MemoryStream();
            var img = LegacyImage.Draw.DrawScore(scorePanelData);
            await img.SaveAsync(stream, command.res ? new PngEncoder() : new JpegEncoder());
            stream.TryGetBuffer(out ArraySegment<byte> buffer);
            target.reply(new Chain().image(Convert.ToBase64String(buffer.Array!, 0, (int)stream.Length), ImageSegment.Type.Base64));
        }
    }
}
