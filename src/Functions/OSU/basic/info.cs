using System.CommandLine;
using System.IO;
using KanonBot.API;
using KanonBot.Command;
using KanonBot.Drivers;
using KanonBot.Functions.OSU;
using KanonBot.Functions.OSU.RosuPP;
using KanonBot.Message;
using LanguageExt.UnsafeValueAccess;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using static LinqToDB.Common.Configuration;
using static KanonBot.BindService;
using static System.Runtime.CompilerServices.RuntimeHelpers;
using static KanonBot.API.OSU.DataStructure;
using SixLabors.ImageSharp;

namespace KanonBot.OSU
{
    public static partial class Basic
    {
        private async static Task<API.OSU.Models.PPlusData.UserData> GetPPlusInfo(API.OSU.Models.User user)
        {
            var d = await Database.Client.GetOsuPPlusData(user.Id);
            if (d != null)
            {
                return d;
            }
            else
            {
                // 异步获取osupp数据，下次请求的时候就有了
                new Task(async () =>
                {
                    try
                    {
                        await Database.Client.UpdateOsuPPlusData(
                            (await API.OSU.V2.TryGetUserPlusData(user!))!.User!,
                            user!.Id
                        );
                    }
                    catch { } //更新pp+失败，不返回信息
                }).RunSynchronously();
                return new();
            }
        }

        [Command("info", "stat")]
        [Params("m", "mode", "l", "lookback", "q", "quality")]
        public async static Task info(CommandContext args, Target target)
        {
            var osu_username = "";
            bool isSelfQuery = false;
            bool quality = false;
            API.OSU.Enums.Mode? mode = API.OSU.Enums.Mode.OSU;
            int lookback = 0;

            args.GetDefault<string>().Match
                (
                Some: try_name =>
                {
                    osu_username = try_name;
                },
                None: () =>
                {
                    isSelfQuery = true;
                }
                );
            args.GetParameters<string>(["m", "mode"]).Match
                (
                Some: try_mode =>
                {
                    mode = API.OSU.Enums.String2Mode(try_mode) ?? API.OSU.Enums.Mode.OSU;
                },
                None: () => { }
                );

            args.GetParameters<string>(["l", "lookback"]).Match
                (
                Some: try_lookback =>
                {
                    lookback = int.Parse(try_lookback);
                },
                None: () => { }
                );
            args.GetParameters<string>(["q", "quality"]).Match
                (
                Some: try_quality =>
                {
                    if (try_quality == "high" || try_quality == "h")
                        quality = true;
                },
                None: () => { });

            var (DBUser, DBOsuInfo, OnlineOSUUserInfo) = await GetOSUOperationInfo(target, isSelfQuery, osu_username, mode); // 查詢用戶是否有效（是否綁定，是否存在，osu!用戶是否可用），并返回所有信息
            bool IsBound = DBOsuInfo != null;
            if (OnlineOSUUserInfo == null) return; // 查询失败

            // 操作部分
            UserPanelData data = new() { userInfo = OnlineOSUUserInfo };
            data.userInfo.PlayMode = OnlineOSUUserInfo.PlayMode;
            if (IsBound)
            {
                if (lookback > 0)
                {
                    // 从数据库取指定天数前的记录
                    (data.daysBefore, data.prevUserInfo) = await Database.Client.GetOsuUserData(
                        OnlineOSUUserInfo.Id,
                        data.userInfo.PlayMode,
                        lookback
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
                            OnlineOSUUserInfo.Id,
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

            int custominfoengineVer = 2;
            if (IsBound)
            {
                custominfoengineVer = DBOsuInfo!.customInfoEngineVer;
                if (Enum.IsDefined(typeof(UserPanelData.CustomMode), DBOsuInfo.InfoPanelV2_Mode))
                {
                    data.customMode = (UserPanelData.CustomMode)DBOsuInfo.InfoPanelV2_Mode;
                    if (data.customMode == UserPanelData.CustomMode.Custom)
                        data.ColorConfigRaw = DBOsuInfo.InfoPanelV2_CustomMode!;
                }
                else
                {
                    throw new Exception("未知的自定义模式");
                }
            }

            if(custominfoengineVer == 1)
                data.pplusInfo = await GetPPlusInfo(OnlineOSUUserInfo);
            
            using var stream = new MemoryStream();
            SixLabors.ImageSharp.Image img;
            API.OSU.Models.Score[]? allBP = System.Array.Empty<API.OSU.Models.Score>();
            switch (custominfoengineVer) //0=null 1=v1 2=v2
            {
                case 1:
                    img = await Image.OSU.OsuInfoPanelV1.Draw(
                        data,
                        DBOsuInfo != null,
                        false,
                        (IsBound && lookback > 0)
                    );
                    await img.SaveAsync(stream, new PngEncoder());
                    break;
                case 2:
                    var v2Options = data.customMode switch
                    {
                        UserPanelData.CustomMode.Custom => Image.OSU.OsuInfoPanelV2.InfoCustom.ParseColors(data.ColorConfigRaw!, None),
                        UserPanelData.CustomMode.Light => Image.OSU.OsuInfoPanelV2.InfoCustom.LightDefault,
                        UserPanelData.CustomMode.Dark => Image.OSU.OsuInfoPanelV2.InfoCustom.DarkDefault,
                        _ => throw new ArgumentOutOfRangeException("未知的自定义模式")
                    };
                    allBP = await API.OSU.V2.GetUserScores(
                        data.userInfo.Id,
                        API.OSU.Enums.UserScoreType.Best,
                        data.userInfo.PlayMode,
                        100,
                        0
                    );
                    img = await Image.OSU.OsuInfoPanelV2.Draw(
                        data,
                        allBP!,
                        v2Options,
                        DBOsuInfo != null,
                        false,
                        (IsBound && lookback > 0),
                        quality
                    );
                    await img.SaveAsync(stream, new PngEncoder());
                    break;
                default:
                    return;
            }
            img.Dispose(); 
            await target.reply(
                new Chain().image(
                    Convert.ToBase64String(stream.ToArray(), 0, (int)stream.Length),
                    ImageSegment.Type.Base64
                ));



            //try
            //{
            //    if (data.userInfo.PlayMode == API.OSU.Enums.Mode.OSU) //只存std的
            //        if (allBP!.Length > 0)
            //            await InsertBeatmapTechInfo(allBP);
            //        else
            //        {
            //            allBP = await API.OSU.GetUserScores(
            //            data.userInfo.Id,
            //            API.OSU.Enums.UserScoreType.Best,
            //            API.OSU.Enums.Mode.OSU,
            //            100,
            //            0
            //        );
            //            if (allBP!.Length > 0)
            //                await InsertBeatmapTechInfo(allBP);
            //        }
            //}
            //catch { }
        }
    }
}
