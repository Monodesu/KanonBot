using KanonBot.Drivers;
using KanonBot.Message;
using KanonBot.API;
using osu.Framework.Extensions.ObjectExtensions;
using static KanonBot.functions.Accounts;
using osu.Game.Users;
using static KanonBot.API.OSU.Legacy;

namespace KanonBot.functions.osubot
{
    public class Badge
    {
        public static void Execute(Target target, string cmd)
        {
            // 验证账户
            var AccInfo = Accounts.GetAccInfo(target);
            if (Accounts.GetAccount(AccInfo.uid, AccInfo.platform)!.uid == -1)
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
                    SudoExecute(target, childCmd); return;
                case "set":
                    Set(target, childCmd); return;
                case "info":
                    Info(target, childCmd); return;
                case "list":
                    List(target, childCmd); return;
                default:
                    return;
            }
        }
        private static void SudoExecute(Target target, string cmd)
        {
            var userinfo = Database.Client.GetUsersByUID(target.account!, target.platform);
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


        }
        //注：没有完全适配多徽章安装，需要等新面板后再做适配
        private static void Set(Target target, string cmd)
        {
            if (int.TryParse(cmd, out int badgeNum))
            {
                var userinfo = Database.Client.GetUsersByUID(target.account!, target.platform);
                if (userinfo!.owned_badge_ids.IsNull())
                {
                    target.reply("你还没有牌子呢..."); return;
                }

                //获取已拥有的牌子
                List<string> owned_badges = new();
                if (userinfo.owned_badge_ids.IndexOf(";") < 1)
                {
                    owned_badges.Add(userinfo.owned_badge_ids.Trim());
                }
                else
                {
                    var owned_badges_temp1 = userinfo.owned_badge_ids.Split(";");
                    foreach (var x in owned_badges_temp1)
                        owned_badges.Add(x);
                }

                //获取当前已安装的牌子
                List<string> displayed_badges = new();
                if (userinfo.displayed_badge_ids!.IndexOf(";") < 1)
                {
                    if (!userinfo.displayed_badge_ids.IsNull())
                        displayed_badges.Add(userinfo.displayed_badge_ids.Trim());
                }
                else
                {
                    var displayed_badges_temp1 = userinfo!.displayed_badge_ids.Split(";");
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
                        settemp1 += x + ";";
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
        private static void Info(Target target, string cmd)
        {
            int badgeNum = -1;
            if (int.TryParse(cmd, out badgeNum))
            {
                var userinfo = Database.Client.GetUsersByUID(target.account!, target.platform);
                if (userinfo!.owned_badge_ids.IsNull())
                {
                    target.reply("你还没有牌子呢..."); return;
                }

                //获取已拥有的牌子
                List<string> owned_badges = new();
                if (userinfo.owned_badge_ids.IndexOf(";") < 1)
                {
                    owned_badges.Add(userinfo.owned_badge_ids.Trim());
                }
                else
                {
                    var owned_badges_temp1 = userinfo.owned_badge_ids.Split(";");
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
        private static void List(Target target, string cmd)
        {
            var userinfo = Database.Client.GetUsersByUID(target.account!, target.platform);
            if (userinfo!.owned_badge_ids.IsNull())
            {
                target.reply("你还没有牌子呢..."); return;
            }

            //获取已拥有的牌子
            List<string> owned_badges = new();
            if (userinfo.owned_badge_ids.IndexOf(";") < 1)
            {
                owned_badges.Add(userinfo.owned_badge_ids.Trim());
            }
            else
            {
                var owned_badges_temp1 = userinfo.owned_badge_ids.Split(";");
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

        private static void SudoCreate(Accounts.AccInfo AccInfo, Target target, string cmd)
        {

        }
        private static void SudoDelete(Accounts.AccInfo AccInfo, Target target, string cmd)
        {

        }
        private static void SudoGetUser(Accounts.AccInfo AccInfo, Target target, string cmd)
        {

        }
        private static void SudoAdd(Accounts.AccInfo AccInfo, Target target, string cmd)
        {

        }
        private static void SudoRemove(Accounts.AccInfo AccInfo, Target target, string cmd)
        {

        }
        private static void SudoList(Accounts.AccInfo AccInfo, Target target, string cmd)
        {

        }
    }
}
