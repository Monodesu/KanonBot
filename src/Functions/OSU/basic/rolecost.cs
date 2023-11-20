using System.CommandLine;
using System.IO;
using KanonBot.API;
using KanonBot.Command;
using KanonBot.Drivers;
using KanonBot.Functions.OSU;
using KanonBot.Functions.OSU.RosuPP;
using KanonBot.Message;
using LanguageExt.UnsafeValueAccess;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using static LinqToDB.Common.Configuration;
using static KanonBot.BindService;
using static System.Runtime.CompilerServices.RuntimeHelpers;
using static KanonBot.API.OSU.DataStructure;
using SixLabors.ImageSharp;
using KanonBot.API.OSU;
using static KanonBot.API.OSU.Models.PPlusData;
using static KanonBot.API.OSU.Models;
using Newtonsoft.Json.Linq;

namespace KanonBot.OSU
{
    public static partial class Basic
    {
        [Command("rolecost")]
        [Params("u", "user", "username", "role", "r")]
        public async static Task rolecost(CommandContext args, Target target)
        {
            var osu_username = "";
            var role = "";
            bool isSelfQuery = false;
            //API.OSU.Enums.Mode? mode = API.OSU.Enums.Mode.OSU;

            args.GetParameters<string>(["u", "user", "username"]).Match
                (
                Some: try_username =>
                {
                    osu_username = try_username;
                },
                None: () =>
                {
                    isSelfQuery = true;
                }
                );
            args.GetParameters<string>(["role", "r"]).Match
                (
                Some: try_role =>
                {
                    osu_username = try_role;
                },
                None: () =>
                {
                    isSelfQuery = true;
                }
                );
            args.GetDefault<string>().Match
                (
                Some: try_role =>
                {
                    role = try_role;
                },
                None: () => { }
                );


            var (DBUser, DBOsuInfo, OnlineOSUUserInfo) = await GetOSUOperationInfo(target, isSelfQuery, osu_username, Enums.Mode.OSU); // 查詢用戶是否有效（是否綁定，是否存在，osu!用戶是否可用），并返回所有信息
            bool IsBound = DBOsuInfo != null;
            if (OnlineOSUUserInfo == null) return; // 查询失败


            static double occost(User userInfo, UserData pppData)
            {
                double a,
                    c,
                    z,
                    p;
                p = userInfo.Statistics!.PP;
                z =
                    1.92 * Math.Pow(pppData.JumpAimTotal, 0.953)
                    + 69.7 * Math.Pow(pppData.FlowAimTotal, 0.596)
                    + 0.588 * Math.Pow(pppData.SpeedTotal, 1.175)
                    + 3.06 * Math.Pow(pppData.StaminaTotal, 0.993);
                a = Math.Pow(pppData.AccuracyTotal, 1.2768) * Math.Pow(p, 0.88213);
                c =
                    Math.Min(
                        0.00930973 * Math.Pow(p / 1000, 2.64192) * Math.Pow(z / 4000, 1.48422),
                        7
                    ) + Math.Min(a / 7554280, 3);
                return Math.Round(c, 2);
            }
            static double oncost(User userInfo)
            {
                double fx,
                    pp;
                pp = userInfo.Statistics!.PP;
                if (pp <= 4000 && pp >= 2000)
                {
                    fx = Math.Round(Math.Pow(1.00053, pp) - 2.88, 2);
                    return fx;
                }
                else
                {
                    return -1;
                }
            }
            static double zkfccost(User userInfo, API.OSU.Models.Score score)
            {
                //formula  cost=bp1pp*0.6+(bp1pp-bp100pp)*0.4+tth/175+PPTotal*0.05      !!!!not this one
                //formula  cost=pp/1831+tth/13939393  !!!!current
                double t = 0.0;
                try
                {
                    t = (double)score.PP / 125.0;
                }
                catch
                {
                    t = 0.0;
                }
                return (double)userInfo.Statistics!.PP / 1200.0
                    + (double)userInfo.Statistics.TotalHits / 1333333.0 + t;
            }


            switch (role)
            {
                case "occ":
                    try
                    {
                        var pppData = await API.OSU.V2.GetUserPlusData(OnlineOSUUserInfo.Id);
                        await target.reply(
                            $"在猫猫杯S1中，{OnlineOSUUserInfo.Username} 的cost为：{occost(OnlineOSUUserInfo, pppData.User!)}"
                        );
                    }
                    catch
                    {
                        await target.reply($"获取pp+失败");
                        return;
                    }
                    break;
                ////////////////////////////////////////////////////////////////////////////////////////
                case "onc":
                    var onc = oncost(OnlineOSUUserInfo);
                    if (onc == -1)
                        await target.reply($"{OnlineOSUUserInfo.Username} 不在参赛范围内。");
                    else
                        await target.reply($"在ONC中，{OnlineOSUUserInfo.Username} 的cost为：{onc}");
                    break;
                ////////////////////////////////////////////////////////////////////////////////////////
                case "zkfc":
                    var scores = await API.OSU.V2.GetUserScores(
                                                         OnlineOSUUserInfo.Id,
                                                         API.OSU.Enums.UserScoreType.Best,
                                                         API.OSU.Enums.Mode.OSU,
                                                         1,
                                                         100
                                                        );
                    if (scores == null)
                    {
                        await target.reply("查询成绩时出错。");
                        return;
                    }
                    if (scores!.Length > 0)
                    {
                        await target.reply(
                            $"在ZKFC S2中，{OnlineOSUUserInfo.Username} 的cost为：{Math.Round(zkfccost(OnlineOSUUserInfo, scores[0]), 2)}"
                        );
                    }
                    break;
                ////////////////////////////////////////////////////////////////////////////////////////
                default:
                    await target.reply(
                        $"请输入要查询cost的比赛名称的缩写。\n当前已支持的比赛：onc/occ/ost/zkfc\n其他比赛请联系赛事主办方提供cost算法"
                    );
                    break;
            }




        }
    }
}
