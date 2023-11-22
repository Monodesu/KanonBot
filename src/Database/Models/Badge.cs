using LinqToDB;
using LinqToDB.Mapping;

namespace KanonBot.Database
{
    public static partial class Models
    {
        [Table("badge_list")]
        public class Badge
        {
            [PrimaryKey, Identity]
            public int id { get; set; }

            [Column]
            public string? name { get; set; }

            [Column]
            public string? name_chinese { get; set; }

            [Column]
            public string? description { get; set; }

            [Column]
            public DateTimeOffset expire_at { get; set; }
        }

        [Table("badge_redemption_code")]
        public class BadgeRedemptionCode
        {
            [PrimaryKey]
            public int id { get; set; }

            [Column]
            public int badge_id { get; set; }

            [Column]
            public bool can_repeatedly { get; set; }

            [Column]
            public int redeem_count { get; set; }

            [Column]
            public DateTimeOffset expire_at { get; set; }

            [Column]
            public DateTimeOffset gen_time { get; set; }

            [Column]
            public DateTimeOffset redeem_time { get; set; }

            [Column]
            public string? redeem_user { get; set; }

            [Column]
            public string? code { get; set; }

            [Column]
            public int badge_expiration_day { get; set; }
        }

        [Table("badge_expiration_date_rec")]
        public class BadgeExpirationDateRec
        {
            [Column]
            public int uid { get; set; }

            [Column]
            public int badge_id { get; set; }

            [Column]
            public DateTimeOffset expire_at { get; set; }
        }
    }
}
