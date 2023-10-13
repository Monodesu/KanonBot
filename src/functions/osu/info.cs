using System.IO;
using KanonBot.API;
using KanonBot.Drivers;
using KanonBot.Functions.OSU;
using KanonBot.Functions.OSU.RosuPP;
using KanonBot.Message;
using LanguageExt.UnsafeValueAccess;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using static LinqToDB.Common.Configuration;

namespace KanonBot.Functions.OSUBot
{
    public class Info
    {
        async public static Task Execute(Target target, string cmd)
        {
            #region 验证
            long? osuID = null;
            API.OSU.Enums.Mode? mode;
            Database.Model.User? DBUser = null;
            Database.Model.UserOSU? DBOsuInfo = null;

            // 解析指令
            var command = BotCmdHelper.CmdParser(cmd, BotCmdHelper.FuncType.Info);
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
                if (atOSU.IsNone && !atDBUser.IsNone)
                {
                    await target.reply("ta还没有绑定osu账户呢。");
                    return;
                }
                else if (!atOSU.IsNone && atDBUser.IsNone)
                {
                    var _osuinfo = atOSU.ValueUnsafe();
                    mode ??= _osuinfo.PlayMode;
                    osuID = _osuinfo.Id;
                }
                else if (!atOSU.IsNone && !atDBUser.IsNone)
                {
                    DBUser = atDBUser.ValueUnsafe();
                    DBOsuInfo = await Accounts.CheckOsuAccount(DBUser.uid);
                    var _osuinfo = atOSU.ValueUnsafe();
                    mode ??= API.OSU.Enums.String2Mode(DBOsuInfo!.osu_mode)!.Value;
                    osuID = _osuinfo.Id;
                }
                else
                {
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

            #region 获取信息
            LegacyImage.Draw.UserPanelData data = new()
            {
                userInfo = tempOsuInfo!
            };
            // 覆写
            data.userInfo.PlayMode = mode!.Value;
            // 查询

            if (DBOsuInfo != null)
            {
                if (command.order_number > 0)
                {
                    // 从数据库取指定天数前的记录
                    (data.daysBefore, data.prevUserInfo) = await Database.Client.GetOsuUserData(
                        DBOsuInfo!.osu_uid,
                        data.userInfo.PlayMode,
                        command.order_number
                    );
                    if (data.daysBefore > 0)
                        ++data.daysBefore;
                }
                else
                {
                    // 从数据库取最近的一次记录
                    try
                    {
                        (data.daysBefore, data.prevUserInfo) = await Database.Client.GetOsuUserData(
                            DBOsuInfo!.osu_uid,
                            data.userInfo.PlayMode,
                            0
                        );
                        if (data.daysBefore > 0)
                            ++data.daysBefore;
                    }
                    catch
                    {
                        data.daysBefore = 0;
                    }
                }

                var d = await Database.Client.GetOsuPPlusData(osuID!.Value);
                if (d != null)
                {
                    data.pplusInfo = d;
                }
                else
                {
                    // 设置空数据
                    data.pplusInfo = new();
                    // 异步获取osupp数据，下次请求的时候就有了
                    new Task(async () =>
                    {
                        try
                        {
                            await Database.Client.UpdateOsuPPlusData(
                                (await API.OSU.TryGetUserPlusData(tempOsuInfo!))!.User,
                                tempOsuInfo!.Id
                            );
                        }
                        catch { } //更新pp+失败，不返回信息
                    }).RunSynchronously();
                }

                var badgeID = DBUser!.displayed_badge_ids;
                // 由于v1v2绘制位置以及绘制方向的不同，legacy(v1)只取第一个badge
                if (badgeID != null)
                {
                    try
                    {
                        if (badgeID!.IndexOf(",") != -1)
                        {
                            var y = badgeID.Split(",");
                            foreach (var x in y)
                                data.badgeId.Add(int.Parse(x));
                        }
                        else
                        {
                            data.badgeId.Add(int.Parse(badgeID!));
                        }
                    }
                    catch
                    {
                        data.badgeId = new() { -1 };
                    }
                }
                else
                {
                    data.badgeId = new() { -1 };
                }
            }
            else
            {
                var d = await Database.Client.GetOsuPPlusData(osuID!.Value);
                if (d != null)
                {
                    data.pplusInfo = d;
                }
                else
                {
                    // 设置空数据
                    data.pplusInfo = new();
                    // 异步获取osupp数据，下次请求的时候就有了
                    new Task(async () =>
                    {
                        try
                        {
                            var temppppinfo = await API.OSU.TryGetUserPlusData(tempOsuInfo!);
                            if (temppppinfo == null)
                                return;
                            await Database.Client.UpdateOsuPPlusData(
                                temppppinfo!.User,
                                tempOsuInfo!.Id
                            );
                        }
                        catch { } //更新pp+失败，不返回信息
                    }).RunSynchronously();
                }
            }

            #endregion

            var isDataOfDayAvaiavle = false;
            if (data.daysBefore > 0)
                isDataOfDayAvaiavle = true;

            int custominfoengineVer = 2;
            if (DBOsuInfo != null)
            {
                custominfoengineVer = DBOsuInfo!.customInfoEngineVer;
                if (Enum.IsDefined(typeof(LegacyImage.Draw.UserPanelData.CustomMode), DBOsuInfo.InfoPanelV2_Mode))
                {
                    data.customMode = (LegacyImage.Draw.UserPanelData.CustomMode)DBOsuInfo.InfoPanelV2_Mode;
                    if (data.customMode == LegacyImage.Draw.UserPanelData.CustomMode.Custom)
                        data.ColorConfigRaw = DBOsuInfo.InfoPanelV2_CustomMode!;
                }
                else
                {
                    throw new Exception("未知的自定义模式");
                }
            }

            using var stream = new MemoryStream();
            //info默认输出高质量图片？
            SixLabors.ImageSharp.Image img;
            API.OSU.Models.Score[]? allBP = System.Array.Empty<API.OSU.Models.Score>();
            switch (custominfoengineVer) //0=null 1=v1 2=v2
            {
                case 1:
                    img = await LegacyImage.Draw.DrawInfo(
                        data,
                        DBOsuInfo != null,
                        isDataOfDayAvaiavle
                    );
                    //await img.SaveAsync(stream, command.res ? new PngEncoder() : new JpegEncoder());
                    await img.SaveAsync(stream, new PngEncoder());
                    break;
                case 2:
                    var v2Options = data.customMode switch
                    {
                        LegacyImage.Draw.UserPanelData.CustomMode.Custom => DrawV2.OsuInfoPanelV2.InfoCustom.ParseColors(data.ColorConfigRaw, None),
                        LegacyImage.Draw.UserPanelData.CustomMode.Light => DrawV2.OsuInfoPanelV2.InfoCustom.LightDefault,
                        LegacyImage.Draw.UserPanelData.CustomMode.Dark => DrawV2.OsuInfoPanelV2.InfoCustom.DarkDefault,
                        _ => throw new ArgumentOutOfRangeException("未知的自定义模式")
                    };
                    allBP = await API.OSU.GetUserScores(
                        data.userInfo.Id,
                        API.OSU.Enums.UserScoreType.Best,
                        data.userInfo.PlayMode,
                        100,
                        0
                    );
                    img = await DrawV2.OsuInfoPanelV2.Draw(
                        data,
                        allBP!,
                        v2Options,
                        DBOsuInfo != null,
                        false,
                        isDataOfDayAvaiavle,
                        command.res
                    );
                    await img.SaveAsync(stream, new PngEncoder());
                    break;
                default:
                    return;
            }
            // 关闭流
            img.Dispose();


            await target.reply(
                new Chain().image(
                    Convert.ToBase64String(stream.ToArray(), 0, (int)stream.Length),
                    ImageSegment.Type.Base64
                )
            );
            try
            {
                if (data.userInfo.PlayMode == API.OSU.Enums.Mode.OSU) //只存std的
                    if (allBP!.Length > 0)
                        await InsertBeatmapTechInfo(allBP);
                    else
                    {
                        allBP = await API.OSU.GetUserScores(
                        data.userInfo.Id,
                        API.OSU.Enums.UserScoreType.Best,
                        API.OSU.Enums.Mode.OSU,
                        100,
                        0
                    );
                        if (allBP!.Length > 0)
                            await InsertBeatmapTechInfo(allBP);
                    }
            }
            catch { }
        }

        async public static Task InsertBeatmapTechInfo(API.OSU.Models.Score[] allbp)
        {
            foreach (var score in allbp)
            {
                //计算pp
                try
                {
                    if (score.Rank.ToUpper() == "XH" ||
                           score.Rank.ToUpper() == "X" ||
                           score.Rank.ToUpper() == "SH" ||
                           score.Rank.ToUpper() == "S" ||
                           score.Rank.ToUpper() == "A")
                    {
                        var data = await PerformanceCalculator.CalculatePanelData(score);
                        await Database.Client.InsertOsuStandardBeatmapTechData(
                        score.Beatmap!.BeatmapId,
                        data.ppInfo.star,
                                    (int)data.ppInfo.ppStats![0].total,
                                    (int)data.ppInfo.ppStats![0].acc!,
                                    (int)data.ppInfo.ppStats![0].speed!,
                                    (int)data.ppInfo.ppStats![0].aim!,
                                    (int)data.ppInfo.ppStats![1].total,
                                    (int)data.ppInfo.ppStats![2].total,
                                    (int)data.ppInfo.ppStats![3].total,
                                    (int)data.ppInfo.ppStats![4].total,
                                    score.Mods
                                );
                    }
                }
                catch
                {
                    //无视错误
                }
            }
        }
    }
}
