using static KanonBot.functions.Accounts;
using KanonBot.Drivers;

namespace KanonBot.functions.osu
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
                            permissions_flag = -1;
                            break;
                    }

                }
                foreach (var x in permissions)
                {
                    Console.WriteLine(x);
                }
                Console.WriteLine(permissions + "\r\n" + permissions_flag);

                if (permissions_flag != 3) return; //权限不够不处理

                //execute
                string rootCmd, childCmd = "";
                try
                {
                    rootCmd = cmd[..cmd.IndexOf(" ")].Trim();
                    childCmd = cmd[(cmd.IndexOf(" ") + 1)..].Trim();
                }
                catch { rootCmd = cmd; }

                switch (rootCmd.ToLower())
                {
                    case "updateall":
                        await SuDailyUpdateAsync(target); return;
                    default:
                        return;
                }
            }
            catch { }//直接忽略
        }

        public static async Task SuDailyUpdateAsync(Target target)
        {
            target.reply("已手动开始数据更新，稍后会发送结果。");
            var (count, span) = await GeneralUpdate.UpdateUsers();
            var Text = "共用时";
            if (span.Hours > 0) Text += $" {span.Hours} 小时";
            if (span.Minutes > 0) Text += $" {span.Minutes} 分钟";
            Text += $" {span.Seconds} 秒";
            try
            {
                target.reply($"数据更新完成，一共更新了 {count} 个用户\n{Text}");
            }
            catch
            {
                target.reply($"数据更新完成\n{Text}");
            }
        }



    }
}
