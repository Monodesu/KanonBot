namespace PerformanceCalculator;

using Newtonsoft.Json.Linq;
using PerformanceCalculator.API;

public class Calculator
{
       public static string Calculate(string BeatmapPath, int Mode, bool is_passed, double Accuracy, int MaxCombo, List<string> Mods, int Great = 0, int Ok = 0, int Meh = 0,
    int Miss = 0, int Geki = 0, int Katu = 0, bool is_more = false, string? TotalScore = null)
        {
            JObject Statistics = new();
            switch (Mode)
            {
                case 0:
                    Statistics = new()
                    {
                        { "Great", Great },
                        { "Ok", Ok },
                        { "Meh", Meh },
                        { "Miss", Miss },
                    };
                    break;
                case 1:
                    Statistics = new()
                    {
                        { "Great", Great },
                        { "Ok", Ok },
                        { "Miss", Miss },
                    };
                    break;
                case 2:
                    Statistics = new()
                    {
                        { "Great", Great },
                        { "LargeTickHit", Ok },
                        { "SmallTickHit", Meh },
                        { "SmallTickMiss", Katu },
                        { "Miss", Miss }
                    };
                    break;
                case 3:
                    Statistics = new()
                    {
                        { "Great", Great },
                        { "Perfect", Geki },
                        { "Good", Katu },
                        { "Ok", Ok },
                        { "Meh", Meh },
                        { "Miss", Miss }
                    };
                    break;
            }
            JObject Result = PerformanceCalculator.PPCalculator(BeatmapPath, is_passed, Accuracy.ToString(),
               MaxCombo.ToString(), Mods, Mode, Statistics,
               TotalScore, is_more);
            return Result.ToString();
        }
}
