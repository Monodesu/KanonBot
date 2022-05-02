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
            public string osu_mode;           //用于获取具体要查询的模式，未提供返回osu
            public long
                osu_user_id,
                bid, order_number; //用于info的查询n天之前、pr，bp的序号，score的bid，如未提供则返回0
            public bool res; //是否输出高精度图片
        }

        public enum Func_type
        {
            Info,
            BestPerformance,
            Recent,
            PassRecent,
            Score
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="platform">传入core的平台信息，用于读取用户在数据库的信息</param>
        /// <param name="userid">传入core的平台用户信息，用于读取用户在数据库的信息</param>
        /// 以上参数可能需要修改
        /// <returns></returns>
        public static Bot_Parameter CmdParser(string cmd, Func_type type, string platform, string userid)
        {
            cmd = cmd.Trim().ToLower();
            Bot_Parameter param = new() { res = false };
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

                //处理通用信息

                // 尝试拉取对应的用户信息
                // if (arg1 == "") {
                // var db = database.read(platform, userid);
                // param.osu_user_id = db.osu_uid;
                // } else {
                // param.osu_user_id = osu.get_userid_by_username(arg1);
                // }
                // 
                // 传入mode
                // if (arg2 != "") param.osu_mode = arg2[1..];
                // else param.osu_mode = db.osu_mode;

                // 处理info解析
                if (type == Func_type.Info)
                {
                    // 各args在此处表达为
                    // arg1 = username
                    // arg2 = osu_mode
                    // arg3 = osu_days_before_to_query


                    // 传入days_before
                    // if (arg3 == "") param.order_number = 0; //0代表今天，1代表昨天，所以要返回0
                    // else param.order_number = int.parse(arg3[1..]);
                }
                // 处理pr/bp/re解析
                else if (type == Func_type.Recent || type == Func_type.PassRecent || type == Func_type.BestPerformance)
                {
                    // 各args在此处表达为
                    // arg1 = username
                    // arg2 = osu_mode
                    // arg3 = order_number (序号)


                    // 传入order_number
                    // if (arg3 == "") param.order_number = 1; //成绩必须为1
                    // else param.order_number = int.parse(arg3[1..]);
                }
                // 处理score解析
                else if (type == Func_type.Score)
                {
                    // 各args在此处表达为
                    // arg1 = username
                    // arg2 = osu_mode
                    // arg3 = bid
                    // arg4 = mods


                    // 传入bid
                    // if (arg3 == "") param.order_number = -1; //bid必须有效，否则返回错误
                    // else param.order_number = int.parse(arg3[1..]);
                    //
                    // 解析mod
                    // do something here
                }
            }
            else
            {
                //读数据库，找出用户信息，如数据库无用户绑定信息，则全部返回无效值
                // if (database.read(platform, userid) == false) {
                param.osu_user_id = -1;
                param.osu_mode = "osu"; //由于数据库没有此用户信息，所以返回默认std模式

                // }
                // else {
                //   param.osu_user_id = database.osu_user_id;
                //   param.osu_mode = database.osu_mode;
                // }
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
    }
}
