using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Completions;
using OpenAI_API.Models;
using OpenAI_API.Moderation;
using System.Collections.Concurrent;

namespace KanonBot.API
{
    public partial class OpenAI
    {
        private static Config.Base config = Config.inner!;
        private static ConcurrentDictionary<long, List<string>> ChatHistoryDict = new();

        private static readonly int ChatBotMemorySpan = 20;

        public static async Task<string> Chat(string chatmsg, string name, long uid)
        {
            OpenAIAPI? api;
            Conversation? chat;
            var dbinfo = await Database.Client.GetChatBotInfo(uid);

            if (dbinfo != null)
            {
                //使用用户数据库配置
                if (dbinfo.openaikey != "" && dbinfo.openaikey != "default")
                    api = new OpenAIAPI(dbinfo.openaikey);
                else
                    api = new OpenAIAPI(config.openai!.Key);
                chat = api.Chat.CreateConversation();
                if (dbinfo.botdefine != "" || dbinfo.botdefine != "default")
                {
                    if (dbinfo.botdefine!.IndexOf("#") > 0)
                    {
                        chat!.AppendSystemMessage(dbinfo.botdefine!);
                    }
                    else
                    {
                        var t = dbinfo.botdefine!.Split("#");
                        foreach (var item in t) chat!.AppendSystemMessage(item);
                    }
                }
                else
                {
                    if (config.openai!.PreDefine!.IndexOf("#") > 0)
                    {
                        chat!.AppendSystemMessage(config.openai!.PreDefine!);
                    }
                    else
                    {
                        var t = config.openai!.PreDefine!.Split("#");
                        foreach (var item in t) chat!.AppendSystemMessage(item);
                    }
                }
            }
            else
            {
                //使用默认配置
                api = new OpenAIAPI(config.openai!.Key);
                chat = api.Chat.CreateConversation();
                if (config.openai!.PreDefine!.IndexOf("#") > 0)
                {
                    chat!.AppendSystemMessage(config.openai!.PreDefine!);
                }
                else
                {
                    var t = config.openai!.PreDefine!.Split("#");
                    foreach (var item in t) chat!.AppendSystemMessage(item);
                }
            }

            chat.Model = Model.ChatGPTTurbo;
            chat.RequestParameters.Temperature = config.openai!.Temperature;
            chat.RequestParameters.TopP = config.openai.Top_p;
            chat.RequestParameters.MaxTokens = config.openai.MaxTokens;
            chat.RequestParameters.NumChoicesPerMessage = 1;

            if (!ChatHistoryDict.ContainsKey(uid))
            {
                try
                {
                    ChatHistoryDict.TryAdd(uid, new() { chatmsg });
                }
                catch
                {
                    return "猫猫记忆模块出错了喵 T^T";
                }
            }
            else
            {
                if (ChatHistoryDict[uid].Count < ChatBotMemorySpan)
                    ChatHistoryDict[uid].Add(chatmsg);
                else
                {
                    ChatHistoryDict[uid].RemoveAt(0);
                    ChatHistoryDict[uid].Add(chatmsg);
                }
            }
            foreach (var item in ChatHistoryDict[uid])
            {
                chat!.AppendUserInputWithName(name, item);
            }
            chat!.AppendUserInputWithName(name, chatmsg);
            return chat.GetResponseFromChatbotAsync().Result;
        }
    }
}
