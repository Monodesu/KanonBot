#pragma warning disable CS8604 // 引用类型参数可能为 null。
#pragma warning disable CS8602 // 解引用可能出现空引用。
#pragma warning disable CS8600 // 将 null 字面量或可能为 null 的值转换为非 null 类型。
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;
using Serilog;

namespace KanonBot.API
{
    public static class Osu
    {
        private static Config.Base config = Config.inner!;
        public enum Mods
        {
            None = 0,
            NoFail = 1,
            Easy = 2,
            TouchDevice = 4,
            Hidden = 8,
            HardRock = 16,
            SuddenDeath = 32,
            DoubleTime = 64,
            Relax = 128,
            HalfTime = 256,
            Nightcore = 512, // Only set along with DoubleTime. i.e: NC only gives 576
            Flashlight = 1024,
            Autoplay = 2048,
            SpunOut = 4096,
            Relax2 = 8192, // Autopilot
            Perfect = 16384, // Only set along with SuddenDeath. i.e: PF only gives 16416
            Key4 = 32768,
            Key5 = 65536,
            Key6 = 131072,
            Key7 = 262144,
            Key8 = 524288,
            FadeIn = 1048576,
            Random = 2097152,
            Cinema = 4194304,
            Target = 8388608,
            Key9 = 16777216,
            KeyCoop = 33554432,
            Key1 = 67108864,
            Key3 = 134217728,
            Key2 = 268435456,
            ScoreV2 = 536870912,
            Mirror = 1073741824,
            KeyMod = Key1 | Key2 | Key3 | Key4 | Key5 | Key6 | Key7 | Key8 | Key9 | KeyCoop,
            FreeModAllowed =
            NoFail | Easy | Hidden | HardRock | SuddenDeath | Flashlight | FadeIn | Relax | Relax2 | SpunOut | KeyMod,
            ScoreIncreaseMods = Hidden | HardRock | DoubleTime | Flashlight | FadeIn
        }
        public struct BeapmapInfo
        {
            public string mode, beatmapStatus;
            public long beatmapId,
                beatmapsetId,
                creatorId,
                hitLength,     // 第一个note到最后一个note的时长
                totalLength,
                totalPlaycount,
                playCount,
                passCount;
            public int favouriteCount,
                circleCount,
                sliderCount,
                spinnerCount,
                maxCombo;

            // Accuracy == OverallDifficulty(OD)
            public float BPM, circleSize, approachRate, accuracy, HPDrainRate, difficultyRating;
            public List<string> tags;
            public bool hasVideo, isNSFW, canDownload;
            public string previewUrl, artist, artistUnicode, title, titleUnicode, creator, source, version, fileChecksum, backgroundImgUrl, musicUrl;
            public DateTime submitTime,
            lastUpdateTime,
            rankedTime;
        }
        public struct PPInfo
        {
            // Accuracy == OverallDifficulty(OD)
            public double star, circleSize, HPDrainRate, approachRate, hitWindow, accuracy, aim, speed;
            public int maxCombo;
            public PPStat ppStat;
            public List<PPStat> ppStats;
            public struct PPStat
            {
                public double total, aim, speed, acc, strain;
                public int flashlight, effective_miss_count;
            }
        }
        public struct UserInfo
        {
            public long userId;
            public string userName, country, mode, coverUrl, avatarUrl;
            public DateTime registedTimestamp;
            public long playCount,
                // v2不提供
                // n300,
                // n100,
                // n50,

                totalHits,
                totalScore,
                rankedScore,
                countryRank,
                globalRank,
                playTime;
            public int SSH,
                SS,
                SH,
                S,
                A,
                level,
                levelProgress,
                daysBefore;
            public float pp, accuracy;
        }
        public struct PPlusInfo
        {
            public float pp;
            public int jump,
                flow,
                pre,
                acc,
                spd,
                sta;
        }
        public struct ScoreInfo
        {
            public long score,
                beatmapId,
                scoreId,
                userId;
            public int great, ok, meh, katu, geki, combo, miss;
            public float pp, acc;
            public bool hasReplay, convert; //是否为转铺
            public string rank, mode, scoreType, userName, userAvatarUrl;
            public List<string> mods;
            public DateTime achievedTime;
            public BeapmapInfo beatmapInfo;
        }

        private static string Token = "";
        private static long TokenExpireTime = 0;
        public static readonly string OSU_API_V1 = "https://osu.ppy.sh/api/";
        public static readonly string OSU_API_V2 = "https://osu.ppy.sh/api/v2/";
        public static readonly string[] Modes = { "osu", "taiko", "fruits", "mania" };

        public static void checkModes(string mode)
        {
            foreach (string m in Modes)
            {
                if (mode == m) return;
            }
            throw new Exception("OSU 模式不正确");
        }

        private static bool GetToken()
        {
            JObject j = new()
            {
                { "grant_type", "client_credentials" },
                { "client_id", config.osu?.clientId },
                { "client_secret", config.osu?.clientSecret },
                { "scope", "public" },
                { "code", "kanon" },
            };

            var result = Http.PostAsync("https://osu.ppy.sh/oauth/token", j).Result;
            try
            {
                Dictionary<string, string>? body = JsonConvert.DeserializeObject<Dictionary<string, string>>(result.Body);
                Token = body?["access_token"] ?? "";
                TokenExpireTime = DateTimeOffset.Now.ToUnixTimeSeconds() + long.Parse(body?["expires_in"] ?? "0");
                return true;
            }
            catch
            {
                Log.Error($"获取token失败, 返回Body: \n({result.Body})");
                return false;
            }
        }

        public static void CheckToken()
        {
            if (TokenExpireTime == 0)
            {
                Log.Information("正在获取OSUApiV2_Token");
                if (GetToken())
                {
                    Log.Information($"获取成功, Token: {Token.Substring(0, Console.WindowWidth - 38) + "..."}");
                    Log.Information($"Token过期时间: {DateTimeOffset.FromUnixTimeSeconds(TokenExpireTime).DateTime.ToLocalTime()}");
                }
            }
            else if (TokenExpireTime <= DateTimeOffset.Now.ToUnixTimeSeconds())
            {
                Log.Information("OSUApiV2_Token已过期, 正在重新获取");
                if (GetToken())
                {
                    Log.Information($"获取成功, Token: {Token.Substring(0, Console.WindowWidth - 38) + "..."}");
                    Log.Information($"Token过期时间: {DateTimeOffset.FromUnixTimeSeconds(TokenExpireTime).DateTime.ToLocalTime()}");
                }
            }
        }

        // 小夜api版（备选方案）
        public static bool SayoDownloadBeatmapBackgroundImg(long sid, long bid, string filePath)
        {
            var url = $"https://api.sayobot.cn/v2/beatmapinfo?K={sid}";
            var result = Http.GetAsync(url).Result;
            JObject body;
            try { body = JsonConvert.DeserializeObject<JObject>(result.Body); }
            catch { throw new Exception(result.Body); }
            foreach (var item in body["data"]["bid_data"])
            {
                if (long.Parse(item["bid"].Values().ToString()) == bid)
                {
                    string bgFileName;
                    try { bgFileName = item["bg"].Values().ToString(); }
                    catch { return false; }
                    Http.DownloadFile($"https://dl.sayobot.cn/beatmaps/files/{sid}/{bgFileName}", filePath);
                    return true;
                }
            }
            return false;
        }

        // 搜索用户数量 未使用
        public static int SearchUser(string userName, out JArray users)
        {
            CheckToken();
            Dictionary<string, string> headers = new();
            headers.Add("Authorization", $"Bearer {Token}");
            var result = Http.GetAsync(OSU_API_V2 + $"search?mode=user&query={userName}", headers).Result;
            var body = JsonConvert.DeserializeObject<JObject>(result.Body);
            users = (JArray)body["user"]["data"].Values();
            return (int)body["user"]["total"];
        }

        // 获取特定谱面信息
        public static BeapmapInfo GetBeatmap(long bid)
        {
            CheckToken();
            Dictionary<string, string> headers = new();
            headers.Add("Authorization", $"Bearer {Token}");
            var url = OSU_API_V2 + $"beatmaps/{bid}";
            var result = Http.GetAsync(url, headers).Result;
            if (result.Status != HttpStatusCode.OK || result.Body.Length < 20) { throw new Exception(result.Body); }
            JObject beatmap;
            try { beatmap = JsonConvert.DeserializeObject<JObject>(result.Body); }
            catch { throw new Exception(result.Body); }
            BeapmapInfo beatmapInfo = new();
            var beatmapSet = beatmap["beatmapset"];
            beatmapInfo.mode = beatmap["mode"].ToString();
            beatmapInfo.beatmapStatus = beatmap["status"].ToString();
            try { beatmapInfo.beatmapId = (long)beatmap["id"]; } catch { beatmapInfo.beatmapId = 0; }
            try { beatmapInfo.beatmapsetId = (long)beatmap["beatmapset_id"]; } catch { beatmapInfo.beatmapsetId = 0; }
            try { beatmapInfo.creatorId = (long)beatmap["user_id"]; } catch { beatmapInfo.creatorId = 0; }
            try { beatmapInfo.hitLength = (long)beatmap["hit_length"]; } catch { beatmapInfo.hitLength = 0; }
            try { beatmapInfo.totalLength = (long)beatmap["total_length"]; } catch { beatmapInfo.totalLength = 0; }
            try { beatmapInfo.playCount = (long)beatmap["playcount"]; } catch { beatmapInfo.playCount = 0; }
            try { beatmapInfo.passCount = (long)beatmap["passcount"]; } catch { beatmapInfo.passCount = 0; }
            try { beatmapInfo.maxCombo = (int)beatmap["max_combo"]; } catch { beatmapInfo.maxCombo = 0; }
            try { beatmapInfo.circleCount = (int)beatmap["count_circles"]; } catch { beatmapInfo.circleCount = 0; }
            try { beatmapInfo.sliderCount = (int)beatmap["count_sliders"]; } catch { beatmapInfo.sliderCount = 0; }
            try { beatmapInfo.spinnerCount = (int)beatmap["count_spinners"]; } catch { beatmapInfo.spinnerCount = 0; }
            try { beatmapInfo.BPM = (float)beatmap["bpm"]; } catch { beatmapInfo.BPM = 0; }
            try { beatmapInfo.circleSize = (float)beatmap["cs"]; } catch { beatmapInfo.circleSize = 0; }
            try { beatmapInfo.approachRate = (float)beatmap["ar"]; } catch { beatmapInfo.approachRate = 0; }
            try { beatmapInfo.accuracy = (float)beatmap["accuracy"]; } catch { beatmapInfo.accuracy = 0; }
            try { beatmapInfo.HPDrainRate = (float)beatmap["drain"]; } catch { beatmapInfo.HPDrainRate = 0; }
            try { beatmapInfo.difficultyRating = (float)beatmap["difficulty_rating"]; } catch { beatmapInfo.difficultyRating = 0; }
            try { beatmapInfo.version = beatmap["version"].ToString(); } catch { beatmapInfo.version = ""; }
            try { beatmapInfo.fileChecksum = beatmap["checksum"].ToString(); } catch { beatmapInfo.fileChecksum = ""; }
            try { beatmapInfo.favouriteCount = (int)beatmapSet["favourite_count"]; } catch { beatmapInfo.favouriteCount = 0; }
            try { beatmapInfo.artist = beatmapSet["artist"].ToString(); } catch { beatmapInfo.artist = ""; }
            try { beatmapInfo.artistUnicode = beatmapSet["artist_unicode"].ToString(); } catch { beatmapInfo.artistUnicode = ""; }
            try { beatmapInfo.title = beatmapSet["title"].ToString(); } catch { beatmapInfo.title = ""; }
            try { beatmapInfo.titleUnicode = beatmapSet["title_unicode"].ToString(); } catch { beatmapInfo.titleUnicode = ""; }
            try { beatmapInfo.creator = beatmapSet["creator"].ToString(); } catch { beatmapInfo.creator = ""; }
            try { beatmapInfo.source = beatmapSet["source"].ToString(); } catch { beatmapInfo.source = ""; }
            try { beatmapInfo.previewUrl = beatmapSet["preview_url"].ToString(); } catch { beatmapInfo.previewUrl = ""; }
            try { beatmapInfo.totalPlaycount = (long)beatmapSet["play_count"]; } catch { beatmapInfo.totalPlaycount = 0; }
            try { beatmapInfo.hasVideo = (bool)beatmapSet["video"]; } catch { beatmapInfo.hasVideo = false; }
            try { beatmapInfo.isNSFW = (bool)beatmapSet["nsfw"]; } catch { beatmapInfo.isNSFW = false; }
            try { beatmapInfo.canDownload = !(bool)beatmapSet["availability"]["download_disabled"]; } catch { beatmapInfo.canDownload = true; }
            return beatmapInfo;
        }

        // 获取用户成绩
        public static List<ScoreInfo> GetUserScores(long userId, string scoreType = "recent", string mode = "osu", int limit = 1, int offset = 0, bool includeFails = true)
        {
            checkModes(mode);
            CheckToken();
            Dictionary<string, string> headers = new();
            headers.Add("Authorization", $"Bearer {Token}");
            var url = OSU_API_V2 + $"users/{userId}/scores/{scoreType}?include_fails={(includeFails ? 1 : 0)}&limit={limit}&offset={offset}&mode={mode}";
            var result = Http.GetAsync(url, headers).Result;
            JArray body;
            try { body = JsonConvert.DeserializeObject<JArray>(result.Body); } catch { throw new Exception(result.Body); }
            List<ScoreInfo> scoreInfos = new();
            foreach (var score in body)
            {
                ScoreInfo scoreInfo = new();
                scoreInfo.mode = mode;
                scoreInfo.scoreType = scoreType;
                try { scoreInfo.userId = (long)score["user"]["id"]; } catch { scoreInfo.userId = 0; }
                try { scoreInfo.userName = score["user"]["username"].ToString(); } catch { scoreInfo.userName = ""; }
                try { scoreInfo.userAvatarUrl = score["user"]["avatar_url"].ToString(); } catch { scoreInfo.userAvatarUrl = ""; }
                try { scoreInfo.scoreId = (long)score["id"]; } catch { scoreInfo.scoreId = 0; }
                try { scoreInfo.score = (long)score["score"]; } catch { scoreInfo.score = 0; }
                try { scoreInfo.great = (int)score["statistics"]["count_300"]; } catch { scoreInfo.great = 0; } //n300, n100, n50, nmiss, nkatu, ngeki, combo;
                try { scoreInfo.ok = (int)score["statistics"]["count_100"]; } catch { scoreInfo.ok = 0; }
                try { scoreInfo.katu = (int)score["statistics"]["count_katu"]; } catch { scoreInfo.katu = 0; }
                try { scoreInfo.geki = (int)score["statistics"]["count_geki"]; } catch { scoreInfo.geki = 0; }
                try { scoreInfo.meh = (int)score["statistics"]["count_50"]; } catch { scoreInfo.meh = 0; }
                try { scoreInfo.miss = (int)score["statistics"]["count_miss"]; } catch { scoreInfo.miss = 0; }
                try { scoreInfo.combo = (int)score["max_combo"]; } catch { scoreInfo.combo = 0; }
                try { scoreInfo.pp = (float)score["pp"]; } catch { scoreInfo.pp = 0; }
                try { scoreInfo.acc = (float)score["accuracy"]; } catch { scoreInfo.acc = 0; }
                try { scoreInfo.hasReplay = (bool)score["replay"]; } catch { scoreInfo.hasReplay = false; }
                try { scoreInfo.convert = (bool)score["convert"]; } catch { scoreInfo.convert = false; }
                scoreInfo.rank = score["rank"].ToString();
                scoreInfo.achievedTime = DateTime.Parse(score["created_at"].ToString());
                var mods = JsonConvert.DeserializeObject<JArray>(score["mods"].ToString());
                scoreInfo.mods = new();
                foreach (var mod in mods) { scoreInfo.mods.Add(mod.ToString()); }
                var beatmap = score["beatmap"];
                var beatmapSet = score["beatmapset"];
                scoreInfo.beatmapInfo = new();
                scoreInfo.beatmapInfo.mode = mode;
                scoreInfo.beatmapInfo.beatmapStatus = beatmap["status"].ToString();
                try { scoreInfo.beatmapId = (long)beatmap["id"]; } catch { scoreInfo.beatmapId = 0; }
                try { scoreInfo.beatmapInfo.beatmapId = (long)beatmap["id"]; } catch { scoreInfo.beatmapInfo.beatmapId = 0; }
                try { scoreInfo.beatmapInfo.beatmapsetId = (long)beatmap["beatmapset_id"]; } catch { scoreInfo.beatmapInfo.beatmapsetId = 0; }
                try { scoreInfo.beatmapInfo.creatorId = (long)beatmap["user_id"]; } catch { scoreInfo.beatmapInfo.creatorId = 0; }
                try { scoreInfo.beatmapInfo.hitLength = (long)beatmap["hit_length"]; } catch { scoreInfo.beatmapInfo.hitLength = 0; }
                try { scoreInfo.beatmapInfo.totalLength = (long)beatmap["total_length"]; } catch { scoreInfo.beatmapInfo.totalLength = 0; }
                try { scoreInfo.beatmapInfo.playCount = (long)beatmap["playcount"]; } catch { scoreInfo.beatmapInfo.playCount = 0; }
                try { scoreInfo.beatmapInfo.passCount = (long)beatmap["passcount"]; } catch { scoreInfo.beatmapInfo.passCount = 0; }
                try { scoreInfo.beatmapInfo.circleCount = (int)beatmap["count_circles"]; } catch { scoreInfo.beatmapInfo.circleCount = 0; }
                try { scoreInfo.beatmapInfo.sliderCount = (int)beatmap["count_sliders"]; } catch { scoreInfo.beatmapInfo.sliderCount = 0; }
                try { scoreInfo.beatmapInfo.spinnerCount = (int)beatmap["count_spinners"]; } catch { scoreInfo.beatmapInfo.spinnerCount = 0; }
                try { scoreInfo.beatmapInfo.BPM = (float)beatmap["bpm"]; } catch { scoreInfo.beatmapInfo.BPM = 0; }
                try { scoreInfo.beatmapInfo.circleSize = (float)beatmap["cs"]; } catch { scoreInfo.beatmapInfo.circleSize = 0; }
                try { scoreInfo.beatmapInfo.approachRate = (float)beatmap["ar"]; } catch { scoreInfo.beatmapInfo.approachRate = 0; }
                try { scoreInfo.beatmapInfo.accuracy = (float)beatmap["accuracy"]; } catch { scoreInfo.beatmapInfo.accuracy = 0; }
                try { scoreInfo.beatmapInfo.HPDrainRate = (float)beatmap["drain"]; } catch { scoreInfo.beatmapInfo.HPDrainRate = 0; }
                try { scoreInfo.beatmapInfo.difficultyRating = (float)beatmap["difficulty_rating"]; } catch { scoreInfo.beatmapInfo.difficultyRating = 0; }
                try { scoreInfo.beatmapInfo.version = beatmap["version"].ToString(); } catch { scoreInfo.beatmapInfo.version = ""; }
                try { scoreInfo.beatmapInfo.fileChecksum = beatmap["checksum"].ToString(); } catch { scoreInfo.beatmapInfo.fileChecksum = ""; }
                try { scoreInfo.beatmapInfo.favouriteCount = (int)beatmapSet["favourite_count"]; } catch { scoreInfo.beatmapInfo.favouriteCount = 0; }
                try { scoreInfo.beatmapInfo.artist = beatmapSet["artist"].ToString(); } catch { scoreInfo.beatmapInfo.artist = ""; }
                try { scoreInfo.beatmapInfo.artistUnicode = beatmapSet["artist_unicode"].ToString(); } catch { scoreInfo.beatmapInfo.artistUnicode = ""; }
                try { scoreInfo.beatmapInfo.title = beatmapSet["title"].ToString(); } catch { scoreInfo.beatmapInfo.title = ""; }
                try { scoreInfo.beatmapInfo.titleUnicode = beatmapSet["title_unicode"].ToString(); } catch { scoreInfo.beatmapInfo.titleUnicode = ""; }
                try { scoreInfo.beatmapInfo.creator = beatmapSet["creator"].ToString(); } catch { scoreInfo.beatmapInfo.creator = ""; }
                try { scoreInfo.beatmapInfo.source = beatmapSet["source"].ToString(); } catch { scoreInfo.beatmapInfo.source = ""; }
                try { scoreInfo.beatmapInfo.previewUrl = beatmapSet["preview_url"].ToString(); } catch { scoreInfo.beatmapInfo.previewUrl = ""; }
                try { scoreInfo.beatmapInfo.totalPlaycount = (long)beatmapSet["play_count"]; } catch { scoreInfo.beatmapInfo.totalPlaycount = 0; }
                try { scoreInfo.beatmapInfo.hasVideo = (bool)beatmapSet["video"]; } catch { scoreInfo.beatmapInfo.hasVideo = false; }
                try { scoreInfo.beatmapInfo.isNSFW = (bool)beatmapSet["nsfw"]; } catch { scoreInfo.beatmapInfo.isNSFW = false; }
                scoreInfos.Add(scoreInfo);
            }
            return scoreInfos;
        }

        // 获取用户在特定谱面上的成绩
        public static bool GetUserBeatmapScore(out ScoreInfo scoreInfo, long userId, long bid, List<string> mods, string mode = "osu")
        {
            checkModes(mode);
            CheckToken();
            scoreInfo = new();
            var url = OSU_API_V2 + $"beatmaps/{bid}/scores/users/{userId}?mode={mode}";
            foreach (var item in mods) { url += $"&mods[]={item}"; }
            Dictionary<string, string> headers = new();
            headers.Add("Authorization", $"Bearer {Token}");
            var result = Http.GetAsync(url, headers).Result;
            if (result.Status != HttpStatusCode.OK) return false;
            var body = JsonConvert.DeserializeObject<JObject>(result.Body);
            var score = body["score"];
            scoreInfo.mode = mode;
            scoreInfo.scoreType = "score"; // 这边的scoreType固定为score，对应指令score
            try { scoreInfo.userId = (long)score["user"]["id"]; } catch { scoreInfo.userId = 0; }
            try { scoreInfo.userName = score["user"]["username"].ToString(); } catch { scoreInfo.userName = ""; }
            try { scoreInfo.userAvatarUrl = score["user"]["avatar_url"].ToString(); } catch { scoreInfo.userAvatarUrl = ""; }
            try { scoreInfo.scoreId = (long)score["id"]; } catch { scoreInfo.scoreId = 0; }
            try { scoreInfo.score = (long)score["score"]; } catch { scoreInfo.score = 0; }
            try { scoreInfo.great = (int)score["statistics"]["count_300"]; } catch { scoreInfo.great = 0; } // n300, n100, n50, nmiss, nkatu, ngeki, combo;
            try { scoreInfo.ok = (int)score["statistics"]["count_100"]; } catch { scoreInfo.ok = 0; }
            try { scoreInfo.katu = (int)score["statistics"]["count_katu"]; } catch { scoreInfo.katu = 0; }
            try { scoreInfo.geki = (int)score["statistics"]["count_geki"]; } catch { scoreInfo.geki = 0; }
            try { scoreInfo.meh = (int)score["statistics"]["count_50"]; } catch { scoreInfo.meh = 0; }
            try { scoreInfo.miss = (int)score["statistics"]["count_miss"]; } catch { scoreInfo.miss = 0; }
            try { scoreInfo.combo = (int)score["max_combo"]; } catch { scoreInfo.combo = 0; }
            try { scoreInfo.pp = (float)score["pp"]; } catch { scoreInfo.pp = 0; }
            try { scoreInfo.acc = (float)score["accuracy"]; } catch { scoreInfo.acc = 0; }
            try { scoreInfo.hasReplay = (bool)score["replay"]; } catch { scoreInfo.hasReplay = false; }
            scoreInfo.rank = score["rank"].ToString();
            scoreInfo.achievedTime = DateTime.Parse(score["created_at"].ToString());
            scoreInfo.mods = new();
            var s_mods = JsonConvert.DeserializeObject<JArray>(score["mods"].ToString());
            foreach (var mod in s_mods) { scoreInfo.mods.Add(mod.ToString()); }
            scoreInfo.beatmapId = (long)score["beatmap"]["id"];
            scoreInfo.beatmapInfo = GetBeatmap(scoreInfo.beatmapId);
            return true;
        }

        // 通过osu uid获取用户信息
        public static UserInfo GetUser(long userId, string mode = "osu")
        {
            checkModes(mode);
            CheckToken();
            Dictionary<string, string> headers = new();
            headers.Add("Authorization", $"Bearer {Token}");
            var result = Http.GetAsync(OSU_API_V2 + $"users/{userId}/{mode}", headers).Result;
            if (result.Status != HttpStatusCode.OK || result.Body.Length < 20) { throw new Exception(result.Body); }
            var body = JsonConvert.DeserializeObject<JObject>(result.Body);
            UserInfo userInfo = new();
            try { userInfo.country = body["country_code"].ToString(); } catch { userInfo.country = "XX"; }
            userInfo.userName = body["username"].ToString();
            userInfo.userId = (long)body["id"];
            try { userInfo.avatarUrl = body["avatar_url"].ToString(); } catch { userInfo.avatarUrl = @"https://osu.ppy.sh/images/layout/avatar-guest.png"; }
            try { userInfo.coverUrl = body["cover"]["url"].ToString(); } catch { userInfo.coverUrl = ""; }
            var statistics = body["statistics"];
            try { userInfo.totalScore = (long)statistics["total_score"]; } catch { userInfo.totalScore = 0; }
            try { userInfo.totalHits = (long)statistics["total_hits"]; } catch { userInfo.totalHits = 0; }
            try { userInfo.playCount = (long)statistics["play_count"]; } catch { userInfo.playCount = 0; }
            try { userInfo.rankedScore = (long)statistics["ranked_score"]; } catch { userInfo.rankedScore = 0; }
            try { userInfo.countryRank = (long)statistics["country_rank"]; } catch { userInfo.countryRank = 0; }
            try { userInfo.globalRank = (long)statistics["global_rank"]; } catch { userInfo.globalRank = 0; }
            try { userInfo.playTime = (long)statistics["play_time"]; } catch { userInfo.playTime = 0; }
            try { userInfo.accuracy = (float)statistics["hit_accuracy"]; } catch { userInfo.accuracy = 0.00F; }
            try { userInfo.SSH = (int)statistics["grade_counts"]["ssh"]; } catch { userInfo.SSH = 0; }
            try { userInfo.SS = (int)statistics["grade_counts"]["ss"]; } catch { userInfo.SS = 0; }
            try { userInfo.SH = (int)statistics["grade_counts"]["sh"]; } catch { userInfo.SH = 0; }
            try { userInfo.S = (int)statistics["grade_counts"]["s"]; } catch { userInfo.S = 0; }
            try { userInfo.A = (int)statistics["grade_counts"]["a"]; } catch { userInfo.A = 0; }
            try { userInfo.level = (int)statistics["level"]["current"]; } catch { userInfo.level = 0; }
            try { userInfo.levelProgress = (int)statistics["level"]["progress"]; } catch { userInfo.levelProgress = 0; }
            userInfo.registedTimestamp = DateTime.Parse(body["join_date"].ToString());
            try { userInfo.pp = (float)statistics["pp"]; } catch { userInfo.pp = 0; }
            userInfo.mode = mode;
            userInfo.daysBefore = 0;
            return userInfo;
        }

        // 通过osu username获取用户信息
        public static UserInfo GetUser(string userName, string mode = "osu")
        {
            checkModes(mode);
            CheckToken();
            Dictionary<string, string> headers = new();
            headers.Add("Authorization", $"Bearer {Token}");
            var result = Http.GetAsync(OSU_API_V2 + $"users/{userName}/{mode}?key=username", headers).Result;
            if (result.Status != HttpStatusCode.OK || result.Body.Length < 20) { throw new Exception(result.Body); }
            var body = JsonConvert.DeserializeObject<JObject>(result.Body);
            UserInfo userInfo = new();
            try { userInfo.country = body["country_code"].ToString(); } catch { userInfo.country = "XX"; }
            userInfo.userName = body["username"].ToString();
            userInfo.userId = (long)body["id"];
            try { userInfo.avatarUrl = body["avatar_url"].ToString(); } catch { userInfo.avatarUrl = @"https://osu.ppy.sh/images/layout/avatar-guest.png"; }
            try { userInfo.coverUrl = body["cover"]["url"].ToString(); } catch { userInfo.coverUrl = ""; }
            var statistics = body["statistics"];
            try { userInfo.totalScore = (long)statistics["total_score"]; } catch { userInfo.totalScore = 0; }
            try { userInfo.totalHits = (long)statistics["total_hits"]; } catch { userInfo.totalHits = 0; }
            try { userInfo.playCount = (long)statistics["play_count"]; } catch { userInfo.playCount = 0; }
            try { userInfo.rankedScore = (long)statistics["ranked_score"]; } catch { userInfo.rankedScore = 0; }
            try { userInfo.countryRank = (long)statistics["country_rank"]; } catch { userInfo.countryRank = 0; }
            try { userInfo.globalRank = (long)statistics["global_rank"]; } catch { userInfo.globalRank = 0; }
            try { userInfo.playTime = (long)statistics["play_time"]; } catch { userInfo.playTime = 0; }
            try { userInfo.accuracy = (float)statistics["hit_accuracy"]; } catch { userInfo.accuracy = 0.00F; }
            try { userInfo.SSH = (int)statistics["grade_counts"]["ssh"]; } catch { userInfo.SSH = 0; }
            try { userInfo.SS = (int)statistics["grade_counts"]["ss"]; } catch { userInfo.SS = 0; }
            try { userInfo.SH = (int)statistics["grade_counts"]["sh"]; } catch { userInfo.SH = 0; }
            try { userInfo.S = (int)statistics["grade_counts"]["s"]; } catch { userInfo.S = 0; }
            try { userInfo.A = (int)statistics["grade_counts"]["a"]; } catch { userInfo.A = 0; }
            try { userInfo.level = (int)statistics["level"]["current"]; } catch { userInfo.level = 0; }
            try { userInfo.levelProgress = (int)statistics["level"]["progress"]; } catch { userInfo.levelProgress = 0; }
            userInfo.registedTimestamp = DateTime.Parse(body["join_date"].ToString());
            try { userInfo.pp = (float)statistics["pp"]; } catch { userInfo.pp = 0; }
            userInfo.mode = mode;
            userInfo.daysBefore = 0;
            return userInfo;
        }

        // 获取用户Elo信息
        public static JObject? GetUserEloInfo(long uid)
        {
            var r = Http.GetAsync($"http://api.osuwiki.cn:5005/api/users/elo/{uid}").Result;
            return JsonConvert.DeserializeObject<JObject>(r.Body);
        }

        // 获取用户最近的elo游戏记录
        public static int GetUserEloRecentPlay(long uid)
        {
            var r = Http.GetAsync($"http://api.osuwiki.cn:5005/api/users/recentPlay/{uid}").Result;
            var body = JsonConvert.DeserializeObject<JObject>(r.Body);
            try { return (int)body["match_id"]; }
            catch { return 0; }
        }

        // 获取比赛信息
        public static JObject? GetMatchInfo(long matchId)
        {
            var r = Http.GetAsync($"http://api.osuwiki.cn:5005/api/matches/{matchId}").Result;
            return JsonConvert.DeserializeObject<JObject>(r.Body);
        }

        // 获取pp+数据
        public static PPlusInfo GetUserPlusData(long uid)
        {
            var r = Http.GetAsync($"https://syrin.me/pp+/api/user/{uid}/").Result;
            PPlusInfo pplus = new();
            try
            {
                var data = JsonConvert.DeserializeObject<JObject>(r.Body);
                pplus.pp = (float)data["PerformanceTotal"];
                pplus.jump = (int)data["JumpAimTotal"];
                pplus.flow = (int)data["FlowAimTotal"];
                pplus.pre = (int)data["PrecisionTotal"];
                pplus.acc = (int)data["AccuracyTotal"];
                pplus.spd = (int)data["SpeedTotal"];
                pplus.sta = (int)data["StaminaTotal"];
            }
            catch { pplus.pp = 0; pplus.jump = 0; pplus.flow = 0; pplus.pre = 0; pplus.acc = 0; pplus.spd = 0; pplus.sta = 0; }
            return pplus;
        }
    }
}
