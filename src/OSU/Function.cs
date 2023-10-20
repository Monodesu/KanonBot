using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanonBot.OSU
{
    public static class Function
    {
        public static void info()
        {
            Log.Information("you just called info");
        }

        public static void bp()
        {
            Log.Information("you just called bp");
        }

        public static void bestperformance()
        {
            bp();
        }
    }
}
