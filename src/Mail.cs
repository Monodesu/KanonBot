#pragma warning disable IDE0044 // 添加只读修饰符
#pragma warning disable CS8602 // 解引用可能出现空引用。
#pragma warning disable CS8604 // 解引用可能出现空引用。

using System.Net.Mail;
using Serilog;

namespace KanonBot;
public static class Mail
{
    private static Config.Base config = Config.inner!;
    public class MailStruct
    {
        public required Arr<String> MailTo; //收件人，可添加多个
        public Arr<String> MailCC = Arr.empty<String>(); //抄送人，不建议添加
        public required string Subject; //标题
        public required string Body; //正文
        public required bool IsBodyHtml;
    }
    public static void Send(MailStruct ms)
    {
        MailMessage message = new();
        if (ms.MailTo.Length == 0) return; foreach (string s in ms.MailTo) { message.To.Add(s); } //设置收件人
        if (ms.MailCC.Length > 0) foreach (string s in ms.MailCC) { message.CC.Add(s); } //设置发件人
        message.From = new MailAddress(config.mail.userName); //设置发件人
        message.Subject = ms.Subject;
        message.Body = ms.Body;
        message.IsBodyHtml = ms.IsBodyHtml;
        SmtpClient client = new SmtpClient(config.mail.smtpHost, config.mail.smtpPort); //设置邮件服务器
        client.Credentials = new System.Net.NetworkCredential(config.mail.userName, config.mail.passWord); //设置邮箱用户名与密码
        client.EnableSsl = true; //启用SSL
        client.Send(message); //发送
    }
}
