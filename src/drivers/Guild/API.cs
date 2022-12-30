using Newtonsoft.Json.Linq;

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

        async public Task<string> GetWebsocketUrl()
        {
            return (await this.http().AppendPathSegments("gateway", "bot").GetJsonAsync<JObject>())["url"]!.ToString();
        }

        async public Task<Models.MessageData> SendMessage(string ChannelID, Models.SendMessageData data)
        {
            return await this.http()
                .AppendPathSegments("channels", ChannelID, "messages")
                .PostJsonAsync(data)
                .ReceiveJson<Models.MessageData>();
        }



    }
}