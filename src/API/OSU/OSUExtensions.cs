using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanonBot.API.OSU
{
    static public class Extensions
    {
        public static string ToStr(this Enums.UserScoreType type)
        {
            return Utils.GetObjectDescription(type)!;
        }
        public static string ToStr(this Enums.Mode mode)
        {
            return Utils.GetObjectDescription(mode)!;
        }

        public static int ToNum(this Enums.Mode mode)
        {
            return mode switch
            {
                Enums.Mode.OSU => 0,
                Enums.Mode.Taiko => 1,
                Enums.Mode.Fruits => 2,
                Enums.Mode.Mania => 3,
                _ => throw new ArgumentException("UNKNOWN MODE")
            };
        }
    }
}
