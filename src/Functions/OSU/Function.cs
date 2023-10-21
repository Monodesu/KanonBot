using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KanonBot.Drivers;

namespace KanonBot.OSU
{
    public static class Function
    {
        public static void info(Target target)
        {
            Log.Information("you just called info");
        }

        public static void bp(Target target)
        {
            Log.Information("you just called bp");
        }

        public static void bestperformance(Target target)
        {
            bp(target);
        }
    }
}
