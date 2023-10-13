using KanonBot.Drivers;
using KanonBot.Message;
using KanonBot.API;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Jpeg;
using KanonBot.Functions.OSU.RosuPP;
using System.IO;
using LanguageExt.UnsafeValueAccess;

namespace KanonBot.Functions.OSUBot
{
    public class Score
    {
        async public static Task Execute(Target target, string cmd)
        {
            #region 验证
            long? osuID = null;
            API.OSU.Enums.Mode? mode;
            Database.Model.User? DBUser = null;
            Database.Model.UserOSU? DBOsuInfo = null;

            // 解析指令
            var command = BotCmdHelper.CmdParser(cmd, BotCmdHelper.FuncType.Score);
            mode = command.osu_mode;

            // 解析指令
            if (command.self_query)
            {
                // 验证账户
                var AccInfo = Accounts.GetAccInfo(target);
                DBUser = await Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
                if (DBUser == null)
                // { await target.reply("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。"); return; }    // 这里引导到绑定osu
                {
                    await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。");
                    return;
                }
                // 验证账号信息
                DBOsuInfo = await Accounts.CheckOsuAccount(DBUser.uid);
                if (DBOsuInfo == null)
                {
                    await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。");
                    return;
                }

                mode ??= API.OSU.Enums.String2Mode(DBOsuInfo.osu_mode)!.Value; // 从数据库解析，理论上不可能错
                osuID = DBOsuInfo.osu_uid;
            }
            else
            {
                // 查询用户是否绑定
                // 这里先按照at方法查询，查询不到就是普通用户查询
                var (atOSU, atDBUser) = await Accounts.ParseAt(command.osu_username);
                if (atOSU.IsNone && !atDBUser.IsNone) {
                    await target.reply("ta还没有绑定osu账户呢。");
                    return;
                } else if (!atOSU.IsNone && atDBUser.IsNone) {
                    var _osuinfo = atOSU.ValueUnsafe();
                    mode ??= _osuinfo.PlayMode;
                    osuID = _osuinfo.Id;
                } else if (!atOSU.IsNone && !atDBUser.IsNone) {
                    DBUser = atDBUser.ValueUnsafe();
                    DBOsuInfo = await Accounts.CheckOsuAccount(DBUser.uid);
                    var _osuinfo = atOSU.ValueUnsafe();
                    mode ??= API.OSU.Enums.String2Mode(DBOsuInfo!.osu_mode)!.Value ;
                    osuID = _osuinfo.Id;
                } else {
                    // 普通查询
                    var OnlineOsuInfo = await API.OSU.GetUser(
                        command.osu_username,
                        command.osu_mode ?? API.OSU.Enums.Mode.OSU
                    );
                    if (OnlineOsuInfo != null)
                    {
                        DBOsuInfo = await Database.Client.GetOsuUser(OnlineOsuInfo.Id);
                        if (DBOsuInfo != null)
                        {
                            DBUser = await Accounts.GetAccountByOsuUid(OnlineOsuInfo.Id);
                            mode ??= API.OSU.Enums.String2Mode(DBOsuInfo.osu_mode)!.Value;
                        }
                        mode ??= OnlineOsuInfo.PlayMode;
                        osuID = OnlineOsuInfo.Id;
                    }
                    else
                    {
                        // 直接取消查询，简化流程
                        await target.reply("猫猫没有找到此用户。");
                        return;
                    }
                }
            }

            // 验证osu信息
            var tempOsuInfo = await API.OSU.GetUser(osuID!.Value, mode!.Value);
            if (tempOsuInfo == null)
            {
                if (DBOsuInfo != null)
                    await target.reply("被办了。");
                else
                    await target.reply("猫猫没有找到此用户。");
                // 中断查询
                return;
            }

            #endregion

            // 解析Mod
            List<string> mods = new();
            try
            {
                mods = Enumerable
                    .Range(0, command.osu_mods.Length / 2)
                    .Select(p => new string(command.osu_mods.AsSpan().Slice(p * 2, 2)).ToUpper())
                    .ToList();
            }
            catch { }

            // 判断是否给定了bid
            if (command.order_number == -1)
            {
                await target.reply("请提供谱面bid。");
                return;
            }

            var scoreData = await API.OSU.GetUserBeatmapScore(
                osuID!.Value,
                command.order_number,
                mods.ToArray(),
                mode!.Value
            );

            if (scoreData == null)
            {
                if (command.self_query)
                    await target.reply("猫猫没有找到你的成绩");
                else
                    await target.reply("猫猫没有找到TA的成绩");
                return;
            }
            //ppy的getscore api不会返回beatmapsets信息，需要手动获取
            var beatmapSetInfo = await API.OSU.GetBeatmap(scoreData!.Score.Beatmap!.BeatmapId);
            scoreData.Score.Beatmapset = beatmapSetInfo!.Beatmapset;

            var data = await PerformanceCalculator.CalculatePanelData(scoreData.Score);
            using var stream = new MemoryStream();
            using var img = await LegacyImage.Draw.DrawScore(data);
            await img.SaveAsync(stream, command.res ? new PngEncoder() : new JpegEncoder());
            await target.reply(
                new Chain().image(
                    Convert.ToBase64String(stream.ToArray(), 0, (int)stream.Length),
                    ImageSegment.Type.Base64
                )
            );
        }
    }
}
