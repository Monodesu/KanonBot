using System.Reflection.Metadata;
using Img = KanonBot.Image;
using Msg = KanonBot.Message;
using KanonBot.Event;
using KanonBot.Config;
using KanonBot.WebSocket;
using KanonBot.Drivers;
using Serilog;

#region 初始化
Console.WriteLine("---KanonBot---");
var configPath = "config.toml";
if (File.Exists(configPath))
{
    Config.inner = Config.load(configPath);
}
else
{
    Config.inner = Config.Default();
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

#region 功能测试区

// Console.WriteLine(config.ToJson());

// var c = new Msg.Chain().msg("hello").image("C:\\hello.png", Image.Type.file).msg("test\nhaha");
// c.append(new Msg.RawMessage("Test"));
// Console.WriteLine(c);


//////////////////////////////////////////////////////////////////////////////// test area start

// 自定义info图片测试
// Img.Helper helper = new();
// var lines = File.ReadLines("./test files/ImageHelper示例文件.txt");
// try
// {
//     foreach (string s in lines)
//     {
//         helper.Parse(s.Trim());
//     }
//     var image = helper.Build();
//     image.SaveAsFile("./test files/ImageHelper示例文件.png");
// }
// catch (Exception ex) { Console.WriteLine("图片生成失败：" + ex.Message); }



// 邮件测试
// KanonBot.Mail.MailStruct ms = new()
// {
//     MailTo = new string[] { "deleted" },
//     Subject = "你好！",
//     Body = "你好！这是一封来自猫猫的测试邮件！"
// };
// KanonBot.Mail.Send(ms); Environment.Exit(0);




// CQ.Message.Build(new Msg.Chain().msg("hello").image("C:\\hello.png", Msg.ImageSegment.Type.file).msg("test\nhaha"));
// Environment.Exit(0);
//////////////////////////////////////////////////////////////////////////////// test area end

#endregion

var ExitEvent = new ManualResetEvent(false);
var drivers = new Drivers();
drivers.append(
    new OneBot.Driver($"ws://{config.cqhttp?.host}:{config.cqhttp?.port}")
    .onMessage((target) =>
    {
        Log.Information("收到消息 {msg}", target.msg);
        Log.Debug("↑ 接上 {@raw}", target.raw);
        var res = target.api.SendGroupMessage(195135404, target.msg);
        Log.Debug("→ 发送消息ID {@res}", res);
    })
    .onEvent((client, e) =>
    {
        switch (e)
        {
            case HeartBeat h:
                Log.Debug("收到心跳包 {h}", h);
                break;
            case Lifecycle l:
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
drivers.StartAll();
ExitEvent.WaitOne();
Log.CloseAndFlush();
