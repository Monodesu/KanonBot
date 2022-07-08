#pragma warning disable CS8602 // 解引用可能出现空引用。
using System;
using KanonBot.Drivers;
using KanonBot.Message;
using KanonBot.API;

namespace KanonBot.functions
{
    public static class Accounts
    {
        public struct AccInfo
        {
            public Platform platform;
            public string uid;
        }
        public static void RegAccount(Target target, string cmd)
        {
            //var is_private_msg = target.raw switch
            //{
            //    OneBot.Models.GroupMessage => false,
            //    OneBot.Models.PrivateMessage => true,
            //    _ => true,//无法调试qq频道私聊 暂时默认私聊
            //};
            //if (!is_private_msg)
            //{
            //    target.reply("请私聊bot以使用本功能。");
            //    return;
            //}

            var mailAddr = cmd.Trim(); // reg 邮箱地址
            var verifyCode = Utils.RandomStr(22, true); //生成验证码

            if (!Utils.IsMailAddr(mailAddr))
            {
                target.reply("请输入有效的电子邮件地址。");
                return;
            }
            string uid = "-1";
            bool is_regd = false;
            is_regd = Database.Client.IsRegd(mailAddr);
            Database.Model.Users dbuser = new();

            if (is_regd) dbuser = Database.Client.GetUsers(mailAddr);
            switch (target.platform) //获取用户ID及平台信息 平台： qq qguild khl discord 四个
            {
                case Platform.Guild:
                    if (target.raw is Guild.Models.MessageData g)
                    {
                        uid = g.Author.ID;
                        if (is_regd)
                            if (dbuser.qq_guild_uid == g.Author.ID) { target.reply("您提供的邮箱已经与您目前的平台绑定了。"); return; }
                        var g1 = Database.Client.GetUsersByUID(g.Author.ID, Platform.Guild);
                        if (g1 != null)
                        {
                            target.reply(new Chain()
                                .msg($"您目前的平台账户已经和邮箱为" +
                                $"{Utils.HideMailAddr(g1.email ?? "undefined@undefined.undefined")}" +
                                $"的用户绑定了。"));
                            return;
                        }
                    }
                    break;
                case Platform.OneBot:
                    if (target.raw is OneBot.Models.Sender o)
                    {
                        uid = o.UserId.ToString();
                        if (is_regd)
                            if (dbuser.qq_id == o.UserId) { target.reply("您提供的邮箱已经与您目前的平台绑定了。"); return; }
                        var o1 = Database.Client.GetUsersByUID(o.UserId.ToString(), Platform.OneBot);
                        if (o1 != null)
                        {
                            target.reply(new Chain()
                                .msg($"您目前的平台账户已经和邮箱为" +
                                $"{Utils.HideMailAddr(o1.email ?? "undefined@undefined.undefined")}" +
                                $"的用户绑定了。"));
                            return;
                        }
                    }
                    break;
                default: break;
            }
            if (is_regd) //检查此邮箱是否已存在于数据库中
            {
                // 如果存在，执行绑定

                Mail.MailStruct ms = new()
                {
                    MailTo = new string[] { mailAddr },
                    Subject = "desu.life - 请验证您的邮箱",
                    Body = $"https://desu.life/verify-email?mailAddr={mailAddr}&verifyCode={verifyCode}&platform={target.platform!}&uid={uid}&op=2",
                    IsBodyHtml = false
                };
                try
                {
                    Mail.Send(ms);
                    target.reply("绑定验证邮件发送成功，请继续从邮箱内操作，注意检查垃圾箱。");
                    Database.Client.SetVerifyMail(mailAddr, verifyCode); //设置临时验证码
                }
                catch
                {
                    target.reply("发送验证邮件失败，请联系管理员。");
                }
            }
            else
            {
                //如果不存在，新建
                Mail.MailStruct ms = new()
                {
                    MailTo = new string[] { mailAddr },
                    Subject = "desu.life - 请验证您的邮箱",
                    Body = $"https://desu.life/verify-email?mailAddr={mailAddr}&verifyCode={verifyCode}&platform={target.platform!}&uid={uid}&op=1",
                    IsBodyHtml = false
                };
                try
                {
                    Mail.Send(ms);
                    target.reply("注册验证邮件发送成功，请继续从邮箱内操作，注意检查垃圾箱。");
                    Database.Client.SetVerifyMail(mailAddr, verifyCode); //设置临时验证码
                }
                catch
                {
                    target.reply("发送验证邮件失败，请联系管理员。");
                }
            }
        }

        async public static void BindService(Target target, string cmd)
        {
            string uid = "-1";
            switch (target.platform)
            {
                case Platform.Guild:
                    if (target.raw is Guild.Models.MessageData g)
                    {
                        uid = g.Author.ID;
                        if (!Database.Client.IsRegd(uid, Platform.Guild))
                        {
                            target.reply("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。"); return;
                        }
                    }
                    break;
                case Platform.OneBot:
                    if (target.raw is OneBot.Models.Sender o)
                    {
                        uid = o.UserId.ToString();
                        if (!Database.Client.IsRegd(uid, Platform.OneBot))
                        {
                            target.reply("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。"); return;
                        }
                    }
                    break;
                default: return;
            }

            cmd = cmd.Trim();
            string childCmd_1 = cmd[..cmd.IndexOf(" ")];
            string childCmd_2 = cmd[(cmd.IndexOf(" ") + 1)..];

            if (childCmd_1 == "osu")
            {
                OSU.UserInfo online_osu_userinfo = new();
                var globaluserinfo = Database.Client.GetUsersByUID(uid, target.platform);

                // 检查用户是否已绑定osu账户
                var osuuserinfo = Database.Client.GetOSUUsersByUID(globaluserinfo.uid);
                if (osuuserinfo != null) { target.reply(new Chain().msg($"您已经与osu uid为 {osuuserinfo.osu_uid} 的用户绑定过了。")); return; }

                // 通过osu username搜索osu用户id
                try { online_osu_userinfo = await OSU.GetUserLegacy(childCmd_2); }
                catch { target.reply(new Chain().msg($"没有找到osu用户名为 {childCmd_2} 的osu用户，绑定失败。")); return; }

                // 检查要绑定的osu是否没有被Kanon用户绑定过
                var db_osu_userinfo = Database.Client.GetOSUUsers(online_osu_userinfo.userId);
                if (db_osu_userinfo == null)
                {
                    // 没被他人绑定，开始绑定流程
                    if (Database.Client.InsertOsuUser(globaluserinfo.uid, online_osu_userinfo.userId, online_osu_userinfo.coverUrl == "" ? 0 : 2))
                    { target.reply(new Chain().msg($"绑定成功，已将osu用户 {online_osu_userinfo.userId} 绑定至Kanon账户 {globaluserinfo.uid} 。")); }
                    else { target.reply(new Chain().msg($"绑定失败，请稍后再试。")); }
                }
                else { target.reply(new Chain().msg($"此osu账户已被用户ID为 {db_osu_userinfo.uid} 的用户绑定了，如果您认为他人恶意绑定了您的账户，请联系管理员。")); return; }
            }
            else
            {
                target.reply("请按照以下格式进行绑定。\r\n !set osu 您的osu用户名"); return;
            }
        }

        public static Database.Model.Users? GetAccount(string uid, Platform platform)
        {
            var globaluserinfo = Database.Client.GetUsersByUID(uid, platform);
            return globaluserinfo;
        }
        public static Database.Model.Users? GetAccount(long osu_uid)
        {
            var globaluserinfo = Database.Client.GetUsersByOsuUID(osu_uid);
            return globaluserinfo;
        }
        public static Database.Model.Users_osu? CheckOsuAccount(long uid)
        {
            var db_osu_userinfo = Database.Client.GetOSUUsersByUID(uid);
            if (db_osu_userinfo == null) return null;
            return db_osu_userinfo;
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
                    if (target.raw is OneBot.Models.Sender o)
                    {
                        return new AccInfo() { platform = Platform.OneBot, uid = o.UserId.ToString() };
                    }
                    break;
            }
            return new() { platform = Platform.Unknown, uid = "" };
        }
    }
}
