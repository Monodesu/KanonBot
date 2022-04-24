using KanonBot.Message;
using KanonBot.Config;
using KanonBot.WebSocket;
using KanonBot.Drivers;

var ExitEvent = new ManualResetEvent(false);

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

// var c = new Chain().msg("hello").image("C:\\hello.png", Image.Type.file).msg("test\nhaha");
// c.append(new RawMessage("Test"));
// Console.WriteLine(c);


//////////////////////////////////////////////////////////////////////////////// test area start

// 自定义info图片测试
// KanonBot.KanonBotImage image = new();
// var lines = File.ReadLines("./test files/ImageHelper示例文件.txt");
// try
// {
//     foreach (string s in lines)
//     {
//         image.Parse(s.Trim());
//     }
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









Environment.Exit(0);
//////////////////////////////////////////////////////////////////////////////// test area end

#endregion

var drivers = new Drivers();
drivers.append(
    new CQ.Driver($"ws://{config.cqhttp?.host}:{config.cqhttp?.port}")
    .onMessage((client, msg) =>
    {
        Console.WriteLine(msg);
        // client.Send("xxxxx");
    })
    .onEvent((client, e) =>
    {
        Console.WriteLine(e);
        // client.Send("xxxxx");
    })
);
drivers.StartAll();
ExitEvent.WaitOne();
