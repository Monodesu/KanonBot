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

        public static async Task RegAccount(Target target, string cmd)
        {
            var mailAddr = cmd.Trim(); // reg �����ַ
            var verifyCode = Utils.RandomStr(22, true); //������֤��

            if (!Utils.IsMailAddr(mailAddr))
            {
                await target.reply("��������Ч�ĵ����ʼ���ַ��");
                return;
            }
            string uid = "-1";
            bool is_regd = await Database.Client.IsRegd(mailAddr);
            bool is_append = false;
            Database.Models.User dbuser = new();

            if (is_regd) dbuser = (await Database.Client.GetUser(mailAddr))!;
            switch (target.platform) //��ȡ�û�ID��ƽ̨��Ϣ ƽ̨�� qq qguild khl discord �ĸ�
            {
                case Platform.Guild:
                    if (target.raw is Guild.Models.MessageData g)
                    {
                        uid = g.Author.ID;
                        if (is_regd)
                            if (dbuser.qq_guild_uid == g.Author.ID) { await target.reply("���ṩ�������Ѿ�����Ŀǰ��ƽ̨���ˡ�"); return; }
                        var g1 = await Database.Client.GetUsersByUID(uid, Platform.Guild);
                        if (g1 != null)
                        {
                            if (g1.email != null)
                            {
                                is_append = true;
                            }
                            else
                            {
                                await target.reply(new Chain()
                                    .msg($"��Ŀǰ��ƽ̨�˻��Ѿ�������Ϊ" +
                                    $"{Utils.HideMailAddr(g1.email ?? "undefined@undefined.undefined")}" +
                                    $"���û����ˡ�"));
                                return;
                            }
                        }
                    }
                    break;
                case Platform.OneBot:
                    if (target.raw is OneBot.Models.CQMessageEventBase o)
                    {
                        uid = o.UserId.ToString();
                        if (is_regd)
                            if (dbuser.qq_id == o.UserId) { await target.reply("���ṩ�������Ѿ�����Ŀǰ��ƽ̨���ˡ�"); return; }
                        var o1 = await Database.Client.GetUsersByUID(uid, Platform.OneBot);
                        if (o1 != null)
                        {
                            if (o1.email != null)
                            {
                                is_append = true;
                            }
                            else
                            {
                                await target.reply(new Chain()
                                    .msg($"��Ŀǰ��ƽ̨�˻��Ѿ�������Ϊ" +
                                    $"{Utils.HideMailAddr(o1.email ?? "undefined@undefined.undefined")}" +
                                    $"���û����ˡ�"));
                                return;
                            }
                        }
                    }
                    break;
                case Platform.KOOK:
                    if (target.raw is Kook.WebSocket.SocketMessage k)
                    {
                        uid = k.Author.Id.ToString();
                        if (is_regd)
                            if (dbuser.kook_uid == uid) { await target.reply("���ṩ�������Ѿ�����Ŀǰ��ƽ̨���ˡ�"); return; }
                        var k1 = await Database.Client.GetUsersByUID(uid, Platform.KOOK);
                        if (k1 != null)
                        {
                            if (k1.email != null)
                            {
                                is_append = true;
                            }
                            else
                            {
                                await target.reply(new Chain()
                                    .msg($"��Ŀǰ��ƽ̨�˻��Ѿ�������Ϊ" +
                                    $"{Utils.HideMailAddr(k1.email ?? "undefined@undefined.undefined")}" +
                                    $"���û����ˡ�"));
                                return;
                            }
                        }
                    }
                    break;
                case Platform.Discord:
                    if (target.raw is Discord.WebSocket.SocketMessage d)
                    {
                        uid = d.Author.Id.ToString();
                        if (is_regd)
                            if (dbuser.discord_uid == uid) { await target.reply("���ṩ�������Ѿ�����Ŀǰ��ƽ̨���ˡ�"); return; }
                        var k1 = await Database.Client.GetUsersByUID(uid, Platform.Discord);
                        if (k1 != null)
                        {
                            if (k1.email != null)
                            {
                                is_append = true;
                            }
                            else
                            {
                                await target.reply(new Chain()
                                    .msg($"��Ŀǰ��ƽ̨�˻��Ѿ�������Ϊ" +
                                    $"{Utils.HideMailAddr(k1.email ?? "undefined@undefined.undefined")}" +
                                    $"���û����ˡ�"));
                                return;
                            }
                        }
                    }
                    break;
                default: break;
            }
            var platform = target.platform! switch
            {
                Platform.Guild => "qguild",
                Platform.KOOK => "kook",
                Platform.OneBot => "qq",
                Platform.Discord => "discord",
                _ => throw new NotSupportedException()
            };



            string read_html = System.IO.File.ReadAllText("./mail_desu_life_mailaddr_verify_template.txt");

            if (is_regd) //���������Ƿ��Ѵ��������ݿ���
            {
                // ������ڣ�ִ�а�
                read_html = read_html.Replace("{{{{mailaddress}}}}", mailAddr).Replace("{{{{veritylink}}}}", $"https://desu.life/verify-email?mailAddr={mailAddr}&verifyCode={verifyCode}&platform={platform}&uid={uid}&op=2");
                try
                {
                    SendMail(mailAddr, "[����desu.life�Զ����͵��ʼ�]����֤��������", read_html, true);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error("Error: " + ex.ToString());
                    await target.reply("�����ʼ�ʧ�ܣ�����ϵ����Ա��");
                }
                await target.reply("����֤�ʼ����ͳɹ���������������ڲ�����ע���������䡣");
            }
            else if (!is_append)
            {
                //��������ڣ��½�
                read_html = read_html.Replace("{{{{mailaddress}}}}", mailAddr).Replace("{{{{veritylink}}}}", $"https://desu.life/verify-email?mailAddr={mailAddr}&verifyCode={verifyCode}&platform={platform}&uid={uid}&op=1");
                try
                {
                    SendMail(mailAddr, "[����desu.life�Զ����͵��ʼ�]����֤��������", read_html, true);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error("Error: " + ex.ToString());
                    await target.reply("�����ʼ�ʧ�ܣ�����ϵ����Ա��");
                }     
                await target.reply("ע����֤�ʼ����ͳɹ���������������ڲ�����ע���������䡣");
            }
            else
            {
                //׷��������Ϣ
                read_html = read_html.Replace("{{{{mailaddress}}}}", mailAddr).Replace("{{{{veritylink}}}}", $"https://desu.life/verify-email?mailAddr={mailAddr}&verifyCode={verifyCode}&platform={platform}&uid={uid}&op=3");
                try
                {
                    SendMail(mailAddr, "[����desu.life�Զ����͵��ʼ�]����֤��������", read_html, true);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error("Error: " + ex.ToString());
                    await target.reply("�����ʼ�ʧ�ܣ�����ϵ����Ա��");
                }
                await target.reply("��������׷����֤�ʼ����ͳɹ���������������ڲ�����ע���������䡣");
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