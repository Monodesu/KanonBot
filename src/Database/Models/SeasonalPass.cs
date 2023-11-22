using LinqToDB;
using LinqToDB.Mapping;

namespace KanonBot.Database
{
    public static partial class Models
    {
        [Table("seasonalpass_scorerecords")]
        public class SeasonalPassScoreRecords
        {
            [Column]
            public long score_id { get; set; }

            [Column]
            public string? mode { get; set; }
        }

        [Table("seasonalpass_2023_s2")]
        public class SeasonalPass
        {
            [Column]
            public long osu_id { get; set; }

            [Column]
            public string? mode { get; set; }

            [Column]
            public int point { get; set; }
        }
    }
}
