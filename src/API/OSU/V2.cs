using Newtonsoft.Json.Linq;
using System.Net;
using KanonBot.Serializer;
using System.IO;
using Flurl.Http;
using static KanonBot.API.OSU.Models;

namespace KanonBot.API.OSU
{
    static public partial class V2
    {
        private static Config.Base config = Config.inner!;
        private static string Token { get; set; } = "";
        private static long TokenExpireTime { get; set; } = 0;

        public static readonly string EndPointV1 = "https://osu.ppy.sh/api/";
        public static readonly string EndPointV2 = "https://osu.ppy.sh/api/v2/";

        public static async Task<IFlurlRequest> HttpAsync()
        {
            await CheckTokenAsync();
            return EndPointV2.WithHeader("Authorization", $"Bearer {Token}").AllowHttpStatus(HttpStatusCode.NotFound);
        }

        // 异步获取特定谱面信息 搜索
        public static async Task<Models.BeatmapSearchResult?> SearchBeatmapAsync(string filters)
        {
            try
            {
                var request = await HttpAsync();

                var response = await request
                    .AppendPathSegments("beatmapsets", "search")
                    .SetQueryParams(new { q = filters })
                    .GetAsync();

                if (response.ResponseMessage.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                else if (!response.ResponseMessage.IsSuccessStatusCode)
                {
                    Log.Error($"请求失败，状态码：{response.StatusCode}");
                    return null;
                }

                return await response.GetJsonAsync<Models.BeatmapSearchResult>();
            }
            catch (Exception ex)
            {
                Log.Error($"获取谱面信息时出错：{ex.Message}");
                return null;
            }
        }

        // 异步获取特定谱面信息 获取
        public static async Task<Models.Beatmap?> GetBeatmapAsync(long bid)
        {
            try
            {
                var request = await HttpAsync();

                var response = await request
                    .AppendPathSegments("beatmaps", bid)
                    .GetAsync();

                if (response.ResponseMessage.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                else if (!response.ResponseMessage.IsSuccessStatusCode)
                {
                    Log.Error($"请求失败，状态码：{response.ResponseMessage.StatusCode}");
                    return null;
                }

                return await response.GetJsonAsync<Models.Beatmap>();
            }
            catch (Exception ex)
            {
                Log.Error($"获取谱面信息时出错：{ex.Message}");
                return null;
            }
        }

        // 获取用户成绩 Score type. Must be one of these: best, firsts, recent. def.best
        public static async Task<Models.Score[]?> GetUserScores(
            long userId,
            Enums.UserScoreType scoreType = Enums.UserScoreType.Best,
            Enums.Mode mode = Enums.Mode.OSU,
            int limit = 1, int offset = 0,
            bool includeFails = true)
        {
            try
            {
                var request = await HttpAsync();

                var response = await request
                    .AppendPathSegments(new object[] { "users", userId, "scores", scoreType.ToStr() })
                    .SetQueryParams(
                        new
                        {
                            include_fails = includeFails ? 1 : 0,
                            limit,
                            offset,
                            mode = mode.ToStr()
                        }
                    )
                    .GetAsync();

                if (response.ResponseMessage.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                else if (!response.ResponseMessage.IsSuccessStatusCode)
                {
                    Log.Error($"请求失败，状态码：{response.ResponseMessage.StatusCode}");
                    return null;
                }

                return await response.GetJsonAsync<Models.Score[]>();
            }
            catch (Exception ex)
            {
                Log.Error($"获取用户成绩时出错：{ex.Message}");
                return null;
            }
        }

        // 获取用户在特定谱面上的成绩
        public static async Task<Models.BeatmapScore?> GetUserBeatmapScore(
            long UserId, long bid, string[] mods,
            Enums.Mode mode = Enums.Mode.OSU)
        {
            try
            {
                var request = await HttpAsync();

                var response = await request
                    .AppendPathSegments(new object[] { "beatmaps", bid, "scores", "users", UserId })
                    .SetQueryParam("mode", mode.ToStr())
                    .SetQueryParam("mods[]", mods)
                    .GetAsync();

                if (response.ResponseMessage.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                else if (!response.ResponseMessage.IsSuccessStatusCode)
                {
                    Log.Error($"请求失败，状态码：{response.StatusCode}");
                    return null;
                }

                return await response.GetJsonAsync<Models.BeatmapScore>();
            }
            catch (Exception ex)
            {
                Log.Error($"获取谱面信息时出错：{ex.Message}");
                return null;
            }
        }

        // 获取用户在特定谱面上的成绩  返回null代表找不到beatmap / beatmap无排行榜  返回[]则用户无在此谱面的成绩
        public static async Task<Models.Score[]?> GetUserBeatmapScores(long UserId, long bid, Enums.Mode mode = Enums.Mode.OSU)
        {
            try
            {
                var request = await HttpAsync();

                var response = await request
                    .AppendPathSegments(new object[] { "beatmaps", bid, "scores", "users", UserId, "all" })
                    .SetQueryParam("mode", mode.ToStr())
                    .GetAsync();

                if (response.ResponseMessage.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                else if (!response.ResponseMessage.IsSuccessStatusCode)
                {
                    Log.Error($"请求失败，状态码：{response.StatusCode}");
                    return null;
                }

                return (await response.GetJsonAsync<JObject>())["scores"]!.ToObject<Models.Score[]>();
            }
            catch (Exception ex)
            {
                Log.Error($"获取谱面信息时出错：{ex.Message}");
                return null;
            }
        }

        // 通过osu uid获取用户信息
        public static async Task<Models.User?> GetUser(long userId, Enums.Mode mode = Enums.Mode.OSU)
        {
            try
            {
                var request = await HttpAsync();

                var response = await request
                    .AppendPathSegments(new object[] { "users", userId, mode.ToStr() })
                    .GetAsync();

                if (response.ResponseMessage.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                else if (!response.ResponseMessage.IsSuccessStatusCode)
                {
                    Log.Error($"请求失败，状态码：{response.StatusCode}");
                    return null;
                }

                return await response.GetJsonAsync<Models.User>();
            }
            catch (Exception ex)
            {
                Log.Error($"获取用户信息时出错：{ex.Message}");
                return null;
            }
        }

        // 通过osu username获取用户信息
        public static async Task<Models.User?> GetUser(string userName, Enums.Mode mode = Enums.Mode.OSU)
        {
            try
            {
                var request = await HttpAsync();

                var response = await request
                    .AppendPathSegments(new object[] { "users", userName, mode.ToStr() })
                    .SetQueryParams("key", "username")
                    .GetAsync();

                if (response.ResponseMessage.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                else if (!response.ResponseMessage.IsSuccessStatusCode)
                {
                    Log.Error($"请求失败，状态码：{response.StatusCode}");
                    return null;
                }

                return await response.GetJsonAsync<Models.User>();
            }
            catch (Exception ex)
            {
                Log.Error($"获取用户信息时出错：{ex.Message}");
                return null;
            }
        }

        // 获取谱面参数
        public static async Task<Models.BeatmapAttributes?> GetBeatmapAttributes(
            long bid, string[] mods,
            Enums.Mode mode = Enums.Mode.OSU)
        {
            try
            {
                JObject j = new()
                {
                    { "mods", new JArray(mods) },
                    { "ruleset", mode.ToStr() },
                };

                var request = await HttpAsync();

                var response = await request
                    .AppendPathSegments(new object[] { "beatmaps", bid, "attributes" })
                    .PostJsonAsync(j);

                if (response.ResponseMessage.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                else if (!response.ResponseMessage.IsSuccessStatusCode)
                {
                    Log.Error($"请求失败，状态码：{response.StatusCode}");
                    return null;
                }

                var body = await response.GetJsonAsync<JObject>();
                var beatmap = body["attributes"]!.ToObject<Models.BeatmapAttributes>()!;
                beatmap.Mode = mode;
                return beatmap;
            }
            catch (Exception ex)
            {
                Log.Error($"获取谱面信息时出错：{ex.Message}");
                return null;
            }
        }

        // 搜索用户数量 未使用
        public static async Task<JObject?> SearchUser(string userName)
        {
            try
            {
                var request = await HttpAsync();

                var response = await request
                    .AppendPathSegments("search")
                    .SetQueryParams(new { mode = "user", query = userName })
                    .GetAsync();

                if (response.ResponseMessage.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                else if (!response.ResponseMessage.IsSuccessStatusCode)
                {
                    Log.Error($"请求失败，状态码：{response.StatusCode}");
                    return null;
                }

                return (await response.GetJsonAsync<JObject>())["user"] as JObject;
            }
            catch (Exception ex)
            {
                Log.Error($"获取谱面信息时出错：{ex.Message}");
                return null;
            }
        }


        // =============以下内容暂未修改============= //

        // 小夜api版（备选方案）
        async public static Task<string?> SayoDownloadBeatmapBackgroundImg(long sid, long bid, string folderPath, string? fileName = null)
        {
            var url = $"https://api.sayobot.cn/v2/beatmapinfo?K={sid}";
            var body = await url.GetJsonAsync<JObject>()!;
            fileName ??= $"{bid}.png";

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


        // 获取pp+数据
        async public static Task<Models.PPlusData> GetUserPlusData(long uid)
        {
            var res = await $"https://syrin.me/pp+/api/user/{uid}/".GetJsonAsync<JObject>();
            var data = new Models.PPlusData()
            {
                User = res["user_data"]!.ToObject<Models.PPlusData.UserData>()!,
                Performances = res["user_performances"]!["total"]!.ToObject<Models.PPlusData.UserPerformances[]>()
            };
            return data;
        }

        async public static Task<Models.PPlusData> GetUserPlusData(string username)
        {
            var res = await $"https://syrin.me/pp+/api/user/{username}/".GetJsonAsync<JObject>();
            var data = new Models.PPlusData()
            {
                User = res["user_data"]!.ToObject<Models.PPlusData.UserData>()!,
                Performances = res["user_performances"]!["total"]!.ToObject<Models.PPlusData.UserPerformances[]>()
            };
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
                    return await GetUserPlusData(user.Username!);
                }
                catch
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
                await Http.DownloadFileAsync($"http://osu.ppy.sh/osu/{bid}", $"./work/beatmap/{bid}.osu");
            }
        }
    }
}

