using RosuPP;
using SixLabors.ImageSharp.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanonBot.functions.osu.rosupp
{
    public static class PerformanceCalculator
    {
        public static List<string> mods_str = new(){ "NF", "EZ", "TD", "HD", "HR", "SD", "DT", "RX",
                                                    "HT", "NC", "FL", "AU", "SO", "AP", "PF", "K4",
                                                    "K5", "K6", "K7", "K8", "FI", "RD", "CN", "TG",
                                                    "K9", "KC", "K1", "K3", "K2", "S2", "MR" };
        public struct PPInfo
        {
            public double star, CS, HP, AR, OD;
            public double? accuracy;
            public uint? maxCombo;
            public PPStat ppStat;
            public List<PPStat>? ppStats;
            public struct PPStat
            {
                public double total;
                public double? aim, speed, acc, strain, flashlight;
            }
        }

        public enum Mods
        {
            None = 1 >> 1,
            NoFail = 1 << 0,
            Easy = 1 << 1,
            TouchDevice = 1 << 2,
            Hidden = 1 << 3,
            HardRock = 1 << 4,
            SuddenDeath = 1 << 5,
            DoubleTime = 1 << 6,
            Relax = 1 << 7,
            HalfTime = 1 << 8,
            Nightcore = 1 << 9 | DoubleTime, // Only set along with DoubleTime. i.e: NC only gives 576
            Flashlight = 1 << 10,
            Autoplay = 1 << 11,
            SpunOut = 1 << 12,
            Relax2 = 1 << 13, // Autopilot
            Perfect =
            1 << 14 | SuddenDeath, // Only set along with SuddenDeath. i.e: PF only gives 16416
            Key4 = 1 << 15,
            Key5 = 1 << 16,
            Key6 = 1 << 17,
            Key7 = 1 << 18,
            Key8 = 1 << 19,
            FadeIn = 1 << 20,
            Random = 1 << 21,
            Cinema = 1 << 22,
            Target = 1 << 23,
            Key9 = 1 << 24,
            KeyCoop = 1 << 25,
            Key1 = 1 << 26,
            Key3 = 1 << 27,
            Key2 = 1 << 28,
            ScoreV2 = 1 << 29,
            Mirror = 1 << 30,
            KeyMod = Key1 | Key2 | Key3 | Key4 | Key5 | Key6 | Key7 | Key8 | Key9 | KeyCoop,
            FreeModAllowed = NoFail | Easy | Hidden | HardRock | SuddenDeath | Flashlight | FadeIn
            | Relax | Relax2 | SpunOut | KeyMod,
            ScoreIncreaseMods = Hidden | HardRock | DoubleTime | Flashlight | FadeIn
        };

        public static PPInfo Result2Info(CalculateResult result) {
            return new PPInfo() {
                star = result.stars,
                CS = result.cs,
                HP = result.hp,
                AR = result.ar,
                OD = result.od,
                accuracy = result.ppAcc.ToNullable(),
                maxCombo = result.maxCombo.ToNullable(),
                ppStat = new PPInfo.PPStat() {
                    total = result.pp,
                    aim = result.ppAim.ToNullable(),
                    speed = result.ppSpeed.ToNullable(),
                    acc = result.ppAcc.ToNullable(),
                    strain = result.ppStrain.ToNullable(),
                    flashlight = result.ppFlashlight.ToNullable(),
                },
                ppStats = null
            };
        }

        public static CalculateResult Calculate(string beatmapPath, int mode, string[] mods, double? accuracy,
            int? n300, int? n100, int? n50, int? nmiss, int? nkatu, int? combo, int? score)
        {
            var ser = Calculator.New(beatmapPath);
            var p = ScoreParams.New();
            switch (mode)
            {
                case 0:
                    p.Mode(Mode.Osu);
                    break;
                case 1:
                    p.Mode(Mode.Taiko);
                    break;
                case 2:
                    p.Mode(Mode.Catch);
                    break;
                case 3:
                    p.Mode(Mode.Mania);
                    break;
                default:
                    p.Mode(Mode.Osu);
                    break;
            }
            if (accuracy != null) p.Acc(accuracy.Value);
            if (n300 != null) p.N300((uint)n300.Value);
            if (n100 != null) p.N100((uint)n100.Value);
            if (n50 != null) p.N50((uint)n50.Value);
            if (nmiss != null) p.NMisses((uint)nmiss.Value);
            if (nkatu != null) p.NKatu((uint)nkatu.Value);
            if (combo != null) p.Combo((uint)combo.Value);
            if (score != null) p.Score((uint)score.Value);
            if (mods != null) p.Mods(Intmod_parser(mods));
            return ser.Calculate(p.Context);
        }

        public static uint Intmod_parser(string[] mods)
        {
            List<Mods> enabled_mods = new();
            uint num = 0;
            foreach (var x in mods)
            {
                var t = x.ToUpper();
                for (int i = 0; i < 31; ++i)
                {
                    {
                        if (mods_str[i] == t)
                        {
                            uint mod_num = (uint)1 << i;
                            if (i == 9)
                            {
                                mod_num += (uint)Mods.DoubleTime;
                            }
                            if (i == 14)
                            {
                                mod_num += (uint)Mods.SuddenDeath;
                            }
                            enabled_mods.Add((Mods)mod_num);
                            break;
                        }
                    }
                }
            }
            //get mod number
            foreach (var xx in enabled_mods)
                num |= (uint)xx;
            return num;
        }
    }
}