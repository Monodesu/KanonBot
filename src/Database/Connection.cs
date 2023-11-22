using LinqToDB;
using LinqToDB.Mapping;
using static KanonBot.Database.Models;

namespace KanonBot.Database;
public class Connection : LinqToDB.Data.DataConnection
{
    public Connection(DataOptions options) : base(options) { }

    public ITable<OsuStandardBeatmapTechData> OsuStandardBeatmapTechData => this.GetTable<OsuStandardBeatmapTechData>();
    public ITable<SeasonalPass> OSUSeasonalPass => this.GetTable<SeasonalPass>();
    public ITable<SeasonalPassScoreRecords> OSUSeasonalPass_ScoreRecords => this.GetTable<SeasonalPassScoreRecords>();
    public ITable<User> Users => this.GetTable<User>();
    public ITable<UserOSU> UserOSU => this.GetTable<UserOSU>();
    public ITable<OsuArchivedRec> OsuArchivedRec => this.GetTable<OsuArchivedRec>();
    public ITable<OsuPPlus> OsuPPlus => this.GetTable<OsuPPlus>();
    public ITable<Badge> Badges => this.GetTable<Badge>();
    public ITable<MailVerify> MailVerify => this.GetTable<MailVerify>();
    public ITable<Bottle> Bottle => this.GetTable<Bottle>();
    public ITable<BadgeRedemptionCode> BadgeRedemptionCode => this.GetTable<BadgeRedemptionCode>();
    public ITable<BadgeExpirationDateRec> BadgeExpirationDateRec => this.GetTable<BadgeExpirationDateRec>();
    public ITable<ChatBot> ChatBot => this.GetTable<ChatBot>();

    // ... other tables ...
}