using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanonBot
{
    public static class BotCmdHelper
    {
        public struct Bot_Parameter
        {
            public string osu_mode, osu_username, osu_mods;           //用于获取具体要查询的模式，未提供返回osu
            public long
                osu_user_id,
                bid;
            public int order_number;//用于info的查询n天之前、pr，bp的序号，score的bid，如未提供则返回0
            public bool res; //是否输出高精度图片
            public bool selfquery;
        }

        public enum Func_type
        {
            Info,
            BestPerformance,
            Recent,
            PassRecent,
            Score,
            Leeway
        }

        public static Bot_Parameter CmdParser(string cmd, Func_type type)
        {
            cmd = cmd.Trim().ToLower();
            Bot_Parameter param = new() { res = false, selfquery = false };
            if (cmd != null)
            {
                string arg1 = "", arg2 = "", arg3 = "", arg4 = "";
                int section = 0;
                // 解析所有可能的参数
                for (var i = 0; i < cmd.Length; i++)
                {
                    if (cmd[i] == ':') section = 1;
                    else if (cmd[i] == '#') section = 2;
                    else if (cmd[i] == '+') section = 3;
                    else if (cmd[i] == '&') section = 4;
                    switch (section)
                    {
                        case 1:
                            arg2 += cmd[i];
                            break;
                        case 2:
                            arg3 += cmd[i];
                            break;
                        case 3:
                            arg4 += cmd[i];
                            break;
                        case 4:
                            param.res = true;
                            break;
                        default:
                            arg1 += cmd[i];
                            break;
                    }
                }
                // 处理info解析
                if (type == Func_type.Info)
                {
                    // arg1 = username
                    // arg2 = osu_mode
                    // arg3 = osu_days_before_to_query
                    param.osu_username = arg1;
                    param.osu_mode = arg2 != "" ? GetMode(int.Parse(arg2[1..])) : "";
                    if (arg3 == "") param.order_number = 0;
                    else param.order_number = int.Parse(arg3[1..]);
                    if (param.osu_username == "") param.selfquery = true;
                }
                // 处理pr/bp/re解析
                else if (type == Func_type.Recent || type == Func_type.PassRecent || type == Func_type.BestPerformance)
                {
                    // arg1 = username
                    // arg2 = osu_mode
                    // arg3 = order_number (序号)
                    param.osu_username = arg1;
                    param.osu_mode = arg2 != "" ? GetMode(int.Parse(arg2[1..])) : "";
                    if (arg3 == "") param.order_number = 1; //成绩必须为1
                    else param.order_number = int.Parse(arg3[1..]);
                    if (param.osu_username == "") param.selfquery = true;
                }
                // 处理score解析
                else if (type == Func_type.Score)
                {
                    // arg1 = bid
                    // arg2 = osu_mode
                    // arg3 = username
                    // arg4 = mods
                    param.osu_username = arg3;
                    param.osu_mode = arg2 != "" ? GetMode(int.Parse(arg2[1..])) : "";
                    param.osu_mods = arg4 != "" ? arg4[1..] : "";
                    if (arg1 == "") param.order_number = -1; //bid必须有效，否则返回 -1
                    else { var index = int.Parse(arg1); param.order_number = index < 1 ? -1 : index; }
                    if (param.osu_username == "") param.selfquery = true;
                }
                else if (type == Func_type.Leeway)
                {
                    // arg1 = bid
                    // arg2 = osu_mode
                    // arg3 = 
                    // arg4 = mods
                    if (arg1 == "") param.order_number = 0; // 若bid为空，返回0
                    else { var index = int.Parse(arg1); param.order_number = index < 1 ? -1 : index; }
                    param.osu_mode = arg2 != "" ? GetMode(int.Parse(arg2[1..])) : "";
                    param.osu_mods = arg4 != "" ? arg4[1..] : "";
                    param.selfquery = true; // 只查自己
                }
            }
            else
            {
                param.selfquery = true;
                if (type == Func_type.Info)
                {
                    param.order_number = 0; //由于cmd为空，所以默认查询当日信息
                }
                else if (type == Func_type.Score)
                {
                    param.order_number = -1; //由于cmd为空，所以没有bid等关键信息，返回错误
                }
                else if (type == Func_type.Recent || type == Func_type.PassRecent || type == Func_type.BestPerformance)
                {
                    param.order_number = 1; //由于cmd为空，所以默认返回第一张谱面成绩
                }
            }
            return param;
        }

        public static string GetMode(int i)
        {
            return i switch
            {
                0 => "osu",
                1 => "taiko",
                2 => "fruits",
                3 => "mania",
                _ => "osu",
            };
        }
    }
}
