using LinqToDB;
using LinqToDB.Mapping;

namespace KanonBot.Database
{
    public static partial class Models
    {
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
}
