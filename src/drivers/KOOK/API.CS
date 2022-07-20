using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using KanonBot.Message;
using KanonBot.Serializer;
using Flurl;
using Flurl.Http;
using khl = KaiHeiLa;
using KaiHeiLa.WebSocket;
using KaiHeiLa.API;
using KaiHeiLa.API.Rest;


using Serilog;

namespace KanonBot.Drivers;
public partial class KOOK
{
    // API 部分 * 包装 Driver
    public class API
    {
        string AuthToken;
        public static readonly string EndPoint = "https://www.kaiheila.cn/api/v3";
        public API(string authToken)
        {
            this.AuthToken = $"Bot {authToken}";
        }

        IFlurlRequest http()
        {
            return EndPoint.WithHeader("Authorization", this.AuthToken);
        }

        async public Task<string> GetWebsocketUrl()
        {
            var res = await this.http()
                .AppendPathSegments("gateway", "index")
                .SetQueryParam("compress", 0)
                .GetJsonAsync<JObject>();

            if (((int)res["code"]!) != 0)
            {
                throw new Exception($"无法获取开黑啦WebSocket地址，Code：{res["code"]}，Message：{res["message"]}");
            }

            return res["data"]!["url"]!.ToString();
        }


        /// <summary>
        /// 传入文件数据与文件名，如无文件名则会随机生成字符串
        /// </summary>
        /// <param name="data"></param>
        /// <param name="filename"></param>
        /// <returns>url</returns>
        async public Task<string> CreateAsset(Stream data, string? filename = null)
        {
            var res = await this.http()
                .AppendPathSegments("asset", "create")
                .SetQueryParam("compress", 0)
                .PostMultipartAsync(mp => mp
                    .AddFile("file", data, filename ?? Utils.RandomStr(10))
                );
            var j = await res.GetJsonAsync<JObject>();
            return j["data"]!["url"]!.ToString();
        }

        

        async public Task SendPrivateMessage(string userID, Chain msgChain, Guid? QuotedMessageId = null)
        {
            var messages = await Message.Build(this, msgChain);
            foreach (var msg in messages)
            {
                msg.TargetId = userID;
                msg.QuotedMessageId = QuotedMessageId;
                await this.http()
                .AppendPathSegments("direct-message", "create")
                .PostJsonAsync(msg);
            }
        }
        async public Task SendChannelMessage(string channelID, Chain msgChain, Guid? QuotedMessageId = null, string? TempMsgTargetId = null)
        {
            var messages = await Message.Build(this, msgChain);
            if (messages.Count > 0)
                messages[0].QuotedMessageId = QuotedMessageId;
            foreach (var msg in messages)
            {
                msg.TargetId = channelID;
                msg.EphemeralUserId = TempMsgTargetId;
                await this.http()
                .AppendPathSegments("message", "create")
                .PostJsonAsync(msg);
            }
        }
    }
}