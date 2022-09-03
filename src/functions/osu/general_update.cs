using KanonBot.API;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static KanonBot.Database.Model;

namespace KanonBot.functions.osu
{
    public static class General_update
    {
        private static bool is_updated = false;
        async public static void Daily_Update(object override_update_flag)
        {
        DUSTART:
            if ((bool)(override_update_flag as bool?)!) is_updated = false;
            if (!is_updated)
            {
                if (DateTime.Now.Hour == 4 || (bool)(override_update_flag as bool?)!)
                {
                    var userlist = Database.Client.GetOsuUserList();
                    foreach (var user in userlist)
                    {
                        await Osu_Update(user, false);
                        Thread.Sleep(200);
                    }
                    is_updated = true;
                }
                Thread.Sleep(1000 * 60);
            }
            else
            {
                if (DateTime.Now.Hour == 3)
                    if (is_updated) is_updated = false;
                Thread.Sleep(1000 * 60);
            }
            if (!(bool)(override_update_flag as bool?)!) goto DUSTART;
        }

        async public static Task Osu_Update(long user, bool newuser)
        {
            for (int i = 0; i < 4; ++i)
            {
                try
                {
                    string mode = "osu";
                    switch (i)
                    {
                        case 0:
                            mode = "osu";
                            break;
                        case 1:
                            mode = "taiko";
                            break;
                        case 2:
                            mode = "fruits";
                            break;
                        case 3:
                            mode = "mania";
                            break;
                        default:
                            break;
                    }
                    Log.Information($"正在更新用户数据....[{user}/{mode}]");
                    var userInfo = await OSU.GetUser(user);
                    OsuArchivedRec rec = new()
                    {
                        uid = (int)userInfo!.Id,
                        play_count = (int)userInfo.Statistics.PlayCount,
                        ranked_score = userInfo.Statistics.RankedScore,
                        total_score = userInfo.Statistics.TotalScore,
                        total_hit = userInfo.Statistics.TotalHits,
                        level = userInfo.Statistics.Level.Current,
                        level_percent = userInfo.Statistics.Level.Progress,
                        performance_point = userInfo.Statistics.PP,
                        accuracy = userInfo.Statistics.HitAccuracy,
                        count_SSH = userInfo.Statistics.GradeCounts.SSH,
                        count_SS = userInfo.Statistics.GradeCounts.SS,
                        count_SH = userInfo.Statistics.GradeCounts.SH,
                        count_S = userInfo.Statistics.GradeCounts.S,
                        count_A = userInfo.Statistics.GradeCounts.A,
                        playtime = (int)userInfo.Statistics.PlayTime,
                        country_rank = (int)userInfo.Statistics.CountryRank,
                        global_rank = (int)userInfo.Statistics.GlobalRank,
                        gamemode = mode
                    };
                    Database.Client.InsertOsuUserData(rec, newuser);
                }
                catch
                {
                    //do nothing
                }
            }
        }
    }
}
