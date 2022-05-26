using KanonBot.Drivers;
using KanonBot.functions;
using KanonBot.functions.osubot;

namespace KanonBot.command_parser
{
    public static class Universal
    {
        public static void Parser(Target target)
        {
            string? cmd = null;
            var msg = target.msg;
            if (msg.StartsWith(new Message.AtSegment(target.account!, target.platform!.Value)))
                msg = Message.Chain.FromList(msg.ToList().Slice(1, msg.Length()));

            if (msg.StartsWith("!") || msg.StartsWith("/") || msg.StartsWith("！"))
            {
                cmd = msg.Build();
                cmd = cmd.Substring(1); //删除命令唤起符
                // cmd = cmd[0] < 0 ? cmd[3..] : cmd[1..]; // c#用utf8编码，无需处理中文
            }


            if (cmd != null)
            {
                cmd = cmd.ToLower(); //转小写
                cmd = Utils.ToDBC(cmd); //转半角
                // cmd = Utils.ParseAt(cmd);

                string rootCmd, childCmd = "";
                try
                {
                    rootCmd = cmd[..cmd.IndexOf(" ")].Trim();
                    childCmd = cmd[(cmd.IndexOf(" ") + 1)..].Trim();
                }
                catch { rootCmd = cmd; }
                switch (rootCmd)
                {
                    case "test": Test.run(target, childCmd); return;
                    case "reg": Accounts.RegAccount(target, childCmd); return;
                    case "bind": Accounts.BindService(target, childCmd); return;
                    case "info": Info.Execute(target, childCmd); return;
                    case "recent": Recent.Execute(target, childCmd, true); return;
                    case "re": Recent.Execute(target, childCmd, true); return;
                    case "pr": Recent.Execute(target, childCmd, false); return;
                    case "bp": BestPerformance.Execute(target, childCmd); return;
                    case "score": Score.Execute(target, childCmd); return;
                    case "help": Help.Execute(target, childCmd); return;
                    case "update": Update.Execute(target, childCmd); return;
                    case "get": // get bonuspp/elo/rolecost/bpht/todaybp/annualpass
                        return;
                    case "set": // set osu_mode/osu_infopanel
                        return;
                    case "badge":
                        return;
                    case "ppvs":
                        return;


                    // Admin
                    case "sudo": //管理员
                        return;
                    case "su": //超级管理员
                        return;
                    case "dailyupdate":
                        return;
                    default: return;
                }
            }
            else { return; }
        }
    }
}
