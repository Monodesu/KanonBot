using System.Globalization;
using System.Text.RegularExpressions;

namespace KanonBot.functions.osubot;

public class Leeway_Calculator
{
    private const double DT = 1.5f;


    private const double HT = 0.75f;


    private const int HR = 16;


    private const int EZ = 2;


    private const int CIRCLE = 0;


    private const int SLIDER = 1;


    private const int SPINNER = 3;

    public double CalcRotations(int length, double adjustTime)
    {
        var rotations = 0d;
        var maxAccel = (double)(8E-05 + Math.Max(0f, 5000f - length) / 1000.0 / 2000.0) / adjustTime;
        var velocityCurrent = 0d;
        var temp = (int)(length - Math.Floor(16.666666666666668 * adjustTime));
        for (var i = 0; i < temp; i++)
        {
            velocityCurrent += maxAccel;
            rotations += (double)(Math.Min(velocityCurrent, 0.05) / 3.141592653589793);
        }

        return rotations;
    }


    public double CalcLeeway(int length, double adjustTime, double od, int difficultyModifier)
    {
        var rotReq = CalcRotReq(length, od, difficultyModifier);
        var thRot = CalcRotations(length, adjustTime);
        if (rotReq % 2 != 0 && Math.Floor(thRot) % 2.0 != 0.0)
            return thRot - Math.Floor(thRot) + 1.0;

        return thRot - Math.Floor(thRot);
    }

    public int CalcRotReq(int length, double od, int difficultyModifier)
    {
        switch (difficultyModifier)
        {
            case 16:
                od = Math.Min(10.0, od * 1.4);
                break;
            case 2:
                od /= 2.0;
                break;
        }

        var spinDiff = od > 5.0 ? 2.5 + 0.5 * od : 3.0 + 0.4 * od;
        return (int)(length / 1000f * spinDiff);
    }

    public string CalcAmount(int rotations, int rotReq)
    {
        double bonus = Math.Max(0, rotations - (rotReq + 3));

        if (rotReq % 2 != 0) return Math.Floor(bonus / 2.0) + "k (F)";

        if (bonus % 2.0 == 0.0)
            return bonus / 2.0 + "k (T)";
        return Math.Floor(bonus / 2.0) + "k+100 (T)";
    }

    public int CalcSpinBonus(int length, double od, double adjustTime, int difficultyModifier)
    {
        var maxRot = (int)CalcRotations(length, adjustTime);
        var rotReq = CalcRotReq(length, od, difficultyModifier);
        int bonus;
        if (rotReq % 2 == 0)
            bonus = (int)Math.Floor(maxRot / 2.0) * 100;
        else
            bonus = (rotReq + 3) / 2 * 100;
        return bonus + (int)Math.Floor((maxRot - (rotReq + 3)) / 2.0) * 1100;
    }

    public List<string> GetBeatmapHitObjects(string beatmap)
    {
        var line = beatmap.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        var hitObjects = new List<string>();
        for (var i = 0; i < line.Length; i++)
        {
            if (!line[i].Contains("HitObjects")) continue;

            for (var j = i + 1; j < line.Length; j++)
            {
                if (line[j].Length <= 1) break;

                hitObjects.Add(line[j]);
            }

            break;
        }

        return hitObjects;
    }

    public double GetHP(string beatmap)
    {
        return double.Parse(Regex.Match(beatmap, "HPDrainRate:(.*?)\n").Groups[1].Value, CultureInfo.InvariantCulture);
    }

    public double GetCS(string beatmap)
    {
        return double.Parse(Regex.Match(beatmap, "CircleSize:(.*?)\n").Groups[1].Value, CultureInfo.InvariantCulture);
    }

    public double GetOD(string beatmap)
    {
        return double.Parse(Regex.Match(beatmap, "OverallDifficulty:(.*?)\n").Groups[1].Value,
            CultureInfo.InvariantCulture);
    }


    public double GetSliderMult(string beatmap)
    {
        return double.Parse(Regex.Match(beatmap, "SliderMultiplier:(.*?)\n").Groups[1].Value,
            CultureInfo.InvariantCulture);
    }

    public double GetSliderTRate(string beatmap)
    {
        return double.Parse(Regex.Match(beatmap, "SliderTickRate:(.*?)\n").Groups[1].Value,
            CultureInfo.InvariantCulture);
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

    public double GetAdjustTime(string[] mods)
    {
        foreach (var mod in mods)
        {
            switch (mod)
            {
                case "DT":
                case "NC":
                    return 1.5f;

                case "HT":
                    return 0.75f;
            }
        }

        return 1.0f;
    }

    public int GetDifficultyModifier(string[] mods)
    {
        foreach (var mod in mods)
            switch (mod)
            {
                case "HR":
                    return 16;
                case "EZ":
                    return 2;
            }

        return 0;
    }

    public int GetBeatmapVersion(string beatmap)
    {
        return int.Parse(Regex.Match(beatmap, "osu file format v([0-9]+)").Groups[1].Value);
    }

    public List<int[]> GetSpinners(string beatmap)
    {
        var hitObjects = GetBeatmapHitObjects(beatmap);
        var timingPoints = GetTimingPoints(beatmap);
        var beatmapVersion = GetBeatmapVersion(beatmap);
        var sliderMult = GetSliderMult(beatmap);
        var sliderTRate = GetSliderTRate(beatmap);
        var spinners = new List<int[]>();
        var combo = 0;
        foreach (var hitObject in hitObjects)
        {
            var objData = hitObject.Split(new[]
            {
                ','
            });
            var objType = GetObjectType(int.Parse(objData[3]));
            switch (objType)
            {
                case 0:
                    combo++;
                    break;
                default:
                    switch (objType)
                    {
                        case 1:
                            {
                                var length = double.Parse(objData[7], CultureInfo.InvariantCulture);
                                var slides = int.Parse(objData[6]);
                                var beatLength = GetBeatLengthAt(int.Parse(objData[2]), timingPoints);
                                var tics = CalculateTickCount(length, slides, sliderMult, sliderTRate, beatLength[0], beatLength[1],
                                    beatmapVersion);
                                combo += tics + slides + 1;
                                break;
                            }
                        case 3:
                            spinners.Add(new[]
                            {
                                combo,
                                int.Parse(objData[5]) - int.Parse(objData[2])
                            });
                            combo++;
                            break;
                    }

                    break;
            }
        }

        return spinners;
    }

    public string GetModsString(string[] mods)
    {
        return mods.Aggregate("", (current, mod) => current + mod);
    }

    public int CalculateMaxScore(string beatmap, string[] mods)
    {
        double hp = GetHP(beatmap);
        double cs = GetCS(beatmap);
        double od = GetOD(beatmap);
        var beatmapVersion = GetBeatmapVersion(beatmap);
        var adjustTime = GetAdjustTime(mods);
        var difficultyModifier = GetDifficultyModifier(mods);
        var sliderMult = GetSliderMult(beatmap);
        var sliderTRate = GetSliderTRate(beatmap);
        var hitObjects = GetBeatmapHitObjects(beatmap);
        var startTime = int.Parse(hitObjects[0].Split(',')[2]);
        var endTime = int.Parse(hitObjects[hitObjects.Count - 1].Split(',')[2]);
        var timingPoints = GetTimingPoints(beatmap);
        var currentScore = 0;
        var currentCombo = 0;
        var bonusScore = 0;
        var drainLength = CalculateDrainTime(beatmap, startTime, endTime) / 1000;
        var difficulty = (int)Math.Round((hp + od + cs + Clamp(hitObjects.Count / (double)drainLength * 8f, 0f, 16f)) /
            38.0 * 5.0);
        var scoreMultipler = difficulty * CalculateModMultiplier(mods);
        foreach (var obj in hitObjects)
        {
            var objData = obj.Split(',');
            var objType = GetObjectType(int.Parse(objData[3]));
            if (objType == 0)
            {
                currentScore += 300 + (int)(Math.Max(0, currentCombo - 1) * (12.0 * scoreMultipler));
                currentCombo++;
            }

            switch (objType)
            {
                case 1:
                    {
                        var length = double.Parse(objData[7], CultureInfo.InvariantCulture);
                        var slides = int.Parse(objData[6]);
                        var beatLength = GetBeatLengthAt(int.Parse(objData[2]), timingPoints);
                        var tics = CalculateTickCount(length, slides, sliderMult, sliderTRate, beatLength[0], beatLength[1],
                            beatmapVersion);
                        bonusScore += tics * 10 + (slides + 1) * 30;
                        currentCombo += tics + slides + 1;
                        currentScore += 300 + (int)(Math.Max(0, currentCombo - 1) * (12.0 * scoreMultipler));
                        break;
                    }
                default:
                    {
                        if (objType != 3) continue;

                        currentScore += 300 + (int)(Math.Max(0, currentCombo - 1) * (12.0 * scoreMultipler));
                        var length2 = int.Parse(objData[5]) - int.Parse(objData[2]);
                        bonusScore += CalcSpinBonus(length2, od, adjustTime, difficultyModifier);
                        currentCombo++;
                        break;
                    }
            }
        }

        return currentScore + bonusScore;
    }

    public int CalculateTickCount(double length, int slides, double sliderMult, double sliderTRate, double beatLength,
        double sliderVMult, int beatmapVersion)
    {
        var sliderLength = Clamp(Math.Abs(sliderVMult), 10.0, 1000.0) * length * beatLength / (sliderMult * 10000.0);
        var tickLength = beatLength / sliderTRate;
        var flag = beatmapVersion < 8;
        if (flag) tickLength *= Clamp(Math.Abs(sliderVMult), 10.0, 1000.0) / 100.0;
        var tickTime = sliderLength - tickLength;
        var tics = 0;
        while (tickTime >= 10.0)
        {
            tics++;
            tickTime -= tickLength;
        }

        return tics + tics * (slides - 1);
    }

    public int CalculateDrainTime(string beatmap, int startTime, int endTime)
    {
        var line = beatmap.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        var breakPeriods = new List<int>();
        for (var i = 0; i < line.Length; i++)
        {
            if (!line[i].Contains("Break Periods")) continue;

            for (var j = i + 1; j < line.Length; j++)
            {
                var b = line[j].Split(',');
                if (b.Length != 3) break;
                breakPeriods.Add(int.Parse(b[2]) - int.Parse(b[1]));
            }

            break;
        }

        var drainTime = endTime - startTime;
        return breakPeriods.Aggregate(drainTime, (current, k) => current - k);
    }

    public List<double[]> GetTimingPoints(string beatmap)
    {
        var line = beatmap.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        var timingPoints = new List<double[]>();
        for (var i = 0; i < line.Length; i++)
        {
            if (!line[i].Contains("TimingPoints")) continue;

            for (var j = i + 1; j < line.Length; j++)
            {
                var tp = line[j].Split(',');
                if (tp.Length <= 1) break;

                var time = double.Parse(tp[0], CultureInfo.InvariantCulture);
                var beatLength = double.Parse(tp[1], CultureInfo.InvariantCulture);
                timingPoints.Add(new[]
                {
                    time,
                    beatLength
                });
            }

            break;
        }

        for (var index = 0; index < timingPoints.Count; index++)
        {
            var tp2 = timingPoints[index];

            if (!(tp2[1] > 0.0)) continue;

            timingPoints.Insert(0, new[]
            {
                0.0,
                tp2[1]
            });
            break;
        }

        return timingPoints;
    }

    public double[] GetBeatLengthAt(int time, List<double[]> timingPoints)
    {
        var beatLength = 0.0;
        var sliderVMult = -100.0;
        foreach (var t in timingPoints.Where(t => time >= t[0]))
            if (t[1] > 0.0)
            {
                beatLength = t[1];
                sliderVMult = -100.0;
            }
            else
            {
                sliderVMult = t[1];
            }

        return new[]
        {
            beatLength,
            sliderVMult
        };
    }

    public double Clamp(double value, double min, double max)
    {
        return value > max ? max : value < min ? min : value;
    }

    public int GetObjectType(int id)
    {
        var binary = "00000000" + Convert.ToString(id, 2);
        binary = binary.Substring(binary.Length - 8, 8);
        if (binary[4].Equals('1')) return 3;

        return binary[6].Equals('1') ? 1 : 0;
    }

    public string[] GetMods(string mods)
    {
        if (mods == null || mods.Length < 2 || mods.Length % 2 != 0)
            return new[]
            {
                "HD",
                "NC",
                "HR",
                "FL"
            };

        var mod = new string[mods.Length / 2];
        for (var i = 0; i < mod.Length; i++) mod[i] = mods.Substring(i * 2, 2);
        return mod;
    }

    public double CalculateModMultiplier(string[] mods)
    {
        return mods.Aggregate(1.0, (current, mod) => current * mod switch
        {
            "NF" => 0.5,
            "EZ" => 0.5,
            "HT" => 0.3,
            "HD" => 1.06,
            "HR" => 1.06,
            "DT" => 1.12,
            "NC" => 1.12,
            "FL" => 1.12,
            "SO" => 0.9,
            _ => 0.0
        });
    }
}
