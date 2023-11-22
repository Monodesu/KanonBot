using System.CommandLine;
using System.IO;
using KanonBot.Command;
using KanonBot.Drivers;
using KanonBot.Event;
using KanonBot.Serializer;
using LanguageExt.UnsafeValueAccess;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static KanonBot.Command.CommandSystem;
using static KanonBot.Command.CommandRegister;
using Msg = KanonBot.Message;
using static KanonBot.Drivers.OneBot.Models;

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

if (config.dev)
{
    var log = new LoggerConfiguration().WriteTo.Async(a => a.Console());
    log = log.MinimumLevel.Debug();
    Log.Logger = log.CreateLogger();
}
else
{
    var log = new LoggerConfiguration().WriteTo
        .Async(a => a.Console())
        .WriteTo.Async(a => a.File("logs/log-.log", rollingInterval: RollingInterval.Day));
    if (config.debug)
        log = log.MinimumLevel.Debug();
    Log.Logger = log.CreateLogger();
}

// 注册主指令列表
Register();

Log.Information("初始化成功 {@config}", config);



if (config.dev)
{
    var sender = parseInt(Environment.GetEnvironmentVariable("KANONBOT_TEST_USER_ID"));
    sender.IfNone(() =>
    {
        Log.Error("未设置测试环境变量 KANONBOT_TEST_USER_ID");
        Thread.Sleep(500);
        Environment.Exit(1);
    });

    while (true)
    {
        Log.Warning("请输入消息: ");
        var input = Console.ReadLine();
        if (string.IsNullOrEmpty(input)) return;
        Log.Warning("解析消息: {0}", input);
        await ProcessCommand(new Target()
        {
            msg = new Msg.Chain().msg(input!.Trim()),
            sender = $"{sender.Value()}",
            platform = Platform.OneBot,
            selfAccount = null,
            socket = new FakeSocket()
            {
                action = (msg) =>
                {
                    Log.Information("本地测试消息 {0}", msg);
                }
            },
            raw = new OneBot.Models.CQMessageEventBase()
            {
                UserId = sender.Value(),
            }
        });
    }
}



// 测试消息处理

while (true)
{
    Log.Warning("请输入消息: ");
    var input = Console.ReadLine();
    if (string.IsNullOrEmpty(input)) return;
    var sender = parseInt("123456789");
    Log.Warning("解析消息: {0}", input);
    await ProcessCommand(new Target()
    {

        msg = new Msg.Chain().msg(input!.Trim()),
        sender = $"{sender.Value()}",
        platform = Platform.OneBot,
        selfAccount = null,
        socket = new FakeSocket()
        {
            action = (msg) =>
            {
                Log.Information("本地测试消息 {0}", msg);
            }
        },
        raw = new OneBot.Models.CQMessageEventBase()
        {
            UserId = sender.Value(),
        }
    });
}


await Task.Delay(500);
Environment.Exit(0);


Log.Information("注册用户数据更新事件");
//GeneralUpdate.DailyUpdate();

#endregion


var ExitEvent = new ManualResetEvent(false);
var drivers = new Drivers()
    .append(
        new OneBot.Server($"ws://0.0.0.0:{config.onebot?.serverPort}")
            .onMessage(
                async (target) =>
                {
                    var api = (target.socket as OneBot.Server.Socket)!.api;
                    Log.Information("← 收到OneBot用户 {0} 的消息 {1}", target.sender, target.msg);
                    Log.Debug("↑ OneBot详情 {@0}", target.raw!);
                    try
                    {
                        await ProcessCommand(target);
                    }
                    finally
                    {
                        //Universal.reduplicateTargetChecker.TryUnlock(target);
                    }
                }
            )
            .onEvent(
                (client, e) =>
                {
                    switch (e)
                    {
                        case HeartBeat h:
                            Log.Debug("收到OneBot心跳包 {h}", h);
                            break;
                        case Ready l:
                            Log.Debug("收到OneBot生命周期事件 {h}", l);
                            break;
                        case RawEvent r:
                            Log.Debug("收到OneBot事件 {r}", r);
                            break;
                        default:
                            break;
                    }
                }
            )
    )
    .append(
        new Guild(
            config.guild!.appID,
            config.guild.token!,
            Guild.Enums.Intent.GuildAtMessage | Guild.Enums.Intent.DirectMessages,
            config.guild.sandbox
        )
            .onMessage(
                async (target) =>
                {
                    var api = (target.socket as Guild)!.api;
                    var messageData = (target.raw as Guild.Models.MessageData)!;
                    Log.Information("← 收到QQ Guild消息 {0}", target.msg);
                    Log.Debug("↑ QQ Guild详情 {@0}", messageData);
                    Log.Debug("↑ QQ Guild附件 {@0}", Json.Serialize(messageData.Attachments));
                    try
                    {
                        await ProcessCommand(target);
                    }
                    catch (Flurl.Http.FlurlHttpException ex)
                    {
                        Log.Error("请求 API 时发生异常<QQ Guild>，{0}", ex);
                        await target.reply("请求 API 时发生异常");
                    }
                    catch (Exception ex)
                    {
                        Log.Error("发生未知错误<QQ Guild>，{0}", ex);
                        await target.reply("发生未知错误");
                    }
                }
            )
            .onEvent(
                (client, e) =>
                {
                    switch (e)
                    {
                        case RawEvent r:
                            var data = (r.value as Guild.Models.PayloadBase<JToken>)!;
                            Log.Debug(
                                "收到QQ Guild事件: {@0} 数据: {1}",
                                data,
                                data.Data?.ToString(Formatting.None)
                            );
                            break;
                        case Ready l:
                            Log.Debug("收到QQ Guild生命周期事件 {h}", l);
                            break;
                        default:
                            break;
                    }
                }
            )
    )
    .append(
        new KanonBot.Drivers.Kook(config.kook!.token!, config.kook!.botID!).onMessage(
            async (target) =>
            {
                await ProcessCommand(target);
            }
        )
    )
    .StartAll();
ExitEvent.WaitOne();
Log.CloseAndFlush();
