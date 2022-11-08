#pragma warning disable CS8602 // 解引用可能出现空引用。
#pragma warning disable IDE0044 // 添加只读修饰符
using KanonBot.Drivers;
using Serilog;
using SqlSugar;
using static KanonBot.API.OSU.Models;
using static KanonBot.Database.Model;

namespace KanonBot.Database;

public class Client
{
    private static Config.Base config = Config.inner!;

    static private SqlSugarClient GetInstance()
    {
        // 暂时只有Mysql
        var db = new SqlSugarClient(new ConnectionConfig()
        {
            ConnectionString = $"server={config.database.host};" +
            $"port={config.database.port};" +
            $"database={config.database.db};" +
            $"user={config.database.user};" +
            $"password={config.database.password};charset=utf8mb4",

            DbType = SqlSugar.DbType.MySql,
            IsAutoCloseConnection = true
        });


        // 添加Sql打印事件
        db.Aop.OnLogExecuting = (sql, pars) =>
        {
            Log.Debug("[Database] {0}", sql);
        };

        db.Ado.CommandTimeOut = 30;

        return db;
    }


    static public async Task<bool> SetVerifyMail(string mailAddr, string verify)
    {
        var db = GetInstance();
        var newverify = new Model.MailVerify()
        {
            mailAddr = mailAddr,
            verify = verify,
            gen_time = Utils.GetTimeStamp(false)
        };
        try { await db.Insertable(newverify).ExecuteCommandAsync(); return true; } catch { return false; }
    }

    static public async Task<bool> IsRegd(string mailAddr)
    {
        var db = GetInstance();
        var li = await db.Queryable<Model.User>().Where(it => it.email == mailAddr).Select(it => it.uid).ToListAsync();
        if (li.Count > 0)
            return true;
        return false;
    }

    static public async Task<bool> IsRegd(string uid, Platform platform)
    {
        var db = GetInstance();
        switch (platform)
        {
            case Platform.OneBot:
                var li1 = await db.Queryable<Model.User>().Where(it => it.qq_id == long.Parse(uid)).Select(it => it.uid).ToListAsync();
                if (li1.Count > 0) return true;
                return false;
            case Platform.Guild:
                var li2 = await db.Queryable<Model.User>().Where(it => it.qq_guild_uid == uid).Select(it => it.uid).ToListAsync();
                if (li2.Count > 0) return true;
                return false;
            case Platform.KOOK:
                var li3 = await db.Queryable<Model.User>().Where(it => it.kook_uid == uid).Select(it => it.uid).ToListAsync();
                if (li3.Count > 0) return true;
                return false;
            // case "discord":
            //     var li4 = db.Queryable<Model.Users>().Where(it => it.qq_guild_uid == uid).Select(it => it.uid).ToList();
            //     if (li4.Count > 0) return true;
            //     return false;
            default:
                return true;
        }
    }

    static public async Task<Model.User> GetUsers(string mailAddr)
    {
        var db = GetInstance();
        return await db.Queryable<Model.User>().Where(it => it.email == mailAddr).FirstAsync();
    }

    static public async Task<Model.User?> GetUsersByUID(string UID, Platform platform)
    {
        var db = GetInstance();
        switch (platform)
        {
            case Platform.OneBot:
                var li1 = await db.Queryable<Model.User>().Where(it => it.qq_id == long.Parse(UID)).ToListAsync();
                if (li1.Count > 0) return li1[0];
                return null;
            case Platform.Guild:
                var li2 = await db.Queryable<Model.User>().Where(it => it.qq_guild_uid == UID).ToListAsync();
                if (li2.Count > 0) return li2[0];
                return null;
            case Platform.KOOK:
                var li3 = await db.Queryable<Model.User>().Where(it => it.kook_uid == UID).ToListAsync();
                if (li3.Count > 0) return li3[0];
                return null;
            // case "discord":  // 还没写
            //     var li4 = db.Queryable<Model.Users>().Where(it => it.discord_uid == UID).ToList();
            //     if (li4.Count > 0) return li4[0];
            //     return null;
            default:
                return null;
        }
    }
    static public async Task<Model.User?> GetUserByOsuUID(long osu_uid)
    {
        var user = await GetOsuUser(osu_uid);
        if (user == null) { return null; }
        return await GetInstance().Queryable<Model.User>().Where(it => it.uid == user.uid).FirstAsync();
    }

    static public async Task<Model.UserOSU?> GetOsuUser(long osu_uid)
    {
        return await GetInstance().Queryable<Model.UserOSU>().Where(it => it.osu_uid == osu_uid).FirstAsync();
    }

    static public async Task<Model.UserOSU?> GetOsuUserByUID(long kanon_uid)
    {
        return await GetInstance().Queryable<Model.UserOSU>().Where(it => it.uid == kanon_uid).FirstAsync();
    }

    static public async Task<bool> InsertOsuUser(long kanon_uid, long osu_uid, int customBannerStatus)
    {
        //customBannerStatus: 0=没有自定义banner 1=在猫猫上设置了自定义banner 2=在osuweb上设置了自定义banner但是猫猫上没有
        var d = new Model.UserOSU()
        {
            uid = kanon_uid,
            osu_uid = osu_uid,
            osu_mode = "osu",
            customBannerStatus = customBannerStatus
        };
        var d2 = GetInstance();
        try { await d2.Insertable(d).ExecuteCommandAsync(); return true; } catch { return false; }

    }

    static public async Task<API.OSU.Models.PPlusData.UserData?> GetOsuPPlusData(long osu_uid)
    {
        var data = await GetInstance().Queryable<Model.OsuPPlus>().FirstAsync(it => it.uid == osu_uid && it.pp != 0);
        if (data != null)
        {
            var realData = new API.OSU.Models.PPlusData.UserData();
            realData.UserId = osu_uid;
            realData.PerformanceTotal = data.pp;
            realData.AccuracyTotal = data.acc;
            realData.FlowAimTotal = data.flow;
            realData.JumpAimTotal = data.jump;
            realData.PrecisionTotal = data.pre;
            realData.SpeedTotal = data.spd;
            realData.StaminaTotal = data.sta;
            return realData;
        }
        else
        {
            return null;
        }
    }

    static public async Task<bool> UpdateOsuPPlusData(API.OSU.Models.PPlusData.UserData ppdata, long osu_uid)
    {
        var db = GetInstance();
        var data = await db.Queryable<Model.OsuPPlus>().FirstAsync(it => it.uid == osu_uid);
        if (data != null)
        {
            var result = await db.Updateable<Model.OsuPPlus>()
                            .SetColumns(it => new Model.OsuPPlus()
                            {
                                pp = ppdata.PerformanceTotal,
                                acc = ppdata.AccuracyTotal,
                                flow = ppdata.FlowAimTotal,
                                jump = ppdata.JumpAimTotal,
                                pre = ppdata.PrecisionTotal,
                                spd = ppdata.SpeedTotal,
                                sta = ppdata.StaminaTotal
                            })
                            .Where(it => it.uid == osu_uid)
                            .ExecuteCommandHasChangeAsync();
            return result;
        }
        // 数据库没有数据，新插入数据
        try
        {
            var init = new Model.OsuPPlus();
            init.uid = osu_uid;
            init.pp = ppdata.PerformanceTotal;
            init.acc = ppdata.AccuracyTotal;
            init.flow = ppdata.FlowAimTotal;
            init.jump = ppdata.JumpAimTotal;
            init.pre = ppdata.PrecisionTotal;
            init.spd = ppdata.SpeedTotal;
            init.sta = ppdata.StaminaTotal;
            await db.Insertable(init).ExecuteCommandAsync();
        }
        catch (Exception ex)
        {
            Log.Warning(ex.Message);
            return false;
        }
        return true;
    }

    static public async Task<bool> SetDisplayedBadge(string userid, string displayed_ids)
    {
        var db = GetInstance();
        var data = await db.Queryable<Model.User>().FirstAsync(it => it.uid == long.Parse(userid));
        var result = await db.Updateable<Model.User>()
            .SetColumns(it => new Model.User()
            {
                displayed_badge_ids = displayed_ids
            })
            .Where(it => it.uid == long.Parse(userid))
            .ExecuteCommandHasChangeAsync();
        return result;
    }

    static public async Task<Model.BadgeList?> GetBadgeInfo(string badgeid)
    {
        return await GetInstance().Queryable<Model.BadgeList>().Where(it => it.id == int.Parse(badgeid)).FirstAsync();
    }

    static public async Task<bool> SetOwnedBadge(string userid, string owned_ids)
    {
        var db = GetInstance();
        var data = await db.Queryable<Model.User>().FirstAsync(it => it.uid == long.Parse(userid));
        var result = await db.Updateable<Model.User>()
            .SetColumns(it => new Model.User()
            {
                owned_badge_ids = owned_ids
            })
            .Where(it => it.uid == long.Parse(userid))
            .ExecuteCommandHasChangeAsync();
        return result;
    }

    static public async Task<List<long>> GetOsuUserList()
    {
        return await GetInstance().Queryable<Model.UserOSU>().Select(it => it.osu_uid).ToListAsync();
    }

    static public async Task<int> InsertOsuUserData(OsuArchivedRec rec, bool is_newuser)
    {
        rec.lastupdate = is_newuser ? DateTime.Today.AddDays(-1) : DateTime.Today;
        return await GetInstance().Insertable(rec).ExecuteReturnIdentityAsync();
    }

    static public async Task<bool> SetOsuUserMode(long osu_uid, API.OSU.Enums.Mode mode)
    {
        var db = GetInstance();
        var result = await db.Updateable<Model.UserOSU>()
            .SetColumns(it => new Model.UserOSU()
            {
                osu_mode = API.OSU.Enums.ParseMode(mode),
            })
            .Where(it => it.osu_uid == osu_uid)
            .ExecuteCommandHasChangeAsync();
        return result;
    }

    //返回值为天数（几天前）
    public static async Task<(int, API.OSU.Models.User?)> GetOsuUserData(long oid, API.OSU.Enums.Mode mode, int days = 0)
    {
        OsuArchivedRec? data;
        var db = GetInstance();
        var ui = new API.OSU.Models.User();
        if (days <= 0)
        {
            data = await db.Queryable<OsuArchivedRec>().OrderBy(it => it.lastupdate, OrderByType.Desc).FirstAsync(it => it.uid == oid && it.gamemode == API.OSU.Enums.ParseMode(mode));
        }
        else
        {
            var date = DateTime.Today;
            try {
                date = date.AddDays(-days);
            } catch (ArgumentOutOfRangeException) {
                return (-1, null);
            }
            data = await db.Queryable<OsuArchivedRec>().OrderBy(it => it.lastupdate, OrderByType.Desc).FirstAsync(it => it.uid == oid && it.gamemode == API.OSU.Enums.ParseMode(mode) && it.lastupdate <= date);
            if (data == null)
            {
                data = await db.Queryable<OsuArchivedRec>().OrderBy(it => it.lastupdate).FirstAsync(it => it.uid == oid && it.gamemode == API.OSU.Enums.ParseMode(mode));
            }
        }
        if (data == null) return (-1, null);

        ui.Statistics = new();
        ui.Statistics.GradeCounts = new();
        ui.Statistics.Level = new();
        ui.Id = oid;
        ui.Statistics.TotalScore = data.total_score;
        ui.Statistics.TotalHits = data.total_hit;
        ui.Statistics.PlayCount = data.play_count;
        ui.Statistics.RankedScore = data.ranked_score;
        ui.Statistics.CountryRank = data.country_rank;
        ui.Statistics.GlobalRank = data.global_rank;
        ui.Statistics.HitAccuracy = data.accuracy;
        ui.Statistics.GradeCounts.SSH = data.count_SSH;
        ui.Statistics.GradeCounts.SS = data.count_SS;
        ui.Statistics.GradeCounts.SH = data.count_SH;
        ui.Statistics.GradeCounts.S = data.count_S;
        ui.Statistics.GradeCounts.A = data.count_A;
        ui.Statistics.Level.Current = data.level;
        ui.Statistics.Level.Progress = data.level_percent;
        ui.Statistics.PP = data.performance_point;
        ui.PlayMode = mode;
        //ui.daysBefore = (t - data.lastupdate).Days;
        return ((DateTime.Today - data.lastupdate).Days, ui);
    }


}