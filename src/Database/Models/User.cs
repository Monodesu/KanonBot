using LinqToDB;
using LinqToDB.Mapping;

namespace KanonBot.Database
{
    public static partial class Models
    {
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
    }
}
