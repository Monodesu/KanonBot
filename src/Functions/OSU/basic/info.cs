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

namespace KanonBot.OSU
{
    public static partial class Basic
    {

        [Command("info", "stat")]
        [Params("m", "mode")]
        public async static Task info(CommandContext args, Target target)
        {
            var osu_username = "";
            long osu_uid = 0;
            bool isSelfQuery = false;
            API.OSU.Enums.Mode mode = API.OSU.Enums.Mode.Unknown;

            args.GetParameters<string>(["m", "mode"]).Match
                (
                Some: try_mode =>
                {
                    mode = API.OSU.Enums.String2Mode(try_mode) ?? API.OSU.Enums.Mode.OSU;
                },
                None: () => { }
                );

            args.GetDefault<string>().Match
                (
                Some: try_name =>
                {
                    osu_username = try_name;
                },
                None: () =>
                {
                    isSelfQuery = true;
                }
                );


            var (osuID, osu_mode) = await GetOSUOperationInfo(target, isSelfQuery, osu_username);
            if (osu_mode == API.OSU.Enums.Mode.Unknown) return; // 查询失败
            osu_uid = osuID;
            mode = osu_mode;

            // 操作部分

            var OnlineOSUUserInfo = await API.OSU.V2.GetUser(osu_uid, mode);

            Log.Information(
                $"""

                osu!status
                username: {OnlineOSUUserInfo!.Username}
                osu_uid: {OnlineOSUUserInfo.Id}
                osu_mode: {OnlineOSUUserInfo.PlayMode}
                osu_pp: {OnlineOSUUserInfo!.Statistics!.PP}
                """
                );

            await Task.CompletedTask;
        }
    }
}
