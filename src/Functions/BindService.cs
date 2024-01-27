using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KanonBot.Command;
using KanonBot.Drivers;
using KanonBot.OSU;
using LanguageExt.SomeHelp;
using LanguageExt.UnsafeValueAccess;
using static KanonBot.Config;

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
        public static async Task Bind(CommandContext args, Target target)
        {
            string? token = null;
            args.GetDefault<string>().IfSome(u => token = u);

            if (string.IsNullOrEmpty(token))
            {
                await target.reply("请指定Token。");
                return;

            }

            // get email
            var email = await Database.Client.GetEmailAddressByVerifyToken(token, "link", "qq");

            if (email == null)
            {
                await target.reply("Token无效。");
                return;
            }

            if (target.platform == Platform.OneBot)
            {
                if (!await Database.Client.LinkOneBot(email, long.Parse(target.sender!)))
                {
                    await target.reply("连接失败了！请联系管理员。");
                    return;
                }
                await target.reply("连接成功~");
            }
            else if (target.platform == Platform.Guild)
            {
                // 检查是否已绑定
                if (await Database.Client.GetUserByQQGuildID(target.sender!) != null)
                {
                    await target.reply("已经连接过了哦，不可以再连接了。");
                    return;
                }

                var userInfo = await Database.Client.GetUser(email);
                if (!await Database.Client.LinkQQGuild(userInfo!.uid, target.sender!))
                {
                    await target.reply("连接失败了！请联系管理员。");
                    return;
                }
                await target.reply("连接成功~");
            }
            else
            {
                await target.reply("该指令在当前平台不可用。");
            }
        }

        public static (string baseuid, Platform platform) RetrieveCurrentUserInfo(Target target)
        {
            var AccInfo = GetBaseAccInfo(target);
            return (AccInfo.uid, AccInfo.platform);
        }

        public static async Task<(
            Option<API.OSU.Models.User>,
            Option<Database.Models.User>
        )> ParseAt(string atmsg)
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

            var osuacc = await API.OSU
                .V2
                .GetUser(
                    dbosu.osu_uid,
                    (API.OSU.Enums.Mode)API.OSU.Enums.String2Mode(dbosu.osu_mode)!
                );
            if (osuacc is null)
                return (None, Some(dbuser!));
            else
                return (Some(osuacc!), Some(dbuser!));
        }

        public static async Task<Database.Models.User?> GetBaseAccount(
            string uid,
            Platform platform
        )
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
                    if (target.raw is KanonBot.Drivers.Guild.Models.MessageData g)
                    {
                        return new AccInfo() { platform = Platform.Guild, uid = g.Author.ID };
                    }
                    break;
                case Platform.OneBot:
                    if (target.raw is KanonBot.Drivers.OneBot.Models.CQMessageEventBase o)
                    {
                        return new AccInfo()
                        {
                            platform = Platform.OneBot,
                            uid = o.UserId.ToString()
                        };
                    }
                    break;
                case Platform.KOOK:
                    if (target.raw is Kook.WebSocket.SocketMessage k)
                    {
                        return new AccInfo()
                        {
                            platform = Platform.KOOK,
                            uid = k.Author.Id.ToString()
                        };
                    }
                    break;
                case Platform.Discord:
                    if (target.raw is Discord.WebSocket.SocketMessage d)
                    {
                        return new AccInfo()
                        {
                            platform = Platform.Discord,
                            uid = d.Author.Id.ToString()
                        };
                    }
                    break;
            }
            return new() { platform = Platform.Unknown, uid = "" };
        }
    }
}
