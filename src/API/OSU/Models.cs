#pragma warning disable CS8618 // 非null 字段未初始化
using Newtonsoft.Json;
using KanonBot.Serializer;
using Newtonsoft.Json.Linq;
using NullValueHandling = Newtonsoft.Json.NullValueHandling;

namespace KanonBot.API
{
    public partial class OSU
    {
        public class Models
        {

            public class PPlusData
            {
                public UserData User { get; set; }

                public UserPerformances[]? Performances { get; set; }


                public class UserData
                {
                    [JsonProperty("Rank")]
                    public int Rank { get; set; }

                    [JsonProperty("CountryRank")]
                    public int CountryRank { get; set; }

                    [JsonProperty("UserID")]
                    public long UserId { get; set; }

                    [JsonProperty("UserName")]
                    public string UserName { get; set; }

                    [JsonProperty("CountryCode")]
                    public string CountryCode { get; set; }

                    [JsonProperty("PerformanceTotal")]
                    public double PerformanceTotal { get; set; }

                    [JsonProperty("AimTotal")]
                    public double AimTotal { get; set; }

                    [JsonProperty("JumpAimTotal")]
                    public double JumpAimTotal { get; set; }

                    [JsonProperty("FlowAimTotal")]
                    public double FlowAimTotal { get; set; }

                    [JsonProperty("PrecisionTotal")]
                    public double PrecisionTotal { get; set; }

                    [JsonProperty("SpeedTotal")]
                    public double SpeedTotal { get; set; }

                    [JsonProperty("StaminaTotal")]
                    public double StaminaTotal { get; set; }

                    [JsonProperty("AccuracyTotal")]
                    public double AccuracyTotal { get; set; }

                    [JsonProperty("AccuracyPercentTotal")]
                    public double AccuracyPercentTotal { get; set; }

                    [JsonProperty("PlayCount")]
                    public int PlayCount { get; set; }

                    [JsonProperty("CountRankSS")]
                    public int CountRankSS { get; set; }

                    [JsonProperty("CountRankS")]
                    public int CountRankS { get; set; }
                }

                public class UserPerformances
                {
                    [JsonProperty("SetID")]
                    public long SetId { get; set; }

                    [JsonProperty("Artist")]
                    public string Artist { get; set; }

                    [JsonProperty("Title")]
                    public string Title { get; set; }

                    [JsonProperty("Version")]
                    public string Version { get; set; }

                    [JsonProperty("MaxCombo")]
                    public int MaxCombo { get; set; }

                    [JsonProperty("UserID")]
                    public long UserId { get; set; }

                    [JsonProperty("BeatmapID")]
                    public long BeatmapId { get; set; }

                    [JsonProperty("Total")]
                    public double TotalTotal { get; set; }

                    [JsonProperty("Aim")]
                    public double Aim { get; set; }

                    [JsonProperty("JumpAim")]
                    public double JumpAim { get; set; }

                    [JsonProperty("FlowAim")]
                    public double FlowAim { get; set; }

                    [JsonProperty("Precision")]
                    public double Precision { get; set; }

                    [JsonProperty("Speed")]
                    public double Speed { get; set; }

                    [JsonProperty("Stamina")]
                    public double Stamina { get; set; }

                    [JsonProperty("HigherSpeed")]
                    public double HigherSpeed { get; set; }

                    [JsonProperty("Accuracy")]
                    public double Accuracy { get; set; }

                    [JsonProperty("Count300")]
                    public int CountGreat { get; set; }

                    [JsonProperty("Count100")]
                    public int CountOk { get; set; }

                    [JsonProperty("Count50")]
                    public int CountMeh { get; set; }

                    [JsonProperty("Misses")]
                    public int CountMiss { get; set; }

                    [JsonProperty("AccuracyPercent")]
                    public double AccuracyPercent { get; set; }

                    [JsonProperty("Combo")]
                    public int Combo { get; set; }

                    [JsonProperty("EnabledMods")]
                    public int EnabledMods { get; set; }

                    [JsonProperty("Rank")]
                    public string Rank { get; set; }

                    [JsonProperty("Date")]
                    public DateTimeOffset Date { get; set; }
                }
            }

            public class BeatmapAttributes
            {
                // 不包含在json解析中，用作分辨mode
                public Enums.Mode Mode { get; set; }
                [JsonProperty(PropertyName = "max_combo")]

                // 共有部分
                public int MaxCombo { get; set; }
                [JsonProperty(PropertyName = "star_rating")]
                public double StarRating { get; set; }

                // osu, taiko, fruits包含
                [JsonProperty(PropertyName = "approach_rate")]
                public double ApproachRate { get; set; }

                // taiko, mania包含
                [JsonProperty(PropertyName = "great_hit_window")]
                public double GreatHitWindow { get; set; }

                // osu部分
                [JsonProperty(PropertyName = "aim_difficulty")]
                public double AimDifficulty { get; set; }
                [JsonProperty(PropertyName = "flashlight_difficulty")]
                public double FlashlightDifficulty { get; set; }
                [JsonProperty(PropertyName = "overall_difficulty")]
                public double OverallDifficulty { get; set; }
                [JsonProperty(PropertyName = "slider_factor")]
                public double SliderFactor { get; set; }
                [JsonProperty(PropertyName = "speed_difficulty")]
                public double SpeedDifficulty { get; set; }

                // taiko
                [JsonProperty(PropertyName = "stamina_difficulty")]
                public double StaminaDifficulty { get; set; }
                [JsonProperty(PropertyName = "rhythm_difficulty")]
                public double RhythmDifficulty { get; set; }
                [JsonProperty(PropertyName = "colour_difficulty")]
                public double ColourDifficulty { get; set; }

                // mania
                [JsonProperty(PropertyName = "score_multiplier")]
                public double ScoreMultiplier { get; set; }


            }

            public class Beatmap
            {
                [JsonProperty(PropertyName = "beatmapset_id")]
                public long BeatmapsetId { get; set; }

                [JsonProperty(PropertyName = "difficulty_rating")]
                public double DifficultyRating { get; set; }

                [JsonProperty(PropertyName = "id")]
                public long BeatmapId { get; set; }

                [JsonProperty(PropertyName = "mode")]
                [JsonConverter(typeof(JsonEnumConverter))]
                public Enums.Mode Mode { get; set; }

                [JsonProperty(PropertyName = "status")]
                [JsonConverter(typeof(JsonEnumConverter))]
                public Enums.Status Status { get; set; }

                [JsonProperty(PropertyName = "total_length")]
                public int TotalLength { get; set; }

                [JsonProperty(PropertyName = "user_id")]
                public long UserId { get; set; }

                [JsonProperty(PropertyName = "version")]
                public string Version { get; set; }
                // Accuracy = OD

                [JsonProperty(PropertyName = "accuracy")]
                public double Accuracy { get; set; }

                [JsonProperty(PropertyName = "ar")]
                public double AR { get; set; }

                [JsonProperty(PropertyName = "bpm", NullValueHandling = NullValueHandling.Ignore)]
                public double? BPM { get; set; }

                [JsonProperty(PropertyName = "convert")]
                public bool Convert { get; set; }

                [JsonProperty(PropertyName = "count_circles")]
                public long CountCircles { get; set; }

                [JsonProperty(PropertyName = "count_sliders")]
                public long CountSliders { get; set; }

                [JsonProperty(PropertyName = "count_spinners")]
                public long CountSpinners { get; set; }

                [JsonProperty(PropertyName = "cs")]
                public double CS { get; set; }

                [JsonProperty(PropertyName = "deleted_at", NullValueHandling = NullValueHandling.Ignore)]
                public DateTimeOffset? DeletedAt { get; set; }

                [JsonProperty(PropertyName = "drain")]
                public long HPDrain { get; set; }

                [JsonProperty(PropertyName = "hit_length")]
                public long HitLength { get; set; }

                [JsonProperty(PropertyName = "is_scoreable")]
                public bool IsScoreable { get; set; }

                [JsonProperty(PropertyName = "last_updated")]
                public DateTimeOffset LastUpdated { get; set; }

                [JsonProperty(PropertyName = "mode_int")]
                public long ModeInt { get; set; }

                [JsonProperty(PropertyName = "passcount")]
                public long Passcount { get; set; }

                [JsonProperty(PropertyName = "playcount")]
                public long Playcount { get; set; }

                [JsonProperty(PropertyName = "ranked")]
                public long Ranked { get; set; }

                [JsonProperty(PropertyName = "url")]
                public Uri Url { get; set; }

                [JsonProperty(PropertyName = "checksum", NullValueHandling = NullValueHandling.Ignore)]
                public string? Checksum { get; set; }

                [JsonProperty(PropertyName = "beatmapset", NullValueHandling = NullValueHandling.Ignore)]
                public Beatmapset? Beatmapset { get; set; }

                [JsonProperty(PropertyName = "failtimes")]
                public BeatmapFailtimes Failtimes { get; set; }

                [JsonProperty(PropertyName = "max_combo")]
                public long MaxCombo { get; set; }
            }

            public class Beatmapset
            {
                [JsonProperty(PropertyName = "artist")]
                public string Artist { get; set; }

                [JsonProperty(PropertyName = "artist_unicode")]
                public string ArtistUnicode { get; set; }

                [JsonProperty(PropertyName = "covers")]
                public BeatmapCovers Covers { get; set; }

                [JsonProperty(PropertyName = "creator")]
                public string Creator { get; set; }

                [JsonProperty(PropertyName = "favourite_count")]
                public long FavouriteCount { get; set; }

                [JsonProperty(PropertyName = "hype", NullValueHandling = NullValueHandling.Ignore)]
                public BeatmapHype? Hype { get; set; }

                [JsonProperty(PropertyName = "id")]
                public long Id { get; set; }

                [JsonProperty(PropertyName = "nsfw")]
                public bool IsNsfw { get; set; }

                [JsonProperty(PropertyName = "offset")]
                public long Offset { get; set; }

                [JsonProperty(PropertyName = "play_count")]
                public long PlayCount { get; set; }

                [JsonProperty(PropertyName = "preview_url")]
                public string PreviewUrl { get; set; }

                [JsonProperty(PropertyName = "source")]
                public string Source { get; set; }

                [JsonProperty(PropertyName = "spotlight")]
                public bool Spotlight { get; set; }

                [JsonProperty(PropertyName = "status")]
                public string Status { get; set; }

                [JsonProperty(PropertyName = "title")]
                public string Title { get; set; }

                [JsonProperty(PropertyName = "title_unicode")]
                public string TitleUnicode { get; set; }

                [JsonProperty(PropertyName = "user_id")]
                public long UserId { get; set; }

                [JsonProperty(PropertyName = "video")]
                public bool Video { get; set; }

                [JsonProperty(PropertyName = "availability")]
                public BeatmapAvailability Availability { get; set; }

                [JsonProperty(PropertyName = "bpm")]
                public long BPM { get; set; }

                [JsonProperty(PropertyName = "can_be_hyped")]
                public bool CanBeHyped { get; set; }

                [JsonProperty(PropertyName = "discussion_enabled")]
                public bool DiscussionEnabled { get; set; }

                [JsonProperty(PropertyName = "discussion_locked")]
                public bool DiscussionLocked { get; set; }

                [JsonProperty(PropertyName = "is_scoreable")]
                public bool IsScoreable { get; set; }

                [JsonProperty(PropertyName = "last_updated")]
                public DateTimeOffset LastUpdated { get; set; }

                [JsonProperty(PropertyName = "legacy_thread_url", NullValueHandling = NullValueHandling.Ignore)]
                public Uri? LegacyThreadUrl { get; set; }

                [JsonProperty(PropertyName = "nominations_summary")]
                public NominationsSummary NominationsSummary { get; set; }

                [JsonProperty(PropertyName = "ranked")]
                public long Ranked { get; set; }

                [JsonProperty(PropertyName = "ranked_date", NullValueHandling = NullValueHandling.Ignore)]
                public DateTimeOffset? RankedDate { get; set; }

                [JsonProperty(PropertyName = "storyboard")]
                public bool Storyboard { get; set; }

                [JsonProperty(PropertyName = "submitted_date", NullValueHandling = NullValueHandling.Ignore)]
                public DateTimeOffset? SubmittedDate { get; set; }

                [JsonProperty(PropertyName = "tags")]
                public string Tags { get; set; }

                [JsonProperty(PropertyName = "ratings")]
                public long[] Ratings { get; set; }

                // [JsonProperty(PropertyName = "track_id")]
                // public JObject TrackId { get; set; }
            }
            public class BeatmapCovers
            {
                [JsonProperty(PropertyName = "cover")]
                public string Cover { get; set; }
                [JsonProperty(PropertyName = "cover@2x")]
                public string Cover2x { get; set; }
                [JsonProperty(PropertyName = "card")]
                public string Card { get; set; }
                [JsonProperty(PropertyName = "card@2x")]
                public string Card2x { get; set; }
                [JsonProperty(PropertyName = "list")]
                public string List { get; set; }
                [JsonProperty(PropertyName = "list@2x")]
                public string List2x { get; set; }
                [JsonProperty(PropertyName = "slimcover")]
                public string SlimCover { get; set; }
                [JsonProperty(PropertyName = "slimcover@2x")]
                public string SlimCover2x { get; set; }
            }

            public class BeatmapAvailability
            {
                [JsonProperty(PropertyName = "download_disabled")]
                public bool DownloadDisabled { get; set; }

                [JsonProperty(PropertyName = "more_information", NullValueHandling = NullValueHandling.Ignore)]
                public string? MoreInformation { get; set; }
            }
            public class BeatmapHype
            {
                [JsonProperty(PropertyName = "current")]
                public int DownloadDisabled { get; set; }

                [JsonProperty(PropertyName = "required")]
                public int MoreInformation { get; set; }
            }

            public class NominationsSummary
            {
                [JsonProperty(PropertyName = "current")]
                public int Current { get; set; }

                [JsonProperty(PropertyName = "required")]
                public int NominationsSummaryRequired { get; set; }
            }

            public class BeatmapFailtimes
            {
                [JsonProperty(PropertyName = "fail", NullValueHandling = NullValueHandling.Ignore)]
                public int[]? Fail { get; set; }

                [JsonProperty(PropertyName = "exit", NullValueHandling = NullValueHandling.Ignore)]
                public int[]? Exit { get; set; }
            }
            public class UserV1
            {
                [JsonProperty("user_id")]
                [JsonConverter(typeof(ParseStringConverter))]
                public long UserId { get; set; }

                [JsonProperty("username")]
                public string Username { get; set; }

                [JsonProperty("join_date")]
                public DateTimeOffset JoinDate { get; set; }

                [JsonProperty("count300")]
                [JsonConverter(typeof(ParseStringConverter))]
                public long Count300 { get; set; }

                [JsonProperty("count100")]
                [JsonConverter(typeof(ParseStringConverter))]
                public long Count100 { get; set; }

                [JsonProperty("count50")]
                [JsonConverter(typeof(ParseStringConverter))]
                public long Count50 { get; set; }

                [JsonProperty("playcount")]
                [JsonConverter(typeof(ParseStringConverter))]
                public long Playcount { get; set; }

                [JsonProperty("ranked_score")]
                public string RankedScore { get; set; }

                [JsonProperty("total_score")]
                public string TotalScore { get; set; }

                [JsonProperty("pp_rank")]
                [JsonConverter(typeof(ParseStringConverter))]
                public long PpRank { get; set; }

                [JsonProperty("level")]
                public string Level { get; set; }

                [JsonProperty("pp_raw")]
                public string PpRaw { get; set; }

                [JsonProperty("accuracy")]
                public string Accuracy { get; set; }

                [JsonProperty("count_rank_ss")]
                [JsonConverter(typeof(ParseStringConverter))]
                public long CountRankSs { get; set; }

                [JsonProperty("count_rank_ssh")]
                [JsonConverter(typeof(ParseStringConverter))]
                public long CountRankSsh { get; set; }

                [JsonProperty("count_rank_s")]
                [JsonConverter(typeof(ParseStringConverter))]
                public long CountRankS { get; set; }

                [JsonProperty("count_rank_sh")]
                [JsonConverter(typeof(ParseStringConverter))]
                public long CountRankSh { get; set; }

                [JsonProperty("count_rank_a")]
                [JsonConverter(typeof(ParseStringConverter))]
                public long CountRankA { get; set; }

                [JsonProperty("country")]
                public string Country { get; set; }

                [JsonProperty("total_seconds_played")]
                [JsonConverter(typeof(ParseStringConverter))]
                public long TotalSecondsPlayed { get; set; }

                [JsonProperty("pp_country_rank")]
                [JsonConverter(typeof(ParseStringConverter))]
                public long PpCountryRank { get; set; }

                [JsonProperty("events")]
                public object[] Events { get; set; }

                internal class ParseStringConverter : JsonConverter
                {
                    public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

                    public override object? ReadJson(JsonReader reader, Type t, object? existingValue, JsonSerializer serializer)
                    {
                        if (reader.TokenType == JsonToken.Null) return null;
                        var value = serializer.Deserialize<string>(reader);
                        if (Int64.TryParse(value, out long l))
                        {
                            return l;
                        }
                        throw new Exception("Cannot unmarshal type long");
                    }

                    public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer)
                    {
                        if (untypedValue == null)
                        {
                            serializer.Serialize(writer, null);
                            return;
                        }
                        var value = (long)untypedValue;
                        serializer.Serialize(writer, value.ToString());
                        return;
                    }

                    public static readonly ParseStringConverter Singleton = new();
                }
            }

            public class User
            {
                [JsonProperty("avatar_url")]
                public Uri AvatarUrl { get; set; }

                [JsonProperty("country_code")]
                public string CountryCode { get; set; }

                [JsonProperty("default_group")]
                public string DefaultGroup { get; set; }

                [JsonProperty("id")]
                public long Id { get; set; }

                [JsonProperty("is_active")]
                public bool IsActive { get; set; }

                [JsonProperty("is_bot")]
                public bool IsBot { get; set; }

                [JsonProperty("is_deleted")]
                public bool IsDeleted { get; set; }

                [JsonProperty("is_online")]
                public bool IsOnline { get; set; }

                [JsonProperty("is_supporter")]
                public bool IsSupporter { get; set; }

                [JsonProperty("last_visit", NullValueHandling = NullValueHandling.Ignore)]
                public DateTimeOffset? LastVisit { get; set; }

                [JsonProperty("pm_friends_only")]
                public bool PmFriendsOnly { get; set; }

                [JsonProperty("profile_colour", NullValueHandling = NullValueHandling.Ignore)]
                public string? ProfileColour { get; set; }

                [JsonProperty("username")]
                public string Username { get; set; }

                [JsonProperty("cover_url")]
                public Uri CoverUrl { get; set; }

                [JsonProperty("discord", NullValueHandling = NullValueHandling.Ignore)]
                public string? Discord { get; set; }

                [JsonProperty("has_supported")]
                public bool HasSupported { get; set; }

                [JsonProperty("interests", NullValueHandling = NullValueHandling.Ignore)]
                public string? Interests { get; set; }

                [JsonProperty("join_date")]
                public DateTimeOffset JoinDate { get; set; }

                [JsonProperty("kudosu")]
                public JObject Kudosu { get; set; }

                [JsonProperty("location", NullValueHandling = NullValueHandling.Ignore)]
                public string? Location { get; set; }

                [JsonProperty("max_blocks")]
                public long MaxBlocks { get; set; }

                [JsonProperty("max_friends")]
                public long MaxFriends { get; set; }

                [JsonProperty("occupation", NullValueHandling = NullValueHandling.Ignore)]
                public string? Occupation { get; set; }

                /// <summary>
                /// 这个是指的官网用户的默认游玩模式，并非查询的成绩模式！！！
                /// 使用时请把此值强制赋值成查询的模式
                /// </summary>
                [JsonProperty("playmode")]
                [JsonConverter(typeof(JsonEnumConverter))]
                public Enums.Mode PlayMode { get; set; }

                [JsonProperty("playstyle")]
                public string[] Playstyle { get; set; }

                [JsonProperty("post_count")]
                public long PostCount { get; set; }

                [JsonProperty("profile_order")]
                public string[] ProfileOrder { get; set; }

                [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
                public string? Title { get; set; }

                [JsonProperty("title_url", NullValueHandling = NullValueHandling.Ignore)]
                public string? TitleUrl { get; set; }

                [JsonProperty("twitter", NullValueHandling = NullValueHandling.Ignore)]
                public string? Twitter { get; set; }

                [JsonProperty("website", NullValueHandling = NullValueHandling.Ignore)]
                public string? Website { get; set; }

                [JsonProperty("country")]
                public Country Country { get; set; }

                [JsonProperty("cover")]
                public UserCover Cover { get; set; }

                [JsonProperty("account_history")]
                public UserAccountHistory[] AccountHistory { get; set; }

                [JsonProperty("active_tournament_banner", NullValueHandling = NullValueHandling.Ignore)]
                public JObject? ActiveTournamentBanner { get; set; }

                [JsonProperty("badges")]
                public UserBadge[] Badges { get; set; }

                [JsonProperty("beatmap_playcounts_count", NullValueHandling = NullValueHandling.Ignore)]
                public long BeatmapPlaycountsCount { get; set; }

                [JsonProperty("comments_count")]
                public long CommentsCount { get; set; }

                [JsonProperty("favourite_beatmapset_count")]
                public long FavouriteBeatmapsetCount { get; set; }

                [JsonProperty("follower_count")]
                public long FollowerCount { get; set; }

                [JsonProperty("graveyard_beatmapset_count")]
                public long GraveyardBeatmapsetCount { get; set; }

                [JsonProperty("groups")]
                public JArray Groups { get; set; }

                [JsonProperty("guest_beatmapset_count")]
                public long GuestBeatmapsetCount { get; set; }

                [JsonProperty("loved_beatmapset_count")]
                public long LovedBeatmapsetCount { get; set; }

                [JsonProperty("mapping_follower_count")]
                public long MappingFollowerCount { get; set; }

                [JsonProperty("pending_beatmapset_count")]
                public long PendingBeatmapsetCount { get; set; }

                [JsonProperty("previous_usernames")]
                public string[] PreviousUsernames { get; set; }

                [JsonProperty("ranked_beatmapset_count")]
                public long RankedBeatmapsetCount { get; set; }

                [JsonProperty("scores_best_count")]
                public long ScoresBestCount { get; set; }

                [JsonProperty("scores_first_count")]
                public long ScoresFirstCount { get; set; }

                [JsonProperty("scores_pinned_count")]
                public long ScoresPinnedCount { get; set; }

                [JsonProperty("scores_recent_count")]
                public long ScoresRecentCount { get; set; }
                [JsonProperty("is_restricted", NullValueHandling = NullValueHandling.Ignore)]
                public bool? IsRestricted { get; set; }

                [JsonProperty("statistics")]
                public UserStatistics Statistics { get; set; }

                [JsonProperty("support_level")]
                public long SupportLevel { get; set; }

                [JsonProperty("ranked_and_approved_beatmapset_count")]
                public long RankedAndApprovedBeatmapsetCount { get; set; }

                [JsonProperty("unranked_beatmapset_count")]
                public long UnrankedBeatmapsetCount { get; set; }

                // 搞不懂为啥这里ppy要给两个rankhistory
                [JsonProperty("rankHistory")]
                public RankHistory? RankHistory { get; set; }

                // [JsonProperty("rank_history")]
                // public RankHistory UserRankHistory { get; set; }

                // [JsonProperty("user_achievements")]
                // public UserAchievement[] UserAchievements { get; set; }

                // [JsonProperty("page")]
                // public UserPage Page { get; set; }

                // [JsonProperty("monthly_playcounts")]
                // public Count[] MonthlyPlaycounts { get; set; }

                // [JsonProperty("replays_watched_counts")]
                // public Count[] ReplaysWatchedCounts { get; set; }
            }
            public class Country
            {
                [JsonProperty("code")]
                public string Code { get; set; }

                [JsonProperty("name")]
                public string Name { get; set; }
            }
            public class UserCover
            {
                [JsonProperty("custom_url")]
                public Uri CustomUrl { get; set; }

                [JsonProperty("url")]
                public Uri Url { get; set; }

                [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
                public JValue? Id { get; set; }
            }
            public class Count
            {
                [JsonProperty("start_date")]
                public DateTimeOffset StartDate { get; set; }

                [JsonProperty("count")]
                public long CountCount { get; set; }
            }

            public class UserPage
            {
                [JsonProperty("html")]
                public string Html { get; set; }

                [JsonProperty("raw")]
                public string Raw { get; set; }
            }

            public class RankHistory
            {
                [JsonProperty("mode")]
                [JsonConverter(typeof(JsonEnumConverter))]
                public Enums.Mode Mode { get; set; }

                [JsonProperty("data")]
                public long[] Data { get; set; }
            }

            public class UserStatistics
            {
                [JsonProperty("level")]
                public UserLevel Level { get; set; }

                [JsonProperty("global_rank", NullValueHandling = NullValueHandling.Ignore)]
                public long GlobalRank { get; set; }

                [JsonProperty("pp")]
                public double PP { get; set; }

                [JsonProperty("ranked_score")]
                public long RankedScore { get; set; }

                [JsonProperty("hit_accuracy")]
                public double HitAccuracy { get; set; }

                [JsonProperty("play_count")]
                public long PlayCount { get; set; }

                [JsonProperty("play_time")]
                public long PlayTime { get; set; }

                [JsonProperty("total_score")]
                public long TotalScore { get; set; }

                [JsonProperty("total_hits")]
                public long TotalHits { get; set; }

                [JsonProperty("maximum_combo")]
                public long MaximumCombo { get; set; }

                [JsonProperty("replays_watched_by_others")]
                public long ReplaysWatchedByOthers { get; set; }

                [JsonProperty("is_ranked")]
                public bool IsRanked { get; set; }

                [JsonProperty("grade_counts")]
                public UserGradeCounts GradeCounts { get; set; }

                [JsonProperty("country_rank", NullValueHandling = NullValueHandling.Ignore)]
                public long CountryRank { get; set; }

                [JsonProperty("rank")]
                public UserRank Rank { get; set; }
            }

            public class UserGradeCounts
            {
                [JsonProperty("ss")]
                public int SS { get; set; }

                [JsonProperty("ssh")]
                public int SSH { get; set; }

                [JsonProperty("s")]
                public int S { get; set; }

                [JsonProperty("sh")]
                public int SH { get; set; }

                [JsonProperty("a")]
                public int A { get; set; }
            }

            public class UserLevel
            {
                [JsonProperty("current")]
                public int Current { get; set; }

                [JsonProperty("progress")]
                public int Progress { get; set; }
            }

            public class UserRank
            {
                [JsonProperty("country", NullValueHandling = NullValueHandling.Ignore)]
                public int Country { get; set; }
            }

            public class UserAchievement
            {
                [JsonProperty("achieved_at")]
                public DateTimeOffset AchievedAt { get; set; }

                [JsonProperty("achievement_id")]
                public long AchievementId { get; set; }
            }
            public class UserBadge
            {
                [JsonProperty("awarded_at")]
                public DateTimeOffset AwardedAt { get; set; }

                [JsonProperty("description")]
                public string Description { get; set; }

                [JsonProperty("image_url")]
                public Uri ImageUrl { get; set; }

                [JsonProperty("url")]
                public Uri Url { get; set; }
            }
            public class UserAccountHistory
            {
                [JsonProperty("timestamp")]
                public DateTimeOffset Time { get; set; }

                [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
                public string? Description { get; set; }

                [JsonProperty("type")]
                public string Type { get; set; }

                [JsonProperty("id")]
                public long Id { get; set; }

                [JsonProperty("length")]
                public int Length { get; set; }
            }

            public class BeatmapScore   // 只是比score多了个当前bid的排名
            {
                [JsonProperty("position")]
                public int Position { get; set; }
                [JsonProperty("score")]
                public Score Score { get; set; }
            }
            public class Score
            {
                [JsonProperty("accuracy")]
                public double Accuracy { get; set; }

                [JsonProperty("best_id", NullValueHandling = NullValueHandling.Ignore)]
                public long BestId { get; set; }

                [JsonProperty("created_at")]
                public DateTimeOffset CreatedAt { get; set; }

                [JsonProperty("id")]
                public long Id { get; set; }

                [JsonProperty("max_combo")]
                public uint MaxCombo { get; set; }

                [JsonProperty("mode")]
                [JsonConverter(typeof(JsonEnumConverter))]
                public Enums.Mode Mode { get; set; }

                [JsonProperty("mode_int")]
                public int ModeInt { get; set; }

                [JsonProperty("mods")]
                public string[] Mods { get; set; }

                [JsonProperty("passed")]
                public bool Passed { get; set; }

                [JsonProperty("perfect")]
                public bool Perfect { get; set; }

                [JsonProperty("pp", NullValueHandling = NullValueHandling.Ignore)]
                public double PP { get; set; }

                [JsonProperty("rank")]
                public string Rank { get; set; }

                [JsonProperty("replay")]
                public bool Replay { get; set; }

                [JsonProperty("score")]
                public int Scores { get; set; }

                [JsonProperty("statistics")]
                public ScoreStatistics Statistics { get; set; }

                [JsonProperty("user_id")]
                public long UserId { get; set; }

                [JsonProperty("beatmap", NullValueHandling = NullValueHandling.Ignore)]
                public Beatmap? Beatmap { get; set; }

                [JsonProperty("beatmapset", NullValueHandling = NullValueHandling.Ignore)]
                public Beatmapset? Beatmapset { get; set; }

                [JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
                public User? User { get; set; }

                [JsonProperty("weight", NullValueHandling = NullValueHandling.Ignore)]
                public ScoreWeight? Weight { get; set; }
            }

            public class ScoreStatistics
            {
                [JsonProperty("count_100")]
                public uint CountOk { get; set; }

                [JsonProperty("count_300")]
                public uint CountGreat { get; set; }

                [JsonProperty("count_50")]
                public uint CountMeh { get; set; }

                [JsonProperty("count_geki")]
                public uint CountGeki { get; set; }

                [JsonProperty("count_katu")]
                public uint CountKatu { get; set; }

                [JsonProperty("count_miss")]
                public uint CountMiss { get; set; }
            }

            public class ScoreWeight
            {
                [JsonProperty("percentage")]
                public double Percentage { get; set; }

                [JsonProperty("pp")]
                public double PP { get; set; }
            }

            public class PPData
            {
                [JsonProperty("Mods")]
                public List<string> Mods { get; set; }

                [JsonProperty("Star")]
                public float Star { get; set; }

                [JsonProperty("CS")]
                public float CS { get; set; }

                [JsonProperty("HP")]
                public int HP { get; set; }

                [JsonProperty("Aim")]
                public float Aim { get; set; }

                [JsonProperty("Speed")]
                public float Speed { get; set; }

                [JsonProperty("MaxCombo")]
                public int MaxCombo { get; set; }

                [JsonProperty("AR")]
                public float AR { get; set; }

                [JsonProperty("OD")]
                public float OD { get; set; }

                [JsonProperty("PPInfo")]
                public PPData_Info PPInfo { get; set; }
            }

            public class PPData_Info
            {
                [JsonProperty("Total")]
                public double Total { get; set; }
                [JsonProperty("aim")]
                public double aim { get; set; }
                [JsonProperty("speed")]
                public double speed { get; set; }
                [JsonProperty("accuracy")]
                public double accuracy { get; set; }
                [JsonProperty("flashlight")]
                public double flashlight { get; set; }
                [JsonProperty("effective_miss_count")]
                public double effective_miss_count { get; set; }
                [JsonProperty("pp")]
                public double pp { get; set; }
            }
        }
    }
}
