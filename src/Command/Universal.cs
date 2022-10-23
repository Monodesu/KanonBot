using KanonBot.Drivers;
using KanonBot.functions;
using KanonBot.functions.osu;
using KanonBot.functions.osubot;

namespace KanonBot.command_parser
{
    public static class Universal
    {
        public static async Task Parser(Target target)
        {
            string? cmd = null;
            var msg = target.msg;
            if (msg.StartsWith(new Message.AtSegment(target.account!, target.platform)))
                msg = Message.Chain.FromList(msg.ToList().Slice(1, msg.Length()));

            if (msg.StartsWith("!") || msg.StartsWith("/") || msg.StartsWith("！"))
            {
                cmd = msg.Build();
                cmd = cmd.Substring(1); //删除命令唤起符
            }


            if (cmd != null)
            {
                cmd = cmd.ToLower(); //转小写
                cmd = Utils.ToDBC(cmd); //转半角
                // cmd = Utils.ParseAt(cmd);

                // 有些例外，用StartsWith匹配
                if (cmd.StartsWith("bp"))
                {
                    if (cmd.StartsWith("bpme")) return;
                    await BestPerformance.Execute(target, cmd[2..].Trim());
                    return;
                }

                string rootCmd, childCmd = "";
                try
                {
                    rootCmd = cmd[..cmd.IndexOf(" ")].Trim();
                    childCmd = cmd[(cmd.IndexOf(" ") + 1)..].Trim();
                }
                catch { rootCmd = cmd; }

                //Console.WriteLine(target.account + target.platform);
                try
                {
                    switch (rootCmd)
                    {
                        //case "test": Test.run(target, childCmd); return;
                        case "reg": await Accounts.RegAccount(target, childCmd); return;
                        case "bind": await Accounts.BindService(target, childCmd); return;
                        case "info": await Info.Execute(target, childCmd); return;
                        case "recent": await Recent.Execute(target, childCmd, true); return;
                        case "re": await Recent.Execute(target, childCmd, true); return;
                        case "pr": await Recent.Execute(target, childCmd, false); return;
                        case "bp": await BestPerformance.Execute(target, childCmd); return;
                        case "score": await Score.Execute(target, childCmd); return;
                        case "help": Help.Execute(target, childCmd); return;
                        case "update": await Update.Execute(target, childCmd); return;
                        case "get": await Get.Execute(target, childCmd); return;// get bonuspp/elo/rolecost/bpht/todaybp/annualpass
                        case "badge": await Badge.Execute(target, childCmd); return;
                        case "leeway": await Leeway.Execute(target, childCmd); return;
                        case "set": await Set.Execute(target, childCmd); return;
                        case "ppvs":
                            return;

                        // Admin
                        case "sudo": //管理员
                            return;
                        case "su": //超级管理员
                            await Su.Execute(target, childCmd);
                            return;
                        case "dailyupdate":
                            return;
                        default: return;
                    }
                }
                catch (Flurl.Http.FlurlHttpTimeoutException)
                {
                    target.reply("API访问超时，请稍后重试。");
                }
                catch (Flurl.Http.FlurlHttpException ex)
                {
                    target.reply("网络出现错误！错误内容已自动上报。");
                    var rtmp =
                        $"Message: {ex.Message}\r\n" +
                        $"Source: {ex.Source}\r\n" +
                        $"StackTrace: {ex.StackTrace}";
                    Utils.SendDebugMail("mono@desu.life", rtmp);
                    Utils.SendDebugMail("fantasyzhjk@qq.com", rtmp);
                }
                catch (Exception ex)
                {
                    target.reply("出现了未知错误，错误内容已自动上报。");
                    var rtmp =
                        $"Message: {ex.Message}\r\n" +
                        $"Source: {ex.Source}\r\n" +
                        $"StackTrace: {ex.StackTrace}";
                    Utils.SendDebugMail("mono@desu.life", rtmp);
                    Utils.SendDebugMail("fantasyzhjk@qq.com", rtmp);
                }

            }
            else { return; }
        }
    }
}
