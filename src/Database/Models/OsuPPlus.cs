using LinqToDB;
using LinqToDB.Mapping;

namespace KanonBot.Database
{
    public static partial class Models
    {
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
    }
}
