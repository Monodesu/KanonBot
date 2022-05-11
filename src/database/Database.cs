#pragma warning disable CS8602 // 解引用可能出现空引用。
#pragma warning disable IDE0044 // 添加只读修饰符
using SqlSugar;
using Serilog;

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

    static public bool IsRegd(string uid, string platform)
    {
        var db = GetInstance();
        switch (platform)
        {
            case "qq":
                var li1 = db.Queryable<Model.Users>().Where(it => it.qq_id == long.Parse(uid)).Select(it => it.uid).ToList();
                if (li1.Count > 0) return true;
                return false;
            case "qguild":
                var li2 = db.Queryable<Model.Users>().Where(it => it.qq_guild_uid == uid).Select(it => it.uid).ToList();
                if (li2.Count > 0) return true;
                return false;
            case "khl":
                var li3 = db.Queryable<Model.Users>().Where(it => it.qq_guild_uid == uid).Select(it => it.uid).ToList();
                if (li3.Count > 0) return true;
                return false;
            case "discord":
                var li4 = db.Queryable<Model.Users>().Where(it => it.qq_guild_uid == uid).Select(it => it.uid).ToList();
                if (li4.Count > 0) return true;
                return false;
            default:
                return true;
        }
    }

    static public Model.Users GetUsers(string mailAddr)
    {
        var db = GetInstance();
        return db.Queryable<Model.Users>().Where(it => it.email == mailAddr).First();
    }

    static public Model.Users? GetUsersByUID(string UID, string platform)
    {
        var db = GetInstance();
        switch (platform)
        {
            case "qq":
                var li1 = db.Queryable<Model.Users>().Where(it => it.qq_id == long.Parse(UID)).ToList();
                if (li1.Count > 0) return li1[0];
                return null;
            case "qguild":
                var li2 = db.Queryable<Model.Users>().Where(it => it.qq_guild_uid == UID).ToList();
                if (li2.Count > 0) return li2[0];
                return null;
            case "khl":
                var li3 = db.Queryable<Model.Users>().Where(it => it.khl_uid == UID).ToList();
                if (li3.Count > 0) return li3[0];
                return null;
            case "discord":
                var li4 = db.Queryable<Model.Users>().Where(it => it.discord_uid == UID).ToList();
                if (li4.Count > 0) return li4[0];
                return null;
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
}