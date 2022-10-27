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
                case "osumode":
                    await Osu_mode(target, childCmd);
                    break;
                default:
                    target.reply("!set osumode");
                    return;
            }
        }

        async public static Task Osu_mode(Target target, string cmd)
        {
            var AccInfo = Accounts.GetAccInfo(target);
            var DBUser = await Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
            if (DBUser == null)
            // { target.reply("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。"); return; }    // 这里引导到绑定osu
            { target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }
            var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
            var DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
            if (DBOsuInfo == null)
            { target.reply("您还没有绑定osu账户，请使用!set osu 您的osu用户名来绑定您的osu账户。"); return; }

            cmd = cmd.ToLower().Trim();

            var mode = OSU.Enums.ParseMode(cmd);
            if (mode == null)
            {
                target.reply("提供的模式不正确，请重新确认 (osu/taiko/fruits/mania)");
                return;
            }
            else
            {
                try
                {
                    await Database.Client.SetOsuUserMode(DBOsuInfo.osu_uid, mode.Value);
                    target.reply("成功设置模式为 " + cmd);
                }
                catch
                {
                    target.reply("发生了错误，无法设置osu模式，请联系管理员。");
                }
            }
        }
    }
}
