using KanonBot.Drivers;
using KanonBot.Message;
using KanonBot.API;

namespace KanonBot.functions.osu
{
    public class Info
    {
        public static void Execute(Target target, string cmd)
        {
            LegacyImage.Draw.UserPanelData data = new();
            int bannerStatus = 0;
            bool isBonded = false;

            // 解析指令
            var command = BotCmdHelper.CmdParser(cmd, BotCmdHelper.Func_type.Info);


            if (command.selfquery)
            {
                // 验证账户
                var AccInfo = Accounts.GetAccInfo(target);
                if (Accounts.CheckAccount(AccInfo.uid, AccInfo.platform) == -1)
                { target.reply(new Chain().msg("您还没有绑定Kanon账户，请使用!reg 您的邮箱来进行绑定或注册。")); return; }

                // 验证osu信息
                var DBOsuInfo = Accounts.CheckOsuAccount(Database.Client.GetUsersByUID(AccInfo.uid, AccInfo.platform)!.uid);
                if (DBOsuInfo == null)
                { target.reply(new Chain().msg("您还没有绑定osu账户，请使用!set osu 您的osu用户名来绑定您的osu账户。")); return; }

                var OnlineOSUInfo = Osu.GetUser(DBOsuInfo.osu_uid);
                if (DBOsuInfo == null)
                { target.reply(new Chain().msg("被办了。")); return; }

                isBonded = true;

                if (command.order_number > 0)
                {
                    // 从数据库取指定天数前的记录
                    // DB.GetOsuUserData(out data.prevUserInfo, oid, mode, days))
                }
                else
                {
                    // 从数据库取最近的一次记录
                    // DB.GetOsuUserData(out data.prevUserInfo, oid, mode, days))
                }

                switch (DBOsuInfo.customInfoEngineVer)
                {
                    case 1:
                        //new
                        break;
                    default:
                        //legacy
                        if (command.osu_mode == "") data.userInfo.mode = DBOsuInfo.osu_mode ?? "osu";// 取mode信息

                        // 取PP+信息
                        // if (mode == "osu") DB.GetOsuPPlusData(out data.pplusInfo, oid);

                        var badgeID = DBOsuInfo.displayed_badge_ids;
                        // legacy只取第一个badge
                        if (badgeID != null)
                            if (badgeID.IndexOf(",") != -1) badgeID = badgeID[..badgeID.IndexOf(",")];

                        bannerStatus = DBOsuInfo.customBannerStatus;// 取bannerStatus

                        data.userInfo = OnlineOSUInfo;
                        //data.prevUserInfo = xxx; //取之前数据
                        data.prevUserInfo.daysBefore = 0;
                        break;
                }
            }
            else
            {
            }
            var isDataOfDayAvaiavle = false;
            if (data.prevUserInfo.daysBefore > 0) isDataOfDayAvaiavle = true;
            MemoryStream img = LegacyImage.Draw.DrawInfo(data, bannerStatus, isBonded, isDataOfDayAvaiavle);
            img.TryGetBuffer(out ArraySegment<byte> buffer);
            target.reply(new Chain().msg("test").image(Convert.ToBase64String(buffer.Array!, 0, (int)img.Length), ImageSegment.Type.Base64));
            //AnnualPass(data.userInfo.userId, data.userInfo.mode, data.userInfo.totalHits); //季票内容
        }
    }
}
