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
using KanonBot.Image.OSU;

namespace KanonBot.OSU
{
    public static partial class Basic
    {
        [Command("tbp", "todaybp", "todaysbp")]
        [Params("m", "mode", "u", "user", "username")]
        public async static Task todaybp(CommandContext args, Target target)
        {
            var osu_username = "";
            bool isSelfQuery = false;
            API.OSU.Enums.Mode? mode = API.OSU.Enums.Mode.OSU;

            args.GetParameters<string>(["u", "user", "username"]).Match
                (
                Some: try_username =>
                {
                    osu_username = try_username;
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
                    if (osu_username == "") isSelfQuery = true;
                }
                );
            args.GetParameters<string>(["m", "mode"]).Match
                (
                Some: try_mode =>
                {
                    mode = API.OSU.Enums.String2Mode(try_mode) ?? API.OSU.Enums.Mode.OSU;
                },
                None: () => { }
                );


            var (DBUser, DBOsuInfo, OnlineOSUUserInfo) = await GetOSUOperationInfo(target, isSelfQuery, osu_username, mode); // 查詢用戶是否有效（是否綁定，是否存在，osu!用戶是否可用），并返回所有信息
            bool IsBound = DBOsuInfo != null;
            if (OnlineOSUUserInfo == null) return; // 查询失败

            var allBP = await API.OSU.V2.GetUserScores(
                OnlineOSUUserInfo!.Id,
                API.OSU.Enums.UserScoreType.Best,
                mode!.Value,
                100,
                0
                );
            if (allBP == null)
            {
                await target.reply("查询成绩时出错。");
                return;
            }
            List<API.OSU.Models.Score> TBP = new();
            List<int> Rank = new();

            var t =
                DateTime.Now.Hour < 4
                    ? DateTime.Now.Date.AddDays(-1).AddHours(4)
                    : DateTime.Now.Date.AddHours(4);
            for (int i = 0; i < allBP.Length; i++)
            {
                var item = allBP[i];
                var ts = (item.CreatedAt - t).Days;
                if (0 <= ts && ts < 1)
                {
                    TBP.Add(item);
                    Rank.Add(i + 1);
                }
            }
            if (TBP.Count == 0)
            {
                if (osu_username == "")
                    await target.reply($"你今天在 {OnlineOSUUserInfo.PlayMode.ToStr()} 模式上还没有新bp呢。。");
                else
                    await target.reply(
                        $"{OnlineOSUUserInfo.Username} 今天在 {OnlineOSUUserInfo.PlayMode.ToStr()} 模式上还没有新bp呢。。"
                    );
            }
            else
            {
                var image = await KanonBot.Image.OSU.OsuScoreList.Draw(
                    OsuScoreList.Type.TODAYBP,
                    TBP,
                    Rank,
                    OnlineOSUUserInfo
                );
                using var stream = new MemoryStream();
                await image.SaveAsync(stream, new PngEncoder());
                await target.reply(
                    new Chain().image(
                        Convert.ToBase64String(stream.ToArray(), 0, (int)stream.Length),
                        ImageSegment.Type.Base64
                    )
                );
            }
        }
    }
}
