using Flurl.Util;
using KanonBot.Drivers;
using KanonBot.Functions.osubot;
using static KanonBot.Functions.Accounts;

namespace KanonBot.Functions.OSU
{
    public static class Su
    {
        public static async Task Execute(Target target, string cmd)
        {
            try
            {
                var AccInfo = GetAccInfo(target);
                var userinfo = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
                if (userinfo == null)
                {
                    return;//直接忽略
                }
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
                        case "restricted":
                            permissions_flag = -3;
                            break;
                        case "banned":
                            permissions_flag = -1;
                            break;
                        case "user":
                            if (permissions_flag < 1) permissions_flag = 1;
                            break;
                        case "mod":
                            if (permissions_flag < 2) permissions_flag = 2;
                            break;
                        case "admin":
                            if (permissions_flag < 3) permissions_flag = 3;
                            break;
                        case "system":
                            permissions_flag = -2;
                            break;
                        default:
                            break;
                    }

                }

                if (permissions_flag != 3) return; //权限不够不处理

                //execute
                string rootCmd, childCmd = "";
                try
                {
                    var tmp = cmd.SplitOnFirstOccurence(" ");
                    rootCmd = tmp[0].Trim();
                    childCmd = tmp[1].Trim();
                }
                catch { rootCmd = cmd; }

                switch (rootCmd.ToLower())
                {
                    case "updateall":
                        await SuDailyUpdateAsync(target);
                        return;
                    case "restrict_user_byoid":
                        await RestriceUser(target, childCmd, 1);
                        return;
                    case "restrict_user_byemail":
                        await RestriceUser(target, childCmd, 2);
                        return;
                    default:
                        return;
                }
            }
            catch { }//直接忽略
        }


        public static async Task RestriceUser(Target target, string cmd, int bywhat) //1=byoid 2=byemail
        {
            //SetOsuUserPermissionByOid
            switch (bywhat)
            {
                case 1:
                    if (await Database.Client.GetUserByOsuUID(long.Parse(cmd)) == null)
                    {

                        await target.reply($"该用户未注册desu.life账户或osu!账户不存在，请重新确认");
                        return;
                    }
                    await Database.Client.SetOsuUserPermissionByOid(long.Parse(cmd), "restricted");
                    await target.reply($"restricted");
                    return;
                case 2:
                    if (await Database.Client.GetUser(cmd) == null)
                    {
                        await target.reply($"该用户未注册desu.life账户，请重新确认");
                        return;
                    }
                    await Database.Client.SetOsuUserPermissionByEmail(cmd, "restricted");
                    await target.reply($"restricted");
                    return;
                default:
                    return;
            }
        }

        public static async Task SuDailyUpdateAsync(Target target)
        {
            await target.reply("已手动开始数据更新，稍后会发送结果。");
            var (count, span) = await GeneralUpdate.UpdateUsers();
            var Text = "共用时";
            if (span.Hours > 0) Text += $" {span.Hours} 小时";
            if (span.Minutes > 0) Text += $" {span.Minutes} 分钟";
            Text += $" {span.Seconds} 秒";
            try
            {
                await target.reply($"数据更新完成，一共更新了 {count} 个用户\n{Text}");
            }
            catch
            {
                await target.reply($"数据更新完成\n{Text}");
            }
        }



    }
}
