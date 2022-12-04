#pragma warning disable CS8602 // 解引用可能出现空引用。
#pragma warning disable IDE0044 // 添加只读修饰符
using KanonBot.Drivers;
using LinqToDB;
using LinqToDB.Configuration;
using LinqToDB.Data;
using MySqlConnector;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Ocsp;
using Polly.Caching;
using RosuPP;
using Serilog;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using Tomlyn.Model;
using static KanonBot.API.OSU.Enums;
using static KanonBot.API.OSU.Models;
using static KanonBot.Database.Model;

namespace KanonBot.Database;

public class Client
{
    private static Config.Base config = Config.inner!;

    static private DB GetInstance()
    {
        var builder = new LinqToDBConnectionOptionsBuilder();
        builder.UseMySqlConnector(
            new MySqlConnectionStringBuilder
            {
                Server = config.database.host,
                Port = (uint)config.database.port,
                UserID = config.database.user,
                Password = config.database.password,
                Database = config.database.db,
                CharacterSet = "utf8mb4",
            }.ConnectionString
        );
        // 暂时只有Mysql
        var db = new DB(builder.Build());
        return db;
    }

    static public async Task<bool> SetVerifyMail(string mailAddr, string verify)
    {
        using var db = GetInstance();
        var newverify = new Model.MailVerify()
        {
            mailAddr = mailAddr,
            verify = verify,
            gen_time = Utils.GetTimeStamp(false)
        };

        try
        {
            await db.InsertAsync(newverify);
            return true;
        }
        catch
        {
            return false;
        }
    }

    static public async Task<bool> IsRegd(string mailAddr)
    {
        using var db = GetInstance();
        var li = await db.User.Where(it => it.email == mailAddr).Select(it => it.uid).ToListAsync();
        if (li.Count > 0)
            return true;
        return false;
    }

    static public async Task<bool> IsRegd(string uid, Platform platform)
    {
        using var db = GetInstance();
        switch (platform)
        {
            case Platform.OneBot:
                var qid = long.Parse(uid);
                var li1 = await db.User
                    .Where(it => it.qq_id == qid)
                    .Select(it => it.uid)
                    .ToListAsync();
                if (li1.Count > 0)
                    return true;
                return false;
            case Platform.Guild:
                var li2 = await db.User
                    .Where(it => it.qq_guild_uid == uid)
                    .Select(it => it.uid)
                    .ToListAsync();
                if (li2.Count > 0)
                    return true;
                return false;
            case Platform.KOOK:
                var li3 = await db.User
                    .Where(it => it.kook_uid == uid)
                    .Select(it => it.uid)
                    .ToListAsync();
                if (li3.Count > 0)
                    return true;
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
        using var db = GetInstance();
        return await db.User.Where(it => it.email == mailAddr).FirstAsync();
    }

    static public async Task<Model.User?> GetUsersByUID(string UID, Platform platform)
    {
        using var db = GetInstance();
        switch (platform)
        {
            case Platform.OneBot:
                var qid = long.Parse(UID);
                var li1 = await db.User.Where(it => it.qq_id == qid).ToListAsync();
                if (li1.Count > 0)
                    return li1[0];
                return null;
            case Platform.Guild:
                var li2 = await db.User.Where(it => it.qq_guild_uid == UID).ToListAsync();
                if (li2.Count > 0)
                    return li2[0];
                return null;
            case Platform.KOOK:
                var li3 = await db.User.Where(it => it.kook_uid == UID).ToListAsync();
                if (li3.Count > 0)
                    return li3[0];
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
        using var db = GetInstance();
        var user = await GetOsuUser(osu_uid);
        if (user == null)
        {
            return null;
        }
        return await db.User.Where(it => it.uid == user.uid).FirstAsync();
    }

    static public async Task<Model.UserOSU?> GetOsuUser(long osu_uid)
    {
        using var db = GetInstance();
        return await db.UserOSU.Where(it => it.osu_uid == osu_uid).FirstAsync();
    }

    static public async Task<Model.UserOSU?> GetOsuUserByUID(long kanon_uid)
    {
        using var db = GetInstance();
        return await db.UserOSU.Where(it => it.uid == kanon_uid).FirstAsync();
    }

    static public async Task<bool> InsertOsuUser(
        long kanon_uid,
        long osu_uid,
        int customBannerStatus
    )
    {
        using var db = GetInstance();
        var d = new Model.UserOSU()
        {
            uid = kanon_uid,
            osu_uid = osu_uid,
            osu_mode = "osu",
            customInfoEngineVer = 2,
            InfoPanelV2_Mode = 1
        };
        try
        {
            await db.InsertAsync(d);
            return true;
        }
        catch
        {
            return false;
        }
    }

    static public async Task<API.OSU.Models.PPlusData.UserData?> GetOsuPPlusData(long osu_uid)
    {
        using var db = GetInstance();
        var data = await db.OsuPPlus.FirstAsync(it => it.uid == osu_uid && it.pp != 0);
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

    static public async Task<bool> UpdateOsuPPlusData(
        API.OSU.Models.PPlusData.UserData ppdata,
        long osu_uid
    )
    {
        using var db = GetInstance();
        var data = await db.OsuPPlus.FirstAsync(it => it.uid == osu_uid);
        var result = await db.InsertOrReplaceAsync(
            new Model.OsuPPlus()
            {
                uid = osu_uid,
                pp = ppdata.PerformanceTotal,
                acc = ppdata.AccuracyTotal,
                flow = ppdata.FlowAimTotal,
                jump = ppdata.JumpAimTotal,
                pre = ppdata.PrecisionTotal,
                spd = ppdata.SpeedTotal,
                sta = ppdata.StaminaTotal
            }
        );
        return result > -1;
    }

    static public async Task<bool> SetDisplayedBadge(string userid, string displayed_ids)
    {
        using var db = GetInstance();
        var data = await db.User.FirstAsync(it => it.uid == long.Parse(userid));
        var res = await db.User
            .Where(it => it.uid == long.Parse(userid))
            .Set(it => it.displayed_badge_ids, displayed_ids)
            .UpdateAsync();

        return res > -1;
    }

    static public async Task<Model.BadgeList?> GetBadgeInfo(string badgeid)
    {
        using var db = GetInstance();
        return await db
            .BadgeList
            .Where(it => it.id == int.Parse(badgeid))
            .FirstAsync();
    }

    static public async Task<bool> SetOwnedBadge(string email, string owned_ids)
    {
        using var db = GetInstance();
        var data = await db.User.FirstAsync(it => it.email == email);
        data.owned_badge_ids = owned_ids;
        var res = await db.UpdateAsync(data);
        return res > -1;
    }

    static public async Task<bool> SetOwnedBadgeByOsuUid(string osu_uid, string owned_ids)
    {
        var user = await GetOsuUser(long.Parse(osu_uid));
        if (user == null)
        {
            return false;
        }
        using var db = GetInstance();
        var userinfo = await db
            .User
            .Where(it => it.uid == user.uid)
            .FirstAsync();
        userinfo.owned_badge_ids = owned_ids;
        var res = await db.UpdateAsync(userinfo);
        return res > -1;
    }

    static public async Task<List<long>> GetOsuUserList()
    {
        using var db = GetInstance();
        return await db
            .UserOSU
            .Select(it => it.osu_uid)
            .ToListAsync();
    }

    static public async Task<int> InsertOsuUserData(OsuArchivedRec rec, bool is_newuser)
    {
        using var db = GetInstance();
        rec.lastupdate = is_newuser ? DateTime.Today.AddDays(-1) : DateTime.Today;
        return await db.InsertAsync(rec);
    }

    static public async Task<bool> SetOsuUserMode(long osu_uid, API.OSU.Enums.Mode mode)
    {
        using var db = GetInstance();
        var result = await db.UserOSU
            .Where(it => it.osu_uid == osu_uid)
            .Set(it => it.osu_mode, API.OSU.Enums.Mode2String(mode)).UpdateAsync();
        return result > -1;
    }

    //返回值为天数（几天前）
    public static async Task<(int, API.OSU.Models.User?)> GetOsuUserData(
        long oid,
        API.OSU.Enums.Mode mode,
        int days = 0
    )
    {
        OsuArchivedRec? data;
        using var db = GetInstance();
        var ui = new API.OSU.Models.User();
        if (days <= 0)
        {
            var q = from p in db.OsuArchivedRec
                where p.uid == oid && p.gamemode == API.OSU.Enums.Mode2String(mode)
                orderby p.lastupdate descending
                select p;
            data = await q.FirstAsync();
        }
        else
        {
            var date = DateTime.Today;
            try
            {
                date = date.AddDays(-days);
            }
            catch (ArgumentOutOfRangeException)
            {
                return (-1, null);
            }
            var q = from p in db.OsuArchivedRec
                where p.uid == oid && p.gamemode == API.OSU.Enums.Mode2String(mode) && p.lastupdate <= date
                orderby p.lastupdate descending
                select p;
            data = await q.FirstAsync();
            if (data == null)
            {
                var tq = from p in db.OsuArchivedRec
                    where p.uid == oid && p.gamemode == API.OSU.Enums.Mode2String(mode)
                    orderby p.lastupdate
                    select p;
                    data = await tq.FirstAsync();
            }
        }
        if (data == null)
            return (-1, null);

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
        ui.Statistics.PlayTime = data.playtime;
        //ui.daysBefore = (t - data.lastupdate).Days;
        return ((DateTime.Today - data.lastupdate).Days, ui);
    }

    //return badge_id
    public static async Task<int> InsertBadge(string ENG_NAME, string CHN_NAME, string CHN_DECS)
    {
        using var db = GetInstance();
        BadgeList bl =
            new()
            {
                name = ENG_NAME,
                name_chinese = CHN_NAME,
                description = CHN_DECS
            };
        return await db.InsertAsync(bl);
    }

    public static async Task<bool> UpdateSeasonalPass(long oid, string mode, long tth)
    {
        //检查数据库中有无信息
        using var db = GetInstance();
        var db_info = await db
            .OSUSeasonalPass
            .Where(it => it.uid == oid)
            .Where(it => it.mode == mode)
            .ToListAsync();
        if (db_info.Count > 0)
        {
            return await db
                .OSUSeasonalPass
                .Where(it => it.uid == oid)
                .Where(it => it.mode == mode)
                .Set(it => it.tth, tth)
                .UpdateAsync() > -1;
        }
        var t = false;
        if (
            await db
                .InsertAsync(
                    new OSUSeasonalPass()
                    {
                        inittth = tth,
                        tth = tth,
                        mode = mode,
                        uid = oid
                    }
                ) > -1
        )
            t = true;
        return t;
    }

    static public async Task<bool> SetOsuInfoPanelVersion(long osu_uid, int ver)
    {
        using var db = GetInstance();
        var result = await db
            .UserOSU
            .Where(it => it.osu_uid == osu_uid)
            .Set(it => it.customInfoEngineVer, ver)
            .UpdateAsync();
        return result > -1;
    }

    static public async Task<bool> SetOsuInfoPanelV2ColorMode(long osu_uid, int ver)
    {
        using var db = GetInstance();
        var result = await db
            .UserOSU
            .Where(it => it.osu_uid == osu_uid)
            .Set(it => it.InfoPanelV2_Mode, ver)
            .UpdateAsync();
        return result > -1;
    }

    public static async Task<bool> UpdateInfoPanelV2CustomCmd(long osu_uid, string cmd)
    {
        using var db = GetInstance();
        var result = await db
            .UserOSU
            .Where(it => it.osu_uid == osu_uid)
            .Set(it => it.InfoPanelV2_CustomMode, cmd)
            .UpdateAsync();
        return result > -1;
    }

    static public async Task<bool> SetOsuUserPermissionByOid(long osu_uid, string permission)
    {
        var DBUser = await GetUserByOsuUID(osu_uid);
        using var db = GetInstance();
        var result = await db
            .User
            .Where(it => it.uid == DBUser.uid)
            .Set(it => it.permissions, permission)
            .UpdateAsync();
        return result > -1;
    }

    static public async Task<bool> SetOsuUserPermissionByEmail(string email, string permission)
    {
        using var db = GetInstance();
        var result = await db
            .User
            .Where(it => it.email == email)
            .Set(it => it.permissions, permission)
            .UpdateAsync();
        return result > -1;
    }

    public static async Task<bool> InsertOsuStandardBeatmapTechData(
        long bid,
        int total,
        int acc,
        int speed,
        int aim,
        string[] mods
    )
    {
        using var db = GetInstance();
        var modstring = "";
        if (mods.Length > 0)
        {
            foreach (var x in mods)
                modstring += x + ",";
            modstring = modstring[..^1];
        }

        //查找谱面对应的mod数据是否存在
        var db_info = await db
            .OsuStandardBeatmapTechData
            .Where(it => it.bid == bid)
            .Where(it => it.mod == modstring)
            .ToListAsync();
        if (db_info.Count == 0)
        {
            //不存在再执行添加
            OsuStandardBeatmapTechData t =
                new()
                {
                    bid = bid,
                    total = total,
                    acc = acc,
                    speed = speed,
                    aim = aim,
                    mod = modstring
                };
            try
            {
                await db.InsertAsync(t);
                return true;
            }
            catch
            {
                return false;
            }
        }
        else
        {
            return true;
        }
    }
}
