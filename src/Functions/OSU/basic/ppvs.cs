using System.CommandLine;
using System.IO;
using KanonBot.API;
using KanonBot.Command;
using KanonBot.Drivers;
using KanonBot.Functions.OSU;

using KanonBot.Message;
using LanguageExt.UnsafeValueAccess;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using static LinqToDB.Common.Configuration;
using static KanonBot.BindService;
using static System.Runtime.CompilerServices.RuntimeHelpers;
using static KanonBot.API.OSU.DataStructure;
using SixLabors.ImageSharp;
using KanonBot.Image.OSU;

namespace KanonBot.OSU
{
    public static partial class Basic
    {
        [Command("ppvs")]
        [Params("u", "user", "username", "c", "compare", "comparewith")]
        public async static Task ppvs(CommandContext args, Target target)
        {
            var osu_username = "";
            var compare_username = "";
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
            args.GetParameters<string>(["c", "compare", "comparewith"]).Match
                (
                Some: try_c =>
                {
                    compare_username = try_c;
                },
                None: () => { }
                );


            var (DBUser, DBOsuInfo, OnlineOSUUserInfo) = await GetOSUOperationInfo(target, isSelfQuery, osu_username, mode); // 查詢用戶是否有效（是否綁定，是否存在，osu!用戶是否可用），并返回所有信息
            bool IsBound = DBOsuInfo != null;
            if (OnlineOSUUserInfo == null) return; // 查询失败

            if (compare_username == "" && osu_username == "")
            {
                await target.reply("请指定要比较的对象");
                return;
            }

            API.OSU.Models.User user1 = null!, user2 = null!;

            if ((compare_username != "" && osu_username == "") || (compare_username == "" && osu_username != ""))
            {
                user1 = OnlineOSUUserInfo;
                var x = await API.OSU.V2.GetUser(compare_username);
                if (x == null)
                {
                    await target.reply("要比较的用户不存在哦");
                    return;
                }
                user2 = x;
            }

            if (compare_username != "" && osu_username != "")
            {
                var z = await API.OSU.V2.GetUser(osu_username);
                if (z == null)
                {
                    await target.reply("要比较的用户不存在哦");
                    return;
                }
                var x = await API.OSU.V2.GetUser(compare_username);
                if (x == null)
                {
                    await target.reply("要比较的用户不存在哦");
                    return;
                }
                user1 = z;
                user2 = x;
            }

            API.OSU.DataStructure.PPVSPanelData data = new();

            var d1 = await Database.Client.GetOsuPPlusData(user1.Id);
            if (d1 is null)
            {
                var d1temp = await API.OSU.V2.TryGetUserPlusData(user1);
                if (d1temp is null)
                {
                    await target.reply("获取pp+数据时出错，等会儿再试试吧");
                    return;
                }
                d1 = d1temp.User;
                await Database.Client.UpdateOsuPPlusData(d1temp.User!, OnlineOSUUserInfo.Id);
            }
            data.u2Name = OnlineOSUUserInfo.Username;
            data.u2 = d1;

            var d2 = await Database.Client.GetOsuPPlusData(user2.Id);
            if (d2 == null)
            {
                var d2temp = await API.OSU.V2.TryGetUserPlusData(user2);
                if (d2temp == null)
                {
                    await target.reply("获取pp+数据时出错，等会儿再试试吧");
                    return;
                }
                d2 = d2temp.User;
                await Database.Client.UpdateOsuPPlusData(d2temp.User!, user2.Id);
            }
            data.u1Name = user2.Username;
            data.u1 = d2;

            using var stream = new MemoryStream();
            using var img = await OsuPPPVs.Draw(data);
            await img.SaveAsync(stream, new JpegEncoder());
            await target.reply(new Chain().image(Convert.ToBase64String(stream.ToArray(), 0, (int)stream.Length), ImageSegment.Type.Base64));
        }
    }
}
