using System.Net;
using System.Net.Mail;

namespace KanonBot
{
    public static class Mail
    {
        static Mail()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        private static readonly Config.Base config = Config.inner!;

        public class MailContent
        {
            public List<string> Recipients { get; } // 收件人列表
            public List<string> CC { get; } // 抄送列表
            public string Subject { get; }
            public string Body { get; }
            public bool IsBodyHtml { get; }

            public MailContent(
                List<string> recipients,
                string subject,
                string body,
                bool isBodyHtml,
                List<string>? cc = null
            )
            {
                if (recipients == null || recipients.Count == 0)
                    throw new ArgumentException("Recipients cannot be empty.", nameof(recipients));

                Recipients = recipients;
                Subject = subject ?? throw new ArgumentNullException(nameof(subject));
                Body = body ?? throw new ArgumentNullException(nameof(body));
                IsBodyHtml = isBodyHtml;
                CC = cc ?? [ ];
            }
        }

        public static void Send(MailContent content)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            using var message = new MailMessage
            {
                From = new MailAddress(config.mail!.userName!), // 设置发件人
                Subject = content.Subject,
                Body = content.Body,
                IsBodyHtml = content.IsBodyHtml
            };

            content.Recipients.ForEach(recipient => message.To.Add(recipient));
            content.CC.ForEach(cc => message.CC.Add(cc));

            using var client = new SmtpClient(config.mail.smtpHost, config.mail.smtpPort)
            {
                Credentials = new NetworkCredential(config.mail.userName, config.mail.passWord),
                EnableSsl = true
            };

            client.Send(message);
        }
    }
}
