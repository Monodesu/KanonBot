using KanonBot.Drivers;
using KanonBot.Message;
using KanonBot.API;
using static KanonBot.functions.Accounts;
using Flurl.Util;
using JetBrains.Annotations;
using System.Security.Cryptography;
using K4os.Hash.xxHash;

namespace KanonBot.functions.osubot
{
    public class Badge
    {
        public static void Execute(Target target, string cmd)
        {
            // 验证账户
            var AccInfo = GetAccInfo(target);
            if (AccInfo.uid == null)
            { target.reply("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。"); return; }

            string rootCmd, childCmd = "";
            try
            {
                rootCmd = cmd[..cmd.IndexOf(" ")].Trim();
                childCmd = cmd[(cmd.IndexOf(" ") + 1)..].Trim();
            }
            catch { rootCmd = cmd; }
            switch (rootCmd)
            {
                case "sudo":
                    SudoExecute(target, childCmd, AccInfo); return;
                case "set":
                    Set(target, childCmd, AccInfo); return;
                case "info":
                    Info(target, childCmd, AccInfo); return;
                case "list":
                    List(target, AccInfo); return;
                default:
                    return;
            }
        }
        private static void SudoExecute(Target target, string cmd, AccInfo accinfo)
        {
            var userinfo = Database.Client.GetUsersByUID(accinfo.uid, accinfo.platform);
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
                        permissions_flag = 1;
                        break;
                    case "mod":
                        if (permissions_flag > 2) permissions_flag = 2;
                        break;
                    case "admin":
                        if (permissions_flag > 3) permissions_flag = 3;
                        break;
                    case "system":
                        if (permissions_flag > 4) permissions_flag = -2;
                        break;
                    default:
                        permissions_flag = -1;
                        break;
                }

            }




            //execute
            string rootCmd, childCmd = "";
            try
            {
                rootCmd = cmd[..cmd.IndexOf(" ")].Trim();
                childCmd = cmd[(cmd.IndexOf(" ") + 1)..].Trim();
            }
            catch { rootCmd = cmd; }

            switch (rootCmd)
            {
                case "create":
                    SudoCreate(target, childCmd); return;
                case "delete":
                    SudoDelete(target, childCmd); return;
                case "getuser":
                    SudoGetUser(target, childCmd); return;
                case "list":
                    List(target, accinfo); return;
                default:
                    return;
            }

        }
        //注：没有完全适配多徽章安装，需要等新面板后再做适配
        private static void Set(Target target, string cmd, AccInfo accinfo)
        {
            if (int.TryParse(cmd, out int badgeNum))
            {
                var userinfo = Database.Client.GetUsersByUID(accinfo.uid, accinfo.platform);
                if (userinfo!.owned_badge_ids == null)
                {
                    target.reply("你还没有牌子呢..."); return;
                }

                //获取已拥有的牌子
                List<string> owned_badges = new();
                if (userinfo.owned_badge_ids.IndexOf(",") < 1)
                {
                    owned_badges.Add(userinfo.owned_badge_ids.Trim());
                }
                else
                {
                    var owned_badges_temp1 = userinfo.owned_badge_ids.Split(",");
                    foreach (var x in owned_badges_temp1)
                        owned_badges.Add(x);
                }

                //获取当前已安装的牌子
                List<string> displayed_badges = new();
                if (userinfo.displayed_badge_ids!.IndexOf(",") < 1)
                {
                    if (userinfo.displayed_badge_ids != null)
                        displayed_badges.Add(userinfo.displayed_badge_ids.Trim());
                }
                else
                {
                    var displayed_badges_temp1 = userinfo!.displayed_badge_ids.Split(",");
                    foreach (var x in displayed_badges_temp1)
                        displayed_badges.Add(x);
                }

                //检查当前badge
                foreach (var x in displayed_badges)
                {
                    if (x == badgeNum.ToString())
                    {
                        target.reply($"你现在的主显badge已经是 {x} 了！"); return;
                    }
                }

                //检查用户是否拥有此badge
                if (owned_badges.Count <= badgeNum)
                {
                    target.reply($"你好像没有编号为 {badgeNum} 的badge呢..."); return;
                }

                //设置badge
                if (displayed_badges.Count == 0)
                {
                    if (Database.Client.SetDisplayedBadge(userinfo.uid.ToString(), owned_badges[badgeNum - 1]))
                        target.reply($"设置成功");
                    else
                        target.reply($"因数据库原因设置失败，请稍后再试。");
                    return;
                }
                else
                {
                    string settemp1 = "";
                    foreach (var x in displayed_badges)
                        settemp1 += x + ",";
                    settemp1 += owned_badges[badgeNum - 1];
                    if (Database.Client.SetDisplayedBadge(userinfo.uid.ToString(), settemp1))
                        target.reply($"设置成功");
                    else
                        target.reply($"因数据库原因设置失败，请稍后再试。");
                    return;
                }
            }
            else
            {
                target.reply("你提供的badge id不正确，请重新检查。");
            }
        }
        private static void Info(Target target, string cmd, AccInfo accinfo)
        {
            int badgeNum = -1;
            if (int.TryParse(cmd, out badgeNum))
            {
                var userinfo = Database.Client.GetUsersByUID(accinfo.uid, accinfo.platform);
                if (userinfo!.owned_badge_ids == null)
                {
                    target.reply("你还没有牌子呢..."); return;
                }

                //获取已拥有的牌子
                List<string> owned_badges = new();
                if (userinfo.owned_badge_ids.IndexOf(",") < 1)
                {
                    owned_badges.Add(userinfo.owned_badge_ids.Trim());
                }
                else
                {
                    var owned_badges_temp1 = userinfo.owned_badge_ids.Split(",");
                    foreach (var x in owned_badges_temp1)
                        owned_badges.Add(x);
                }

                //检查用户是否拥有此badge
                if (owned_badges.Count <= badgeNum)
                {
                    target.reply($"你好像没有编号为 {badgeNum} 的badge呢..."); return;
                }


                //获取badge信息
                var badgeinfo = Database.Client.GetBadgeInfo(owned_badges[badgeNum - 1]);
                target.reply($"badge信息:\n" +
                    $"名称: {badgeinfo.name}({badgeinfo.id})\n" +
                    $"中文名称: {badgeinfo.name_chinese}\n" +
                    $"描述: {badgeinfo.description}");
            }
            else
            {
                target.reply("你提供的badge id不正确，请重新检查。");
            }
        }
        private static void List(Target target, AccInfo accinfo)
        {
            var userinfo = Database.Client.GetUsersByUID(accinfo.uid, accinfo.platform);
            if (userinfo!.owned_badge_ids == null)
            {
                target.reply("你还没有牌子呢..."); return;
            }

            //获取已拥有的牌子
            List<string> owned_badges = new();
            if (userinfo.owned_badge_ids.IndexOf(",") < 1)
            {
                owned_badges.Add(userinfo.owned_badge_ids.Trim());
            }
            else
            {
                var owned_badges_temp1 = userinfo.owned_badge_ids.Split(",");
                foreach (var x in owned_badges_temp1)
                    owned_badges.Add(x);
            }

            //获取badge信息
            var msg = $"以下是你拥有的badge列表:";
            for (int i = 0; i < owned_badges.Count; i++)
            {
                var badgeinfo = Database.Client.GetBadgeInfo(owned_badges[i]);
                msg += $"\n{i + 1}:{badgeinfo.name_chinese} ({badgeinfo.name})";
            }
            target.reply(msg);
        }

        private static void SudoCreate(Target target, string cmd)
        {
            var args = cmd.Split("#");
            if (args.Length >= 3)
            {
                target.reply($"已提交，请在30s内发送要上传的badge图片，");
            }
            else
            {
                target.reply("输入不正确，!badge sudo create 英文名称#中文名称#详细信息");
                return;
            }



            //target.reply((target.msg.ToList()[1] as ImageSegment)!.value);
        }
        private static void SudoDelete(Target target, string cmd)
        {
            //不是真正的删除，而是禁用某个badge，使其无法被检索到
            //以后再说 到真正需要此功能的时候再写
        }
        private static void SudoGetUser(Target target, string cmd)
        {

        }
        private static void SudoAdd(Target target, string cmd)
        {
            var args = cmd.Split("#");
            var badgeid_s = args[2].Trim();

            List<string> user_list = new();
            //检查输入
            if (!int.TryParse(badgeid_s, out _)) { target.reply("输入不正确，!badge sudo add [oid]#[badgeId]"); return; }
            if (args[1].IndexOf(",") > 0)
            {
                var users = args[1].Split(",");
                foreach (var x in users)
                {
                    if (!long.TryParse(x, out _)) { target.reply("输入不正确，!badge sudo add [oid]#[badgeId]"); return; }
                    user_list.Add(x.Trim());
                }
            }
            else
            {
                if (!long.TryParse(args[1], out _)) { target.reply("输入不正确，!badge sudo add [oid]#[badgeId]"); return; }
                user_list.Add(args[1].Trim());
            }

            //确认badge是否存在
            var badge = Database.Client.GetBadgeInfo(badgeid_s);
            if (badge == null) { target.reply($"似乎没有badgeid为 {badgeid_s} 的badge呢"); return; }

            //发送开始消息
            if (user_list.Count > 1) target.reply($"开始添加任务。");

            //添加badge
            foreach (var x in user_list)
            {
                var userInfo = Database.Client.GetUsersByOsuUID(long.Parse(x));
                if (userInfo == null) { target.reply($"osu!用户 {x} 不存在，无法添加，请重新检查。"); }
                else
                {
                    //获取已拥有的牌子
                    List<string> owned_badges = new();
                    if (userInfo.owned_badge_ids == null)
                    {
                        owned_badges.Add(badgeid_s);
                    }
                    else
                    {
                        if (userInfo.owned_badge_ids!.IndexOf(",") < 1)
                        {
                            owned_badges.Add(userInfo.owned_badge_ids.Trim());
                        }
                        else
                        {
                            var owned_badges_temp1 = userInfo.owned_badge_ids.Split(",");
                            foreach (var xx in owned_badges_temp1)
                                owned_badges.Add(xx);
                        }
                        owned_badges.Add(badgeid_s);
                    }

                    //添加
                    string t = "";
                    foreach (var xx in owned_badges)
                        t += xx + ",";
                    if (!Database.Client.SetOwnedBadge(x, t[..^1]))
                        target.reply($"数据库错误，无法为osu!用户 {x} 添加。");
                }
            }
            target.reply($"完成。");
        }
        private static void SudoRemove(Target target, string cmd)
        {

        }
        private static void SudoList(Target target, string cmd)
        {

        }
    }
}
