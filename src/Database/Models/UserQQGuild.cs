using LinqToDB;
using LinqToDB.Mapping;

namespace KanonBot.Database
{
    public static partial class Models
    {
        [Table("user_qqguild")]
        public class UserQQGuild
        {
            [Column]
            public long uid { get; set; }

            [Column]
            public string? guild_id { get; set; }
        }
    }
}