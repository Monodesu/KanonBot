using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using KanonBot.API;
using KanonBot.LegacyImage;
using RosuPP;
using Serilog;
using SixLabors.ImageSharp.Diagnostics;

namespace KanonBot.functions.osu.rosupp
{

    public static class PerformanceCalculator
    {
        class Beatmap
        {
            Calculator calculator;
            public Beatmap(byte[] beatmapData)
            {
                // 手动处理内存
                var data = GCHandle.Alloc(beatmapData, GCHandleType.Pinned);
                this.calculator = Calculator.New(new Sliceu8(data, (ulong)beatmapData.Length));
                data.Free();
            }

            public ref Calculator GetRef()
            {
                return ref this.calculator;
            }
        }
        public static List<string> mods_str = new(){ "NF", "EZ", "TD", "HD", "HR", "SD", "DT", "RX",
                                                    "HT", "NC", "FL", "AU", "SO", "AP", "PF", "K4",
                                                    "K5", "K6", "K7", "K8", "FI", "RD", "CN", "TG",
                                                    "K9", "KC", "K1", "K3", "K2", "S2", "MR" };
        public struct PPInfo
        {
            public required double star, CS, HP, AR, OD;
            public double? accuracy;
            public uint? maxCombo;
            public required PPStat ppStat;
            public List<PPStat>? ppStats;
            public struct PPStat
            {
                public required double total;
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

        public static PPInfo Result2Info(CalculateResult result)
        {
            return new PPInfo()
            {
                star = result.stars,
                CS = result.cs,
                HP = result.hp,
                AR = result.ar,
                OD = result.od,
                accuracy = result.ppAcc.ToNullable(),
                maxCombo = result.maxCombo.ToNullable(),
                ppStat = new PPInfo.PPStat()
                {
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

        public struct Params
        {
            public required OSU.Enums.Mode mode;
            public string[]? mods;
            public double? acc;
            public uint? n300, n100, n50, nmisses, nkatu, combo, passedObjects, clockRate;
            public void build(ref ScoreParams p)
            {
                p.Mode(mode switch
                {
                    OSU.Enums.Mode.OSU => Mode.Osu,
                    OSU.Enums.Mode.Taiko => Mode.Taiko,
                    OSU.Enums.Mode.Fruits => Mode.Catch,
                    OSU.Enums.Mode.Mania => Mode.Mania,
                    _ => throw new ArgumentException()
                });
                if (acc != null) p.Acc(acc.Value);
                if (n300 != null) p.N300(n300.Value);
                if (n100 != null) p.N100(n100.Value);
                if (n50 != null) p.N50(n50.Value);
                if (nmisses != null) p.NMisses(nmisses.Value);
                if (nkatu != null) p.NKatu(nkatu.Value);
                if (combo != null) p.Combo(combo.Value);
                if (mods != null) p.Mods(Intmod_parser(mods));
            }
        }

        async public static Task<Draw.ScorePanelData> CalculatePanelData(OSU.Models.Score score)
        {
            var data = new Draw.ScorePanelData();
            data.scoreInfo = score;
            var statistics = data.scoreInfo.Statistics;
            Beatmap beatmap;
            try
            {
                // 下载谱面
                await OSU.BeatmapFileChecker(score.Beatmap!.BeatmapId);
                // 读取铺面
                beatmap = new Beatmap(await File.ReadAllBytesAsync($"./work/beatmap/{data.scoreInfo.Beatmap!.BeatmapId}.osu"));
            }
            catch (Exception)
            {
                // 加载失败，删除重新抛异常
                File.Delete($"./work/beatmap/{data.scoreInfo.Beatmap!.BeatmapId}.osu");
                throw;
            }

            var p = ScoreParams.New();
            new Params
            {
                mode = data.scoreInfo.Mode,
                mods = data.scoreInfo.Mods,
                combo = data.scoreInfo.MaxCombo,
                n300 = statistics.CountGreat,
                n100 = statistics.CountOk,
                n50 = statistics.CountMeh,
                nmisses = statistics.CountMiss,
                nkatu = statistics.CountKatu,
            }.build(ref p);
            // 开始计算
            data.ppInfo = Result2Info(beatmap.GetRef().Calculate(p.Context));

            // 5种acc + 全连
            double[] accs = { 100.00, 99.00, 98.00, 97.00, 95.00, data.scoreInfo.Accuracy * 100.00 };
            data.ppInfo.ppStats = accs.Select(
                acc =>
                {
                    var p = ScoreParams.New();
                    new Params
                    {
                        mode = data.scoreInfo.Mode,
                        mods = data.scoreInfo.Mods,
                        acc = acc,
                    }.build(ref p);
                    return Result2Info(
                    beatmap.GetRef().Calculate(
                        p.Context
                        )
                    ).ppStat;
                }
            ).ToList();

            return data;
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
