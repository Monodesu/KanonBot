using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KanonBot.Drivers;

namespace KanonBot.OSU
{
    public static partial class Basic
    {
        public static void info1(Target target)
        {
            Log.Information("you just called info");
        }

        public static void bp1(Target target)
        {
            Log.Information("you just called bp");
        }

        public static void bestperformance1(Target target)
        {
            bp1(target);
        }
    }
}
