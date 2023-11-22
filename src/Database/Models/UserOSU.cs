using LinqToDB;
using LinqToDB.Mapping;

namespace KanonBot.Database
{
    public static partial class Models
    {
        [Table("users_osu")]
        public class UserOSU
        {
            [PrimaryKey]
            public long uid { get; set; }

            [PrimaryKey]
            public long osu_uid { get; set; }

            [Column]
            public string? osu_mode { get; set; }

            [Column]
            public int customInfoEngineVer { get; set; } // 1=v1 2=v2

            [Column]
            public string? InfoPanelV2_CustomMode { get; set; }

            [Column]
            public int InfoPanelV2_Mode { get; set; }
        }
    }
}
