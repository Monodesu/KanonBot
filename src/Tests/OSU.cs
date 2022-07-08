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
using Flurl.Http;

namespace KanonBot.Tests;

[TestClass]
public class OSU
{
    public OSU()
    {
        var log = new LoggerConfiguration().WriteTo.Console();
        log = log.MinimumLevel.Warning();
        Log.Logger = log.CreateLogger();
        var configPath = "../../../config.toml";
        Config.inner = Config.load(configPath);
    }

    [TestMethod]
    public void ModesTest()
    {
        Assert.AreEqual(API.OSU.Enums.Mode.Taiko.ToModeStr(), "taiko");
        Assert.AreEqual(API.OSU.Enums.Mode.Mania.ToModeNum(), 3);
        Assert.AreEqual(API.OSU.Enums.ParseMode("osu"), API.OSU.Enums.Mode.OSU);
        Assert.AreEqual(API.OSU.Enums.ParseMode("xasfasf"), null);
        Assert.AreEqual(API.OSU.Enums.ParseMode(2), API.OSU.Enums.Mode.Fruits);
        Assert.AreEqual(API.OSU.Enums.ParseMode(100), null);
    }

    [TestMethod]
    public void GetBeatmapAttr()
    {
        var res = API.OSU.GetBeatmapAttributes(3323074, new string[]{"HD", "DT"}, API.OSU.Enums.Mode.OSU).Result;
        Assert.IsTrue(res!.OverallDifficulty > 0);
        res = API.OSU.GetBeatmapAttributes(3323074, new string[]{"HD", "DT"}, API.OSU.Enums.Mode.Taiko).Result;
        // Assert.IsTrue(res!.StaminaDifficulty > 0);   // 不知道为啥taiko这里除了great_hit_window都是0
        Assert.IsTrue(res!.GreatHitWindow > 0);
        res = API.OSU.GetBeatmapAttributes(3323074, new string[]{"HD", "DT"}, API.OSU.Enums.Mode.Mania).Result;
        Assert.IsTrue(res!.ScoreMultiplier > 0);
        res = API.OSU.GetBeatmapAttributes(3323074, new string[]{"HD", "DT"}, API.OSU.Enums.Mode.Fruits).Result;
        Assert.IsTrue(res!.ApproachRate > 0);
        res = API.OSU.GetBeatmapAttributes(3323074000, new string[]{"HD", "DT"}, API.OSU.Enums.Mode.Fruits).Result;
        Assert.IsNull(res);
    }

    [TestMethod]
    public void GetBeatmap()
    {
        Assert.IsNull(API.OSU.GetBeatmap(332307400).Result);
        Assert.IsTrue(API.OSU.GetBeatmap(3323074).Result!.BeatmapId == 3323074);
    }

    [TestMethod]
    public void GetUser()
    {
        Assert.AreEqual(API.OSU.GetUser(9037287).Result!.Username, "Zh_Jk");
        Assert.AreEqual(API.OSU.GetUser("Zh_Jk").Result!.Id, 9037287);
        Assert.IsNull(API.OSU.GetUser("你谁啊").Result);
    }

    [TestMethod]
    public void GetUserScores()
    {
        // 查BP
        Assert.IsTrue(API.OSU.GetUserScores(9037287, API.OSU.Enums.UserScoreType.Best, API.OSU.Enums.Mode.OSU, 20, 0, false).Result!.Length == 20);
        Assert.IsNull(API.OSU.GetUserScores(903728700).Result);
    }

    [TestMethod]
    public void GetUserBeatmapScore()
    {
        // 查score
        Assert.IsTrue(API.OSU.GetUserBeatmapScore(9037287, 3323074, new string[]{"HD"}).Result!.Score.User!.Id == 9037287);
        Assert.IsNull(API.OSU.GetUserBeatmapScore(9037287000, 3323074, new string[]{"HD"}).Result);
        Assert.IsNull(API.OSU.GetUserBeatmapScore(9037287, 3323074, new string[]{"HR"}).Result);
    }

    [TestMethod]
    public void GetUserBeatmapScores()
    {
        Assert.IsTrue(API.OSU.GetUserBeatmapScores(9037287, 3657206).Result!.Length == 0);
        Assert.IsNull(API.OSU.GetUserBeatmapScores(9037287, 114514).Result);
    }

    [TestMethod]
    public void ppplus()
    {
        var res = API.OSU.GetUserPlusData(9037287).Result;
        Log.Warning("{@0}", res.User);
        Assert.IsTrue(res.User.UserId == 9037287);
    }
}

