using System;
using System.CommandLine;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using Flurl;
using Flurl.Http;
using Flurl.Util;
using JetBrains.Annotations;
using KanonBot.API;
using KanonBot.Drivers;
using KanonBot.Message;
using LanguageExt;
using Microsoft.CodeAnalysis;
using Org.BouncyCastle.Asn1.X509;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using static KanonBot.functions.Accounts;
using static LinqToDB.Common.Configuration;
using Img = SixLabors.ImageSharp.Image;

namespace KanonBot.functions.osubot
{
    public class Badge
    {
        public static async Task Execute(Drivers.Target target, string cmd)
        {
            // 验证账户
            var AccInfo = GetAccInfo(target);
            Database.Model.User? DBUser;
            DBUser = await Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
            if (DBUser == null)
            // { await target.reply("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。"); return; }    // 这里引导到绑定osu
            {
                await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。");
                return;
            }
            string rootCmd,
                childCmd = "";
            try
            {
                var tmp = cmd.SplitOnFirstOccurence(" ");
                rootCmd = tmp[0].Trim();
                childCmd = tmp[1].Trim();
            }
            catch
            {
                rootCmd = cmd;
            }
            switch (rootCmd.ToLower())
            {
                case "sudo":
                    await SudoExecute(target, childCmd, AccInfo);
                    return;
                case "set":
                    await Set(target, childCmd, AccInfo);
                    return;
                case "info":
                    await Info(target, childCmd, AccInfo);
                    return;
                case "list":
                    await List(target, AccInfo);
                    return;
                case "redeem":
                    await RedeemBadge(target, childCmd, DBUser.uid);
                    return;
                default:
                    await target.reply("!badge set/info/list/redeem");
                    return;
            }
        }

        private static async Task SudoExecute(Drivers.Target target, string cmd, AccInfo accinfo)
        {
            var userinfo = await Database.Client.GetUsersByUID(accinfo.uid, accinfo.platform);
            List<string> permissions = new();
            if (userinfo!.permissions!.IndexOf(";") < 1) //一般不会出错，默认就是user
            {
                permissions.Add(userinfo.permissions);
            }
            else
            {
                var t1 = userinfo.permissions.Split(";");
                foreach (var x in t1)
                {
                    permissions.Add(x);
                }
            }
            //检查用户权限
            int permissions_flag = -1;
            foreach (var x in permissions)
            {
                switch (x)
                {
                    case "banned":
                        permissions_flag = -1;
                        break;
                    case "user":
                        if (permissions_flag < 1)
                            permissions_flag = 1;
                        break;
                    case "mod":
                        if (permissions_flag < 2)
                            permissions_flag = 2;
                        break;
                    case "admin":
                        if (permissions_flag < 3)
                            permissions_flag = 3;
                        break;
                    case "system":
                        permissions_flag = -2;
                        break;
                    default:
                        break;
                }
            }

            if (permissions_flag < 2)
                return; //权限不够不处理

            //execute
            string rootCmd,
                childCmd = "";
            try
            {
                var tmp = cmd.SplitOnFirstOccurence(" ");
                rootCmd = tmp[0].Trim();
                childCmd = tmp[1].Trim();
            }
            catch
            {
                rootCmd = cmd;
            }

            switch (rootCmd)
            {
                case "create":
                    await SudoCreate(target, childCmd);
                    return;
                case "delete":
                    SudoDelete(target, childCmd);
                    return;
                case "getuser":
                    SudoGetUser(target, childCmd);
                    return;
                case "remove":
                    await SudoRemove(target, childCmd);
                    return;
                case "list":
                    //await List(target, accinfo);
                    return;
                case "addbyemail":
                    await SudoAdd(target, childCmd, 0);
                    return;
                case "addbyoid":
                    await SudoAdd(target, childCmd, 1);
                    return;
                case "createbadgeredemptioncode":
                    if (permissions_flag < 3)
                        await target.reply("权限不足。");
                    else
                        await SudoCreateBadgeRedemptionCode(target, childCmd);
                    return;
                case "runvaildcheckjob":
                    if (permissions_flag < 3)
                        await target.reply("权限不足。");
                    await target.reply("开始任务。");
                    await CheckBadgeIsVaild_Job();
                    await target.reply("已完成。");
                    return;
                default:
                    return;
            }
        }

        //注：没有完全适配多徽章安装，需要等新面板后再做适配
        private static async Task Set(Drivers.Target target, string cmd, AccInfo accinfo)
        {
            //获取用户信息
            var userinfo = await Database.Client.GetUsersByUID(accinfo.uid, accinfo.platform);
            if (userinfo == null)
            {
                await target.reply("未找到账号信息...");
                return;
            }

            if (userinfo.owned_badge_ids == null)
            {
                await target.reply("你还没有牌子呢...");
                return;
            }

            //获取已拥有的牌子
            List<string> owned_badges = new();
            if (userinfo.owned_badge_ids.Contains(','))
            {
                owned_badges = userinfo.owned_badge_ids.Split(',').ToList();
            }
            else
            {
                owned_badges = new()
                {
                    userinfo.owned_badge_ids.Trim()
                };
            }

            List<string> badge_temp = new();

            //如果存在多badge
            if (cmd.IndexOf(",") != -1)
            {

                badge_temp = cmd.Split(",").ToList();

                foreach (var badge in badge_temp)
                {
                    if (int.TryParse(badge, out int badgeNum))
                    {
                        if (badgeNum == 0)
                        {
                            await target.reply($"你提供的badge id({badge})有误，请重新检查。");
                            return;
                        }
                        //检查用户是否拥有此badge
                        if (badgeNum > 0)
                        {
                            if (badgeNum > owned_badges.Count)
                            {
                                await target.reply($"你好像没有编号为 {cmd} 的badge呢..."); ;
                                return;
                            }
                            bool is_badge_owned = false;
                            foreach (var x in owned_badges)
                                if (x == owned_badges[badgeNum - 1])
                                {
                                    is_badge_owned = true;
                                    break;
                                }

                            if (!is_badge_owned)
                            {
                                await target.reply($"你好像没有编号为 {badgeNum} 的badge呢...");
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (badge.Length > 0)
                        {
                            await target.reply($"你提供的badge id({badge})有误，请重新检查。");
                            return;
                        }
                    }
                }

                //去重
                badge_temp = badge_temp.Distinct().ToList();
            }
            //单badge
            else
            {
                if (int.TryParse(cmd, out int badgeNum))
                {
                    if (badgeNum == 0)
                    {
                        await target.reply($"你提供的badge id({cmd})有误，请重新检查。");
                        return;
                    }
                    if (badgeNum != -1 && badgeNum > -2)
                    {
                        //检查用户是否拥有此badge
                        if (badgeNum > owned_badges.Count)
                        {
                            await target.reply($"你好像没有编号为 {cmd} 的badge呢..."); ;
                            return;
                        }
                        bool is_badge_owned = false;
                        foreach (var x in owned_badges)
                            if (x == owned_badges[badgeNum - 1])
                            {
                                is_badge_owned = true;
                                break;
                            }

                        if (!is_badge_owned)
                        {
                            await target.reply($"你好像没有编号为 {cmd} 的badge呢...");
                            return;
                        }
                    }
                    else
                    {
                        if (badgeNum != -1)
                        {
                            await target.reply($"你提供的badge id({cmd})有误，请重新检查。");
                            return;
                        }
                    }
                }
                else
                {
                    if (cmd.Length > 0)
                    {
                        await target.reply($"你提供的badge id({cmd})有误，请重新检查。");
                    }
                    else
                    {
                        await target.reply($"请提供要设置的badge id。");
                    }
                    return;
                }

                badge_temp.Add(badgeNum.ToString());
            }

            //设置badge
            if (badge_temp.Count > 19)
            {
                await target.reply($"提供的badge数量不可超过 19 个，当前提供的badge数量为 {badge_temp.Count} 个。");
                return;
            }

            if (badge_temp.Count == 1 && badge_temp[0] == "-1") //关闭badge显示
            {
                if (await Database.Client.SetDisplayedBadge(userinfo.uid.ToString(), "-1"))
                    await target.reply($"设置成功，已关闭badge显示。");
                else
                    await target.reply($"因数据库原因设置失败，请稍后再试。");
                return;
            }
            else
            {
                var text_temp = "";
                foreach (var x in badge_temp)
                {
                    if (!int.TryParse(x, out int a))
                        a = -9;
                    if (a > 0) text_temp += $"{owned_badges[a - 1]},";
                    else text_temp += $"-9,";
                }
                text_temp = text_temp[..^1];
                if (
                    await Database.Client.SetDisplayedBadge(
                        userinfo.uid.ToString(),
                        text_temp
                    )
                )
                    await target.reply($"设置成功");
                else
                    await target.reply($"因数据库原因设置失败，请稍后再试。");
                return;
            }
        }

        private static async Task Info(Drivers.Target target, string cmd, AccInfo accinfo)
        {
            if (int.TryParse(cmd, out int badgeNum))
            {
                if (badgeNum < 1)
                {
                    await target.reply("你提供的badge id不正确，请重新检查。");
                }

                var userinfo = await Database.Client.GetUsersByUID(accinfo.uid, accinfo.platform);
                if (userinfo == null)
                {
                    await target.reply("未找到账号信息...");
                    return;
                }

                if (userinfo!.owned_badge_ids == null)
                {
                    await target.reply("你还没有牌子呢...");
                    return;
                }

                //获取已拥有的牌子
                List<string> owned_badges;
                if (userinfo.owned_badge_ids.Contains(','))
                {
                    owned_badges = userinfo.owned_badge_ids.Split(',').ToList();
                }
                else
                {
                    owned_badges = new()
                    {
                        userinfo.owned_badge_ids.Trim()
                    };
                }

                //检查用户是否拥有此badge
                if (owned_badges.Count < badgeNum)
                {
                    await target.reply($"你好像没有编号为 {badgeNum} 的badge呢...");
                    return;
                }

                //获取badge信息
                var badgeinfo = await Database.Client.GetBadgeInfo(owned_badges[badgeNum - 1]);

                var rtmsg = new Chain();
                using var stream = new MemoryStream();
                var badge_img = await ReadImageRgba($"./work/badges/{badgeinfo!.id}.png");
                await badge_img.SaveAsync(stream, new PngEncoder());
                rtmsg.image(Convert.ToBase64String(stream.ToArray(), 0, (int)stream.Length), ImageSegment.Type.Base64).msg(
                    $"徽章信息如下：\n" +
                    $"名称：{badgeinfo!.name}\n" + //({badgeinfo.id})\n" +
                    $"中文名称: {badgeinfo.name_chinese}\n" +
                    $"描述: {badgeinfo.description}");
                await target.reply(rtmsg);

                //await target.reply(
                //    $"badge信息:\n"
                //        + $"名称: {badgeinfo!.name}({badgeinfo.id})\n"
                //        + $"中文名称: {badgeinfo.name_chinese}\n"
                //        + $"描述: {badgeinfo.description}"
                //);
            }
            else
            {
                await target.reply("你提供的badge id不正确，请重新检查。");
            }
        }

        private static async Task List(Drivers.Target target, AccInfo accinfo)
        {
            var userinfo = await Database.Client.GetUsersByUID(accinfo.uid, accinfo.platform);
            if (userinfo == null)
            {
                await target.reply("未找到账号信息...");
                return;
            }

            if (userinfo!.owned_badge_ids == null)
            {
                await target.reply("你还没有牌子呢...");
                return;
            }

            //获取已拥有的牌子
            List<string> owned_badges;
            if (userinfo.owned_badge_ids.Contains(','))
            {
                owned_badges = userinfo.owned_badge_ids.Split(',').ToList();
            }
            else
            {
                owned_badges = new()
                {
                    userinfo.owned_badge_ids.Trim()
                };
            }

            //获取badge信息
            var msg = $"以下是你拥有的badge列表:";
            for (int i = 0; i < owned_badges.Count; i++)
            {
                var badgeinfo = await Database.Client.GetBadgeInfo(owned_badges[i]);
                msg += $"\n{i + 1}:{badgeinfo!.name_chinese} ({badgeinfo.name})";
            }
            await target.reply(msg);
        }

        private static async Task SudoCreate(Drivers.Target target, string cmd)
        {
            //badge sudo create IMG_URL#英文名称#中文名称#详细信息
            //检查参数数量
            var args = cmd.Split("#");
            if (args.Length < 5)
            {
                await target.reply("缺少参数。[!badge sudo create IMG_URL#英文名称#中文名称#详细信息#过期天数]");
                return;
            }
            //检查URL
            var img_url = args[0].Trim();
            if (!Utils.IsUrl(img_url))
            {
                await target.reply("提供的IMG_URL不正确。[!badge sudo create IMG_URL#英文名称#中文名称#详细信息#过期天数]");
                return;
            }
            //检查badge图片是否符合要求规范 https://desu.life/test/test_badge.png
            //下载图片
            var randomstr = Utils.RandomStr(50);
            await img_url.DownloadFileAsync(@$"./work/tmp/", $"{randomstr}.png");
            var filepath = @$"./work/tmp/{randomstr}.png";
            using var source = await Img.LoadAsync(filepath);
            if (source.Width / 21.5 != source.Height / 10)
            {
                await target.reply("badge尺寸不符合要求，应为 21.5 : 10（推荐为688*320），操作取消。");
                return;
            }
            source.Mutate(x => x.Resize(688, 320));

            //保存badge图片&数据库插入新的badge
            var db_badgeid = await Database.Client.InsertBadge(args[1], args[2], args[3], int.Parse(args[4]) > 0 ? DateTimeOffset.Now.AddDays(int.Parse(args[4])) : DateTimeOffset.Parse("1970-01-01 00:00:00"));
            //source.Save($"./work/badges/{db_badgeid}.png");
            await source.SaveAsync($"./work/badges/{db_badgeid}.png", new PngEncoder());
            await target.reply($"图片成功上传，新的badgeID为{db_badgeid}");
            Mail.MailStruct ms = new()
            {
                MailTo = new string[] { "mono@desu.life", "fantasyzhjk@qq.com" },
                Subject = "desu.life - 有新的徽章被创建",
                Body = $"有新的徽章兑换码被使用\n\nENG_name: {args[1]}\nCHN_name: {args[2]}\nDISC: {args[3]}\nExpire at: {(int.Parse(args[4]) > 0 ? DateTimeOffset.Now.AddDays(int.Parse(args[4])) : "never")}",
                IsBodyHtml = false
            };
            try
            {
                Mail.Send(ms);
            }
            catch { }
            File.Delete(filepath);
        }

        private static void SudoDelete(Drivers.Target target, string cmd)
        {
            //不是真正的删除，而是禁用某个badge，使其无法被检索到
            //以后再说 到真正需要此功能的时候再写
        }

        private static void SudoGetUser(Drivers.Target target, string cmd) { }

        private static async Task SudoAdd(Drivers.Target target, string cmd, int addMethod)
        {
            var args = cmd.Split("#"); //args[0]=badge id      args[1]=user(s)
            var badgeid = args[0].Trim();
            string[] users;
            if (args[1].Contains('/'))
                users = args[1].Split("/");
            else
                users = new string[] { args[1] };
            string replymsg;
            List<string> failed_msg;
            Database.Model.BadgeList? badge;
            bool skip;
            //0=email 1=oid
            switch (addMethod)
            {
                case 0:
                    #region email
                    //检查各个email是否合法
                    replymsg = "";
                    failed_msg = new();
                    foreach (var user in users)
                        if (!Utils.IsMailAddr(user.Trim()))
                            failed_msg.Add($"{user} 为无效的email，请重新检查。");
                    if (failed_msg.Count > 0)
                    {
                        replymsg += $"检查email有效性失败，共有{failed_msg.Count}个email为无效email，详细信息如下：";
                        foreach (var x in failed_msg)
                            replymsg += $"\n{x}";
                        await target.reply(replymsg);
                        return;
                    }

                    //检查badge是否合法以及是否存在
                    if (!int.TryParse(badgeid, out _))
                    {
                        await target.reply("badgeid不正确，请重新检查。");
                        return;
                    }
                    badge = await Database.Client.GetBadgeInfo(badgeid);
                    if (badge == null)
                    {
                        await target.reply($"似乎没有badgeid为 {badgeid} 的badge呢。");
                        return;
                    }

                    await target.reply($"开始徽章添加任务。");
                    //添加badge
                    failed_msg = new();
                    foreach (var x in users)
                    {
                        skip = false;
                        var userInfo = await Database.Client.GetUser(x);
                        if (userInfo == null)
                            failed_msg.Add($"desu.life用户 {x} 未注册desu.life账户或email未绑定，请重新确认");
                        else
                        {
                            //获取已拥有的牌子
                            List<string> owned_badges = new();
                            if (userInfo.owned_badge_ids != null || userInfo.owned_badge_ids != "") //用户没有badge的情况下，直接写入
                            {
                                //用户只有一个badge的时候直接追加
                                if (!userInfo.owned_badge_ids!.Contains(','))
                                {
                                    if (userInfo.owned_badge_ids != "")
                                        owned_badges.Add(userInfo.owned_badge_ids.Trim());
                                }
                                else
                                {
                                    //用户有2个或以上badge的情况下，先解析再追加新的badge后写入
                                    var owned_badges_temp1 = userInfo.owned_badge_ids.Split(",");
                                    foreach (var xx in owned_badges_temp1)
                                        owned_badges.Add(xx);
                                }
                                //添加之前先查重
                                foreach (var xx in owned_badges)
                                {
                                    if (xx.Contains(badgeid))
                                    {
                                        skip = true;
                                        failed_msg.Add($"desu.life用户 {x} 已拥有此badge，跳过");
                                        break;
                                    }
                                }
                            }
                            //添加
                            if (!skip)
                            {
                                owned_badges.Add(badgeid);
                                string t = "";
                                foreach (var xxx in owned_badges)
                                    t += xxx + ",";
                                if (!await Database.Client.SetOwnedBadge(x, t[..^1]))
                                    failed_msg.Add($"数据库发生了错误，无法为desu.life用户 {x} 添加badge，请稍后重试");
                            }
                        }
                    }
                    replymsg = "完成。";
                    if (failed_msg.Count > 0)
                    {
                        replymsg += $"\n共有{failed_msg.Count}名用户添加失败，错误信息如下：";
                        foreach (var x in failed_msg)
                            replymsg += $"\n{x}";
                    }
                    await target.reply(replymsg);
                    #endregion
                    break;
                case 1:
                    #region oid
                    //检查各个oid是否合法
                    replymsg = "";
                    failed_msg = new();
                    foreach (var user in users)
                        if (!long.TryParse(user.Trim(), out _))
                            failed_msg.Add($"{user} 为无效的uid，请重新检查。");
                    if (failed_msg.Count > 0)
                    {
                        replymsg += $"检查osu!uid有效性失败，共有{failed_msg.Count}个uid为无效uid，详细信息如下：";
                        foreach (var x in failed_msg)
                            replymsg += $"\n{x}";
                        await target.reply(replymsg);
                        return;
                    }

                    //检查badge是否合法以及是否存在
                    if (!int.TryParse(badgeid, out _))
                    {
                        await target.reply("badgeid不正确，请重新检查。");
                        return;
                    }
                    badge = await Database.Client.GetBadgeInfo(badgeid);
                    if (badge == null)
                    {
                        await target.reply($"似乎没有badgeid为 {badgeid} 的badge呢。");
                        return;
                    }

                    await target.reply($"开始徽章添加任务。");
                    //添加badge
                    failed_msg = new();
                    foreach (var x in users)
                    {
                        skip = false;
                        var userInfo = await Database.Client.GetUserByOsuUID(long.Parse(x));
                        if (userInfo == null)
                            failed_msg.Add($"osu!用户 {x} 未注册desu.life账户或osu!账户不存在，请重新确认");
                        else
                        {
                            //获取已拥有的牌子
                            List<string> owned_badges = new();
                            if (userInfo.owned_badge_ids != null && userInfo.owned_badge_ids != "") //用户没有badge的情况下，直接写入
                            {
                                //用户只有一个badge的时候直接追加
                                if (!userInfo.owned_badge_ids.Contains(','))
                                {
                                    if (userInfo.owned_badge_ids != "")
                                        owned_badges.Add(userInfo.owned_badge_ids.Trim());
                                }
                                else
                                {
                                    //用户有2个或以上badge的情况下，先解析再追加新的badge后写入
                                    var owned_badges_temp1 = userInfo.owned_badge_ids.Split(",");
                                    foreach (var xx in owned_badges_temp1)
                                        owned_badges.Add(xx);
                                }
                                //添加之前先查重
                                foreach (var xx in owned_badges)
                                {
                                    if (xx.Contains(badgeid))
                                    {
                                        skip = true;
                                        failed_msg.Add($"osu!用户 {x} 已拥有此badge，跳过");
                                        break;
                                    }
                                }
                            }
                            //添加
                            if (!skip)
                            {
                                owned_badges.Add(badgeid);
                                string t = "";
                                foreach (var xxx in owned_badges)
                                {
                                    t += xxx + ",";
                                }

                                if (!await Database.Client.SetOwnedBadgeByOsuUid(x, t[..^1]))
                                    failed_msg.Add($"数据库发生了错误，无法为osu!用户 {x} 添加badge，请稍后重试");
                            }
                        }
                    }
                    replymsg = "完成。";
                    if (failed_msg.Count > 0)
                    {
                        replymsg += $"\n共有{failed_msg.Count}名用户添加失败，错误信息如下：";
                        foreach (var x in failed_msg)
                            replymsg += $"\n{x}";
                    }
                    await target.reply(replymsg);
                    break;
                #endregion
                default:
                    return;
            }
        }

        private static async Task SudoRemove(Drivers.Target target, string cmd)
        {
            try
            {
                var temp = cmd.Split("#");
                var badge_id = int.Parse(temp[0]);
                var user_id = -999;
                if (temp[1] != "all") user_id = int.Parse(temp[1]);
                if (user_id == -999)
                {
                    //all
                    var users = await Database.Client.GetAllUsersWhoHadBadge();
                    foreach (var user in users)
                    {
                        //获取拥有的牌子信息
                        List<string> owned_badges;
                        if (user.owned_badge_ids!.Contains(','))
                        {
                            owned_badges = user.owned_badge_ids.Split(',').ToList();
                        }
                        else
                        {
                            owned_badges = new()
                                        {
                                            user.owned_badge_ids.Trim()
                                        };
                        }

                        //检查
                        await RemoveBadge((int)user.uid, badge_id);
                    }
                    await target.reply($"已为全部desu.life用户移除了badge_id为 {badge_id} 的徽章。");
                }
                else
                {
                    await RemoveBadge(user_id, badge_id);
                    await target.reply($"已为desu.life用户 {user_id} 移除了badge_id为 {badge_id} 的徽章。");
                }
            }
            catch
            {
                await target.reply("!badge sudo remove badgeid#uid");
            }
        }

        private static void SudoList(Drivers.Target target, string cmd) { }

        private static async Task RedeemBadge(Drivers.Target target, string cmd, long uid)
        {
            if (cmd.Length < 2)
            {
                await target.reply("请提供正确的兑换码。");
                return;
            }
            else
            {
                var data = await Database.Client.RedeemBadgeRedemptionCode(uid, cmd);
                if (data != null)
                {
                    //检测牌子是否已过兑换期限
                    if (data.expire_at.Ticks < DateTimeOffset.Now.Ticks)
                    {
                        await target.reply("该兑换码不存在或已失效。");
                        return;
                    }
                    //检测牌子是否可被重复兑换
                    if (data.redeem_count != 0 && data.can_repeatedly == false)
                    {
                        await target.reply("该兑换码不存在或已失效。");
                        return;
                    }
                    //添加badge
                    var userInfo = await Database.Client.GetUser((int)uid);
                    if (userInfo == null) return;

                    //获取badge信息
                    var badgeinfo = await Database.Client.GetBadgeInfo(data.badge_id.ToString());
                    var bet_info = await Database.Client.GetBadgeExpirationTime((int)userInfo.uid, data.badge_id);

                    //获取已拥有的牌子
                    List<string> owned_badges = new();
                    if (userInfo.owned_badge_ids != null && userInfo.owned_badge_ids != "") //用户没有badge的情况下，直接写入
                    {
                        //用户只有一个badge的时候直接追加
                        if (!userInfo.owned_badge_ids.Contains(',', StringComparison.CurrentCulture))
                        {
                            if (userInfo.owned_badge_ids != "")
                                owned_badges.Add(userInfo.owned_badge_ids.Trim());
                        }
                        else
                        {
                            //用户有2个或以上badge的情况下，先解析再追加新的badge后写入
                            var owned_badges_temp1 = userInfo.owned_badge_ids.Split(",");
                            foreach (var xx in owned_badges_temp1)
                                owned_badges.Add(xx);
                        }
                        //添加之前先查重
                        foreach (var xx in owned_badges)
                        {
                            if (xx == data.badge_id.ToString())
                            {
                                if (bet_info == null)
                                {
                                    await target.reply("您已经拥有此badge了，无法再继续兑换了。");
                                    return;
                                }
                            }
                        }
                    }
                    //设置兑换码状态
                    try
                    {
                        if (!await Database.Client.SetBadgeRedemptionCodeStatus(data.id, uid, cmd))
                        {
                            await target.reply("数据库发生了错误，无法兑换badge，请联系管理员处理。(1)");
                            return;
                        }
                    }
                    catch
                    {
                        await target.reply("数据库发生了错误，无法兑换badge，请联系管理员处理。(2)");
                        return;
                    }

                    //设置badge有效期
                    if (data.badge_expiration_day > 0)
                        if (!await Database.Client.UpdateBadgeExpirationTime((int)userInfo.uid, data.badge_id, data.badge_expiration_day))
                        {
                            await target.reply("数据库发生了错误，无法兑换badge，请联系管理员处理。(3)");
                            return;
                        }

                    //添加
                    if (bet_info == null) //非期限制badge，期限制badge已更新时间，无需再处理
                    {
                        owned_badges.Add(data.badge_id.ToString());
                        string t = "";
                        foreach (var xxx in owned_badges)
                        {
                            t += xxx + ",";
                        }

                        if (!await Database.Client.SetOwnedBadge((int)userInfo.uid, t[..^1]))
                        {
                            await target.reply($"数据库发生了错误，无法兑换badge，请联系管理员处理。(4)");
                            return;
                        }
                    }




                    var rtmsg = new Chain();
                    using var stream = new MemoryStream();
                    var badge_img = await ReadImageRgba($"./work/badges/{badgeinfo!.id}.png");
                    await badge_img.SaveAsync(stream, new PngEncoder());
                    var rtmsg_s = $"已成功兑换徽章。\n" +
                        $"徽章信息如下：\n" +
                        $"名称：{badgeinfo!.name}({badgeinfo.id})\n" +
                        $"中文名称: {badgeinfo.name_chinese}\n" +
                        $"描述: {badgeinfo.description}";
                    if (badgeinfo.expire_at.DateTime != new DateTime(1970, 1, 1))
                        rtmsg_s += $"\n*注意：此徽章将于 {badgeinfo.expire_at.Date} 过期，在此日期后徽章将会被删除。";


                    if (bet_info != null)
                    {
                        rtmsg_s += $"\n*注意：此徽章将于 {bet_info.expire_at.Date.AddDays(data.badge_expiration_day)} 后过期，在此日期后徽章将会被删除。";
                    }
                    else
                    {
                        if (data.badge_expiration_day > 0)
                            rtmsg_s += $"\n*注意：此徽章自兑换之日起 {data.badge_expiration_day} 日后过期，在此日期后徽章将会被删除。";
                    }

                    rtmsg.image(Convert.ToBase64String(stream.ToArray(), 0, (int)stream.Length), ImageSegment.Type.Base64).msg(rtmsg_s);

                    Mail.MailStruct ms = new()
                    {
                        MailTo = new string[] { "mono@desu.life", "fantasyzhjk@qq.com" },
                        Subject = "desu.life - 有新的徽章兑换码被使用",
                        Body = $"有新的徽章兑换码被使用\n\n用户id：{userInfo.uid}\n" +
                        $"徽章id：{badgeinfo!.id}\n" +
                        $"徽章名称：{badgeinfo!.name}({badgeinfo.id})\n" +
                        $"徽章中文名称: {badgeinfo.name_chinese}\n" +
                        $"徽章描述: {badgeinfo.description}\n" +
                        $"兑换码：{data.code}",
                        IsBodyHtml = false
                    };
                    try
                    {
                        Mail.Send(ms);
                    }
                    catch { }

                    await target.reply(rtmsg);
                    return;
                }
                else
                {
                    await target.reply("该兑换码不存在或已失效。");
                    return;
                }
            }
        }

        private static async Task SudoCreateBadgeRedemptionCode(Drivers.Target target, string cmd)
        {
            //badgeid#amount#can_repeatedly(true/false)#expire_at(num of days later)#badge_expire_days
            try
            {
                var tmp_op = cmd.Split("#"); //0=badgeid 1=how many codes need to be generated(amount)
                var badge_id = int.Parse(tmp_op[0]);
                var amount = int.Parse(tmp_op[1]);
                bool can_repeatedly = bool.TryParse(tmp_op[2], out bool crt) && crt;
                var expire_at = int.Parse(tmp_op[3]);
                var badge_expire_days = int.Parse(tmp_op[4]);

                if (amount < 1)
                {
                    await target.reply("amount必须大于0。");
                    return;
                }

                List<string> codes = new();
                for (int i = 0; i < amount; i++)
                {
                    var error_count = 0;
                    while (error_count < 4)
                    {
                        var code = RandomRedemptionCode();
                        var status = await Database.Client.CreateBadgeRedemptionCode(badge_id, code, can_repeatedly, DateTimeOffset.Parse(DateTime.Now.AddDays(expire_at).ToString()), badge_expire_days);
                        if (status)
                        {
                            codes.Add(code);
                            break;
                        }
                        else
                        {
                            await Task.Delay(200);
                            error_count++;
                        }
                    }
                }
                var badgeinfo = await Database.Client.GetBadgeInfo(badge_id.ToString());
                string str = "";
                str += $"此次操作生成了id为 {badge_id} 的徽章\n\n";
                if (can_repeatedly) str += $"注意：此兑换码可被重复兑换\n";
                if (badge_expire_days > 0) str += $"注意：此徽章在兑换后将于自兑换之日起 {badge_expire_days}日后过期\n";
                str += $"此兑换码将于{DateTime.Parse(DateTime.Now.AddDays(expire_at).ToString())}过期，在此时间后将不能再被兑换。\n" +
                    $"徽章信息如下：\n" +
                    $"名称：{badgeinfo!.name}({badgeinfo.id})\n" +
                    $"中文名称: {badgeinfo.name_chinese}\n" +
                    $"描述: {badgeinfo.description}\n\n" +
                    $"此次共生成了 {codes.Count} 个兑换码，";
                if (codes.Count != amount)
                    str += $"有 {amount - codes.Count} 个兑换码生成失败。";
                str += "\n生成的兑换码如下：";
                var count = 1;
                foreach (var x in codes)
                {
                    str += $"\n{count}: {x}";
                    count++;
                }

                Mail.MailStruct ms = new()
                {
                    MailTo = new string[] { "mono@desu.life", "fantasyzhjk@qq.com" },
                    Subject = "desu.life - 有新的徽章兑换码被创建",
                    Body = str,
                    IsBodyHtml = false
                };
                try
                {
                    Mail.Send(ms);
                    await target.reply("徽章兑换码已通过邮件发送至管理员邮箱，请从邮箱内查阅。");
                }
                catch
                {
                    await target.reply("徽章兑换码无法通过邮件发送至管理员邮箱，但已成功添加至数据库，请从数据库内查询。");
                }
            }
            catch
            {
                await target.reply("发生了错误。[!badge sudo createbadgeredemptioncode badge_id#amount#can_repeatedly(true/false)#code_expire_at#badge_expire_days]");
                return;
            }
        }

        private static async Task<bool> RemoveBadge(int userid, int badgeid)
        {

            var userInfo = await Database.Client.GetUser(userid);
            if (userInfo == null)
                return false;
            if (userInfo.owned_badge_ids == null)
                return false;

            //check owned
            if (!userInfo.owned_badge_ids.Contains(','))
            {
                if (userInfo.owned_badge_ids != "")
                    if (userInfo.owned_badge_ids.Trim() == badgeid.ToString())
                    {
                        if (!await Database.Client.SetOwnedBadge(userid, null))
                            return false;
                    }
            }
            else
            {
                //用户有2个或以上badge的情况下，先删除badge后写入
                var owned_badges_temp1 = userInfo.owned_badge_ids.Split(",");
                var badge_text = "";
                foreach (var xx in owned_badges_temp1)
                    if (xx != badgeid.ToString()) badge_text += xx + ",";
                if (!await Database.Client.SetOwnedBadge(userid, badge_text[..^1]))
                    return false;
            }

            //check displayed
            if (userInfo.displayed_badge_ids != null)
                if (!userInfo.displayed_badge_ids.Contains(','))
                {
                    if (userInfo.displayed_badge_ids != "")
                        if (userInfo.displayed_badge_ids.Trim() == badgeid.ToString())
                            await Database.Client.SetDisplayedBadge(userid.ToString(), "-1");
                }
                else
                {
                    //用户有2个或以上badge的情况下，先删除badge后写入
                    var displayed_badges_temp1 = userInfo.displayed_badge_ids.Split(",");
                    var badge_text = "";
                    foreach (var xx in displayed_badges_temp1)
                        if (xx != badgeid.ToString()) badge_text += xx + ",";
                        else badge_text += $"-9,";
                    await Database.Client.SetDisplayedBadge(userid.ToString(), badge_text[..^1]);
                }


            return true;
        }

        public static async Task CheckBadgeIsVaild_Job()
        {
            //获取全部徽章信息
            var allBadges = await Database.Client.GetAllBadges();
            var allBadges_s = await Database.Client.GetAllBadgeExpirationTime();
            var users = await Database.Client.GetAllUsersWhoHadBadge();

            //创建徽章字典 badgeid,datetime
            Dictionary<int, DateTimeOffset> badges = new();
            #region 全局badge
            foreach (var badge in allBadges)
            {
                if (badge.expire_at.DateTime > new DateTime(1990, 1, 1))
                    badges.Add(badge.id, badge.expire_at.DateTime);
            }
            //检查徽章是否在有效期内
            foreach (var user in users)
            {
                //if (user.owned_badge_ids == null) // don't need this


                //获取已拥有的牌子
                List<string> owned_badges;
                if (user.owned_badge_ids!.Contains(','))
                {
                    owned_badges = user.owned_badge_ids.Split(',').ToList();
                }
                else
                {
                    owned_badges = new()
                                        {
                                            user.owned_badge_ids.Trim()
                                        };
                }


                //检查
                string mailmsg = "你有徽章过期了，以下是详细信息\n";
                bool hadbadgeexpired = false;
                for (int i = 0; i < owned_badges.Count; i++)
                    if (badges.TryGetValue(int.Parse(owned_badges[i]), out var value))
                        if (value.DateTime < DateTime.Now) //expired
                        {
                            hadbadgeexpired = true;
                            var badgeinfo = await Database.Client.GetBadgeInfo(owned_badges[i]);
                            mailmsg += $"\n{badgeinfo!.name_chinese} ({badgeinfo.name})";
                            try
                            {
                                await RemoveBadge((int)user.uid, int.Parse(owned_badges[i]));
                                await Database.Client.RemoveBadgeExpirationRecord((int)user.uid, int.Parse(owned_badges[i]));
                            }
                            catch { }
                            Log.Information($"用户 {user.uid} 的徽章({owned_badges[i]}) 已过期，已移除。");
                        }
                mailmsg += "\n\n desu.life";
                if (hadbadgeexpired && user.email!.Length > 4)
                {
                    SendMail(user.email!, "desu.life - 徽章过期通知", mailmsg, false);
                    Log.Information($"已向用户 {user.uid} 发送徽章过期通知邮件。");
                }
            }
            #endregion
            #region 独立badge
            if (allBadges_s == null)
            {
                Log.Information("没有需要验证的数据，跳过。");
                return;
            }
            foreach (var badge in allBadges_s)
            {
                if (badge.expire_at.DateTime < DateTime.Now)
                {
                    var user = await Database.Client.GetUser(badge.uid);
                    List<string> owned_badges;
                    if (user!.owned_badge_ids!.Contains(','))
                    {
                        owned_badges = user.owned_badge_ids.Split(',').ToList();
                    }
                    else
                    {
                        owned_badges = new()
                                        {
                                            user.owned_badge_ids.Trim()
                                        };
                    }


                    //检查
                    string mailmsg = "你有徽章过期了，以下是详细信息\n";

                    var badgeinfo = await Database.Client.GetBadgeInfo(badge.badge_id.ToString());
                    mailmsg += $"\n{badgeinfo!.name_chinese} ({badgeinfo.name})";
                    try
                    {
                        await RemoveBadge((int)user.uid, badge.badge_id);
                        await Database.Client.RemoveBadgeExpirationRecord((int)user.uid, badge.badge_id);
                    }
                    catch { }
                    Log.Information($"用户 {user.uid} 的徽章({badge.badge_id}) 已过期，已移除。");

                    mailmsg += "\n\n desu.life";
                    if (user.email!.Length > 5)
                    {
                        SendMail(user.email!, "desu.life - 徽章过期通知", mailmsg, false);
                        Log.Information($"已向用户 {user.uid} 发送徽章过期通知邮件。");
                    }
                }
            }
            #endregion
        }
    }
}
