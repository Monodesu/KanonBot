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
using LanguageExt;

namespace KanonBot.OSU
{
    public static partial class Basic
    {
        [Command("seasonalpass")]
        [Params("m", "mode")]
        public async static Task seasonalpass(CommandContext args, Target target)
        {
            Enums.Mode mode = Enums.Mode.Unknown;

            args.GetDefault<string>().Match
                (
                Some: try_mode =>
                {
                    mode = API.OSU.Enums.String2Mode(try_mode) ?? API.OSU.Enums.Mode.OSU;
                },
                None: () => { }
                );
            args.GetParameters<string>(["m", "mode"]).Match
                (
                Some: try_mode =>
                {
                    mode = API.OSU.Enums.String2Mode(try_mode) ?? API.OSU.Enums.Mode.OSU;
                },
                None: () => { }
                );


            var (DBUser, DBOsuInfo, OnlineOSUUserInfo) = await GetOSUOperationInfo(target, true, "", mode); // 查詢用戶是否有效（是否綁定，是否存在，osu!用戶是否可用），并返回所有信息
            bool IsBound = DBOsuInfo != null;
            if (OnlineOSUUserInfo == null) return; // 查询失败

            if (mode == Enums.Mode.Unknown) mode = (Enums.Mode)Enums.String2Mode(DBOsuInfo!.osu_mode)!;


            var seasonalpassinfo = await Database.Client.GetSeasonalPassInfo(
               OnlineOSUUserInfo!.Id,
               mode.ToStr()  //GetObjectDescription(mode!)!
           )!;
            if (seasonalpassinfo == null)
            {
                await target.reply("用户在本季度暂无季票信息。");
                return;
            }

            //100point一级，每升1级所需point+20
            long temppoint = seasonalpassinfo.point;
            int levelcount = 0;
            while (true)
            {
                temppoint -= (100 + levelcount * 20);
                if (temppoint > 0)
                    levelcount++;
                else
                    break;
            }
            int tt = 0;
            for (int i = 0; i < levelcount; ++i)
            {
                tt += 100 + i * 20;
            }
            double t = Math.Round(
                Math.Round(
                    (
                        (double)((seasonalpassinfo.point - tt) * 100)
                        / (double)(100 + levelcount * 20)
                    ),
                    4
                ),
                4
            );

            string str;
            str =
                $"{OnlineOSUUserInfo.Username}\n自2023年12月1日以来\n您在{OnlineOSUUserInfo!.PlayMode!.ToStr()}模式下的等级为{levelcount}级 "
                + $"({t}%)"
                + $"\n共获得了了{seasonalpassinfo.point}pt\n距离升级大约还需要{Math.Abs(temppoint)}pt";
            await target.reply(str);

        }
    }
}
