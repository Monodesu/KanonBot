using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using KanonBot.API;
using KanonBot.functions.osu.rosupp;
using KanonBot.functions.osubot;
using KanonBot.Image;
using KanonBot.LegacyImage;
using MySqlX.XDevAPI.Relational;
using Serilog;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SqlSugar;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static KanonBot.functions.osu.rosupp.PerformanceCalculator;
using static KanonBot.LegacyImage.Draw;
using Img = SixLabors.ImageSharp.Image;
using ResizeOptions = SixLabors.ImageSharp.Processing.ResizeOptions;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

namespace KanonBot.image
{
    public static class OsuInfoPanelV2
    {
        public static async Task<Img> Draw(UserPanelData data, bool isBonded = false, bool isDataOfDayAvaiavle = true, bool eventmode = false)
        {
            var ColorMode = data.InfoPanelV2_Mode;

            #region Colors_config
            Color UsernameColor = new(),
                  ModeIconColor = new(),
                  RankColor = new(),
                  CountryRankColor = new(),
                  CountryRankDiffColor = new(),
                  CountryRankDiffIconColor = new(),
                  RankLineChartColor = new(),
                  RankLineChartTextColor = new(),
                  RankLineChartDotColor = new(),
                  RankLineChartDotStrokeColor = new(),
                  RankLineChartDashColor = new(),
                  RankLineChartDateTextColor = new(),
                  ppMainColor = new(),
                  ppDiffColor = new(),
                  ppDiffIconColor = new(),
                  ppProgressBarColorTextColor = new(),
                  ppProgressBarColor = new(),
                  ppProgressBarBackgroundColor = new(),
                  accMainColor = new(),
                  accDiffColor = new(),
                  accDiffIconColor = new(),
                  accProgressBarColorTextColor = new(),
                  accProgressBarColor = new(),
                  accProgressBarBackgroundColor = new(),
                  GradeStatisticsColor_XH = new(),
                  GradeStatisticsColor_X = new(),
                  GradeStatisticsColor_SH = new(),
                  GradeStatisticsColor_S = new(),
                  GradeStatisticsColor_A = new(),
                  Details_PlayTimeColor = new(),
                  Details_TotalHitsColor = new(),
                  Details_PlayCountColor = new(),
                  Details_RankedScoreColor = new(),
                  DetailsDiff_PlayTimeColor = new(),
                  DetailsDiff_TotalHitsColor = new(),
                  DetailsDiff_PlayCountColor = new(),
                  DetailsDiff_RankedScoreColor = new(),
                  DetailsDiff_PlayTimeIconColor = new(),
                  DetailsDiff_TotalHitsIconColor = new(),
                  DetailsDiff_PlayCountIconColor = new(),
                  DetailsDiff_RankedScoreIconColor = new(),
                  LevelTitleColor = new(),
                  LevelProgressBarColor = new(),
                  LevelProgressBarBackgroundColor = new(),
                  MainBPTitleColor = new(),
                  MainBPArtistColor = new(),
                  MainBPMapperColor = new(),
                  MainBPBIDColor = new(),
                  MainBPStarsColor = new(),
                  MainBPAccColor = new(),
                  MainBPRankColor = new(),
                  MainBPppMainColor = new(),
                  MainBPppTitleColor = new(),
                  SubBp2ndModeColor = new(),
                  SubBp2ndBPTitleColor = new(),
                  SubBp2ndBPVersionColor = new(),
                  SubBp2ndBPBIDColor = new(),
                  SubBp2ndBPStarsColor = new(),
                  SubBp2ndBPAccColor = new(),
                  SubBp2ndBPRankColor = new(),
                  SubBp2ndBPppMainColor = new(),
                  SubBp3rdModeColor = new(),
                  SubBp3rdBPTitleColor = new(),
                  SubBp3rdBPVersionColor = new(),
                  SubBp3rdBPBIDColor = new(),
                  SubBp3rdBPStarsColor = new(),
                  SubBp3rdBPAccColor = new(),
                  SubBp3rdBPRankColor = new(),
                  SubBp3rdBPppMainColor = new(),
                  SubBp4thModeColor = new(),
                  SubBp4thBPTitleColor = new(),
                  SubBp4thBPVersionColor = new(),
                  SubBp4thBPBIDColor = new(),
                  SubBp4thBPStarsColor = new(),
                  SubBp4thBPAccColor = new(),
                  SubBp4thBPRankColor = new(),
                  SubBp4thBPppMainColor = new(),
                  SubBp5thModeColor = new(),
                  SubBp5thBPTitleColor = new(),
                  SubBp5thBPVersionColor = new(),
                  SubBp5thBPBIDColor = new(),
                  SubBp5thBPStarsColor = new(),
                  SubBp5thBPAccColor = new(),
                  SubBp5thBPRankColor = new(),
                  SubBp5thBPppMainColor = new(),
                  footerColor = new(),
                  SubBpInfoSplitColor = new();

            float SideImgBrightness = 1.0f,
                  AvatarBrightness = 1.0f,
                  BadgeBrightness = 1.0f,
                  MainBPImgBrightness = 1.0f,
                  CountryFlagBrightness = 1.0f,
                  ModeCaptionBrightness = 1.0f,
                  ModIconBrightness = 1.0f,
                  ScoreModeIconBrightness = 1.0f,
                  OsuSupporterIconBrightness = 1.0f;

            float CountryFlagAlpha = 1.0f,
                  OsuSupporterIconAlpha = 1.0f,
                  BadgeAlpha = 1.0f,
                  AvatarAlpha = 1.0f,
                  ModIconAlpha = 1.0f;

            bool FixedScoreModeIconColor = false;
            bool ModeIconAlpha = false,
                 Score1ModeIconAlpha = false,
                 Score2ModeIconAlpha = false,
                 Score3ModeIconAlpha = false,
                 Score4ModeIconAlpha = false,
                 DetailsPlaytimeIconAlpha = false,
                 DetailsTotalHitIconAlpha = false,
                 DetailsPlayCountIconAlpha = false,
                 DetailsRankedScoreIconAlpha = false,
                 ppDiffIconColorAlpha = false,
                 accDiffIconColorAlpha = false,
                 CountryRankDiffIconColorAlpha = false;
            #endregion
            //配色方案 0=用户自定义 1=模板light 2=模板dark 3...4...5...
            //本来应该做个class的 算了 懒了 就这样吧 复制粘贴没什么不好的
            switch (ColorMode)
            {
                case 0:
                    //custom
                    //解析
                    var argstemp = data.ColorConfigRaw.Split("\n");
                    foreach (string arg in argstemp)
                    {
                        switch (arg.Split(":")[0].Trim())
                        {
                            case "DetailsDiff_PlayTimeColor":
                                DetailsDiff_PlayTimeColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "DetailsDiff_PlayTimeIconColor":
                                DetailsDiff_PlayTimeIconColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                if (arg.Split(":")[1].Trim().ToLower().Length > 7) DetailsPlaytimeIconAlpha = true;
                                break;
                            case "DetailsDiff_TotalHitsColor":
                                DetailsDiff_TotalHitsColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "DetailsDiff_TotalHitsIconColor":
                                DetailsDiff_TotalHitsIconColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                if (arg.Split(":")[1].Trim().ToLower().Length > 7) DetailsTotalHitIconAlpha = true;
                                break;
                            case "DetailsDiff_PlayCountColor":
                                DetailsDiff_PlayCountColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "DetailsDiff_PlayCountIconColor":
                                DetailsDiff_PlayCountIconColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                if (arg.Split(":")[1].Trim().ToLower().Length > 7) DetailsPlayCountIconAlpha = true;
                                break;
                            case "DetailsDiff_RankedScoreColor":
                                DetailsDiff_RankedScoreColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "DetailsDiff_RankedScoreIconColor":
                                DetailsDiff_RankedScoreIconColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                if (arg.Split(":")[1].Trim().ToLower().Length > 7) DetailsRankedScoreIconAlpha = true;
                                break;
                            case "ppDiffColor":
                                ppDiffColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "ppDiffIconColor":
                                ppDiffIconColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                if (arg.Split(":")[1].Trim().ToLower().Length > 7) ppDiffIconColorAlpha = true;
                                break;
                            case "accDiffColor":
                                accDiffColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "accDiffIconColor":
                                accDiffIconColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                if (arg.Split(":")[1].Trim().ToLower().Length > 7) accDiffIconColorAlpha = true;
                                break;
                            case "CountryRankDiffColor":
                                CountryRankDiffColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "CountryRankDiffIconColor":
                                CountryRankDiffIconColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                if (arg.Split(":")[1].Trim().ToLower().Length > 7) CountryRankDiffIconColorAlpha = true;
                                break;
                            case "OsuSupporterIconBrightness":
                                OsuSupporterIconBrightness = float.Parse($"{arg.Split(":")[1].Trim()}");
                                break;
                            case "CountryFlagAlpha":
                                CountryFlagAlpha = float.Parse($"{arg.Split(":")[1].Trim()}");
                                break;
                            case "OsuSupporterIconAlpha":
                                OsuSupporterIconAlpha = float.Parse($"{arg.Split(":")[1].Trim()}");
                                break;
                            case "BadgeAlpha":
                                BadgeAlpha = float.Parse($"{arg.Split(":")[1].Trim()}");
                                break;
                            case "AvatarAlpha":
                                AvatarAlpha = float.Parse($"{arg.Split(":")[1].Trim()}");
                                break;
                            case "ModIconAlpha":
                                ModIconAlpha = float.Parse($"{arg.Split(":")[1].Trim()}");
                                break;

                            ////
                            case "UsernameColor":
                                UsernameColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "ModeIconColor":
                                ModeIconColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                if (arg.Split(":")[1].Trim().ToLower().Length > 7) { ModeIconAlpha = true; }
                                break;
                            case "RankColor":
                                RankColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "CountryRankColor":
                                CountryRankColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "RankLineChartColor":
                                RankLineChartColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "RankLineChartTextColor":
                                RankLineChartTextColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "RankLineChartDotColor":
                                RankLineChartDotColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "RankLineChartDotStrokeColor":
                                RankLineChartDotStrokeColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "RankLineChartDashColor":
                                RankLineChartDashColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "RankLineChartDateTextColor":
                                RankLineChartDateTextColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "ppMainColor":
                                ppMainColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "ppProgressBarColorTextColor":
                                ppProgressBarColorTextColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "ppProgressBarColor":
                                ppProgressBarColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "ppProgressBarBackgroundColor":
                                ppProgressBarBackgroundColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "accMainColor":
                                accMainColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "accProgressBarColorTextColor":
                                accProgressBarColorTextColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "accProgressBarColor":
                                accProgressBarColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "accProgressBarBackgroundColor":
                                accProgressBarBackgroundColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "GradeStatisticsColor_XH":
                                GradeStatisticsColor_XH = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "GradeStatisticsColor_X":
                                GradeStatisticsColor_X = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "GradeStatisticsColor_SH":
                                GradeStatisticsColor_SH = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "GradeStatisticsColor_S":
                                GradeStatisticsColor_S = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "GradeStatisticsColor_A":
                                GradeStatisticsColor_A = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "Details_PlayTimeColor":
                                Details_PlayTimeColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "Details_TotalHitsColor":
                                Details_TotalHitsColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "Details_PlayCountColor":
                                Details_PlayCountColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "Details_RankedScoreColor":
                                Details_RankedScoreColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "LevelTitleColor":
                                LevelTitleColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "LevelProgressBarColor":
                                LevelProgressBarColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "LevelProgressBarBackgroundColor":
                                LevelProgressBarBackgroundColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "MainBPTitleColor":
                                MainBPTitleColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "MainBPArtistColor":
                                MainBPArtistColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "MainBPMapperColor":
                                MainBPMapperColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "MainBPBIDColor":
                                MainBPBIDColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "MainBPStarsColor":
                                MainBPStarsColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "MainBPAccColor":
                                MainBPAccColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "MainBPRankColor":
                                MainBPRankColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "MainBPppMainColor":
                                MainBPppMainColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "MainBPppTitleColor":
                                MainBPppTitleColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "SubBp2ndModeColor":
                                SubBp2ndModeColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                if (arg.Split(":")[1].Trim().ToLower().Length > 7) { Score1ModeIconAlpha = true; }
                                break;
                            case "SubBp2ndBPTitleColor":
                                SubBp2ndBPTitleColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "SubBp2ndBPVersionColor":
                                SubBp2ndBPVersionColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "SubBp2ndBPBIDColor":
                                SubBp2ndBPBIDColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "SubBp2ndBPStarsColor":
                                SubBp2ndBPStarsColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "SubBp2ndBPAccColor":
                                SubBp2ndBPAccColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "SubBp2ndBPRankColor":
                                SubBp2ndBPRankColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "SubBp2ndBPppMainColor":
                                SubBp2ndBPppMainColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "SubBp3rdModeColor":
                                SubBp3rdModeColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                if (arg.Split(":")[1].Trim().ToLower().Length > 7) { Score2ModeIconAlpha = true; }
                                break;
                            case "SubBp3rdBPTitleColor":
                                SubBp3rdBPTitleColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "SubBp3rdBPVersionColor":
                                SubBp3rdBPVersionColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "SubBp3rdBPBIDColor":
                                SubBp3rdBPBIDColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "SubBp3rdBPStarsColor":
                                SubBp3rdBPStarsColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "SubBp3rdBPAccColor":
                                SubBp3rdBPAccColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "SubBp3rdBPRankColor":
                                SubBp3rdBPRankColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "SubBp3rdBPppMainColor":
                                SubBp3rdBPppMainColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "SubBp4thModeColor":
                                SubBp4thModeColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                if (arg.Split(":")[1].Trim().ToLower().Length > 7) { Score3ModeIconAlpha = true; }
                                break;
                            case "SubBp4thBPTitleColor":
                                SubBp4thBPTitleColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "SubBp4thBPVersionColor":
                                SubBp4thBPVersionColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "SubBp4thBPBIDColor":
                                SubBp4thBPBIDColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "SubBp4thBPStarsColor":
                                SubBp4thBPStarsColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "SubBp4thBPAccColor":
                                SubBp4thBPAccColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "SubBp4thBPRankColor":
                                SubBp4thBPRankColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "SubBp4thBPppMainColor":
                                SubBp4thBPppMainColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "SubBp5thModeColor":
                                SubBp5thModeColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                if (arg.Split(":")[1].Trim().ToLower().Length > 7) { Score4ModeIconAlpha = true; }
                                break;
                            case "SubBp5thBPTitleColor":
                                SubBp5thBPTitleColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "SubBp5thBPVersionColor":
                                SubBp5thBPVersionColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "SubBp5thBPBIDColor":
                                SubBp5thBPBIDColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "SubBp5thBPStarsColor":
                                SubBp5thBPStarsColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "SubBp5thBPAccColor":
                                SubBp5thBPAccColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "SubBp5thBPRankColor":
                                SubBp5thBPRankColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "SubBp5thBPppMainColor":
                                SubBp5thBPppMainColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "SubBpInfoSplitColor":
                                SubBpInfoSplitColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "footerColor":
                                footerColor = Color.ParseHex(arg.Split(":")[1].Trim());
                                break;
                            case "FixedScoreModeIconColor":
                                FixedScoreModeIconColor = bool.Parse($"{arg.Split(":")[1].Trim()}");
                                break;
                            case "SideImgBrightness":
                                SideImgBrightness = float.Parse($"{arg.Split(":")[1].Trim()}");
                                break;
                            case "AvatarBrightness":
                                AvatarBrightness = float.Parse($"{arg.Split(":")[1].Trim()}");
                                break;
                            case "BadgeBrightness":
                                BadgeBrightness = float.Parse($"{arg.Split(":")[1].Trim()}");
                                break;
                            case "MainBPImgBrightness":
                                MainBPImgBrightness = float.Parse($"{arg.Split(":")[1].Trim()}");
                                break;
                            case "CountryFlagBrightness":
                                CountryFlagBrightness = float.Parse($"{arg.Split(":")[1].Trim()}");
                                break;
                            case "ModeCaptionBrightness":
                                ModeCaptionBrightness = float.Parse($"{arg.Split(":")[1].Trim()}");
                                break;
                            case "ModIconBrightness":
                                ModIconBrightness = float.Parse($"{arg.Split(":")[1].Trim()}");
                                break;
                            case "ScoreModeIconBrightness":
                                ScoreModeIconBrightness = float.Parse($"{arg.Split(":")[1].Trim()}");
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                case 1:
                    #region LightMode
                    UsernameColor = Color.ParseHex("#4d4d4d");
                    RankColor = Color.ParseHex("#5872df");
                    CountryRankColor = Color.ParseHex("#5872df");
                    RankLineChartColor = Color.ParseHex("#80caea");
                    RankLineChartTextColor = Color.ParseHex("#80caea");
                    RankLineChartDotColor = Color.ParseHex("#80caea");
                    RankLineChartDotStrokeColor = Color.ParseHex("#eff1f3");
                    RankLineChartDashColor = Color.ParseHex("#dbe1e4");
                    RankLineChartDateTextColor = Color.ParseHex("#666666");
                    ppMainColor = Color.ParseHex("#e36a79");
                    ppProgressBarColorTextColor = Color.ParseHex("#d84356");
                    ppProgressBarColor = Color.ParseHex("#f7bebe");
                    ppProgressBarBackgroundColor = Color.ParseHex("#fddcd7");
                    accMainColor = Color.ParseHex("#6cac9c");
                    accProgressBarColorTextColor = Color.ParseHex("#006837");
                    accProgressBarColor = Color.ParseHex("#a4d8b1");
                    accProgressBarBackgroundColor = Color.ParseHex("#c3e7cb");
                    GradeStatisticsColor_XH = Color.ParseHex("#3a4d78");
                    GradeStatisticsColor_X = Color.ParseHex("#3a4d78");
                    GradeStatisticsColor_SH = Color.ParseHex("#3a4d78");
                    GradeStatisticsColor_S = Color.ParseHex("#3a4d78");
                    GradeStatisticsColor_A = Color.ParseHex("#3a4d78");
                    Details_PlayTimeColor = Color.ParseHex("#7f7f7f");
                    Details_TotalHitsColor = Color.ParseHex("#7f7f7f");
                    Details_PlayCountColor = Color.ParseHex("#7f7f7f");
                    Details_RankedScoreColor = Color.ParseHex("#7f7f7f");
                    LevelTitleColor = Color.ParseHex("#656b6d");
                    LevelProgressBarColor = Color.ParseHex("#f3b6cd");
                    LevelProgressBarBackgroundColor = Color.ParseHex("#e6e6e6");
                    MainBPTitleColor = Color.ParseHex("#656b6d");
                    MainBPArtistColor = Color.ParseHex("#656b6d");
                    MainBPMapperColor = Color.ParseHex("#656b6d");
                    MainBPBIDColor = Color.ParseHex("#656b6d");
                    MainBPStarsColor = Color.ParseHex("#656b6d");
                    MainBPAccColor = Color.ParseHex("#656b6d");
                    MainBPRankColor = Color.ParseHex("#656b6d");
                    MainBPppMainColor = Color.ParseHex("#364a75");
                    MainBPppTitleColor = Color.ParseHex("#656b6d");
                    SubBp2ndBPTitleColor = Color.ParseHex("#656b6d");
                    SubBp2ndBPVersionColor = Color.ParseHex("#656b6d");
                    SubBp2ndBPBIDColor = Color.ParseHex("#656b6d");
                    SubBp2ndBPStarsColor = Color.ParseHex("#656b6d");
                    SubBp2ndBPAccColor = Color.ParseHex("#ffcd22");
                    SubBp2ndBPRankColor = Color.ParseHex("#656b6d");
                    SubBp2ndBPppMainColor = Color.ParseHex("#ff7bac");
                    SubBp3rdBPTitleColor = Color.ParseHex("#656b6d");
                    SubBp3rdBPVersionColor = Color.ParseHex("#656b6d");
                    SubBp3rdBPBIDColor = Color.ParseHex("#656b6d");
                    SubBp3rdBPStarsColor = Color.ParseHex("#656b6d");
                    SubBp3rdBPAccColor = Color.ParseHex("#ffcd22");
                    SubBp3rdBPRankColor = Color.ParseHex("#656b6d");
                    SubBp3rdBPppMainColor = Color.ParseHex("#ff7bac");
                    SubBp4thBPTitleColor = Color.ParseHex("#656b6d");
                    SubBp4thBPVersionColor = Color.ParseHex("#656b6d");
                    SubBp4thBPBIDColor = Color.ParseHex("#656b6d");
                    SubBp4thBPStarsColor = Color.ParseHex("#656b6d");
                    SubBp4thBPAccColor = Color.ParseHex("#ffcd22");
                    SubBp4thBPRankColor = Color.ParseHex("#656b6d");
                    SubBp4thBPppMainColor = Color.ParseHex("#ff7bac");
                    SubBp5thBPTitleColor = Color.ParseHex("#656b6d");
                    SubBp5thBPVersionColor = Color.ParseHex("#656b6d");
                    SubBp5thBPBIDColor = Color.ParseHex("#656b6d");
                    SubBp5thBPStarsColor = Color.ParseHex("#656b6d");
                    SubBp5thBPAccColor = Color.ParseHex("#ffcd22");
                    SubBp5thBPRankColor = Color.ParseHex("#656b6d");
                    SubBp5thBPppMainColor = Color.ParseHex("#ff7bac");
                    footerColor = Color.ParseHex("#7f7f7f");
                    SubBpInfoSplitColor = Color.ParseHex("#656b6d");
                    ModeIconColor = Color.ParseHex("#7f7f7f");
                    SubBp2ndModeColor = Color.White;
                    SubBp3rdModeColor = Color.White;
                    SubBp4thModeColor = Color.White;
                    SubBp5thModeColor = Color.White;
                    FixedScoreModeIconColor = false;
                    DetailsDiff_PlayTimeColor = Color.ParseHex("#6cac9c");
                    DetailsDiff_PlayTimeIconColor = Color.ParseHex("#6cac9c");
                    DetailsDiff_TotalHitsColor = Color.ParseHex("#6cac9c");
                    DetailsDiff_TotalHitsIconColor = Color.ParseHex("#6cac9c");
                    DetailsDiff_PlayCountColor = Color.ParseHex("#5872df");
                    DetailsDiff_PlayCountIconColor = Color.ParseHex("#5872df");
                    DetailsDiff_RankedScoreColor = Color.ParseHex("#3a4d78");
                    DetailsDiff_RankedScoreIconColor = Color.ParseHex("#3a4d78");
                    ppDiffColor = Color.ParseHex("#e36a79");
                    ppDiffIconColor = Color.ParseHex("#e36a79");
                    accDiffColor = Color.ParseHex("#6cac9c");
                    accDiffIconColor = Color.ParseHex("#6cac9c");
                    CountryRankDiffColor = Color.ParseHex("#808080");
                    CountryRankDiffIconColor = Color.ParseHex("#3a4d78");
                    //do not change brightness;
                    break;
                #endregion
                case 2:
                    #region DarkMode
                    UsernameColor = Color.ParseHex("#e6e6e6");
                    RankColor = Color.ParseHex("#5872DF");
                    CountryRankColor = Color.ParseHex("#5872DF");
                    RankLineChartColor = Color.ParseHex("#2784ac");
                    RankLineChartTextColor = Color.ParseHex("#2784ac");
                    RankLineChartDotColor = Color.ParseHex("#2784ac");
                    RankLineChartDotStrokeColor = Color.ParseHex("#b2b5b7");
                    RankLineChartDashColor = Color.ParseHex("#e6e6e6");
                    RankLineChartDateTextColor = Color.ParseHex("#e6e6e6");
                    ppMainColor = Color.ParseHex("#e36a79");
                    ppProgressBarColorTextColor = Color.ParseHex("#FF7BAC");
                    ppProgressBarColor = Color.ParseHex("#5D3B3A");
                    ppProgressBarBackgroundColor = Color.ParseHex("#44312F");
                    accMainColor = Color.ParseHex("#6cac9c");
                    accProgressBarColorTextColor = Color.ParseHex("#00DE75");
                    accProgressBarColor = Color.ParseHex("#294B32");
                    accProgressBarBackgroundColor = Color.ParseHex("#243829");
                    GradeStatisticsColor_XH = Color.ParseHex("#6C91E0");
                    GradeStatisticsColor_X = Color.ParseHex("#6C91E0");
                    GradeStatisticsColor_SH = Color.ParseHex("#6C91E0");
                    GradeStatisticsColor_S = Color.ParseHex("#6C91E0");
                    GradeStatisticsColor_A = Color.ParseHex("#6C91E0");
                    Details_PlayTimeColor = Color.ParseHex("#e6e6e6");
                    Details_TotalHitsColor = Color.ParseHex("#e6e6e6");
                    Details_PlayCountColor = Color.ParseHex("#e6e6e6");
                    Details_RankedScoreColor = Color.ParseHex("#e6e6e6");
                    LevelTitleColor = Color.ParseHex("#e6e6e6");
                    LevelProgressBarColor = Color.ParseHex("#85485F");
                    LevelProgressBarBackgroundColor = Color.ParseHex("#000000");
                    MainBPTitleColor = Color.ParseHex("#e6e6e6");
                    MainBPArtistColor = Color.ParseHex("#e6e6e6");
                    MainBPMapperColor = Color.ParseHex("#e6e6e6");
                    MainBPBIDColor = Color.ParseHex("#e6e6e6");
                    MainBPStarsColor = Color.ParseHex("#e6e6e6");
                    MainBPAccColor = Color.ParseHex("#e6e6e6");
                    MainBPRankColor = Color.ParseHex("#e6e6e6");
                    MainBPppMainColor = Color.ParseHex("#5979bd");
                    MainBPppTitleColor = Color.ParseHex("#e6e6e6");
                    SubBp2ndBPTitleColor = Color.ParseHex("#e6e6e6");
                    SubBp2ndBPVersionColor = Color.ParseHex("#e6e6e6");
                    SubBp2ndBPBIDColor = Color.ParseHex("#e6e6e6");
                    SubBp2ndBPStarsColor = Color.ParseHex("#e6e6e6");
                    SubBp2ndBPAccColor = Color.ParseHex("#ffcd22");
                    SubBp2ndBPRankColor = Color.ParseHex("#e6e6e6");
                    SubBp2ndBPppMainColor = Color.ParseHex("#e36a79");
                    SubBp3rdBPTitleColor = Color.ParseHex("#e6e6e6");
                    SubBp3rdBPVersionColor = Color.ParseHex("#e6e6e6");
                    SubBp3rdBPBIDColor = Color.ParseHex("#e6e6e6");
                    SubBp3rdBPStarsColor = Color.ParseHex("#e6e6e6");
                    SubBp3rdBPAccColor = Color.ParseHex("#ffcd22");
                    SubBp3rdBPRankColor = Color.ParseHex("#e6e6e6");
                    SubBp3rdBPppMainColor = Color.ParseHex("#e36a79");
                    SubBp4thBPTitleColor = Color.ParseHex("#e6e6e6");
                    SubBp4thBPVersionColor = Color.ParseHex("#e6e6e6");
                    SubBp4thBPBIDColor = Color.ParseHex("#e6e6e6");
                    SubBp4thBPStarsColor = Color.ParseHex("#e6e6e6");
                    SubBp4thBPAccColor = Color.ParseHex("#ffcd22");
                    SubBp4thBPRankColor = Color.ParseHex("#e6e6e6");
                    SubBp4thBPppMainColor = Color.ParseHex("#e36a79");
                    SubBp5thBPTitleColor = Color.ParseHex("#e6e6e6");
                    SubBp5thBPVersionColor = Color.ParseHex("#e6e6e6");
                    SubBp5thBPBIDColor = Color.ParseHex("#e6e6e6");
                    SubBp5thBPStarsColor = Color.ParseHex("#e6e6e6");
                    SubBp5thBPAccColor = Color.ParseHex("#ffcd22");
                    SubBp5thBPRankColor = Color.ParseHex("#e6e6e6");
                    SubBp5thBPppMainColor = Color.ParseHex("#e36a79");
                    footerColor = Color.ParseHex("#e6e6e6");
                    SubBpInfoSplitColor = Color.ParseHex("#e6e6e6");
                    //change brightness
                    SideImgBrightness = 0.6f;
                    AvatarBrightness = 0.6f;
                    BadgeBrightness = 0.6f;
                    MainBPImgBrightness = 0.6f;
                    CountryFlagBrightness = 0.6f;
                    ModeCaptionBrightness = 0.6f;
                    ModIconBrightness = 0.6f;
                    ScoreModeIconBrightness = 0.6f;
                    OsuSupporterIconBrightness = 0.6f;

                    ModeIconColor = Color.ParseHex("#e6e6e6");
                    SubBp2ndModeColor = Color.White;
                    SubBp3rdModeColor = Color.White;
                    SubBp4thModeColor = Color.White;
                    SubBp5thModeColor = Color.White;
                    FixedScoreModeIconColor = false;

                    DetailsDiff_PlayTimeColor = Color.ParseHex("#6cac9c");
                    DetailsDiff_PlayTimeIconColor = Color.ParseHex("#6cac9c");
                    DetailsDiff_TotalHitsColor = Color.ParseHex("#6cac9c");
                    DetailsDiff_TotalHitsIconColor = Color.ParseHex("#6cac9c");
                    DetailsDiff_PlayCountColor = Color.ParseHex("#5872df");
                    DetailsDiff_PlayCountIconColor = Color.ParseHex("#5872df");
                    DetailsDiff_RankedScoreColor = Color.ParseHex("#3a4d78");
                    DetailsDiff_RankedScoreIconColor = Color.ParseHex("#3a4d78");
                    ppDiffColor = Color.ParseHex("#e36a79");
                    ppDiffIconColor = Color.ParseHex("#e36a79");
                    accDiffColor = Color.ParseHex("#6cac9c");
                    accDiffIconColor = Color.ParseHex("#6cac9c");
                    CountryRankDiffColor = Color.ParseHex("#e6e6e6");
                    CountryRankDiffIconColor = Color.ParseHex("#3a4d78");

                    break;
                    #endregion
            }
            var info = new Image<Rgba32>(4000, 2640);
            //获取全部bp
            var allBP = await OSU.GetUserScores(data.userInfo.Id, OSU.Enums.UserScoreType.Best, data.userInfo.PlayMode, 100, 0);

            //设定textOption/drawOption
            var textOptions = new TextOptions(new Font(TorusSemiBold, 120))
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            var drawOptions = new DrawingOptions
            {
                GraphicsOptions = new GraphicsOptions
                {
                    Antialias = true
                }
            };

            //自定义侧图
            string sidePicPath;
            Img sidePic;
            if (File.Exists($"./work/panelv2/user_customimg/{data.userInfo.Id}.png"))
                sidePicPath = $"./work/panelv2/user_customimg/{data.userInfo.Id}.png";
            else sidePicPath = ColorMode switch
            {
                0 => "./work/panelv2/infov2-dark-customimg.png",
                1 => "./work/panelv2/infov2-light-customimg.png",
                2 => "./work/panelv2/infov2-dark-customimg.png",
                _ => throw new Exception(),
            };
            sidePic = Img.Load(await Utils.LoadFile2Byte(sidePicPath)).CloneAs<Rgba32>();    // 读取
            sidePic.Mutate(x => x.Brightness(SideImgBrightness));
            info.Mutate(x => x.DrawImage(sidePic, new Point(90, 72), 1));


            //进度条 - 先绘制进度条，再覆盖面板
            //pp
            Img pp_background = new Image<Rgba32>(1443, 68);
            pp_background.Mutate(x => x.Fill(ppProgressBarBackgroundColor));
            pp_background.Mutate(x => x.RoundCorner_Parts(new Size(1443, 68), 10, 10, 20, 20));
            info.Mutate(x => x.DrawImage(pp_background, new Point(2358, 410), 1));
            //获取bnspp
            double bounsPP = 0.00;
            double scorePP = 0.00;
            #region bnspp
            if (allBP == null || allBP.Length < 100) { scorePP = data.userInfo.Statistics.PP; }
            else if (allBP!.Length == 0) { scorePP = data.userInfo.Statistics.PP; }
            else
            {
                double pp = 0.0, sumOxy = 0.0, sumOx2 = 0.0, avgX = 0.0, avgY = 0.0, sumX = 0.0;
                List<double> ys = new();
                for (int i = 0; i < allBP.Length; ++i)
                {
                    var tmp = allBP[i].PP * Math.Pow(0.95, i);
                    scorePP += tmp;
                    ys.Add(Math.Log10(tmp) / Math.Log10(100));
                }
                // calculateLinearRegression
                for (int i = 1; i <= ys.Count; ++i)
                {
                    double weight = Utils.log1p(i + 1.0);
                    sumX += weight;
                    avgX += i * weight;
                    avgY += ys[i - 1] * weight;
                }
                avgX /= sumX;
                avgY /= sumX;
                for (int i = 1; i <= ys.Count; ++i)
                {
                    sumOxy += (i - avgX) * (ys[i - 1] - avgY) * Utils.log1p(i + 1.0);
                    sumOx2 += Math.Pow(i - avgX, 2.0) * Utils.log1p(i + 1.0);
                }
                double Oxy = sumOxy / sumX;
                double Ox2 = sumOx2 / sumX;
                // end
                var b = new double[] { avgY - (Oxy / Ox2) * avgX, Oxy / Ox2 };
                for (double i = 100; i <= data.userInfo.Statistics.PlayCount; ++i)
                {
                    double val = Math.Pow(100.0, b[0] + b[1] * i);
                    if (val <= 0.0)
                    {
                        break;
                    }
                    pp += val;
                }
                scorePP += pp;
                bounsPP = data.userInfo.Statistics.PP - scorePP;
                int totalscores = data.userInfo.Statistics.GradeCounts.A + data.userInfo.Statistics.GradeCounts.S + data.userInfo.Statistics.GradeCounts.SH + data.userInfo.Statistics.GradeCounts.SS + data.userInfo.Statistics.GradeCounts.SSH;
                bool max;
                if (totalscores >= 25397 || bounsPP >= 416.6667)
                    max = true;
                else
                    max = false;
                int rankedScores = max ? Math.Max(totalscores, 25397) : (int)Math.Round(Math.Log10(-(bounsPP / 416.6667) + 1.0) / Math.Log10(0.9994));
                if (double.IsNaN(scorePP) || double.IsNaN(bounsPP))
                {
                    scorePP = 0.0;
                    bounsPP = 0.0;
                    rankedScores = 0;
                }

            }
            #endregion
            //绘制mainpp
            int pp_front_length = 1443 - (int)(1443.0 * (bounsPP / scorePP));
            if (pp_front_length < 1) pp_front_length = 1443;
            Img pp_front = new Image<Rgba32>(pp_front_length, 68);
            pp_front.Mutate(x => x.Fill(ppProgressBarColor));
            pp_front.Mutate(x => x.RoundCorner_Parts(new Size(pp_front_length, 68), 10, 10, 20, 20));
            info.Mutate(x => x.DrawImage(pp_front, new Point(2358, 410), 1));

            //50&100
            Img acc_background = new Image<Rgba32>(1443, 68);
            acc_background.Mutate(x => x.Fill(accProgressBarBackgroundColor));
            acc_background.Mutate(x => x.RoundCorner_Parts(new Size(1443, 68), 10, 10, 20, 20));
            info.Mutate(x => x.DrawImage(acc_background, new Point(2358, 611), 1));

            //300
            int acc_front_length = (int)(1443.00 * (data.userInfo.Statistics.HitAccuracy / 100.0));
            if (acc_front_length < 1) acc_front_length = 1443;
            Img acc_300 = new Image<Rgba32>(acc_front_length, 68);
            acc_300.Mutate(x => x.Fill(accProgressBarColor));
            acc_300.Mutate(x => x.RoundCorner_Parts(new Size((int)(1443.00 * (data.userInfo.Statistics.HitAccuracy / 100.0)), 68), 10, 10, 20, 20));
            info.Mutate(x => x.DrawImage(acc_300, new Point(2358, 611), 1));

            #region junkcodes
            //acc
            //var v1infos = await OSU.GetUserWithV1API(data.userInfo.Id, data.userInfo.PlayMode);
            //var v1info = v1infos!.First();
            //var v1n50 = v1info.Count50;
            //var v1n100 = v1info.Count100;
            //var v1n300 = v1info.Count300;
            //var v1totalhits = v1n50 + v1n100 + v1n300;
            //50&100
            //Img acc_background = new Image<Rgba32>(1443, 68);
            //acc_background.Mutate(x => x.Fill(Color.ParseHex("#c3e7cb")));
            //acc_background.Mutate(x => x.RoundCorner_Parts(new Size(1443, 68), 10, 10, 20, 20));
            //info.Mutate(x => x.DrawImage(acc_background, new Point(2358, 611), 1));
            //only 300
            //Img acc_100 = new Image<Rgba32>((int)(1443.00 * ((double)v1n300 / (double)v1totalhits)), 68);
            //acc_100.Mutate(x => x.Fill(Color.ParseHex("#a4d8b1")));
            //acc_100.Mutate(x => x.RoundCorner_Parts(new Size((int)(1443.0 * ((double)v1n300 / (double)v1totalhits)), 68), 10, 10, 20, 20));
            //info.Mutate(x => x.DrawImage(acc_100, new Point(2358, 611), 1));
            /*
            //100
            Img acc_100 = new Image<Rgba32>((int)(1443.0 - 1443.00 * ((double)v1n50 / (double)v1totalhits)), 68);
            acc_100.Mutate(x => x.Fill(Color.ParseHex("#a4d8b1")));
            acc_100.Mutate(x => x.RoundCorner_Parts(new Size((int)(1443.0 - 1443.0 * ((double)v1n50 / (double)v1totalhits)), 68), 10, 10, 20, 20));
            info.Mutate(x => x.DrawImage(acc_100, new Point(2358, 611), 1));
            //300
            Img acc_300 = new Image<Rgba32>((int)(1443.0 - 1443.0 * (((double)v1n50 + (double)v1n100) / (double)v1totalhits)), 68);
            acc_300.Mutate(x => x.Fill(Color.ParseHex("#92cca7")));
            acc_300.Mutate(x => x.RoundCorner_Parts(new Size((int)(1443.0 - 1443.0 * (((double)v1n50 + (double)v1n100) / (double)v1totalhits)), 68), 10, 10, 20, 20));
            info.Mutate(x => x.DrawImage(acc_300, new Point(2358, 611), 1));
            */
            #endregion

            //top score image 先绘制top bp图片再覆盖面板
            //download background image
            Img bp1bg;
            if (allBP!.Length > 5)
            {
                var bp1bgPath = $"./work/background/{allBP![0].Beatmap!.BeatmapId}.png";
                if (!File.Exists(bp1bgPath))
                {
                    try
                    {
                        bp1bgPath = await OSU.SayoDownloadBeatmapBackgroundImg(allBP![0].Beatmapset!.Id, allBP![0].Beatmap!.BeatmapId, "./work/background/");
                    }
                    catch (Exception ex)
                    {
                        var msg = $"从API下载背景图片时发生了一处异常\n异常类型: {ex.GetType()}\n异常信息: '{ex.Message}'";
                        Log.Warning(msg);
                    }
                }
                try { bp1bg = Img.Load(bp1bgPath).CloneAs<Rgba32>(); }
                catch { bp1bg = Img.Load(await Utils.LoadFile2Byte("./work/legacy/load-failed-img.png")); }
                //bp1bg.Mutate(x => x.Resize(355, 200));
                bp1bg.Mutate(x => x.Resize(new ResizeOptions() { Size = new Size(355, 0), Mode = ResizeMode.Max })); //355x200
                bp1bg.Mutate(x => x.Brightness(MainBPImgBrightness));
                info.Mutate(x => x.DrawImage(bp1bg, new Point(1566, 1550), 1));
            }
            else
            {
                bp1bg = new Image<Rgba32>(355, 200);
                switch (ColorMode)
                {
                    case 0:
                        bp1bg.Mutate(x => x.Fill(Color.White));
                        break;
                    case 1:
                        //light
                        //do nothing
                        break;
                    case 2:
                        //dark
                        bp1bg.Mutate(x => x.Fill(Color.White));
                        break;
                }
                info.Mutate(x => x.DrawImage(bp1bg, new Point(1566, 1550), 1));
            }


            //level progress
            Img levelprogress_background = new Image<Rgba32>(2312, 12);
            levelprogress_background.Mutate(x => x.Fill(LevelProgressBarBackgroundColor));
            //levelprogress_background.Mutate(x => x.RoundCorner(new Size(2312, 6), 4.5f));
            int levelprogressFrontPos = (int)((double)2312 * (double)data.userInfo.Statistics.Level.Progress / 100.0);
            if (levelprogressFrontPos > 0)
            {
                Img levelprogress_front = new Image<Rgba32>(levelprogressFrontPos, 6);
                levelprogress_front.Mutate(x => x.Fill(LevelProgressBarColor));
                levelprogress_front.Mutate(x => x.RoundCorner(new Size(levelprogressFrontPos, 12), 8));
                levelprogress_background.Mutate(x => x.DrawImage(levelprogress_front, new Point(0, 0), 1));
            }
            levelprogress_background.Mutate(x => x.Rotate(-90));
            info.Mutate(x => x.DrawImage(levelprogress_background, new Point(3900, 72), 1));


            //用户面板/自定义面板
            string panelPath;
            Img panel;
            if (File.Exists($"./work/panelv2/user_infopanel/{data.userInfo.Id}.png")) panelPath = $"./work/panelv2/user_infopanel/{data.userInfo.Id}.png";
            else panelPath = ColorMode switch
            {
                0 => "./work/panelv2/infov2-light.png",
                1 => "./work/panelv2/infov2-light.png",
                2 => "./work/panelv2/infov2-dark.png",
                _ => throw new Exception(),
            };
            panel = Img.Load(await Utils.LoadFile2Byte(panelPath)).CloneAs<Rgba32>();    // 读取
            info.Mutate(x => x.DrawImage(panel, new Point(0, 0), 1));



            //avatar
            var avatarPath = $"./work/avatar/{data.userInfo.Id}.png";
            Img avatar;
            try
            {
                avatar = Img.Load(await Utils.LoadFile2Byte(avatarPath)).CloneAs<Rgba32>();    // 读取
            }
            catch
            {
                try
                {
                    avatarPath = await data.userInfo.AvatarUrl.DownloadFileAsync("./work/avatar/", $"{data.userInfo.Id}.png");
                }
                catch (Exception ex)
                {
                    var msg = $"从API下载用户头像时发生了一处异常\n异常类型: {ex.GetType()}\n异常信息: '{ex.Message}'";
                    Log.Error(msg);
                    throw;  // 下载失败直接抛出error
                }
                avatar = Img.Load(await Utils.LoadFile2Byte(avatarPath)).CloneAs<Rgba32>();    // 下载后再读取
            }
            // 亮度
            avatar.Mutate(x => x.Brightness(AvatarBrightness));
            avatar.Mutate(x => x.Resize(200, 200).RoundCorner(new Size(200, 200), 25));
            avatar.Mutate(x => x.ProcessPixelRowsAsVector4(row =>
            {
                for (int p = 0; p < row.Length; p++)
                    if (row[p].W > 0.2f) row[p].W = AvatarAlpha;
            }));
            info.Mutate(x => x.DrawImage(avatar, new Point(1531, 72), 1));


            //username
            textOptions.Origin = new PointF(1780, 230);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, data.userInfo.Username, new SolidBrush(UsernameColor), null));

            //rank
            textOptions.Font = new Font(TorusRegular, 60);
            textOptions.Origin = new PointF(1972, 481);
            textOptions.VerticalAlignment = VerticalAlignment.Center;
            info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:N0}", data.userInfo.Statistics.GlobalRank), new SolidBrush(RankColor), null));

            //country_flag
            Img flags = Img.Load(await Utils.LoadFile2Byte($"./work/flags/{data.userInfo.Country.Code}.png"));
            flags.Mutate(x => x.Resize(100, 67).Brightness(CountryFlagBrightness));
            flags.Mutate(x => x.ProcessPixelRowsAsVector4(row =>
            {
                for (int p = 0; p < row.Length; p++)
                    if (row[p].W > 0.2f) row[p].W = CountryFlagAlpha;
            }));
            info.Mutate(x => x.DrawImage(flags, new Point(1577, 600), 1));

            //country_rank
            textOptions.HorizontalAlignment = HorizontalAlignment.Left;
            textOptions.Origin = new PointF(1687, 629);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("#{0:N0}", data.userInfo.Statistics.CountryRank), new SolidBrush(CountryRankColor), null));
            if (isBonded)
            {
                if (Math.Abs(data.userInfo.Statistics.CountryRank - data.prevUserInfo!.Statistics.CountryRank) >= 1)
                {
                    textOptions.HorizontalAlignment = HorizontalAlignment.Right;
                    textOptions.Origin = new PointF(2250, 629);
                    textOptions.Font = new Font(TorusRegular, 44);
                    info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:N0}",
                        Math.Abs(data.userInfo.Statistics.CountryRank - data.prevUserInfo!.Statistics.CountryRank)), new SolidBrush(CountryRankDiffColor), null));
                    Img cr_indicator_icon_increase = Img.Load(await Utils.LoadFile2Byte($"./work/panelv2/icons/indicator.png")).CloneAs<Rgba32>();
                    cr_indicator_icon_increase.Mutate(x => x.Resize(36, 36));
                    if ((data.userInfo.Statistics.CountryRank - data.prevUserInfo!.Statistics.CountryRank) > 0)
                        cr_indicator_icon_increase.Mutate(x => x.Rotate(180));
                    cr_indicator_icon_increase.Mutate(x => x.ProcessPixelRowsAsVector4(row =>
                    {
                        for (int p = 0; p < row.Length; p++)
                        {
                            row[p].X = ((Vector4)CountryRankDiffIconColor).X;
                            row[p].Y = ((Vector4)CountryRankDiffIconColor).Y;
                            row[p].Z = ((Vector4)CountryRankDiffIconColor).Z;
                            if (CountryRankDiffIconColorAlpha) if (row[p].W > 0.2f) row[p].W = row[p].W * ((Vector4)CountryRankDiffIconColor).W;
                        }
                    }));
                    info.Mutate(x => x.DrawImage(cr_indicator_icon_increase, new Point(2255, 616), 1));
                }
            }

            //pp
            textOptions.HorizontalAlignment = HorizontalAlignment.Left;
            textOptions.Origin = new PointF(3120, 350);
            textOptions.Font = new Font(TorusRegular, 60);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:N0}", data.userInfo.Statistics.PP), new SolidBrush(ppMainColor), null));
            if (isBonded)
            {
                if (Math.Abs(data.userInfo.Statistics.PP - data.prevUserInfo!.Statistics.PP) >= 1.0)
                {
                    textOptions.HorizontalAlignment = HorizontalAlignment.Right;
                    textOptions.Origin = new PointF(3735, 350);
                    textOptions.Font = new Font(TorusRegular, 40);
                    info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:N0}",
                        Math.Abs(data.userInfo.Statistics.PP - data.prevUserInfo!.Statistics.PP)), new SolidBrush(ppDiffColor), null));
                    Img pp_indicator_icon_increase = Img.Load(await Utils.LoadFile2Byte($"./work/panelv2/icons/indicator.png")).CloneAs<Rgba32>();
                    pp_indicator_icon_increase.Mutate(x => x.Resize(36, 36));
                    if ((data.userInfo.Statistics.PP - data.prevUserInfo!.Statistics.PP) < 0)
                        pp_indicator_icon_increase.Mutate(x => x.Rotate(180));
                    pp_indicator_icon_increase.Mutate(x => x.ProcessPixelRowsAsVector4(row =>
                    {
                        for (int p = 0; p < row.Length; p++)
                        {
                            row[p].X = ((Vector4)ppDiffIconColor).X;
                            row[p].Y = ((Vector4)ppDiffIconColor).Y;
                            row[p].Z = ((Vector4)ppDiffIconColor).Z;
                            if (ppDiffIconColorAlpha) if (row[p].W > 0.2f) row[p].W = row[p].W * ((Vector4)ppDiffIconColor).W;
                        }
                    }));
                    info.Mutate(x => x.DrawImage(pp_indicator_icon_increase, new Point(3740, 335), 1));
                }
            }

            //acc
            textOptions.HorizontalAlignment = HorizontalAlignment.Left;
            textOptions.Origin = new PointF(3120, 551);
            textOptions.Font = new Font(TorusRegular, 60);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:0.##}%", data.userInfo.Statistics.HitAccuracy), new SolidBrush(accMainColor), null));
            if (isBonded)
            {
                if (Math.Abs(data.userInfo.Statistics.HitAccuracy - data.prevUserInfo!.Statistics.HitAccuracy) >= 0.01)
                {
                    textOptions.HorizontalAlignment = HorizontalAlignment.Right;
                    textOptions.Origin = new PointF(3735, 551);
                    textOptions.Font = new Font(TorusRegular, 40);
                    info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:0.##}%",
                        data.userInfo.Statistics.HitAccuracy - data.prevUserInfo!.Statistics.HitAccuracy), new SolidBrush(accDiffColor), null));
                    Img acc_indicator_icon_increase = Img.Load(await Utils.LoadFile2Byte($"./work/panelv2/icons/indicator.png")).CloneAs<Rgba32>();
                    acc_indicator_icon_increase.Mutate(x => x.Resize(36, 36));
                    if ((data.userInfo.Statistics.HitAccuracy - data.prevUserInfo!.Statistics.HitAccuracy) < 0.0)
                        acc_indicator_icon_increase.Mutate(x => x.Rotate(180));
                    acc_indicator_icon_increase.Mutate(x => x.ProcessPixelRowsAsVector4(row =>
                    {
                        for (int p = 0; p < row.Length; p++)
                        {
                            row[p].X = ((Vector4)accDiffIconColor).X;
                            row[p].Y = ((Vector4)accDiffIconColor).Y;
                            row[p].Z = ((Vector4)accDiffIconColor).Z;
                            if (accDiffIconColorAlpha) if (row[p].W > 0.2f) row[p].W = row[p].W * ((Vector4)accDiffIconColor).W;
                        }
                    }));
                    info.Mutate(x => x.DrawImage(acc_indicator_icon_increase, new Point(3740, 536), 1));
                }
            }
            //ppsub
            textOptions.HorizontalAlignment = HorizontalAlignment.Right;
            textOptions.Font = new Font(TorusRegular, 40);
            var ppsub_point = 2374 + pp_front_length - 40;
            textOptions.Origin = new PointF(ppsub_point, 440);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:N0}", scorePP), new SolidBrush(ppProgressBarColorTextColor), null));

            //acc sub
            var accsub_point = (2374 + (int)(1443.00 * (data.userInfo.Statistics.HitAccuracy / 100.0))) - 40;
            textOptions.Origin = new PointF(accsub_point, 641);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, "300", new SolidBrush(accProgressBarColorTextColor), null));

            //grades
            textOptions.Font = new Font(TorusRegular, 38);
            textOptions.HorizontalAlignment = HorizontalAlignment.Center;
            textOptions.Origin = new PointF(2646, 988);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:N0}", data.userInfo.Statistics.GradeCounts.SSH), new SolidBrush(GradeStatisticsColor_XH), null));
            textOptions.Origin = new PointF(2646 + 218, 988);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:N0}", data.userInfo.Statistics.GradeCounts.SS), new SolidBrush(GradeStatisticsColor_X), null));
            textOptions.Origin = new PointF(2646 + 218 * 2, 988);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:N0}", data.userInfo.Statistics.GradeCounts.SH), new SolidBrush(GradeStatisticsColor_SH), null));
            textOptions.Origin = new PointF(2646 + 218 * 3, 988);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:N0}", data.userInfo.Statistics.GradeCounts.S), new SolidBrush(GradeStatisticsColor_S), null));
            textOptions.Origin = new PointF(2646 + 218 * 4, 988);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:N0}", data.userInfo.Statistics.GradeCounts.A), new SolidBrush(GradeStatisticsColor_A), null));

            //level main
            textOptions.Origin = new PointF(3906, 2470);
            textOptions.Font = new Font(TorusRegular, 48);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, data.userInfo.Statistics.Level.Current.ToString(), new SolidBrush(LevelTitleColor), null));

            //update time
            textOptions.Origin = new PointF(3955, 2582);
            textOptions.HorizontalAlignment = HorizontalAlignment.Right;
            textOptions.Font = new Font(TorusRegular, 40);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, $"Update at {DateTime.Now}", new SolidBrush(footerColor), null));

            //desu.life
            textOptions.HorizontalAlignment = HorizontalAlignment.Left;
            textOptions.Origin = new PointF(90, 2582);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, $"Kanonbot - desu.life", new SolidBrush(footerColor), null));

            //details
            textOptions.Font = new Font(TorusRegular, 50);

            //play time
            textOptions.Origin = new PointF(1705, 1217);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, Utils.Duration2StringWithoutSec(data.userInfo.Statistics.PlayTime), new SolidBrush(Details_PlayTimeColor), null));
            //total hits
            textOptions.Origin = new PointF(2285, 1217);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:N0}", data.userInfo.Statistics.TotalHits), new SolidBrush(Details_TotalHitsColor), null));
            //play count
            textOptions.Origin = new PointF(2853, 1217);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:N0}", data.userInfo.Statistics.PlayCount), new SolidBrush(Details_PlayCountColor), null));
            //ranked scores
            textOptions.Origin = new PointF(3420, 1217);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:N0}", data.userInfo.Statistics.RankedScore), new SolidBrush(Details_RankedScoreColor), null));

            //details diff
            if (isBonded)
            {
                textOptions.Font = new Font(TorusRegular, 36);
                Img indicator_icon_increase = Img.Load(await Utils.LoadFile2Byte($"./work/panelv2/icons/indicator.png")).CloneAs<Rgba32>();
                indicator_icon_increase.Mutate(x => x.Resize(42, 42));
                //Img indicator_icon_decrease = Img.Load(await Utils.LoadFile2Byte($"./work/panelv2/icons/indicator.png")).CloneAs<Rgba32>();
                //indicator_icon_decrease.Mutate(x => x.Resize(42, 42).Rotate(180));
                var text = "";
                //play time
                textOptions.Origin = new PointF(1705, 1265);
                text = Utils.Duration2StringWithoutSec(data.userInfo.Statistics.PlayTime - data.prevUserInfo!.Statistics.PlayTime);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, text, new SolidBrush(DetailsDiff_PlayTimeColor), null));
                var m = TextMeasurer.Measure(text, textOptions);
                indicator_icon_increase.Mutate(x => x.ProcessPixelRowsAsVector4(row =>
                {
                    for (int p = 0; p < row.Length; p++)
                    {
                        row[p].X = ((Vector4)DetailsDiff_PlayTimeIconColor).X;
                        row[p].Y = ((Vector4)DetailsDiff_PlayTimeIconColor).Y;
                        row[p].Z = ((Vector4)DetailsDiff_PlayTimeIconColor).Z;
                        if (DetailsPlaytimeIconAlpha) if (row[p].W > 0.2f) row[p].W = row[p].W * ((Vector4)DetailsDiff_PlayTimeIconColor).W;
                    }
                }));
                info.Mutate(x => x.DrawImage(indicator_icon_increase, new Point(1705 + (int)m.Width + 10, 1247), 1));

                //total hits
                textOptions.Origin = new PointF(2285, 1265);
                text = string.Format("{0:N0}", data.userInfo.Statistics.TotalHits - data.prevUserInfo!.Statistics.TotalHits);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, text, new SolidBrush(DetailsDiff_TotalHitsColor), null));
                m = TextMeasurer.Measure(text, textOptions);
                indicator_icon_increase.Mutate(x => x.ProcessPixelRowsAsVector4(row =>
                {
                    for (int p = 0; p < row.Length; p++)
                    {
                        row[p].X = ((Vector4)DetailsDiff_TotalHitsIconColor).X;
                        row[p].Y = ((Vector4)DetailsDiff_TotalHitsIconColor).Y;
                        row[p].Z = ((Vector4)DetailsDiff_TotalHitsIconColor).Z;
                        if (DetailsTotalHitIconAlpha) if (row[p].W > 0.2f) row[p].W = row[p].W * ((Vector4)DetailsDiff_TotalHitsIconColor).W;
                    }
                }));
                info.Mutate(x => x.DrawImage(indicator_icon_increase, new Point(2285 + (int)m.Width + 10, 1247), 1));

                //play count
                textOptions.Origin = new PointF(2853, 1265);
                text = string.Format("{0:N0}", data.userInfo.Statistics.PlayCount - data.prevUserInfo!.Statistics.PlayCount);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, text, new SolidBrush(DetailsDiff_PlayCountColor), null));
                m = TextMeasurer.Measure(text, textOptions);
                indicator_icon_increase.Mutate(x => x.ProcessPixelRowsAsVector4(row =>
                {
                    for (int p = 0; p < row.Length; p++)
                    {
                        row[p].X = ((Vector4)DetailsDiff_PlayCountIconColor).X;
                        row[p].Y = ((Vector4)DetailsDiff_PlayCountIconColor).Y;
                        row[p].Z = ((Vector4)DetailsDiff_PlayCountIconColor).Z;
                        if (DetailsPlayCountIconAlpha) if (row[p].W > 0.2f) row[p].W = row[p].W * ((Vector4)DetailsDiff_PlayCountIconColor).W;
                    }
                }));
                info.Mutate(x => x.DrawImage(indicator_icon_increase, new Point(2853 + (int)m.Width + 10, 1247), 1));

                //ranked scores
                textOptions.Origin = new PointF(3420, 1265);
                text = string.Format("{0:N0}", data.userInfo.Statistics.RankedScore - data.prevUserInfo!.Statistics.RankedScore);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, text, new SolidBrush(DetailsDiff_RankedScoreColor), null));
                m = TextMeasurer.Measure(text, textOptions);
                indicator_icon_increase.Mutate(x => x.ProcessPixelRowsAsVector4(row =>
                {
                    for (int p = 0; p < row.Length; p++)
                    {
                        row[p].X = ((Vector4)DetailsDiff_RankedScoreIconColor).X;
                        row[p].Y = ((Vector4)DetailsDiff_RankedScoreIconColor).Y;
                        row[p].Z = ((Vector4)DetailsDiff_RankedScoreIconColor).Z;
                        if (DetailsRankedScoreIconAlpha) if (row[p].W > 0.2f) row[p].W = row[p].W * ((Vector4)DetailsDiff_RankedScoreIconColor).W;
                    }
                }));
                info.Mutate(x => x.DrawImage(indicator_icon_increase, new Point(3420 + (int)m.Width + 10, 1247), 1));
            }





            if (allBP.Length > 5)
            {
                #region ppcount>=5
                //top performance
                //title  +mods
                textOptions.Font = new Font(TorusRegular, 90);
                textOptions.Origin = new PointF(1945, 1590);
                var title = "";
                foreach (char c in allBP![0].Beatmapset!.Title)
                {
                    title += c;
                    var m = TextMeasurer.Measure(title, textOptions);
                    if (m.Width > 725)
                    {
                        title += "...";
                        break;
                    }
                }
                info.Mutate(x => x.DrawText(drawOptions, textOptions, title, new SolidBrush(MainBPTitleColor), null));
                //mods
                if (allBP![0].Mods.Length > 0)
                {
                    textOptions.Origin = new PointF(1945 + TextMeasurer.Measure(title, textOptions).Width + 25, 1611);
                    textOptions.Font = new Font(TorusRegular, 40);
                    var mainscoremods = "+";
                    foreach (var x in allBP![0].Mods)
                        mainscoremods += $"{x}, ";
                    info.Mutate(x => x.DrawText(drawOptions, textOptions, mainscoremods[..mainscoremods.LastIndexOf(",")], new SolidBrush(MainBPTitleColor), null));
                }

                //artist
                textOptions.Font = new Font(TorusRegular, 42);
                textOptions.Origin = new PointF(1956, 1668);
                var artist = "";
                foreach (char c in allBP![0].Beatmapset!.Artist)
                {
                    artist += c;
                    var m = TextMeasurer.Measure(artist, textOptions);
                    if (m.Width > 205)
                    {
                        artist += "...";
                        break;
                    }
                }
                info.Mutate(x => x.DrawText(drawOptions, textOptions, artist, new SolidBrush(MainBPArtistColor), null));

                //creator
                textOptions.Origin = new PointF(2231, 1668);
                var creator = "";
                foreach (char c in allBP![0].Beatmapset!.Creator)
                {
                    creator += c;
                    var m = TextMeasurer.Measure(creator, textOptions);
                    if (m.Width > 145)
                    {
                        creator += "...";
                        break;
                    }
                }
                info.Mutate(x => x.DrawText(drawOptions, textOptions, creator, new SolidBrush(MainBPMapperColor), null));

                //bid
                textOptions.Origin = new PointF(2447, 1668);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, allBP![0].Beatmap!.BeatmapId.ToString(), new SolidBrush(MainBPBIDColor), null));

                //get stars from rosupp
                var ppinfo = await PerformanceCalculator.CalculatePanelData(allBP[0]);
                textOptions.Origin = new PointF(2657, 1668);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, ppinfo.ppInfo.star.ToString("0.##*"), new SolidBrush(MainBPStarsColor), null));

                //acc
                textOptions.Origin = new PointF(2813, 1668);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, allBP![0].Accuracy.ToString("0.##%"), new SolidBrush(MainBPAccColor), null));

                //rank
                textOptions.Origin = new PointF(2988, 1668);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, allBP![0].Rank, new SolidBrush(MainBPRankColor), null));

                //2nd~5th bp
                textOptions.HorizontalAlignment = HorizontalAlignment.Left;
                var MainTitleAndDifficultyTitlePos_X = 1673;

                //2nd~5th main title
                textOptions.Font = new Font(TorusRegular, 50);
                for (int i = 1; i < 5; ++i)
                {
                    title = "";
                    foreach (char c in allBP![i].Beatmapset!.Title)
                    {
                        title += c;
                        var m = TextMeasurer.Measure(title, textOptions);
                        if (m.Width > 710)
                        {
                            title += "...";
                            break;
                        }
                    }
                    textOptions.Origin = new PointF(MainTitleAndDifficultyTitlePos_X, 1868 + 186 * (i - 1));
                    switch (i)
                    {
                        case 1:
                            info.Mutate(x => x.DrawText(drawOptions, textOptions, title, new SolidBrush(SubBp2ndBPTitleColor), null));
                            break;
                        case 2:
                            info.Mutate(x => x.DrawText(drawOptions, textOptions, title, new SolidBrush(SubBp3rdBPTitleColor), null));
                            break;
                        case 3:
                            info.Mutate(x => x.DrawText(drawOptions, textOptions, title, new SolidBrush(SubBp4thBPTitleColor), null));
                            break;
                        case 4:
                            info.Mutate(x => x.DrawText(drawOptions, textOptions, title, new SolidBrush(SubBp5thBPTitleColor), null));
                            break;
                        default:
                            break;
                    }
                }

                //2nd~5th version and acc and bid and shdklahdksadkjkcna5hoacsporjasldjlksakdlsa
                textOptions.Font = new Font(TorusRegular, 40);
                var otherbp_mods_pos_y = 1853;
                var score_mode_iconpos_y = 1853;
                for (int i = 1; i < 5; ++i)
                {
                    Color splitC = new(),
                          versionC = new(),
                          bidC = new(),
                          starC = new(),
                          accC = new(),
                          rankC = new(),
                          modeC = new();
                    splitC = SubBpInfoSplitColor;
                    switch (i)
                    {
                        case 1:
                            versionC = SubBp2ndBPVersionColor;
                            bidC = SubBp2ndBPBIDColor;
                            starC = SubBp2ndBPStarsColor;
                            accC = SubBp2ndBPAccColor;
                            rankC = SubBp2ndBPRankColor;
                            modeC = SubBp2ndModeColor;
                            break;
                        case 2:
                            versionC = SubBp3rdBPVersionColor;
                            bidC = SubBp3rdBPBIDColor;
                            starC = SubBp3rdBPStarsColor;
                            accC = SubBp3rdBPAccColor;
                            rankC = SubBp3rdBPRankColor;
                            modeC = SubBp3rdModeColor;
                            break;
                        case 3:
                            versionC = SubBp4thBPVersionColor;
                            bidC = SubBp4thBPBIDColor;
                            starC = SubBp4thBPStarsColor;
                            accC = SubBp4thBPAccColor;
                            rankC = SubBp4thBPRankColor;
                            modeC = SubBp4thModeColor;
                            break;
                        case 4:
                            versionC = SubBp5thBPVersionColor;
                            bidC = SubBp5thBPBIDColor;
                            starC = SubBp5thBPStarsColor;
                            accC = SubBp5thBPAccColor;
                            rankC = SubBp5thBPRankColor;
                            modeC = SubBp5thModeColor;
                            break;
                        default:
                            break;
                    }

                    title = "";
                    foreach (char c in allBP![i].Beatmap!.Version)
                    {
                        title += c;
                        var m = TextMeasurer.Measure(title, textOptions);
                        if (m.Width > 130)
                        {
                            title += "...";
                            break;
                        }
                    }
                    textOptions.Origin = new PointF(MainTitleAndDifficultyTitlePos_X, 1925 + 186 * (i - 1));
                    info.Mutate(x => x.DrawText(drawOptions, textOptions, title, new SolidBrush(versionC), null));
                    var textMeasurePos = MainTitleAndDifficultyTitlePos_X + TextMeasurer.Measure(title, textOptions).Width + 5;
                    //split
                    textOptions.Origin = new PointF(textMeasurePos, 1925 + 186 * (i - 1));
                    info.Mutate(x => x.DrawText(drawOptions, textOptions, " | ", new SolidBrush(splitC), null));
                    textMeasurePos = textMeasurePos + TextMeasurer.Measure(" | ", textOptions).Width + 5;

                    //bid
                    textOptions.Origin = new PointF(textMeasurePos, 1925 + 186 * (i - 1));
                    info.Mutate(x => x.DrawText(drawOptions, textOptions, allBP![i].Beatmap!.BeatmapId.ToString(), new SolidBrush(bidC), null));
                    textMeasurePos = textMeasurePos + TextMeasurer.Measure(allBP![i].Beatmap!.BeatmapId.ToString(), textOptions).Width + 5;

                    //split
                    textOptions.Origin = new PointF(textMeasurePos, 1925 + 186 * (i - 1));
                    info.Mutate(x => x.DrawText(drawOptions, textOptions, " | ", new SolidBrush(splitC), null));
                    textMeasurePos = textMeasurePos + TextMeasurer.Measure(" | ", textOptions).Width + 5;

                    //star
                    var ppinfo1 = await PerformanceCalculator.CalculatePanelData(allBP[i]);
                    textOptions.Origin = new PointF(textMeasurePos, 1925 + 186 * (i - 1));
                    info.Mutate(x => x.DrawText(drawOptions, textOptions, ppinfo1.ppInfo.star.ToString("0.##*"), new SolidBrush(starC), null));
                    textMeasurePos = textMeasurePos + TextMeasurer.Measure(ppinfo1.ppInfo.star.ToString("0.##*"), textOptions).Width + 5;

                    //split
                    textOptions.Origin = new PointF(textMeasurePos, 1925 + 186 * (i - 1));
                    info.Mutate(x => x.DrawText(drawOptions, textOptions, " | ", new SolidBrush(splitC), null));
                    textMeasurePos = textMeasurePos + TextMeasurer.Measure(" | ", textOptions).Width + 5;

                    //acc
                    textOptions.Origin = new PointF(textMeasurePos, 1925 + 186 * (i - 1));
                    info.Mutate(x => x.DrawText(drawOptions, textOptions, allBP![i].Accuracy.ToString("0.##%"), new SolidBrush(accC), null));
                    textMeasurePos = textMeasurePos + TextMeasurer.Measure(allBP![i].Accuracy.ToString("0.##%"), textOptions).Width + 5;

                    //split
                    textOptions.Origin = new PointF(textMeasurePos, 1925 + 186 * (i - 1));
                    info.Mutate(x => x.DrawText(drawOptions, textOptions, " | ", new SolidBrush(splitC), null));
                    textMeasurePos = textMeasurePos + TextMeasurer.Measure(" | ", textOptions).Width + 5;

                    //ranking
                    textOptions.Origin = new PointF(textMeasurePos, 1925 + 186 * (i - 1));
                    info.Mutate(x => x.DrawText(drawOptions, textOptions, allBP![i].Rank, new SolidBrush(rankC), null));
                    //shdklahdksadkjkcna5hoacsporjasldjlksakdlsa
                    if (allBP![i].Mods.Length > 0)
                    {
                        var otherbp_mods_pos_x = 2580;
                        foreach (var x in allBP![i].Mods)
                        {
                            Img modicon = Img.Load(await Utils.LoadFile2Byte($"./work/mods_v2/2x/{x}.png"));
                            modicon.Mutate(x => x.Resize(90, 90).Brightness(ModIconBrightness));
                            modicon.Mutate(x => x.ProcessPixelRowsAsVector4(row =>
                            {
                                for (int p = 0; p < row.Length; p++)
                                    if (row[p].W > 0.2f) row[p].W = ModIconAlpha;
                            }));
                            info.Mutate(x => x.DrawImage(modicon, new Point(otherbp_mods_pos_x, otherbp_mods_pos_y), 1));
                            otherbp_mods_pos_x += 105;
                        }
                    }
                    otherbp_mods_pos_y += 186;

                    //mode_icon
                    Img osuscoremode_icon = Img.Load(await Utils.LoadFile2Byte($"./work/panelv2/icons/mode_icon/score/{data.userInfo.PlayMode.ToStr()}.png")).CloneAs<Rgba32>();
                    osuscoremode_icon.Mutate(x => x.Resize(92, 92));
                    if (FixedScoreModeIconColor)
                    {
                        //固定
                        osuscoremode_icon.Mutate(x => x.ProcessPixelRowsAsVector4(row =>
                        {
                            for (int p = 0; p < row.Length; p++)
                            {
                                row[p].X = ((Vector4)modeC).X;
                                row[p].Y = ((Vector4)modeC).Y;
                                row[p].Z = ((Vector4)modeC).Z;
                                switch (i)
                                {
                                    case 1:
                                        if (Score1ModeIconAlpha) if (row[p].W > 0.0f) row[p].W = row[p].W * ((Vector4)SubBp2ndModeColor).W;
                                        break;
                                    case 2:
                                        if (Score2ModeIconAlpha) if (row[p].W > 0.0f) row[p].W = row[p].W * ((Vector4)SubBp3rdModeColor).W;
                                        break;
                                    case 3:
                                        if (Score3ModeIconAlpha) if (row[p].W > 0.0f) row[p].W = row[p].W * ((Vector4)SubBp4thModeColor).W;
                                        break;
                                    case 4:
                                        if (Score4ModeIconAlpha) if (row[p].W > 0.0f) row[p].W = row[p].W * ((Vector4)SubBp5thModeColor).W;
                                        break;
                                }
                            }
                        }));
                    }
                    else
                    {
                        //随难度渐变
                        modeC = Utils.ForStarDifficulty(ppinfo1.ppInfo.star);
                        osuscoremode_icon.Mutate(x => x.ProcessPixelRowsAsVector4(row =>
                        {
                            for (int p = 0; p < row.Length; p++)
                            {
                                row[p].X = ((Vector4)modeC).X;
                                row[p].Y = ((Vector4)modeC).Y;
                                row[p].Z = ((Vector4)modeC).Z;
                            }
                        }));

                    }
                    info.Mutate(x => x.DrawImage(osuscoremode_icon, new Point(1558, score_mode_iconpos_y), 1));
                    score_mode_iconpos_y += 186;
                }

                //all pp
                textOptions.Font = new Font(TorusRegular, 90);
                textOptions.HorizontalAlignment = HorizontalAlignment.Center;
                textOptions.Origin = new PointF(3642, 1670);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:N1}", allBP![0].PP), new SolidBrush(MainBPppMainColor), null));
                var bp1pptextMeasure = TextMeasurer.Measure(string.Format("{0:N1}", allBP![0].PP), textOptions);
                int bp1pptextpos = 3642 - (int)bp1pptextMeasure.Width / 2;
                textOptions.Font = new Font(TorusRegular, 40);
                textOptions.Origin = new PointF(bp1pptextpos, 1610);
                textOptions.HorizontalAlignment = HorizontalAlignment.Left;
                info.Mutate(x => x.DrawText(drawOptions, textOptions, "pp", new SolidBrush(MainBPppTitleColor), null));

                textOptions.HorizontalAlignment = HorizontalAlignment.Center;
                textOptions.Font = new Font(TorusRegular, 70);
                textOptions.Origin = new PointF(3642, 1895);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:N0}pp", allBP![1].PP), new SolidBrush(SubBp2ndBPppMainColor), null));
                textOptions.Origin = new PointF(3642, 2081);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:N0}pp", allBP![2].PP), new SolidBrush(SubBp3rdBPppMainColor), null));
                textOptions.Origin = new PointF(3642, 2266);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:N0}pp", allBP![3].PP), new SolidBrush(SubBp4thBPppMainColor), null));
                textOptions.Origin = new PointF(3642, 2450);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:N0}pp", allBP![4].PP), new SolidBrush(SubBp5thBPppMainColor), null));
                #endregion
            }
            else
            {
                #region ppcount<5
                var score_mode_iconpos_y = 1853;
                //top performance
                //title  +mods
                textOptions.Font = new Font(TorusRegular, 90);
                textOptions.Origin = new PointF(1945, 1590);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, "-", new SolidBrush(MainBPTitleColor), null));

                //artist
                textOptions.Font = new Font(TorusRegular, 42);
                textOptions.Origin = new PointF(1956, 1668);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, "-", new SolidBrush(MainBPArtistColor), null));

                //creator
                textOptions.Origin = new PointF(2231, 1668);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, "-", new SolidBrush(MainBPMapperColor), null));

                //bid
                textOptions.Origin = new PointF(2447, 1668);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, "-", new SolidBrush(MainBPBIDColor), null));

                //star
                textOptions.Origin = new PointF(2657, 1668);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, "-", new SolidBrush(MainBPStarsColor), null));

                //acc
                textOptions.Origin = new PointF(2813, 1668);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, "-", new SolidBrush(MainBPAccColor), null));

                //rank
                textOptions.Origin = new PointF(2988, 1668);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, "-", new SolidBrush(MainBPRankColor), null));

                //2nd~5th bp
                textOptions.HorizontalAlignment = HorizontalAlignment.Left;
                var MainTitleAndDifficultyTitlePos_X = 1673;

                //2nd~5th main title
                textOptions.Font = new Font(TorusRegular, 50);
                for (int i = 1; i < 5; ++i)
                {
                    textOptions.Origin = new PointF(MainTitleAndDifficultyTitlePos_X, 1868 + 186 * (i - 1));
                    switch (i)
                    {
                        case 1:
                            info.Mutate(x => x.DrawText(drawOptions, textOptions, "-", new SolidBrush(SubBp2ndBPTitleColor), null));
                            break;
                        case 2:
                            info.Mutate(x => x.DrawText(drawOptions, textOptions, "-", new SolidBrush(SubBp3rdBPTitleColor), null));
                            break;
                        case 3:
                            info.Mutate(x => x.DrawText(drawOptions, textOptions, "-", new SolidBrush(SubBp4thBPTitleColor), null));
                            break;
                        case 4:
                            info.Mutate(x => x.DrawText(drawOptions, textOptions, "-", new SolidBrush(SubBp5thBPTitleColor), null));
                            break;
                        default:
                            break;
                    }
                }

                //2nd~5th version and acc and bid and shdklahdksadkjkcna5hoacsporjasldjlksakdlsa
                textOptions.Font = new Font(TorusRegular, 40);
                var otherbp_mods_pos_y = 1853;
                for (int i = 1; i < 5; ++i)
                {
                    Color splitC = new(),
                          versionC = new(),
                          bidC = new(),
                          starC = new(),
                          accC = new(),
                          rankC = new(),
                          modeC = new();
                    splitC = SubBpInfoSplitColor;
                    switch (i)
                    {
                        case 1:
                            versionC = SubBp2ndBPVersionColor;
                            bidC = SubBp2ndBPBIDColor;
                            starC = SubBp2ndBPStarsColor;
                            accC = SubBp2ndBPAccColor;
                            rankC = SubBp2ndBPRankColor;
                            modeC = SubBp2ndModeColor;
                            break;
                        case 2:
                            versionC = SubBp3rdBPVersionColor;
                            bidC = SubBp3rdBPBIDColor;
                            starC = SubBp3rdBPStarsColor;
                            accC = SubBp3rdBPAccColor;
                            rankC = SubBp3rdBPRankColor;
                            modeC = SubBp3rdModeColor;
                            break;
                        case 3:
                            versionC = SubBp4thBPVersionColor;
                            bidC = SubBp4thBPBIDColor;
                            starC = SubBp4thBPStarsColor;
                            accC = SubBp4thBPAccColor;
                            rankC = SubBp4thBPRankColor;
                            modeC = SubBp4thModeColor;
                            break;
                        case 4:
                            versionC = SubBp5thBPVersionColor;
                            bidC = SubBp5thBPBIDColor;
                            starC = SubBp5thBPStarsColor;
                            accC = SubBp5thBPAccColor;
                            rankC = SubBp5thBPRankColor;
                            modeC = SubBp5thModeColor;
                            break;
                        default:
                            break;
                    }

                    textOptions.Origin = new PointF(MainTitleAndDifficultyTitlePos_X, 1925 + 186 * (i - 1));
                    info.Mutate(x => x.DrawText(drawOptions, textOptions, "-", new SolidBrush(versionC), null));
                    var textMeasurePos = MainTitleAndDifficultyTitlePos_X + TextMeasurer.Measure("-", textOptions).Width + 5;
                    //split
                    textOptions.Origin = new PointF(textMeasurePos, 1925 + 186 * (i - 1));
                    info.Mutate(x => x.DrawText(drawOptions, textOptions, " | ", new SolidBrush(splitC), null));
                    textMeasurePos = textMeasurePos + TextMeasurer.Measure(" | ", textOptions).Width + 5;

                    //bid
                    textOptions.Origin = new PointF(textMeasurePos, 1925 + 186 * (i - 1));
                    info.Mutate(x => x.DrawText(drawOptions, textOptions, "-", new SolidBrush(bidC), null));
                    textMeasurePos = textMeasurePos + TextMeasurer.Measure("-", textOptions).Width + 5;

                    //split
                    textOptions.Origin = new PointF(textMeasurePos, 1925 + 186 * (i - 1));
                    info.Mutate(x => x.DrawText(drawOptions, textOptions, " | ", new SolidBrush(splitC), null));
                    textMeasurePos = textMeasurePos + TextMeasurer.Measure(" | ", textOptions).Width + 5;

                    //star
                    textOptions.Origin = new PointF(textMeasurePos, 1925 + 186 * (i - 1));
                    info.Mutate(x => x.DrawText(drawOptions, textOptions, "-", new SolidBrush(starC), null));
                    textMeasurePos = textMeasurePos + TextMeasurer.Measure("-", textOptions).Width + 5;

                    //split
                    textOptions.Origin = new PointF(textMeasurePos, 1925 + 186 * (i - 1));
                    info.Mutate(x => x.DrawText(drawOptions, textOptions, " | ", new SolidBrush(splitC), null));
                    textMeasurePos = textMeasurePos + TextMeasurer.Measure(" | ", textOptions).Width + 5;

                    //acc
                    textOptions.Origin = new PointF(textMeasurePos, 1925 + 186 * (i - 1));
                    info.Mutate(x => x.DrawText(drawOptions, textOptions, "-", new SolidBrush(accC), null));
                    textMeasurePos = textMeasurePos + TextMeasurer.Measure("-", textOptions).Width + 5;

                    //split
                    textOptions.Origin = new PointF(textMeasurePos, 1925 + 186 * (i - 1));
                    info.Mutate(x => x.DrawText(drawOptions, textOptions, " | ", new SolidBrush(splitC), null));
                    textMeasurePos = textMeasurePos + TextMeasurer.Measure(" | ", textOptions).Width + 5;

                    //ranking
                    textOptions.Origin = new PointF(textMeasurePos, 1925 + 186 * (i - 1));
                    info.Mutate(x => x.DrawText(drawOptions, textOptions, "-", new SolidBrush(rankC), null));
                    otherbp_mods_pos_y += 186;

                    //mode_icon
                    Img osuscoremode_icon = Img.Load(await Utils.LoadFile2Byte($"./work/panelv2/icons/mode_icon/score/{data.userInfo.PlayMode.ToStr()}.png")).CloneAs<Rgba32>();
                    osuscoremode_icon.Mutate(x => x.Resize(92, 92));
                    if (FixedScoreModeIconColor)
                    {
                        //固定
                        osuscoremode_icon.Mutate(x => x.ProcessPixelRowsAsVector4(row =>
                        {
                            for (int p = 0; p < row.Length; p++)
                            {
                                row[p].X = ((Vector4)modeC).X;
                                row[p].Y = ((Vector4)modeC).Y;
                                row[p].Z = ((Vector4)modeC).Z;
                                switch (i)
                                {
                                    case 1:
                                        if (Score1ModeIconAlpha) if (row[p].W > 0.0f) row[p].W = row[p].W * ((Vector4)SubBp2ndModeColor).W;
                                        break;
                                    case 2:
                                        if (Score2ModeIconAlpha) if (row[p].W > 0.0f) row[p].W = row[p].W * ((Vector4)SubBp3rdModeColor).W;
                                        break;
                                    case 3:
                                        if (Score3ModeIconAlpha) if (row[p].W > 0.0f) row[p].W = row[p].W * ((Vector4)SubBp4thModeColor).W;
                                        break;
                                    case 4:
                                        if (Score4ModeIconAlpha) if (row[p].W > 0.0f) row[p].W = row[p].W * ((Vector4)SubBp5thModeColor).W;
                                        break;
                                }
                            }
                        }));
                    }
                    else
                    {
                        //随难度渐变
                        modeC = Utils.ForStarDifficulty(0.01);
                        osuscoremode_icon.Mutate(x => x.ProcessPixelRowsAsVector4(row =>
                        {
                            for (int p = 0; p < row.Length; p++)
                            {
                                row[p].X = ((Vector4)modeC).X;
                                row[p].Y = ((Vector4)modeC).Y;
                                row[p].Z = ((Vector4)modeC).Z;
                            }
                        }));

                    }
                    info.Mutate(x => x.DrawImage(osuscoremode_icon, new Point(1558, score_mode_iconpos_y), 1));
                    score_mode_iconpos_y += 186;
                }


                //all pp
                textOptions.Font = new Font(TorusRegular, 90);
                textOptions.HorizontalAlignment = HorizontalAlignment.Center;
                textOptions.Origin = new PointF(3642, 1670);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, "-", new SolidBrush(MainBPppMainColor), null));
                var bp1pptextMeasure = TextMeasurer.Measure("-", textOptions);
                int bp1pptextpos = 3642 - (int)bp1pptextMeasure.Width / 2;
                textOptions.Font = new Font(TorusRegular, 40);
                textOptions.Origin = new PointF(bp1pptextpos, 1610);
                textOptions.HorizontalAlignment = HorizontalAlignment.Left;
                info.Mutate(x => x.DrawText(drawOptions, textOptions, "pp", new SolidBrush(MainBPppTitleColor), null));

                textOptions.HorizontalAlignment = HorizontalAlignment.Center;
                textOptions.Font = new Font(TorusRegular, 70);
                textOptions.Origin = new PointF(3642, 1895);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, "-", new SolidBrush(SubBp2ndBPppMainColor), null));
                textOptions.Origin = new PointF(3642, 2081);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, "-", new SolidBrush(SubBp3rdBPppMainColor), null));
                textOptions.Origin = new PointF(3642, 2266);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, "-", new SolidBrush(SubBp4thBPppMainColor), null));
                textOptions.Origin = new PointF(3642, 2450);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, "-", new SolidBrush(SubBp5thBPppMainColor), null));
                #endregion
            }


            //badges
            if (data.badgeId != -1)
            {
                Img badge;
                badge = Img.Load(await Utils.LoadFile2Byte($"./work/badges/{data.badgeId}.png"), out IImageFormat format).CloneAs<Rgba32>();
                //检测上传的infopanel尺寸是否正确
                if (format.DefaultMimeType.Trim().ToLower()[..3] != "png")
                {
                    Img temp = badge;
                    File.Delete($"./work/badges/{data.badgeId}.png");
                    temp.Save($"./work/badges/{data.badgeId}.png", new PngEncoder());
                    badge = Img.Load(await Utils.LoadFile2Byte($"./work/badges/{data.badgeId}.png")).CloneAs<Rgba32>();
                }
                badge.Mutate(x => x.Resize(236, 110).Brightness(BadgeBrightness).RoundCorner(new Size(236, 110), 20));

                badge.Mutate(x => x.ProcessPixelRowsAsVector4(row =>
                {
                    for (int p = 0; p < row.Length; p++)
                        if (row[p].W > 0.2f) row[p].W = BadgeAlpha;
                }));
                info.Mutate(x => x.DrawImage(badge, new Point(3566, 93), 1));
            }

            //osu!mode
            Img osuprofilemode_icon = Img.Load(await Utils.LoadFile2Byte($"./work/panelv2/icons/mode_icon/profile/{data.userInfo.PlayMode.ToStr()}.png")).CloneAs<Rgba32>();
            var osuprofilemode_text = "";
            switch (data.userInfo.PlayMode)
            {
                case OSU.Enums.Mode.OSU:
                    osuprofilemode_text = "osu!standard";
                    break;
                case OSU.Enums.Mode.Taiko:
                    osuprofilemode_text = "osu!taiko";
                    break;
                case OSU.Enums.Mode.Fruits:
                    osuprofilemode_text = "osu!catch";
                    break;
                case OSU.Enums.Mode.Mania:
                    osuprofilemode_text = "osu!mania";
                    break;
            }
            textOptions.Font = new Font(TorusRegular, 55);
            textOptions.HorizontalAlignment = HorizontalAlignment.Left;
            var osuprofilemode_text_measure = TextMeasurer.Measure(osuprofilemode_text, textOptions);
            Img osuprofilemode = new Image<Rgba32>((int)(70.0f + osuprofilemode_text_measure.Width), 102); //804x102(80?)
            osuprofilemode_icon.Mutate(x => x.Resize(60, 60));
            osuprofilemode.Mutate(x => x.DrawImage(osuprofilemode_icon, new Point(0, 21), 1));
            textOptions.Origin = new PointF(70, 48);
            osuprofilemode.Mutate(x => x.DrawText(drawOptions, textOptions, osuprofilemode_text, new SolidBrush(footerColor), null));
            osuprofilemode.Mutate(x => x.ProcessPixelRowsAsVector4(row =>
            {
                for (int p = 0; p < row.Length; p++)
                {
                    //X、Y、Z和W字段分别映射 RGBA 通道。
                    row[p].X = ((Vector4)ModeIconColor).X;
                    row[p].Y = ((Vector4)ModeIconColor).Y;
                    row[p].Z = ((Vector4)ModeIconColor).Z;
                    if (ModeIconAlpha) if (row[p].W > 0.0f) row[p].W = row[p].W * ((Vector4)ModeIconColor).W;
                }
            }));

            var osuprofilemode_x_pos = 1531 + (804 / 2) - (osuprofilemode.Width / 2);
            info.Mutate(x => x.DrawImage(osuprofilemode, new Point(osuprofilemode_x_pos, 293), 1));

            //osu!rankChart
            try
            {
                var RankHistory = data.userInfo.RankHistory.Data.Take(8).Reverse().ToArray();
                var rankChart = DrawLineChart(714, 240, 7, RankHistory, RankLineChartColor, RankLineChartTextColor, RankLineChartDashColor,
                    RankLineChartDotColor, RankLineChartDotStrokeColor, true, 7f, 5f);
                info.Mutate(x => x.DrawImage(rankChart, new Point(1576, 694), 1));
            }
            catch
            {
                long[] RankHistory = { 0, 0, 0, 0, 0, 0, 0, 0 };
                var rankChart = DrawLineChart(714, 240, 7, RankHistory, RankLineChartColor, RankLineChartTextColor, RankLineChartDashColor,
                    RankLineChartDotColor, RankLineChartDotStrokeColor, true, 7f, 5f);
                info.Mutate(x => x.DrawImage(rankChart, new Point(1576, 694), 1));
            }
            //date
            var DateXPos = 2227;
            var DateValue = DateTime.Now;
            textOptions.HorizontalAlignment = HorizontalAlignment.Center;
            textOptions.Font = new Font(TorusRegular, 34);
            for (int i = 0; i < 7; i++)
            {
                textOptions.Origin = new PointF(DateXPos, 960);
                info.Mutate(x => x.DrawText(drawOptions, textOptions,
                        $"{DateValue.Month}.{DateValue.Day}", new SolidBrush(RankLineChartDateTextColor), null));
                DateXPos -= 100;
                DateValue = DateValue.AddDays(-1);
            }

            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!test info!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            textOptions.Font = new Font(TorusRegular, 40);
            textOptions.Origin = new PointF(2000, 2582);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, "this is a test version and does not represent the final quality", new SolidBrush(footerColor), null));

            //resize to 1920x?
            info.Mutate(x => x.Resize(new ResizeOptions() { Size = new Size(1920, 0), Mode = ResizeMode.Max }));
            return info;
        }


        //RawData[0]不绘制，仅仅为了适配InfoPanelV2的差异文本输出，其它方法调用的时候保持第一个数据为0 DataCount一定为RawData.Length-1
        public static Img DrawLineChart(int Width, int Height,
            int DataCount, long[] RawData, Color ChartLineColor, Color ChartTextColor, Color DashColor, Color DotColor, Color DotStrokeColor,
            bool DrawDiff, float dotThickness, float LineThickness)
        {
            Img image = new Image<Rgba32>(Width, Height);

            List<int> xPos = new();
            List<int> yPos = new();

            //计算x坐标
            var xPosEach = (Width - 10) / DataCount;
            for (int i = 0; i < DataCount; i++)
                xPos.Add(50 + xPosEach * i);

            //计算y坐标
            long[] Data = RawData.Take(7).Reverse().ToArray();

            var yPosMax = Data.Max();
            var yPosMin = Data.Min();

            for (int i = 0; i < DataCount; i++)
            {
                yPos.Add(((int)(((double)Height - 80.00) * ((double)(Data[i] - yPosMin) / (double)(yPosMax - yPosMin)))) + 50);
            }

            //绘制虚线
            for (int i = 0; i < DataCount; i++)
            {
                PointF[] p = {
                new Point(xPos[i], yPos[i]),
                new Point(xPos[i], Height + 20)
                };
                IPen pen = Pens.Dash(DashColor, 3f);
                image.Mutate(x => x.DrawLines(pen, p));
            }

            //绘制线
            for (int i = 0; i < DataCount - 1; i++)
            {
                PointF[] p = {
                new Point(xPos[i], yPos[i]),
                new Point(xPos[i + 1], yPos[i + 1])
                };
                image.Mutate(x => x.DrawLines(ChartLineColor, LineThickness, p));
            }

            //绘制点
            for (int i = 0; i < DataCount; i++)
                image.Mutate(x => x.Fill(DotStrokeColor, new EllipsePolygon(new Point(xPos[i], yPos[i]), dotThickness / 4 * 5)));
            for (int i = 0; i < DataCount; i++)
                image.Mutate(x => x.Fill(DotColor, new EllipsePolygon(new Point(xPos[i], yPos[i]), dotThickness)));






            //绘制差异数值
            if (DrawDiff)
            {
                var textOptions = new TextOptions(new Font(TorusSemiBold, 120))
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                var drawOptions = new DrawingOptions
                {
                    GraphicsOptions = new GraphicsOptions
                    {
                        Antialias = true
                    }
                };

                textOptions.Font = new Font(TorusRegular, 40);
                Data = RawData.Reverse().ToArray();
                for (int i = 0; i < DataCount; i++)
                {
                    textOptions.Origin = new PointF(xPos[i], yPos[i] - 34);
                    image.Mutate(x => x.DrawText(drawOptions, textOptions,
                      ((Data[i + 1] - Data[i]) * -1).ToString(), new SolidBrush(ChartTextColor), null));
                }
            }
            return image;
        }
    }
}
