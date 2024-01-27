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
        public string? AuthToken;
        private DateTime tokenExpiryTime;

        public API(bool sandbox)
        {
            this.EndPoint = sandbox ? SandboxEndPoint : DefaultEndPoint;
        }

        public async Task UpdateAuthTokenAsync(string appId, string clientSecret)
        {
            if (!(DateTime.Now >= this.tokenExpiryTime || string.IsNullOrEmpty(this.AuthToken)))
                return;

            string access_token = "", expires_in = "";

            try
            {
                JObject j = new()
                {
                    { "appId", appId },
                    { "clientSecret", clientSecret },
                };

                var response = await "https://bots.qq.com/app/getAppAccessToken"
                    .PostJsonAsync(j)
                    .ReceiveJson<JObject>();

                access_token = response["access_token"]!.ToString();
                expires_in = response["expires_in"]!.ToString();
            }
            catch (Exception ex)
            {
                Log.Error($"获取 access_token 失败: {ex.Message}");
            }

            this.AuthToken = $"QQBot {access_token}";

            this.tokenExpiryTime = DateTime.Now.AddSeconds(int.Parse(expires_in));
        }

        IFlurlRequest http()
        {
            return this.EndPoint.WithHeader("Authorization", AuthToken);
        }

        public async Task<string> GetWebsocketUrl()
        {
            //var x = await this.http().AppendPathSegments("gateway", "bot").GetJsonAsync<JObject>();
            //Log.Information(x.ToString());
            return (await this.http().AppendPathSegments("gateway", "bot").GetJsonAsync<JObject>())["url"]!.ToString();
        }

        public async Task<Models.MessageData> SendMessage(
            string ChannelID,
            Models.SendMessageData data
        )
        {

            return await this.http()
                .AppendPathSegments("channels", ChannelID, "messages")
                .PostJsonAsync(data)
                .ReceiveJson<Models.MessageData>();
        }



    }
}
