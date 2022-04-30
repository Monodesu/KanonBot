using Img = KanonBot.Image;
using Msg = KanonBot.Message;
using KanonBot.Config;
using KanonBot.WebSocket;
using KanonBot.Drivers;

#region 加载配置文件
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
#endregion

#region 功能测试区

// Console.WriteLine(config);
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
    new CQ.Driver($"ws://{config.cqhttp?.host}:{config.cqhttp?.port}")
    .onMessage((target) =>
    {
        Console.WriteLine(target.msg);
        target.api.SendGroupMessage(195135404, target.msg);
    })
    .onEvent((client, e) =>
    {
    })
);
drivers.StartAll();
ExitEvent.WaitOne();
