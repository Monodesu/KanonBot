using Discord;
using Flurl.Util;
using KanonBot.API;
using KanonBot.Drivers;
using static KanonBot.Functions.Accounts;

namespace KanonBot.Functions.OSUBot
{
    public class ChatBot
    {
        async public static Task Execute(Target target, string cmd)
        {
            if (target.platform != Platform.OneBot) return;

            // 验证账户
            Database.Model.User? DBUser = null;
            var AccInfo = Accounts.GetAccInfo(target);
            DBUser = await Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
            if (DBUser == null)
            {
                await target.reply("你还没有绑定desu.life账户，请使用!reg 您的邮箱来进行绑定或注册喵！");
                return;
            }

            bool chatbot_premission = false, custom_chatbot_premission = false;

            List<string> permissions = new();
            if (DBUser!.permissions!.IndexOf(";") < 1) //一般不会出错，默认就是user
            {
                permissions.Add(DBUser.permissions);
            }
            else
            {
                var t1 = DBUser.permissions.Split(";");
                foreach (var x in t1)
                {
                    permissions.Add(x);
                }
            }
            //判断权限
            foreach (var x in permissions)
            {
                switch (x)
                {
                    case "chatbot":
                        chatbot_premission = true;
                        custom_chatbot_premission = true;
                        break;
                    case "mod":
                        chatbot_premission = true;
                        custom_chatbot_premission = true;
                        break;
                    case "admin":
                        chatbot_premission = true;
                        custom_chatbot_premission = true;
                        break;
                    default:
                        break;
                }
            }

            switch (target.raw)
            {
                case OneBot.Models.GroupMessage g:
                    if (g.GroupId == 217451241) //猫群，非猫群需要判断是否拥有chatbot权限
                        chatbot_premission = true;
                    break;
                default:
                    break;
            }

            //判断子命令
            if (custom_chatbot_premission)
            {
                string rootCmd, childCmd = "";
                try
                {
                    var tmp = cmd.SplitOnFirstOccurence(" ");
                    rootCmd = tmp[0].Trim();
                    childCmd = tmp[1].Trim();
                }
                catch { rootCmd = cmd; }

                switch (rootCmd.ToLower())
                {
                    case "set":
                        try
                        {
                            string botdefine = "", openaikey = "", organization = "";
                            if (childCmd.Contains(';'))
                            {
                                try
                                {
                                    var t = childCmd.Split(";");
                                    foreach (var item in t)
                                    {
                                        if (item.StartsWith("define"))
                                            botdefine = item[(item.IndexOf("=") + 1)..];
                                        if (item.StartsWith("openaikey"))
                                            openaikey = item[(item.IndexOf("=") + 1)..];
                                        if (item.StartsWith("organization"))
                                            organization = item[(item.IndexOf("=") + 1)..];
                                    }
                                }
                                catch
                                {
                                    await target.reply("失败了喵...");
                                }
                            }
                            else
                            {
                                if (childCmd.StartsWith("define"))
                                    botdefine = childCmd[(childCmd.IndexOf("=") + 1)..];
                                if (childCmd.StartsWith("openaikey"))
                                    openaikey = childCmd[(childCmd.IndexOf("=") + 1)..];
                                if (childCmd.StartsWith("organization"))
                                    organization = childCmd[(childCmd.IndexOf("=") + 1)..];
                            }

                            if (botdefine == "")
                            {
                                await target.reply("请使用以下格式上传喵，但是请注意，如果需要上传openai密钥的话，请不要在群内公开你的openai密钥哦\n" +
                                "!cat set define=####;openaikey=####\n" +
                                "(*后两项可省略，如果想删除某些参数的话，使用default就可以啦！比如：define=default;openaikey=default)");
                                return;
                            }

                            if (await Database.Client.UpdateChatBotInfo(DBUser.uid, botdefine, openaikey, organization))
                                await target.reply("成功了喵！");
                            else
                                await target.reply("失败了喵...");
                        }
                        catch
                        {
                            await target.reply("请使用以下格式上传喵，但是请注意，如果需要上传openai密钥的话，请不要在群内公开你的openai密钥哦\n" +
                                //"!cat set define=####;openaikey=####;organization=####\n" +
                                "!cat set define=####;openaikey=####\n" +
                                "(*后两项可省略，如果想删除某些参数的话，使用default就可以啦！比如：define=default;openaikey=default)");
                        }
                        return;
                    default:
                        break;
                }
            }


            try
            {
                if (chatbot_premission)
                    await target.reply(await OpenAI.Chat(cmd, target.sender!, DBUser.uid));
                else
                    await target.reply("你没有使用chatbot的权限呢T^T");
            }
            catch
            {
                await target.reply("目前无法访问API T^T");
            }
        }

    }
}
