namespace KanonBot.osutools;

using KanonBot.LegacyImage;
using KanonBot.API;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

public class Calculator
{
    //public static string Calculate(string BeatmapPath, int Mode, bool is_passed, double Accuracy, int MaxCombo, List<string> Mods, int Great = 0, int Ok = 0, int Meh = 0,
    //int Miss = 0, int Geki = 0, int Katu = 0, bool is_more = false, string? TotalScore = null)

    public static async Task<JObject> CalculateAsync(KanonBot.API.OSU.Models.Score score)
    {
        var data = new Draw.ScorePanelData();
        data.scoreInfo = score;
        string BeatmapPath;

        try
        {
            // 下载谱面
            await OSU.BeatmapFileChecker(score.Beatmap!.BeatmapId);
            // 读取铺面
            BeatmapPath = $"./work/beatmap/{data.scoreInfo.Beatmap!.BeatmapId}.osu";
        }
        catch (Exception)
        {
            // 加载失败，删除重新抛异常
            File.Delete($"./work/beatmap/{data.scoreInfo.Beatmap!.BeatmapId}.osu");
            throw;
        }

        JObject Statistics = new();
        switch (score.Mode)
        {
            case API.OSU.Enums.Mode.OSU:
                Statistics = new()
                    {
                        { "Great", score.Statistics.CountGreat },
                        { "Ok", score.Statistics.CountOk },
                        { "Meh", score.Statistics.CountMeh },
                        { "Miss", score.Statistics.CountMiss },
                    };
                break;
            case API.OSU.Enums.Mode.Taiko:
                Statistics = new()
                    {
                        { "Great", score.Statistics.CountGreat },
                        { "Ok", score.Statistics.CountOk },
                        { "Miss", score.Statistics.CountMiss },
                    };
                break;
            case API.OSU.Enums.Mode.Fruits:
                Statistics = new()
                    {
                        { "Great", score.Statistics.CountGreat },
                        { "LargeTickHit", score.Statistics.CountOk },
                        { "SmallTickHit", score.Statistics.CountMeh },
                        { "SmallTickMiss", score.Statistics.CountKatu },
                        { "Miss", score.Statistics.CountMiss }
                    };
                break;
            case API.OSU.Enums.Mode.Mania:
                Statistics = new()
                    {
                        { "Great", score.Statistics.CountGreat },
                        { "Perfect", score.Statistics.CountGeki },
                        { "Good", score.Statistics.CountKatu },
                        { "Ok", score.Statistics.CountOk },
                        { "Meh", score.Statistics.CountMeh },
                        { "Miss", score.Statistics.CountMiss }
                    };
                break;
        }
        return await PerformanceCalculator.PPCalculator($"./work/beatmap/{score.Beatmap.BeatmapId}.osu", score.Passed, score.Accuracy.ToString(),
           score.MaxCombo.ToString(), score.Mods.ToList(), (int)score.Mode, Statistics,
           true, score.Scores.ToString());
    }
}