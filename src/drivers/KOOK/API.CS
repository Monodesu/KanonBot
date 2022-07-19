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
                .SetQueryParam("compress", 0)
                .GetJsonAsync<JObject>()
                .Result;

            if (((int)res["code"]!) != 0)
            {
                throw new Exception($"无法获取开黑啦WebSocket地址，Code：{res["code"]}，Message：{res["message"]}");
            }

            return res["data"]!["url"]!.ToString();
        }

        async public Task SendMessage(ISocketMessageChannel channel, Chain chain)
        {
            foreach (var seg in chain.ToList())
            {
                switch (seg)
                {
                    case ImageSegment s:
                        switch (s.t)
                        {
                            case ImageSegment.Type.Base64:
                                var _s = Utils.Byte2Stream(Convert.FromBase64String(s.value));
                                _s.Seek(0, SeekOrigin.Begin);
                                var res = await channel.SendImageMessageAsync(_s);
                                Log.Debug(res.MessageId.ToString());
                                break;
                            case ImageSegment.Type.File:
                                var __s = Utils.LoadFile2Stream(s.value);
                                __s.Seek(0, SeekOrigin.Begin);
                                await channel.SendImageMessageAsync(__s);
                                break;
                            case ImageSegment.Type.Url:
                                await channel.SendTextMessageAsync(s.value);
                                break;
                            default:
                                break;
                        }
                        break;
                    case TextSegment s:
                        await channel.SendTextMessageAsync(Utils.KOOKEscape(s.value));
                        break;
                    case AtSegment s:
                        if (s.platform == Platform.KOOK)
                            await channel.SendTextMessageAsync($"(met){s.value}(met)");
                        else
                            throw new ArgumentException("不支持的平台类型");
                        break;
                    default:
                        await channel.SendTextMessageAsync(seg.Build());
                        break;
                }
            }
        }



    }
}