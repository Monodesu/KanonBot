using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace KanonBot.functions.osubot
{
    public class Leeway_Calculator
    {

        public float CalcRotations(int length, float adjustTime)
        {
            float rotations = 0f;
            float maxAccel = (float)(8E-05 + (double)Math.Max(0f, 5000f - (float)length) / 1000.0 / 2000.0) / adjustTime;
            float velocityCurrent = 0f;
            int temp = (int)((double)length - Math.Floor(16.666666666666668 * (double)adjustTime));
            for (int i = 0; i < temp; i++)
            {
                velocityCurrent += maxAccel;
                rotations += (float)(Math.Min((double)velocityCurrent, 0.05) / 3.141592653589793);
            }
            return rotations;
        }


        public double CalcLeeway(int length, float adjustTime, double od, int difficultyModifier)
        {
            int rotReq = this.CalcRotReq(length, od, difficultyModifier);
            float thRot = this.CalcRotations(length, adjustTime);
            bool flag = rotReq % 2 != 0 && Math.Floor((double)thRot) % 2.0 != 0.0;
            double result;
            if (flag)
            {
                result = (double)thRot - Math.Floor((double)thRot) + 1.0;
            }
            else
            {
                result = (double)thRot - Math.Floor((double)thRot);
            }
            return result;
        }

        public int CalcRotReq(int length, double od, int difficultyModifier)
        {
            bool flag = difficultyModifier == 16;
            if (flag)
            {
                od = Math.Min(10.0, od * 1.4);
            }
            else
            {
                bool flag2 = difficultyModifier == 2;
                if (flag2)
                {
                    od /= 2.0;
                }
            }
            bool flag3 = od > 5.0;
            double spinDiff;
            if (flag3)
            {
                spinDiff = 2.5 + 0.5 * od;
            }
            else
            {
                spinDiff = 3.0 + 0.4 * od;
            }
            return (int)((double)((float)length / 1000f) * spinDiff);
        }

        public string CalcAmount(int rotations, int rotReq)
        {
            double bonus = (double)Math.Max(0, rotations - (rotReq + 3));
            bool flag = rotReq % 2 != 0;
            string result;
            if (flag)
            {
                result = Math.Floor(bonus / 2.0) + "k (F)";
            }
            else
            {
                bool flag2 = bonus % 2.0 == 0.0;
                if (flag2)
                {
                    result = bonus / 2.0 + "k (T)";
                }
                else
                {
                    result = Math.Floor(bonus / 2.0) + "k+100 (T)";
                }
            }
            return result;
        }

        public int CalcSpinBonus(int length, double od, float adjustTime, int difficultyModifier)
        {
            int maxRot = (int)this.CalcRotations(length, adjustTime);
            int rotReq = this.CalcRotReq(length, od, difficultyModifier);
            bool flag = rotReq % 2 == 0;
            int bonus;
            if (flag)
            {
                bonus = (int)Math.Floor((double)maxRot / 2.0) * 100;
            }
            else
            {
                bonus = (rotReq + 3) / 2 * 100;
            }
            return bonus + (int)Math.Floor((double)(maxRot - (rotReq + 3)) / 2.0) * 1100;
        }

        public List<string> GetBeatmapHitObjects(string beatmap)
        {
            string[] line = beatmap.Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            List<string> hitObjects = new List<string>();
            for (int i = 0; i < line.Length; i++)
            {
                bool flag = line[i].Contains("HitObjects");
                if (flag)
                {
                    for (int j = i + 1; j < line.Length; j++)
                    {
                        bool flag2 = line[j].Length > 1;
                        if (!flag2)
                        {
                            break;
                        }
                        hitObjects.Add(line[j]);
                    }
                    break;
                }
            }
            return hitObjects;
        }

        public float GetHP(string beatmap)
        {
            return float.Parse(Regex.Match(beatmap, "HPDrainRate:(.*?)\n").Groups[1].Value, CultureInfo.InvariantCulture);
        }

        public float GetCS(string beatmap)
        {
            return float.Parse(Regex.Match(beatmap, "CircleSize:(.*?)\n").Groups[1].Value, CultureInfo.InvariantCulture);
        }

        public float GetOD(string beatmap)
        {
            return float.Parse(Regex.Match(beatmap, "OverallDifficulty:(.*?)\n").Groups[1].Value, CultureInfo.InvariantCulture);
        }


        public double GetSliderMult(string beatmap)
        {
            return double.Parse(Regex.Match(beatmap, "SliderMultiplier:(.*?)\n").Groups[1].Value, CultureInfo.InvariantCulture);
        }

        public double GetSliderTRate(string beatmap)
        {
            return double.Parse(Regex.Match(beatmap, "SliderTickRate:(.*?)\n").Groups[1].Value, CultureInfo.InvariantCulture);
        }

        public string GetTitle(string beatmap)
        {
            return Regex.Match(beatmap, "Title:(.*?)\n").Groups[1].Value.Trim();
        }

        public string GetArtist(string beatmap)
        {
            return Regex.Match(beatmap, "Artist:(.*?)\n").Groups[1].Value.Trim();
        }

        public string GetDifficultyName(string beatmap)
        {
            return Regex.Match(beatmap, "Version:(.*?)\n").Groups[1].Value.Trim();
        }

        public float GetAdjustTime(string[] mods)
        {
            int i = 0;
            while (i < mods.Length)
            {
                string mod = mods[i];
                bool flag = mod.Equals("DT") || mod.Equals("NC");
                float result;
                if (flag)
                {
                    result = 1.5f;
                }
                else
                {
                    bool flag2 = mod.Equals("HT");
                    if (!flag2)
                    {
                        i++;
                        continue;
                    }
                    result = 0.75f;
                }
                return result;
            }
            return 1f;
        }

        public int GetDifficultyModifier(string[] mods)
        {
            int i = 0;
            while (i < mods.Length)
            {
                string mod = mods[i];
                bool flag = mod.Equals("HR");
                int result;
                if (flag)
                {
                    result = 16;
                }
                else
                {
                    bool flag2 = mod.Equals("EZ");
                    if (!flag2)
                    {
                        i++;
                        continue;
                    }
                    result = 2;
                }
                return result;
            }
            return 0;
        }

        public int GetBeatmapVersion(string beatmap)
        {
            return int.Parse(Regex.Match(beatmap, "osu file format v([0-9]+)").Groups[1].Value);
        }

        public List<int[]> GetSpinners(string beatmap)
        {
            List<string> hitObjects = this.GetBeatmapHitObjects(beatmap);
            List<double[]> timingPoints = this.GetTimingPoints(beatmap);
            int beatmapVersion = this.GetBeatmapVersion(beatmap);
            double sliderMult = this.GetSliderMult(beatmap);
            double sliderTRate = this.GetSliderTRate(beatmap);
            List<int[]> spinners = new List<int[]>();
            int combo = 0;
            foreach (string hitObject in hitObjects)
            {
                string[] objData = hitObject.Split(new char[]
                {
                        ','
                });
                int objType = this.GetObjectType(int.Parse(objData[3]));
                bool flag = objType == 0;
                if (flag)
                {
                    combo++;
                }
                else
                {
                    bool flag2 = objType == 1;
                    if (flag2)
                    {
                        double length = double.Parse(objData[7], CultureInfo.InvariantCulture);
                        int slides = int.Parse(objData[6]);
                        double[] beatLength = this.GetBeatLengthAt(int.Parse(objData[2]), timingPoints);
                        int tics = this.CalculateTickCount(length, slides, sliderMult, sliderTRate, beatLength[0], beatLength[1], beatmapVersion);
                        combo += tics + slides + 1;
                    }
                    else
                    {
                        bool flag3 = objType == 3;
                        if (flag3)
                        {
                            spinners.Add(new int[]
                            {
                                    combo,
                                    int.Parse(objData[5]) - int.Parse(objData[2])
                            });
                            combo++;
                        }
                    }
                }
            }
            return spinners;
        }

        public string GetModsString(string[] mods)
        {
            string output = "";
            foreach (string mod in mods)
            {
                output += mod;
            }
            return output;
        }

        public int CalculateMaxScore(string beatmap, string[] mods)
        {
            double hp = (double)this.GetHP(beatmap);
            double cs = (double)this.GetCS(beatmap);
            double od = (double)this.GetOD(beatmap);
            int beatmapVersion = this.GetBeatmapVersion(beatmap);
            float adjustTime = this.GetAdjustTime(mods);
            int difficultyModifier = this.GetDifficultyModifier(mods);
            double sliderMult = this.GetSliderMult(beatmap);
            double sliderTRate = this.GetSliderTRate(beatmap);
            List<string> hitObjects = this.GetBeatmapHitObjects(beatmap);
            int startTime = int.Parse(hitObjects[0].Split(new char[]
            {
                    ','
            })[2]);
            int endTime = int.Parse(hitObjects[hitObjects.Count - 1].Split(new char[]
            {
                    ','
            })[2]);
            List<double[]> timingPoints = this.GetTimingPoints(beatmap);
            int currentScore = 0;
            int currentCombo = 0;
            int bonusScore = 0;
            int drainLength = this.CalculateDrainTime(beatmap, startTime, endTime) / 1000;
            int difficulty = (int)Math.Round((hp + od + cs + (double)this.Clamp((float)hitObjects.Count / (float)drainLength * 8f, 0f, 16f)) / 38.0 * 5.0);
            double scoreMultipler = (double)difficulty * this.CalculateModMultiplier(mods);
            int sliderCount = 0;
            foreach (string obj in hitObjects)
            {
                string[] objData = obj.Split(new char[]
                {
                        ','
                });
                int objType = this.GetObjectType(int.Parse(objData[3]));
                bool flag = objType == 0;
                if (flag)
                {
                    sliderCount++;
                    currentScore += 300 + (int)((double)Math.Max(0, currentCombo - 1) * (12.0 * scoreMultipler));
                    currentCombo++;
                }
                bool flag2 = objType == 1;
                if (flag2)
                {
                    double length = double.Parse(objData[7], CultureInfo.InvariantCulture);
                    int slides = int.Parse(objData[6]);
                    double[] beatLength = this.GetBeatLengthAt(int.Parse(objData[2]), timingPoints);
                    int tics = this.CalculateTickCount(length, slides, sliderMult, sliderTRate, beatLength[0], beatLength[1], beatmapVersion);
                    bonusScore += tics * 10 + (slides + 1) * 30;
                    currentCombo += tics + slides + 1;
                    currentScore += 300 + (int)((double)Math.Max(0, currentCombo - 1) * (12.0 * scoreMultipler));
                }
                else
                {
                    bool flag3 = objType == 3;
                    if (flag3)
                    {
                        currentScore += 300 + (int)((double)Math.Max(0, currentCombo - 1) * (12.0 * scoreMultipler));
                        int length2 = int.Parse(objData[5]) - int.Parse(objData[2]);
                        bonusScore += this.CalcSpinBonus(length2, od, adjustTime, difficultyModifier);
                        currentCombo++;
                    }
                }
            }
            return currentScore + bonusScore;
        }

        public int CalculateTickCount(double length, int slides, double sliderMult, double sliderTRate, double beatLength, double sliderVMult, int beatmapVersion)
        {
            double sliderLength = this.Clamp(Math.Abs(sliderVMult), 10.0, 1000.0) * length * beatLength / (sliderMult * 10000.0);
            double tickLength = beatLength / sliderTRate;
            bool flag = beatmapVersion < 8;
            if (flag)
            {
                tickLength *= this.Clamp(Math.Abs(sliderVMult), 10.0, 1000.0) / 100.0;
            }
            double tickTime = sliderLength - tickLength;
            int tics = 0;
            while (tickTime >= 10.0)
            {
                tics++;
                tickTime -= tickLength;
            }
            return tics + tics * (slides - 1);
        }

        public int CalculateDrainTime(string beatmap, int startTime, int endTime)
        {
            string[] line = beatmap.Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            List<int> breakPeriods = new List<int>();
            for (int i = 0; i < line.Length; i++)
            {
                bool flag = line[i].Contains("Break Periods");
                if (flag)
                {
                    for (int j = i + 1; j < line.Length; j++)
                    {
                        string[] b = line[j].Split(new char[]
                        {
                                ','
                        });
                        bool flag2 = b.Length == 3;
                        if (!flag2)
                        {
                            break;
                        }
                        breakPeriods.Add(int.Parse(b[2]) - int.Parse(b[1]));
                    }
                    break;
                }
            }
            int drainTime = endTime - startTime;
            foreach (int k in breakPeriods)
            {
                drainTime -= k;
            }
            return drainTime;
        }

        public List<double[]> GetTimingPoints(string beatmap)
        {
            string[] line = beatmap.Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            List<double[]> timingPoints = new List<double[]>();
            for (int i = 0; i < line.Length; i++)
            {
                bool flag = line[i].Contains("TimingPoints");
                if (flag)
                {
                    for (int j = i + 1; j < line.Length; j++)
                    {
                        string[] tp = line[j].Split(new char[]
                        {
                                ','
                        });
                        bool flag2 = tp.Length > 1;
                        if (!flag2)
                        {
                            break;
                        }
                        double time = double.Parse(tp[0], CultureInfo.InvariantCulture);
                        double beatLength = double.Parse(tp[1], CultureInfo.InvariantCulture);
                        timingPoints.Add(new double[]
                        {
                                time,
                                beatLength
                        });
                    }
                    break;
                }
            }
            foreach (double[] tp2 in timingPoints)
            {
                bool flag3 = tp2[1] > 0.0;
                if (flag3)
                {
                    timingPoints.Insert(0, new double[]
                    {
                            0.0,
                            tp2[1]
                    });
                    break;
                }
            }
            return timingPoints;
        }

        public double[] GetBeatLengthAt(int time, List<double[]> timingPoints)
        {
            double beatLength = 0.0;
            double sliderVMult = -100.0;
            for (int i = 0; i < timingPoints.Count; i++)
            {
                bool flag = (double)time >= timingPoints[i][0];
                if (flag)
                {
                    bool flag2 = timingPoints[i][1] > 0.0;
                    if (flag2)
                    {
                        beatLength = timingPoints[i][1];
                        sliderVMult = -100.0;
                    }
                    else
                    {
                        sliderVMult = timingPoints[i][1];
                    }
                }
            }
            return new double[]
            {
                    beatLength,
                    sliderVMult
            };
        }

        public double Clamp(double value, double min, double max)
        {
            bool flag = value < min;
            double result;
            if (flag)
            {
                result = min;
            }
            else
            {
                bool flag2 = value > max;
                if (flag2)
                {
                    result = max;
                }
                else
                {
                    result = value;
                }
            }
            return result;
        }

        public float Clamp(float value, float min, float max)
        {
            bool flag = value < min;
            float result;
            if (flag)
            {
                result = min;
            }
            else
            {
                bool flag2 = value > max;
                if (flag2)
                {
                    result = max;
                }
                else
                {
                    result = value;
                }
            }
            return result;
        }

        public int GetObjectType(int id)
        {
            string binary = "00000000" + Convert.ToString(id, 2);
            binary = binary.Substring(binary.Length - 8, 8);
            bool flag = binary[4].Equals('1');
            int result;
            if (flag)
            {
                result = 3;
            }
            else
            {
                bool flag2 = binary[6].Equals('1');
                if (flag2)
                {
                    result = 1;
                }
                else
                {
                    result = 0;
                }
            }
            return result;
        }

        public string[] GetMods(string mods)
        {
            bool flag = mods == null || mods.Length < 2 || mods.Length % 2 != 0;
            string[] result;
            if (flag)
            {
                result = new string[]
                {
                        "HD",
                        "NC",
                        "HR",
                        "FL"
                };
            }
            else
            {
                string[] mod = new string[mods.Length / 2];
                for (int i = 0; i < mod.Length; i++)
                {
                    mod[i] = mods.Substring(i * 2, 2);
                }
                result = mod;
            }
            return result;
        }

        public bool IsValidModCombo(string[] mods)
        {
            bool flag = mods.Equals(new string[]
            {
                    "HD",
                    "DT",
                    "HR",
                    "FL"
            }) || mods.Equals(new string[]
            {
                    "HD",
                    "NC",
                    "HR",
                    "FL"
            });
            bool result;
            if (flag)
            {
                result = true;
            }
            else
            {
                for (int i = 0; i < mods.Length; i++)
                {
                    bool flag2 = !this.IsValidMod(mods[i]);
                    if (flag2)
                    {
                        return false;
                    }
                    bool flag3 = mods[i].Equals("NM") && mods.Length > 2;
                    if (flag3)
                    {
                        return false;
                    }
                    bool flag4 = i + 1 < mods.Length;
                    if (flag4)
                    {
                        bool flag5 = mods[i].Equals("DT");
                        if (flag5)
                        {
                            for (int j = i + 1; j < mods.Length; j++)
                            {
                                bool flag6 = mods[j].Equals("NC") || mods[j].Equals("HT");
                                if (flag6)
                                {
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            bool flag7 = mods[i].Equals("NC");
                            if (flag7)
                            {
                                for (int k = i + 1; k < mods.Length; k++)
                                {
                                    bool flag8 = mods[k].Equals("DT") || mods[k].Equals("HT");
                                    if (flag8)
                                    {
                                        return false;
                                    }
                                }
                            }
                            else
                            {
                                bool flag9 = mods[i].Equals("HT");
                                if (flag9)
                                {
                                    for (int l = i + 1; l < mods.Length; l++)
                                    {
                                        bool flag10 = mods[l].Equals("DT") || mods[l].Equals("NC");
                                        if (flag10)
                                        {
                                            return false;
                                        }
                                    }
                                }
                                else
                                {
                                    bool flag11 = mods[i].Equals("HR");
                                    if (flag11)
                                    {
                                        for (int m = i + 1; m < mods.Length; m++)
                                        {
                                            bool flag12 = mods[m].Equals("EZ");
                                            if (flag12)
                                            {
                                                return false;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        bool flag13 = mods[i].Equals("EZ");
                                        if (flag13)
                                        {
                                            for (int n = i + 1; n < mods.Length; n++)
                                            {
                                                bool flag14 = mods[n].Equals("HR");
                                                if (flag14)
                                                {
                                                    return false;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                result = true;
            }
            return result;
        }

        public bool IsValidMod(string mod)
        {
            return mod.Equals("NF") || mod.Equals("EZ") || mod.Equals("HT") || mod.Equals("HD") || mod.Equals("HR") || mod.Equals("DT") || mod.Equals("NC") || mod.Equals("FL") || mod.Equals("NM");
        }

        public string RemoveUselessMods(string mods)
        {
            bool flag = mods.Equals("None") || mods.Equals("SD") || mods.Equals("PF") || mods.Equals("TD") || mods.Equals("SO");
            string result;
            if (flag)
            {
                result = "NM";
            }
            else
            {
                bool flag2 = mods.Length == 2;
                if (flag2)
                {
                    result = mods;
                }
                else
                {
                    string output = "";
                    foreach (string mod in mods.Split(new char[]
                    {
                            ','
                    }))
                    {
                        bool flag3 = mod.Equals("HD") || mod.Equals("DT") || mod.Equals("NC") || mod.Equals("HR") || mod.Equals("FL") || mod.Equals("EZ") || mod.Equals("HT");
                        if (flag3)
                        {
                            output += mod;
                        }
                    }
                    bool flag4 = string.IsNullOrEmpty(output);
                    if (flag4)
                    {
                        result = "NM";
                    }
                    else
                    {
                        result = output;
                    }
                }
            }
            return result;
        }

        public double CalculateModMultiplier(string[] mods)
        {
            double multiplier = 1.0;
            foreach (string mod in mods)
            {
                bool flag = mod.Equals("NF") || mod.Equals("EZ");
                if (flag)
                {
                    multiplier *= 0.5;
                }
                else
                {
                    bool flag2 = mod.Equals("HT");
                    if (flag2)
                    {
                        multiplier *= 0.3;
                    }
                    else
                    {
                        bool flag3 = mod.Equals("HD") || mod.Equals("HR");
                        if (flag3)
                        {
                            multiplier *= 1.06;
                        }
                        else
                        {
                            bool flag4 = mod.Equals("DT") || mod.Equals("NC") || mod.Equals("FL");
                            if (flag4)
                            {
                                multiplier *= 1.12;
                            }
                            else
                            {
                                bool flag5 = mod.Equals("SO");
                                if (flag5)
                                {
                                    multiplier *= 0.9;
                                }
                                else
                                {
                                    multiplier *= 0.0;
                                }
                            }
                        }
                    }
                }
            }
            return multiplier;
        }

        public bool IsSameMods(string[] mods1, string[] mods2)
        {
            bool flag = mods1.Length != mods2.Length;
            bool result;
            if (flag)
            {
                result = false;
            }
            else
            {
                for (int i = 0; i < mods1.Length; i++)
                {
                    bool flag2 = mods1[i].Equals("DT") || mods1[i].Equals("NC");
                    if (flag2)
                    {
                        bool flag3 = !mods2.Contains("DT") && !mods2.Contains("NC");
                        if (flag3)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        bool flag4 = !mods2.Contains(mods1[i]);
                        if (flag4)
                        {
                            return false;
                        }
                    }
                }
                result = true;
            }
            return result;
        }

        private const float DT = 1.5f;


        private const float HT = 0.75f;


        private const int HR = 16;


        private const int EZ = 2;


        private const int CIRCLE = 0;


        private const int SLIDER = 1;


        private const int SPINNER = 3;

    }
}
