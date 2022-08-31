using System.Reflection.Metadata;
using Msg = KanonBot.Message;
using KanonBot.Drivers;
using KanonBot.Event;
using KanonBot;
using KanonBot.API;
using KanonBot.Serializer;
using KanonBot.command_parser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Flurl;
using Flurl.Http;

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

var config = Config.inner!;

var log = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/log-.log", rollingInterval: RollingInterval.Day);
if (config.debug)
    log = log.MinimumLevel.Debug();
Log.Logger = log.CreateLogger();
Log.Information("初始化成功 {@config}", config);
#endregion

var ExitEvent = new ManualResetEvent(false);
var drivers = new Drivers();
drivers.append(
    new OneBot.Server($"ws://0.0.0.0:{config.onebot?.serverPort}")
    .onMessage( (target) =>
    {
        var api = (target.socket as OneBot.Server.Socket)!.api;
        Log.Information("← 收到消息 {0}", target.msg);
        Log.Information("↑ 详情 {@0}", target.raw!);
        Log.Information("↑ 详情 {0}", Json.Serialize((target.raw! as OneBot.Models.CQMessageEventBase)!.MessageList));
        //switch (target.raw)
        //{
        //    case OneBot.Models.GroupMessage g:
        //        if (g.GroupId == config.onebot!.managementGroup)
        //        {
        //            target.reply(target.msg);
        //        }
        //        break;
        //    case OneBot.Models.PrivateMessage p:
        //        target.reply(target.msg);
        //        break;
        //}
        //var res = api.SendGroupMessage(xxxxx, target.msg);
        //Log.Information("→ 发送消息ID {@0}", res);
         Universal.Parser(target);
    })
    .onEvent((client, e) =>
    {
        switch (e)
        {
            case HeartBeat h:
                Log.Debug("收到心跳包 {h}", h);
                break;
            case Ready l:
                Log.Information("收到生命周期事件 {h}", l);
                break;
            case RawEvent r:
                Log.Information("收到事件 {r}", r);
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
        Log.Information("↑ 详情 {@0}", messageData);
        Log.Information("↑ 附件 {@0}", Json.Serialize(messageData.Attachments));
        // var res = api.SendMessage(messageData.ChannelID, new Guild.Models.SendMessageData() {
        //     MessageId = messageData.ID,
        //     MessageReference = new() { MessageId = messageData.ID }
        // }.Build(target.msg)).Result;
        // Log.Information("→ 发送消息ID {@0}", res);
        // target.reply(target.msg);
        try
        {
            await Universal.Parser(target);
        }
        catch (Flurl.Http.FlurlHttpException ex)
        {
            Log.Error("请求 API 时发生异常，{0}", ex);
            target.reply("请求 API 时发生异常");
        }
        catch (Exception ex)
        {
            Log.Error("发生未知错误，{0}", ex);
            target.reply("发生未知错误");
        }
    })
    .onEvent((client, e) =>
    {
        switch (e)
        {
            case RawEvent r:
                var data = (r.value as Guild.Models.PayloadBase<JToken>)!;
                Log.Information("收到事件: {@0} 数据: {1}", data, data.Data?.ToString(Formatting.None) ?? null);
                break;
            case Ready l:
                Log.Information("收到生命周期事件 {h}", l);
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
        try
        {
            await Universal.Parser(target);
        }
        catch (Flurl.Http.FlurlHttpException ex)
        {
            Log.Error("请求 API 时发生异常，{0}", ex);
            target.reply("请求 API 时发生异常");
        }
        catch (Exception ex)
        {
            Log.Error("发生未知错误，{0}", ex);
            target.reply("发生未知错误");
        }
    })
);
drivers.StartAll();
ExitEvent.WaitOne();
Log.CloseAndFlush();