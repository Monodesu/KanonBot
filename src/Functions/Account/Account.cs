using KanonBot.Drivers;
using KanonBot.Message;
using Platform = KanonBot.Drivers.Platform;

namespace KanonBot.Account
{
    public static class Account
    {
        public static string mailaddr_verify_template_string = "";

        public static async Task RegAccount(Target target, string cmd)
        {
            var mailAddr = cmd.Trim();
            var verifyCode = RandomStr(22, true);

            if (!IsMailAddr(mailAddr))
            {
                await target.reply("请输入有效的电子邮件地址。");
                return;
            }

            var user = await GetOrCreateUser(mailAddr, target);

            // 不处理失败操作
            if (user.Item1 == null || user.Item2 != Enums.Operation.Failed)
                return;
            var platform = GetPlatformString(target.platform);

            var emailContent = PrepareEmailContent(
                mailAddr,
                verifyCode,
                platform,
                user.Item3!,
                (int)user.Item2
            );

            SendMail(mailAddr, "[来自desu.life自动发送的邮件]请验证您的邮箱", emailContent, true);
            await target.reply("验证邮件发送成功，请检查您的邮箱。");
            await Database.Client.SetVerifyMail(mailAddr, verifyCode);
        }

        private static string PrepareEmailContent(
            string mailAddr,
            string verifyCode,
            string platform,
            string uid,
            int operation
        )
        {
            if (mailaddr_verify_template_string == "")
            {
                var templatePath = "./mail_desu_life_mailaddr_verify_template.txt";
                mailaddr_verify_template_string = System.IO.File.ReadAllText(templatePath);
            }

            var verificationLink =
                $"https://desu.life/verify-email?mailAddr={mailAddr}&verifyCode={verifyCode}&platform={platform}&uid={uid}&op={operation}";
            var temp_string = mailaddr_verify_template_string;
            return temp_string
                .Replace("{{mailaddress}}", mailAddr)
                .Replace("{{veritylink}}", verificationLink);
        }

        private static string GetPlatformString(Platform platform)
        {
            return platform switch
            {
                Platform.Guild => "qguild",
                Platform.KOOK => "kook",
                Platform.OneBot => "qq",
                Platform.Discord => "discord",
                _ => throw new NotSupportedException("不支持的平台。")
            };
        }

        private static async Task<(
            Database.Models.User?,
            Enums.Operation,
            string?
        )> GetOrCreateUser(string mailAddr, Target target)
        {
            // 检测该邮箱是否已注册为基础账户
            bool isRegistered = await Database.Client.IsRegd(mailAddr);
            bool isAppend = false;
            string uid = "-1";
            Database.Models.User dbuser = new();
            dbuser = (await Database.Client.GetUser(mailAddr))!;

            // 根据平台类型获取用户ID，并检查用户是否已绑定到邮箱
            switch (target.platform)
            {
                case Platform.Guild:
                    var guildInfo = target.raw as Guild.Models.MessageData;
                    uid = guildInfo?.Author.ID ?? uid;
                    isAppend = await CheckUserBinding(
                        dbuser,
                        guildInfo?.Author.ID,
                        dbuser.qq_guild_uid!,
                        target
                    );
                    break;
                case Platform.OneBot:
                    var oneBotInfo = target.raw as OneBot.Models.CQMessageEventBase;
                    uid = oneBotInfo?.UserId.ToString() ?? uid;
                    isAppend = await CheckUserBinding(
                        dbuser,
                        oneBotInfo?.UserId.ToString(),
                        dbuser.qq_id.ToString(),
                        target
                    );
                    break;
                case Platform.KOOK:
                    var kookInfo = target.raw as Kook.WebSocket.SocketMessage;
                    uid = kookInfo?.Author.Id.ToString() ?? uid;
                    isAppend = await CheckUserBinding(
                        dbuser,
                        kookInfo?.Author.Id.ToString(),
                        dbuser.kook_uid!,
                        target
                    );
                    break;
                case Platform.Discord:
                    var discordInfo = target.raw as Discord.WebSocket.SocketMessage;
                    uid = discordInfo?.Author.Id.ToString() ?? uid;
                    isAppend = await CheckUserBinding(
                        dbuser,
                        discordInfo?.Author.Id.ToString(),
                        dbuser.discord_uid!,
                        target
                    );
                    break;
            }

            // 基本账户存在，且已绑定，发送通知
            if (isAppend)
            {
                await target.reply(
                    new Chain().msg($"您当前平台的账户已绑定到邮箱 {HideMailAddr(dbuser.email!)}。")
                );
                return (dbuser, Enums.Operation.Failed, null);
                ;
            }

            // 基本账户存在，但未绑定，执行绑定操作（平台方）
            if (isRegistered && !isAppend)
            {
                return (dbuser, Enums.Operation.BindPlatform, uid);
            }

            // 基本账户存在，但未绑定，执行追加操作（邮箱）
            if (!isRegistered && isAppend)
            {
                return (dbuser, Enums.Operation.AppendMail, uid);
            }

            // 基本账户不存在，创建新用户，并默认将当前平台的账户绑定至基本账户上
            if (!isRegistered)
            {
                return (dbuser, Enums.Operation.CreateAccount, uid);
            }

            // 失败
            return (null, Enums.Operation.Failed, null);
        }

        private static async Task<bool> CheckUserBinding(
            Database.Models.User dbuser,
            string? currentUid,
            string dbUid,
            Target target
        )
        {
            if (currentUid == null)
                return false;

            if (dbUid == currentUid)
            {
                return true;
            }

            var user = await Database.Client.GetUsersByUID(currentUid, target.platform);
            if (user != null && user.email != null)
            {
                return true;
            }

            return false;
        }
    }
}
