using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanonBot.src.functions
{
    public static class Accounts
    {
        public static void regAccount()
        {
            var is_private_msg = true;
            var mailAddr = "mono@desu.life"; // reg 邮箱地址
            if (!is_private_msg)
            {
                //sendmessage 注册指令只能私聊使用
                return;
            }
            var verifyCode = Utils.RandomStr(22);
            //获取平台信息

            var platform = "qq"; //qq qguild khl discord 四个
            var uid = 12345678;

            //检查此邮箱是否已存在于数据库中
            var is_reg = false;
            if (is_reg)
            {
                //如果存在，执行绑定
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
                    //sendmessage 发送邮件成功，请继续从邮箱内操作，注意检查垃圾箱
                    //设置临时验证代码
                    Database.Client.SetVerifyMail(mailAddr, verifyCode);
                }
                catch
                {
                    //sendmessage 发送邮件失败
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
                    //sendmessage 发送邮件成功，请继续从邮箱内操作，注意检查垃圾箱
                    //设置临时验证代码
                    Database.Client.SetVerifyMail(mailAddr, verifyCode);
                }
                catch
                {
                    //sendmessage 发送邮件失败
                }
            }
        }

        public static void bindService()
        {
            //检测此账户有没有绑定到kanon账户上
            //没有提示使用reg命令注册
            //如果有，则执行绑定其他平台操作，如osu，steam。


        }
    }
}
