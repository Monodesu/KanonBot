using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Reflection;
using System.ComponentModel;
using API = KanonBot.API;
using KanonBot.Serializer;
using KanonBot.Drivers;
using KanonBot;
using Newtonsoft.Json.Linq;
using Msg = KanonBot.Message;
using Img = KanonBot.Image;
using SixLabors.ImageSharp;

namespace Tests;

public class Misc
{
    private readonly ITestOutputHelper Output;
    public Misc(ITestOutputHelper Output)
    {
        this.Output = Output;
        var configPath = "./config.toml";
        if (File.Exists(configPath))
        {
            Config.inner = Config.load(configPath);
        }
        else
        {
            System.IO.Directory.SetCurrentDirectory("../../../../");
            Config.inner = Config.load(configPath);
        }
    }

    [Fact]
    public void Kaiheila()
    {
        var req = new KanonBot.Drivers.Kook.Models.MessageCreate
        {
            MessageType = KanonBot.Drivers.Kook.Enums.MessageType.Text,
            TargetId = "123",
            Content = "hi"
        };
        Output.WriteLine(Json.Serialize(req));
    }

    [Fact]
    public void UtilsTest()
    {
        Assert.Equal("osu", Utils.GetObjectDescription(API.OSU.Enums.Mode.OSU));
        Output.WriteLine(Utils.ForStarDifficulty(1.25).ToString());
        Output.WriteLine(Utils.ForStarDifficulty(2).ToString());
        Output.WriteLine(Utils.ForStarDifficulty(2.5).ToString());
        Output.WriteLine(Utils.ForStarDifficulty(3).ToString());
        Output.WriteLine(Utils.ForStarDifficulty(3.5).ToString());
    }

    [Fact]
    public void MsgChain()
    {
        var c = new Msg.Chain().msg("hello").image("C:\\hello.png", Msg.ImageSegment.Type.Url).msg("test\nhaha");
        c.Add(new Msg.RawSegment("Test", new JObject { { "test", "test" } }));
        Assert.True(c.StartsWith("he"));
        Assert.False(c.StartsWith("!"));
        c = new Msg.Chain().at("zhjk", Platform.OneBot);
        Assert.True(c.StartsWith(new Msg.AtSegment("zhjk", Platform.OneBot)));

        var c1 = OneBot.Message.Build(c);
        Assert.Equal("[{\"type\":\"at\",\"data\":{\"qq\":\"zhjk\"}}]", Json.Serialize(c1));
        var c2 = OneBot.Message.Parse(c1);
        Assert.Equal("<at;OneBot=zhjk>", c2.ToString());
    }

    // [Fact]
    // public void Mail()
    // {
    //     // 邮件测试
    //     KanonBot.Mail.MailStruct ms = new()
    //     {
    //         MailTo = new string[] { "deleted" },
    //         Subject = "你好！",
    //         Body = "你好！这是一封来自猫猫的测试邮件！"
    //     };
    //     KanonBot.Mail.Send(ms);
    // }
}

