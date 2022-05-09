using KanonBot.Drivers;
using KanonBot.functions;

namespace KanonBot.command_parser
{
    public static class Universal
    {
        public static void Parser(Target target)
        {
            string cmd;
            if (target.msg.GetList()[0] is Message.AtSegment) { cmd = target.msg.GetList()[1].Build().ToString(); }
            else cmd = target.msg.GetList()[0].Build().ToString();
            if (cmd.IndexOf("!") == 0 || cmd.IndexOf("/") == 0 || cmd.IndexOf("！") == 0)
            {
                cmd = cmd[0] < 0 ? cmd[3..] : cmd[1..]; //删除命令唤起符
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
                    case "reg": Accounts.RegAccount(target, childCmd); return;
                    case "bind": Accounts.BindService(target, childCmd); return;
                    case "info": OSU.Info(target, childCmd); return;
                    case "recent":
                        return;
                    case "re":
                        return;
                    case "pr":
                        return;
                    case "bp":
                        return;
                    case "score":
                        return;
                    case "ppvs":
                        return;
                    case "badge":
                        return;
                    case "help":
                        return;
                    case "set": // set osu_mode/osu_infopanel
                        return;
                    case "update":
                        return;
                    case "get": // get bonuspp/elo/rolecost/bpht/todaybp/annualpass
                        return;
                    // Admin
                    case "sudo":
                        return;
                    case "su":
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
