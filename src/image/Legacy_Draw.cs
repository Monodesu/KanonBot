using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KanonBot.API;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Drawing;
using Img = SixLabors.ImageSharp.Image;
using SixLabors.Fonts;
using SixLabors.Fonts.WellKnownIds;
using SixLabors.ImageSharp.Processing.Processors;

namespace KanonBot.LegacyImage
{
    public static class Draw
    {
        public class UserPanelData
        {
            public Osu.UserInfo userInfo;
            public Osu.UserInfo prevUserInfo;
            public Osu.PPlusInfo pplusInfo;
            public string? customPanel;
            public int badgeId = -1;
        }
        public class ScorePanelData
        {
            public Osu.PPInfo ppInfo;
            public List<Osu.PPInfo.PPStat>? ppStats;
            public Osu.ScoreInfo scoreInfo;

        }
        public class PPVSPanelData
        {
            public string? u1Name;
            public string? u2Name;
            public Osu.PPlusInfo u1;
            public Osu.PPlusInfo u2;
        }

        //customBannerStatus 0=没有自定义banner 1=在猫猫上设置了自定义banner 
        public static MemoryStream DrawInfo(UserPanelData data, int customBannerStatus, bool isBonded = false, bool isDataOfDayAvaiavle = true, bool eventmode = false)
        {
            using (var info = new Image<Rgba32>(1200, 857))
            {
                var fonts = new FontCollection();
                var Exo2SemiBold = fonts.Add("./work/fonts/Exo2/Exo2-SemiBold.ttf");
                var Exo2Regular = fonts.Add("./work/fonts/Exo2/Exo2-Regular.ttf");
                var PuHuiTiRegular = fonts.Add("./work/fonts/Alibaba-PuHuiTi/Alibaba-PuHuiTi-Regular.ttf");
                // custom panel
                var panelPath = "./work/legacy/default-info-v1.png";
                if (File.Exists($"./work/legacy/v1_infopanel/{data.userInfo.userId}.png")) panelPath = $"./work/legacy/v1_infopanel/{data.userInfo.userId}.png";
                Img panel = Img.Load(panelPath);

                var coverPath = $"./work/legacy/v1_cover/{data.userInfo.userId}.png";
                if (customBannerStatus == 1)
                {
                    coverPath = $"./work/legacy/v1_cover/custom/{data.userInfo.userId}.png";
                }
                else if (!File.Exists(coverPath))
                {
                    if (data.userInfo.coverUrl != "")
                    {
                        Http.DownloadFile(data.userInfo.coverUrl, coverPath);
                    }
                    else
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
                var avatarPath = $"./work/avatar/{data.userInfo.userId}.png";
                if (!File.Exists(avatarPath))
                {
                    Http.DownloadFile(data.userInfo.avatarUrl, avatarPath);
                }
                Img avatar = Img.Load(avatarPath);
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

                Img flags = Img.Load($"./work/flags/{data.userInfo.country}.png");
                info.Mutate(x => x.DrawImage(flags, new Point(272, 212), 1));
                Img modeicon = Img.Load($"./work/legacy/mode_icon/{data.userInfo.mode}.png");
                modeicon.Mutate(x => x.Resize(64, 64));
                info.Mutate(x => x.DrawImage(modeicon, new Point(1125, 10), 1));

                // pp+
                if (data.userInfo.mode == "osu")
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
                    var ppd = new int[6];
                    ppd[0] = data.pplusInfo.acc;
                    ppd[1] = data.pplusInfo.flow;
                    ppd[2] = data.pplusInfo.jump;
                    ppd[3] = data.pplusInfo.pre;
                    ppd[4] = data.pplusInfo.spd;
                    ppd[5] = data.pplusInfo.sta;
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
                        pppto.Origin = new PointF(x_offset[i], (i % 3 != 0) ? (i < 3 ? 642 : 831) : 736);
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
                    HorizontalAlignment = HorizontalAlignment.Left,

                };
                textOptions.Origin = new PointF(15, 25);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, $"update:   {DateTime.Now}", new SolidBrush(Color.White), null));
                if (data.prevUserInfo.daysBefore > 1)
                {
                    textOptions = new TextOptions(new Font(PuHuiTiRegular, 20))
                    {
                        VerticalAlignment = VerticalAlignment.Bottom,
                        HorizontalAlignment = HorizontalAlignment.Left,
                    };
                    if (isDataOfDayAvaiavle)
                    {
                        textOptions.Origin = new PointF(300, 25);
                        info.Mutate(x => x.DrawText(drawOptions, textOptions, $"对 比 自 {data.prevUserInfo.daysBefore}天 前", new SolidBrush(Color.White), null));
                    }
                    else
                    {
                        textOptions.Origin = new PointF(300, 25);
                        info.Mutate(x => x.DrawText(drawOptions, textOptions, $" 请 求 的 日 期 没 有 数 据 .." +
                            $"当 前 数 据 对 比 自 {data.prevUserInfo.daysBefore}天 前", new SolidBrush(Color.White), null));
                    }
                }
                // username
                textOptions.Font = new Font(Exo2SemiBold, 60);
                textOptions.Origin = new PointF(268, 140);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, data.userInfo.userName, new SolidBrush(Color.White), null));
                // country_rank
                string countryRank;
                if (isBonded)
                {
                    var diff = data.userInfo.countryRank - data.prevUserInfo.countryRank;
                    if (diff > 0) countryRank = string.Format("#{0:N0}(-{1:N0})", data.userInfo.countryRank, diff);
                    else if (diff < 0) countryRank = string.Format("#{0:N0}(+{1:N0})", data.userInfo.countryRank, Math.Abs(diff));
                    else countryRank = string.Format("#{0:N0}", data.userInfo.countryRank);
                }
                else
                {
                    countryRank = string.Format("#{0:N0}", data.userInfo.countryRank);
                }
                textOptions.Font = new Font(Exo2SemiBold, 20);
                textOptions.Origin = new PointF(350, 260);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, countryRank, new SolidBrush(Color.White), null));
                // global_rank
                string diffStr;
                if (isBonded)
                {
                    var diff = data.userInfo.globalRank - data.prevUserInfo.globalRank;
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
                info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:N0}", data.userInfo.globalRank), new SolidBrush(Color.White), null));
                textOptions.Font = new Font(PuHuiTiRegular, 14);
                textOptions.Origin = new PointF(40, 430);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, diffStr, new SolidBrush(Color.White), null));
                // pp
                if (isBonded)
                {
                    var diff = data.userInfo.pp - data.prevUserInfo.pp;
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
                info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:0.##}", data.userInfo.pp), new SolidBrush(Color.White), null));
                textOptions.Font = new Font(PuHuiTiRegular, 14);
                textOptions.Origin = new PointF(246, 430);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, diffStr, new SolidBrush(Color.White), null));
                // ssh ss
                textOptions.Font = new Font(Exo2Regular, 30);
                textOptions.HorizontalAlignment = HorizontalAlignment.Center;
                textOptions.Origin = new PointF(246, 410);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, data.userInfo.SSH.ToString(), new SolidBrush(Color.White), null));
                textOptions.Origin = new PointF(191, 540);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, data.userInfo.SS.ToString(), new SolidBrush(Color.White), null));
                textOptions.Origin = new PointF(301, 540);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, data.userInfo.SH.ToString(), new SolidBrush(Color.White), null));
                textOptions.Origin = new PointF(412, 540);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, data.userInfo.S.ToString(), new SolidBrush(Color.White), null));
                textOptions.Origin = new PointF(522, 540);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, data.userInfo.A.ToString(), new SolidBrush(Color.White), null));
                // level
                textOptions.Font = new Font(Exo2SemiBold, 34);
                textOptions.Origin = new PointF(1115, 385);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, data.userInfo.level.ToString(), new SolidBrush(Color.White), null));
                // Level%
                var levelper = data.userInfo.levelProgress;
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
                rankedScore = string.Format("{0:N0}", data.userInfo.rankedScore);
                textOptions.Origin = new PointF(1180, 625);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, rankedScore, new SolidBrush(Color.White), null));
                string acc;
                if (isBonded)
                {
                    var diff = data.userInfo.accuracy - data.prevUserInfo.accuracy;
                    if (diff >= 0.01) acc = string.Format("{0:0.##}%(+{1:0.##}%)", data.userInfo.accuracy, diff);
                    else if (diff <= -0.01) acc = string.Format("{0:0.##}%({1:0.##}%)", data.userInfo.accuracy, diff);
                    else acc = string.Format("{0:0.##}%", data.userInfo.accuracy);
                }
                else
                {
                    acc = string.Format("{0:0.##}%", data.userInfo.accuracy);
                }
                textOptions.Origin = new PointF(1180, 665);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, acc, new SolidBrush(Color.White), null));
                string playCount;
                if (isBonded)
                {
                    var diff = data.userInfo.playCount - data.prevUserInfo.playCount;
                    if (diff > 0) playCount = string.Format("{0:N0}(+{1:N0})", data.userInfo.playCount, diff);
                    else if (diff < 0) playCount = string.Format("{0:N0}({1:N0})", data.userInfo.playCount, diff);
                    else playCount = string.Format("{0:N0}", data.userInfo.playCount);
                }
                else
                {
                    playCount = string.Format("{0:N0}", data.userInfo.playCount);
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
                totalScore = string.Format("{0:N0}", data.userInfo.totalScore);
                textOptions.Origin = new PointF(1180, 745);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, totalScore, new SolidBrush(Color.White), null));
                string totalHits;
                if (isBonded)
                {
                    var diff = data.userInfo.totalHits - data.prevUserInfo.totalHits;
                    if (diff > 0) totalHits = string.Format("{0:N0}(+{1:N0})", data.userInfo.totalHits, diff);
                    else if (diff < 0) totalHits = string.Format("{0:N0}({1:N0})", data.userInfo.totalHits, diff);
                    else totalHits = string.Format("{0:N0}", data.userInfo.totalHits);
                }
                else
                {
                    totalHits = string.Format("{0:N0}", data.userInfo.totalHits);
                }
                textOptions.Origin = new PointF(1180, 785);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, totalHits, new SolidBrush(Color.White), null));
                textOptions.Origin = new PointF(1180, 825);
                info.Mutate(x => x.DrawText(drawOptions, textOptions, Utils.Duration2String(data.userInfo.playTime), new SolidBrush(Color.White), null));
                info.Mutate(x => x.RoundCorner(new Size(1200, 857), 20));
                var stream = new MemoryStream();
                //info.SaveAsJpeg(stream);
                info.SaveAsPng(stream);
                return stream;
            }
        }
        /*
        public static MemoryStream DrawScore(ScorePanelData data, bool res)
        {
            using (var score = Img.Load("work/v2_scorepanel/default-score-v2.png"))
            {
                var fonts = new FontCollection();
                var TorusRegular = fonts.Add("./work/fonts/Torus-Regular.ttf");
                var TorusSemiBold = fonts.Add("./work/fonts/Torus-SemiBold.ttf");
                var PuHuiTiRegular = fonts.Add("./work/fonts/Alibaba-PuHuiTi/Alibaba-PuHuiTi-Regular.ttf");

                Img panel;
                if (data.scoreInfo.mode == "fruits") panel = Img.Load("work/v2_scorepanel/default-score-v2-fruits.png");
                else if (data.scoreInfo.mode == "mania") panel = Img.Load("work/v2_scorepanel/default-score-v2-mania.png");
                else panel = Img.Load("work/v2_scorepanel/default-score-v2.png");
                // bg
                var bgPath = $"work/background/{data.scoreInfo.beatmapInfo.beatmapId}.png";
                var hasBG = false;
                if (!File.Exists(bgPath))
                {
                    try
                    {
                        //hasBG = Kanon.DownloadBeatmapBackgroundImg(data.scoreInfo.beatmapInfo.beatmapId, bgPath);
                        hasBG = false; //暂时停止从kanon API获取背景图片
                        if (!hasBG)
                        {
                            //var msg = $"无法从KanonAPI获取背景图片，尝试从SayoAPI下载";
                            //Logger.Warning(msg);
                            try
                            {
                                hasBG = OSU.SayoDownloadBeatmapBackgroundImg(data.scoreInfo.beatmapInfo.beatmapsetId, data.scoreInfo.beatmapInfo.beatmapId, bgPath);
                            }
                            catch (Exception ex1)
                            {
                                var msg = $"从小夜API下载背景图片时发生了一处异常\n异常类型: {ex1.GetType()}\n异常信息: '{ex1.Message}'";
                                Logger.Warning(msg);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        var msg = $"从KanonAPI下载背景图片时发生了一处异常\n异常类型: {ex.GetType()}\n异常信息: '{ex.Message}'";
                        Logger.Warning(msg);
                        try
                        {
                            hasBG = OSU.SayoDownloadBeatmapBackgroundImg(data.scoreInfo.beatmapInfo.beatmapsetId, data.scoreInfo.beatmapInfo.beatmapId, bgPath);
                        }
                        catch (Exception ex1)
                        {
                            msg = $"从小夜API下载背景图片时发生了一处异常\n异常类型: {ex1.GetType()}\n异常信息: '{ex1.Message}'";
                            Logger.Warning(msg);
                        }
                    }
                }
                else { hasBG = true; }
                Img bg;
                try { if (hasBG) bg = Img.Load(bgPath); else bg = Img.Load("./work/load-failed-img.png"); }
                catch { bg = Img.Load("./work/load-failed-img.png"); }
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
                var mode = data.scoreInfo.mode;
                switch (mode)
                {
                    case "osu":
                        ringFile[0] = "std-easy.png";
                        ringFile[1] = "std-normal.png";
                        ringFile[2] = "std-hard.png";
                        ringFile[3] = "std-insane.png";
                        ringFile[4] = "std-expert.png";
                        ringFile[5] = "std-expertplus.png";
                        break;
                    case "fruits":
                        ringFile[0] = "ctb-easy.png";
                        ringFile[1] = "ctb-normal.png";
                        ringFile[2] = "ctb-hard.png";
                        ringFile[3] = "ctb-insane.png";
                        ringFile[4] = "ctb-expert.png";
                        ringFile[5] = "ctb-expertplus.png";
                        break;
                    case "taiko":
                        ringFile[0] = "taiko-easy.png";
                        ringFile[1] = "taiko-normal.png";
                        ringFile[2] = "taiko-hard.png";
                        ringFile[3] = "taiko-insane.png";
                        ringFile[4] = "taiko-expert.png";
                        ringFile[5] = "taiko-expertplus.png";
                        break;
                    case "mania":
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
                if (data.scoreInfo.beatmapInfo.beatmapStatus == "ranked")
                    score.Mutate(x => x.DrawImage(Img.Load("./work/icons/ranked.png"), new Point(415, 16), 1));
                if (data.scoreInfo.beatmapInfo.beatmapStatus == "approved")
                    score.Mutate(x => x.DrawImage(Img.Load("./work/icons/approved.png"), new Point(415, 16), 1));
                if (data.scoreInfo.beatmapInfo.beatmapStatus == "loved")
                    score.Mutate(x => x.DrawImage(Img.Load("./work/icons/loved.png"), new Point(415, 16), 1));
                // mods
                var mods = data.scoreInfo.mods;
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
                var ranking = data.scoreInfo.rank;
                var rankPic = Img.Load($"./work/ranking/ranking-{ranking}.png");
                score.Mutate(x => x.DrawImage(rankPic, new Point(913, 874), 1));
                // text part (文字部分)
                var drawOptions = new DrawingOptions
                {
                    TextOptions = new TextOptions
                    {
                        VerticalAlignment = VerticalAlignment.Bottom,
                        HorizontalAlignment = HorizontalAlignment.Left,
                    },
                    GraphicsOptions = new GraphicsOptions
                    {
                        Antialias = true
                    }
                };
                // beatmap_info
                var font = new Font(TorusRegular, 60);
                var title = "";
                foreach (char c in data.scoreInfo.beatmapInfo.title)
                {
                    title += c;
                    var m = TextMeasurer.Measure(title, new RendererOptions(font));
                    if (m.Width > 725)
                    {
                        title += "...";
                        break;
                    }
                }
                score.Mutate(x => x.DrawText(drawOptions, title, font, Color.Black, new PointF(499, 110)));
                score.Mutate(x => x.DrawText(drawOptions, title, font, Color.White, new PointF(499, 105)));
                // artist
                font = new Font(TorusRegular, 40);
                var artist = "";
                foreach (char c in data.scoreInfo.beatmapInfo.artist)
                {
                    artist += c;
                    var m = TextMeasurer.Measure(artist, new RendererOptions(font));
                    if (m.Width > 205)
                    {
                        artist += "...";
                        break;
                    }
                }
                score.Mutate(x => x.DrawText(drawOptions, artist, font, Color.Black, new PointF(519, 178)));
                score.Mutate(x => x.DrawText(drawOptions, artist, font, Color.White, new PointF(519, 175)));
                // creator
                var creator = "";
                foreach (char c in data.scoreInfo.beatmapInfo.creator)
                {
                    creator += c;
                    var m = TextMeasurer.Measure(creator, new RendererOptions(font));
                    if (m.Width > 145)
                    {
                        creator += "...";
                        break;
                    }
                }
                score.Mutate(x => x.DrawText(drawOptions, creator, font, Color.Black, new PointF(795, 178)));
                score.Mutate(x => x.DrawText(drawOptions, creator, font, Color.White, new PointF(795, 175)));
                // beatmap_id
                var beatmap_id = data.scoreInfo.beatmapId.ToString();
                score.Mutate(x => x.DrawText(drawOptions, beatmap_id, font, Color.Black, new PointF(1008, 178)));
                score.Mutate(x => x.DrawText(drawOptions, beatmap_id, font, Color.White, new PointF(1008, 175)));
                // ar,od info
                var color = Color.ParseHex("#f1ce59");
                font = new Font(TorusRegular, 24.25f);
                // time
                var song_time = Utils.Duration2TimeString(data.scoreInfo.beatmapInfo.totalLength);
                score.Mutate(x => x.DrawText(drawOptions, song_time, font, Color.Black, new PointF(1741, 127)));
                score.Mutate(x => x.DrawText(drawOptions, song_time, font, color, new PointF(1741, 124)));
                // bpm
                var bpm = data.scoreInfo.beatmapInfo.BPM.ToString("0");
                score.Mutate(x => x.DrawText(drawOptions, bpm, font, Color.Black, new PointF(1457, 127)));
                score.Mutate(x => x.DrawText(drawOptions, bpm, font, color, new PointF(1457, 124)));
                // ar
                var ar = data.scoreInfo.beatmapInfo.approachRate.ToString("0.0#");
                if (data.scoreInfo.mode == "osu" && data.ppInfo.approachRate != -1) ar = data.ppInfo.approachRate.ToString("0.0#");
                score.Mutate(x => x.DrawText(drawOptions, ar, font, Color.Black, new PointF(1457, 218)));
                score.Mutate(x => x.DrawText(drawOptions, ar, font, color, new PointF(1457, 215)));
                // od
                var od = data.scoreInfo.beatmapInfo.accuracy.ToString("0.0#");
                if (data.scoreInfo.mode == "osu" && data.ppInfo.accuracy != -1) od = data.ppInfo.accuracy.ToString("0.0#");
                score.Mutate(x => x.DrawText(drawOptions, od, font, Color.Black, new PointF(1741, 218)));
                score.Mutate(x => x.DrawText(drawOptions, od, font, color, new PointF(1741, 215)));
                // cs
                var cs = data.ppInfo.circleSize.ToString("0.0#");
                if (data.ppInfo.circleSize == -1) cs = data.scoreInfo.beatmapInfo.circleSize.ToString("0.0#");
                score.Mutate(x => x.DrawText(drawOptions, cs, font, Color.Black, new PointF(1457, 312)));
                score.Mutate(x => x.DrawText(drawOptions, cs, font, color, new PointF(1457, 309)));
                // hp
                var hp = data.ppInfo.HPDrainRate.ToString("0.0#");
                if (data.ppInfo.HPDrainRate == -1) cs = data.scoreInfo.beatmapInfo.HPDrainRate.ToString("0.0#");
                score.Mutate(x => x.DrawText(drawOptions, hp, font, Color.Black, new PointF(1741, 312)));
                score.Mutate(x => x.DrawText(drawOptions, hp, font, color, new PointF(1741, 309)));
                // stars, version
                var starText = $"Stars: {star.ToString("0.##")}";
                score.Mutate(x => x.DrawText(drawOptions, starText, font, Color.Black, new PointF(584, 292)));
                score.Mutate(x => x.DrawText(drawOptions, starText, font, color, new PointF(584, 289)));
                var version = "";
                foreach (char c in data.scoreInfo.beatmapInfo.version)
                {
                    version += c;
                    var m = TextMeasurer.Measure(version, new RendererOptions(font));
                    if (m.Width > 140)
                    {
                        version += "...";
                        break;
                    }
                }
                score.Mutate(x => x.DrawText(drawOptions, version, font, Color.Black, new PointF(584, 320)));
                score.Mutate(x => x.DrawText(drawOptions, version, font, Color.White, new PointF(584, 317)));
                // avatar
                var avatarPath = $"./work/avatar/{data.scoreInfo.userId}.png";
                if (!File.Exists(avatarPath))
                {
                    Http.DownloadFile(data.scoreInfo.userAvatarUrl, avatarPath);
                }
                Img avatar = Img.Load(avatarPath);
                avatar.Mutate(x => x.Resize(80, 80).RoundCorner(new Size(80, 80), 40));
                score.Mutate(
                    x => x.Fill(
                        Color.White,
                        new EllipsePolygon(80, 465, 85, 85)
                    )
                );
                score.Mutate(x => x.DrawImage(avatar, new Point(40, 425), 1));
                // username
                font = new Font(TorusSemiBold, 36);
                var username = data.scoreInfo.userName;
                score.Mutate(x => x.DrawText(drawOptions, username, font, Color.Black, new PointF(145, 470)));
                score.Mutate(x => x.DrawText(drawOptions, username, font, Color.White, new PointF(145, 467)));
                // time
                font = new Font(TorusRegular, 27.61f);
                var time = data.scoreInfo.achievedTime.ToString("yyyy/MM/dd HH:mm:ss");
                score.Mutate(x => x.DrawText(drawOptions, time, font, Color.Black, new PointF(145, 505)));
                score.Mutate(x => x.DrawText(drawOptions, time, font, Color.White, new PointF(145, 502)));

                // pp
                var ppTColor = Color.ParseHex("#cf93ae");
                var ppColor = Color.ParseHex("#fc65a9");
                font = new Font(TorusRegular, 33.5f);
                // aim, speed
                string pptext;
                if (data.ppInfo.ppStat.aim == -1) pptext = "-";
                else pptext = data.ppInfo.ppStat.aim.ToString("0");
                var metric = TextMeasurer.Measure(pptext, new RendererOptions(font));
                score.Mutate(x => x.DrawText(drawOptions, pptext, font, ppColor, new PointF(1532, 638)));
                score.Mutate(x => x.DrawText(drawOptions, "pp", font, ppTColor, new PointF(1532 + metric.Width, 638)));
                if (data.ppInfo.ppStat.speed == -1) pptext = "-";
                else pptext = data.ppInfo.ppStat.speed.ToString("0");
                metric = TextMeasurer.Measure(pptext, new RendererOptions(font));
                score.Mutate(x => x.DrawText(drawOptions, pptext, font, ppColor, new PointF(1672, 638)));
                score.Mutate(x => x.DrawText(drawOptions, "pp", font, ppTColor, new PointF(1672 + metric.Width, 638)));
                if (data.ppInfo.ppStat.acc == -1) pptext = "-";
                else pptext = data.ppInfo.ppStat.acc.ToString("0");
                metric = TextMeasurer.Measure(pptext, new RendererOptions(font));
                score.Mutate(x => x.DrawText(drawOptions, pptext, font, ppColor, new PointF(1812, 638)));
                score.Mutate(x => x.DrawText(drawOptions, "pp", font, ppTColor, new PointF(1812 + metric.Width, 638)));

                if (data.scoreInfo.mode == "mania" || data.scoreInfo.mode == "fruits")
                {
                    pptext = "-";
                    metric = TextMeasurer.Measure(pptext, new RendererOptions(font));
                    for (var i = 0; i < 5; i++)
                    {
                        score.Mutate(x => x.DrawText(drawOptions, pptext, font, ppColor, new PointF(50 + 139 * i, 638)));
                        score.Mutate(x => x.DrawText(drawOptions, "pp", font, ppTColor, new PointF(50 + 139 * i + metric.Width, 638)));
                    }
                }
                else
                {
                    for (var i = 0; i < 5; i++)
                    {
                        try
                        {
                            if (data.ppStats[i].total == -1) pptext = "-";
                            else pptext = data.ppStats[5 - (i + 1)].total.ToString("0");
                        }
                        catch
                        {
                            pptext = "-";
                        }
                        metric = TextMeasurer.Measure(pptext, new RendererOptions(font));
                        score.Mutate(x => x.DrawText(drawOptions, pptext, font, ppColor, new PointF(50 + 139 * i, 638)));
                        score.Mutate(x => x.DrawText(drawOptions, "pp", font, ppTColor, new PointF(50 + 139 * i + metric.Width, 638)));
                    }
                }
                // if fc
                font = new Font(TorusRegular, 24.5f);
                if (data.scoreInfo.mode == "mania")
                {
                    pptext = "-";
                }
                else
                {
                    try
                    {
                        if (data.ppStats[5].total == -1) pptext = "-";
                        else pptext = data.ppStats[5].total.ToString("0");
                    }
                    catch
                    {
                        pptext = "-";
                    }
                }
                metric = TextMeasurer.Measure(pptext, new RendererOptions(font));
                score.Mutate(x => x.DrawText(drawOptions, pptext, font, ppColor, new PointF(99, 562)));
                score.Mutate(x => x.DrawText(drawOptions, "pp", font, ppTColor, new PointF(99 + metric.Width, 562)));
                // total pp
                font = new Font(TorusRegular, 61f);
                if (data.ppInfo.ppStat.total == -1) pptext = "-";
                else pptext = Math.Round(data.ppInfo.ppStat.total).ToString("0");
                drawOptions.TextOptions.HorizontalAlignment = HorizontalAlignment.Right;
                score.Mutate(x => x.DrawText(drawOptions, pptext, font, ppColor, new PointF(1825, 500)));
                score.Mutate(x => x.DrawText(drawOptions, "pp", font, ppTColor, new PointF(1899, 500)));

                // score
                drawOptions.TextOptions.HorizontalAlignment = HorizontalAlignment.Center;
                score.Mutate(x => x.DrawText(drawOptions, data.scoreInfo.score.ToString("N0"), new Font(TorusRegular, 40), Color.White, new PointF(980, 750)));
                if (data.scoreInfo.mode == "fruits")
                {
                    font = new Font(TorusRegular, 40.00f);
                    var great = data.scoreInfo.great.ToString();
                    var ok = data.scoreInfo.ok.ToString();
                    var meh = data.scoreInfo.meh.ToString();
                    var miss = data.scoreInfo.miss.ToString();

                    // great
                    score.Mutate(x => x.DrawText(drawOptions, great, font, Color.Black, new PointF(790, 852)));
                    score.Mutate(x => x.DrawText(drawOptions, great, font, Color.White, new PointF(790, 849)));
                    // ok
                    score.Mutate(x => x.DrawText(drawOptions, ok, font, Color.Black, new PointF(790, 975)));
                    score.Mutate(x => x.DrawText(drawOptions, ok, font, Color.White, new PointF(790, 972)));
                    // meh
                    score.Mutate(x => x.DrawText(drawOptions, meh, font, Color.Black, new PointF(1152, 852)));
                    score.Mutate(x => x.DrawText(drawOptions, meh, font, Color.White, new PointF(1152, 849)));
                    // miss
                    score.Mutate(x => x.DrawText(drawOptions, miss, font, Color.Black, new PointF(1152, 975)));
                    score.Mutate(x => x.DrawText(drawOptions, miss, font, Color.White, new PointF(1152, 972)));
                }
                else if (data.scoreInfo.mode == "mania")
                {
                    font = new Font(TorusRegular, 35.00f);
                    var great = data.scoreInfo.great.ToString();
                    var ok = data.scoreInfo.ok.ToString();
                    var meh = data.scoreInfo.meh.ToString();
                    var miss = data.scoreInfo.miss.ToString();
                    var geki = data.scoreInfo.geki.ToString();
                    var katu = data.scoreInfo.katu.ToString();

                    // great
                    score.Mutate(x => x.DrawText(drawOptions, great, font, Color.Black, new PointF(790, 837)));
                    score.Mutate(x => x.DrawText(drawOptions, great, font, Color.White, new PointF(790, 834)));
                    // geki
                    score.Mutate(x => x.DrawText(drawOptions, geki, font, Color.Black, new PointF(1156, 837)));
                    score.Mutate(x => x.DrawText(drawOptions, geki, font, Color.White, new PointF(1156, 834)));
                    // katu
                    score.Mutate(x => x.DrawText(drawOptions, katu, font, Color.Black, new PointF(790, 910)));
                    score.Mutate(x => x.DrawText(drawOptions, katu, font, Color.White, new PointF(790, 907)));
                    // ok
                    score.Mutate(x => x.DrawText(drawOptions, ok, font, Color.Black, new PointF(1156, 910)));
                    score.Mutate(x => x.DrawText(drawOptions, ok, font, Color.White, new PointF(1156, 907)));
                    // meh
                    score.Mutate(x => x.DrawText(drawOptions, meh, font, Color.Black, new PointF(790, 985)));
                    score.Mutate(x => x.DrawText(drawOptions, meh, font, Color.White, new PointF(790, 982)));
                    // miss
                    score.Mutate(x => x.DrawText(drawOptions, miss, font, Color.Black, new PointF(1156, 985)));
                    score.Mutate(x => x.DrawText(drawOptions, miss, font, Color.White, new PointF(1156, 982)));
                }
                else
                {
                    font = new Font(TorusRegular, 53.09f);
                    var great = data.scoreInfo.great.ToString();
                    var ok = data.scoreInfo.ok.ToString();
                    var meh = data.scoreInfo.meh.ToString();
                    var miss = data.scoreInfo.miss.ToString();

                    // great
                    score.Mutate(x => x.DrawText(drawOptions, great, font, Color.Black, new PointF(795, 857)));
                    score.Mutate(x => x.DrawText(drawOptions, great, font, Color.White, new PointF(795, 854)));
                    // ok
                    score.Mutate(x => x.DrawText(drawOptions, ok, font, Color.Black, new PointF(795, 985)));
                    score.Mutate(x => x.DrawText(drawOptions, ok, font, Color.White, new PointF(795, 982)));
                    // meh
                    score.Mutate(x => x.DrawText(drawOptions, meh, font, Color.Black, new PointF(1154, 857)));
                    score.Mutate(x => x.DrawText(drawOptions, meh, font, Color.White, new PointF(1154, 854)));
                    // miss
                    score.Mutate(x => x.DrawText(drawOptions, miss, font, Color.Black, new PointF(1154, 985)));
                    score.Mutate(x => x.DrawText(drawOptions, miss, font, Color.White, new PointF(1154, 982)));
                }

                // acc
                font = new Font(TorusRegular, 53.56f);
                var acc = data.scoreInfo.acc * 100f;
                var hsl = new Hsl(150, 1, 1);
                // ("#ffbd1f") idk?
                color = Color.ParseHex("#87ff6a");
                score.Mutate(x => x.DrawText(drawOptions, $"{acc.ToString("0.0#")}%", font, Color.Black, new PointF(360, 966)));
                var acchue = new Image<Rgba32>(1950 - 2, 1088);
                var hue = acc < 60 ? 260f : (acc - 60) * 2 + 280f;
                acchue.Mutate(x => x.DrawText(drawOptions, $"{acc.ToString("0.0#")}%", font, color, new PointF(360, 963)).Hue(hue));
                score.Mutate(x => x.DrawImage(acchue, 1));
                // combo
                var combo = data.scoreInfo.combo;
                if (data.scoreInfo.mode != "mania")
                {
                    var maxCombo = data.ppInfo.maxCombo;
                    if (maxCombo != -1)
                    {
                        score.Mutate(x => x.DrawText(drawOptions, "/", font, Color.Black, new PointF(1598, 966)));
                        score.Mutate(x => x.DrawText(drawOptions, "/", font, Color.White, new PointF(1598, 963)));
                        drawOptions.TextOptions.HorizontalAlignment = HorizontalAlignment.Left;
                        score.Mutate(x => x.DrawText(drawOptions, $"{maxCombo}x", font, Color.Black, new PointF(1607, 966)));
                        score.Mutate(x => x.DrawText(drawOptions, $"{maxCombo}x", font, color, new PointF(1607, 963)));
                        drawOptions.TextOptions.HorizontalAlignment = HorizontalAlignment.Right;
                        score.Mutate(x => x.DrawText(drawOptions, $"{combo}x", font, Color.Black, new PointF(1588, 966)));
                        var combohue = new Image<Rgba32>(1950 - 2, 1088);
                        hue = (((float)combo / (float)maxCombo) * 100) + 260;
                        combohue.Mutate(x => x.DrawText(drawOptions, $"{combo}x", font, color, new PointF(1588, 963)).Hue(hue));
                        score.Mutate(x => x.DrawImage(combohue, 1));
                    }
                    else
                    {
                        drawOptions.TextOptions.HorizontalAlignment = HorizontalAlignment.Center;
                        score.Mutate(x => x.DrawText(drawOptions, $"{combo}x", font, Color.Black, new PointF(1598, 966)));
                        score.Mutate(x => x.DrawText(drawOptions, $"{combo}x", font, color, new PointF(1598, 963)));
                    }
                }
                else
                {
                    drawOptions.TextOptions.HorizontalAlignment = HorizontalAlignment.Center;
                    score.Mutate(x => x.DrawText(drawOptions, $"{combo}x", font, Color.Black, new PointF(1598, 966)));
                    score.Mutate(x => x.DrawText(drawOptions, $"{combo}x", font, color, new PointF(1598, 963)));
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
        }*/
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
                u1d[0] = data.u1.acc;
                u1d[1] = data.u1.flow;
                u1d[2] = data.u1.jump;
                u1d[3] = data.u1.pre;
                u1d[4] = data.u1.spd;
                u1d[5] = data.u1.sta;
                var u2d = new int[6];
                u2d[0] = data.u2.acc;
                u2d[1] = data.u2.flow;
                u2d[2] = data.u2.jump;
                u2d[3] = data.u2.pre;
                u2d[4] = data.u2.spd;
                u2d[5] = data.u2.sta;
                // acc ,flow, jump, pre, speed, sta

                if (data.u1.pp < data.u2.pp)
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
                ppvsImg.Mutate(x => x.DrawText(drawOptions, textOptions, data.u1.pp.ToString(), new SolidBrush(color), null));
                for (var i = 0; i < u2d.Length; i++)
                {
                    textOptions.Origin = new PointF(424, y_offset[i] + 8);
                    ppvsImg.Mutate(x => x.DrawText(drawOptions, textOptions, u2d[i].ToString(), new SolidBrush(color), null));
                }
                textOptions.Origin = new PointF(424, 974);
                ppvsImg.Mutate(x => x.DrawText(drawOptions, textOptions, data.u2.pp.ToString(), new SolidBrush(color), null));

                // 打印数据差异
                var diffPoint = 960;
                color = Color.ParseHex("#ffcd22");
                textOptions.Origin = new PointF(diffPoint, 974);
                ppvsImg.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:0}", (data.u2.pp - data.u1.pp)), new SolidBrush(color), null));
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
            var HarmonySansSC = fonts.Add("./work/fonts/HarmonyOS_Sans_SC/HarmonyOS_Sans_SC_Regular.ttf");
            var font = new Font(HarmonySansSC, fontSize);
            var textOptions = new TextOptions(new Font(HarmonySansSC, fontSize))
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
    }
}
