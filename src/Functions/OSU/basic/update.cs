using System.CommandLine;
using System.IO;
using KanonBot.API;
using KanonBot.API.OSU;
using KanonBot.Command;
using KanonBot.Drivers;
using KanonBot.Functions.OSU;
using KanonBot.Message;
using LanguageExt.UnsafeValueAccess;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using static KanonBot.API.OSU.DataStructure;
using static KanonBot.BindService;
using static LinqToDB.Common.Configuration;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace KanonBot.OSU
{
    public static partial class Basic
    {
        [Command("update")]
        [Params("u", "user", "username")]
        public static async Task update(CommandContext args, Target target)
        {
            var osu_username = "";
            bool isSelfQuery = false;
            API.OSU.Enums.Mode? mode = API.OSU.Enums.Mode.OSU;

            args.GetParameters<string>([ "u", "user", "username" ])
                .Match(
                    Some: try_username =>
                    {
                        osu_username = try_username;
                    },
                    None: () => { }
                );
            args.GetDefault<string>()
                .Match(
                    Some: try_name =>
                    {
                        osu_username = try_name;
                    },
                    None: () =>
                    {
                        if (osu_username == "")
                            isSelfQuery = true;
                    }
                );

            var (DBUser, DBOsuInfo, OnlineOSUUserInfo) = await GetOSUOperationInfo(
                target,
                isSelfQuery,
                osu_username,
                mode
            ); // 查詢用戶是否有效（是否綁定，是否存在，osu!用戶是否可用），并返回所有信息
            bool IsBound = DBOsuInfo != null;
            if (OnlineOSUUserInfo == null)
                return; // 查询失败

            //await target.reply("少女祈祷中...");
            try
            {
                File.Delete($"./work/avatar/{OnlineOSUUserInfo!.Id}.png");
            }
            catch (Exception ex)
            {
                Log.Warning(ex.Message);
            }
            try
            {
                File.Delete($"./work/legacy/v1_cover/osu!web/{OnlineOSUUserInfo!.Id}.png");
            }
            catch (Exception ex)
            {
                Log.Warning(ex.Message);
            }
            await target.reply("主要数据已更新，pp+数据正在后台更新，请稍后使用info功能查看结果。");

            try
            {
                await Database
                    .Client
                    .UpdateOsuPPlusData(
                        (await API.OSU.V2.TryGetUserPlusData(OnlineOSUUserInfo!))!.User!,
                        OnlineOSUUserInfo!.Id
                    );
            }
            catch { } //更新pp+失败，不返回信息
        }
    }
}
