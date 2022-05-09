using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KanonBot.Drivers;
using KanonBot.Message;
using KanonBot.API;

namespace KanonBot.functions
{
    public static class Test
    {
        public static void run(Target target, string cmd)
        {
            if (cmd.StartsWith("say"))
            {
                target.reply(target.msg);
            }
        }
    }
}
