#pragma warning disable CS8604 // 引用类型参数可能为 null。
#pragma warning disable CS8602 // 解引用可能出现空引用。
#pragma warning disable CS8600 // 将 null 字面量或可能为 null 的值转换为非 null 类型。
#pragma warning disable CS8618 // 非null 字段未初始化
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;
using Serilog;
using Flurl;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Converters;
using System.ComponentModel;


namespace KanonBot.API
{
    public partial class OSU
    {
        public class BeapmapInfo
        {
            public string mode { get; set; }
            public string beatmapStatus { get; set; }

            public long beatmapId { get; set; }
            public long beatmapsetId { get; set; }
            public long creatorId { get; set; }
            public long hitLength { get; set; }     // 第一个note到最后一个note的时长
            public long totalLength { get; set; }
            public long totalPlaycount { get; set; }
            public long playCount { get; set; }
            public long passCount { get; set; }
            public int favouriteCount { get; set; }
            public int circleCount { get; set; }
            public int sliderCount { get; set; }
            public int spinnerCount { get; set; }
            public int maxCombo { get; set; }

            public float BPM { get; set; }
            public float circleSize { get; set; }
            public float approachRate { get; set; }
            // Accuracy == OverallDifficulty(OD)
            public float accuracy { get; set; }
            public float HPDrainRate { get; set; }
            public float difficultyRating { get; set; }
            public List<string> tags { get; set; }
            public bool hasVideo { get; set; }
            public bool isNSFW { get; set; }
            public bool canDownload { get; set; }
            public string previewUrl { get; set; }
            public string artist { get; set; }
            public string artistUnicode { get; set; }
            public string title { get; set; }
            public string titleUnicode { get; set; }
            public string creator { get; set; }
            public string source { get; set; }
            public string version { get; set; }
            public string fileChecksum { get; set; }
            public string backgroundImgUrl { get; set; }
            public string musicUrl { get; set; }
            public DateTimeOffset submitTime { get; set; }
            public DateTimeOffset lastUpdateTime { get; set; }
            public DateTimeOffset rankedTime { get; set; }
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
            public DateTimeOffset registedTimestamp;
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
            public DateTimeOffset achievedTime;
            public BeapmapInfo beatmapInfo;
        }

        public static readonly string[] Modes = { "osu", "taiko", "fruits", "mania" };
        public static void checkModes(string mode)
        {
            foreach (string m in Modes)
            {
                if (mode == m) return;
            }
            throw new Exception("OSU 模式不正确");
        }


        // 获取特定谱面信息
        async public static Task<BeapmapInfo> GetBeatmapLegacy(long bid)
        {
            var beatmap = await http()
                .AppendPathSegment("beatmaps")
                .AppendPathSegment(bid)
                .GetJsonAsync<JObject>();

            if (beatmap.GetValue("error") != null)
                throw new KanonError(beatmap.ToString());


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
        async public static Task<List<ScoreInfo>> GetUserScoresLegacy(long userId, string scoreType = "recent", string mode = "osu", int limit = 1, int offset = 0, bool includeFails = true)
        {
            checkModes(mode);
            var body = await http()
                .AppendPathSegment("users")
                .AppendPathSegment(userId)
                .AppendPathSegment("scores")
                .AppendPathSegment(scoreType)
                .SetQueryParam("include_fails", includeFails ? 1 : 0)
                .SetQueryParam("limit", limit)
                .SetQueryParam("offset", offset)
                .SetQueryParam("mode", mode)
                .GetJsonAsync<JArray>();

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
                scoreInfo.achievedTime = DateTimeOffset.Parse(score["created_at"].ToString());
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
        async public static Task<ScoreInfo?> GetUserBeatmapScoreLegacy(long userId, long bid, List<string> mods, string mode = "osu")
        {
            checkModes(mode);

            var req = http()
                .AppendPathSegment("beatmaps")
                .AppendPathSegment(bid)
                .AppendPathSegment("scores")
                .AppendPathSegment("users")
                .AppendPathSegment(userId)
                .SetQueryParam("mode", mode);

            foreach (var mod in mods) { req.SetQueryParam("mods[]", mod); }
            JObject body;
            var res = await req.GetAsync();
            if (res.StatusCode == 404)
                return null;
            else
                body = await res.GetJsonAsync<JObject>();


            ScoreInfo scoreInfo = new();
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
            scoreInfo.achievedTime = DateTimeOffset.Parse(score["created_at"].ToString());
            scoreInfo.mods = new();
            var s_mods = JsonConvert.DeserializeObject<JArray>(score["mods"].ToString());
            foreach (var mod in s_mods) { scoreInfo.mods.Add(mod.ToString()); }
            scoreInfo.beatmapId = (long)score["beatmap"]["id"];
            scoreInfo.beatmapInfo = await GetBeatmapLegacy(scoreInfo.beatmapId);
            return scoreInfo;
        }

        // 通过osu uid获取用户信息
        async public static Task<UserInfo> GetUserLegacy(long userId, string mode = "osu")
        {
            checkModes(mode);

            var body = await http()
                .AppendPathSegment("users")
                .AppendPathSegment(userId)
                .AppendPathSegment(mode)
                .GetJsonAsync<JObject>();

            if (body.GetValue("error") != null)
                throw new KanonError(body.ToString());

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
            userInfo.registedTimestamp = DateTimeOffset.Parse(body["join_date"].ToString());
            try { userInfo.pp = (float)statistics["pp"]; } catch { userInfo.pp = 0; }
            userInfo.mode = mode;
            userInfo.daysBefore = 0;
            return userInfo;
        }

        // 通过osu username获取用户信息
        async public static Task<UserInfo> GetUserLegacy(string userName, string mode = "osu")
        {
            checkModes(mode);

            var body = await http()
                .AppendPathSegment("users")
                .AppendPathSegment(userName)
                .AppendPathSegment(mode)
                .SetQueryParam("key", "username")
                .GetJsonAsync<JObject>();

            if (body.GetValue("error") != null)
                throw new KanonError(body.ToString());

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
            userInfo.registedTimestamp = DateTimeOffset.Parse(body["join_date"].ToString());
            try { userInfo.pp = (float)statistics["pp"]; } catch { userInfo.pp = 0; }
            userInfo.mode = mode;
            userInfo.daysBefore = 0;
            return userInfo;
        }
    }
}
