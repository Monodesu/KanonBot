using System.Runtime.InteropServices;
using KanonBot;
using KanonBot.API;
using KanonBot.Functions.OSU.RosuPP;
using KanonBot.Image.OSU;
using KanonBot.Serializer;
using RosuPP;
using SixLabors.ImageSharp.Formats.Png;
using static KanonBot.API.OSU.Extensions;
using API = KanonBot.API;
using Kook;
using KanonBot.API.OSU;

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
        var score = API.OSU.V2.GetUserBeatmapScore(1646397, 992512, new string[] { }, API.OSU.Enums.Mode.Mania).Result!;
        score.Score!.Beatmapset = API.OSU.V2.GetBeatmapAsync(score.Score.Beatmap!.BeatmapId).Result!.Beatmapset!;
        var attr = API.OSU.V2.GetBeatmapAttributes(score.Score.Beatmap!.BeatmapId, new string[] { }, API.OSU.Enums.Mode.Mania).Result;
        Output.WriteLine("beatmap attr {0}", Json.Serialize(attr));
        API.OSU.V2.BeatmapFileChecker(score.Score.Beatmap!.BeatmapId).Wait();
        Output.WriteLine("pp {0}", score.Score.PP);
        Output.WriteLine("acc {0}", score.Score.Accuracy);
        var data = PerformanceCalculator.CalculatePanelData(score.Score).Result;
        Output.WriteLine("cal pp {0}", data.ppInfo.ppStat.total);
        Output.WriteLine("cal data {0}", Json.Serialize(data.ppInfo));
        var img = OsuScorePanelV2.Draw(data).Result;
        img.Save(new FileStream("./TestFiles/scoretest.png", FileMode.Create), new PngEncoder());
    }

    [Fact]
    public void V2InfoPanelTest()
    {
        var osuinfo = API.OSU.V2.GetUser(9037287, API.OSU.Enums.Mode.OSU).Result;
        DataStructure.UserPanelData data = new();
        data.userInfo = osuinfo!;
        data.userInfo.PlayMode = API.OSU.Enums.Mode.OSU;
        data.prevUserInfo = data.userInfo;
        data.customMode = DataStructure.UserPanelData.CustomMode.Dark;
        var allBP = API.OSU.V2.GetUserScores(
            data.userInfo.Id,
            API.OSU.Enums.UserScoreType.Best,
            data.userInfo.PlayMode,
            100,
            0
        ).Result;
        var img = OsuInfoPanelV2.Draw(
            data,
            allBP!,
            OsuInfoPanelV2.InfoCustom.DarkDefault,
            false,
            false
        ).Result;
        img.Save(new FileStream("./TestFiles/info.png", FileMode.Create), new PngEncoder());
    }

    [Fact]
    public void PPTest()
    {
        var f = File.ReadAllBytes("./TestFiles/Kakichoco - Zan'ei (Lasse) [Illusion].osu");
        var beatmapData = GCHandle.Alloc(f, GCHandleType.Pinned);
        var cal = Calculator.New(new Sliceu8(beatmapData, (ulong)f.Length));
        beatmapData.Free();
        var p = ScoreParams.New();
        p.Mode(Mode.Mania);
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
        Assert.Equal("taiko", API.OSU.Enums.Mode.Taiko.ToStr());
        Assert.Equal(3, API.OSU.Enums.Mode.Mania.ToNum());
        Assert.Equal(API.OSU.Enums.Mode.OSU, API.OSU.Enums.String2Mode("osu"));
        Assert.Equal(API.OSU.Enums.Mode.Fruits, API.OSU.Enums.Int2Mode(2));
        Assert.Null(API.OSU.Enums.String2Mode("xasfasf"));
        Assert.Null(API.OSU.Enums.Int2Mode(100));
    }

    [Fact]
    public void GetBeatmapAttr()
    {
        var res = API.OSU.V2.GetBeatmapAttributes(3323074, new string[] { "HD", "DT" }, API.OSU.Enums.Mode.OSU).Result;
        Assert.True(res!.OverallDifficulty > 0);
        res = API.OSU.V2.GetBeatmapAttributes(3323074, new string[] { "HD", "DT" }, API.OSU.Enums.Mode.Taiko).Result;
        // Assert.IsTrue(res!.StaminaDifficulty > 0);   // 不知道为啥taiko这里除了great_hit_window都是0
        Assert.True(res!.GreatHitWindow > 0);
        res = API.OSU.V2.GetBeatmapAttributes(3323074, new string[] { "HD", "DT" }, API.OSU.Enums.Mode.Mania).Result;
        Assert.True(res!.MaxCombo > 0);
        res = API.OSU.V2.GetBeatmapAttributes(3323074, new string[] { "HD", "DT" }, API.OSU.Enums.Mode.Fruits).Result;
        Assert.True(res!.ApproachRate > 0);
        res = API.OSU.V2.GetBeatmapAttributes(3323074000, new string[] { "HD", "DT" }, API.OSU.Enums.Mode.Fruits).Result;
        Assert.Null(res);
    }

    [Fact]
    public void GetBeatmap()
    {
        Assert.Null(API.OSU.V2.GetBeatmapAsync(332307400).Result);
        Assert.True(API.OSU.V2.GetBeatmapAsync(3323074).Result!.BeatmapId == 3323074);
    }

    [Fact]
    public void GetUser()
    {
        Assert.Equal("Zh_Jk", API.OSU.V2.GetUser(9037287).Result!.Username);
        Assert.Equal(9037287, API.OSU.V2.GetUser("Zh_Jk").Result!.Id);
        Assert.Null(API.OSU.V2.GetUser("你谁啊").Result);
    }

    [Fact]
    public void GetUserScores()
    {
        // 查BP
        Assert.True(API.OSU.V2.GetUserScores(9037287, API.OSU.Enums.UserScoreType.Best, API.OSU.Enums.Mode.OSU, 20, 0, false).Result!.Length == 20);
        Assert.Null(API.OSU.V2.GetUserScores(903728700).Result);
    }

    [Fact]
    public void GetUserBeatmapScore()
    {
        // 查score
        Assert.True(API.OSU.V2.GetUserBeatmapScore(9037287, 3323074, ["HD"]).Result!.Score!.User!.Id == 9037287);
        Assert.Null(API.OSU.V2.GetUserBeatmapScore(9037287000, 3323074, ["HD"]).Result);
        Assert.Null(API.OSU.V2.GetUserBeatmapScore(9037287, 3323074, ["HR"]).Result);
        Assert.Null(API.OSU.V2.GetUserBeatmapScore(9037287, 850263, ["HD", "FL"]).Result);
        Assert.NotNull(API.OSU.V2.GetUserBeatmapScore(9037287, 850263, ["HD", "DT"]).Result);
    }

    [Fact]
    public void GetUserBeatmapScores()
    {
        Assert.True(API.OSU.V2.GetUserBeatmapScores(9037287, 3657206).Result!.Length == 0);
        Assert.Null(API.OSU.V2.GetUserBeatmapScores(9037287, 114514).Result);
    }

    [Fact]
    public void ppplus()
    {
        var res = API.OSU.V2.GetUserPlusData(9037287).Result;
        Assert.True(res.User!.UserId == 9037287);
    }
}

