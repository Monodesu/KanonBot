#pragma warning disable CS8618 // 非null 字段未初始化
// Flurl.Http.FlurlHttpTimeoutException
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;
using Serilog;
using Flurl;
using Flurl.Http;
using System.Security.Cryptography;
using static KanonBot.API.OSU.Models;
using SqlSugar.Extensions;
using KanonBot.functions.osu.rosupp;

namespace KanonBot.API
{
    public partial class OSU
    {
        private static Config.Base config = Config.inner!;
        private static string Token = "";
        private static long TokenExpireTime = 0;
        public static readonly string EndPointV1 = "https://osu.ppy.sh/api/";
        public static readonly string EndPointV2 = "https://osu.ppy.sh/api/v2/";

        static IFlurlRequest http()
        {
            CheckToken().Wait();
            return EndPointV2.WithHeader("Authorization", $"Bearer {Token}").AllowHttpStatus(HttpStatusCode.NotFound);
        }

        async private static Task<bool> GetToken()
        {
            JObject j = new()
            {
                { "grant_type", "client_credentials" },
                { "client_id", config.osu?.clientId },
                { "client_secret", config.osu?.clientSecret },
                { "scope", "public" },
                { "code", "kanon" },
            };

            var result = await "https://osu.ppy.sh/oauth/token".PostJsonAsync(j);


            var body = await result.GetJsonAsync<JObject>();
            try
            {
                Token = ((string?)body["access_token"]) ?? "";
                TokenExpireTime = DateTimeOffset.Now.ToUnixTimeSeconds() + long.Parse(((string?)body["expires_in"]) ?? "0");
                return true;
            }
            catch
            {
                Log.Error("获取token失败, 返回Body: \n({})", body.ToString());
                return false;
            }
        }

        async public static Task CheckToken()
        {
            if (TokenExpireTime == 0)
            {
                Log.Information("正在获取OSUApiV2_Token");
                if (await GetToken())
                {
                    Log.Information($"获取成功, Token: {Token.Substring(0, Utils.TryGetConsoleWidth() - 38) + "..."}");
                    Log.Information($"Token过期时间: {DateTimeOffset.FromUnixTimeSeconds(TokenExpireTime).DateTime.ToLocalTime()}");
                }
            }
            else if (TokenExpireTime <= DateTimeOffset.Now.ToUnixTimeSeconds())
            {
                Log.Information("OSUApiV2_Token已过期, 正在重新获取");
                if (await GetToken())
                {
                    Log.Information($"获取成功, Token: {Token.Substring(0, Utils.TryGetConsoleWidth() - 38) + "..."}");
                    Log.Information($"Token过期时间: {DateTimeOffset.FromUnixTimeSeconds(TokenExpireTime).DateTime.ToLocalTime()}");
                }
            }
        }


        // 获取特定谱面信息
        async public static Task<Models.Beatmap?> GetBeatmap(long bid)
        {
            var res = await http()
                .AppendPathSegments(new object[] { "beatmaps", bid })
                .GetAsync();

            if (res.StatusCode == 404)
                return null;
            else
                return await res.GetJsonAsync<Models.Beatmap>();
        }


        // 获取用户成绩
        // Score type. Must be one of these: best, firsts, recent.
        // 默认 best
        async public static Task<Models.Score[]?> GetUserScores(long userId, Enums.UserScoreType scoreType = Enums.UserScoreType.Best, Enums.Mode mode = Enums.Mode.OSU, int limit = 1, int offset = 0, bool includeFails = true)
        {
            var res = await http()
                .AppendPathSegments(new object[] { "users", userId, "scores", scoreType.ToScoreTypeStr() })
                .SetQueryParams(new
                {
                    include_fails = includeFails ? 1 : 0,
                    limit = limit,
                    offset = offset,
                    mode = mode.ToModeStr()
                })
                .GetAsync();

            //Console.WriteLine(await res.GetStringAsync());
            if (res.StatusCode == 404)
                return null;
            else
                return await res.GetJsonAsync<Models.Score[]>();
        }

        // 获取用户在特定谱面上的成绩
        async public static Task<Models.BeatmapScore?> GetUserBeatmapScore(long UserId, long bid, string[] mods, Enums.Mode mode = Enums.Mode.OSU)
        {
            var req = http()
                .AppendPathSegments(new object[] { "beatmaps", bid, "scores", "users", UserId })
                .SetQueryParam("mode", mode.ToModeStr());

            foreach (var mod in mods) { req.SetQueryParam("mods[]", mod); }

            var res = await req.GetAsync();
            if (res.StatusCode == 404)
                return null;
            else
                return await res.GetJsonAsync<Models.BeatmapScore>();
        }

        // 获取用户在特定谱面上的成绩
        // 返回null代表找不到beatmap / beatmap无排行榜
        // 返回[]则用户无在此谱面的成绩
        async public static Task<Models.Score[]?> GetUserBeatmapScores(long UserId, long bid, Enums.Mode mode = Enums.Mode.OSU)
        {
            var res = await http()
                .AppendPathSegments(new object[] { "beatmaps", bid, "scores", "users", UserId, "all" })
                .SetQueryParam("mode", mode.ToModeStr())
                .GetAsync();

            if (res.StatusCode == 404)
                return null;
            else
                return (await res.GetJsonAsync<JObject>())["scores"]!.ToObject<Models.Score[]>();
        }

        // 通过osu uid获取用户信息
        async public static Task<Models.User?> GetUser(long userId, Enums.Mode mode = Enums.Mode.OSU)
        {
            var res = await http()
                .AppendPathSegments(new object[] { "users", userId, mode.ToModeStr() })
                .GetAsync();

            if (res.StatusCode == 404)
                return null;
            else
                try
                {
                    return await res.GetJsonAsync<Models.User>();
                }
                catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(res.ResponseMessage.Content.ToString());
                    Console.WriteLine(res.GetStringAsync().Result.ToString());
                    
                    return null;
                }
        }

        // 通过osu username获取用户信息
        async public static Task<Models.User?> GetUser(string userName, Enums.Mode mode = Enums.Mode.OSU)
        {
            var res = await http()
                .AppendPathSegments(new object[] { "users", userName, mode.ToModeStr() })
                .SetQueryParam("key", "username")
                .GetAsync();

            if (res.StatusCode == 404)
                return null;
            else
                return await res.GetJsonAsync<Models.User>();
        }

        // 获取谱面参数
        async public static Task<Models.BeatmapAttributes?> GetBeatmapAttributes(long bid, string[] mods, Enums.Mode mode = Enums.Mode.OSU)
        {
            JObject j = new()
            {
                { "mods", new JArray(mods) },
                { "ruleset", mode.ToModeStr() },
            };

            var res = await http()
                .AppendPathSegments(new object[] { "beatmaps", bid, "attributes" })
                .PostJsonAsync(j);

            if (res.StatusCode == 404)
            {
                return null;
            }
            else
            {
                var body = await res.GetJsonAsync<JObject>();
                var beatmap = body["attributes"]!.ToObject<Models.BeatmapAttributes>()!;
                beatmap.Mode = mode;
                return beatmap;
            }
        }

        // 小夜api版（备选方案）
        async public static Task<string?> SayoDownloadBeatmapBackgroundImg(long sid, long bid, string folderPath, string? fileName = null)
        {
            var url = $"https://api.sayobot.cn/v2/beatmapinfo?K={sid}";
            var body = await url.GetJsonAsync<JObject>()!;
            if (fileName == null)
                fileName = $"{bid}.png";

            foreach (var item in body["data"]!["bid_data"]!)
            {
                if (((long)item["bid"]!) == bid)
                {
                    string bgFileName;
                    try { bgFileName = ((string?)item["bg"])!; }
                    catch { return null; }
                    return await $"https://dl.sayobot.cn/beatmaps/files/{sid}/{bgFileName}".DownloadFileAsync(folderPath, fileName);
                }
            }
            return null;
        }

        // 搜索用户数量 未使用
        async public static Task<JObject?> SearchUser(string userName)
        {
            var body = await http()
                .AppendPathSegment("search")
                .SetQueryParams(new
                {
                    mode = "user",
                    query = "userName"
                })
                .GetJsonAsync<JObject>();
            return body["user"] as JObject;
        }

        // 获取用户Elo信息
        async public static Task<JObject?> GetUserEloInfo(long uid)
        {
            return await $"http://api.osuwiki.cn:5005/api/users/elo/{uid}".GetJsonAsync<JObject>();
        }

        // 获取用户最近的elo游戏记录
        async public static Task<int?> GetUserEloRecentPlay(long uid)
        {
            var body = await $"http://api.osuwiki.cn:5005/api/users/recentPlay/{uid}".GetJsonAsync<JObject>();
            return (int?)body["match_id"];
        }

        // 获取比赛信息
        async public static Task<JObject?> GetMatchInfo(long matchId)
        {
            return await $"http://api.osuwiki.cn:5005/api/matches/{matchId}".GetJsonAsync<JObject>();
        }

        // 获取pp+数据
        async public static Task<Models.PPlusData> GetUserPlusData(long uid)
        {
            var res = await $"https://syrin.me/pp+/api/user/{uid}/".GetJsonAsync<JObject>();
            var data = new Models.PPlusData();
            data.User = res["user_data"]!.ToObject<Models.PPlusData.UserData>()!;
            data.Performances = res["user_performances"]!["total"]!.ToObject<Models.PPlusData.UserPerformances[]>();
            return data;
        }

        async public static Task<Models.PPlusData> GetUserPlusData(string username)
        {
            var res = await $"https://syrin.me/pp+/api/user/{username}/".GetJsonAsync<JObject>();
            var data = new Models.PPlusData();
            data.User = res["user_data"]!.ToObject<Models.PPlusData.UserData>()!;
            data.Performances = res["user_performances"]!["total"]!.ToObject<Models.PPlusData.UserPerformances[]>();
            return data;
        }
        
        async public static Task<Models.PPlusData?> TryGetUserPlusData(OSU.Models.User user)
        {
            try
            {
                return await GetUserPlusData(user.Id);
            }
            catch
            {
                try
                {
                    return await GetUserPlusData(user.Username);
                }
                catch (System.Exception)
                {
                    
                    return null;
                }
            }
        }

        async public static Task BeatmapFileChecker(long bid)
        {
            if (!Directory.Exists("./work/beatmap/")) Directory.CreateDirectory("./work/beatmap/");
            if (!File.Exists($"./work/beatmap/{bid}.osu"))
            {
                await Http.DownloadFile($"http://osu.ppy.sh/osu/{bid}", $"./work/beatmap/{bid}.osu");
            }
        }
    }
}
