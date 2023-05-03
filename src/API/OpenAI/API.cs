using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Completions;
using OpenAI_API.Models;
using OpenAI_API.Moderation;

namespace KanonBot.API
{
    public partial class OpenAI
    {
        private static Config.Base config = Config.inner!;
        private static List<string> ChatHistory = new();
        private static readonly int ChatBotMemorySpan = 10;

        public static string Chat(string chatmsg, string name)
        {
            OpenAIAPI? api;
            Conversation? chat;
            api = new OpenAIAPI(config.openai!.Key);
            chat = api.Chat.CreateConversation();
            chat.Model = Model.ChatGPTTurbo;
            chat.RequestParameters.Temperature = config.openai.Temperature;
            chat.RequestParameters.TopP = config.openai.Top_p;
            chat.RequestParameters.MaxTokens = config.openai.MaxTokens;
            chat.RequestParameters.NumChoicesPerMessage = 1;
            if (config.openai!.PreDefine!.IndexOf("#") > 0)
            {
                chat!.AppendSystemMessage(config.openai!.PreDefine!);
            }
            else
            {
                var t = config.openai!.PreDefine!.Split("#");
                foreach (var item in t) chat!.AppendSystemMessage(item);
            }
            if (ChatHistory.Count < ChatBotMemorySpan)
                ChatHistory.Add(chatmsg);
            else
            {
                ChatHistory.RemoveAt(0);
                ChatHistory.Add(chatmsg);
            }
            chat!.AppendUserInputWithName(name, chatmsg);
            return chat.GetResponseFromChatbotAsync().Result;
        }
    }
}
