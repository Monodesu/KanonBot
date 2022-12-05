using KanonBot.Drivers;
using KanonBot.Message;
using KanonBot.API;
using KanonBot.Serializer;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace KanonBot.functions.osubot
{
    public class PPvs
    {
        public async static Task Execute(Target target, string cmd)
        {
            var cmds = cmd.Split('#');
            if (cmds.Length == 1) {
                if (cmds[0].Length == 0)
                {
                    await target.reply("!ppvs 要对比的用户");
                    return;
                }


                var AccInfo = Accounts.GetAccInfo(target);
                var DBUser = await Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
                if (DBUser == null)
                // { await target.reply("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。"); return; }    // 这里引导到绑定osu
                { await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }

                var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
                var DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
                if (DBOsuInfo == null)
                { await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }

                // 分别获取两位的信息
                var userSelf = await OSU.GetUser(DBOsuInfo.osu_uid);
                if (userSelf == null)
                {
                    await target.reply("被办了。");
                    return;
                }

                var user2 = await OSU.GetUser(cmds[0]);
                if (user2 == null)
                {
                    await target.reply("猫猫没有找到此用户。");
                    return;
                }

                await target.reply("正在获取pp+数据，请稍等。。");

                LegacyImage.Draw.PPVSPanelData data = new();

                var d1 = await Database.Client.GetOsuPPlusData(userSelf.Id);
                if (d1 == null)
                {
                    var d1temp = await API.OSU.TryGetUserPlusData(userSelf);
                    if (d1temp == null)
                    {
                        await target.reply("获取pp+数据时出错，等会儿再试试吧");
                        return;
                    }
                    d1 = d1temp.User;
                    await Database.Client.UpdateOsuPPlusData(d1, userSelf.Id);
                }
                data.u2Name = userSelf.Username;
                data.u2 = d1;

                var d2 = await Database.Client.GetOsuPPlusData(user2.Id);
                if (d2 == null)
                {
                    var d2temp = await API.OSU.TryGetUserPlusData(user2);
                    if (d2temp == null)
                    {
                        await target.reply("获取pp+数据时出错，等会儿再试试吧");
                        return;
                    }
                    d2 = d2temp.User;
                    await Database.Client.UpdateOsuPPlusData(d2, user2.Id);
                }
                data.u1Name = user2.Username;
                data.u1 = d2;

                using var stream = new MemoryStream();
                using var img = await LegacyImage.Draw.DrawPPVS(data);
                await img.SaveAsync(stream, new JpegEncoder());
                await target.reply(new Chain().image(Convert.ToBase64String(stream.ToArray(), 0, (int)stream.Length), ImageSegment.Type.Base64));
            } else if (cmds.Length == 2) {
                if (cmds[0].Length == 0 || cmds[1].Length == 0)
                {
                    await target.reply("!ppvs 用户1#用户2");
                    return;
                }

                // 分别获取两位的信息
                var user1 = await OSU.GetUser(cmds[0]);
                if (user1 == null)
                {
                    await target.reply($"猫猫没有找到叫 {cmds[0]} 用户。");
                    return;
                }

                var user2 = await OSU.GetUser(cmds[1]);
                if (user2 == null)
                {
                    await target.reply($"猫猫没有找到叫 {cmds[1]} 用户。");
                    return;
                }

                await target.reply("正在获取pp+数据，请稍等。。");

                LegacyImage.Draw.PPVSPanelData data = new();

                var d1 = await Database.Client.GetOsuPPlusData(user1.Id);
                if (d1 == null)
                {
                    var d1temp = await API.OSU.TryGetUserPlusData(user1);
                    if (d1temp == null)
                    {
                        await target.reply("获取pp+数据时出错，等会儿再试试吧");
                        return;
                    }
                    d1 = d1temp.User;
                    await Database.Client.UpdateOsuPPlusData(d1, user1.Id);
                }
                data.u2Name = user1.Username;
                data.u2 = d1;

                var d2 = await Database.Client.GetOsuPPlusData(user2.Id);
                if (d2 == null)
                {
                    var d2temp = await API.OSU.TryGetUserPlusData(user2);
                    if (d2temp == null)
                    {
                        await target.reply("获取pp+数据时出错，等会儿再试试吧");
                        return;
                    }
                    d2 = d2temp.User;
                    await Database.Client.UpdateOsuPPlusData(d2, user2.Id);
                }
                data.u1Name = user2.Username;
                data.u1 = d2;


                using var stream = new MemoryStream();
                using var img = await LegacyImage.Draw.DrawPPVS(data);
                await img.SaveAsync(stream, new JpegEncoder());
                await target.reply(new Chain().image(Convert.ToBase64String(stream.ToArray(), 0, (int)stream.Length), ImageSegment.Type.Base64));
            } else {
                await target.reply("!ppvs 用户1#用户2/!ppvs 要对比的用户");
            }
        }
    }
}
