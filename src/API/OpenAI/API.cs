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
        private static OpenAIAPI? api;
        private static Conversation? chat;

        public static void Init()
        {
            api = new OpenAIAPI(config.openai!.Key);
            chat = api.Chat.CreateConversation();
            chat.Model = Model.ChatGPTTurbo;
            chat.RequestParameters.Temperature = config.openai.Temperature;
            chat.RequestParameters.TopP = config.openai.Top_p;
            chat.RequestParameters.MaxTokens = config.openai.MaxTokens;
            chat.RequestParameters.NumChoicesPerMessage = 1;
            var t = config.openai!.PreDefine!.Split("#");
            foreach (var item in t) chat!.AppendSystemMessage(item);
        }

        public static string Chat(string chatmsg, string name, bool admin)
        {
            if (admin)
                chat!.AppendSystemMessage(chatmsg);
            else
                chat!.AppendUserInputWithName(name, chatmsg);
            return chat.GetResponseFromChatbotAsync().Result;
        }
    }
}
