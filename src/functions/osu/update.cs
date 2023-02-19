using KanonBot.Drivers;
using KanonBot.Message;
using KanonBot.API;
using KanonBot.functions.osu;
using System.IO;
using LanguageExt.UnsafeValueAccess;

namespace KanonBot.functions.osubot
{
    public class Update
    {
        async public static Task Execute(Target target, string cmd)
        {
            #region 验证
            long? osuID = null;
            OSU.Enums.Mode? mode;
            Database.Model.User? DBUser = null;
            Database.Model.UserOSU? DBOsuInfo = null;

            // 解析指令
            var command = BotCmdHelper.CmdParser(cmd, BotCmdHelper.FuncType.Info);
            mode = command.osu_mode;

            // 解析指令
            if (command.self_query)
            {
                // 验证账户
                var AccInfo = Accounts.GetAccInfo(target);
                DBUser = await Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
                if (DBUser == null)
                // { await target.reply("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。"); return; }    // 这里引导到绑定osu
                { await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }
                // 验证账号信息
                DBOsuInfo = await Accounts.CheckOsuAccount(DBUser.uid);
                if (DBOsuInfo == null)
                { await target.reply("您还没有绑定osu账户，请使用!bind osu 您的osu用户名 来绑定您的osu账户。"); return; }

                mode ??= OSU.Enums.String2Mode(DBOsuInfo.osu_mode)!.Value;    // 从数据库解析，理论上不可能错
                osuID = DBOsuInfo.osu_uid;
            }
            else
            {
                // 查询用户是否绑定
                var (atOSU, atDBUser) = await Accounts.ParseAt(command.osu_username);
                if (atOSU.IsNone && !atDBUser.IsNone) {
                    await target.reply("ta还没有绑定osu账户呢。");
                    return;
                } else if (!atOSU.IsNone && atDBUser.IsNone) {
                    var _osuinfo = atOSU.ValueUnsafe();
                    mode ??= _osuinfo.PlayMode;
                    osuID = _osuinfo.Id;
                } else if (!atOSU.IsNone && !atDBUser.IsNone) {
                    DBUser = atDBUser.ValueUnsafe();
                    DBOsuInfo = await Accounts.CheckOsuAccount(DBUser.uid);
                    var _osuinfo = atOSU.ValueUnsafe();
                    mode ??= OSU.Enums.String2Mode(DBOsuInfo!.osu_mode)!.Value;
                    osuID = _osuinfo.Id;
                } else {
                    // 普通查询
                    var tempOsuInfo = await OSU.GetUser(command.osu_username, command.osu_mode ?? OSU.Enums.Mode.OSU);
                    if (tempOsuInfo != null)
                    {
                        DBOsuInfo = await Database.Client.GetOsuUser(tempOsuInfo.Id);
                        if (DBOsuInfo != null)
                        {
                            DBUser = await Accounts.GetAccountByOsuUid(tempOsuInfo.Id);
                            mode ??= OSU.Enums.String2Mode(DBOsuInfo.osu_mode)!.Value;
                        }
                        mode ??= tempOsuInfo.PlayMode;
                        osuID = tempOsuInfo.Id;
                    }
                    else
                    {
                        // 直接取消查询，简化流程
                        await target.reply("猫猫没有找到此用户。");
                        return;
                    }
                }
            }

            // 验证osu信息
            var OnlineOsuInfo = await OSU.GetUser(osuID!.Value, mode!.Value);
            if (OnlineOsuInfo == null)
            {
                if (DBOsuInfo != null)
                    await target.reply("被办了。");
                else
                    await target.reply("猫猫没有找到此用户。");
                // 中断查询
                return;
            }
            OnlineOsuInfo.PlayMode = mode!.Value;
            #endregion

            await target.reply("少女祈祷中...");
            //try { File.Delete($"./work/v1_cover/{OnlineOsuInfo!.Id}.png"); } catch { }
            try { File.Delete($"./work/avatar/{OnlineOsuInfo!.Id}.png"); } catch { }
            try { File.Delete($"./work/legacy/v1_cover/osu!web/{OnlineOsuInfo!.Id}.png"); } catch { }
            await target.reply("主要数据已更新完毕，pp+数据正在后台更新，请稍后使用info功能查看结果。");

            try { await Database.Client.UpdateOsuPPlusData((await API.OSU.TryGetUserPlusData(OnlineOsuInfo!))!.User, OnlineOsuInfo!.Id); }
            catch { }//更新pp+失败，不返回信息
        }
    }
}
