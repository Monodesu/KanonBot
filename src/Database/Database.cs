using KanonBot.Drivers;
using LinqToDB;
using MySqlConnector;
using static KanonBot.Database.Models;

namespace KanonBot.Database
{
    public class Client
    {
        private static Config.Base config = Config.inner!;

        private static DB GetInstance()
        {
            var options = new DataOptions().UseMySqlConnector(
                new MySqlConnectionStringBuilder
                {
                    Server = config.database!.host,
                    Port = (uint)config.database.port,
                    UserID = config.database.user,
                    Password = config.database.password,
                    Database = config.database.db,
                    CharacterSet = "utf8mb4",
                    CancellationTimeout = 5,
                }.ConnectionString
            );
            // 暂时只有Mysql
            return new DB(options);
        }

        public static async Task<bool> SetVerifyMail(string mailAddr, string verify)
        {
            using var db = GetInstance();
            var newverify = new MailVerify()
            {
                mailAddr = mailAddr,
                verify = verify,
                gen_time = Utils.GetTimeStamp(false)
            };

            try
            {
                await db.InsertAsync(newverify);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> IsRegd(string mailAddr)
        {
            using var db = GetInstance();
            var li = db.User.Where(it => it.email == mailAddr).Select(it => it.uid);
            if (await li.CountAsync() > 0)
                return true;
            return false;
        }

        public static async Task<Models.User?> GetUser(string mailAddr)
        {
            using var db = GetInstance();
            return await db.User.Where(it => it.email == mailAddr).FirstOrDefaultAsync();
        }

        public static async Task<Models.User?> GetUser(int uid)
        {
            using var db = GetInstance();
            return await db.User.Where(it => it.uid == uid).FirstOrDefaultAsync();
        }

        public static async Task<Models.User?> GetUsersByUID(string UID, Platform platform)
        {
            using var db = GetInstance();
            switch (platform)
            {
                case Platform.OneBot:
                    if (long.TryParse(UID, out var qid))
                        return await db.User.Where(it => it.qq_id == qid).FirstOrDefaultAsync();
                    else
                        return null;
                case Platform.Guild:
                    return await db.User.Where(it => it.qq_guild_uid == UID).FirstOrDefaultAsync();
                case Platform.KOOK:
                    return await db.User.Where(it => it.kook_uid == UID).FirstOrDefaultAsync();
                case Platform.Discord:
                    return await db.User.Where(it => it.discord_uid == UID).FirstOrDefaultAsync();
                default:
                    return null;
            }
        }

        public static async Task<Models.User?> GetUserByOsuUID(long osu_uid)
        {
            using var db = GetInstance();
            var user = await GetOsuUser(osu_uid);
            if (user == null)
            {
                return null;
            }
            return await db.User.Where(it => it.uid == user.uid).FirstOrDefaultAsync();
        }

        public static async Task<UserOSU?> GetOsuUser(long osu_uid)
        {
            using var db = GetInstance();
            return await db.UserOSU.Where(it => it.osu_uid == osu_uid).FirstOrDefaultAsync();
        }

        public static async Task<UserOSU?> GetOsuUserByUID(long kanon_uid)
        {
            using var db = GetInstance();
            return await db.UserOSU.Where(it => it.uid == kanon_uid).FirstOrDefaultAsync();
        }

        public static async Task<bool> InsertOsuUser(
            long kanon_uid,
            long osu_uid,
            int customBannerStatus
        )
        {
            using var db = GetInstance();
            var d = new UserOSU()
            {
                uid = kanon_uid,
                osu_uid = osu_uid,
                osu_mode = "osu",
                customInfoEngineVer = 2,
                InfoPanelV2_Mode = 1
            };
            try
            {
                await db.InsertAsync(d);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<API.OSU.Models.PPlusData.UserData?> GetOsuPPlusData(long osu_uid)
        {
            using var db = GetInstance();
            var data = await db.OsuPPlus.FirstOrDefaultAsync(it => it.uid == osu_uid && it.pp != 0);
            if (data != null)
            {
                var realData = new API.OSU.Models.PPlusData.UserData
                {
                    UserId = osu_uid,
                    PerformanceTotal = data.pp,
                    AccuracyTotal = data.acc,
                    FlowAimTotal = data.flow,
                    JumpAimTotal = data.jump,
                    PrecisionTotal = data.pre,
                    SpeedTotal = data.spd,
                    StaminaTotal = data.sta
                };
                return realData;
            }
            else
            {
                return null;
            }
        }

        public static async Task<bool> UpdateOsuPPlusData(
            API.OSU.Models.PPlusData.UserData ppdata,
            long osu_uid
        )
        {
            using var db = GetInstance();
            var data = await db.OsuPPlus.FirstOrDefaultAsync(it => it.uid == osu_uid);
            var result = await db.InsertOrReplaceAsync(
                new OsuPPlus()
                {
                    uid = osu_uid,
                    pp = ppdata.PerformanceTotal,
                    acc = ppdata.AccuracyTotal,
                    flow = ppdata.FlowAimTotal,
                    jump = ppdata.JumpAimTotal,
                    pre = ppdata.PrecisionTotal,
                    spd = ppdata.SpeedTotal,
                    sta = ppdata.StaminaTotal
                }
            );
            return result > -1;
        }

        public static async Task<bool> SetDisplayedBadge(string userid, string displayed_ids)
        {
            using var db = GetInstance();
            var data = await db.User.FirstOrDefaultAsync(it => it.uid == long.Parse(userid));
            var res = await db.User
                .Where(it => it.uid == long.Parse(userid))
                .Set(it => it.displayed_badge_ids, displayed_ids)
                .UpdateAsync();

            return res > -1;
        }

        public static async Task<BadgeList?> GetBadgeInfo(string badgeid)
        {
            using var db = GetInstance();
            return await db.BadgeList.Where(it => it.id == int.Parse(badgeid)).FirstOrDefaultAsync();
        }

        public static async Task<bool> SetOwnedBadge(string email, string? owned_ids)
        {
            using var db = GetInstance();
            var data = await db.User.FirstOrDefaultAsync(it => it.email == email);
            data!.owned_badge_ids = owned_ids;
            var res = await db.UpdateAsync(data);
            return res > -1;
        }

        public static async Task<bool> SetOwnedBadge(int uid, string? owned_ids)
        {
            using var db = GetInstance();
            var data = await db.User.FirstOrDefaultAsync(it => it.uid == uid);
            data!.owned_badge_ids = owned_ids;
            var res = await db.UpdateAsync(data);
            return res > -1;
        }

        public static async Task<bool> SetOwnedBadgeByOsuUid(string osu_uid, string? owned_ids)
        {
            var user = await GetOsuUser(long.Parse(osu_uid));
            if (user == null)
            {
                return false;
            }
            using var db = GetInstance();
            var userinfo = await db.User.Where(it => it.uid == user.uid).FirstOrDefaultAsync();
            userinfo!.owned_badge_ids = owned_ids;
            var res = await db.UpdateAsync(userinfo);
            return res > -1;
        }

        public static async Task<List<long>> GetOsuUserList()
        {
            using var db = GetInstance();
            return await db.UserOSU.Select(it => it.osu_uid).ToListAsync();
        }

        public static async Task<int> InsertOsuUserData(OsuArchivedRec rec, bool is_newuser)
        {
            using var db = GetInstance();
            rec.lastupdate = is_newuser ? DateTime.Today.AddDays(-1) : DateTime.Today;
            return await db.InsertAsync(rec);
        }

        public static async Task<bool> SetOsuUserMode(long osu_uid, API.OSU.Enums.Mode mode)
        {
            using var db = GetInstance();
            var result = await db.UserOSU
                .Where(it => it.osu_uid == osu_uid)
                .Set(it => it.osu_mode, API.OSU.Enums.Mode2String(mode))
                .UpdateAsync();
            return result > -1;
        }

        //返回值为天数（几天前）
        public static async Task<(int, API.OSU.Models.User?)> GetOsuUserData(
            long oid,
            API.OSU.Enums.Mode mode,
            int days = 0
        )
        {
            OsuArchivedRec? data;
            using var db = GetInstance();
            var ui = new API.OSU.Models.User();
            if (days <= 0)
            {
                var q =
                    from p in db.OsuArchivedRec
                    where p.uid == oid && p.gamemode == API.OSU.Enums.Mode2String(mode)
                    orderby p.lastupdate descending
                    select p;
                data = await q.FirstOrDefaultAsync();
            }
            else
            {
                var date = DateTime.Today;
                try
                {
                    date = date.AddDays(-days);
                }
                catch (ArgumentOutOfRangeException)
                {
                    return (-1, null);
                }
                var q =
                    from p in db.OsuArchivedRec
                    where
                        p.uid == oid
                        && p.gamemode == API.OSU.Enums.Mode2String(mode)
                        && p.lastupdate <= date
                    orderby p.lastupdate descending
                    select p;
                data = await q.FirstOrDefaultAsync();
                if (data == null)
                {
                    var tq =
                        from p in db.OsuArchivedRec
                        where p.uid == oid && p.gamemode == API.OSU.Enums.Mode2String(mode)
                        orderby p.lastupdate
                        select p;
                    data = await tq.FirstOrDefaultAsync();
                }
            }
            if (data == null)
                return (-1, null);

            ui.Statistics = new() { GradeCounts = new(), Level = new() };
            ui.Id = oid;
            ui.Statistics.TotalScore = data.total_score;
            ui.Statistics.TotalHits = data.total_hit;
            ui.Statistics.PlayCount = data.play_count;
            ui.Statistics.RankedScore = data.ranked_score;
            ui.Statistics.CountryRank = data.country_rank;
            ui.Statistics.GlobalRank = data.global_rank;
            ui.Statistics.HitAccuracy = data.accuracy;
            ui.Statistics.GradeCounts.SSH = data.count_SSH;
            ui.Statistics.GradeCounts.SS = data.count_SS;
            ui.Statistics.GradeCounts.SH = data.count_SH;
            ui.Statistics.GradeCounts.S = data.count_S;
            ui.Statistics.GradeCounts.A = data.count_A;
            ui.Statistics.Level.Current = data.level;
            ui.Statistics.Level.Progress = data.level_percent;
            ui.Statistics.PP = data.performance_point;
            ui.PlayMode = mode;
            ui.Statistics.PlayTime = data.playtime;
            //ui.daysBefore = (t - data.lastupdate).Days;
            return ((DateTime.Today - data.lastupdate).Days, ui);
        }

        //return badge_id
        public static async Task<int> InsertBadge(
            string ENG_NAME,
            string CHN_NAME,
            string CHN_DECS,
            DateTimeOffset expire_at
        )
        {
            using var db = GetInstance();
            BadgeList bl =
                new()
                {
                    name = ENG_NAME,
                    name_chinese = CHN_NAME,
                    description = CHN_DECS,
                    expire_at = expire_at
                };
            return await db.InsertWithInt32IdentityAsync(bl);
        }

        public static async Task<bool> UpdateSeasonalPass(long oid, string mode, int add_point)
        {
            //检查数据库中有无信息
            using var db = GetInstance();
            var db_info = db.OSUSeasonalPass.Where(it => it.osu_id == oid).Where(it => it.mode == mode);
            if (await db_info.CountAsync() > 0)
            {
                return await db.OSUSeasonalPass
                        .Where(it => it.osu_id == oid && it.mode == mode)
                        .Set(it => it.point, it => it.point + add_point)
                        .UpdateAsync() > -1;
            }
            var t = false;
            if (
                await db.InsertAsync(
                    new OSUSeasonalPass()
                    {
                        point = add_point,
                        mode = mode,
                        osu_id = oid
                    }
                ) > -1
            )
                t = true;
            return t;
        }

        public static async Task<bool> SetOsuInfoPanelVersion(long osu_uid, int ver)
        {
            using var db = GetInstance();
            var result = await db.UserOSU
                .Where(it => it.osu_uid == osu_uid)
                .Set(it => it.customInfoEngineVer, ver)
                .UpdateAsync();
            return result > -1;
        }

        public static async Task<bool> SetOsuInfoPanelV2ColorMode(long osu_uid, int ver)
        {
            using var db = GetInstance();
            var result = await db.UserOSU
                .Where(it => it.osu_uid == osu_uid)
                .Set(it => it.InfoPanelV2_Mode, ver)
                .UpdateAsync();
            return result > -1;
        }

        public static async Task<bool> UpdateInfoPanelV2CustomCmd(long osu_uid, string cmd)
        {
            using var db = GetInstance();
            var result = await db.UserOSU
                .Where(it => it.osu_uid == osu_uid)
                .Set(it => it.InfoPanelV2_CustomMode, cmd)
                .UpdateAsync();
            return result > -1;
        }

        public static async Task<bool> SetOsuUserPermissionByOid(long osu_uid, string permission)
        {
            var DBUser = await GetUserByOsuUID(osu_uid);
            using var db = GetInstance();
            var result = await db.User
                .Where(it => it.uid == DBUser!.uid)
                .Set(it => it.permissions, permission)
                .UpdateAsync();
            return result > -1;
        }

        public static async Task<bool> SetOsuUserPermissionByEmail(string email, string permission)
        {
            using var db = GetInstance();
            var result = await db.User
                .Where(it => it.email == email)
                .Set(it => it.permissions, permission)
                .UpdateAsync();
            return result > -1;
        }

        public static async Task<bool> InsertOsuStandardBeatmapTechData(
            long bid,
            double stars,
            int total,
            int acc,
            int speed,
            int aim,
            int a99,
            int a98,
            int a97,
            int a95,
            string[] mods
        )
        {
            using var db = GetInstance();
            var modstring = "";
            if (mods.Length > 0)
            {
                foreach (var x in mods)
                {
                    if (x == "NC")
                    {
                        modstring += "DT,";
                    }
                    else
                    {
                        modstring += x + ",";
                    }
                    if (x == "PF" || x == "SD" || x == "AP" || x == "RX" || x.ToLower() == "v2")
                        return true; //不保存以上mod
                }
                modstring = modstring[..^1];
            }

            //查找谱面对应的mod数据是否存在
            var db_info = db.OsuStandardBeatmapTechData
                .Where(it => it.bid == bid)
                .Where(it => it.mod == modstring);
            if (await db_info.CountAsync() == 0)
            {
                //不存在再执行添加
                OsuStandardBeatmapTechData t =
                    new()
                    {
                        bid = bid,
                        stars = stars,
                        total = total,
                        acc = acc,
                        speed = speed,
                        aim = aim,
                        mod = modstring,
                        pp_95acc = a95,
                        pp_97acc = a97,
                        pp_98acc = a98,
                        pp_99acc = a99,
                    };
                try
                {
                    await db.InsertAsync(t);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        public static async Task<OSUSeasonalPass?> GetSeasonalPassInfo(long oid, string mode)
        {
            using var db = GetInstance();
            return await db.OSUSeasonalPass
                .Where(it => it.osu_id == oid)
                .Where(it => it.mode == mode)
                .FirstOrDefaultAsync();
        }

        //true=数据库不存在，已成功插入数据，可以进行pt计算
        public static async Task<bool> SeasonalPass_Query_Score_Status(string mode, long score_id)
        {
            using var db = GetInstance();
            var li = db.OSUSeasonalPass_ScoreRecords
                .Where(it => it.score_id == score_id && it.mode == mode)
                .Select(it => it.score_id);
            if (await li.CountAsync() > 0)
                return false;

            //insert
            var d = new OSUSeasonalPass_ScoreRecords() { score_id = score_id, mode = mode };
            try
            {
                await db.InsertAsync(d);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<List<OsuStandardBeatmapTechData>> GetOsuStandardBeatmapTechData(
            int aim,
            int speed,
            int acc,
            int range = 20,
            bool boost = false
        )
        {
            using var db = GetInstance();
            var trange = range;
            if (boost)
                trange = 50;
            return await db.OsuStandardBeatmapTechData
                .Where(
                    it =>
                        it.aim > aim - range / 2
                        && it.aim < aim + trange
                        && it.speed > speed - range / 2
                        && it.speed < speed + trange
                        && it.acc > acc - range / 2
                        && it.acc < acc + trange
                )
                .ToListAsync();
        }

        //true=成功生成代码
        public static async Task<bool> CreateBadgeRedemptionCode(
            int badge_id,
            string code,
            bool can_repeatedly,
            DateTimeOffset expire_at,
            int badge_expire_days
        )
        {
            using var db = GetInstance();
            var li = db.BadgeRedemptionCode.Where(it => it.code == code).Select(it => it.id);
            if (await li.CountAsync() > 0)
                return false;

            //insert
            var d = new BadgeRedemptionCode()
            {
                badge_id = badge_id,
                gen_time = DateTime.Now,
                code = code,
                can_repeatedly = can_repeatedly,
                expire_at = expire_at,
                badge_expiration_day = badge_expire_days
            };
            try
            {
                await db.InsertAsync(d);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public static async Task<BadgeRedemptionCode> RedeemBadgeRedemptionCode(long uid, string code)
        {
            using var db = GetInstance();
            var li = await db.BadgeRedemptionCode.Where(it => it.code == code).FirstOrDefaultAsync();

            if (li == null)
                return null!;
            if (!li.can_repeatedly)
                if (li.redeem_count > 0)
                    return null!;

            return li!;
        }

        public static async Task<bool> SetBadgeRedemptionCodeStatus(int id, long uid, string code)
        {
            using var db = GetInstance();
            var result = await db.BadgeRedemptionCode
                .Where(it => it.id == id)
                .Set(it => it.redeem_time, DateTime.Now)
                .Set(
                    it => it.redeem_user,
                    it =>
                        it.redeem_user == null ? uid.ToString() : it.redeem_user + "," + uid.ToString()
                )
                .Set(it => it.redeem_count, it => it.redeem_count + 1)
                .UpdateAsync();
            if (result > -1)
                return true;
            return false;
        }

        public static async Task<BadgeExpirationDateRec?> GetBadgeExpirationTime(
            int userid,
            int badgeid
        )
        {
            using var db = GetInstance();
            return await db.BadgeExpirationDateRec
                .Where(it => it.uid == userid && it.badge_id == badgeid)
                .FirstOrDefaultAsync();
        }

        public static async Task<List<BadgeExpirationDateRec>?> GetAllBadgeExpirationTime()
        {
            using var db = GetInstance();
            return await db.BadgeExpirationDateRec.ToListAsync();
        }

        public static async Task<bool> UpdateBadgeExpirationTime(
            int userid,
            int badgeid,
            int daysneedtobeadded
        )
        {
            using var db = GetInstance();
            var result = await db.BadgeExpirationDateRec
                .Where(it => it.uid == userid && it.badge_id == badgeid)
                .FirstOrDefaultAsync();
            if (result == null)
            {
                try
                {
                    BadgeExpirationDateRec bed =
                        new()
                        {
                            badge_id = badgeid,
                            uid = userid,
                            expire_at = DateTimeOffset.Now.AddDays(daysneedtobeadded)
                        };
                    await db.InsertAsync(bed);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                try
                {
                    result.expire_at.AddDays(daysneedtobeadded);
                    _ =
                        await db.BadgeExpirationDateRec
                            .Where(it => it.uid == userid && it.badge_id == badgeid)
                            .Set(
                                it => it.expire_at,
                                it => it.expire_at.DateTime.AddDays(daysneedtobeadded)
                            )
                            .UpdateAsync() > -1;
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static async Task<List<Models.User>> GetAllUsersWhoHadBadge()
        {
            using var db = GetInstance();
            return await db.User.Where(it => it.owned_badge_ids != null).ToListAsync();
        }

        public static async Task<List<BadgeList>> GetAllBadges()
        {
            using var db = GetInstance();
            return await db.BadgeList.ToListAsync();
        }

        public static async Task<int> RemoveBadgeExpirationRecord(int userid, int badgeid)
        {
            using var db = GetInstance();
            return await db.BadgeExpirationDateRec
                .Where(x => x.uid == userid && x.badge_id == badgeid)
                .DeleteAsync();
        }

        public static async Task<bool> UpdateChatBotInfo(
            long uid,
            string botdefine,
            string openaikey,
            string organization
        )
        {
            using var db = GetInstance();
            var data = await db.ChatBot.FirstOrDefaultAsync(it => it.uid == uid);
            var result = await db.InsertOrReplaceAsync(
                new ChatBot()
                {
                    uid = (int)uid,
                    botdefine = botdefine,
                    openaikey = openaikey,
                    organization = organization
                }
            );
            return result > -1;
        }

        public static async Task<ChatBot?> GetChatBotInfo(long uid)
        {
            using var db = GetInstance();
            return await db.ChatBot.Where(it => it.uid == uid).FirstOrDefaultAsync();
        }
    }
}
