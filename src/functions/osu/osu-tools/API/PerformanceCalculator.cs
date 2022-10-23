﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using osu.Game.Rulesets.Catch.Difficulty;
using osu.Game.Rulesets.Osu.Difficulty;
using osu.Game.Rulesets.Taiko.Difficulty;
using System.Globalization;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using osu.Game.Skinning;
using osu.Game.Utils;
using KanonBot.LegacyImage;
using osu.Framework.Graphics.Containers;
using osu.Game.Scoring;
using osu.Game.Rulesets.Difficulty;
using static KanonBot.functions.osu.rosupp.PerformanceCalculator;
using System.Xml.Linq;

namespace KanonBot.osutools
{
    public static class PerformanceCalculator
    {
        public static Task<JObject> PPCalculator(string Beatmap, bool is_passed, string Accuracy, string MaxCombo,
            List<string> Mods, int Mode, JObject StatisticsJson, bool is_legacy_score = true, string? TotalScore = null)
        {
            int n100, n300;
            var workingBeatmap = ProcessorWorkingBeatmap.FromFileOrId(Beatmap);

            osu.Game.Rulesets.Difficulty.PerformanceCalculator performanceCalculator;
            osu.Game.Rulesets.Difficulty.DifficultyAttributes difficultyAttributes;
            osu.Game.Scoring.Score score = new();

            //score.ScoreInfo.Ruleset =
            //    workingBeatmap.BeatmapInfo.Ruleset.OnlineID != 0 ?
            //    LegacyHelper.GetRulesetFromLegacyID(Mode).RulesetInfo :
            //    workingBeatmap.BeatmapInfo.Ruleset;
            score.ScoreInfo.Ruleset = LegacyHelper.GetRulesetFromLegacyID(Mode).RulesetInfo;
            score.ScoreInfo.Ruleset.CreateInstance();

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

            //+CL　由于ppy的API不返回是否为CL成绩，所以一律加
            if (is_legacy_score)
                ModsList.Add(new JObject() {
                    { "acronym", "CL" },
                    { "settings", new JObject() }
                });

            score.ScoreInfo.ModsJson = ModsList.ToString();

            Console.WriteLine(score.ScoreInfo.ModsJson);

            performanceCalculator = score.ScoreInfo.Ruleset.CreateInstance().CreatePerformanceCalculator();

            //Trace.Assert(performanceCalculator != null);
            difficultyAttributes = score.ScoreInfo.Ruleset.CreateInstance().CreateDifficultyCalculator(workingBeatmap).Calculate(score.ScoreInfo.Mods.ToArray());
            PerformanceAttributes ppAttributes;

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
                { "Star", difficultyAttributes.StarRating.ToString("N4") },
                { "CS", CS.ToString("N4") },
                { "HP", HP.ToString("N4") }
            };

            switch (difficultyAttributes)
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


            //当前成绩
            JObject CurrentPPInfo = new();

            ppAttributes = performanceCalculator?.Calculate(score.ScoreInfo, difficultyAttributes)!;
            ppAttributeValues = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(ppAttributes)) ?? new Dictionary<string, object>();
            CurrentPPInfo = new()
                {
                    { "Total", ppAttributes.Total.ToString(CultureInfo.InvariantCulture) }
                };

            foreach (var kvp in ppAttributeValues)
            {
                CurrentPPInfo.Add(kvp.Key, kvp.Value.ToString());
            }


            CurrentPPInfo.Remove("OD");
            CurrentPPInfo.Remove("AR");
            CurrentPPInfo.Remove("Max Combo");
            Json.Add("CurrentPPInfo", CurrentPPInfo);

            //预测成绩
            if (Mode != 3)
            {
                JObject PredictivePPInfo = new();
                Dictionary<string, double> PPList = new()
                {
                    { "100", 1.00 },
                    { "99", 0.99 },
                    { "98", 0.98 },
                    { "97", 0.97 },
                    { "95", 0.95 },
                };
                JObject Statistics_more = new();
                foreach (KeyValuePair<string, double> PPName in PPList)
                {
                    score = new();
                    score.ScoreInfo.MaxCombo = int.Parse(Json["MaxCombo"]!.ToString());
                    score.ScoreInfo.Ruleset =
                        workingBeatmap.BeatmapInfo.Ruleset.OnlineID != 0 ?
                        LegacyHelper.GetRulesetFromLegacyID(Mode).RulesetInfo :
                        workingBeatmap.BeatmapInfo.Ruleset;
                    score.ScoreInfo.Ruleset.CreateInstance();
                    score.ScoreInfo.Accuracy = PPName.Value;
                    score.ScoreInfo.ModsJson = ModsList.ToString();

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
                    //score.ScoreInfo.Statistics.Clear();
                    //score.ScoreInfo.HitEvents.Clear();
                    score.ScoreInfo.StatisticsJson = Statistics_more.ToString();

                    performanceCalculator = score.ScoreInfo.Ruleset.CreateInstance().CreatePerformanceCalculator();

                    ppAttributes = performanceCalculator?.Calculate(score.ScoreInfo, difficultyAttributes)!;
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
                    PredictivePPInfo.Add(PPName.Key, Temp);
                }

                // FullCombo
                score = new();
                score.ScoreInfo.MaxCombo = int.Parse(Json["MaxCombo"]!.ToString());
                score.ScoreInfo.Ruleset =
                    workingBeatmap.BeatmapInfo.Ruleset.OnlineID != 0 ?
                    LegacyHelper.GetRulesetFromLegacyID(Mode).RulesetInfo :
                    workingBeatmap.BeatmapInfo.Ruleset;
                score.ScoreInfo.Ruleset.CreateInstance();
                score.ScoreInfo.ModsJson = ModsList.ToString();
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
                PredictivePPInfo.Add("FullCombo", Temp1);
                Json.Add("PredictivePPInfo", PredictivePPInfo);
            }
            //Console.WriteLine("\r\n\r\n\r\n\r\n\r\n" + Json.ToString());
            return Task.FromResult(Json);
        }
    }
}