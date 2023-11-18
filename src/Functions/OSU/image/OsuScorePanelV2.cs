using System.IO;
using System.Numerics;
using KanonBot.API;
using KanonBot.Functions.OSU.RosuPP;
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
using Img = SixLabors.ImageSharp.Image;
using ResizeOptions = SixLabors.ImageSharp.Processing.ResizeOptions;
using System.Collections.Generic;
using SixLabors.ImageSharp.Diagnostics;
using static KanonBot.API.OSU.DataStructure;
using KanonBot.API.OSU;
using static KanonBot.Image.OSU.ResourceRegistrar;
using static KanonBot.Image.OSU.OsuResourceHelper;
using KanonBot.Image;
using LanguageExt.ClassInstances;

namespace KanonBot.Image.OSU
{
    public static class OsuScorePanelV2
    {
        public static async Task<Img> Draw(ScorePanelData data)
        {
            var ppInfo = data.ppInfo;
            var score = new Image<Rgba32>(1950, 1088);

            //avatar
            using var avatar = await GetUserAvatarAsync(data.scoreInfo!.User!.Id, data.scoreInfo.User!.AvatarUrl!);

            using var panel = data.scoreInfo.Mode switch
            {
                Enums.Mode.Fruits
                    => await Img.LoadAsync("work/legacy/v2_scorepanel/default-score-v2-fruits.png"),
                Enums.Mode.Mania
                    => await Img.LoadAsync("work/legacy/v2_scorepanel/default-score-v2-mania.png"),
                _ => await Img.LoadAsync("work/legacy/v2_scorepanel/default-score-v2.png")
            };

            // bg
            using var bg = await TryAsync(GetBeatmapBackgroundImageAsync(
                data.scoreInfo.Beatmapset!.Id, data.scoreInfo.Beatmap!.BeatmapId))
                .IfFail(await ReadImageRgba("./work/legacy/load-failed-img.png"));
            using var smallBg = bg.Clone(x => x.RoundCorner(new Size(433, 296), 20));
            using var backBlack = new Image<Rgba32>(1950 - 2, 1088);
            backBlack.Mutate(
                x => x.BackgroundColor(Color.Black).RoundCorner(new Size(1950 - 2, 1088), 20)
            );
            bg.Mutate(x => x.GaussianBlur(5).RoundCorner(new Size(1950 - 2, 1088), 20));
            score.Mutate(x => x.DrawImage(bg, 1));
            score.Mutate(x => x.DrawImage(backBlack, 0.33f));
            score.Mutate(x => x.DrawImage(panel, 1));
            score.Mutate(x => x.DrawImage(smallBg, new Point(27, 34), 1));
            bg.Dispose();

            // StarRing
            // diff circle
            // green, blue, yellow, red, purple, black
            // [0,2), [2,3), [3,4), [4,5), [5,7), [7,?)
            var ringFile = new string[6];
            switch (data.scoreInfo.Mode)
            {
                case Enums.Mode.OSU:
                    ringFile[0] = "std-easy.png";
                    ringFile[1] = "std-normal.png";
                    ringFile[2] = "std-hard.png";
                    ringFile[3] = "std-insane.png";
                    ringFile[4] = "std-expert.png";
                    ringFile[5] = "std-expertplus.png";
                    break;
                case Enums.Mode.Fruits:
                    ringFile[0] = "ctb-easy.png";
                    ringFile[1] = "ctb-normal.png";
                    ringFile[2] = "ctb-hard.png";
                    ringFile[3] = "ctb-insane.png";
                    ringFile[4] = "ctb-expert.png";
                    ringFile[5] = "ctb-expertplus.png";
                    break;
                case Enums.Mode.Taiko:
                    ringFile[0] = "taiko-easy.png";
                    ringFile[1] = "taiko-normal.png";
                    ringFile[2] = "taiko-hard.png";
                    ringFile[3] = "taiko-insane.png";
                    ringFile[4] = "taiko-expert.png";
                    ringFile[5] = "taiko-expertplus.png";
                    break;
                case Enums.Mode.Mania:
                    ringFile[0] = "mania-easy.png";
                    ringFile[1] = "mania-normal.png";
                    ringFile[2] = "mania-hard.png";
                    ringFile[3] = "mania-insane.png";
                    ringFile[4] = "mania-expert.png";
                    ringFile[5] = "mania-expertplus.png";
                    break;
            }
            string temp;
            var star = ppInfo.star;
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
            using var diffCircle = await Img.LoadAsync("./work/icons/" + temp);
            diffCircle.Mutate(x => x.Resize(65, 65));
            score.Mutate(x => x.DrawImage(diffCircle, new Point(512, 257), 1));
            // beatmap_status
            if (data.scoreInfo.Beatmap.Status is Enums.Status.ranked)
            {
                using var c = await Img.LoadAsync("./work/icons/ranked.png");
                score.Mutate(x => x.DrawImage(c, new Point(415, 16), 1));
            }
            if (data.scoreInfo.Beatmap.Status is Enums.Status.approved)
            {
                using var c = await Img.LoadAsync("./work/icons/approved.png");
                score.Mutate(x => x.DrawImage(c, new Point(415, 16), 1));
            }
            if (data.scoreInfo.Beatmap.Status is Enums.Status.loved)
            {
                using var c = await Img.LoadAsync("./work/icons/loved.png");
                score.Mutate(x => x.DrawImage(c, new Point(415, 16), 1));
            }
            // mods
            var mods = data.scoreInfo.Mods;
            var modp = 0;
            foreach (var mod in mods!)
            {
                try
                {
                    using var modPic = await Img.LoadAsync($"./work/mods/{mod}.png");
                    modPic.Mutate(x => x.Resize(200, 61));
                    score.Mutate(x => x.DrawImage(modPic, new Point((modp * 160) + 440, 440), 1));
                    modp += 1;
                }
                catch
                {
                    continue;
                }
            }
            // rankings
            var ranking = data.scoreInfo.Passed ? data.scoreInfo.Rank : "F";
            using var rankPic = await Img.LoadAsync($"./work/ranking/ranking-{ranking}.png");
            score.Mutate(x => x.DrawImage(rankPic, new Point(913, 874), 1));
            // text part (文字部分)
            var font = new Font(TorusRegular, 60);
            var drawOptions = new DrawingOptions
            {
                GraphicsOptions = new GraphicsOptions { Antialias = true }
            };
            var textOptions = new RichTextOptions(new Font(font, 60))
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            // beatmap_info
            var title = "";
            foreach (char c in data.scoreInfo.Beatmapset!.Title!)
            {
                title += c;
                var m = TextMeasurer.MeasureSize(title, textOptions);
                if (m.Width > 725)
                {
                    title += "...";
                    break;
                }
            }
            textOptions.Origin = new PointF(499, 110);
            score.Mutate(
                x => x.DrawText(drawOptions, textOptions, title, new SolidBrush(Color.Black), null)
            );
            textOptions.Origin = new PointF(499, 105);
            score.Mutate(
                x => x.DrawText(drawOptions, textOptions, title, new SolidBrush(Color.White), null)
            );
            // artist
            textOptions.Font = new Font(TorusRegular, 40);
            var artist = "";
            foreach (char c in data.scoreInfo.Beatmapset.Artist!)
            {
                artist += c;
                var m = TextMeasurer.MeasureSize(artist, textOptions);
                if (m.Width > 205)
                {
                    artist += "...";
                    break;
                }
            }
            textOptions.Origin = new PointF(519, 178);
            score.Mutate(
                x => x.DrawText(drawOptions, textOptions, artist, new SolidBrush(Color.Black), null)
            );
            textOptions.Origin = new PointF(519, 175);
            score.Mutate(
                x => x.DrawText(drawOptions, textOptions, artist, new SolidBrush(Color.White), null)
            );
            // creator
            var creator = "";
            foreach (char c in data.scoreInfo.Beatmapset.Creator!)
            {
                creator += c;
                var m = TextMeasurer.MeasureSize(creator, textOptions);
                if (m.Width > 145)
                {
                    creator += "...";
                    break;
                }
            }
            textOptions.Origin = new PointF(795, 178);
            score.Mutate(
                x =>
                    x.DrawText(drawOptions, textOptions, creator, new SolidBrush(Color.Black), null)
            );
            textOptions.Origin = new PointF(795, 175);
            score.Mutate(
                x =>
                    x.DrawText(drawOptions, textOptions, creator, new SolidBrush(Color.White), null)
            );
            // beatmap_id
            var beatmap_id = data.scoreInfo.Beatmap.BeatmapId.ToString();
            textOptions.Origin = new PointF(1008, 178);
            score.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        beatmap_id,
                        new SolidBrush(Color.Black),
                        null
                    )
            );
            textOptions.Origin = new PointF(1008, 175);
            score.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        beatmap_id,
                        new SolidBrush(Color.White),
                        null
                    )
            );
            // ar,od info
            var color = Color.ParseHex("#f1ce59");
            textOptions.Font = new Font(TorusRegular, 24.25f);
            // time
            var song_time = Utils.Duration2TimeString(data.scoreInfo.Beatmap.TotalLength);
            textOptions.Origin = new PointF(1741, 127);
            score.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        song_time,
                        new SolidBrush(Color.Black),
                        null
                    )
            );
            textOptions.Origin = new PointF(1741, 124);
            score.Mutate(
                x => x.DrawText(drawOptions, textOptions, song_time, new SolidBrush(color), null)
            );
            // bpm
            var bpm = data.scoreInfo.Beatmap.BPM.GetValueOrDefault().ToString("0");
            textOptions.Origin = new PointF(1457, 127);
            score.Mutate(
                x => x.DrawText(drawOptions, textOptions, bpm, new SolidBrush(Color.Black), null)
            );
            textOptions.Origin = new PointF(1457, 124);
            score.Mutate(
                x => x.DrawText(drawOptions, textOptions, bpm, new SolidBrush(color), null)
            );
            // ar
            var ar = ppInfo.AR.ToString("0.0#");
            textOptions.Origin = new PointF(1457, 218);
            score.Mutate(
                x => x.DrawText(drawOptions, textOptions, ar, new SolidBrush(Color.Black), null)
            );
            textOptions.Origin = new PointF(1457, 215);
            score.Mutate(
                x => x.DrawText(drawOptions, textOptions, ar, new SolidBrush(color), null)
            );
            // od
            var od = ppInfo.OD.ToString("0.0#");
            textOptions.Origin = new PointF(1741, 218);
            score.Mutate(
                x => x.DrawText(drawOptions, textOptions, od, new SolidBrush(Color.Black), null)
            );
            textOptions.Origin = new PointF(1741, 215);
            score.Mutate(
                x => x.DrawText(drawOptions, textOptions, od, new SolidBrush(color), null)
            );
            // cs
            var cs = ppInfo.CS.ToString("0.0#");
            textOptions.Origin = new PointF(1457, 312);
            score.Mutate(
                x => x.DrawText(drawOptions, textOptions, cs, new SolidBrush(Color.Black), null)
            );
            textOptions.Origin = new PointF(1457, 309);
            score.Mutate(
                x => x.DrawText(drawOptions, textOptions, cs, new SolidBrush(color), null)
            );
            // hp
            var hp = ppInfo.HP.ToString("0.0#");
            textOptions.Origin = new PointF(1741, 312);
            score.Mutate(
                x => x.DrawText(drawOptions, textOptions, hp, new SolidBrush(Color.Black), null)
            );
            textOptions.Origin = new PointF(1741, 309);
            score.Mutate(
                x => x.DrawText(drawOptions, textOptions, hp, new SolidBrush(color), null)
            );
            // stars, version
            var starText = $"Stars: {star:0.##}";
            textOptions.Origin = new PointF(584, 292);
            score.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        starText,
                        new SolidBrush(Color.Black),
                        null
                    )
            );
            textOptions.Origin = new PointF(584, 289);
            score.Mutate(
                x => x.DrawText(drawOptions, textOptions, starText, new SolidBrush(color), null)
            );
            var version = "";
            foreach (char c in data.scoreInfo.Beatmap.Version!)
            {
                version += c;
                var m = TextMeasurer.MeasureSize(version, textOptions);
                if (m.Width > 140)
                {
                    version += "...";
                    break;
                }
            }
            textOptions.Origin = new PointF(584, 320);
            score.Mutate(
                x =>
                    x.DrawText(drawOptions, textOptions, version, new SolidBrush(Color.Black), null)
            );
            textOptions.Origin = new PointF(584, 317);
            score.Mutate(
                x =>
                    x.DrawText(drawOptions, textOptions, version, new SolidBrush(Color.White), null)
            );
            // avatar
            avatar.Mutate(x => x.Resize(80, 80).RoundCorner(new Size(80, 80), 40));
            score.Mutate(x => x.Fill(Color.White, new EllipsePolygon(80, 465, 85, 85)));
            score.Mutate(x => x.DrawImage(avatar, new Point(40, 425), 1));
            // username
            textOptions.Font = new Font(TorusSemiBold, 36);
            var username = data.scoreInfo.User!.Username;
            textOptions.Origin = new PointF(145, 470);
            score.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        username!,
                        new SolidBrush(Color.Black),
                        null
                    )
            );
            textOptions.Origin = new PointF(145, 467);
            score.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        username!,
                        new SolidBrush(Color.White),
                        null
                    )
            );
            // time
            textOptions.Font = new Font(TorusRegular, 27.61f);
            data.scoreInfo.CreatedAt.AddHours(8); //to UTC+8
            var time = data.scoreInfo.CreatedAt.ToString("yyyy/MM/dd HH:mm:ss");
            textOptions.Origin = new PointF(145, 505);
            score.Mutate(
                x => x.DrawText(drawOptions, textOptions, time, new SolidBrush(Color.Black), null)
            );
            textOptions.Origin = new PointF(145, 502);
            score.Mutate(
                x => x.DrawText(drawOptions, textOptions, time, new SolidBrush(Color.White), null)
            );

            // pp
            var ppTColor = Color.ParseHex("#cf93ae");
            var ppColor = Color.ParseHex("#fc65a9");
            textOptions.Font = new Font(TorusRegular, 33.5f);
            // aim, speed
            string pptext;
            if (ppInfo.ppStat.aim == null)
                pptext = "-";
            else
                pptext = ppInfo.ppStat.aim.Value.ToString("0");
            var metric = TextMeasurer.MeasureSize(pptext, textOptions);
            textOptions.Origin = new PointF(1532, 638);
            score.Mutate(
                x => x.DrawText(drawOptions, textOptions, pptext, new SolidBrush(ppColor), null)
            );
            textOptions.Origin = new PointF(1532 + metric.Width, 638);
            score.Mutate(
                x => x.DrawText(drawOptions, textOptions, "pp", new SolidBrush(ppTColor), null)
            );
            if (ppInfo.ppStat.speed == null)
                pptext = "-";
            else
                pptext = ppInfo.ppStat.speed.Value.ToString("0");
            metric = TextMeasurer.MeasureSize(pptext, textOptions);
            textOptions.Origin = new PointF(1672, 638);
            score.Mutate(
                x => x.DrawText(drawOptions, textOptions, pptext, new SolidBrush(ppColor), null)
            );
            textOptions.Origin = new PointF(1672 + metric.Width, 638);
            score.Mutate(
                x => x.DrawText(drawOptions, textOptions, "pp", new SolidBrush(ppTColor), null)
            );
            if (ppInfo.ppStat.acc == null)
                pptext = "-";
            else
                pptext = ppInfo.ppStat.acc.Value.ToString("0");
            metric = TextMeasurer.MeasureSize(pptext, textOptions);
            textOptions.Origin = new PointF(1812, 638);
            score.Mutate(
                x => x.DrawText(drawOptions, textOptions, pptext, new SolidBrush(ppColor), null)
            );
            textOptions.Origin = new PointF(1812 + metric.Width, 638);
            score.Mutate(
                x => x.DrawText(drawOptions, textOptions, "pp", new SolidBrush(ppTColor), null)
            );

            for (var i = 0; i < 5; i++)
            {
                try
                {
                    pptext = ppInfo.ppStats![5 - (i + 1)].total.ToString("0");
                }
                catch
                {
                    pptext = "-";
                }
                metric = TextMeasurer.MeasureSize(pptext, textOptions);
                textOptions.Origin = new PointF(50 + 139 * i, 638);
                score.Mutate(
                    x => x.DrawText(drawOptions, textOptions, pptext, new SolidBrush(ppColor), null)
                );
                textOptions.Origin = new PointF(50 + 139 * i + metric.Width, 638);
                score.Mutate(
                    x => x.DrawText(drawOptions, textOptions, "pp", new SolidBrush(ppTColor), null)
                );
            }

            // if fc
            textOptions.Font = new Font(TorusRegular, 24.5f);
            try
            {
                pptext = ppInfo.ppStats![5].total.ToString("0");
            }
            catch
            {
                pptext = "-";
            }
            metric = TextMeasurer.MeasureSize(pptext, textOptions);
            textOptions.Origin = new PointF(99, 562);
            score.Mutate(
                x => x.DrawText(drawOptions, textOptions, pptext, new SolidBrush(ppColor), null)
            );
            textOptions.Origin = new PointF(99 + metric.Width, 562);
            score.Mutate(
                x => x.DrawText(drawOptions, textOptions, "pp", new SolidBrush(ppTColor), null)
            );

            // total pp
            textOptions.Font = new Font(TorusRegular, 61f);
            pptext = Math.Round(ppInfo.ppStat.total).ToString("0");
            textOptions.HorizontalAlignment = HorizontalAlignment.Right;
            textOptions.Origin = new PointF(1825, 500);
            score.Mutate(
                x => x.DrawText(drawOptions, textOptions, pptext, new SolidBrush(ppColor), null)
            );
            textOptions.Origin = new PointF(1899, 500);
            score.Mutate(
                x => x.DrawText(drawOptions, textOptions, "pp", new SolidBrush(ppTColor), null)
            );

            // score
            textOptions.HorizontalAlignment = HorizontalAlignment.Center;
            textOptions.Font = new Font(TorusRegular, 40);
            textOptions.Origin = new PointF(980, 750);
            score.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        data.scoreInfo.Scores.ToString("N0"),
                        new SolidBrush(Color.White),
                        null
                    )
            );
            if (data.scoreInfo.Mode is Enums.Mode.Fruits)
            {
                textOptions.Font = new Font(TorusRegular, 40.00f);
                var great = data.scoreInfo.Statistics!.CountGreat.ToString();
                var ok = data.scoreInfo.Statistics.CountOk.ToString();
                var meh = data.scoreInfo.Statistics.CountMeh.ToString();
                var miss = data.scoreInfo.Statistics.CountMiss.ToString();

                // great
                textOptions.Origin = new PointF(790, 852);
                score.Mutate(
                    x =>
                        x.DrawText(
                            drawOptions,
                            textOptions,
                            great,
                            new SolidBrush(Color.Black),
                            null
                        )
                );
                textOptions.Origin = new PointF(790, 849);
                score.Mutate(
                    x =>
                        x.DrawText(
                            drawOptions,
                            textOptions,
                            great,
                            new SolidBrush(Color.White),
                            null
                        )
                );
                // ok
                textOptions.Origin = new PointF(790, 975);
                score.Mutate(
                    x => x.DrawText(drawOptions, textOptions, ok, new SolidBrush(Color.Black), null)
                );
                textOptions.Origin = new PointF(790, 972);
                score.Mutate(
                    x => x.DrawText(drawOptions, textOptions, ok, new SolidBrush(Color.White), null)
                );
                // meh
                textOptions.Origin = new PointF(1152, 852);
                score.Mutate(
                    x =>
                        x.DrawText(drawOptions, textOptions, meh, new SolidBrush(Color.Black), null)
                );
                textOptions.Origin = new PointF(1152, 849);
                score.Mutate(
                    x =>
                        x.DrawText(drawOptions, textOptions, meh, new SolidBrush(Color.White), null)
                );
                // miss
                textOptions.Origin = new PointF(1152, 975);
                score.Mutate(
                    x =>
                        x.DrawText(
                            drawOptions,
                            textOptions,
                            miss,
                            new SolidBrush(Color.Black),
                            null
                        )
                );
                textOptions.Origin = new PointF(1152, 972);
                score.Mutate(
                    x =>
                        x.DrawText(
                            drawOptions,
                            textOptions,
                            miss,
                            new SolidBrush(Color.White),
                            null
                        )
                );
            }
            else if (data.scoreInfo.Mode is Enums.Mode.Mania)
            {
                textOptions.Font = new Font(TorusRegular, 35.00f);
                var great = data.scoreInfo.Statistics!.CountGreat.ToString();
                var ok = data.scoreInfo.Statistics.CountOk.ToString();
                var meh = data.scoreInfo.Statistics.CountMeh.ToString();
                var miss = data.scoreInfo.Statistics.CountMiss.ToString();
                var geki = data.scoreInfo.Statistics.CountGeki.ToString();
                var katu = data.scoreInfo.Statistics.CountKatu.ToString();

                // great
                textOptions.Origin = new PointF(790, 837);
                score.Mutate(
                    x =>
                        x.DrawText(
                            drawOptions,
                            textOptions,
                            great,
                            new SolidBrush(Color.Black),
                            null
                        )
                );
                textOptions.Origin = new PointF(790, 834);
                score.Mutate(
                    x =>
                        x.DrawText(
                            drawOptions,
                            textOptions,
                            great,
                            new SolidBrush(Color.White),
                            null
                        )
                );
                // geki
                textOptions.Origin = new PointF(1156, 837);
                score.Mutate(
                    x =>
                        x.DrawText(
                            drawOptions,
                            textOptions,
                            geki,
                            new SolidBrush(Color.Black),
                            null
                        )
                );
                textOptions.Origin = new PointF(1156, 834);
                score.Mutate(
                    x =>
                        x.DrawText(
                            drawOptions,
                            textOptions,
                            geki,
                            new SolidBrush(Color.White),
                            null
                        )
                );
                // katu
                textOptions.Origin = new PointF(790, 910);
                score.Mutate(
                    x =>
                        x.DrawText(
                            drawOptions,
                            textOptions,
                            katu,
                            new SolidBrush(Color.Black),
                            null
                        )
                );
                textOptions.Origin = new PointF(790, 907);
                score.Mutate(
                    x =>
                        x.DrawText(
                            drawOptions,
                            textOptions,
                            katu,
                            new SolidBrush(Color.White),
                            null
                        )
                );
                // ok
                textOptions.Origin = new PointF(1156, 910);
                score.Mutate(
                    x => x.DrawText(drawOptions, textOptions, ok, new SolidBrush(Color.Black), null)
                );
                textOptions.Origin = new PointF(1156, 907);
                score.Mutate(
                    x => x.DrawText(drawOptions, textOptions, ok, new SolidBrush(Color.White), null)
                );
                // meh
                textOptions.Origin = new PointF(790, 985);
                score.Mutate(
                    x =>
                        x.DrawText(drawOptions, textOptions, meh, new SolidBrush(Color.Black), null)
                );
                textOptions.Origin = new PointF(790, 982);
                score.Mutate(
                    x =>
                        x.DrawText(drawOptions, textOptions, meh, new SolidBrush(Color.White), null)
                );
                // miss
                textOptions.Origin = new PointF(1156, 985);
                score.Mutate(
                    x =>
                        x.DrawText(
                            drawOptions,
                            textOptions,
                            miss,
                            new SolidBrush(Color.Black),
                            null
                        )
                );
                textOptions.Origin = new PointF(1156, 982);
                score.Mutate(
                    x =>
                        x.DrawText(
                            drawOptions,
                            textOptions,
                            miss,
                            new SolidBrush(Color.White),
                            null
                        )
                );
            }
            else
            {
                textOptions.Font = new Font(TorusRegular, 53.09f);
                var great = data.scoreInfo.Statistics!.CountGreat.ToString();
                var ok = data.scoreInfo.Statistics.CountOk.ToString();
                var meh = data.scoreInfo.Statistics.CountMeh.ToString();
                var miss = data.scoreInfo.Statistics.CountMiss.ToString();

                // great
                textOptions.Origin = new PointF(795, 857);
                score.Mutate(
                    x =>
                        x.DrawText(
                            drawOptions,
                            textOptions,
                            great,
                            new SolidBrush(Color.Black),
                            null
                        )
                );
                textOptions.Origin = new PointF(795, 854);
                score.Mutate(
                    x =>
                        x.DrawText(
                            drawOptions,
                            textOptions,
                            great,
                            new SolidBrush(Color.White),
                            null
                        )
                );
                // ok
                textOptions.Origin = new PointF(795, 985);
                score.Mutate(
                    x => x.DrawText(drawOptions, textOptions, ok, new SolidBrush(Color.Black), null)
                );
                textOptions.Origin = new PointF(795, 982);
                score.Mutate(
                    x => x.DrawText(drawOptions, textOptions, ok, new SolidBrush(Color.White), null)
                );
                // meh
                textOptions.Origin = new PointF(1154, 857);
                score.Mutate(
                    x =>
                        x.DrawText(drawOptions, textOptions, meh, new SolidBrush(Color.Black), null)
                );
                textOptions.Origin = new PointF(1154, 854);
                score.Mutate(
                    x =>
                        x.DrawText(drawOptions, textOptions, meh, new SolidBrush(Color.White), null)
                );
                // miss
                textOptions.Origin = new PointF(1154, 985);
                score.Mutate(
                    x =>
                        x.DrawText(
                            drawOptions,
                            textOptions,
                            miss,
                            new SolidBrush(Color.Black),
                            null
                        )
                );
                textOptions.Origin = new PointF(1154, 982);
                score.Mutate(
                    x =>
                        x.DrawText(
                            drawOptions,
                            textOptions,
                            miss,
                            new SolidBrush(Color.White),
                            null
                        )
                );
            }

            // acc
            textOptions.Font = new Font(TorusRegular, 53.56f);
            var acc = data.scoreInfo.Accuracy * 100f;
            var hsl = new Hsl(150, 1, 1);
            // ("#ffbd1f") idk?
            color = Color.ParseHex("#87ff6a");
            textOptions.Origin = new PointF(360, 966);
            score.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        $"{acc:0.0#}%",
                        new SolidBrush(Color.Black),
                        null
                    )
            );
            using var acchue = new Image<Rgba32>(1950 - 2, 1088);
            var hue = acc < 60 ? 260f : (acc - 60) * 2 + 280f;
            textOptions.Origin = new PointF(360, 963);
            acchue.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        $"{acc:0.0#}%",
                        new SolidBrush(color),
                        null
                    )
            );
            acchue.Mutate(x => x.Hue(((float)hue)));
            score.Mutate(x => x.DrawImage(acchue, 1));
            // combo
            var combo = data.scoreInfo.MaxCombo;
            if (ppInfo.maxCombo != null)
            {
                var maxCombo = ppInfo.maxCombo.Value;
                if (maxCombo > 0)
                {
                    textOptions.Origin = new PointF(1598, 966);
                    score.Mutate(
                        x =>
                            x.DrawText(
                                drawOptions,
                                textOptions,
                                " / ",
                                new SolidBrush(Color.Black),
                                null
                            )
                    );
                    textOptions.Origin = new PointF(1598, 963);
                    score.Mutate(
                        x =>
                            x.DrawText(
                                drawOptions,
                                textOptions,
                                " / ",
                                new SolidBrush(Color.White),
                                null
                            )
                    );
                    textOptions.HorizontalAlignment = HorizontalAlignment.Left;
                    textOptions.Origin = new PointF(1607, 966);
                    score.Mutate(
                        x =>
                            x.DrawText(
                                drawOptions,
                                textOptions,
                                $"{maxCombo}x",
                                new SolidBrush(Color.Black),
                                null
                            )
                    );
                    textOptions.Origin = new PointF(1607, 963);
                    score.Mutate(
                        x =>
                            x.DrawText(
                                drawOptions,
                                textOptions,
                                $"{maxCombo}x",
                                new SolidBrush(color),
                                null
                            )
                    );
                    textOptions.HorizontalAlignment = HorizontalAlignment.Right;
                    textOptions.Origin = new PointF(1588, 966);
                    score.Mutate(
                        x =>
                            x.DrawText(
                                drawOptions,
                                textOptions,
                                $"{combo}x",
                                new SolidBrush(Color.Black),
                                null
                            )
                    );
                    using var combohue = new Image<Rgba32>(1950 - 2, 1088);
                    hue = (((float)combo / (float)maxCombo) * 100) + 260;
                    textOptions.Origin = new PointF(1588, 963);
                    combohue.Mutate(
                        x =>
                            x.DrawText(
                                drawOptions,
                                textOptions,
                                $"{combo}x",
                                new SolidBrush(color),
                                null
                            )
                    );
                    combohue.Mutate(x => x.Hue(((float)hue)));
                    score.Mutate(x => x.DrawImage(combohue, 1));
                }
                else
                {
                    textOptions.HorizontalAlignment = HorizontalAlignment.Center;
                    textOptions.Origin = new PointF(1598, 966);
                    score.Mutate(
                        x =>
                            x.DrawText(
                                drawOptions,
                                textOptions,
                                $"{combo}x",
                                new SolidBrush(Color.Black),
                                null
                            )
                    );
                    textOptions.Origin = new PointF(1598, 963);
                    score.Mutate(
                        x =>
                            x.DrawText(
                                drawOptions,
                                textOptions,
                                $"{combo}x",
                                new SolidBrush(color),
                                null
                            )
                    );
                }
            }
            else
            {
                textOptions.HorizontalAlignment = HorizontalAlignment.Center;
                textOptions.Origin = new PointF(1598, 966);
                score.Mutate(
                    x =>
                        x.DrawText(
                            drawOptions,
                            textOptions,
                            $"{combo}x",
                            new SolidBrush(Color.Black),
                            null
                        )
                );
                textOptions.Origin = new PointF(1598, 963);
                score.Mutate(
                    x =>
                        x.DrawText(
                            drawOptions,
                            textOptions,
                            $"{combo}x",
                            new SolidBrush(color),
                            null
                        )
                );
            }
            return score;
        }
    }
}
