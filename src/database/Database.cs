#pragma warning disable CS8602 // 解引用可能出现空引用。
#pragma warning disable IDE0044 // 添加只读修饰符
#pragma warning disable IDE1006 // 命名样式

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;
using Serilog;
using System.Data;

namespace KanonBot.Database;

public class Client
{
    private static Config.Config config = Config.Config.inner!;
    
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
}