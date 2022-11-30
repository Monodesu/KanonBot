using KanonBot.Drivers;
using KanonBot.Message;
using KanonBot.API;
using System.Security.Cryptography;
using static KanonBot.API.OSU.Enums;
using KanonBot.functions.osu.rosupp;
using RosuPP;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace KanonBot.functions.osubot
{
    public class BestPerformance
    {
        async public static Task Execute(Target target, string cmd)
        {
            var is_bounded = false;
            OSU.Models.User? OnlineOsuInfo;
            Database.Model.UserOSU DBOsuInfo;

            // 解析指令
            var command = BotCmdHelper.CmdParser(cmd, BotCmdHelper.FuncType.BestPerformance);
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
                OnlineOsuInfo = await OSU.GetUser(command.osu_username, command.osu_mode ?? OSU.Enums.Mode.OSU);
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


            var scores = await OSU.GetUserScores(OnlineOsuInfo.Id, OSU.Enums.UserScoreType.Best, command.osu_mode ?? OSU.Enums.Mode.OSU, 1, command.order_number - 1);
            if (scores == null) { await target.reply("查询成绩时出错。"); return; }
            if (scores!.Length > 0)
            {
                try
                {
                    if (scores![0].Mode == OSU.Enums.Mode.OSU)
                    {
                        //osu-tools
                        var data = await PerformanceCalculator.CalculatePanelData(scores![0]);
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
                        var data = await PerformanceCalculator.CalculatePanelData(scores![0]);
                        // 绘制
                        var stream = new MemoryStream();
                        var img = await LegacyImage.Draw.DrawScore(data);
                        await img.SaveAsync(stream, command.res ? new PngEncoder() : new JpegEncoder());
                        stream.TryGetBuffer(out ArraySegment<byte> buffer);
                        await target.reply(new Chain().image(Convert.ToBase64String(buffer.Array!, 0, (int)stream.Length), ImageSegment.Type.Base64));
                    }

                }
                catch
                {
                    await target.reply("计算成绩时出错。");
                    return;
                }
            }
            else { await target.reply("猫猫找不到该BP。"); return; }

        }
    }
}
