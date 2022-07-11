using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Reflection;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KanonBot.API;
using KanonBot.Serializer;
using KanonBot.Drivers;
using KanonBot;
using Newtonsoft.Json;
using Serilog;
using Newtonsoft.Json.Linq;
using Msg = KanonBot.Message;
using Img = KanonBot.Image;
using SixLabors.ImageSharp;

namespace KanonBot.Tests;

[TestClass]
public class Misc
{
    public Misc()
    {
        var log = new LoggerConfiguration().WriteTo.Console();
        log = log.MinimumLevel.Warning();
        Log.Logger = log.CreateLogger();
        var configPath = "./config.toml";
        if (File.Exists(configPath))
        {
            Config.inner = Config.load(configPath);
        }
        else
        {
            System.IO.Directory.SetCurrentDirectory("../../../");
            Config.inner = Config.load(configPath);
        }
    }

    [TestMethod]
    public void UtilsTest()
    {
        Assert.AreEqual(Utils.GetObjectDescription(API.OSU.Enums.Mode.OSU), "osu");
    }

    [TestMethod]
    public void MsgChain()
    {
        var c = new Msg.Chain().msg("hello").image("C:\\hello.png", Msg.ImageSegment.Type.Url).msg("test\nhaha");
        c.Add(new Msg.RawSegment("Test", new JObject { { "test", "test" } }));
        Assert.IsTrue(c.StartsWith("he"));
        Assert.IsFalse(c.StartsWith("!"));
        c = new Msg.Chain().at("zhjk", Platform.OneBot);
        Assert.IsTrue(c.StartsWith(new Msg.AtSegment("zhjk", Platform.OneBot)));

        var c1 = OneBot.Message.Build(c);
        Assert.AreEqual(Json.Serialize(c1), "[{\"type\":\"at\",\"data\":{\"qq\":\"zhjk\"}}]");
        var c2 = OneBot.Message.Parse(c1);
        Assert.AreEqual(c2.ToString(), "<at;OneBot=zhjk>");
    }

    [TestMethod]
    public void Mail()
    {
        // 邮件测试
        KanonBot.Mail.MailStruct ms = new()
        {
            MailTo = new string[] { "deleted" },
            Subject = "你好！",
            Body = "你好！这是一封来自猫猫的测试邮件！"
        };
        KanonBot.Mail.Send(ms);
    }

    [TestMethod]
    public void Image()
    {
        // 自定义info图片测试
        Img.Helper helper = new();
        var lines = File.ReadLines("./TestFiles/ImageHelper示例文件.txt");
        foreach (string s in lines)
        {
            helper.Parse(s.Trim());
        }
        var image = helper.Build();
        image.Save("./TestFiles/ImageHelper示例文件.png");
    }
}

