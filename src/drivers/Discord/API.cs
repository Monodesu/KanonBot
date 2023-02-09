using Newtonsoft.Json.Linq;
using KanonBot.Message;
using System.IO;

namespace KanonBot.Drivers;
public partial class Discord
{
    // API 部分 * 包装 Driver
    public class API
    {
        string AuthToken;
        public static readonly string EndPoint = "https://www.kookapp.cn/api/v3";
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
                throw new Exception($"无法获取KOOK WebSocket地址，Code：{res["code"]}，Message：{res["message"]}");
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

        

    }
}