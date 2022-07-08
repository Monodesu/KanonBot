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
            Database.Model.Users? DBUser = null;
            Database.Model.Users_osu? DBOsuInfo = null;
            OSU.Models.User? OnlineOsuInfo;

            // 解析指令
            var command = BotCmdHelper.CmdParser(cmd, BotCmdHelper.Func_type.Info);

            // 解析指令
            if (command.selfquery)
            {
                // 验证账户
                var AccInfo = Accounts.GetAccInfo(target);
                DBUser = Accounts.GetAccount(AccInfo.uid, AccInfo.platform);
                if (DBUser == null)
                { target.reply("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。"); return; }

                // 验证osu信息
                DBOsuInfo = Accounts.CheckOsuAccount(Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform)!.uid);
                if (DBOsuInfo == null)
                { target.reply("您还没有绑定osu账户，请使用!set osu 您的osu用户名来绑定您的osu账户。"); return; }

                if (command.osu_mode == null) {
                    command.osu_mode = OSU.Enums.ParseMode(DBOsuInfo.osu_mode);
                }

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
                var temp_uid = Database.Client.GetOSUUsers(OnlineOsuInfo.Id);
                DBOsuInfo = Accounts.CheckOsuAccount(temp_uid == null ? -1 : temp_uid.uid)!;
                if (DBOsuInfo != null)
                {
                    is_bounded = true;
                    DBUser = Accounts.GetAccount(OnlineOsuInfo.Id);
                    if (command.osu_mode == null) {
                        command.osu_mode = OSU.Enums.ParseMode(DBOsuInfo.osu_mode);
                    }
                    OnlineOsuInfo = await OSU.GetUser(command.osu_username, command.osu_mode ?? OSU.Enums.Mode.OSU)!;   // 这里正常是能查询到的，所以用非空处理(!)
                }
            }
            #endregion

            #region 获取信息

            data.userInfo = OnlineOsuInfo!;

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
                        data.daysBefore = 0;
                        break;
                }
            }
            else
            {
                // 未绑定用户默认用新面板

            }
            #endregion

            var isDataOfDayAvaiavle = false;
            if (data.daysBefore > 0) isDataOfDayAvaiavle = true;
            MemoryStream img = LegacyImage.Draw.DrawInfo(data, bannerStatus, is_bounded, isDataOfDayAvaiavle);
            img.TryGetBuffer(out ArraySegment<byte> buffer);
            target.reply(new Chain().msg("test").image(Convert.ToBase64String(buffer.Array!, 0, (int)img.Length), ImageSegment.Type.Base64));
            //AnnualPass(data.userInfo.userId, data.userInfo.mode, data.userInfo.totalHits); //季票内容
        }
    }
}
