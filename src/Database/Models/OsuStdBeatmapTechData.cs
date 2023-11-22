using LinqToDB;
using LinqToDB.Mapping;

namespace KanonBot.Database
{
    public static partial class Models
    {
        [Table("osu_standard_beatmap_tech_data")]
        public class OsuStandardBeatmapTechData
        {
            [PrimaryKey]
            public long bid { get; set; }

            [Column]
            public double stars { get; set; }

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

            [Column]
            public int pp_99acc { get; set; }

            [Column]
            public int pp_98acc { get; set; }

            [Column]
            public int pp_97acc { get; set; }

            [Column]
            public int pp_95acc { get; set; }
        }
    }
}
