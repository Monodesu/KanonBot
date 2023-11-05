using Flurl.Util;
using KanonBot.API;
using KanonBot.OSU;
using KanonBot.Drivers;
using KanonBot.Message;
using System.Net;
using KanonBot.Command;

namespace KanonBot
{
    public static class BaseAccount
    {
        public enum AccountOperation
        {
            None,
            Register,
            Verify,
            AppendEmail,
        }

        public static async Task RegAccount(Target target, string cmd)
        {
            var mailAddr = cmd.Trim();
            var verifyCode = RandomStr(32, true);
            if (!IsMailAddr(mailAddr))
            {
                await target.reply("请输入有效的电子邮件地址。");
                return;
            }

            var dbuser = await GetDbUser(mailAddr);
            var op = await GetUserIdAndCheckBindStatusAsync(target, dbuser);
            if (op == AccountOperation.None) return;

            string platform = GetPlatform(target.platform);
            string uid = GetPlatformUID(target);
            string read_html = System.IO.File.ReadAllText("./mail_desu_life_mailaddr_verify_template.txt");

            await SendVerificationMail(op, target, mailAddr, verifyCode, platform, uid);

        }

        private static async Task<Database.Models.User?> GetDbUser(string mailAddr)
        {
            bool mail_has_regd = await Database.Client.IsRegd(mailAddr);
            return mail_has_regd ? (await Database.Client.GetUser(mailAddr))! : null;
        }

        private static async Task<AccountOperation> GetUserIdAndCheckBindStatusAsync(Target target, Database.Models.User? dbuser)
        {
            return target.platform switch
            {
                Platform.Guild => await HandleGuildAsync(target, dbuser),
                Platform.OneBot => await HandleOneBotAsync(target, dbuser),
                Platform.KOOK => await HandleKookAsync(target, dbuser),
                Platform.Discord => await HandleDiscordAsync(target, dbuser),
                _ => AccountOperation.None,
            };
        }

        private static async Task<AccountOperation> HandleGuildAsync(Target target, Database.Models.User? dbuser)
        {
            if (target.raw is Guild.Models.MessageData g)
            {
                bool mail_has_regd = dbuser != null;
                if (mail_has_regd)
                {
                    if (dbuser!.qq_guild_uid == g.Author.ID)
                    {
                        return await ReplyUserBoundMsg(target);
                    }
                }

                var g1 = await Database.Client.GetUsersByUID(g.Author.ID, Platform.Guild);
                if (g1 != null)
                {
                    if (g1.email != null)
                    {
                        return AccountOperation.AppendEmail;
                    }
                    else
                    {
                        return await ReplyUserExistingMsg(target, g1.email);
                    }
                }
            }
            return AccountOperation.None;
        }

        private static async Task<AccountOperation> HandleOneBotAsync(Target target, Database.Models.User? dbuser)
        {
            if (target.raw is OneBot.Models.CQMessageEventBase o)
            {
                bool mail_has_regd = dbuser != null;
                if (mail_has_regd)
                {
                    if (dbuser!.qq_id == o.UserId)
                    {
                        return await ReplyUserBoundMsg(target);
                    }
                }

                var o1 = await Database.Client.GetUsersByUID(o.UserId.ToString(), Platform.OneBot);
                if (o1 != null)
                {
                    if (o1.email != null)
                    {
                        return AccountOperation.AppendEmail;
                    }
                    else
                    {
                        return await ReplyUserExistingMsg(target, o1.email);
                    }
                }
            }
            return AccountOperation.None;
        }

        private static async Task<AccountOperation> HandleKookAsync(Target target, Database.Models.User? dbuser)
        {
            if (target.raw is Kook.WebSocket.SocketMessage k)
            {
                bool mail_has_regd = dbuser != null;
                if (mail_has_regd)
                {
                    if (dbuser!.kook_uid == k.Author.Id.ToString())
                    {
                        return await ReplyUserBoundMsg(target);
                    }
                }
                var k1 = await Database.Client.GetUsersByUID(k.Author.Id.ToString(), Platform.KOOK);
                if (k1 != null)
                {
                    if (k1.email != null)
                    {
                        return AccountOperation.AppendEmail;
                    }
                    else
                    {
                        return await ReplyUserExistingMsg(target, k1.email);
                    }
                }
            }
            return AccountOperation.None;
        }

        private static async Task<AccountOperation> HandleDiscordAsync(Target target, Database.Models.User? dbuser)
        {
            if (target.raw is Discord.WebSocket.SocketMessage d)
            {
                bool mail_has_regd = dbuser != null;
                if (mail_has_regd)
                {
                    if (dbuser!.discord_uid == d.Author.Id.ToString())
                    {
                        return await ReplyUserBoundMsg(target);
                    }
                }
                var d1 = await Database.Client.GetUsersByUID(d.Author.Id.ToString(), Platform.Discord);
                if (d1 != null)
                {
                    if (d1.email != null)
                    {
                        return AccountOperation.AppendEmail;
                    }
                    else
                    {
                        return await ReplyUserExistingMsg(target, d1.email);
                    }
                }
            }
            return AccountOperation.None;
        }

        private static string GetPlatform(Platform platform)
        {
            return platform switch
            {
                Platform.Guild => "qguild",
                Platform.KOOK => "kook",
                Platform.OneBot => "qq",
                Platform.Discord => "discord",
                _ => throw new NotSupportedException()
            };
        }

        private static string GetPlatformUID(Target target)
        {
            switch (target.platform)
            {
                case Platform.Guild:
                    if (target.raw is Guild.Models.MessageData g) return g.Author.ID.ToString();
                    break;
                case Platform.OneBot:
                    if (target.raw is OneBot.Models.CQMessageEventBase o) return o.UserId.ToString();
                    break;
                case Platform.KOOK:
                    if (target.raw is Kook.WebSocket.SocketMessage k) return k.Author.Id.ToString();
                    break;
                case Platform.Discord:
                    if (target.raw is Discord.WebSocket.SocketMessage d) return d.Author.Id.ToString();
                    break;
                default:
                    throw new NotSupportedException();
            }
            throw new NotSupportedException();
        }

        private static async Task<AccountOperation> ReplyUserExistingMsg(Target target, string? email)
        {
            await target.reply(new Chain()
                            .msg($"您目前的平台账户已经和邮箱为" +
                            $"{HideMailAddr(email ?? "undefined@undefined.undefined")}" +
                            $"的用户绑定了。"));
            return AccountOperation.None;
        }

        private static async Task<AccountOperation> ReplyUserBoundMsg(Target target)
        {
            await target.reply("您提供的邮箱已经与您目前的平台绑定了。");
            return AccountOperation.None;
        }

        private static async Task SendVerificationMail(AccountOperation op, Target target, string mailAddr, string verifyCode, string platform, string uid)
        {
            string read_html = System.IO.File.ReadAllText("./mail_desu_life_mailaddr_verify_template.txt");

            string mail_subject = "[来自desu.life自动发送的邮件]请验证您的邮箱";
            string send_fail_msg = "发送邮件失败，请联系管理员。";


            switch (op)
            {
                case AccountOperation.Register:
                    read_html = read_html.Replace("{{{{mailaddress}}}}", mailAddr).Replace("{{{{veritylink}}}}",
                        $"https://desu.life/verify-email?mailAddr={mailAddr}&verifyCode={verifyCode}&platform={platform}&uid={uid}&op=1");
                    try
                    {
                        SendMail(mailAddr, mail_subject, read_html, true);
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error("Error: " + ex.ToString());
                        await target.reply($"{send_fail_msg} [{ex}]");
                        break;
                    }
                    await target.reply("注册验证邮件发送成功，请继续从邮箱内操作，注意检查垃圾箱。");
                    break;
                case AccountOperation.Verify:
                    read_html = read_html.Replace("{{{{mailaddress}}}}", mailAddr).Replace("{{{{veritylink}}}}",
                        $"https://desu.life/verify-email?mailAddr={mailAddr}&verifyCode={verifyCode}&platform={platform}&uid={uid}&op=2");
                    try
                    {
                        SendMail(mailAddr, mail_subject, read_html, true);
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error("Error: " + ex.ToString());
                        await target.reply($"{send_fail_msg} [{ex}]");
                        break;
                    }
                    await target.reply("绑定验证邮件发送成功，请继续从邮箱内操作，注意检查垃圾箱。");
                    break;
                case AccountOperation.AppendEmail:
                    read_html = read_html.Replace("{{{{mailaddress}}}}", mailAddr).Replace("{{{{veritylink}}}}",
                        $"https://desu.life/verify-email?mailAddr={mailAddr}&verifyCode={verifyCode}&platform={platform}&uid={uid}&op=3");
                    try
                    {
                        SendMail(mailAddr, mail_subject, read_html, true);
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error("Error: " + ex.ToString());
                        await target.reply($"{send_fail_msg} [{ex}]");
                        break;
                    }
                    await target.reply("电子邮箱追加验证邮件发送成功，请继续从邮箱内操作，注意检查垃圾箱。");
                    break;
                default:
                    break;
            }
        }

    }
}