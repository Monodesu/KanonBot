using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KanonBot.Command;
using KanonBot.Drivers;
using LanguageExt.UnsafeValueAccess;
using LanguageExt.SomeHelp;
using KanonBot.OSU;

namespace KanonBot
{
    public static class BindService
    {
        public struct AccInfo
        {
            public required Platform platform;
            public required string uid;
        }

        [Command("bind", "link")]
        [Params("osu", "steam")]
        public async static Task Bind(CommandContext args, Target target)
        {
            var op_osu = args.GetParameter<string>("osu");

            await op_osu.Match
                (
                Some: async op =>
                {
                    await HandleOSULink(target, op);
                },
                None: async () =>
                {
                    await target.reply("请按照以下格式进行绑定。\n!link osu=您的osu用户名 ");
                    return;
                }
                );
            await Task.CompletedTask;
        }

        private async static Task HandleOSULink(Target target, string osu_username)
        {
            var (baseuid, platform) = RetrieveCurrentUserInfo(target);
            var online_osu_userinfo = await API.OSU.V2.GetUser(osu_username);

            if (online_osu_userinfo == null)
            {
                await target.reply($"没有找到osu用户名为 {osu_username} 的osu用户，绑定失败。");
                return;
            }

            var db_osu_userinfo = await CheckOSUUserBinding(online_osu_userinfo.Id);
            if (db_osu_userinfo != null)
            {
                if (baseuid == db_osu_userinfo.uid.ToString())
                {
                    await target.reply($"你已绑定该账户。");
                    return;
                }
                await target.reply($"此osu账户已被用户ID为 {db_osu_userinfo.uid} 的用户绑定了，如果这是你的账户，请联系管理员更新账户信息。");
                return;
            }

            var DBUser = await GetBaseAccount(baseuid, platform);
            if (DBUser == null)
            {
                await target.reply("您还没有绑定desu.life账户，请使用!reg 您的邮箱来进行绑定或注册。");
                return;
            }

            var osuuserinfo = await CheckCurrentUserOSUBinding(long.Parse(baseuid));
            if (osuuserinfo != null)
            {
                await target.reply($"您已经与osu uid为 {osuuserinfo.osu_uid} 的用户绑定过了。");
                return;
            }

            try
            {
                if (await PerformBinding(long.Parse(baseuid), online_osu_userinfo))
                {
                    await target.reply($"绑定成功，已将osu用户 {online_osu_userinfo.Id} 绑定至desu.life账户 {baseuid} 。");
                    await GeneralUpdate.UpdateUser(online_osu_userinfo.Id, true);
                }
                else
                {
                    await target.reply($"绑定失败，请稍后再试。");
                }
            }
            catch
            {
                await target.reply($"在绑定用户时出错，请联系管理员。");
                return;
            }
        }

        public static (string baseuid, Platform platform) RetrieveCurrentUserInfo(Target target)
        {
            var AccInfo = GetBaseAccInfo(target);
            return (AccInfo.uid, AccInfo.platform);
        }

        public async static Task<Database.Models.UserOSU?> CheckOSUUserBinding(long osuUserId)
        {
            return await Database.Client.GetOsuUser(osuUserId);
        }

        public async static Task<Database.Models.UserOSU?> CheckCurrentUserOSUBinding(long base_uid)
        {
            return await Database.Client.GetOsuUserByUID(base_uid);
        }

        private async static Task<bool> PerformBinding(long desulife_uid, API.OSU.Models.User online_osu_userinfo)
        {
            if (online_osu_userinfo.CoverUrl == null) online_osu_userinfo.CoverUrl = new Uri("");
            return await Database.Client.InsertOsuUser(desulife_uid, online_osu_userinfo.Id, online_osu_userinfo.CoverUrl.ToString() == "" ? 0 : 2);
        }

        public static async Task<(Option<API.OSU.Models.User>, Option<Database.Models.User>)> ParseAt(string atmsg)
        {
            var res = SplitKvp(atmsg);
            if (res.IsNone)
                return (None, None);

            var (k, v) = res.Value();
            if (k == "osu")
            {
                var uid = parseLong(v).IfNone(() => 0);
                if (uid == 0)
                    return (None, None);

                var osuacc_ = await API.OSU.V2.GetUser(uid);
                if (osuacc_ is null)
                    return (None, None);

                var dbuser_ = await GetBaseAccountByOsuUid(uid);
                if (dbuser_ is null)
                    return (Some(osuacc_!), None);
                else
                    return (Some(osuacc_!), Some(dbuser_!));
            }

            var platform = k switch
            {
                "qq" => Platform.OneBot,
                "gulid" => Platform.Guild,
                "discord" => Platform.Discord,
                "kook" => Platform.KOOK,
                _ => Platform.Unknown
            };
            if (platform == Platform.Unknown)
                return (None, None);

            var dbuser = await GetBaseAccount(v, platform);
            if (dbuser is null)
                return (None, None);

            var dbosu = await CheckOsuAccount(dbuser.uid);
            if (dbosu is null)
                return (None, Some(dbuser!));

            var osuacc = await API.OSU.V2.GetUser(dbosu.osu_uid);
            if (osuacc is null)
                return (None, Some(dbuser!));
            else
                return (Some(osuacc!), Some(dbuser!));
        }

        public static async Task<Database.Models.User?> GetBaseAccount(string uid, Platform platform)
        {
            return await Database.Client.GetUsersByUID(uid, platform);
        }

        public static async Task<Database.Models.User?> GetBaseAccountByOsuUid(long osu_uid)
        {
            return await Database.Client.GetUserByOsuUID(osu_uid);
        }

        public static async Task<Database.Models.UserOSU?> CheckOsuAccount(long uid)
        {
            return await Database.Client.GetOsuUserByUID(uid);
        }

        public static AccInfo GetBaseAccInfo(Target target)
        {
            switch (target.platform)
            {
                case Platform.Guild:
                    if (target.raw is Guild.Models.MessageData g)
                    {
                        return new AccInfo() { platform = Platform.Guild, uid = g.Author.ID };
                    }
                    break;
                case Platform.OneBot:
                    if (target.raw is OneBot.Models.CQMessageEventBase o)
                    {
                        return new AccInfo() { platform = Platform.OneBot, uid = o.UserId.ToString() };
                    }
                    break;
                case Platform.KOOK:
                    if (target.raw is Kook.WebSocket.SocketMessage k)
                    {
                        return new AccInfo() { platform = Platform.KOOK, uid = k.Author.Id.ToString() };
                    }
                    break;
                case Platform.Discord:
                    if (target.raw is Discord.WebSocket.SocketMessage d)
                    {
                        return new AccInfo() { platform = Platform.Discord, uid = d.Author.Id.ToString() };
                    }
                    break;
            }
            return new() { platform = Platform.Unknown, uid = "" };
        }

    }
}
