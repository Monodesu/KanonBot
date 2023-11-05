using KanonBot.Account;
using KanonBot.Drivers;
using KanonBot;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KanonBot.API;
using static KanonBot.BindService;
using LanguageExt.UnsafeValueAccess;

namespace KanonBot.OSU
{
    public static partial class Basic
    {

        async static public Task<(long osu_uid, API.OSU.Enums.Mode mode)> GetOSUOperationInfo(Target target, bool isSelfQuery, string osu_username)
        {
            API.OSU.Enums.Mode? mode = null;
            long osu_uid = 0;
            if (isSelfQuery)
            {
                var (base_uid, osuID, osu_mode) = await VerifyBaseAccount(target);
                if (osuID == 0) return (0, API.OSU.Enums.Mode.Unknown); // 查询失败
                mode ??= API.OSU.Enums.String2Mode(osu_mode)!.Value;
                osu_uid = osuID;
            }
            else
            {
                var (osuID, osu_mode) = await QueryOtherUser(target, osu_username, mode);
                if (osuID == 0) return (0, API.OSU.Enums.Mode.Unknown); // 查询失败
                mode = osu_mode;
                osu_uid = osuID;
            }
            return (osu_uid, mode.Value);
        }

        async static private Task<(string base_uid, long osu_uid, string? osu_mode)> VerifyBaseAccount(Target target)
        {
            Database.Models.User? DBUser = null;
            Database.Models.UserOSU? DBOsuInfo = null;
            var (baseuid, platform) = RetrieveCurrentUserInfo(target);
            DBUser = await GetBaseAccount(baseuid, platform);
            if (DBUser == null)
            {
                await target.reply("您还没有绑定desu.life账户，请使用!link email=您的邮箱 来进行绑定或注册。");
                return ("", 0, "");
            }
            DBOsuInfo = await CheckOsuAccount(DBUser.uid);
            if (DBOsuInfo == null)
            {
                await target.reply("您还没有绑定osu账户，请使用!link osu=您的osu用户名 来绑定您的osu账户。");
                return ("", 0, "");
            }
            return (DBUser.uid.ToString(), DBOsuInfo.osu_uid, DBOsuInfo.osu_mode);
        }

        async static private Task<(long osu_uid, API.OSU.Enums.Mode osu_mode)> QueryOtherUser(Target target, string osu_username, API.OSU.Enums.Mode? mode)
        {
            // 查询用户是否绑定，这里先按照at方法查询，查询不到就是普通用户查询
            Database.Models.User? DBUser = null;
            Database.Models.UserOSU? DBOsuInfo = null;
            long osuID = 0;

            var (atOSU, atDBUser) = await ParseAt(osu_username);
            if (atOSU.IsNone && !atDBUser.IsNone)
            {
                await target.reply("ta还没有绑定osu账户呢。");
                return (0, API.OSU.Enums.Mode.Unknown);
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
                DBOsuInfo = await CheckOsuAccount(DBUser.uid);
                var _osuinfo = atOSU.ValueUnsafe();
                mode ??= API.OSU.Enums.String2Mode(DBOsuInfo!.osu_mode)!.Value;
                osuID = _osuinfo.Id;
            }
            else
            {
                // 普通查询
                var (osu_uid, osu_mode) = await VerifyOSUAccount(target, osu_username, mode);
                if (osu_uid == 0) return (osu_uid, API.OSU.Enums.Mode.Unknown); // 查询失败
                mode ??= API.OSU.Enums.String2Mode(osu_mode)!.Value;
                osuID = osu_uid;
            }
            return (osuID, (API.OSU.Enums.Mode)mode);
        }

        async static private Task<(long osu_uid, string? osu_mode)> VerifyOSUAccount(Target target, string osu_username, API.OSU.Enums.Mode? mode)
        {
            Database.Models.User? DBUser = null;
            Database.Models.UserOSU? DBOsuInfo = null;

            var OnlineOSUUserInfo = await API.OSU.V2.GetUser(osu_username, mode ?? API.OSU.Enums.Mode.OSU);
            if (OnlineOSUUserInfo != null)
            {
                DBOsuInfo = await Database.Client.GetOsuUser(OnlineOSUUserInfo.Id);
                if (DBOsuInfo != null)
                {
                    DBUser = await GetBaseAccountByOsuUid(OnlineOSUUserInfo.Id);
                    mode ??= API.OSU.Enums.String2Mode(DBOsuInfo.osu_mode)!.Value;
                }
                mode ??= OnlineOSUUserInfo.PlayMode;
                return (OnlineOSUUserInfo.Id, OnlineOSUUserInfo.PlayMode.ToString());
            }
            else
            {
                // 直接取消查询，简化流程
                await target.reply("猫猫没有找到此用户。");
            }
            return (0, "");
        }

    }
}
