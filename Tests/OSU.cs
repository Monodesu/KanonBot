using API = KanonBot.API;
using static KanonBot.API.OSUExtensions;
using KanonBot;
using KanonBot.API;
using KanonBot.LegacyImage;
using KanonBot.Serializer;
using KanonBot.functions.osu.rosupp;
using RosuPP;
using SixLabors.ImageSharp.Formats.Png;
using System.Runtime.InteropServices;

namespace Tests;

public class OSU
{
    private readonly ITestOutputHelper Output;
    public OSU(ITestOutputHelper Output)
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
    public void ScorePanelTest()
    {
        var score = API.OSU.GetUserBeatmapScore(1646397, 992512, new string[] { }, API.OSU.Enums.Mode.Mania).Result!;
        score.Score.Beatmapset = API.OSU.GetBeatmap(score.Score.Beatmap!.BeatmapId).Result!.Beatmapset!;
        API.OSU.BeatmapFileChecker(score.Score.Beatmap!.BeatmapId).Wait();
        Output.WriteLine("pp {0}", score.Score.PP);
        Output.WriteLine("acc {0}", score.Score.Accuracy);
        var data = PerformanceCalculator.CalculatePanelData(score.Score).Result;
        Output.WriteLine("cal pp {0}", data.ppInfo.ppStat.total);
        Output.WriteLine("cal data {0}", Json.Serialize(data.ppInfo));
        var img = Draw.DrawScore(data).Result;
        img.Save(new FileStream("./TestFiles/scoretest.png", FileMode.Create), new PngEncoder());
    }

    [Fact]
    public void PPTest()
    {
        var f = File.ReadAllBytes("./TestFiles/Kakichoco - Zan'ei (Lasse) [Illusion].osu");
        var beatmapData = GCHandle.Alloc(f, GCHandleType.Pinned);
        var cal = Calculator.New(new Sliceu8(beatmapData, (ulong)f.Length));
        beatmapData.Free();
        var p = ScoreParams.New();
        p.Mode(Mode.Osu);
        p.NKatu(0);
        p.NMisses(6);
        p.N100(29);
        p.N300(213);
        p.N50(0);
        var res = cal.Calculate(p.Context);
        // Rosu.debug_result(ref res);
        Output.WriteLine("{0}", Json.Serialize(res));
    }

    [Fact]
    public void ModesTest()
    {
        Assert.Equal("taiko", API.OSU.Enums.Mode.Taiko.ToModeStr());
        Assert.Equal(3, API.OSU.Enums.Mode.Mania.ToModeNum());
        Assert.Equal(API.OSU.Enums.Mode.OSU, API.OSU.Enums.ParseMode("osu"));
        Assert.Equal(API.OSU.Enums.Mode.Fruits, API.OSU.Enums.ParseMode(2));
        Assert.Null(API.OSU.Enums.ParseMode("xasfasf"));
        Assert.Null(API.OSU.Enums.ParseMode(100));
    }

    [Fact]
    public void GetBeatmapAttr()
    {
        var res = API.OSU.GetBeatmapAttributes(3323074, new string[] { "HD", "DT" }, API.OSU.Enums.Mode.OSU).Result;
        Assert.True(res!.OverallDifficulty > 0);
        res = API.OSU.GetBeatmapAttributes(3323074, new string[] { "HD", "DT" }, API.OSU.Enums.Mode.Taiko).Result;
        // Assert.IsTrue(res!.StaminaDifficulty > 0);   // 不知道为啥taiko这里除了great_hit_window都是0
        Assert.True(res!.GreatHitWindow > 0);
        res = API.OSU.GetBeatmapAttributes(3323074, new string[] { "HD", "DT" }, API.OSU.Enums.Mode.Mania).Result;
        Assert.True(res!.ScoreMultiplier > 0);
        res = API.OSU.GetBeatmapAttributes(3323074, new string[] { "HD", "DT" }, API.OSU.Enums.Mode.Fruits).Result;
        Assert.True(res!.ApproachRate > 0);
        res = API.OSU.GetBeatmapAttributes(3323074000, new string[] { "HD", "DT" }, API.OSU.Enums.Mode.Fruits).Result;
        Assert.Null(res);
    }

    [Fact]
    public void GetBeatmap()
    {
        Assert.Null(API.OSU.GetBeatmap(332307400).Result);
        Assert.True(API.OSU.GetBeatmap(3323074).Result!.BeatmapId == 3323074);
    }

    [Fact]
    public void GetUser()
    {
        Assert.Equal("Zh_Jk", API.OSU.GetUser(9037287).Result!.Username);
        Assert.Equal(9037287, API.OSU.GetUser("Zh_Jk").Result!.Id);
        Assert.Null(API.OSU.GetUser("你谁啊").Result);
    }

    [Fact]
    public void GetUserScores()
    {
        // 查BP
        Assert.True(API.OSU.GetUserScores(9037287, API.OSU.Enums.UserScoreType.Best, API.OSU.Enums.Mode.OSU, 20, 0, false).Result!.Length == 20);
        Assert.Null(API.OSU.GetUserScores(903728700).Result);
    }

    [Fact]
    public void GetUserBeatmapScore()
    {
        // 查score
        Assert.True(API.OSU.GetUserBeatmapScore(9037287, 3323074, new string[] { "HD" }).Result!.Score.User!.Id == 9037287);
        Assert.Null(API.OSU.GetUserBeatmapScore(9037287000, 3323074, new string[] { "HD" }).Result);
        Assert.Null(API.OSU.GetUserBeatmapScore(9037287, 3323074, new string[] { "HR" }).Result);
    }

    [Fact]
    public void GetUserBeatmapScores()
    {
        Assert.True(API.OSU.GetUserBeatmapScores(9037287, 3657206).Result!.Length == 0);
        Assert.Null(API.OSU.GetUserBeatmapScores(9037287, 114514).Result);
    }

    [Fact]
    public void ppplus()
    {
        var res = API.OSU.GetUserPlusData(9037287).Result;
        Assert.True(res.User.UserId == 9037287);
    }
}

