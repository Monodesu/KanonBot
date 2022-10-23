using KanonBot.Drivers;
using KanonBot.Message;
using KanonBot.API;

namespace KanonBot.functions.osubot
{
    public class Update
    {
        async public static Task Execute(Target target, string cmd)
        {
            #region 验证
            bool is_bounded = false;
            Database.Model.User? DBUser = null;
            Database.Model.UserOSU? DBOsuInfo = null;
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
                DBOsuInfo = await Accounts.CheckOsuAccount(_u!.uid);
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
                    DBUser = await Accounts.GetAccount(OnlineOsuInfo.Id);
                    command.osu_mode ??= OSU.Enums.ParseMode(DBOsuInfo.osu_mode);
                    OnlineOsuInfo = await OSU.GetUser(command.osu_username, command.osu_mode ?? OSU.Enums.Mode.OSU)!;   // 这里正常是能查询到的，所以用非空处理(!)
                }
            }
            #endregion

            target.reply("少女祈祷中...");
            try { File.Delete($"./work/v1_cover/{OnlineOsuInfo!.Id}.png"); } catch { }
            try { File.Delete($"./work/avatar/{OnlineOsuInfo!.Id}.png"); } catch { }
            target.reply("主要数据已更新完毕，pp+数据正在后台更新，请稍后使用info功能查看结果。");

            try { await Database.Client.UpdateOsuPPlusData((await API.OSU.GetUserPlusData(OnlineOsuInfo!.Id)).User, OnlineOsuInfo!.Id); }
            catch { }//更新pp+失败，不返回信息
        }
    }
}
