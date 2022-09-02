using RosuPP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanonBot.functions.osu.rosupp
{
    public static class PerformanceCalculator
    {
        public struct PPInfo
        {
            public double star, CS, HP, AR, OD, accuracy;
            public int maxCombo;
            public PPStat ppStat;
            public List<PPStat> ppStats;
            public struct PPStat
            {
                public double total, aim, speed, acc, strain;
                public int flashlight, effective_miss_count;
            }
        }




        public static CalculateResult Calculate(string beatmapPath, int mode, double? accuracy,
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
            return ser.Calculate(p.Context);
        }
    }
}
