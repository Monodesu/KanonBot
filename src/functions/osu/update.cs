using KanonBot.Drivers;
using KanonBot.Message;
using KanonBot.API;

namespace KanonBot.functions.osubot
{
    public class Update
    {
        async public static void Execute(Target target, string cmd)
        {
            #region 验证
            Osu.UserInfo OnlineOsuInfo;
            Database.Model.Users_osu DBOsuInfo;
            bool is_bounded;

            // 解析指令
            var command = BotCmdHelper.CmdParser(cmd, BotCmdHelper.Func_type.BestPerformance);
            if (command.selfquery)
            {
                // 验证账户
                var AccInfo = Accounts.GetAccInfo(target);
                if (Accounts.GetAccount(AccInfo.uid, AccInfo.platform)!.uid == -1)
                { target.reply("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。"); return; }

                // 验证osu信息
                DBOsuInfo = Accounts.CheckOsuAccount(Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform)!.uid)!;
                if (DBOsuInfo == null)
                { target.reply("您还没有绑定osu账户，请使用!set osu 您的osu用户名来绑定您的osu账户。"); return; }

                // 验证osu信息
                try { OnlineOsuInfo = await Osu.GetUser(DBOsuInfo.osu_uid); }
                catch { OnlineOsuInfo = new Osu.UserInfo(); }
                is_bounded = true;
            }
            else
            {
                // 验证osu信息
                try { OnlineOsuInfo = await Osu.GetUser(command.osu_username); }
                catch { OnlineOsuInfo = new Osu.UserInfo(); }
                is_bounded = false;
            }

            // 验证osu信息
            if (OnlineOsuInfo.userName == null)
            {
                if (is_bounded) { target.reply("被办了。"); return; }
                target.reply("猫猫没有找到此用户。"); return;
            }
            #endregion

            target.reply("少女祈祷中...");
            try { File.Delete($"./work/v1_cover/{OnlineOsuInfo.userId}.png"); } catch { }
            try { File.Delete($"./work/avatar/{OnlineOsuInfo.userId}.png"); } catch { }
            target.reply("主要数据已更新完毕，pp+数据正在后台更新，请稍后使用info功能查看结果。");

            try { Database.Client.UpdateOsuPPlusData(await API.Osu.GetUserPlusData(OnlineOsuInfo.userId), OnlineOsuInfo.userId); }
            catch { }//更新pp+失败，不返回信息
        }
    }
}
