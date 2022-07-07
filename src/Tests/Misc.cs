using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Reflection;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KanonBot.API;
using KanonBot.Serializer;
using KanonBot;
using Newtonsoft.Json;
using Serilog;

namespace KanonBot.Tests;

[TestClass]
public class Misc
{
    public Misc()
    {
        var log = new LoggerConfiguration().WriteTo.Console();
        log = log.MinimumLevel.Warning();
        Log.Logger = log.CreateLogger();
        // var configPath = "../../../config.toml";
        // Config.inner = Config.load(configPath);
    }

    [TestMethod]
    public void UtilsTest()
    {
        Assert.AreEqual(Utils.GetObjectDescription(API.OSU.Enums.Mode.OSU), "osu");
    }

}

