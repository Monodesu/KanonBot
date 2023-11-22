using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using static KanonBot.API.OSU.V2;

namespace KanonBot.API.OSU
{
    public static partial class V2
    {
        public static async Task<bool> GetTokenAsync()
        {
            var requestData = new JObject
            {
                { "grant_type", "client_credentials" },
                { "client_id", config.osu?.clientId },
                { "client_secret", config.osu?.clientSecret },
                { "scope", "public" },
                { "code", "kanon" },
            };

            JObject responseBody = new();

            try
            {
                var response = await "https://osu.ppy.sh/oauth/token".PostJsonAsync(requestData);
                responseBody = await response.GetJsonAsync<JObject>();

                Token = responseBody["access_token"]?.ToString() ?? "";
                TokenExpireTime =
                    DateTimeOffset.Now.ToUnixTimeSeconds()
                    + (
                        long.TryParse(responseBody["expires_in"]?.ToString(), out var expiresIn)
                            ? expiresIn
                            : 0
                    );

                return true;
            }
            catch (Exception ex) // 指定具体的异常类型
            {
                Log.Error(
                    $"获取token失败: {ex.Message}, 返回Body: \n({responseBody?.ToString() ?? "无"})"
                );
                return false;
            }
        }

        public static async Task CheckTokenAsync()
        {
            if (TokenExpireTime <= DateTimeOffset.Now.ToUnixTimeSeconds())
            {
                string tokenStatus =
                    TokenExpireTime == 0 ? "正在获取OSUApiV2_Token" : "OSUApiV2_Token已过期, 正在重新获取";
                Log.Information(tokenStatus);

                if (await GetTokenAsync())
                {
                    // 避免在日志中显示完整的令牌信息
                    Log.Information(
                        $"获取成功, Token: {Token.Substring(0, Math.Min(Token.Length, 3))}..."
                    );
                    Log.Information(
                        $"Token过期时间: {DateTimeOffset.FromUnixTimeSeconds(TokenExpireTime).DateTime.ToLocalTime()}"
                    );
                }
            }
        }
    }
}
