using KanonBot.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanonBot.functions.osu
{
    public static class Seasonalpass
    {
        //查询seasonal pass放在了get.cs里 todo
        public static async Task<bool> Update(long oid, string mode, long tth)
        {
            return await Database.Client.UpdateSeasonalPass(oid, mode, tth);
        }
    }
}
