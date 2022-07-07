#pragma warning disable CS8602 // 解引用可能出现空引用。
#pragma warning disable IDE0044 // 添加只读修饰符
#pragma warning disable IDE1006 // 命名样式
using SqlSugar;

namespace KanonBot.Database;

public class Model
{

    [SugarTable("osu_seasonalpass_2022_s2")]
    public class OSUSeasonalPass
    {
        public long uid { get; set; }
        public string? mode { get; set; }
        public long tth { get; set; }
        public long inittth { get; set; }
    }

    [SugarTable("users")]
    public class Users
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long uid { get; set; }
        [SugarColumn(IsPrimaryKey = true)]
        public string? email { get; set; }
        public string? passwd { get; set; }
        public long qq_id { get; set; }
        public string? qq_guild_uid { get; set; }
        public string? khl_uid { get; set; }
        public string? discord_uid { get; set; }
        public string? permissions { get; set; }
        public string? last_login_ip { get; set; }
        public string? last_login_time { get; set; }
        public int status { get; set; }
        public string? displayed_badge_ids { get; set; }
        public string? owned_badge_ids { get; set; }
    }

    [SugarTable("users_osu")]
    public class Users_osu
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public long uid { get; set; }
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public long osu_uid { get; set; }
        public string? osu_mode { get; set; }
        public int customBannerStatus { get; set; }
        public int customInfoEngineVer { get; set; } // 0=legacy 1=current
        public string? customInfov2_cmd { get; set; }
    }

    [SugarTable("osu_archived_record")]
    public class OsuArchivedRec
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public int uid { get; set; }
        public int play_count { get; set; }
        public long ranked_score { get; set; }
        public long total_score { get; set; }
        public long total_hit { get; set; }
        public int level { get; set; }
        public int level_percent { get; set; }
        public float performance_point { get; set; }
        public float accuracy { get; set; }
        public int count_SSH { get; set; }
        public int count_SS { get; set; }
        public int count_SH { get; set; }
        public int count_S { get; set; }
        public int count_A { get; set; }
        public int playtime { get; set; }
        public int country_rank { get; set; }
        public int global_rank { get; set; }
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public string? gamemode { get; set; }
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public DateTimeOffset lastupdate { get; set; }
    }

    [SugarTable("osu_performancepointplus_record")]
    public class OsuPPlus
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public long uid { get; set; }
        public float pp { get; set; }
        public int jump { get; set; }
        public int flow { get; set; }
        public int pre { get; set; }
        public int acc { get; set; }
        public int spd { get; set; }
        public int sta { get; set; }
    }

    [SugarTable("badge_list")]
    public class BadgeList
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int id { get; set; }
        public string? name { get; set; }
        public string? name_chinese { get; set; }
        public string? description { get; set; }
    }
    [SugarTable("mail_verify")]
    public class MailVerify
    {
        public string? mailAddr { get; set; }
        public string? verify { get; set; }
        public string? gen_time { get; set; }
    }
}