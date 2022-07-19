#pragma warning disable CS8618 // 非null 字段未初始化
using System.Numerics;
using KanonBot.API;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Drawing;
using Img = SixLabors.ImageSharp.Image;
using SixLabors.Fonts;
using SixLabors.ImageSharp.ColorSpaces;
using Serilog;
using Flurl;
using Flurl.Http;

namespace KanonBot.LegacyImage
{
    public static class Draw
    {
        public class UserPanelData
        {
            public OSU.Models.User userInfo;
            public OSU.Models.User? prevUserInfo;
            public OSU.Models.PPlusData.UserData? pplusInfo;
            public string? customPanel;
            public int daysBefore = 0;
            public int badgeId = -1;
        }
        public class ScorePanelData
        {
            public OSU.PPInfo ppInfo;
            public List<OSU.PPInfo.PPStat>? ppStats;
            public OSU.Models.Score scoreInfo;

        }
        public class PPVSPanelData
        {
            public string u1Name;
            public string u2Name;
            public OSU.Models.PPlusData.UserData u1;
            public OSU.Models.PPlusData.UserData u2;
        }

        //customBannerStatus 0=没有自定义banner 1=在猫猫上设置了自定义banner 
        public static MemoryStream DrawInfo(UserPanelData data, int customBannerStatus, bool isBonded = false, bool isDataOfDayAvaiavle = true, bool eventmode = false)
        {
            using var info = new Image<Rgba32>(1200, 857);
            var fonts = new FontCollection();
            var Exo2SemiBold = fonts.Add("./work/fonts/Exo2/Exo2-SemiBold.ttf");
            var Exo2Regular = fonts.Add("./work/fonts/Exo2/Exo2-Regular.ttf");
            var HarmonySans = fonts.Add("./work/fonts/HarmonyOS_Sans_SC/HarmonyOS_Sans_SC_Regular.ttf");
            // custom panel
            var panelPath = "./work/legacy/default-info-v1.png";
            if (File.Exists($"./work/legacy/v1_infopanel/{data.userInfo.Id}.png")) panelPath = $"./work/legacy/v1_infopanel/{data.userInfo.Id}.png";
            Img panel = Img.Load(panelPath);

            var coverPath = $"./work/legacy/v1_cover/{data.userInfo.Id}.png";
            if (customBannerStatus == 1)
            {
                coverPath = $"./work/legacy/v1_cover/custom/{data.userInfo.Id}.png";
            }
            else if (!File.Exists(coverPath))
            {
                try
                {
                    coverPath = data.userInfo.CoverUrl.DownloadFileAsync("./work/legacy/v1_cover/", $"{data.userInfo.Id}.png").Result;
                }
                catch
                {
                    int n = new Random().Next(1, 6);
                    coverPath = $"./work/legacy/v1_cover/default/default_{n}.png";
                }
            }
            Img cover = Img.Load(coverPath);
            var resizeOptions = new ResizeOptions
            {
                Size = new Size(1200, 350),
                Sampler = KnownResamplers.Lanczos3,
                Compand = true,
                Mode = ResizeMode.Crop
            };
            cover.Mutate(x => x.Resize(resizeOptions));
            // cover.Mutate(x => x.RoundCorner(new Size(1200, 350), 20));
            info.Mutate(x => x.DrawImage(cover, 1));
            info.Mutate(x => x.DrawImage(panel, 1));


            //avatar
            var avatarPath = $"./work/avatar/{data.userInfo.Id}.png";
            Img avatar;
            try
            {
                avatar = Img.Load(avatarPath).CloneAs<Rgba32>();    // 读取
            }
            catch
            {
                try
                {
                    avatarPath = data.userInfo.AvatarUrl.DownloadFileAsync("./work/avatar/", $"{data.userInfo.Id}.png").Result;
                }
                catch (Exception ex)
                {
                    var msg = $"从API下载用户头像时发生了一处异常\n异常类型: {ex.GetType()}\n异常信息: '{ex.Message}'";
                    Log.Error(msg);
                    throw;  // 下载失败直接抛出error
                }
                avatar = Img.Load(avatarPath).CloneAs<Rgba32>();    // 下载后再读取
            }
            avatar.Mutate(x => x.Resize(190, 190).RoundCorner(new Size(190, 190), 40));
            info.Mutate(x => x.DrawImage(avatar, new Point(39, 55), 1));

            // badge
            if (data.badgeId != -1)
            {
                try
                {
                    Img badge = Img.Load($"./work/badges/{data.badgeId}.png");
                    badge.Mutate(x => x.Resize(86, 40).RoundCorner(new Size(86, 40), 5));
                    info.Mutate(x => x.DrawImage(badge, new Point(272, 152), 1));
                }
                catch { }
            }

            // obj
            var drawOptions = new DrawingOptions
            {
                GraphicsOptions = new GraphicsOptions
                {
                    Antialias = true
                }
            };

            Img flags = Img.Load($"./work/flags/{data.userInfo.Country.Code}.png");
            info.Mutate(x => x.DrawImage(flags, new Point(272, 212), 1));
            Img modeicon = Img.Load($"./work/legacy/mode_icon/{data.userInfo.PlayMode.ToModeStr()}.png");
            modeicon.Mutate(x => x.Resize(64, 64));
            info.Mutate(x => x.DrawImage(modeicon, new Point(1125, 10), 1));

            // pp+
            if (data.userInfo.PlayMode is OSU.Enums.Mode.OSU)
            {
                Img ppdataPanel = Img.Load("./work/legacy/pp+-v1.png");
                info.Mutate(x => x.DrawImage(ppdataPanel, new Point(0, 0), 1));
                Hexagram.HexagramInfo hi = new();
                hi.abilityFillColor = Color.FromRgba(253, 148, 62, 128);
                hi.abilityLineColor = Color.ParseHex("#fd943e");
                hi.nodeMaxValue = 10000;
                hi.nodeCount = 6;
                hi.size = 200;
                hi.sideLength = 200;
                hi.mode = 1;
                hi.strokeWidth = 2f;
                hi.nodesize = new SizeF(5f, 5f);
                // acc ,flow, jump, pre, speed, sta
                var ppd = new int[6];       // 这里就强制转换了
                ppd[0] = (int)data.pplusInfo!.AccuracyTotal;
                ppd[1] = (int)data.pplusInfo.FlowAimTotal;
                ppd[2] = (int)data.pplusInfo.JumpAimTotal;
                ppd[3] = (int)data.pplusInfo.PrecisionTotal;
                ppd[4] = (int)data.pplusInfo.SpeedTotal;
                ppd[5] = (int)data.pplusInfo.StaminaTotal;
                // x_offset  pp+数据的坐标偏移量
                var x_offset = new int[6] { 372, 330, 122, 52, 128, 317 };   // pp+数据的x轴坐标
                var multi = new double[6] { 14.1, 69.7, 1.92, 19.8, 0.588, 3.06 };
                var exp = new double[6] { 0.769, 0.596, 0.953, 0.8, 1.175, 0.993 };
                var pppImg = Hexagram.Draw(ppd, multi, exp, hi);
                info.Mutate(x => x.DrawImage(pppImg, new Point(132, 626), 1));
                var f = new Font(Exo2Regular, 18);
                var pppto = new TextOptions(f)
                {
                    VerticalAlignment = VerticalAlignment.Bottom,
                    HorizontalAlignment = HorizontalAlignment.Left,
                };
                var color = Color.ParseHex("#FFCC33");
                for (var i = 0; i < hi.nodeCount; i++)
                {
                    pppto.Origin = new Vector2(x_offset[i], (i % 3 != 0) ? (i < 3 ? 642 : 831) : 736);
                    info.Mutate(x => x.DrawText(drawOptions, pppto, $"({ppd[i]})", new SolidBrush(color), null));
                }
            }
            else
            {
                Img ppdataPanel = Img.Load("./work/legacy/nopp+info-v1.png");
                info.Mutate(x => x.DrawImage(ppdataPanel, new Point(0, 0), 1));
            }

            // time
            var textOptions = new TextOptions(new Font(Exo2Regular, 20))
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            textOptions.Origin = new PointF(15, 25);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, $"update: {(DateTime.Now).ToString().Replace("/", " / ")}", new SolidBrush(Color.White), null));
            if (data.daysBefore > 1)
            {
                textOptions = new TextOptions(new Font(HarmonySans, 20))
                {
                    VerticalAlignment = VerticalAlignment.Bottom,
                    HorizontalAlignment = HorizontalAlignment.Left,
                };
                if (isDataOfDayAvaiavle)
                {
                    textOptions.Origin = new PointF(300, 25);
                    info.Mutate(x => x.DrawText(drawOptions, textOptions, $"对比自{data.daysBefore}天前", new SolidBrush(Color.White), null));
                }
                else
                {
                    textOptions.Origin = new PointF(300, 25);
                    info.Mutate(x => x.DrawText(drawOptions, textOptions, $" 请求的日期没有数据.." +
                        $"当前数据对比自{data.daysBefore}天前", new SolidBrush(Color.White), null));
                }
            }
            // username
            textOptions.Font = new Font(Exo2SemiBold, 60);
            textOptions.Origin = new PointF(268, 140);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, data.userInfo.Username, new SolidBrush(Color.White), null));

            var Statistics = data.userInfo.Statistics;
            var prevStatistics = data.prevUserInfo?.Statistics ?? data.userInfo.Statistics; // 没有就为当前数据

            // country_rank
            string countryRank;
            if (isBonded)
            {
                var diff = Statistics.CountryRank - prevStatistics!.CountryRank;
                if (diff > 0) countryRank = string.Format("#{0:N0}(-{1:N0})", Statistics.CountryRank, diff);
                else if (diff < 0) countryRank = string.Format("#{0:N0}(+{1:N0})", Statistics.CountryRank, Math.Abs(diff));
                else countryRank = string.Format("#{0:N0}", Statistics.CountryRank);
            }
            else
            {
                countryRank = string.Format("#{0:N0}", Statistics.CountryRank);
            }
            textOptions.Font = new Font(Exo2SemiBold, 20);
            textOptions.Origin = new PointF(350, 260);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, countryRank, new SolidBrush(Color.White), null));
            // global_rank
            string diffStr;
            if (isBonded)
            {
                var diff = Statistics.GlobalRank - prevStatistics!.GlobalRank;
                if (diff > 0) diffStr = string.Format("↓ {0:N0}", diff);
                else if (diff < 0) diffStr = string.Format("↑ {0:N0}", Math.Abs(diff));
                else diffStr = "↑ -";
            }
            else
            {
                diffStr = "↑ -";
            }
            textOptions.Font = new Font(Exo2Regular, 40);
            textOptions.Origin = new PointF(40, 410);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:N0}", Statistics.GlobalRank), new SolidBrush(Color.White), null));
            textOptions.Font = new Font(HarmonySans, 14);
            textOptions.Origin = new PointF(40, 430);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, diffStr, new SolidBrush(Color.White), null));
            // pp
            if (isBonded)
            {
                var diff = Statistics.PP - prevStatistics!.PP;
                if (diff >= 0.01) diffStr = string.Format("↑ {0:0.##}", diff);
                else if (diff <= -0.01) diffStr = string.Format("↓ {0:0.##}", Math.Abs(diff));
                else diffStr = "↑ -";
            }
            else
            {
                diffStr = "↑ -";
            }
            textOptions.Font = new Font(Exo2Regular, 40);
            textOptions.Origin = new PointF(246, 410);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:0.##}", Statistics.PP), new SolidBrush(Color.White), null));
            textOptions.Font = new Font(HarmonySans, 14);
            textOptions.Origin = new PointF(246, 430);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, diffStr, new SolidBrush(Color.White), null));
            // ssh ss
            textOptions.Font = new Font(Exo2Regular, 30);
            textOptions.HorizontalAlignment = HorizontalAlignment.Center;
            textOptions.Origin = new PointF(80, 540);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, Statistics.GradeCounts.SSH.ToString(), new SolidBrush(Color.White), null));
            textOptions.Origin = new PointF(191, 540);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, Statistics.GradeCounts.SS.ToString(), new SolidBrush(Color.White), null));
            textOptions.Origin = new PointF(301, 540);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, Statistics.GradeCounts.SH.ToString(), new SolidBrush(Color.White), null));
            textOptions.Origin = new PointF(412, 540);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, Statistics.GradeCounts.S.ToString(), new SolidBrush(Color.White), null));
            textOptions.Origin = new PointF(522, 540);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, Statistics.GradeCounts.A.ToString(), new SolidBrush(Color.White), null));
            // level
            textOptions.Font = new Font(Exo2SemiBold, 34);
            textOptions.Origin = new PointF(1115, 385);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, Statistics.Level.Current.ToString(), new SolidBrush(Color.White), null));
            // Level%
            var levelper = Statistics.Level.Progress;
            textOptions.HorizontalAlignment = HorizontalAlignment.Right;
            textOptions.Font = new Font(Exo2SemiBold, 20);
            textOptions.Origin = new PointF(1060, 400);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, $"{levelper}%", new SolidBrush(Color.White), null));
            try
            {
                var levelRoundrect = new Image<Rgba32>(4 * levelper, 7);
                levelRoundrect.Mutate(x => x.Fill(Color.ParseHex("#FF66AB")).RoundCorner(new Size(4 * levelper, 7), 5));
                info.Mutate(x => x.DrawImage(levelRoundrect, new Point(662, 370), 1));
            }
            catch (ArgumentOutOfRangeException) { }
            // SCORES
            textOptions.Font = new Font(Exo2Regular, 36);
            string rankedScore;
            // if (isBonded){
            //     var diff = data.userInfo.rankedScore - data.prevUserInfo.rankedScore;
            //     if (diff > 0) rankedScore = string.Format("{0:N0}(+{1:N0})", data.userInfo.rankedScore, diff);
            //     else if (diff < 0) rankedScore = string.Format("{0:N0}({1:N0})", data.userInfo.rankedScore, diff);
            //     else rankedScore = string.Format("{0:N0}", data.userInfo.rankedScore);
            // } else {
            //     rankedScore = string.Format("{0:N0}", data.userInfo.rankedScore);
            // }
            rankedScore = string.Format("{0:N0}", Statistics.RankedScore);
            textOptions.Origin = new PointF(1180, 625);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, rankedScore, new SolidBrush(Color.White), null));
            string acc;
            if (isBonded)
            {
                var diff = Statistics.HitAccuracy - prevStatistics!.HitAccuracy;
                if (diff >= 0.01) acc = string.Format("{0:0.##}%(+{1:0.##}%)", Statistics.HitAccuracy, diff);
                else if (diff <= -0.01) acc = string.Format("{0:0.##}%({1:0.##}%)", Statistics.HitAccuracy, diff);
                else acc = string.Format("{0:0.##}%", Statistics.HitAccuracy);
            }
            else
            {
                acc = string.Format("{0:0.##}%", Statistics.HitAccuracy);
            }
            textOptions.Origin = new PointF(1180, 665);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, acc, new SolidBrush(Color.White), null));
            string playCount;
            if (isBonded)
            {
                var diff = Statistics.PlayCount - prevStatistics!.PlayCount;
                if (diff > 0) playCount = string.Format("{0:N0}(+{1:N0})", Statistics.PlayCount, diff);
                else if (diff < 0) playCount = string.Format("{0:N0}({1:N0})", Statistics.PlayCount, diff);
                else playCount = string.Format("{0:N0}", Statistics.PlayCount);
            }
            else
            {
                playCount = string.Format("{0:N0}", Statistics.PlayCount);
            }
            textOptions.Origin = new PointF(1180, 705);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, playCount, new SolidBrush(Color.White), null));
            string totalScore;
            // if (isBonded){
            //     var diff = data.userInfo.totalScore - data.prevUserInfo.totalScore;
            //     if (diff > 0) totalScore = string.Format("{0:N0}(+{1:N0})", data.userInfo.totalScore, diff);
            //     else if (diff < 0) totalScore = string.Format("{0:N0}({1:N0})", data.userInfo.totalScore, diff);
            //     else totalScore = string.Format("{0:N0}", data.userInfo.totalScore);
            // } else {
            //     totalScore = string.Format("{0:N0}", data.userInfo.totalScore);
            // }
            totalScore = string.Format("{0:N0}", Statistics.TotalScore);
            textOptions.Origin = new PointF(1180, 745);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, totalScore, new SolidBrush(Color.White), null));
            string totalHits;
            if (isBonded)
            {
                var diff = Statistics.TotalHits - prevStatistics!.TotalHits;
                if (diff > 0) totalHits = string.Format("{0:N0}(+{1:N0})", Statistics.TotalHits, diff);
                else if (diff < 0) totalHits = string.Format("{0:N0}({1:N0})", Statistics.TotalHits, diff);
                else totalHits = string.Format("{0:N0}", Statistics.TotalHits);
            }
            else
            {
                totalHits = string.Format("{0:N0}", Statistics.TotalHits);
            }
            textOptions.Origin = new PointF(1180, 785);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, totalHits, new SolidBrush(Color.White), null));
            textOptions.Origin = new PointF(1180, 825);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, Utils.Duration2String(Statistics.PlayTime), new SolidBrush(Color.White), null));
            info.Mutate(x => x.RoundCorner(new Size(1200, 857), 24));
            var stream = new MemoryStream();

            info.SaveAsPng(stream);
            return stream;
        }
        public static MemoryStream DrawScore(ScorePanelData data, bool res)
        {
            // 先下载必要文件
            var bgPath = $"./work/background/{data.scoreInfo.Beatmap!.BeatmapId}.png";
            if (!File.Exists(bgPath))
            {
                try
                {
                    bgPath = OSU.SayoDownloadBeatmapBackgroundImg(data.scoreInfo.Beatmap.BeatmapsetId, data.scoreInfo.Beatmap.BeatmapId, "./work/background/").Result;
                }
                catch (Exception ex)
                {
                    var msg = $"从API下载背景图片时发生了一处异常\n异常类型: {ex.GetType()}\n异常信息: '{ex.Message}'";
                    Log.Warning(msg);
                }
            }

            var avatarPath = $"./work/avatar/{data.scoreInfo.UserId}.png";
            Img avatar;
            try
            {
                avatar = Img.Load(avatarPath).CloneAs<Rgba32>();    // 读取
            }
            catch
            {
                try
                {
                    avatarPath = data.scoreInfo.User!.AvatarUrl.DownloadFileAsync("./work/avatar/", $"{data.scoreInfo.UserId}.png").Result;
                }
                catch (Exception ex)
                {
                    var msg = $"从API下载用户头像时发生了一处异常\n异常类型: {ex.GetType()}\n异常信息: '{ex.Message}'";
                    Log.Error(msg);
                    throw;
                }
                avatar = Img.Load(avatarPath).CloneAs<Rgba32>();    // 下载后再读取
            }

            using (var score = Img.Load("work/legacy/v2_scorepanel/default-score-v2.png"))
            {
                var fonts = new FontCollection();
                var TorusRegular = fonts.Add("./work/fonts/Torus-Regular.ttf");
                var TorusSemiBold = fonts.Add("./work/fonts/Torus-SemiBold.ttf");
                var HarmonySans = fonts.Add("./work/fonts/HarmonyOS_Sans_SC/HarmonyOS_Sans_SC_Regular.ttf");

                Img panel;
                if (data.scoreInfo.Mode is OSU.Enums.Mode.Fruits) panel = Img.Load("work/legacy/v2_scorepanel/default-score-v2-fruits.png");
                else if (data.scoreInfo.Mode is OSU.Enums.Mode.Mania) panel = Img.Load("work/legacy/v2_scorepanel/default-score-v2-mania.png");
                else panel = Img.Load("work/legacy/v2_scorepanel/default-score-v2.png");
                // bg
                Img bg;
                try { bg = Img.Load(bgPath).CloneAs<Rgba32>(); }
                catch { bg = Img.Load("./work/legacy/load-failed-img.png"); }
                var smallBg = bg.Clone(x => x.RoundCorner(new Size(433, 296), 20));
                Img backBlack = new Image<Rgba32>(1950 - 2, 1088);
                backBlack.Mutate(x => x.BackgroundColor(Color.Black).RoundCorner(new Size(1950 - 2, 1088), 20));
                bg.Mutate(x => x.GaussianBlur(5).RoundCorner(new Size(1950 - 2, 1088), 20));
                score.Mutate(x => x.DrawImage(bg, 1));
                score.Mutate(x => x.DrawImage(backBlack, 0.33f));
                score.Mutate(x => x.DrawImage(panel, 1));
                score.Mutate(x => x.DrawImage(smallBg, new Point(27, 34), 1));
                // StarRing
                // diff circle
                // green, blue, yellow, red, purple, black
                // [0,2), [2,3), [3,4), [4,5), [5,7), [7,?)
                var ringFile = new string[6];
                switch (data.scoreInfo.Mode)
                {
                    case OSU.Enums.Mode.OSU:
                        ringFile[0] = "std-easy.png";
                        ringFile[1] = "std-normal.png";
                        ringFile[2] = "std-hard.png";
                        ringFile[3] = "std-insane.png";
                        ringFile[4] = "std-expert.png";
                        ringFile[5] = "std-expertplus.png";
                        break;
                    case OSU.Enums.Mode.Fruits:
                        ringFile[0] = "ctb-easy.png";
                        ringFile[1] = "ctb-normal.png";
                        ringFile[2] = "ctb-hard.png";
                        ringFile[3] = "ctb-insane.png";
                        ringFile[4] = "ctb-expert.png";
                        ringFile[5] = "ctb-expertplus.png";
                        break;
                    case OSU.Enums.Mode.Taiko:
                        ringFile[0] = "taiko-easy.png";
                        ringFile[1] = "taiko-normal.png";
                        ringFile[2] = "taiko-hard.png";
                        ringFile[3] = "taiko-insane.png";
                        ringFile[4] = "taiko-expert.png";
                        ringFile[5] = "taiko-expertplus.png";
                        break;
                    case OSU.Enums.Mode.Mania:
                        ringFile[0] = "mania-easy.png";
                        ringFile[1] = "mania-normal.png";
                        ringFile[2] = "mania-hard.png";
                        ringFile[3] = "mania-insane.png";
                        ringFile[4] = "mania-expert.png";
                        ringFile[5] = "mania-expertplus.png";
                        break;
                }
                string temp;
                var star = data.ppInfo.star;
                if (star < 2)
                {
                    temp = ringFile[0];
                }
                else if (star < 2.7)
                {
                    temp = ringFile[1];
                }
                else if (star < 4)
                {
                    temp = ringFile[2];
                }
                else if (star < 5.3)
                {
                    temp = ringFile[3];
                }
                else if (star < 6.5)
                {
                    temp = ringFile[4];
                }
                else
                {
                    temp = ringFile[5];
                }
                var diffCircle = Img.Load("./work/icons/" + temp);
                diffCircle.Mutate(x => x.Resize(65, 65));
                score.Mutate(x => x.DrawImage(diffCircle, new Point(512, 257), 1));
                // beatmap_status
                if (data.scoreInfo.Beatmap.Status is OSU.Enums.Status.ranked)
                    score.Mutate(x => x.DrawImage(Img.Load("./work/icons/ranked.png"), new Point(415, 16), 1));
                if (data.scoreInfo.Beatmap.Status is OSU.Enums.Status.approved)
                    score.Mutate(x => x.DrawImage(Img.Load("./work/icons/approved.png"), new Point(415, 16), 1));
                if (data.scoreInfo.Beatmap.Status is OSU.Enums.Status.loved)
                    score.Mutate(x => x.DrawImage(Img.Load("./work/icons/loved.png"), new Point(415, 16), 1));
                // mods
                var mods = data.scoreInfo.Mods;
                var modp = 0;
                foreach (var mod in mods)
                {
                    Img modPic;
                    try { modPic = Img.Load($"./work/mods/{mod}.png"); } catch { continue; }
                    modPic.Mutate(x => x.Resize(200, 61));
                    score.Mutate(x => x.DrawImage(modPic, new Point((modp * 160) + 440, 440), 1));
                    modp += 1;
                }
                // rankings
                var ranking = data.scoreInfo.Rank;
                var rankPic = Img.Load($"./work/ranking/ranking-{ranking}.png");
                score.Mutate(x => x.DrawImage(rankPic, new Point(913, 874), 1));
                // text part (文字部分)
                var font = new Font(TorusRegular, 60);
                var drawOptions = new DrawingOptions
                {
                    GraphicsOptions = new GraphicsOptions
                    {
                        Antialias = true
                    }
                };
                var textOptions = new TextOptions(new Font(font, 60))
                {
                    VerticalAlignment = VerticalAlignment.Bottom,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                // beatmap_info
                var title = "";
                foreach (char c in data.scoreInfo.Beatmapset!.Title)
                {
                    title += c;
                    var m = TextMeasurer.Measure(title, textOptions);
                    if (m.Width > 725)
                    {
                        title += "...";
                        break;
                    }
                }
                textOptions.Origin = new PointF(499, 110);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, title, new SolidBrush(Color.Black), null));
                textOptions.Origin = new PointF(499, 105);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, title, new SolidBrush(Color.White), null));
                // artist
                textOptions.Font = new Font(TorusRegular, 40);
                var artist = "";
                foreach (char c in data.scoreInfo.Beatmapset.Artist)
                {
                    artist += c;
                    var m = TextMeasurer.Measure(artist, textOptions);
                    if (m.Width > 205)
                    {
                        artist += "...";
                        break;
                    }
                }
                textOptions.Origin = new PointF(519, 178);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, artist, new SolidBrush(Color.Black), null));
                textOptions.Origin = new PointF(519, 175);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, artist, new SolidBrush(Color.White), null));
                // creator
                var creator = "";
                foreach (char c in data.scoreInfo.Beatmapset.Creator)
                {
                    creator += c;
                    var m = TextMeasurer.Measure(creator, textOptions);
                    if (m.Width > 145)
                    {
                        creator += "...";
                        break;
                    }
                }
                textOptions.Origin = new PointF(795, 178);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, creator, new SolidBrush(Color.Black), null));
                textOptions.Origin = new PointF(795, 175);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, creator, new SolidBrush(Color.White), null));
                // beatmap_id
                var beatmap_id = data.scoreInfo.Beatmap.BeatmapId.ToString();
                textOptions.Origin = new PointF(1008, 178);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, beatmap_id, new SolidBrush(Color.Black), null));
                textOptions.Origin = new PointF(1008, 175);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, beatmap_id, new SolidBrush(Color.White), null));
                // ar,od info
                var color = Color.ParseHex("#f1ce59");
                textOptions.Font = new Font(TorusRegular, 24.25f);
                // time
                var song_time = Utils.Duration2TimeString(data.scoreInfo.Beatmap.TotalLength);
                textOptions.Origin = new PointF(1741, 127);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, song_time, new SolidBrush(Color.Black), null));
                textOptions.Origin = new PointF(1741, 124);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, song_time, new SolidBrush(Color.White), null));
                // bpm
                var bpm = data.scoreInfo.Beatmap.BPM.GetValueOrDefault().ToString("0");
                textOptions.Origin = new PointF(1457, 127);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, bpm, new SolidBrush(Color.Black), null));
                textOptions.Origin = new PointF(1457, 124);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, bpm, new SolidBrush(Color.White), null));
                // ar
                var ar = data.scoreInfo.Beatmap.AR.ToString("0.0#");
                if (data.scoreInfo.Mode is OSU.Enums.Mode.OSU && data.ppInfo.approachRate != -1) ar = data.ppInfo.approachRate.ToString("0.0#");
                textOptions.Origin = new PointF(1457, 218);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, ar, new SolidBrush(Color.Black), null));
                textOptions.Origin = new PointF(1457, 215);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, ar, new SolidBrush(Color.White), null));
                // od
                var od = data.scoreInfo.Beatmap.Accuracy.ToString("0.0#");
                if (data.scoreInfo.Mode is OSU.Enums.Mode.OSU && data.ppInfo.accuracy != -1) od = data.ppInfo.accuracy.ToString("0.0#");
                textOptions.Origin = new PointF(1741, 218);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, od, new SolidBrush(Color.Black), null));
                textOptions.Origin = new PointF(1741, 215);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, od, new SolidBrush(Color.White), null));
                // cs
                var cs = data.ppInfo.circleSize.ToString("0.0#");
                if (data.ppInfo.circleSize == -1) cs = data.scoreInfo.Beatmap.CS.ToString("0.0#");
                textOptions.Origin = new PointF(1457, 312);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, cs, new SolidBrush(Color.Black), null));
                textOptions.Origin = new PointF(1457, 309);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, cs, new SolidBrush(Color.White), null));
                // hp
                var hp = data.ppInfo.HPDrainRate.ToString("0.0#");
                if (data.ppInfo.HPDrainRate == -1) cs = data.scoreInfo.Beatmap.HPDrain.ToString("0.0#");
                textOptions.Origin = new PointF(1741, 312);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, hp, new SolidBrush(Color.Black), null));
                textOptions.Origin = new PointF(1741, 309);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, hp, new SolidBrush(Color.White), null));
                // stars, version
                var starText = $"Stars: {star.ToString("0.##")}";
                textOptions.Origin = new PointF(584, 292);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, starText, new SolidBrush(Color.Black), null));
                textOptions.Origin = new PointF(584, 289);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, starText, new SolidBrush(Color.White), null));
                var version = "";
                foreach (char c in data.scoreInfo.Beatmap.Version)
                {
                    version += c;
                    var m = TextMeasurer.Measure(version, textOptions);
                    if (m.Width > 140)
                    {
                        version += "...";
                        break;
                    }
                }
                textOptions.Origin = new PointF(584, 320);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, version, new SolidBrush(Color.Black), null));
                textOptions.Origin = new PointF(584, 317);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, version, new SolidBrush(Color.White), null));
                // avatar
                avatar.Mutate(x => x.Resize(80, 80).RoundCorner(new Size(80, 80), 40));
                score.Mutate(
                    x => x.Fill(
                        Color.White,
                        new EllipsePolygon(80, 465, 85, 85)
                    )
                );
                score.Mutate(x => x.DrawImage(avatar, new Point(40, 425), 1));
                // username
                textOptions.Font = new Font(TorusSemiBold, 36);
                var username = data.scoreInfo.User!.Username;
                textOptions.Origin = new PointF(145, 470);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, username, new SolidBrush(Color.Black), null));
                textOptions.Origin = new PointF(145, 467);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, username, new SolidBrush(Color.White), null));
                // time
                textOptions.Font = new Font(TorusRegular, 27.61f);
                var time = data.scoreInfo.CreatedAt.ToString("yyyy/MM/dd HH:mm:ss");
                textOptions.Origin = new PointF(145, 505);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, time, new SolidBrush(Color.Black), null));
                textOptions.Origin = new PointF(145, 502);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, time, new SolidBrush(Color.White), null));

                // pp
                var ppTColor = Color.ParseHex("#cf93ae");
                var ppColor = Color.ParseHex("#fc65a9");
                textOptions.Font = new Font(TorusRegular, 33.5f);
                // aim, speed
                string pptext;
                if (data.ppInfo.ppStat.aim == -1) pptext = "-";
                else pptext = data.ppInfo.ppStat.aim.ToString("0");
                var metric = TextMeasurer.Measure(pptext, textOptions);
                textOptions.Origin = new PointF(1532, 638);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, pptext, new SolidBrush(ppColor), null));
                textOptions.Origin = new PointF(1532 + metric.Width, 638);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, "pp", new SolidBrush(ppTColor), null));
                if (data.ppInfo.ppStat.speed == -1) pptext = "-";
                else pptext = data.ppInfo.ppStat.speed.ToString("0");
                metric = TextMeasurer.Measure(pptext, textOptions);
                textOptions.Origin = new PointF(1672, 638);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, pptext, new SolidBrush(ppColor), null));
                textOptions.Origin = new PointF(1672 + metric.Width, 638);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, "pp", new SolidBrush(ppTColor), null));
                if (data.ppInfo.ppStat.acc == -1) pptext = "-";
                else pptext = data.ppInfo.ppStat.acc.ToString("0");
                metric = TextMeasurer.Measure(pptext, textOptions);
                textOptions.Origin = new PointF(1812, 638);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, pptext, new SolidBrush(ppColor), null));
                textOptions.Origin = new PointF(1812 + metric.Width, 638);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, "pp", new SolidBrush(ppTColor), null));

                if (data.scoreInfo.Mode is OSU.Enums.Mode.Mania || data.scoreInfo.Mode is OSU.Enums.Mode.Fruits)
                {
                    pptext = "-";
                    metric = TextMeasurer.Measure(pptext, textOptions);
                    for (var i = 0; i < 5; i++)
                    {
                        textOptions.Origin = new PointF(50 + 139 * i, 638);
                        score.Mutate(x => x.DrawText(drawOptions, textOptions, pptext, new SolidBrush(ppColor), null));
                        textOptions.Origin = new PointF(50 + 139 * i + metric.Width, 638);
                        score.Mutate(x => x.DrawText(drawOptions, textOptions, "pp", new SolidBrush(ppTColor), null));
                    }
                }
                else
                {
                    for (var i = 0; i < 5; i++)
                    {
                        try
                        {
                            if (data.ppStats![i].total == -1) pptext = "-";
                            else pptext = data.ppStats[5 - (i + 1)].total.ToString("0");
                        }
                        catch
                        {
                            pptext = "-";
                        }
                        metric = TextMeasurer.Measure(pptext, textOptions);
                        textOptions.Origin = new PointF(50 + 139 * i, 638);
                        score.Mutate(x => x.DrawText(drawOptions, textOptions, pptext, new SolidBrush(ppColor), null));
                        textOptions.Origin = new PointF(50 + 139 * i + metric.Width, 638);
                        score.Mutate(x => x.DrawText(drawOptions, textOptions, "pp", new SolidBrush(ppTColor), null));
                    }
                }
                // if fc
                textOptions.Font = new Font(TorusRegular, 24.5f);
                if (data.scoreInfo.Mode is OSU.Enums.Mode.Mania)
                {
                    pptext = "-";
                }
                else
                {
                    try
                    {
                        if (data.ppStats![5].total == -1) pptext = "-";
                        else pptext = data.ppStats[5].total.ToString("0");
                    }
                    catch
                    {
                        pptext = "-";
                    }
                }
                metric = TextMeasurer.Measure(pptext, textOptions);
                textOptions.Origin = new PointF(99, 562);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, pptext, new SolidBrush(ppColor), null));
                textOptions.Origin = new PointF(99 + metric.Width, 562);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, "pp", new SolidBrush(ppTColor), null));
                // total pp
                textOptions.Font = new Font(TorusRegular, 61f);
                if (data.ppInfo.ppStat.total == -1) pptext = "-";
                else pptext = Math.Round(data.ppInfo.ppStat.total).ToString("0");
                textOptions.HorizontalAlignment = HorizontalAlignment.Right;
                textOptions.Origin = new PointF(1825, 500);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, pptext, new SolidBrush(ppColor), null));
                textOptions.Origin = new PointF(1899, 500);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, "pp", new SolidBrush(ppTColor), null));

                // score
                textOptions.HorizontalAlignment = HorizontalAlignment.Center;
                textOptions.Font = new Font(TorusRegular, 40);
                textOptions.Origin = new PointF(980, 750);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, data.scoreInfo.ScoreScore.ToString("N0"), new SolidBrush(Color.White), null));
                if (data.scoreInfo.Mode is OSU.Enums.Mode.Mania)
                {
                    textOptions.Font = new Font(TorusRegular, 40.00f);
                    var great = data.scoreInfo.Statistics.CountGreat.ToString();
                    var ok = data.scoreInfo.Statistics.CountOk.ToString();
                    var meh = data.scoreInfo.Statistics.CountMeh.ToString();
                    var miss = data.scoreInfo.Statistics.CountMiss.ToString();

                    // great
                    textOptions.Origin = new PointF(790, 852);
                    score.Mutate(x => x.DrawText(drawOptions, textOptions, great, new SolidBrush(Color.Black), null));
                    textOptions.Origin = new PointF(790, 849);
                    score.Mutate(x => x.DrawText(drawOptions, textOptions, great, new SolidBrush(Color.White), null));
                    // ok
                    textOptions.Origin = new PointF(790, 975);
                    score.Mutate(x => x.DrawText(drawOptions, textOptions, ok, new SolidBrush(Color.Black), null));
                    textOptions.Origin = new PointF(790, 972);
                    score.Mutate(x => x.DrawText(drawOptions, textOptions, ok, new SolidBrush(Color.White), null));
                    // meh
                    textOptions.Origin = new PointF(1152, 852);
                    score.Mutate(x => x.DrawText(drawOptions, textOptions, meh, new SolidBrush(Color.Black), null));
                    textOptions.Origin = new PointF(1152, 849);
                    score.Mutate(x => x.DrawText(drawOptions, textOptions, meh, new SolidBrush(Color.White), null));
                    // miss
                    textOptions.Origin = new PointF(1152, 975);
                    score.Mutate(x => x.DrawText(drawOptions, textOptions, miss, new SolidBrush(Color.Black), null));
                    textOptions.Origin = new PointF(1152, 972);
                    score.Mutate(x => x.DrawText(drawOptions, textOptions, miss, new SolidBrush(Color.White), null));
                }
                else if (data.scoreInfo.Mode is OSU.Enums.Mode.Mania)
                {
                    textOptions.Font = new Font(TorusRegular, 35.00f);
                    var great = data.scoreInfo.Statistics.CountGreat.ToString();
                    var ok = data.scoreInfo.Statistics.CountOk.ToString();
                    var meh = data.scoreInfo.Statistics.CountMeh.ToString();
                    var miss = data.scoreInfo.Statistics.CountMiss.ToString();
                    var geki = data.scoreInfo.Statistics.CountGeki.ToString();
                    var katu = data.scoreInfo.Statistics.CountKatu.ToString();

                    // great
                    textOptions.Origin = new PointF(790, 837);
                    score.Mutate(x => x.DrawText(drawOptions, textOptions, great, new SolidBrush(Color.Black), null));
                    textOptions.Origin = new PointF(790, 834);
                    score.Mutate(x => x.DrawText(drawOptions, textOptions, great, new SolidBrush(Color.White), null));
                    // geki
                    textOptions.Origin = new PointF(1156, 837);
                    score.Mutate(x => x.DrawText(drawOptions, textOptions, geki, new SolidBrush(Color.Black), null));
                    textOptions.Origin = new PointF(1156, 834);
                    score.Mutate(x => x.DrawText(drawOptions, textOptions, geki, new SolidBrush(Color.White), null));
                    // katu
                    textOptions.Origin = new PointF(790, 910);
                    score.Mutate(x => x.DrawText(drawOptions, textOptions, katu, new SolidBrush(Color.Black), null));
                    textOptions.Origin = new PointF(790, 907);
                    score.Mutate(x => x.DrawText(drawOptions, textOptions, katu, new SolidBrush(Color.White), null));
                    // ok
                    textOptions.Origin = new PointF(1156, 910);
                    score.Mutate(x => x.DrawText(drawOptions, textOptions, ok, new SolidBrush(Color.Black), null));
                    textOptions.Origin = new PointF(1156, 907);
                    score.Mutate(x => x.DrawText(drawOptions, textOptions, ok, new SolidBrush(Color.White), null));
                    // meh
                    textOptions.Origin = new PointF(790, 985);
                    score.Mutate(x => x.DrawText(drawOptions, textOptions, meh, new SolidBrush(Color.Black), null));
                    textOptions.Origin = new PointF(790, 982);
                    score.Mutate(x => x.DrawText(drawOptions, textOptions, meh, new SolidBrush(Color.White), null));
                    // miss
                    textOptions.Origin = new PointF(1156, 985);
                    score.Mutate(x => x.DrawText(drawOptions, textOptions, miss, new SolidBrush(Color.Black), null));
                    textOptions.Origin = new PointF(1156, 982);
                    score.Mutate(x => x.DrawText(drawOptions, textOptions, miss, new SolidBrush(Color.White), null));
                }
                else
                {
                    textOptions.Font = new Font(TorusRegular, 53.09f);
                    var great = data.scoreInfo.Statistics.CountGreat.ToString();
                    var ok = data.scoreInfo.Statistics.CountOk.ToString();
                    var meh = data.scoreInfo.Statistics.CountMeh.ToString();
                    var miss = data.scoreInfo.Statistics.CountMiss.ToString();

                    // great
                    textOptions.Origin = new PointF(795, 857);
                    score.Mutate(x => x.DrawText(drawOptions, textOptions, great, new SolidBrush(Color.Black), null));
                    textOptions.Origin = new PointF(795, 854);
                    score.Mutate(x => x.DrawText(drawOptions, textOptions, great, new SolidBrush(Color.White), null));
                    // ok
                    textOptions.Origin = new PointF(795, 985);
                    score.Mutate(x => x.DrawText(drawOptions, textOptions, ok, new SolidBrush(Color.Black), null));
                    textOptions.Origin = new PointF(795, 982);
                    score.Mutate(x => x.DrawText(drawOptions, textOptions, ok, new SolidBrush(Color.White), null));
                    // meh
                    textOptions.Origin = new PointF(1154, 857);
                    score.Mutate(x => x.DrawText(drawOptions, textOptions, meh, new SolidBrush(Color.Black), null));
                    textOptions.Origin = new PointF(1154, 854);
                    score.Mutate(x => x.DrawText(drawOptions, textOptions, meh, new SolidBrush(Color.White), null));
                    // miss
                    textOptions.Origin = new PointF(1154, 985);
                    score.Mutate(x => x.DrawText(drawOptions, textOptions, miss, new SolidBrush(Color.Black), null));
                    textOptions.Origin = new PointF(1154, 982);
                    score.Mutate(x => x.DrawText(drawOptions, textOptions, miss, new SolidBrush(Color.White), null));
                }

                // acc
                textOptions.Font = new Font(TorusRegular, 53.56f);
                var acc = data.scoreInfo.Accuracy * 100f;
                var hsl = new Hsl(150, 1, 1);
                // ("#ffbd1f") idk?
                color = Color.ParseHex("#87ff6a");
                textOptions.Origin = new PointF(360, 966);
                score.Mutate(x => x.DrawText(drawOptions, textOptions, $"{acc.ToString("0.0#")}%", new SolidBrush(Color.Black), null));
                var acchue = new Image<Rgba32>(1950 - 2, 1088);
                var hue = acc < 60 ? 260f : (acc - 60) * 2 + 280f;
                textOptions.Origin = new PointF(360, 963);
                acchue.Mutate(x => x.DrawText(drawOptions, textOptions, $"{acc.ToString("0.0#")}%", new SolidBrush(color), null).Hue(((float)hue)));
                score.Mutate(x => x.DrawImage(acchue, 1));
                // combo
                var combo = data.scoreInfo.MaxCombo;
                if (data.scoreInfo.Mode is not OSU.Enums.Mode.Mania)
                {
                    var maxCombo = data.ppInfo.maxCombo;
                    if (maxCombo > 0)
                    {
                        textOptions.Origin = new PointF(1598, 966);
                        score.Mutate(x => x.DrawText(drawOptions, textOptions, " / ", new SolidBrush(Color.Black), null));
                        textOptions.Origin = new PointF(1598, 963);
                        score.Mutate(x => x.DrawText(drawOptions, textOptions, " / ", new SolidBrush(Color.White), null));
                        textOptions.HorizontalAlignment = HorizontalAlignment.Left;
                        textOptions.Origin = new PointF(1607, 966);
                        score.Mutate(x => x.DrawText(drawOptions, textOptions, $"{maxCombo}x", new SolidBrush(Color.Black), null));
                        textOptions.Origin = new PointF(1607, 963);
                        score.Mutate(x => x.DrawText(drawOptions, textOptions, $"{maxCombo}x", new SolidBrush(color), null));
                        textOptions.HorizontalAlignment = HorizontalAlignment.Right;
                        textOptions.Origin = new PointF(1588, 966);
                        score.Mutate(x => x.DrawText(drawOptions, textOptions, $"{combo}x", new SolidBrush(Color.Black), null));
                        var combohue = new Image<Rgba32>(1950 - 2, 1088);
                        hue = (((float)combo / (float)maxCombo) * 100) + 260;
                        textOptions.Origin = new PointF(1588, 963);
                        combohue.Mutate(x => x.DrawText(drawOptions, textOptions, $"{combo}x", new SolidBrush(color), null).Hue(((float)hue)));
                        score.Mutate(x => x.DrawImage(combohue, 1));
                    }
                    else
                    {
                        textOptions.HorizontalAlignment = HorizontalAlignment.Center;
                        textOptions.Origin = new PointF(1598, 966);
                        score.Mutate(x => x.DrawText(drawOptions, textOptions, $"{combo}x", new SolidBrush(Color.Black), null));
                        textOptions.Origin = new PointF(1598, 963);
                        score.Mutate(x => x.DrawText(drawOptions, textOptions, $"{combo}x", new SolidBrush(color), null));
                    }
                }
                else
                {
                    textOptions.HorizontalAlignment = HorizontalAlignment.Center;
                    textOptions.Origin = new PointF(1598, 966);
                    score.Mutate(x => x.DrawText(drawOptions, textOptions, $"{combo}x", new SolidBrush(Color.Black), null));
                    textOptions.Origin = new PointF(1598, 963);
                    score.Mutate(x => x.DrawText(drawOptions, textOptions, $"{combo}x", new SolidBrush(color), null));
                }
                // 
                var stream = new MemoryStream();
                if (res)
                {
                    score.SaveAsPng(stream);
                }
                else
                {
                    score.SaveAsJpeg(stream);
                }
                return stream;
            }
        }
        public static MemoryStream DrawPPVS(PPVSPanelData data)
        {
            using (var ppvsImg = Img.Load("work/legacy/ppvs.png"))
            {
                Hexagram.HexagramInfo hi = new();
                // hi.abilityLineColor = Color.ParseHex("#FF7BAC");
                hi.nodeCount = 6;
                hi.nodeMaxValue = 12000;
                hi.size = 1134;
                hi.sideLength = 791;
                hi.mode = 2;
                hi.strokeWidth = 6f;
                hi.nodesize = new SizeF(15f, 15f);
                var multi = new double[6] { 14.1, 69.7, 1.92, 19.8, 0.588, 3.06 };
                var exp = new double[6] { 0.769, 0.596, 0.953, 0.8, 1.175, 0.993 };
                var u1d = new int[6];
                u1d[0] = (int)data.u1.AccuracyTotal;
                u1d[1] = (int)data.u1.FlowAimTotal;
                u1d[2] = (int)data.u1.JumpAimTotal;
                u1d[3] = (int)data.u1.PrecisionTotal;
                u1d[4] = (int)data.u1.SpeedTotal;
                u1d[5] = (int)data.u1.StaminaTotal;
                var u2d = new int[6];
                u2d[0] = (int)data.u2.AccuracyTotal;
                u2d[1] = (int)data.u2.FlowAimTotal;
                u2d[2] = (int)data.u2.JumpAimTotal;
                u2d[3] = (int)data.u2.PrecisionTotal;
                u2d[4] = (int)data.u2.SpeedTotal;
                u2d[5] = (int)data.u2.StaminaTotal;
                // acc ,flow, jump, pre, speed, sta

                if (data.u1.PerformanceTotal < data.u2.PerformanceTotal)
                {
                    hi.abilityFillColor = Color.FromRgba(255, 123, 172, 50);
                    hi.abilityLineColor = Color.FromRgba(255, 123, 172, 255);
                    ppvsImg.Mutate(x => x.DrawImage(Hexagram.Draw(u1d, multi, exp, hi), new Point(0, -120), 1));
                    hi.abilityFillColor = Color.FromRgba(41, 171, 226, 50);
                    hi.abilityLineColor = Color.FromRgba(41, 171, 226, 255);
                    ppvsImg.Mutate(x => x.DrawImage(Hexagram.Draw(u2d, multi, exp, hi), new Point(0, -120), 1));
                }
                else
                {
                    hi.abilityFillColor = Color.FromRgba(41, 171, 226, 50);
                    hi.abilityLineColor = Color.FromRgba(41, 171, 226, 255);
                    ppvsImg.Mutate(x => x.DrawImage(Hexagram.Draw(u2d, multi, exp, hi), new Point(0, -120), 1));
                    hi.abilityFillColor = Color.FromRgba(255, 123, 172, 50);
                    hi.abilityLineColor = Color.FromRgba(255, 123, 172, 255);
                    ppvsImg.Mutate(x => x.DrawImage(Hexagram.Draw(u1d, multi, exp, hi), new Point(0, -120), 1));
                }

                // text
                var fonts = new FontCollection();
                var avenirLTStdMedium = fonts.Add("./work/fonts/AvenirLTStd-Medium.ttf");
                var drawOptions = new DrawingOptions
                {
                    GraphicsOptions = new GraphicsOptions
                    {
                        Antialias = true
                    }
                };



                // 打印用户名
                var font = new Font(avenirLTStdMedium, 36);
                var textOptions = new TextOptions(font)
                {
                    VerticalAlignment = VerticalAlignment.Bottom,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Origin = new PointF(808, 882)
                };
                var color = Color.ParseHex("#999999");
                ppvsImg.Mutate(x => x.DrawText(drawOptions, textOptions, data.u1Name, new SolidBrush(color), null));
                textOptions.Origin = new PointF(264, 882);
                ppvsImg.Mutate(x => x.DrawText(drawOptions, textOptions, data.u2Name, new SolidBrush(color), null));


                // 打印每个用户数据
                var y_offset = new int[6] { 1471, 1136, 1052, 1220, 1304, 1389 };   // pp+数据的y轴坐标
                font = new Font(avenirLTStdMedium, 32);
                textOptions = new TextOptions(font)
                {
                    VerticalAlignment = VerticalAlignment.Bottom,
                    HorizontalAlignment = HorizontalAlignment.Left,
                };
                for (var i = 0; i < u1d.Length; i++)
                {
                    textOptions.Origin = new PointF(664, y_offset[i] + 8);
                    ppvsImg.Mutate(x => x.DrawText(drawOptions, textOptions, u1d[i].ToString(), new SolidBrush(color), null));
                }
                textOptions.Origin = new PointF(664, 974);
                ppvsImg.Mutate(x => x.DrawText(drawOptions, textOptions, data.u1.PerformanceTotal.ToString(), new SolidBrush(color), null));
                for (var i = 0; i < u2d.Length; i++)
                {
                    textOptions.Origin = new PointF(424, y_offset[i] + 8);
                    ppvsImg.Mutate(x => x.DrawText(drawOptions, textOptions, u2d[i].ToString(), new SolidBrush(color), null));
                }
                textOptions.Origin = new PointF(424, 974);
                ppvsImg.Mutate(x => x.DrawText(drawOptions, textOptions, data.u2.PerformanceTotal.ToString(), new SolidBrush(color), null));

                // 打印数据差异
                var diffPoint = 960;
                color = Color.ParseHex("#ffcd22");
                textOptions.Origin = new PointF(diffPoint, 974);
                ppvsImg.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:0}", (data.u2.PerformanceTotal - data.u1.PerformanceTotal)), new SolidBrush(color), null));
                textOptions.Origin = new PointF(diffPoint, 1060);
                ppvsImg.Mutate(x => x.DrawText(drawOptions, textOptions, (u2d[2] - u1d[2]).ToString(), new SolidBrush(color), null));
                textOptions.Origin = new PointF(diffPoint, 1144);
                ppvsImg.Mutate(x => x.DrawText(drawOptions, textOptions, (u2d[1] - u1d[1]).ToString(), new SolidBrush(color), null));
                textOptions.Origin = new PointF(diffPoint, 1228);
                ppvsImg.Mutate(x => x.DrawText(drawOptions, textOptions, (u2d[3] - u1d[3]).ToString(), new SolidBrush(color), null));
                textOptions.Origin = new PointF(diffPoint, 1312);
                ppvsImg.Mutate(x => x.DrawText(drawOptions, textOptions, (u2d[4] - u1d[4]).ToString(), new SolidBrush(color), null));
                textOptions.Origin = new PointF(diffPoint, 1397);
                ppvsImg.Mutate(x => x.DrawText(drawOptions, textOptions, (u2d[5] - u1d[5]).ToString(), new SolidBrush(color), null));
                textOptions.Origin = new PointF(diffPoint, 1479);
                ppvsImg.Mutate(x => x.DrawText(drawOptions, textOptions, (u2d[0] - u1d[0]).ToString(), new SolidBrush(color), null));

                Img title = Img.Load($"work/legacy/ppvs_title.png");
                ppvsImg.Mutate(x => x.DrawImage(title, new Point(0, 0), 1));

                var stream = new MemoryStream();
                ppvsImg.SaveAsPng(stream);
                return stream;
            }
        }
        public static MemoryStream DrawString(string str, float fontSize)
        {
            var fonts = new FontCollection();
            var HarmonySans = fonts.Add("./work/fonts/HarmonyOS_Sans_SC/HarmonyOS_Sans_SC_Regular.ttf");
            var font = new Font(HarmonySans, fontSize);
            var textOptions = new TextOptions(new Font(HarmonySans, fontSize))
            {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Origin = new PointF(fontSize / 2, fontSize / 2)
            };
            var m = TextMeasurer.Measure(str, textOptions);

            using (var img = new Image<Rgba32>((int)(m.Width + fontSize), (int)(m.Height + fontSize)))
            {
                img.Mutate(x => x.Fill(Color.White));
                var drawOptions = new DrawingOptions
                {
                    GraphicsOptions = new GraphicsOptions
                    {
                        Antialias = true
                    }
                };
                img.Mutate(x => x.DrawText(drawOptions, textOptions, str, new SolidBrush(Color.Black), null));
                var stream = new MemoryStream();
                img.SaveAsJpeg(stream);
                return stream;
            }
        }

        #region Hexagram
        public class Hexagram
        {
            public struct R8 { public double r, _8; }
            public struct HexagramInfo
            {
                public int size, nodeCount, nodeMaxValue, sideLength, mode;
                public float strokeWidth;
                public SizeF nodesize;
                public Color abilityFillColor, abilityLineColor;
            }

            // 极坐标转直角坐标系
            public static PointF r82xy(R8 r8)
            {
                PointF xy = new();
                xy.X = (float)(r8.r * Math.Sin(r8._8 * Math.PI / 180));
                xy.Y = (float)(r8.r * Math.Cos(r8._8 * Math.PI / 180));
                return xy;
            }

            // ppd          pp_plus_data, 注意要与下面的multi和exp参数数量相同且对齐
            // multi, exp   加权值 y = multi * x ^ exp
            // hi           pp+图片的一些设置参数, hi.nodeCount
            public static Img Draw(int[] ppd, double[] multi, double[] exp, HexagramInfo hi)
            {
                var image = new Image<Rgba32>(hi.size, hi.size);
                PointF[] points = new PointF[hi.nodeCount];
                for (var i = 0; i < hi.nodeCount; i++)
                {
                    var r = Math.Pow((multi[i] * Math.Pow(ppd[i], exp[i]) / hi.nodeMaxValue), 0.8) * hi.size / 2.0;
                    if (hi.mode == 1 && r > 100.00) r = 100.00;
                    if (hi.mode == 2 && r > 395.00) r = 395.00;
                    if (hi.mode == 3 && r > 495.00) r = 495.00;
                    Hexagram.R8 r8 = new();
                    r8.r = r;
                    r8._8 = 360.0 / hi.nodeCount * i + 90;
                    var xy = Hexagram.r82xy(r8);
                    xy.X += hi.size / 2;
                    xy.Y += hi.size / 2;
                    points[i] = xy;
                    xy.X += hi.nodesize.Width / 10;
                    xy.Y += hi.nodesize.Height / 10;
                    image.Mutate(
                        x => x.Fill(
                            hi.abilityLineColor,
                            new EllipsePolygon(xy, hi.nodesize)
                        )
                    );
                }
                image.Mutate(
                    x => x.DrawPolygon(
                        hi.abilityLineColor,
                        hi.strokeWidth,
                        points
                    ).FillPolygon(hi.abilityFillColor, points)
                );
                return image;
            }
        }
        #endregion

        #region RoundedCorners
        public static IImageProcessingContext ApplyRoundedCorners(this IImageProcessingContext ctx, float cornerRadius)
        {
            Size size = ctx.GetCurrentSize();
            IPathCollection corners = BuildCorners(size.Width, size.Height, cornerRadius);

            ctx.SetGraphicsOptions(new GraphicsOptions()
            {
                Antialias = true,
                AlphaCompositionMode = PixelAlphaCompositionMode.DestOut // enforces that any part of this shape that has color is punched out of the background
            });

            // mutating in here as we already have a cloned original
            // use any color (not Transparent), so the corners will be clipped
            foreach (var c in corners)
            {
                ctx = ctx.Fill(Color.Red, c);
            }
            return ctx;
        }
        public static IImageProcessingContext RoundCorner(this IImageProcessingContext processingContext, Size size, float cornerRadius)
        {
            return processingContext.Resize(new ResizeOptions
            {
                Size = size,
                Mode = ResizeMode.Crop
            }).ApplyRoundedCorners(cornerRadius);
        }
        public static IPathCollection BuildCorners(int imageWidth, int imageHeight, float cornerRadius)
        {
            // first create a square
            var rect = new RectangularPolygon(-0.5f, -0.5f, cornerRadius, cornerRadius);

            // then cut out of the square a circle so we are left with a corner
            IPath cornerTopLeft = rect.Clip(new EllipsePolygon(cornerRadius - 0.5f, cornerRadius - 0.5f, cornerRadius));

            // corner is now a corner shape positions top left
            //lets make 3 more positioned correctly, we can do that by translating the original around the center of the image

            float rightPos = imageWidth - cornerTopLeft.Bounds.Width + 1;
            float bottomPos = imageHeight - cornerTopLeft.Bounds.Height + 1;

            // move it across the width of the image - the width of the shape
            IPath cornerTopRight = cornerTopLeft.RotateDegree(90).Translate(rightPos, 0);
            IPath cornerBottomLeft = cornerTopLeft.RotateDegree(-90).Translate(0, bottomPos);
            IPath cornerBottomRight = cornerTopLeft.RotateDegree(180).Translate(rightPos, bottomPos);

            return new PathCollection(cornerTopLeft, cornerBottomLeft, cornerTopRight, cornerBottomRight);
        }
        #endregion
    }
}
