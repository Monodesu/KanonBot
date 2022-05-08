using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KanonBot.Drivers;
using KanonBot.Message;

namespace KanonBot.src.functions
{
    public static class Accounts
    {
        public static void regAccount(Target target, string cmd)
        {
            //var is_private_msg = target.raw switch
            //{
            //    OneBot.Models.GroupMessage => false,
            //    OneBot.Models.PrivateMessage => true,
            //    _ => true,//无法调试qq频道私聊 暂时默认私聊
            //};
            //if (!is_private_msg)
            //{
            //    target.reply(new Chain().msg("请私聊bot以使用本功能。"));
            //    return;
            //}

            var mailAddr = cmd.Trim(); // reg 邮箱地址
            var verifyCode = Utils.RandomStr(22, true); //生成验证码

            if (!Utils.IsMailAddr(mailAddr))
            {
                target.reply(new Chain().msg("请输入有效的电子邮件地址。"));
                return;
            }

            string uid = "-1", platform = "none";
            bool is_regd = false;
            is_regd = Database.Client.IsRegd(mailAddr);
            Database.Model.Users dbuser = new();
            if (is_regd) dbuser = Database.Client.GetUsers(mailAddr);


            switch (target.socket) //获取用户ID及平台信息 平台： qq qguild khl discord 四个
            {
                case Guild:
                    if (target.raw is Guild.Models.MessageData g)
                    {
                        uid = g.Author.ID; platform = "qguild";
                        if (is_regd)
                            if (dbuser.qq_guild_uid == g.Author.ID) { target.reply(new Chain().msg("您提供的邮箱已经与您目前的平台绑定了。")); return; }
                        var g1 = Database.Client.GetUsersByUID(g.Author.ID, "qguild");
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
                case OneBot.Server:
                    if (target.raw is OneBot.Models.Sender o)
                    {
                        uid = o.UserId.ToString(); platform = "qq";
                        if (is_regd)
                            if (dbuser.qq_id == o.UserId) { target.reply(new Chain().msg("您提供的邮箱已经与您目前的平台绑定了。")); return; }
                        var o1 = Database.Client.GetUsersByUID(o.UserId.ToString(), "qq");
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
                    Body = $"https://desu.life/verify-email?mailAddr={mailAddr}&verifyCode={verifyCode}&platform={platform}&uid={uid}&op=2",
                    IsBodyHtml = false
                };
                try
                {
                    Mail.Send(ms);
                    target.reply(new Chain().msg("绑定验证邮件发送成功，请继续从邮箱内操作，注意检查垃圾箱。"));
                    Database.Client.SetVerifyMail(mailAddr, verifyCode); //设置临时验证码
                }
                catch
                {
                    target.reply(new Chain().msg("发送验证邮件失败，请联系管理员。"));
                }
            }
            else
            {
                //如果不存在，新建
                Mail.MailStruct ms = new()
                {
                    MailTo = new string[] { mailAddr },
                    Subject = "desu.life - 请验证您的邮箱",
                    Body = $"https://desu.life/verify-email?mailAddr={mailAddr}&verifyCode={verifyCode}&platform={platform}&uid={uid}&op=1",
                    IsBodyHtml = false
                };
                try
                {
                    Mail.Send(ms);
                    target.reply(new Chain().msg("注册验证邮件发送成功，请继续从邮箱内操作，注意检查垃圾箱。"));
                    Database.Client.SetVerifyMail(mailAddr, verifyCode); //设置临时验证码
                }
                catch
                {
                    target.reply(new Chain().msg("发送验证邮件失败，请联系管理员。"));
                }
            }
        }

        public static void bindService(Target target)
        {
            //检测此账户有没有绑定到kanon账户上
            //没有提示使用reg命令注册
            //如果有，则执行绑定其他平台操作，如osu，steam。


        }
    }
}
