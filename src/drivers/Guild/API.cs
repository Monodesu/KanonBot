using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using KanonBot.Message;
using KanonBot.Serializer;
using Flurl;
using Flurl.Http;

namespace KanonBot.Drivers;
public partial class Guild
{
    // API 部分 * 包装 Driver
    public class API
    {
        public static readonly string DefaultEndPoint = "https://api.sgroup.qq.com";
        public static readonly string SandboxEndPoint = "https://sandbox.api.sgroup.qq.com";
        string EndPoint;
        string AuthToken;
        public API(string authToken,bool sandbox)
        {
            this.EndPoint = sandbox ? SandboxEndPoint : DefaultEndPoint;
            this.AuthToken = authToken;
        }

        IFlurlRequest http()
        {
            return this.EndPoint.WithHeader("Authorization", this.AuthToken);
        }

        public string GetWebsocketUrl()
        {
            return this.http()
                .AppendPathSegments("gateway", "bot")
                .GetJsonAsync<JObject>()
                .Result["url"]!.ToString();
        }

        async public Task<Models.MessageData> SendMessage(string ChannelID, string Content, string MsgID)
        {
            return await this.http()
                .AppendPathSegments("channels", ChannelID, "messages")
                .PostJsonAsync(new 
                    { content = Content, msg_id = MsgID }
                )
                .ReceiveJson<Models.MessageData>();
        }


    }
}