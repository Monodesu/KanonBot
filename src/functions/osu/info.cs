using KanonBot.Drivers;
using KanonBot.Message;
using KanonBot.API;

namespace KanonBot.functions.osubot
{
    public class Info
    {
        async public static void Execute(Target target, string cmd)
        {
            #region 验证
            LegacyImage.Draw.UserPanelData data = new();
            int bannerStatus = 0;
            bool is_bounded = false;
            Database.Model.Users DBUser = new();
            Database.Model.Users_osu DBOsuInfo;
            OSU.UserInfo OnlineOsuInfo;

            // 解析指令
            var command = BotCmdHelper.CmdParser(cmd, BotCmdHelper.Func_type.Info);

            // 取mode信息
            if (command.osu_mode != "") data.userInfo.mode = command.osu_mode;


            // 解析指令
            if (command.selfquery)
            {
                // 验证账户
                var AccInfo = Accounts.GetAccInfo(target);
                DBUser = Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
                if (DBUser == null)
                { target.reply("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。"); return; }

                // 验证osu信息
                DBOsuInfo = Accounts.CheckOsuAccount(Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform)!.uid)!;
                if (DBOsuInfo == null)
                { target.reply("您还没有绑定osu账户，请使用!set osu 您的osu用户名来绑定您的osu账户。"); return; }

                // 取mode信息
                if (command.osu_mode == "") data.userInfo.mode = DBOsuInfo.osu_mode ?? "osu";

                // 验证osu信息
                try { OnlineOsuInfo = await OSU.GetUserLegacy(DBOsuInfo.osu_uid, data.userInfo.mode); }
                catch { OnlineOsuInfo = new OSU.UserInfo(); }
                is_bounded = true;
            }
            else
            {
                // 取mode信息
                var temp_mode_has_value = false;
                if (command.osu_mode == "") data.userInfo.mode = "osu"; else temp_mode_has_value = true;
                // 验证osu信息
                try { OnlineOsuInfo = await OSU.GetUserLegacy(command.osu_username, data.userInfo.mode); }
                catch { OnlineOsuInfo = new OSU.UserInfo(); }
                var temp_uid = Database.Client.GetOSUUsers(OnlineOsuInfo.userId);
                DBOsuInfo = Accounts.CheckOsuAccount(temp_uid == null ? -1 : temp_uid.uid)!;
                if (DBOsuInfo != null)
                {
                    is_bounded = true;
                    DBUser = Accounts.GetAccount(OnlineOsuInfo.userId);
                    if (!temp_mode_has_value)
                    {
                        data.userInfo.mode = DBOsuInfo.osu_mode ?? "osu";
                        try { OnlineOsuInfo = await OSU.GetUserLegacy(command.osu_username, data.userInfo.mode); }
                        catch { OnlineOsuInfo = new OSU.UserInfo(); }
                    }
                }
            }

            // 验证osu信息
            if (OnlineOsuInfo.userName == null)
            {
                if (is_bounded) { target.reply("被办了。"); return; }
                target.reply("猫猫没有找到此用户。"); return;
            }
            #endregion

            #region 获取信息

            data.userInfo = OnlineOsuInfo;

            // 查询
            if (command.order_number > 0 && is_bounded)
            {
                // 从数据库取指定天数前的记录
                // DB.GetOsuUserData(out data.prevUserInfo, oid, mode, days))
            }
            else
            {
                // 从数据库取最近的一次记录
                // DB.GetOsuUserData(out data.prevUserInfo, oid, mode, days))
            }

            if (is_bounded)
            {
                switch (DBOsuInfo!.customInfoEngineVer)
                {
                    case 1:
                        //new
                        break;
                    default:
                        //legacy
                        // 取mode信息


                        // 取PP+信息
                        // if (mode == "osu") DB.GetOsuPPlusData(out data.pplusInfo, oid);

                        var badgeID = DBUser!.displayed_badge_ids;
                        // legacy只取第一个badge
                        if (badgeID != null)
                            try { if (badgeID.IndexOf(",") != -1) badgeID = badgeID[..badgeID.IndexOf(",")]; }
                            catch { badgeID = "-1"; }
                        try { data.badgeId = int.Parse(badgeID!); }
                        catch { data.badgeId = -1; }
                        bannerStatus = DBOsuInfo.customBannerStatus;// 取bannerStatus


                        //data.prevUserInfo = xxx; //取之前数据
                        data.prevUserInfo.daysBefore = 0;
                        break;
                }
            }
            else
            {
                // 未绑定用户默认用新面板

            }
            #endregion

            var isDataOfDayAvaiavle = false;
            if (data.prevUserInfo.daysBefore > 0) isDataOfDayAvaiavle = true;
            MemoryStream img = LegacyImage.Draw.DrawInfo(data, bannerStatus, is_bounded, isDataOfDayAvaiavle);
            img.TryGetBuffer(out ArraySegment<byte> buffer);
            target.reply(new Chain().msg("test").image(Convert.ToBase64String(buffer.Array!, 0, (int)img.Length), ImageSegment.Type.Base64));
            //AnnualPass(data.userInfo.userId, data.userInfo.mode, data.userInfo.totalHits); //季票内容
        }
    }
}
