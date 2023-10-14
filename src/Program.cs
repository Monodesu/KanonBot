using System.IO;
using KanonBot.command_parser;
using KanonBot.Drivers;
using KanonBot.Event;
using KanonBot.Functions.OSU;
using KanonBot.Functions.OSU.RosuPP;
using KanonBot.Serializer;
using LanguageExt.UnsafeValueAccess;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RosuPP;
using SixLabors.ImageSharp.Diagnostics;
using API = KanonBot.API;
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
Log.Information("初始化成功 {@config}", config);

if (config.dev)
{
    // var res = API.OSU.SearchBeatmap("exit this ato").Result;
    // Log.Information("{@res}", res);
    // var score = API.OSU.GetUserBeatmapScore(1646397, 992512, new string[] { }, API.OSU.Enums.Mode.Mania).Result!;
    // score.Score.Beatmapset = API.OSU.GetBeatmap(score.Score.Beatmap!.BeatmapId).Result!.Beatmapset!;
    // var attr = API.OSU.GetBeatmapAttributes(score.Score.Beatmap!.BeatmapId, new string[] { }, API.OSU.Enums.Mode.Mania).Result;
    // Console.WriteLine("beatmap attr {0}", Json.Serialize(attr));
    // API.OSU.BeatmapFileChecker(score.Score.Beatmap!.BeatmapId).Wait();
    // Console.WriteLine("pp {0}", score.Score.PP);
    // Console.WriteLine("acc {0}", score.Score.Accuracy);
    // var data = PerformanceCalculator.CalculatePanelData(score.Score).Result;
    // Console.WriteLine("cal pp {0}", data.ppInfo.ppStat.total);
    // Console.WriteLine("cal data {0}", Json.Serialize(data.ppInfo));

    var sender = parseInt(Environment.GetEnvironmentVariable("KANONBOT_TEST_USER_ID"));
    sender.IfNone(() =>
    {
        Log.Error("未设置测试环境变量 KANONBOT_TEST_USER_ID");
        Thread.Sleep(500);
        Environment.Exit(1);
    });

    while (true) {
        Log.Warning("请输入消息: ");
        var input = Console.ReadLine();
        if (string.IsNullOrEmpty(input)) return;
        Log.Warning("解析消息: {0}", input);
        await Universal.Parser(new Target()
        {
            msg = new Msg.Chain().msg(input!.Trim()),
            sender = $"{sender.Value()}",
            platform = Platform.OneBot,
            selfAccount = null,
            socket = new FakeSocket() {
                action = (msg) => {
                    Log.Information("本地测试消息 {0}", msg);
                }
            },
            raw = new OneBot.Models.CQMessageEventBase() {
                UserId = sender.Value(),
            }
        });
    }
}

Log.Information("注册用户数据更新事件");
GeneralUpdate.DailyUpdate();

//这个东西很占资源，先注释了
//MemoryDiagnostics.UndisposedAllocation += allocationStackTrace =>
//{
//    Log.Warning($@"Undisposed allocation detected at:{Environment.NewLine}{allocationStackTrace}");
//};

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
                        await Universal.Parser(target);
                    }
                    finally
                    {
                        Universal.reduplicateTargetChecker.TryUnlock(target);
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
                                data.Data?.ToString(Formatting.None) ?? null
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
                await Universal.Parser(target);
            }
        )
    )
    .StartAll();
ExitEvent.WaitOne();
Log.CloseAndFlush();
