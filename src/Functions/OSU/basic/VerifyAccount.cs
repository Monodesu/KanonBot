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
using System.Runtime.CompilerServices;

namespace KanonBot.OSU
{
    public static partial class Basic
    {
        public static async Task<(
            Database.Models.User?,
            Database.Models.UserOSU?,
            API.OSU.Models.User?
        )> GetOSUOperationInfo(
            Target target,
            bool isSelfQuery,
            string osu_username,
            API.OSU.Enums.Mode? mode
        ) =>
            isSelfQuery
                ? await VerifyBaseAccount(target, mode)
                : await QueryOtherUser(target, osu_username, mode);

        private static async Task<(
            Database.Models.User?,
            Database.Models.UserOSU?,
            API.OSU.Models.User?
        )> VerifyBaseAccount(Target target, API.OSU.Enums.Mode? mode)
        {
            Database.Models.User? DBUser = null;
            Database.Models.UserOSU? DBOsuInfo = null;
            API.OSU.Models.User? OnlineOsuInfo = null;
            var (baseuid, platform) = RetrieveCurrentUserInfo(target);
            DBUser = await GetBaseAccount(baseuid, platform);
            if (DBUser == null)
            {
                await target.reply("您还没有绑定desu.life账户，请使用!link email=您的邮箱 来进行绑定或注册。");
                return (DBUser, DBOsuInfo, OnlineOsuInfo);
            }
            DBOsuInfo = await CheckOsuAccount(DBUser.uid);
            if (DBOsuInfo == null)
            {
                await target.reply("您还没有绑定osu账户，请使用!link osu=您的osu用户名 来绑定您的osu账户。");
                return (DBUser, DBOsuInfo, OnlineOsuInfo);
            }
            mode ??= API.OSU.Enums.String2Mode(DBOsuInfo.osu_mode);
            OnlineOsuInfo = await API.OSU.V2.GetUser(DBOsuInfo.osu_uid, (API.OSU.Enums.Mode)mode!);
            if (OnlineOsuInfo == null)
            {
                if (DBOsuInfo != null)
                    await target.reply("被办了。");
                return (DBUser, DBOsuInfo, OnlineOsuInfo);
            }
            return (DBUser, DBOsuInfo, OnlineOsuInfo);
        }

        private static async Task<(
            Database.Models.User?,
            Database.Models.UserOSU?,
            API.OSU.Models.User?
        )> QueryOtherUser(Target target, string osu_username, API.OSU.Enums.Mode? mode)
        {
            // 查询用户是否绑定，这里先按照at方法查询，查询不到就是普通用户查询
            Database.Models.User? DBUser = null;
            Database.Models.UserOSU? DBOsuInfo = null;
            API.OSU.Models.User? OnlineOsuInfo = null;
            var (atOSU, atDBUser) = await ParseAt(osu_username);
            if (atOSU.IsNone && !atDBUser.IsNone)
            {
                await target.reply("ta还没有绑定osu账户呢。");
                return (DBUser, DBOsuInfo, OnlineOsuInfo);
            }
            else if (!atOSU.IsNone && atDBUser.IsNone)
            {
                OnlineOsuInfo = atOSU.ValueUnsafe();
                if (mode != null)
                {
                    if (OnlineOsuInfo.PlayMode != (API.OSU.Enums.Mode)mode)
                    {
                        OnlineOsuInfo = await API.OSU.V2.GetUser(
                            osu_username,
                            (API.OSU.Enums.Mode)mode
                        );
                    }
                }
            }
            else if (!atOSU.IsNone && !atDBUser.IsNone)
            {
                DBUser = atDBUser.ValueUnsafe();
                DBOsuInfo = await CheckOsuAccount(DBUser.uid);
                OnlineOsuInfo = atOSU.ValueUnsafe();
                if (mode != null)
                {
                    if (OnlineOsuInfo.PlayMode != (API.OSU.Enums.Mode)mode)
                    {
                        OnlineOsuInfo = await API.OSU.V2.GetUser(
                            osu_username,
                            (API.OSU.Enums.Mode)mode
                        );
                    }
                }
            }
            else
            {
                // 普通查询
                var (DBUser_, DBOsuInfo_, OnlineOsuInfo_) = await VerifyOSUAccount(
                    target,
                    osu_username,
                    mode
                );
                OnlineOsuInfo = OnlineOsuInfo_;
                DBOsuInfo = DBOsuInfo_;
                DBUser = DBUser_;
                return (DBUser, DBOsuInfo, OnlineOsuInfo);
            }
            if (OnlineOsuInfo == null)
            {
                if (DBOsuInfo != null)
                    await target.reply("被办了。");
                else
                    await target.reply("猫猫没有找到此用户。");
                return (null, null, null);
            }
            return (DBUser, DBOsuInfo, OnlineOsuInfo);
        }

        private static async Task<(
            Database.Models.User?,
            Database.Models.UserOSU?,
            API.OSU.Models.User?
        )> VerifyOSUAccount(Target target, string osu_username, API.OSU.Enums.Mode? mode)
        {
            Database.Models.User? DBUser = null;
            Database.Models.UserOSU? DBOsuInfo = null;
            API.OSU.Models.User? OnlineOsuInfo = null;

            if (mode == null)
            {
                OnlineOsuInfo = await API.OSU.V2.GetUser(osu_username, API.OSU.Enums.Mode.OSU);
            }
            else
            {
                OnlineOsuInfo = await API.OSU.V2.GetUser(osu_username, (API.OSU.Enums.Mode)mode);
            }

            if (OnlineOsuInfo != null)
            {
                DBOsuInfo = await Database.Client.GetOsuUser(OnlineOsuInfo.Id);
                if (DBOsuInfo != null)
                {
                    DBUser = await GetBaseAccountByOsuUid(OnlineOsuInfo.Id);
                    if (
                        mode == null
                        && API.OSU.Enums.String2Mode(DBOsuInfo.osu_mode)!.Value
                            != OnlineOsuInfo.PlayMode
                    ) // 如果查询的模式與數據庫中用戶保存的不一致，那么就需要重新查询一次
                    {
                        OnlineOsuInfo = await API.OSU.V2.GetUser(
                            osu_username,
                            API.OSU.Enums.String2Mode(DBOsuInfo.osu_mode)!.Value
                        ); // 重新获取正確的用户信息
                    }
                    else if (mode != null && mode != OnlineOsuInfo.PlayMode)
                    {
                        OnlineOsuInfo = await API.OSU.V2.GetUser(
                            osu_username,
                            (API.OSU.Enums.Mode)mode
                        ); // 重新获取正確的用户信息
                    }
                }
                if (OnlineOsuInfo == null)
                {
                    if (DBOsuInfo != null)
                        await target.reply("被办了。");
                    else
                        await target.reply("猫猫没有找到此用户。");
                    return (DBUser, null, null);
                }
                return (DBUser, DBOsuInfo, OnlineOsuInfo);
            }
            else
            {
                // 直接取消查询，简化流程
                await target.reply("猫猫没有找到此用户。");
            }
            return (null, null, null);
        }
    }
}
