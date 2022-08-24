using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using osu.Game.Rulesets.Catch.Difficulty;
using osu.Game.Rulesets.Osu.Difficulty;
using osu.Game.Rulesets.Taiko.Difficulty;
using System.Globalization;


namespace PerformanceCalculator.API
{
    public static class PerformanceCalculator
    {
        public static JObject PPCalculator(string Beatmap, bool is_passed, string Accuracy, string MaxCombo, List<string> Mods, int Mode, JObject StatisticsJson, string? TotalScore = null, bool IsMore = false)
        {
            int n100, n300;
            var workingBeatmap = ProcessorWorkingBeatmap.FromFileOrId(Beatmap);

            osu.Game.Rulesets.Difficulty.PerformanceCalculator performanceCalculator;
            osu.Game.Rulesets.Difficulty.DifficultyAttributes attributes;
            osu.Game.Scoring.Score score = new();

            score.ScoreInfo.Ruleset =
                workingBeatmap.BeatmapInfo.Ruleset.OnlineID != 0 ?
                LegacyHelper.GetRulesetFromLegacyID(Mode).RulesetInfo :
                workingBeatmap.BeatmapInfo.Ruleset;
            var ruleset = score.ScoreInfo.Ruleset.CreateInstance();

            score.ScoreInfo.Accuracy = double.Parse(Accuracy);
            score.ScoreInfo.MaxCombo = int.Parse(MaxCombo);
            score.ScoreInfo.TotalScore = TotalScore != null ? long.Parse(TotalScore) : 0;

            // std模式中没有pass的成绩，估算距当前acc最相近的数据 @Eiko Tokura
            if (!is_passed && Mode == 0)
            {
                n100 = (int)(1.50 * (1.00 - score.ScoreInfo.Accuracy) * (double)workingBeatmap.Beatmap.HitObjects.Count);
                n300 = workingBeatmap.Beatmap.HitObjects.Count - n100;
                StatisticsJson = new()
                {
                    { "Great", n300 },
                    { "Ok", n100 },
                    { "Meh", 0 },
                    { "Miss", 0 },
                };
            }
            score.ScoreInfo.StatisticsJson = StatisticsJson.ToString();

            // Mods
            JArray ModsList = new();
            foreach (string Mod in Mods)
            {
                ModsList.Add(new JObject() {
                    { "acronym", Mod },
                    { "settings", new JObject() }
                });
            }
            score.ScoreInfo.ModsJson = ModsList.ToString();

            performanceCalculator = score.ScoreInfo.Ruleset.CreateInstance().CreatePerformanceCalculator();

            //Trace.Assert(performanceCalculator != null);
            attributes = score.ScoreInfo.Ruleset.CreateInstance().CreateDifficultyCalculator(workingBeatmap).Calculate(score.ScoreInfo.Mods.ToArray());
            osu.Game.Rulesets.Difficulty.PerformanceAttributes ppAttributes;

            Dictionary<string, object> ppAttributeValues = new();
            double CS = workingBeatmap.BeatmapInfo.Difficulty.CircleSize;
            double HP = workingBeatmap.BeatmapInfo.Difficulty.DrainRate;

            if (Mods.Contains("HR"))
            {
                HP = (HP * 0.4) + HP;

                if (score.ScoreInfo.RulesetID == 0 || score.ScoreInfo.RulesetID == 2)
                {
                    CS = (CS * 0.3) + CS;
                }
            }
            else if (Mods.Contains("EZ"))
            {
                HP -= (HP * 0.5);

                if (score.ScoreInfo.RulesetID == 0 || score.ScoreInfo.RulesetID == 2)
                {
                    CS -= (CS * 0.5);
                }
            }

            JObject Json = new()
            {
                {
                    "Mods",
                    new JArray() {
                    score.ScoreInfo.Mods.Length > 0
                    ? score.ScoreInfo.Mods.Select(m => m.Acronym).Aggregate((c, n) => $"{c}, {n}")
                    : "None"
                }
                },
                { "Star", attributes.StarRating.ToString("N4") },
                { "CS", CS.ToString("N4") },
                { "HP", HP.ToString("N4") }
            };

            switch (attributes)
            {
                case OsuDifficultyAttributes osu:
                    Json.Add("Aim", osu.AimDifficulty.ToString("N4"));
                    Json.Add("Speed", osu.AimDifficulty.ToString("N4"));
                    Json.Add("MaxCombo", osu.MaxCombo);
                    Json.Add("AR", osu.ApproachRate.ToString("N4"));
                    Json.Add("OD", osu.OverallDifficulty.ToString("N4"));
                    break;

                case TaikoDifficultyAttributes taiko:
                    Json.Add("MaxCombo", taiko.MaxCombo);
                    Json.Add("HitWindow", taiko.GreatHitWindow.ToString("N4"));
                    break;

                case CatchDifficultyAttributes @catch:
                    Json.Add("MaxCombo", @catch.MaxCombo);
                    Json.Add("AR", @catch.ApproachRate.ToString("N4"));
                    break;
            }
            JObject PPInfo = new();
            if (IsMore)
            {
                Dictionary<string, double> PPList = new()
                {
                    { "100", 1 },
                    { "99", 0.99 },
                    { "98", 0.98 },
                    { "97", 0.97 },
                    { "95", 0.95 },
                };
                score.ScoreInfo.MaxCombo = int.Parse(Json["MaxCombo"]!.ToString());

                JObject Statistics_more = new();
                foreach (KeyValuePair<string, double> PPName in PPList)
                {
                    score.ScoreInfo.Accuracy = PPName.Value;
                    switch (Mode)
                    {
                        case 0:
                            n100 = (int)(1.50 * (1.00 - score.ScoreInfo.Accuracy) * (double)workingBeatmap.Beatmap.HitObjects.Count);
                            n300 = workingBeatmap.Beatmap.HitObjects.Count - n100;
                            Statistics_more = new()
                            {
                                { "Great", n300 },
                                { "Ok", n100 },
                                { "Meh", 0 },
                                { "Miss", 0 },
                            };
                            break;
                        case 1:
                            Statistics_more = new()
                            {
                                { "Great", int.Parse(StatisticsJson["Great"]!.ToString()) + int.Parse(StatisticsJson["Miss"]!.ToString()) },
                                { "Ok", int.Parse(StatisticsJson["Ok"]!.ToString()) },
                                { "Miss", 0 },
                            };
                            break;
                        case 2:
                            Statistics_more = new()
                            {
                                { "Great", int.Parse(StatisticsJson["Great"]!.ToString()) + int.Parse(StatisticsJson["Miss"]!.ToString()) },
                                { "LargeTickHit", int.Parse(StatisticsJson["Ok"]!.ToString()) },
                                { "SmallTickHit", int.Parse(StatisticsJson["Meh"]!.ToString()) + int.Parse(StatisticsJson["Katu"]!.ToString()) },
                                { "SmallTickMiss", 0 },
                                { "Miss", 0 }
                            };
                            break;
                        case 3:
                            //do nothing
                            break;
                    }
                    score.ScoreInfo.StatisticsJson = Statistics_more.ToString();

                    performanceCalculator = score.ScoreInfo.Ruleset.CreateInstance().CreatePerformanceCalculator();

                    ppAttributes = performanceCalculator?.Calculate(score.ScoreInfo, workingBeatmap)!;
                    ppAttributeValues = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(ppAttributes)) ?? new Dictionary<string, object>();

                    JObject Temp = new()
                    {
                        { "Total", ppAttributes.Total.ToString(CultureInfo.InvariantCulture) }
                    };

                    foreach (var kvp in ppAttributeValues)
                    {
                        Temp.Add(kvp.Key, kvp.Value.ToString());
                    }

                    Temp.Remove("OD");
                    Temp.Remove("AR");
                    Temp.Remove("Max Combo");
                    PPInfo.Add(PPName.Key, Temp);
                }

                // FullCombo
                score.ScoreInfo.Accuracy = double.Parse(Accuracy);
                switch (Mode)
                {
                    case 0:
                        n100 = (int)(1.50 * (1.00 - score.ScoreInfo.Accuracy) * (double)workingBeatmap.Beatmap.HitObjects.Count);
                        n300 = workingBeatmap.Beatmap.HitObjects.Count - n100;
                        Statistics_more = new()
                        {
                            { "Great", n300 },
                            { "Ok", n100 },
                            { "Meh", 0 },
                            { "Miss", 0 },
                        };
                        break;
                    case 1:
                        Statistics_more = new()
                        {
                            { "Great", int.Parse(StatisticsJson["Great"]!.ToString()) + int.Parse(StatisticsJson["Miss"]!.ToString()) },
                            { "Ok", int.Parse(StatisticsJson["Ok"]!.ToString()) },
                            { "Miss", 0 },
                        };
                        break;
                    case 2:
                        Statistics_more = new()
                        {
                            { "Great", int.Parse(StatisticsJson["Great"]!.ToString()) + int.Parse(StatisticsJson["Miss"]!.ToString()) },
                            { "LargeTickHit", int.Parse(StatisticsJson["Ok"]!.ToString()) },
                            { "SmallTickHit", int.Parse(StatisticsJson["Meh"]!.ToString()) + int.Parse(StatisticsJson["Katu"]!.ToString()) },
                            { "SmallTickMiss", 0 },
                            { "Miss", 0 }
                        };
                        break;
                    case 3:
                        //do nothing
                        break;
                }
                score.ScoreInfo.StatisticsJson = Statistics_more.ToString();
                performanceCalculator = score.ScoreInfo.Ruleset.CreateInstance().CreatePerformanceCalculator();

                ppAttributes = performanceCalculator?.Calculate(score.ScoreInfo, workingBeatmap)!;
                ppAttributeValues = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(ppAttributes)) ?? new Dictionary<string, object>();
                JObject Temp1 = new()
                {
                    { "Total", ppAttributes.Total.ToString(CultureInfo.InvariantCulture) }
                };

                foreach (var kvp in ppAttributeValues)
                    Temp1.Add(kvp.Key, kvp.Value.ToString());
                Temp1.Remove("OD");
                Temp1.Remove("AR");
                Temp1.Remove("Max Combo");
                PPInfo.Add("FullCombo", Temp1);
                Json.Add("PPInfo", PPInfo);
            }
            else
            {
                ppAttributes = performanceCalculator?.Calculate(score.ScoreInfo, workingBeatmap)!;
                ppAttributeValues = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(ppAttributes)) ?? new Dictionary<string, object>();
                PPInfo = new()
                {
                    { "Total", ppAttributes.Total.ToString(CultureInfo.InvariantCulture) }
                };

                foreach (var kvp in ppAttributeValues)
                {
                    PPInfo.Add(kvp.Key, kvp.Value.ToString());
                }


                PPInfo.Remove("OD");
                PPInfo.Remove("AR");
                PPInfo.Remove("Max Combo");
                Json.Add("PPInfo", PPInfo);
            }
            return Json;
        }
    }
}
