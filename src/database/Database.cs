#pragma warning disable CS8602 // 解引用可能出现空引用。
#pragma warning disable IDE0044 // 添加只读修饰符
using SqlSugar;
using Serilog;
using KanonBot.Drivers;
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

        return db;
    }


    static public bool SetVerifyMail(string mailAddr, string verify)
    {
        var db = GetInstance();
        var newverify = new Model.MailVerify()
        {
            mailAddr = mailAddr,
            verify = verify,
            gen_time = Utils.GetTimeStamp(false)
        };
        try { db.Insertable(newverify).ExecuteCommand(); return true; } catch { return false; }
    }

    static public bool IsRegd(string mailAddr)
    {
        var db = GetInstance();
        var li = db.Queryable<Model.Users>().Where(it => it.email == mailAddr).Select(it => it.uid).ToList();
        if (li.Count > 0)
            return true;
        return false;
    }

    static public bool IsRegd(string uid, Platform platform)
    {
        var db = GetInstance();
        switch (platform)
        {
            case Platform.OneBot:
                var li1 = db.Queryable<Model.Users>().Where(it => it.qq_id == long.Parse(uid)).Select(it => it.uid).ToList();
                if (li1.Count > 0) return true;
                return false;
            case Platform.Guild:
                var li2 = db.Queryable<Model.Users>().Where(it => it.qq_guild_uid == uid).Select(it => it.uid).ToList();
                if (li2.Count > 0) return true;
                return false;
            case Platform.KOOK:
                var li3 = db.Queryable<Model.Users>().Where(it => it.kook_uid == uid).Select(it => it.uid).ToList();
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

    static public Model.Users GetUsers(string mailAddr)
    {
        var db = GetInstance();
        return db.Queryable<Model.Users>().Where(it => it.email == mailAddr).First();
    }

    static public Model.Users? GetUsersByUID(string UID, Platform platform)
    {
        var db = GetInstance();
        switch (platform)
        {
            case Platform.OneBot:
                var li1 = db.Queryable<Model.Users>().Where(it => it.qq_id == long.Parse(UID)).ToList();
                if (li1.Count > 0) return li1[0];
                return null;
            case Platform.Guild:
                var li2 = db.Queryable<Model.Users>().Where(it => it.qq_guild_uid == UID).ToList();
                if (li2.Count > 0) return li2[0];
                return null;
            case Platform.KOOK:
                var li3 = db.Queryable<Model.Users>().Where(it => it.kook_uid == UID).ToList();
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
    static public Model.Users? GetUsersByOsuUID(long osu_uid)
    {
        var db = GetInstance();
        var li1 = db.Queryable<Model.Users>().Where(it => it.uid == GetOsuUsersByOsuUID(osu_uid).uid).ToList();
        if (li1.Count > 0) return li1[0];
        return null;
    }
    static public Model.Users_osu? GetOsuUsersByOsuUID(long osu_uid)
    {
        var db = GetInstance();
        var li1 = db.Queryable<Model.Users_osu>().Where(it => it.osu_uid == osu_uid).ToList();
        if (li1.Count > 0) return li1[0];
        return null;
    }

    static public Model.Users_osu GetOSUUsers(long osu_uid)
    {
        var db = GetInstance();
        return db.Queryable<Model.Users_osu>().Where(it => it.osu_uid == osu_uid).First();
    }

    static public Model.Users_osu GetOSUUsersByUID(long kanon_uid)
    {
        var db = GetInstance();
        return db.Queryable<Model.Users_osu>().Where(it => it.uid == kanon_uid).First();
    }

    static public bool InsertOsuUser(long kanon_uid, long osu_uid, int customBannerStatus)
    {
        //customBannerStatus: 0=没有自定义banner 1=在猫猫上设置了自定义banner 2=在osuweb上设置了自定义banner但是猫猫上没有
        var d = new Model.Users_osu()
        {
            uid = kanon_uid,
            osu_uid = osu_uid,
            osu_mode = "osu",
            customBannerStatus = customBannerStatus
        };
        var d2 = GetInstance();
        try { d2.Insertable(d).ExecuteCommand(); return true; } catch { return false; }

    }

    static public bool UpdateOsuPPlusData(API.OSU.Models.PPlusData.UserData ppdata, long osu_uid)
    {
        var db = GetInstance();
        var data = db.Queryable<Model.OsuPPlus>().First(it => it.uid == osu_uid);
        if (data != null)
        {
            var result = db.Updateable<Model.OsuPPlus>()
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
                            .ExecuteCommandHasChange();
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
            db.Insertable(init).ExecuteCommand();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
        return true;
    }

    static public bool SetDisplayedBadge(string userid, string displayed_ids)
    {
        var db = GetInstance();
        var data = db.Queryable<Model.Users>().First(it => it.uid == long.Parse(userid));
        var result = db.Updateable<Model.Users>()
            .SetColumns(it => new Model.Users()
            {
                displayed_badge_ids = displayed_ids
            })
            .Where(it => it.uid == long.Parse(userid))
            .ExecuteCommandHasChange();
        return result;
    }

    static public Model.BadgeList GetBadgeInfo(string badgeid)
    {
        var db = GetInstance();
        return db.Queryable<Model.BadgeList>().Where(it => it.id == int.Parse(badgeid)).First();
    }

    static public bool SetOwnedBadge(string userid, string owned_ids)
    {
        var db = GetInstance();
        var data = db.Queryable<Model.Users>().First(it => it.uid == long.Parse(userid));
        var result = db.Updateable<Model.Users>()
            .SetColumns(it => new Model.Users()
            {
                owned_badge_ids = owned_ids
            })
            .Where(it => it.uid == long.Parse(userid))
            .ExecuteCommandHasChange();
        return result;
    }

    static public long[] GetOsuUserList()
    {
        List<long> list = new();
        var db = GetInstance();
        var user = db.Queryable<Model.Users_osu>().ToList();
        foreach (var x in user)
        {
            list.Add(x.osu_uid);
        }
        return list.ToArray();
    }

    static public void InsertOsuUserData(OsuArchivedRec rec, bool is_newuser)
    {
        rec.lastupdate = is_newuser ? DateTime.Today.AddDays(-1) : DateTime.Today;
        var d2 = GetInstance();
        try { d2.Insertable(rec).ExecuteCommand(); } catch { }
    }







}