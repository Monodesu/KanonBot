using System.Collections.Generic;
using System.IO;
using System.Numerics;
using KanonBot.API;
using KanonBot.API.OSU;
using KanonBot.Image;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.Diagnostics;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using static KanonBot.API.OSU.DataStructure;
using static KanonBot.Image.OSU.OsuInfoPanelV2;
using static KanonBot.Image.OSU.OsuResourceHelper;
using static KanonBot.Image.OSU.ResourceRegistrar;
using Img = SixLabors.ImageSharp.Image;
using ResizeOptions = SixLabors.ImageSharp.Processing.ResizeOptions;

namespace KanonBot.Image.OSU
{
    public static class OsuInfoPanelV1
    {
        public static async Task<Img> Draw(
            UserPanelData data,
            bool isBonded = false,
            bool eventmode = false,
            bool isDataOfDayAvaiavle = true
        )
        {
            var info = new Image<Rgba32>(1200, 857);
            // custom panel
            using var panel = await GetInfoV1PanelAsync(data.userInfo!.Id);

            // cover
            using var cover = await GetInfoCoverAsync(data.userInfo!.Id, data.userInfo.CoverUrl!);
            var resizeOptions = new ResizeOptions
            {
                Size = new Size(1200, 350),
                Sampler = KnownResamplers.Lanczos3,
                Compand = true,
                Mode = ResizeMode.Crop
            };
            cover.Mutate(x => x.Resize(resizeOptions));
            info.Mutate(x => x.DrawImage(cover, 1));
            info.Mutate(x => x.DrawImage(panel, 1));

            //avatar
            var avatarPath = $"./work/avatar/{data.userInfo.Id}.png";
            using var avatar = await GetUserAvatarAsync(
                data.userInfo!.Id,
                data.userInfo!.AvatarUrl!
            );

            avatar.Mutate(x => x.Resize(190, 190).RoundCorner(new Size(190, 190), 40));
            info.Mutate(x => x.DrawImage(avatar, new Point(39, 55), 1));

            // badge 取第一个绘制
            if (data.badgeId[0] != -1)
            {
                try
                {
                    int dbcountl = 0;
                    for (int i = 0; i < data.badgeId.Count; ++i)
                    {
                        if (data.badgeId[i] > -1)
                        {
                            using var badge = await Img.LoadAsync<Rgba32>(
                                $"./work/badges/{data.badgeId[i]}.png"
                            );
                            badge.Mutate(x => x.Resize(86, 40));
                            info.Mutate(
                                x => x.DrawImage(badge, new Point(272 + (dbcountl * 100), 152), 1)
                            );
                            ++dbcountl;
                            if (dbcountl > 4)
                                break;
                        }
                    }
                }
                catch { }
            }

            // obj
            var drawOptions = new DrawingOptions
            {
                GraphicsOptions = new GraphicsOptions { Antialias = true }
            };

            using var flags = await Img.LoadAsync(
                $"./work/flags/{data.userInfo.Country!.Code}.png"
            );
            info.Mutate(x => x.DrawImage(flags, new Point(272, 212), 1));
            using var modeicon = await Img.LoadAsync(
                $"./work/legacy/mode_icon/{data.userInfo.PlayMode.ToStr()}.png"
            );
            modeicon.Mutate(x => x.Resize(64, 64));
            info.Mutate(x => x.DrawImage(modeicon, new Point(1125, 10), 1));

            // pp+
            if (data.userInfo.PlayMode is Enums.Mode.OSU)
            {
                using var ppdataPanel = await Img.LoadAsync("./work/legacy/pp+-v1.png");
                info.Mutate(x => x.DrawImage(ppdataPanel, new Point(0, 0), 1));
                Hexagram.HexagramInfo hi =
                    new()
                    {
                        abilityFillColor = Color.FromRgba(253, 148, 62, 128),
                        abilityLineColor = Color.ParseHex("#fd943e"),
                        nodeMaxValue = 10000,
                        nodeCount = 6,
                        size = 200,
                        sideLength = 200,
                        mode = 1,
                        strokeWidth = 2f,
                        nodesize = new SizeF(5f, 5f)
                    };
                // acc ,flow, jump, pre, speed, sta
                var ppd = new int[6]; // 这里就强制转换了
                try
                {
                    ppd[0] = (int)data.pplusInfo!.AccuracyTotal;
                    ppd[1] = (int)data.pplusInfo.FlowAimTotal;
                    ppd[2] = (int)data.pplusInfo.JumpAimTotal;
                    ppd[3] = (int)data.pplusInfo.PrecisionTotal;
                    ppd[4] = (int)data.pplusInfo.SpeedTotal;
                    ppd[5] = (int)data.pplusInfo.StaminaTotal;
                }
                catch
                {
                    for (int i = 0; i < 6; i++)
                        ppd[i] = 0;
                }
                // x_offset  pp+数据的坐标偏移量
                var x_offset = new int[6] { 372, 330, 122, 52, 128, 317 }; // pp+数据的x轴坐标
                var multi = new double[6] { 14.1, 69.7, 1.92, 19.8, 0.588, 3.06 };
                var exp = new double[6] { 0.769, 0.596, 0.953, 0.8, 1.175, 0.993 };
                using var pppImg = Hexagram.Draw(ppd, multi, exp, hi);
                info.Mutate(x => x.DrawImage(pppImg, new Point(132, 626), 1));
                var f = new Font(Exo2Regular, 18);
                var pppto = new RichTextOptions(f)
                {
                    VerticalAlignment = VerticalAlignment.Bottom,
                    HorizontalAlignment = HorizontalAlignment.Left,
                };
                var color = Color.ParseHex("#FFCC33");
                for (var i = 0; i < hi.nodeCount; i++)
                {
                    pppto.Origin = new Vector2(
                        x_offset[i],
                        (i % 3 != 0) ? (i < 3 ? 642 : 831) : 736
                    );
                    info.Mutate(
                        x =>
                            x.DrawText(
                                drawOptions,
                                pppto,
                                $"({ppd[i]})",
                                new SolidBrush(color),
                                null
                            )
                    );
                }
            }
            else
            {
                using var ppdataPanel = await Img.LoadAsync("./work/legacy/nopp+info-v1.png");
                info.Mutate(x => x.DrawImage(ppdataPanel, new Point(0, 0), 1));
            }

            // time
            var textOptions = new RichTextOptions(new Font(Exo2Regular, 20))
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Left,
                Origin = new PointF(15, 25)
            };
            info.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        $"update: {DateTime.Now:yyyy/MM/dd HH:mm:ss}",
                        new SolidBrush(Color.White),
                        null
                    )
            );
            if (data.daysBefore > 1)
            {
                textOptions = new RichTextOptions(new Font(HarmonySans, 20))
                {
                    VerticalAlignment = VerticalAlignment.Bottom,
                    HorizontalAlignment = HorizontalAlignment.Left,
                };
                if (isDataOfDayAvaiavle)
                {
                    textOptions.Origin = new PointF(300, 25);
                    info.Mutate(
                        x =>
                            x.DrawText(
                                drawOptions,
                                textOptions,
                                $"对比自{data.daysBefore}天前",
                                new SolidBrush(Color.White),
                                null
                            )
                    );
                }
                else
                {
                    textOptions.Origin = new PointF(300, 25);
                    info.Mutate(
                        x =>
                            x.DrawText(
                                drawOptions,
                                textOptions,
                                $" 请求的日期没有数据.." + $"当前数据对比自{data.daysBefore}天前",
                                new SolidBrush(Color.White),
                                null
                            )
                    );
                }
            }
            // username
            textOptions.Font = new Font(Exo2SemiBold, 60);
            textOptions.Origin = new PointF(268, 140);
            info.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        data.userInfo.Username!,
                        new SolidBrush(Color.White),
                        null
                    )
            );

            var Statistics = data.userInfo.Statistics;
            var prevStatistics = data.prevUserInfo?.Statistics ?? data.userInfo.Statistics; // 没有就为当前数据

            // country_rank
            string countryRank;
            if (isBonded)
            {
                var diff = Statistics!.CountryRank - prevStatistics!.CountryRank;
                if (diff > 0)
                    countryRank = string.Format("#{0:N0}(-{1:N0})", Statistics.CountryRank, diff);
                else if (diff < 0)
                    countryRank = string.Format(
                        "#{0:N0}(+{1:N0})",
                        Statistics.CountryRank,
                        Math.Abs(diff)
                    );
                else
                    countryRank = string.Format("#{0:N0}", Statistics.CountryRank);
            }
            else
            {
                countryRank = string.Format("#{0:N0}", Statistics!.CountryRank);
            }
            textOptions.Font = new Font(Exo2SemiBold, 20);
            textOptions.Origin = new PointF(350, 260);
            info.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        countryRank,
                        new SolidBrush(Color.White),
                        null
                    )
            );
            // global_rank
            string diffStr;
            if (isBonded)
            {
                var diff = Statistics.GlobalRank - prevStatistics!.GlobalRank;
                if (diff > 0)
                    diffStr = string.Format("↓ {0:N0}", diff);
                else if (diff < 0)
                    diffStr = string.Format("↑ {0:N0}", Math.Abs(diff));
                else
                    diffStr = "↑ -";
            }
            else
            {
                diffStr = "↑ -";
            }
            textOptions.Font = new Font(Exo2Regular, 40);
            textOptions.Origin = new PointF(40, 410);
            info.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        string.Format("{0:N0}", Statistics.GlobalRank),
                        new SolidBrush(Color.White),
                        null
                    )
            );
            textOptions.Font = new Font(HarmonySans, 14);
            textOptions.Origin = new PointF(40, 430);
            info.Mutate(
                x =>
                    x.DrawText(drawOptions, textOptions, diffStr, new SolidBrush(Color.White), null)
            );
            // pp
            if (isBonded)
            {
                var diff = Statistics.PP - prevStatistics!.PP;
                if (diff >= 0.01)
                    diffStr = string.Format("↑ {0:0.##}", diff);
                else if (diff <= -0.01)
                    diffStr = string.Format("↓ {0:0.##}", Math.Abs(diff));
                else
                    diffStr = "↑ -";
            }
            else
            {
                diffStr = "↑ -";
            }
            textOptions.Font = new Font(Exo2Regular, 40);
            textOptions.Origin = new PointF(246, 410);
            info.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        string.Format("{0:0.##}", Statistics.PP),
                        new SolidBrush(Color.White),
                        null
                    )
            );
            textOptions.Font = new Font(HarmonySans, 14);
            textOptions.Origin = new PointF(246, 430);
            info.Mutate(
                x =>
                    x.DrawText(drawOptions, textOptions, diffStr, new SolidBrush(Color.White), null)
            );
            // ssh ss
            textOptions.Font = new Font(Exo2Regular, 30);
            textOptions.HorizontalAlignment = HorizontalAlignment.Center;
            textOptions.Origin = new PointF(80, 540);
            info.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        Statistics.GradeCounts!.SSH.ToString(),
                        new SolidBrush(Color.White),
                        null
                    )
            );
            textOptions.Origin = new PointF(191, 540);
            info.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        Statistics.GradeCounts!.SS.ToString(),
                        new SolidBrush(Color.White),
                        null
                    )
            );
            textOptions.Origin = new PointF(301, 540);
            info.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        Statistics.GradeCounts!.SH.ToString(),
                        new SolidBrush(Color.White),
                        null
                    )
            );
            textOptions.Origin = new PointF(412, 540);
            info.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        Statistics.GradeCounts!.S.ToString(),
                        new SolidBrush(Color.White),
                        null
                    )
            );
            textOptions.Origin = new PointF(522, 540);
            info.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        Statistics.GradeCounts!.A.ToString(),
                        new SolidBrush(Color.White),
                        null
                    )
            );
            // level
            textOptions.Font = new Font(Exo2SemiBold, 34);
            textOptions.Origin = new PointF(1115, 385);
            info.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        Statistics.Level!.Current.ToString(),
                        new SolidBrush(Color.White),
                        null
                    )
            );
            // Level%
            var levelper = Statistics.Level!.Progress;
            textOptions.HorizontalAlignment = HorizontalAlignment.Right;
            textOptions.Font = new Font(Exo2SemiBold, 20);
            textOptions.Origin = new PointF(1060, 400);
            info.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        $"{levelper}%",
                        new SolidBrush(Color.White),
                        null
                    )
            );
            try
            {
                using var levelRoundrect = new Image<Rgba32>(4 * levelper, 7);
                levelRoundrect.Mutate(
                    x => x.Fill(Color.ParseHex("#FF66AB")).RoundCorner(new Size(4 * levelper, 7), 5)
                );
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
            info.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        rankedScore,
                        new SolidBrush(Color.White),
                        null
                    )
            );
            string acc;
            if (isBonded)
            {
                var diff = Statistics.HitAccuracy - prevStatistics!.HitAccuracy;
                if (diff >= 0.01)
                    acc = string.Format("{0:0.##}%(+{1:0.##}%)", Statistics.HitAccuracy, diff);
                else if (diff <= -0.01)
                    acc = string.Format("{0:0.##}%({1:0.##}%)", Statistics.HitAccuracy, diff);
                else
                    acc = string.Format("{0:0.##}%", Statistics.HitAccuracy);
            }
            else
            {
                acc = string.Format("{0:0.##}%", Statistics.HitAccuracy);
            }
            textOptions.Origin = new PointF(1180, 665);
            info.Mutate(
                x => x.DrawText(drawOptions, textOptions, acc, new SolidBrush(Color.White), null)
            );
            string playCount;
            if (isBonded)
            {
                var diff = Statistics.PlayCount - prevStatistics!.PlayCount;
                if (diff > 0)
                    playCount = string.Format("{0:N0}(+{1:N0})", Statistics.PlayCount, diff);
                else if (diff < 0)
                    playCount = string.Format("{0:N0}({1:N0})", Statistics.PlayCount, diff);
                else
                    playCount = string.Format("{0:N0}", Statistics.PlayCount);
            }
            else
            {
                playCount = string.Format("{0:N0}", Statistics.PlayCount);
            }
            textOptions.Origin = new PointF(1180, 705);
            info.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        playCount,
                        new SolidBrush(Color.White),
                        null
                    )
            );
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
            info.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        totalScore,
                        new SolidBrush(Color.White),
                        null
                    )
            );
            string totalHits;
            if (isBonded)
            {
                var diff = Statistics.TotalHits - prevStatistics!.TotalHits;
                if (diff > 0)
                    totalHits = string.Format("{0:N0}(+{1:N0})", Statistics.TotalHits, diff);
                else if (diff < 0)
                    totalHits = string.Format("{0:N0}({1:N0})", Statistics.TotalHits, diff);
                else
                    totalHits = string.Format("{0:N0}", Statistics.TotalHits);
            }
            else
            {
                totalHits = string.Format("{0:N0}", Statistics.TotalHits);
            }
            textOptions.Origin = new PointF(1180, 785);
            info.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        totalHits,
                        new SolidBrush(Color.White),
                        null
                    )
            );
            textOptions.Origin = new PointF(1180, 825);
            info.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        Utils.Duration2String(Statistics.PlayTime),
                        new SolidBrush(Color.White),
                        null
                    )
            );
            info.Mutate(x => x.RoundCorner(new Size(1200, 857), 24));
            return info;
        }

        public static Img DrawString(string str, float fontSize)
        {
            var font = new Font(HarmonySans, fontSize);
            var textOptions = new RichTextOptions(new Font(HarmonySans, fontSize))
            {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Origin = new PointF(fontSize / 2, fontSize / 2)
            };
            var m = TextMeasurer.MeasureSize(str, textOptions);

            var img = new Image<Rgba32>((int)(m.Width + fontSize), (int)(m.Height + fontSize));
            img.Mutate(x => x.Fill(Color.White));
            var drawOptions = new DrawingOptions
            {
                GraphicsOptions = new GraphicsOptions { Antialias = true }
            };
            img.Mutate(
                x => x.DrawText(drawOptions, textOptions, str, new SolidBrush(Color.Black), null)
            );
            return img;
        }
    }
}
