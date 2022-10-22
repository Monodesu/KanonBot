using KanonBot.API;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Threading.Tasks;
using static KanonBot.Database.Model;
using CronNET.Impl;

namespace KanonBot.functions.osu
{
    public static class GeneralUpdate
    {
        private static readonly CronDaemon daemon = new CronDaemon();
        private static bool is_updated = false;
        public static void DailyUpdate()
        {
            daemon.Add(new CronJob(async () =>
            {
                var span = await UpdateUsers();
                Log.Information("更新完毕，总花费时间 {0}s", span.TotalSeconds);
            }, "DailyUpdate", "0 18 4 1/1 * ? *"));   // 每天早上4点运行的意思，可以在这里生成 http://www.cronmaker.com/
            daemon.Start(CancellationToken.None);
            while (true) ;//卡住更新线程
        }



        async public static Task<TimeSpan> UpdateUsers()
        {
            var stopwatch = Stopwatch.StartNew();
            var userList = Database.Client.GetOsuUserList();
            ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = 4 };
            await Parallel.ForEachAsync(userList, options, async (userID, _) => {
                try
                {
                    await UpdateUser(userID, false);
                }
                catch (Exception e)
                {
                    Log.Warning("更新用户信息时出错，ex: {@0}", e);
                }
            });
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }

        async public static Task UpdateUser(long userID, bool is_newuser)
        {
            var modes = new OSU.Enums.Mode[] { OSU.Enums.Mode.OSU, OSU.Enums.Mode.Taiko, OSU.Enums.Mode.Fruits, OSU.Enums.Mode.Mania };
            foreach (var mode in modes)
            {
                Log.Information($"正在更新用户数据....[{userID}/{mode}]");
                var userInfo = await OSU.GetUser(userID, mode);
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
                    gamemode = mode.ToModeStr()
                };
                Database.Client.InsertOsuUserData(rec, false);
            }
        }
    }
}
