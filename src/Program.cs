﻿global using Serilog;
global using Flurl;
global using Flurl.Http;
global using LanguageExt;
global using static LanguageExt.Prelude;

using KanonBot;
using KanonBot.API;
using KanonBot.command_parser;
using KanonBot.Drivers;
using KanonBot.Event;
using KanonBot.functions.osu;
using KanonBot.Serializer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Msg = KanonBot.Message;



#region 初始化
Console.WriteLine("---KanonBot---");
var configPath = "config.toml";
if (File.Exists(configPath))
{
    Config.inner = Config.load(configPath);
}
else
{
    Config.inner = Config.Base.Default();
    Config.inner.save(configPath);
}
FlurlHttp.GlobalSettings.Redirects.Enabled = true;
FlurlHttp.GlobalSettings.Redirects.MaxAutoRedirects = 10;
FlurlHttp.GlobalSettings.Redirects.ForwardAuthorizationHeader = true;
FlurlHttp.GlobalSettings.Redirects.AllowSecureToInsecure = true;
var config = Config.inner!;


var myins = new MySql.Data.MySqlClient.MySqlConnection($"server={config.database.host};" +
            $"port={config.database.port};" +
            $"database={config.database.db};" +
            $"user={config.database.user};" +
            $"password={config.database.password};CharSet=utf8mb4;SslMode=none");

myins.Open();
myins.Close();

var log = new LoggerConfiguration()
                .WriteTo.Async(a => a.Console())
                .WriteTo.Async(a => a.File("logs/log-.log", rollingInterval: RollingInterval.Day));
if (config.debug)
    log = log.MinimumLevel.Debug();
Log.Logger = log.CreateLogger();
Log.Information("初始化成功 {@config}", config);

Log.Information("注册用户数据更新事件");
GeneralUpdate.DailyUpdate();
#endregion


var ExitEvent = new ManualResetEvent(false);
var drivers = new Drivers();
drivers.append(
    new OneBot.Server($"ws://0.0.0.0:{config.onebot?.serverPort}")
    .onMessage(async (target) =>
    {
        var api = (target.socket as OneBot.Server.Socket)!.api;
        Log.Information("← 收到消息 {0}", target.msg);
        Log.Debug("↑ 详情 {@0}", target.raw!);
        // Log.Debug("↑ 详情 {0}", Json.Serialize((target.raw! as OneBot.Models.CQMessageEventBase)!.MessageList));
        //switch (target.raw)
        //{
        //    case OneBot.Models.GroupMessage g:
        //        if (g.GroupId == config.onebot!.managementGroup)
        //        {
        //            await target.reply(target.msg);
        //        }
        //        break;
        //    case OneBot.Models.PrivateMessage p:
        //        await target.reply(target.msg);
        //        break;
        //}
        //var res = api.SendGroupMessage(xxxxx, target.msg);
        //Log.Information("→ 发送消息ID {@0}", res);
        await Universal.Parser(target);
    })
    .onEvent((client, e) =>
    {
        switch (e)
        {
            case HeartBeat h:
                Log.Debug("收到心跳包 {h}", h);
                break;
            case Ready l:
                Log.Debug("收到生命周期事件 {h}", l);
                break;
            case RawEvent r:
                Log.Debug("收到事件 {r}", r);
                break;
            default:
                break;
        }
    })
);
//drivers.append(
//    new OneBot.Client($"ws://{config.onebot?.host}:{config.onebot?.port}")
//);
drivers.append(
    new Guild(config.guild!.appID, config.guild.token!, Guild.Enums.Intent.GuildAtMessage | Guild.Enums.Intent.DirectMessages, config.guild.sandbox)
    .onMessage(async (target) =>
    {
        var api = (target.socket as Guild)!.api;
        var messageData = (target.raw as Guild.Models.MessageData)!;
        Log.Information("← 收到消息 {0}", target.msg);
        Log.Debug("↑ 详情 {@0}", messageData);
        Log.Debug("↑ 附件 {@0}", Json.Serialize(messageData.Attachments));
        // var res = api.SendMessage(messageData.ChannelID, new Guild.Models.SendMessageData() {
        //     MessageId = messageData.ID,
        //     MessageReference = new() { MessageId = messageData.ID }
        // }.Build(target.msg)).Result;
        // Log.Information("→ 发送消息ID {@0}", res);
        // await target.reply(target.msg);
        try
        {
            await Universal.Parser(target);
        }
        catch (Flurl.Http.FlurlHttpException ex)
        {
            Log.Error("请求 API 时发生异常，{0}", ex);
            await target.reply("请求 API 时发生异常");
        }
        catch (Exception ex)
        {
            Log.Error("发生未知错误，{0}", ex);
            await target.reply("发生未知错误");
        }
    })
    .onEvent((client, e) =>
    {
        switch (e)
        {
            case RawEvent r:
                var data = (r.value as Guild.Models.PayloadBase<JToken>)!;
                Log.Debug("收到事件: {@0} 数据: {1}", data, data.Data?.ToString(Formatting.None) ?? null);
                break;
            case Ready l:
                Log.Debug("收到生命周期事件 {h}", l);
                break;
            default:
                break;
        }
    })
);
drivers.append(
    new KanonBot.Drivers.Kook(config.kook!.token!, config.kook!.botID!)
    .onMessage(async (target) =>
    {
        await Universal.Parser(target);
    })
);
drivers.StartAll();
ExitEvent.WaitOne();
Log.CloseAndFlush();
