using KanonBot.API;
using KanonBot.Drivers;
using static KanonBot.functions.Accounts;

namespace KanonBot.functions.osubot
{
    public class ChatBot
    {
        async public static Task Execute(Target target, string cmd, bool isadmin)
        {
            if (target.platform != Platform.OneBot) return;

            bool chatbot_premission = false;

            if (!isadmin)
            {
                switch (target.raw)
                {
                    case OneBot.Models.GroupMessage g:
                        if (g.GroupId == 217451241) //猫群，非猫群需要判断是否拥有chatbot权限
                            chatbot_premission = true;
                        else
                        {
                            var AccInfo = GetAccInfo(target);
                            var userinfo = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
                            if (userinfo == null)
                            {
                                return;//直接忽略
                            }
                            List<string> permissions = new();
                            if (userinfo!.permissions!.IndexOf(";") < 1) //一般不会出错，默认就是user
                            {
                                permissions.Add(userinfo.permissions);
                            }
                            else
                            {
                                var t1 = userinfo.permissions.Split(";");
                                foreach (var x in t1)
                                {
                                    permissions.Add(x);
                                }
                            }
                            foreach (var x in permissions)
                            {
                                switch (x)
                                {
                                    case "chatbot":
                                        chatbot_premission = true;
                                        break;
                                    case "mod":
                                        chatbot_premission = true;
                                        break;
                                    case "admin":
                                        chatbot_premission = true;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            else
                chatbot_premission = true;

            if (chatbot_premission)
                await target.reply(OpenAI.Chat(cmd, target.sender!, isadmin));
            else
                await target.reply("你没有使用chatbot的权限呢T^T");
        }

    }
}
