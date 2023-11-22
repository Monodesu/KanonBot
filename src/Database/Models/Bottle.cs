using LinqToDB;
using LinqToDB.Mapping;

namespace KanonBot.Database
{
    public static partial class Models
    {
        [Table("bottle")]
        public class Bottle
        {
            [PrimaryKey]
            public int id { get; set; }

            [Column]
            public string? time { get; set; }

            [Column]
            public string? platform { get; set; }

            [Column]
            public string? user { get; set; }

            [Column]
            public string? message { get; set; }

            [Column]
            public int pickedcount { get; set; }

            [Column]
            public bool haspickedup { get; set; }
        }
    }
}
