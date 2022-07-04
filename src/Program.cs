using System.Reflection.Metadata;
using Img = KanonBot.Image;
using Msg = KanonBot.Message;
using KanonBot.Drivers;
using KanonBot.Event;
using KanonBot;
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
//初始化kanonbot相关内容
KanonBot.API.Osu.CheckToken();

//结束
Log.Information("初始化成功 {@config}", config);
#endregion

#region 功能测试区
//////////////////////////////////////////////////////////////////////////////// test area start


// Log.Debug("{0}", config.ToJson());
//JArray j = new();
//KanonBot.src.API.Osu.BeapmapInfo b = KanonBot.src.API.Osu.GetBeatmap(3563009);
//Console.WriteLine(b.title);
//Console.WriteLine(b.playCount);
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
//     new OneBot.Server($"ws://0.0.0.0:{config.onebot?.serverPort}")
//     .onMessage((target) =>
//     {
//         var api = (target.socket as OneBot.Server.Socket)!.api;
//         Log.Information("← 收到消息 {0}", target.msg);
//         Log.Information("↑ 详情 {@0}", target.raw!);
//         Log.Information("↑ 详情 {0}", Json.Serialize((target.raw! as OneBot.Models.CQMessageEventBase)!.MessageList));
//         // switch (target.raw)
//         // {
//         //     case OneBot.Models.GroupMessage g:
//         //         if (g.GroupId == config.onebot!.managementGroup) 
//         //         {
//         //             target.reply(target.msg);
//         //         }
//         //         break;
//         //     case OneBot.Models.PrivateMessage p:
//         //         target.reply(target.msg);
//         //         break;
//         // }
//         // var res = api.SendGroupMessage(xxxxx, target.msg);
//         // Log.Information("→ 发送消息ID {@0}", res);
//         Universal.Parser(target);
//     })
//     .onEvent((client, e) =>
//     {
//         switch (e)
//         {
//             case HeartBeat h:
//                 Log.Debug("收到心跳包 {h}", h);
//                 break;
//             case Ready l:
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
// drivers.append(
//     new OneBot.Client($"ws://{config.ontbot?.host}:{config.ontbot?.port}")
// );
// drivers.append(
//     new Guild(config.guild!.appID, config.guild.token!, Guild.Enums.Intent.GuildAtMessage | Guild.Enums.Intent.DirectMessages, config.guild.sandbox)
//     .onMessage((target) =>
//     {
//         var api = (target.socket as Guild)!.api;
//         var messageData = (target.raw as Guild.Models.MessageData)!;
//         Log.Information("← 收到消息 {0}", target.msg);
//         Log.Information("↑ 详情 {@0}", messageData);
//         Log.Information("↑ 附件 {@0}", Json.Serialize(messageData.Attachments));
//         // var res = api.SendMessage(messageData.ChannelID, new Guild.Models.SendMessageData() {
//         //     MessageId = messageData.ID,
//         //     MessageReference = new() { MessageId = messageData.ID }
//         // }.Build(target.msg)).Result;
//         // Log.Information("→ 发送消息ID {@0}", res);
//         // target.reply(target.msg);
//         Universal.Parser(target);
//     })
//     .onEvent((client, e) =>
//     {
//         switch (e)
//         {
//             case RawEvent r:
//                 var data = (r.value as Guild.Models.PayloadBase<JToken>)!;
//                 Log.Information("收到事件: {@0} 数据: {1}", data, data.Data?.ToString(Formatting.None) ?? null);
//                 break;
//             case Ready l:
//                 Log.Information("收到生命周期事件 {h}", l);
//                 break;
//             default:
//                 break;
//         }
//     })
// );
drivers.append(
    new KOOK(config.kook!.token!, config.kook!.botID!)
);
drivers.StartAll();
ExitEvent.WaitOne();
Log.CloseAndFlush();