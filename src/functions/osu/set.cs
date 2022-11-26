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
                case "osuinfopanelversion":
                    await Osu_InfoPanelVersion(target, childCmd);
                    break;
                case "osuinfopanelv2colormode":
                    await Osu_InfoPanelV2ColorMode(target, childCmd);
                    break;
                default:
                    target.reply("!set osumode/osuinfopanelversion/osuinfopanelv1img(not enabled)/osuinfopanelv2img(not enabled)/osuinfopanelv1panel(not enabled)/osuinfopanelv2panel(not enabled)/osuinfopanelv2colormode");
                    return;
            }
        }
        async public static Task Osu_InfoPanelVersion(Target target, string cmd)
        {
            var AccInfo = Accounts.GetAccInfo(target);
            var DBUser = await Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
            if (DBUser == null)
            { target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }
            var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
            var DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
            if (DBOsuInfo == null)
            { target.reply("您还没有绑定osu账户，请使用!set osu 您的osu用户名来绑定您的osu账户。"); return; }

            cmd = cmd.ToLower().Trim();
            if (int.TryParse(cmd, out _))
            {
                try
                {
                    var t = int.Parse(cmd);
                    if (t < 1 || t > 2)
                    {
                        target.reply("版本号不正确。(1、2)");
                        return;
                    }
                    await Database.Client.SetOsuInfoPanelVersion(DBOsuInfo.osu_uid, t);
                    target.reply("成功设置infopanel版本。");
                }
                catch
                {
                    target.reply("发生了错误，无法设置infopanel版本，请联系管理员。");
                }
            }
            else
            {
                target.reply("版本号不正确。(1、2)");
            }
        }
        async public static Task Osu_InfoPanelV2ColorMode(Target target, string cmd)
        {
            var AccInfo = Accounts.GetAccInfo(target);
            var DBUser = await Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
            if (DBUser == null)
            { target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }
            var _u = await Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform);
            var DBOsuInfo = (await Accounts.CheckOsuAccount(_u!.uid))!;
            if (DBOsuInfo == null)
            { target.reply("您还没有绑定osu账户，请使用!set osu 您的osu用户名来绑定您的osu账户。"); return; }

            cmd = cmd.ToLower().Trim();
            if (int.TryParse(cmd, out _))
            {
                try
                {
                    var t = int.Parse(cmd);
                    if (t < 0 || t > 1)
                    {
                        target.reply("配色方案号码不正确。(0=light、1=dark)");
                        return;
                    }
                    await Database.Client.SetOsuInfoPanelV2ColorMode(DBOsuInfo.osu_uid, t);
                    target.reply("成功设置配色方案。");
                }
                catch
                {
                    target.reply("发生了错误，无法设置配色方案，请联系管理员。");
                }
            }
            else
            {
                target.reply("配色方案号码不正确。(0=light、1=dark)");
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
