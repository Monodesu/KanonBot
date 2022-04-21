#pragma warning disable IDE0044 // 添加只读修饰符
#pragma warning disable CS8602 // 解引用可能出现空引用。
#pragma warning disable CS8604 // 解引用可能出现空引用。

using System.Net.Mail;

namespace KanonBot;
public static class Mail
{
    private static Config.Config config = Config.Config.inner!;
    public struct MailStruct
    {
        public string[] MailTo; //收件人，可添加多个
        public string[] MailCC; //抄送人，不建议添加
        public string Subject; //标题
        public string Body; //正文
        public bool IsBodyHtml;
        public MailStruct()
        {
            MailTo = Array.Empty<string>();
            MailCC = Array.Empty<string>();
            Subject = string.Empty;
            Body = string.Empty;
            IsBodyHtml = false;
        }
    }
    public static void Send(MailStruct ms)
    {
        Console.WriteLine(config.ToString());
        MailMessage message = new();
        if (ms.MailTo.Length == 0) return; foreach (string s in ms.MailTo) { message.To.Add(s); } //设置收件人
        if (ms.MailCC.Length > 0) foreach (string s in ms.MailCC) { message.CC.Add(s); } //设置发件人
        message.From = new MailAddress(config.mail.username); //设置发件人
        message.Subject = ms.Subject;
        message.Body = ms.Body;
        message.IsBodyHtml = ms.IsBodyHtml;
        SmtpClient client = new SmtpClient(config.mail.smtp_host, config.mail.smtp_port); //设置邮件服务器
        client.Credentials = new System.Net.NetworkCredential(config.mail.username, config.mail.password); //设置邮箱用户名与密码
        client.EnableSsl = true; //启用SSL
        client.Send(message); //发送
    }
}
