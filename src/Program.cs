using System.Reflection.Metadata;
using Img = KanonBot.Image;
using Msg = KanonBot.Message;
using KanonBot.Drivers;
using KanonBot.Event;
using KanonBot;
using KanonBot.Serializer;
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

#region 功能测试区
//////////////////////////////////////////////////////////////////////////////// test area start


// Log.Debug("{0}", config.ToJson());


// var c = new Msg.Chain().msg("hello").image("C:\\hello.png", Msg.ImageSegment.Type.File).msg("test\nhaha");
// c.append(new Msg.RawMessage("Test", new JObject { { "test", "test" } }));
// Log.Debug("{0}", c);
// Log.Debug("{0}", c.StartsWith("he"));
// Log.Debug("{0}", c.StartsWith("!"));
// Log.Debug("{0}", c.StartsWith(""));

// var c1 = OneBot.Message.Build(c);
// Log.Debug("{0}", Json.Serialize(c1));
// var c2 = OneBot.Message.Parse(c1);
// Log.Debug("{0}", c2);



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
// catch (Exception ex) { Log.Error("图片生成失败：{0}", ex.Message); }



// 邮件测试
// KanonBot.Mail.MailStruct ms = new()
// {
//     MailTo = new string[] { "deleted" },
//     Subject = "你好！",
//     Body = "你好！这是一封来自猫猫的测试邮件！"
// };
// KanonBot.Mail.Send(ms);


// var result = await "https://sandbox.api.sgroup.qq.com"
//     .AppendPathSegment("gateway")
//     .WithHeader("Authorization", $"Bot {config.guild?.appID}.{config.guild?.token}")
//     .OnError((response) => { Log.Error("{@0}", response.Response.GetJsonAsync().Result); })
//     .GetJsonAsync<JObject>();
    
// Log.Debug("{0}", ((string?)result["url"]));


// Environment.Exit(0);
//////////////////////////////////////////////////////////////////////////////// test area end
#endregion


var ExitEvent = new ManualResetEvent(false);
var drivers = new Drivers();
// drivers.append(
//     new OneBot($"ws://{config.ontbot?.host}:{config.ontbot?.port}")
//     .onMessage((target) =>
//     {
//         Log.Information("← 收到消息 {msg}", target.msg);
//         Log.Debug("↑ 详情 {@raw}", target.raw);
//         var res = target.api!.SendGroupMessage(195135404, target.msg);
//         Log.Debug("→ 发送消息ID {@res}", res);
//     })
//     .onEvent((client, e) =>
//     {
//         switch (e)
//         {
//             case HeartBeat h:
//                 Log.Debug("收到心跳包 {h}", h);
//                 break;
//             case Lifecycle l:
//                 Log.Information("收到生命周期事件 {h}", l);
//                 break;
//             case RawEvent r:
//                 Log.Information("收到事件 {r}", r);
//                 break;
//             default:
//                 break;
//         }
        
//     })
// );
drivers.append(
    new Guild(config.guild!.appID, config.guild.token!, Guild.Enums.Intent.GuildMessages, config.guild.sandbox)
);
drivers.StartAll();
ExitEvent.WaitOne();
Log.CloseAndFlush();
