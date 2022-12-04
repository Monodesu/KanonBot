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
        public DB(LinqToDBConnectionOptions options) : base(options)
        {
        }

        public ITable<OsuStandardBeatmapTechData>  OsuStandardBeatmapTechData  => this.GetTable<OsuStandardBeatmapTechData>();
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
        public int total { get; set; }
        public int aim { get; set; }
        public int speed { get; set; }
        public int acc { get; set; }
        public string? mod { get; set; }
    }
    [Table("osu_seasonalpass_2022_s4")]
    public class OSUSeasonalPass
    {
        [PrimaryKey]
        public long uid { get; set; }
        public string? mode { get; set; }
        public long tth { get; set; }
        public long inittth { get; set; }
    }

    [Table("users")]
    public class User
    {
        [PrimaryKey, Identity]
        public long uid { get; set; }
        [PrimaryKey]
        public string? email { get; set; }
        public string? passwd { get; set; }
        public long qq_id { get; set; }
        public string? qq_guild_uid { get; set; }
        public string? kook_uid { get; set; }
        public string? discord_uid { get; set; }
        public string? permissions { get; set; }
        public string? last_login_ip { get; set; }
        public string? last_login_time { get; set; }
        public int status { get; set; }
        public string? displayed_badge_ids { get; set; }
        public string? owned_badge_ids { get; set; }
    }

    [Table("users_osu")]
    public class UserOSU
    {
        [PrimaryKey]
        public long uid { get; set; }
        [PrimaryKey]
        public long osu_uid { get; set; }
        public string? osu_mode { get; set; }
        public int customInfoEngineVer { get; set; } // 1=v1 2=v2
        public string? InfoPanelV2_CustomMode { get; set; }
        public int InfoPanelV2_Mode { get; set; }
    }

    [Table("osu_archived_record")]
    public class OsuArchivedRec
    {
        [PrimaryKey]
        public int uid { get; set; }
        public int play_count { get; set; }
        public long ranked_score { get; set; }
        public long total_score { get; set; }
        public long total_hit { get; set; }
        public int level { get; set; }
        public int level_percent { get; set; }
        public double performance_point { get; set; }
        public double accuracy { get; set; }
        public int count_SSH { get; set; }
        public int count_SS { get; set; }
        public int count_SH { get; set; }
        public int count_S { get; set; }
        public int count_A { get; set; }
        public int playtime { get; set; }
        public int country_rank { get; set; }
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
        public double pp { get; set; }
        public double jump { get; set; }
        public double flow { get; set; }
        public double pre { get; set; }
        public double acc { get; set; }
        public double spd { get; set; }
        public double sta { get; set; }
    }

    [Table("badge_list")]
    public class BadgeList
    {
        [PrimaryKey, Identity]
        public int id { get; set; }
        public string? name { get; set; }
        public string? name_chinese { get; set; }
        public string? description { get; set; }
    }
    [Table("mail_verify")]
    public class MailVerify
    {
        public string? mailAddr { get; set; }
        public string? verify { get; set; }
        public string? gen_time { get; set; }
    }
}