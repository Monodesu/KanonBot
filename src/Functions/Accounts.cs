using Flurl.Util;
using KanonBot.API;
using KanonBot.OSU;
using KanonBot.Drivers;
using KanonBot.Message;
using LanguageExt.SomeHelp;
using LanguageExt.UnsafeValueAccess;
using System.Net;

namespace KanonBot
{
    public static class Accounts
    {
        public struct AccInfo
        {
            public required Platform platform;
            public required string uid;
        }

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
                await target.reply("��������Ч�ĵ����ʼ���ַ��");
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
                Platform.OneBot => await HandleOneBot(target, dbuser),
                Platform.KOOK => await HandleKook(target, dbuser),
                Platform.Discord => await HandleDiscord(target, dbuser),
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

        private static async Task<AccountOperation> HandleOneBot(Target target, Database.Models.User? dbuser)
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

        private static async Task<AccountOperation> HandleKook(Target target, Database.Models.User? dbuser)
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

        private static async Task<AccountOperation> HandleDiscord(Target target, Database.Models.User? dbuser)
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
                            .msg($"��Ŀǰ��ƽ̨�˻��Ѿ�������Ϊ" +
                            $"{HideMailAddr(email ?? "undefined@undefined.undefined")}" +
                            $"���û����ˡ�"));
            return AccountOperation.None;
        }

        private static async Task<AccountOperation> ReplyUserBoundMsg(Target target)
        {
            await target.reply("���ṩ�������Ѿ�����Ŀǰ��ƽ̨���ˡ�");
            return AccountOperation.None;
        }

        private static async Task SendVerificationMail(AccountOperation op, Target target, string mailAddr, string verifyCode, string platform, string uid)
        {
            string read_html = System.IO.File.ReadAllText("./mail_desu_life_mailaddr_verify_template.txt");

            string mail_subject = "[����desu.life�Զ����͵��ʼ�]����֤��������";
            string send_fail_msg = "�����ʼ�ʧ�ܣ�����ϵ����Ա��";


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
                    await target.reply("ע����֤�ʼ����ͳɹ���������������ڲ�����ע���������䡣");
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
                    await target.reply("����֤�ʼ����ͳɹ���������������ڲ�����ע���������䡣");
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
                    await target.reply("��������׷����֤�ʼ����ͳɹ���������������ڲ�����ע���������䡣");
                    break;
                default:
                    break;
            }
        }






























        async public static Task BindService(Target target, string cmd)
        {
            cmd = cmd.Trim();
            string childCmd_1 = "", childCmd_2 = "";
            try
            {
                var tmp = cmd.SplitOnFirstOccurence(" ");
                childCmd_1 = tmp[0];
                childCmd_2 = tmp[1];
            }
            catch { }

            var AccInfo = Accounts.GetAccInfo(target);
            var DBUser = await Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
            //����dbuser�ɿգ�����һ��Ҫ���


            if (childCmd_1 == "osu")
            {
                // �ȼ���ѯ���û��Ƿ���Ч
                API.OSU.Models.User? online_osu_userinfo;
                online_osu_userinfo = await API.OSU.V2.GetUser(childCmd_2);
                if (online_osu_userinfo == null) { await target.reply($"û���ҵ�osu�û���Ϊ {childCmd_2} ��osu�û�����ʧ�ܡ�"); return; }

                // ���Ҫ�󶨵�osu�Ƿ�û�б�Kanon�û��󶨹�
                var db_osu_userinfo = await Database.Client.GetOsuUser(online_osu_userinfo.Id);
                if (db_osu_userinfo != null)
                {
                    if (DBUser != null && DBUser.uid == db_osu_userinfo.uid)
                    {
                        await target.reply($"���Ѱ󶨸��˻���"); return;
                    }
                    await target.reply($"��osu�˻��ѱ��û�IDΪ {db_osu_userinfo.uid} ���û����ˣ������������˻�������ϵ����Ա�����˻���Ϣ��"); return;
                }

                // ��ѯ��ǰkanon�˻��Ƿ���Ч
                if (DBUser == null) { await target.reply("����û�а�Kanon�˻�����ʹ��!reg �������������а󶨻�ע�ᡣ"); return; }

                // ����û��Ƿ��Ѱ�osu�˻�
                var osuuserinfo = await Database.Client.GetOsuUserByUID(DBUser.uid);
                if (osuuserinfo != null) { await target.reply($"���Ѿ���osu uidΪ {osuuserinfo.osu_uid} ���û��󶨹��ˡ�"); return; }

                // ͨ��osu username����osu�û�id
                try
                {
                    // û�����˰󶨣���ʼ������
                    if (await Database.Client.InsertOsuUser(DBUser.uid, online_osu_userinfo.Id, online_osu_userinfo.CoverUrl.ToString() == "" ? 0 : 2))   //?����url�����Ϊ����  �Ҳ�����
                    {
                        await target.reply($"�󶨳ɹ����ѽ�osu�û� {online_osu_userinfo.Id} ����Kanon�˻� {DBUser.uid} ��");
                        await GeneralUpdate.UpdateUser(online_osu_userinfo.Id, true); //�����û�ÿ�����ݼ�¼
                    }
                    else { await target.reply($"��ʧ�ܣ����Ժ����ԡ�"); }
                }
                catch { await target.reply($"�ڰ��û�ʱ��������ϵè�账��.png"); return; }
            }
            else
            {
                await target.reply("�밴�����¸�ʽ���а󶨡�\n!bind osu ����osu�û��� "); return;
            }
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

                var dbuser_ = await GetAccountByOsuUid(uid);
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

            var dbuser = await GetAccount(v, platform);
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

        public static async Task<Database.Models.User?> GetAccount(string uid, Platform platform)
        {
            return await Database.Client.GetUsersByUID(uid, platform);
        }

        public static async Task<Database.Models.User?> GetAccountByOsuUid(long osu_uid)
        {
            return await Database.Client.GetUserByOsuUID(osu_uid);
        }

        public static async Task<Database.Models.UserOSU?> CheckOsuAccount(long uid)
        {
            return await Database.Client.GetOsuUserByUID(uid);
        }

        public static AccInfo GetAccInfo(Target target)
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