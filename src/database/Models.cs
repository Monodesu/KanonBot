#pragma warning disable IDE0044 // 添加只读修饰符
#pragma warning disable IDE1006 // 命名样式
using System;
using LinqToDB;
using LinqToDB.Configuration;
using LinqToDB.Mapping;

namespace KanonBot.Database;

public class Model
{
    public class DB : LinqToDB.Data.DataConnection
    {
        public DB(LinqToDBConnectionOptions options) : base(options) { }

        public ITable<OsuStandardBeatmapTechData> OsuStandardBeatmapTechData =>
            this.GetTable<OsuStandardBeatmapTechData>();
        public ITable<OSUSeasonalPass> OSUSeasonalPass => this.GetTable<OSUSeasonalPass>();
        public ITable<User> User => this.GetTable<User>();
        public ITable<UserOSU> UserOSU => this.GetTable<UserOSU>();
        public ITable<OsuArchivedRec> OsuArchivedRec => this.GetTable<OsuArchivedRec>();
        public ITable<OsuPPlus> OsuPPlus => this.GetTable<OsuPPlus>();
        public ITable<BadgeList> BadgeList => this.GetTable<BadgeList>();
        public ITable<MailVerify> MailVerify => this.GetTable<MailVerify>();

        // ... other tables ...
    }

    [Table("osu_standard_beatmap_tech_data")]
    public class OsuStandardBeatmapTechData
    {
        [PrimaryKey]
        public long bid { get; set; }

        [Column]
        public int total { get; set; }

        [Column]
        public int aim { get; set; }

        [Column]
        public int speed { get; set; }

        [Column]
        public int acc { get; set; }

        [Column]
        public string? mod { get; set; }
    }

    [Table("osu_seasonalpass_2022_s4")]
    public class OSUSeasonalPass
    {
        [PrimaryKey]
        public long uid { get; set; }

        [Column]
        public string? mode { get; set; }

        [Column]
        public long tth { get; set; }

        [Column]
        public long inittth { get; set; }
    }

    [Table("users")]
    public class User
    {
        [PrimaryKey, Identity]
        public long uid { get; set; }

        [PrimaryKey]
        public string? email { get; set; }

        [Column]
        public string? passwd { get; set; }

        [Column]
        public long qq_id { get; set; }

        [Column]
        public string? qq_guild_uid { get; set; }

        [Column]
        public string? kook_uid { get; set; }

        [Column]
        public string? discord_uid { get; set; }

        [Column]
        public string? permissions { get; set; }

        [Column]
        public string? last_login_ip { get; set; }

        [Column]
        public string? last_login_time { get; set; }

        [Column]
        public int status { get; set; }

        [Column]
        public string? displayed_badge_ids { get; set; }

        [Column]
        public string? owned_badge_ids { get; set; }
    }

    [Table("users_osu")]
    public class UserOSU
    {
        [PrimaryKey]
        public long uid { get; set; }

        [PrimaryKey]
        public long osu_uid { get; set; }

        [Column]
        public string? osu_mode { get; set; }

        [Column]
        public int customInfoEngineVer { get; set; } // 1=v1 2=v2

        [Column]
        public string? InfoPanelV2_CustomMode { get; set; }

        [Column]
        public int InfoPanelV2_Mode { get; set; }
    }

    [Table("osu_archived_record")]
    public class OsuArchivedRec
    {
        [PrimaryKey]
        public int uid { get; set; }

        [Column]
        public int play_count { get; set; }

        [Column]
        public long ranked_score { get; set; }

        [Column]
        public long total_score { get; set; }

        [Column]
        public long total_hit { get; set; }

        [Column]
        public int level { get; set; }

        [Column]
        public int level_percent { get; set; }

        [Column]
        public double performance_point { get; set; }

        [Column]
        public double accuracy { get; set; }

        [Column]
        public int count_SSH { get; set; }

        [Column]
        public int count_SS { get; set; }

        [Column]
        public int count_SH { get; set; }

        [Column]
        public int count_S { get; set; }

        [Column]
        public int count_A { get; set; }

        [Column]
        public int playtime { get; set; }

        [Column]
        public int country_rank { get; set; }

        [Column]
        public int global_rank { get; set; }

        [PrimaryKey]
        public string? gamemode { get; set; }

        [PrimaryKey]
        public DateTimeOffset lastupdate { get; set; }
    }

    [Table("osu_performancepointplus_record")]
    public class OsuPPlus
    {
        [PrimaryKey]
        public long uid { get; set; }

        [Column]
        public double pp { get; set; }

        [Column]
        public double jump { get; set; }

        [Column]
        public double flow { get; set; }

        [Column]
        public double pre { get; set; }

        [Column]
        public double acc { get; set; }

        [Column]
        public double spd { get; set; }

        [Column]
        public double sta { get; set; }
    }

    [Table("badge_list")]
    public class BadgeList
    {
        [PrimaryKey, Identity]
        public int id { get; set; }

        [Column]
        public string? name { get; set; }

        [Column]
        public string? name_chinese { get; set; }

        [Column]
        public string? description { get; set; }
    }

    [Table("mail_verify")]
    public class MailVerify
    {
        [Column]
        public string? mailAddr { get; set; }

        [Column]
        public string? verify { get; set; }

        [Column]
        public string? gen_time { get; set; }
    }
}
