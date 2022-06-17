using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using KanonBot.Message;
using KanonBot.Serializer;
using Flurl;
using Flurl.Http;

namespace KanonBot.Drivers;
public partial class Kaiheila
{
    // API 部分 * 包装 Driver
    public class API
    {
        string AuthToken;
        public static readonly string EndPoint = "https://www.kaiheila.cn/api/v3";
        public API(string authToken)
        {
            this.AuthToken = authToken;
        }

        IFlurlRequest http()
        {
            return EndPoint.WithHeader("Authorization", this.AuthToken);
        }

        public string GetWebsocketUrl()
        {
            var res = this.http()
                .AppendPathSegments("gateway", "index")
                .GetJsonAsync<JObject>()
                .Result;

            if (((int)res["code"]!) != 0)
            {
                throw new Exception($"无法获取开黑啦WebSocket地址，Code：{res["code"]}，Message：{res["message"]}");
            }

            return res["data"]!["url"]!.ToString();
        }

        // async public Task<Models.MessageData> SendMessage(string ChannelID, Models.SendMessageData data)
        // {
        //     return await this.http()
        //         .AppendPathSegments("channels", ChannelID, "messages")
        //         .PostJsonAsync(data)
        //         .ReceiveJson<Models.MessageData>();
        // }



    }
}