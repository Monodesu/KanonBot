using LinqToDB;
using LinqToDB.Mapping;

namespace KanonBot.Database
{
    public static partial class Models
    {
        [Table("osu_archived_record")]
        public class OsuArchivedRec
        {
            [PrimaryKey]
            public int uid { get; set; }

            [Column]
            public int play_count { get; set; }

            [Column]
            public long ranked_score { get; set; }

            [Column]
            public long total_score { get; set; }

            [Column]
            public long total_hit { get; set; }

            [Column]
            public int level { get; set; }

            [Column]
            public int level_percent { get; set; }

            [Column]
            public double performance_point { get; set; }

            [Column]
            public double accuracy { get; set; }

            [Column]
            public int count_SSH { get; set; }

            [Column]
            public int count_SS { get; set; }

            [Column]
            public int count_SH { get; set; }

            [Column]
            public int count_S { get; set; }

            [Column]
            public int count_A { get; set; }

            [Column]
            public int playtime { get; set; }

            [Column]
            public int country_rank { get; set; }

            [Column]
            public int global_rank { get; set; }

            [PrimaryKey]
            public string? gamemode { get; set; }

            [PrimaryKey]
            public DateTimeOffset lastupdate { get; set; }
        }
    }
}
