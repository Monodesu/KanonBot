using KanonBot.Drivers;
using KanonBot.Message;
using KanonBot.API;

namespace KanonBot.functions.osubot
{
    public class Set
    {
        public static async Task Execute(Target target, string cmd)
        {
            string rootCmd, childCmd = "";
            try
            {
                rootCmd = cmd[..cmd.IndexOf(" ")].Trim();
                childCmd = cmd[(cmd.IndexOf(" ") + 1)..].Trim();
            }
            catch { rootCmd = cmd; }
            switch (rootCmd)
            {
                case "osu_mode":
                    await Osu_mode(target, childCmd);
                    break;
                default:
                    return;
            }
        }

        async public static Task Osu_mode(Target target, string cmd)
        {
            #region 验证
            bool is_bounded = false;
            Database.Model.User? DBUser;
            Database.Model.UserOSU? DBOsuInfo;
            OSU.Models.User? OnlineOsuInfo;

            // 解析指令
            var command = BotCmdHelper.CmdParser(cmd, BotCmdHelper.Func_type.Info);

            // 解析指令
            if (command.selfquery)
            {
                // 验证账户
                var AccInfo = Accounts.GetAccInfo(target);
                DBUser = await Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
                if (DBUser == null)
                { target.reply("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。"); return; }

                // 验证osu信息
                var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
                DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
                if (DBOsuInfo == null)
                { target.reply("您还没有绑定osu账户，请使用!set osu 您的osu用户名来绑定您的osu账户。"); return; }

                command.osu_mode ??= OSU.Enums.ParseMode(DBOsuInfo.osu_mode);
                // 验证osu信息
                OnlineOsuInfo = await OSU.GetUser(DBOsuInfo.osu_uid, command.osu_mode!.Value);
                is_bounded = true;
            }
            else
            {
                // 验证osu信息
                OnlineOsuInfo = await OSU.GetUser(command.osu_username, command.osu_mode ?? OSU.Enums.Mode.OSU);
                is_bounded = false;
            }

            // 验证osu信息
            if (OnlineOsuInfo == null)
            {
                if (is_bounded) { target.reply("被办了。"); return; }
                target.reply("猫猫没有找到此用户。"); return;
            }

            if (!is_bounded) // 未绑定用户回数据库查询找模式
            {
                var temp_uid = await Database.Client.GetOsuUser(OnlineOsuInfo.Id);
                DBOsuInfo = await Accounts.CheckOsuAccount(temp_uid == null ? -1 : temp_uid.uid)!;
                if (DBOsuInfo != null)
                {
                    is_bounded = true;
                    command.osu_mode ??= OSU.Enums.ParseMode(DBOsuInfo.osu_mode);
                }
            }
            #endregion

            cmd = cmd.ToLower().Trim();

            switch (cmd)
            {
                case "osu": break;
                case "taiko": break;
                case "fruits": break;
                case "mania": break;
                default: cmd = "null"; break;
            }
            if (cmd == "null")
            {
                target.reply("提供的模式不正确，请重新确认 (osu/taiko/fruits/mania)");
                return;
            }
            try
            {
                await Database.Client.SetOsuUserMode(OnlineOsuInfo.Id, cmd);
            }
            catch
            {
                target.reply("发生了错误，无法设置osu模式，请联系管理员。");
            }
        }




    }
}
