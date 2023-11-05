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
            long? osu_uid = null;
            bool isSelfQuery = false;
            API.OSU.Enums.Mode? mode = null;

            args.GetParameters<string>(["m", "mode"]).Match
                (
                Some: try_mode =>
                {
                    mode = API.OSU.Enums.String2Mode(try_mode);
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

            if (isSelfQuery)
            {
                var (base_uid, osuID, osu_mode) = await VerifyBaseAccount(target);
                if (osu_uid == 0) return; // 查询失败
                mode ??= API.OSU.Enums.String2Mode(osu_mode)!.Value;
                osu_uid = osuID;
            }
            else
            {
                var (osuID, osu_mode) = await QueryOtherUser(target, osu_username, mode);
                mode = osu_mode;
                osu_uid = osuID;
            }

            // 操作部分











            await Task.CompletedTask;
        }
    }
}
