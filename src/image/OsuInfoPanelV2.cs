using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using KanonBot.functions.osu.rosupp;
using KanonBot.LegacyImage;
using static KanonBot.LegacyImage.Draw;
using KanonBot.Image;
using SqlSugar;
using System.IO;
using SixLabors.ImageSharp.Formats.Png;
using ResizeOptions = SixLabors.ImageSharp.Processing.ResizeOptions;

namespace KanonBot.image
{
    public static class OsuInfoPanelV2
    {
        public static async Task<Img> Draw(UserPanelData data, bool isBonded = false, bool isDataOfDayAvaiavle = true, bool eventmode = false)
        {
            var ColorMode = data.InfoPanelV2_Mode;
            Color mainColor = new(),
                ppMainColor = new(),
                accMainColor = new(),
                rankMainColor = new(),
                gradeCountsColor = new(),
                usernameColor = new(),
                detailsColor = new(),
                ppBackgroundColor = new(),
                ppFrontColor = new(),
                accBackgroundColor = new(),
                accFrontColor = new(),
                ppSubColor = new(),
                accSubColor = new(),
                scoreppTopColor = new(),
                scoreppColor = new(),
                highlightColor = new(),
                levelprogressBackgroundColor = new(),
                levelprogressFrontColor = new();
            //配色方案 0=模板light 1=模板dark 2...3...4...
            switch (ColorMode)
            {
                case 0:
                    mainColor = Color.ParseHex("#656b6d");
                    usernameColor = Color.ParseHex("#4d4d4d");
                    ppMainColor = Color.ParseHex("#e36a79");
                    accMainColor = Color.ParseHex("#6cac9c");
                    rankMainColor = Color.ParseHex("#5872df");
                    gradeCountsColor = Color.ParseHex("#3a4d78");
                    detailsColor = Color.ParseHex("#7f7f7f");
                    ppBackgroundColor = Color.ParseHex("#fddcd7");
                    ppFrontColor = Color.ParseHex("#f7bebe");
                    accBackgroundColor = Color.ParseHex("#c3e7cb");
                    accFrontColor = Color.ParseHex("#a4d8b1");
                    ppSubColor = Color.ParseHex("#d84356");
                    accSubColor = Color.ParseHex("#006837");
                    scoreppTopColor = Color.ParseHex("#364a75");
                    scoreppColor = Color.ParseHex("#ff7bac");
                    highlightColor = Color.ParseHex("#ffcd22");
                    levelprogressBackgroundColor = Color.ParseHex("#E6E6E6");
                    levelprogressFrontColor = Color.ParseHex("#F3B6CD");
                    break;
                case 1:
                    mainColor = Color.ParseHex("#e6e6e6");
                    usernameColor = Color.ParseHex("#e6e6e6");
                    ppMainColor = Color.ParseHex("#e36a79");
                    accMainColor = Color.ParseHex("#6cac9c");
                    rankMainColor = Color.ParseHex("#5872DF");
                    gradeCountsColor = Color.ParseHex("#6C91E0");
                    detailsColor = Color.ParseHex("#e6e6e6");
                    ppBackgroundColor = Color.ParseHex("#44312F");
                    ppFrontColor = Color.ParseHex("#5D3B3A");
                    accBackgroundColor = Color.ParseHex("#243829");
                    accFrontColor = Color.ParseHex("#294B32");
                    ppSubColor = Color.ParseHex("#FF7BAC");
                    accSubColor = Color.ParseHex("#00DE75");
                    scoreppTopColor = Color.ParseHex("#5979bd");
                    scoreppColor = Color.ParseHex("#E36A79");
                    highlightColor = Color.ParseHex("#ffcd22");
                    levelprogressBackgroundColor = Color.ParseHex("#000000");
                    levelprogressFrontColor = Color.ParseHex("#85485F");
                    break;
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
                0 => "./work/panelv2/infov2-light-customimg.png",
                1 => "./work/panelv2/infov2-dark-customimg.png",
                _ => throw new Exception(),
            };
            sidePic = Img.Load(await Utils.LoadFile2Byte(sidePicPath)).CloneAs<Rgba32>();    // 读取
            switch (ColorMode)
            {
                case 0:
                    //light
                    //do nothing  
                    break;
                case 1:
                    //dark
                    sidePic.Mutate(x => x.Brightness(0.6f));
                    break;
            }
            info.Mutate(x => x.DrawImage(sidePic, new Point(73, 41), 1));


            //进度条 - 先绘制进度条，再覆盖面板
            //pp
            Img pp_background = new Image<Rgba32>(1443, 68);
            pp_background.Mutate(x => x.Fill(ppBackgroundColor));
            pp_background.Mutate(x => x.RoundCorner_Parts(new Size(1443, 68), 10, 10, 20, 20));
            info.Mutate(x => x.DrawImage(pp_background, new Point(2358, 410), 1));
            //获取bnspp
            double bounsPP = 0.00;
            double scorePP = 0.00;
            #region bnspp
            if (allBP == null) { scorePP = data.userInfo.Statistics.PP; }
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
            Img pp_front = new Image<Rgba32>(pp_front_length, 68);
            pp_front.Mutate(x => x.Fill(ppFrontColor));
            pp_front.Mutate(x => x.RoundCorner_Parts(new Size(pp_front_length, 68), 10, 10, 20, 20));
            info.Mutate(x => x.DrawImage(pp_front, new Point(2358, 410), 1));

            //50&100
            Img acc_background = new Image<Rgba32>(1443, 68);
            acc_background.Mutate(x => x.Fill(accBackgroundColor));
            acc_background.Mutate(x => x.RoundCorner_Parts(new Size(1443, 68), 10, 10, 20, 20));
            info.Mutate(x => x.DrawImage(acc_background, new Point(2358, 611), 1));

            //300
            Img acc_300 = new Image<Rgba32>((int)(1443.00 * (data.userInfo.Statistics.HitAccuracy / 100.0)), 68);
            acc_300.Mutate(x => x.Fill(accFrontColor));
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
            bp1bg.Mutate(x => x.Resize(new ResizeOptions() { Size = new Size(355, 200), Mode = ResizeMode.Min }));
            switch (ColorMode)
            {
                case 0:
                    //light
                    //do nothing  
                    break;
                case 1:
                    //dark
                    bp1bg.Mutate(x => x.Brightness(0.6f));
                    break;
            }
            info.Mutate(x => x.DrawImage(bp1bg, new Point(1566, 1550), 1));

            //level progress
            Img levelprogress_background = new Image<Rgba32>(2312, 12);
            levelprogress_background.Mutate(x => x.Fill(levelprogressBackgroundColor));
            //levelprogress_background.Mutate(x => x.RoundCorner(new Size(2312, 6), 4.5f));
            int levelprogressFrontPos = (int)((double)2312 * (double)data.userInfo.Statistics.Level.Progress / 100.0);
            if (levelprogressFrontPos > 0)
            {
                Img levelprogress_front = new Image<Rgba32>(levelprogressFrontPos, 6);
                levelprogress_front.Mutate(x => x.Fill(levelprogressFrontColor));
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
                1 => "./work/panelv2/infov2-dark.png",
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
            switch (ColorMode)
            {
                case 0:
                    //light
                    //do nothing  
                    break;
                case 1:
                    //dark
                    avatar.Mutate(x => x.Brightness(0.6f));
                    break;
            }
            avatar.Mutate(x => x.Resize(200, 200).RoundCorner(new Size(200, 200), 25));
            info.Mutate(x => x.DrawImage(avatar, new Point(1531, 72), 1));


            //username
            textOptions.Origin = new PointF(1780, 230);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, data.userInfo.Username, new SolidBrush(usernameColor), null));

            //rank
            textOptions.Font = new Font(TorusRegular, 60);
            textOptions.Origin = new PointF(1972, 481);
            textOptions.VerticalAlignment = VerticalAlignment.Center;
            info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("#{0:N0}", data.userInfo.Statistics.GlobalRank), new SolidBrush(rankMainColor), null));

            //country_flag
            Img flags = Img.Load(await Utils.LoadFile2Byte($"./work/flags/{data.userInfo.Country.Code}.png"));
            flags.Mutate(x => x.Resize(100, 67));
            info.Mutate(x => x.DrawImage(flags, new Point(1577, 600), 1));

            //country_rank
            textOptions.Origin = new PointF(1687, 629);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("#{0:N0}", data.userInfo.Statistics.CountryRank), new SolidBrush(rankMainColor), null));

            //pp
            textOptions.Origin = new PointF(3120, 350);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:N0}", data.userInfo.Statistics.PP), new SolidBrush(ppMainColor), null));

            //acc
            textOptions.Origin = new PointF(3120, 551);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:0.##}%", data.userInfo.Statistics.HitAccuracy), new SolidBrush(accMainColor), null));

            //ppsub
            textOptions.HorizontalAlignment = HorizontalAlignment.Right;
            textOptions.Font = new Font(TorusRegular, 40);
            var ppsub_point = 2374 + pp_front_length - 40;
            textOptions.Origin = new PointF(ppsub_point, 440);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:N0}", scorePP), new SolidBrush(ppSubColor), null));

            //acc sub
            var accsub_point = (2374 + (int)(1443.00 * (data.userInfo.Statistics.HitAccuracy / 100.0))) - 40;
            textOptions.Origin = new PointF(accsub_point, 641);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, "300", new SolidBrush(accSubColor), null));

            //grades
            textOptions.Font = new Font(TorusRegular, 38);
            textOptions.HorizontalAlignment = HorizontalAlignment.Center;
            textOptions.Origin = new PointF(2646, 988);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:N0}", data.userInfo.Statistics.GradeCounts.SSH), new SolidBrush(gradeCountsColor), null));
            textOptions.Origin = new PointF(2646 + 218, 988);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:N0}", data.userInfo.Statistics.GradeCounts.SS), new SolidBrush(gradeCountsColor), null));
            textOptions.Origin = new PointF(2646 + 218 * 2, 988);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:N0}", data.userInfo.Statistics.GradeCounts.SH), new SolidBrush(gradeCountsColor), null));
            textOptions.Origin = new PointF(2646 + 218 * 3, 988);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:N0}", data.userInfo.Statistics.GradeCounts.S), new SolidBrush(gradeCountsColor), null));
            textOptions.Origin = new PointF(2646 + 218 * 4, 988);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:N0}", data.userInfo.Statistics.GradeCounts.A), new SolidBrush(gradeCountsColor), null));

            //level main
            textOptions.Origin = new PointF(3906, 2470);
            textOptions.Font = new Font(TorusRegular, 48);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, data.userInfo.Statistics.Level.Current.ToString(), new SolidBrush(mainColor), null));

            //update time
            textOptions.Origin = new PointF(3955, 2582);
            textOptions.HorizontalAlignment = HorizontalAlignment.Right;
            textOptions.Font = new Font(TorusRegular, 40);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, $"Update at {DateTime.Now}", new SolidBrush(detailsColor), null));

            //details
            textOptions.Font = new Font(TorusRegular, 50);
            textOptions.HorizontalAlignment = HorizontalAlignment.Left;
            //play time
            textOptions.Origin = new PointF(1705, 1217);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, Utils.Duration2StringWithoutSec(data.userInfo.Statistics.PlayTime), new SolidBrush(detailsColor), null));
            //total hits
            textOptions.Origin = new PointF(2285, 1217);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:N0}", data.userInfo.Statistics.TotalHits), new SolidBrush(detailsColor), null));
            //play count
            textOptions.Origin = new PointF(2853, 1217);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:N0}", data.userInfo.Statistics.PlayCount), new SolidBrush(detailsColor), null));
            //ranked scores
            textOptions.Origin = new PointF(3420, 1217);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:N0}", data.userInfo.Statistics.RankedScore), new SolidBrush(detailsColor), null));

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
            info.Mutate(x => x.DrawText(drawOptions, textOptions, title, new SolidBrush(mainColor), null));
            //mods
            if (allBP![0].Mods.Length > 0)
            {
                textOptions.Origin = new PointF(1945 + TextMeasurer.Measure(title, textOptions).Width + 25, 1611);
                textOptions.Font = new Font(TorusRegular, 40);
                var mainscoremods = "+";
                foreach (var x in allBP![0].Mods)
                    mainscoremods += $"{x}, ";
                info.Mutate(x => x.DrawText(drawOptions, textOptions, mainscoremods[..mainscoremods.LastIndexOf(",")], new SolidBrush(mainColor), null));
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
            info.Mutate(x => x.DrawText(drawOptions, textOptions, artist, new SolidBrush(mainColor), null));

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
            info.Mutate(x => x.DrawText(drawOptions, textOptions, creator, new SolidBrush(mainColor), null));

            //bid
            textOptions.Origin = new PointF(2447, 1668);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, allBP![0].Beatmap!.BeatmapId.ToString(), new SolidBrush(mainColor), null));

            //get stars from rosupp
            var ppinfo = await PerformanceCalculator.CalculatePanelData(allBP[0]);
            textOptions.Origin = new PointF(2657, 1668);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, ppinfo.ppInfo.star.ToString("0.##*"), new SolidBrush(mainColor), null));

            //acc
            textOptions.Origin = new PointF(2813, 1668);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, allBP![0].Accuracy.ToString("0.##%"), new SolidBrush(mainColor), null));

            //rank
            textOptions.Origin = new PointF(2988, 1668);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, allBP![0].Rank, new SolidBrush(mainColor), null));

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
                info.Mutate(x => x.DrawText(drawOptions, textOptions, title, new SolidBrush(mainColor), null));
            }

            //2nd~5th version and acc and bid and shdklahdksadkjkcna5hoacsporjasldjlksakdlsa
            textOptions.Font = new Font(TorusRegular, 40);
            var otherbp_mods_pos_y = 1853;
            for (int i = 1; i < 5; ++i)
            {
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
                info.Mutate(x => x.DrawText(drawOptions, textOptions, title + " | ", new SolidBrush(mainColor), null));
                var textMeasurePos = MainTitleAndDifficultyTitlePos_X + TextMeasurer.Measure(title + " | ", textOptions).Width + 5;
                //bid
                textOptions.Origin = new PointF(textMeasurePos, 1925 + 186 * (i - 1));
                info.Mutate(x => x.DrawText(drawOptions, textOptions, allBP![i].Beatmap!.BeatmapId.ToString() + " | ", new SolidBrush(mainColor), null));
                textMeasurePos = textMeasurePos + TextMeasurer.Measure(allBP![i].Beatmap!.BeatmapId.ToString() + " | ", textOptions).Width + 5;
                //star
                var ppinfo1 = await PerformanceCalculator.CalculatePanelData(allBP[i]);
                textOptions.Origin = new PointF(textMeasurePos, 1925 + 186 * (i - 1));
                info.Mutate(x => x.DrawText(drawOptions, textOptions, ppinfo1.ppInfo.star.ToString("0.##*") + " | ", new SolidBrush(mainColor), null));
                textMeasurePos = textMeasurePos + TextMeasurer.Measure(ppinfo1.ppInfo.star.ToString("0.##*") + " | ", textOptions).Width + 5;
                //acc
                textOptions.Origin = new PointF(textMeasurePos, 1925 + 186 * (i - 1));
                info.Mutate(x => x.DrawText(drawOptions, textOptions, allBP![i].Accuracy.ToString("0.##%"), new SolidBrush(highlightColor), null));
                textMeasurePos = textMeasurePos + TextMeasurer.Measure(allBP![i].Accuracy.ToString("0.##%"), textOptions).Width + 5;
                //ranking
                textOptions.Origin = new PointF(textMeasurePos, 1925 + 186 * (i - 1));
                info.Mutate(x => x.DrawText(drawOptions, textOptions, " | ", new SolidBrush(mainColor), null));
                textMeasurePos = textMeasurePos + TextMeasurer.Measure(" | ", textOptions).Width + 5;
                textOptions.Origin = new PointF(textMeasurePos, 1925 + 186 * (i - 1));
                info.Mutate(x => x.DrawText(drawOptions, textOptions, allBP![i].Rank, new SolidBrush(mainColor), null));
                //shdklahdksadkjkcna5hoacsporjasldjlksakdlsa
                if (allBP![i].Mods.Length > 0)
                {
                    var otherbp_mods_pos_x = 2580;
                    foreach (var x in allBP![i].Mods)
                    {
                        Img modicon = Img.Load(await Utils.LoadFile2Byte($"./work/mods_v2/2x/{x}.png"));
                        modicon.Mutate(x => x.Resize(90, 90));
                        info.Mutate(x => x.DrawImage(modicon, new Point(otherbp_mods_pos_x, otherbp_mods_pos_y), 1));
                        otherbp_mods_pos_x += 105;
                    }
                }
                otherbp_mods_pos_y += 186;
            }

            //all pp
            textOptions.Font = new Font(TorusRegular, 90);
            textOptions.HorizontalAlignment = HorizontalAlignment.Center;
            textOptions.Origin = new PointF(3642, 1670);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:N1}", allBP![0].PP), new SolidBrush(scoreppTopColor), null));
            var bp1pptextMeasure = TextMeasurer.Measure(string.Format("{0:N1}", allBP![0].PP), textOptions);
            int bp1pptextpos = 3642 - (int)bp1pptextMeasure.Width / 2;
            textOptions.Font = new Font(TorusRegular, 40);
            textOptions.Origin = new PointF(bp1pptextpos, 1610);
            textOptions.HorizontalAlignment = HorizontalAlignment.Left;
            info.Mutate(x => x.DrawText(drawOptions, textOptions, "pp", new SolidBrush(mainColor), null));

            textOptions.HorizontalAlignment = HorizontalAlignment.Center;
            textOptions.Font = new Font(TorusRegular, 70);
            textOptions.Origin = new PointF(3642, 1895);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:N0}pp", allBP![1].PP), new SolidBrush(scoreppColor), null));
            textOptions.Origin = new PointF(3642, 2081);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:N0}pp", allBP![2].PP), new SolidBrush(scoreppColor), null));
            textOptions.Origin = new PointF(3642, 2266);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:N0}pp", allBP![3].PP), new SolidBrush(scoreppColor), null));
            textOptions.Origin = new PointF(3642, 2450);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, string.Format("{0:N0}pp", allBP![4].PP), new SolidBrush(scoreppColor), null));

            //badges
            if (data.badgeId != -1)
            {
                Img badge;
                switch (ColorMode)
                {
                    case 0:
                        //light
                        try
                        {
                            badge = Img.Load(await Utils.LoadFile2Byte($"./work/badges/{data.badgeId}.png"), new PngDecoder());
                            badge.Mutate(x => x.Resize(236, 110).RoundCorner(new Size(236, 110), 20));
                            info.Mutate(x => x.DrawImage(badge, new Point(3566, 93), 1));
                        }
                        catch { }
                        break;
                    case 1:
                        //dark
                        try
                        {
                            badge = Img.Load(await Utils.LoadFile2Byte($"./work/badges/{data.badgeId}.png"), new PngDecoder());
                            badge.Mutate(x => x.Resize(236, 110).Brightness(0.6f).RoundCorner(new Size(236, 110), 20));
                            info.Mutate(x => x.DrawImage(badge, new Point(3566, 93), 1));
                        }
                        catch { }
                        break;
                }
            }







            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!test info!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            textOptions.HorizontalAlignment = HorizontalAlignment.Center;
            textOptions.Font = new Font(TorusRegular, 40);
            textOptions.Origin = new PointF(2000, 2582);
            info.Mutate(x => x.DrawText(drawOptions, textOptions, "this is a test version and does not represent the final quality", new SolidBrush(detailsColor), null));













            return info;
        }
    }
}
