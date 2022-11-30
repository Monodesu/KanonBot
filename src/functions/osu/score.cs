using KanonBot.Drivers;
using KanonBot.Message;
using KanonBot.API;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Jpeg;
using KanonBot.functions.osu.rosupp;

namespace KanonBot.functions.osubot
{
    public class Score
    {
        async public static Task Execute(Target target, string cmd)
        {
            var is_bounded = false;
            OSU.Models.User? OnlineOsuInfo;
            Database.Model.UserOSU DBOsuInfo;

            // 解析指令
            var command = BotCmdHelper.CmdParser(cmd, BotCmdHelper.FuncType.Score);
            if (command.self_query)
            {
                // 验证账户
                var AccInfo = Accounts.GetAccInfo(target);
                Database.Model.User? DBUser;
                DBUser = await Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
                if (DBUser == null)
                // { target.reply("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。"); return; }    // 这里引导到绑定osu
                { target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }

                var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
                DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
                if (DBOsuInfo == null)
                { target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }

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
                if (is_bounded) { target.reply("被办了。"); return; }
                target.reply("猫猫没有找到此用户。"); return;
            }

            if (!is_bounded) // 未绑定用户回数据库查询找模式
            {
                var temp_uid = await Database.Client.GetOsuUser(OnlineOsuInfo.Id);
                DBOsuInfo = (await Accounts.CheckOsuAccount(temp_uid == null ? -1 : temp_uid.uid))!;
                if (DBOsuInfo != null)
                {
                    is_bounded = true;
                    command.osu_mode ??= OSU.Enums.String2Mode(DBOsuInfo.osu_mode);
                }
            }

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
            if (command.order_number == -1) { target.reply("请提供谱面bid。"); return; }


            var scoreData = await OSU.GetUserBeatmapScore(OnlineOsuInfo.Id, command.order_number, mods.ToArray(), command.osu_mode ?? OSU.Enums.Mode.OSU);

            if (scoreData == null) { target.reply("猫猫没有找到你的成绩"); return; }
            //ppy的getscore api不会返回beatmapsets信息，需要手动获取
            var beatmapSetInfo = await OSU.GetBeatmap(scoreData!.Score.Beatmap!.BeatmapId);
            scoreData.Score.Beatmapset = beatmapSetInfo!.Beatmapset;

            try
            {
                if(scoreData.Score.Mode == OSU.Enums.Mode.OSU)
                {
                    //rosupp
                    var data = await PerformanceCalculator.CalculatePanelData(scoreData.Score);
                    //osu-tools
                    // var data = await KanonBot.osutools.Calculator.CalculateAsync(scoreData.Score);
                    // 绘制
                    var stream = new MemoryStream();
                    var img = await LegacyImage.Draw.DrawScore(data);
                    await img.SaveAsync(stream, command.res ? new PngEncoder() : new JpegEncoder());
                    stream.TryGetBuffer(out ArraySegment<byte> buffer);
                    target.reply(new Chain().image(Convert.ToBase64String(buffer.Array!, 0, (int)stream.Length), ImageSegment.Type.Base64));
                }
                else
                {
                    //rosupp
                    var data = await PerformanceCalculator.CalculatePanelData(scoreData.Score);
                    // 绘制
                    var stream = new MemoryStream();
                    var img = await LegacyImage.Draw.DrawScore(data);
                    await img.SaveAsync(stream, command.res ? new PngEncoder() : new JpegEncoder());
                    stream.TryGetBuffer(out ArraySegment<byte> buffer);
                    target.reply(new Chain().image(Convert.ToBase64String(buffer.Array!, 0, (int)stream.Length), ImageSegment.Type.Base64));
                }

            }
            catch
            {
                target.reply("计算成绩时出错。"); return;
            }
        }
    }




}

