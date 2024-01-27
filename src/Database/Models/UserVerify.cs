using LinqToDB;
using LinqToDB.Mapping;

namespace KanonBot.Database
{
    public static partial class Models
    {
        [Table("user_verify")]
        public class UserVerify
        {
            [Column]
            public string? email { get; set; }

            [PrimaryKey]
            public string? token { get; set; }

            [Column]
            public string? op { get; set; }

            [Column]
            public string? platform { get; set; }

            [Column]
            public DateTimeOffset gen_time { get; set; }
        }
    }
}
