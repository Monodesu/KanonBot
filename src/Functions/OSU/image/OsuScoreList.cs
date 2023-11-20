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
using KanonBot.Image;
using static KanonBot.Image.OSU.OsuInfoPanelV2;
using static KanonBot.Image.OSU.OsuResourceHelper;

namespace KanonBot.Image.OSU
{
    public static class OsuScoreList
    {
        public enum Type
        {
            TODAYBP,
            BPLIST,
            RECENTLIST
        }

        public static async Task<Img> Draw(
            Type type,
            List<API.OSU.Models.Score> TBP,
            List<int> Rank,
            API.OSU.Models.User userInfo
            )
        {
            //设定textOption/drawOption
            var textOptions = new RichTextOptions(new Font(TorusSemiBold, 120))
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            var drawOptions = new DrawingOptions
            {
                GraphicsOptions = new GraphicsOptions { Antialias = true }
            };

            //页眉 2000x697
            //页中 2000x186
            //页脚 2000x70

            //计算图像大小并生成原始图像
            Img image;
            if (TBP.Count > 1)
            {
                var t = 70 + 697 + 186 * (TBP.Count - 1);
                image = new Image<Rgba32>(2000, t);
            }
            else
            {
                image = new Image<Rgba32>(2000, 767);
            }

            image.Mutate(x => x.Fill(Color.White));

            //绘制页眉
            string MainPicPath = "";
            switch (type)
            {
                case Type.TODAYBP:
                    MainPicPath = "./work/panelv2/tbp_main_score.png";
                    break;
                case Type.BPLIST:
                    MainPicPath = "./work/panelv2/bplist_main_score.png";
                    break;
            }
            using var MainPic = await ReadImageRgba(MainPicPath);

            //绘制beatmap图像
            var scorebgPath = $"./work/background/{TBP[0].Beatmap!.BeatmapId}.png";
            if (!File.Exists(scorebgPath))
            {
                try
                {
                    scorebgPath = await API.OSU.V2.SayoDownloadBeatmapBackgroundImg(
                        TBP[0].Beatmapset!.Id,
                        TBP[0].Beatmap!.BeatmapId,
                        "./work/background/"
                    );
                }
                catch (Exception ex)
                {
                    var msg = $"从API下载背景图片时发生了一处异常\n异常类型: {ex.GetType()}\n异常信息: '{ex.Message}'";
                    Log.Warning(msg);
                }
            }

            using var scorebg = await TryAsync(ReadImageRgba(scorebgPath!))
                .IfFail(await ReadImageRgba("./work/legacy/load-failed-img.png"));

            scorebg.Mutate(
                x =>
                    x.Resize(new ResizeOptions() { Size = new Size(365, 0), Mode = ResizeMode.Max })
            );

            using var bgtemp = new Image<Rgba32>(365, 210);
            bgtemp.Mutate(x => x.DrawImage(scorebg, new Point(0, 0), 1));
            image.Mutate(x => x.DrawImage(bgtemp, new Point(92, 433), 1));
            image.Mutate(x => x.DrawImage(MainPic, new Point(0, 0), 1));

            //头像、用户名、PP
            var avatarPath = $"./work/avatar/{userInfo.Id}.png";
            using var avatar = await TryAsync(ReadImageRgba(avatarPath))
                .IfFail(async () =>
                {
                    try
                    {
                        avatarPath = await userInfo.AvatarUrl.DownloadFileAsync(
                            "./work/avatar/",
                            $"{userInfo.Id}.png"
                        );
                    }
                    catch (Exception ex)
                    {
                        var msg = $"从API下载用户头像时发生了一处异常\n异常类型: {ex.GetType()}\n异常信息: '{ex.Message}'";
                        Log.Error(msg);
                        throw; // 下载失败直接抛出error
                    }
                    return await ReadImageRgba(avatarPath); // 下载后再读取
                });
            avatar.Mutate(x => x.Resize(160, 160).RoundCorner(new Size(160, 160), 25));
            image.Mutate(x => x.DrawImage(avatar, new Point(56, 60), 1));
            //username
            textOptions.Origin = new PointF(256, 195);
            textOptions.Font = new Font(TorusSemiBold, 100);
            image.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        userInfo.Username!,
                        new SolidBrush(Color.ParseHex("#4d4d4d")),
                        null
                    )
            );
            //pp
            textOptions.HorizontalAlignment = HorizontalAlignment.Left;
            textOptions.Origin = new PointF(1782, 168);
            textOptions.Font = new Font(TorusRegular, 58);
            image.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        string.Format("{0:N0}", userInfo.Statistics!.PP),
                        new SolidBrush(Color.ParseHex("#e36a79")),
                        null
                    )
            );

            //绘制页眉的信息 496x585
            //title  +mods
            textOptions.Font = new Font(TorusRegular, 90);
            textOptions.Origin = new PointF(485, 540);
            var title = "";
            foreach (char c in TBP[0].Beatmapset!.Title!)
            {
                title += c;
                var m = TextMeasurer.MeasureSize(title, textOptions);
                if (m.Width > 725)
                {
                    title += "...";
                    break;
                }
            }
            image.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        title,
                        new SolidBrush(Color.ParseHex("#656b6d")),
                        null
                    )
            );
            //mods
            if (TBP[0].Mods!.Length > 0)
            {
                textOptions.Origin = new PointF(
                    485 + TextMeasurer.MeasureSize(title, textOptions).Width + 25,
                    530
                );
                textOptions.Font = new Font(TorusRegular, 40);
                var mainscoremods = "+";
                foreach (var x in TBP[0].Mods!)
                    mainscoremods += $"{x}, ";
                image.Mutate(
                    x =>
                        x.DrawText(
                            drawOptions,
                            textOptions,
                            mainscoremods[..mainscoremods.LastIndexOf(",")] + $" #{Rank[0]}",
                            new SolidBrush(Color.ParseHex("#656b6d")),
                            null
                        )
                );
            }
            else
            {
                textOptions.Origin = new PointF(
                    485 + TextMeasurer.MeasureSize(title, textOptions).Width + 25,
                    530
                );
                textOptions.Font = new Font(TorusRegular, 40);
                image.Mutate(
                    x =>
                        x.DrawText(
                            drawOptions,
                            textOptions,
                            $"#{Rank[0]}",
                            new SolidBrush(Color.ParseHex("#656b6d")),
                            null
                        )
                );
            }

            int mainScoreXPos = 585;
            //artist
            textOptions.Font = new Font(TorusRegular, 38);
            textOptions.Origin = new PointF(495, mainScoreXPos);
            var artist = "";
            foreach (char c in TBP[0].Beatmapset!.Artist!)
            {
                artist += c;
                var m = TextMeasurer.MeasureSize(artist, textOptions);
                if (m.Width > 205)
                {
                    artist += "...";
                    break;
                }
            }
            image.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        artist,
                        new SolidBrush(Color.ParseHex("#656b6d")),
                        null
                    )
            );

            //creator
            textOptions.Origin = new PointF(769, mainScoreXPos);
            var creator = "";
            foreach (char c in TBP[0].Beatmapset!.Creator!)
            {
                creator += c;
                var m = TextMeasurer.MeasureSize(creator, textOptions);
                if (m.Width > 145)
                {
                    creator += "...";
                    break;
                }
            }
            image.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        creator,
                        new SolidBrush(Color.ParseHex("#656b6d")),
                        null
                    )
            );

            //bid
            textOptions.Origin = new PointF(985, mainScoreXPos);
            image.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        TBP[0].Beatmap!.BeatmapId.ToString(),
                        new SolidBrush(Color.ParseHex("#656b6d")),
                        null
                    )
            );

            //get stars from rosupp
            var ppinfo = await PerformanceCalculator.CalculatePanelData(TBP[0]);
            textOptions.Origin = new PointF(1182, mainScoreXPos);
            image.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        ppinfo.ppInfo.star.ToString("0.##*"),
                        new SolidBrush(Color.ParseHex("#656b6d")),
                        null
                    )
            );

            //acc
            textOptions.Origin = new PointF(1308, mainScoreXPos);
            image.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        TBP[0].Accuracy.ToString("0.##%"),
                        new SolidBrush(Color.ParseHex("#656b6d")),
                        null
                    )
            );

            //rank
            textOptions.Origin = new PointF(1459, mainScoreXPos);
            image.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        TBP[0].Rank!,
                        new SolidBrush(Color.ParseHex("#656b6d")),
                        null
                    )
            );

            //pp
            textOptions.Font = new Font(TorusRegular, 90);
            textOptions.HorizontalAlignment = HorizontalAlignment.Center;
            textOptions.Origin = new PointF(1790, 608);
            image.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        string.Format("{0:N1}", TBP[0].PP),
                        new SolidBrush(Color.ParseHex("#364a75")),
                        null
                    )
            );
            var bp1pptextMeasure = TextMeasurer.MeasureSize(
                string.Format("{0:N1}", TBP[0].PP),
                textOptions
            );
            int bp1pptextpos = 1790 - (int)bp1pptextMeasure.Width / 2;
            textOptions.Font = new Font(TorusRegular, 40);
            textOptions.Origin = new PointF(bp1pptextpos, 522);
            textOptions.HorizontalAlignment = HorizontalAlignment.Left;
            image.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        "pp",
                        new SolidBrush(Color.ParseHex("#656b6d")),
                        null
                    )
            );

            //页中
            for (int i = 1; i < TBP.Count; ++i)
            {
                using var SubPic = await ReadImageRgba("./work/panelv2/score_list.png");
                using var osuscoremode_icon = await ReadImageRgba(
                    $"./work/panelv2/icons/mode_icon/score/{TBP[i].Mode.ToStr()}.png"
                );
                var ppinfo1 = await PerformanceCalculator.CalculatePanelData(TBP[i]);
                //Difficulty icon
                Color modeC = ForStarDifficulty(ppinfo1.ppInfo.star);
                osuscoremode_icon.Mutate(x => x.Resize(92, 92));
                osuscoremode_icon.Mutate(
                    x =>
                        x.ProcessPixelRowsAsVector4(row =>
                        {
                            for (int p = 0; p < row.Length; p++)
                            {
                                row[p].X = ((Vector4)modeC).X;
                                row[p].Y = ((Vector4)modeC).Y;
                                row[p].Z = ((Vector4)modeC).Z;
                            }
                        })
                );
                SubPic.Mutate(x => x.DrawImage(osuscoremode_icon, new Point(92, 48), 1));

                //main title
                textOptions.HorizontalAlignment = HorizontalAlignment.Left;
                textOptions.Font = new Font(TorusRegular, 50);
                title = "";
                foreach (char c in TBP[i].Beatmapset!.Title!)
                {
                    title += c;
                    var m = TextMeasurer.MeasureSize(title, textOptions);
                    if (m.Width > 710)
                    {
                        title += "...";
                        break;
                    }
                }
                textOptions.Origin = new PointF(204, 96);
                SubPic.Mutate(
                    x =>
                        x.DrawText(
                            drawOptions,
                            textOptions,
                            title,
                            new SolidBrush(Color.ParseHex("#656b6d")),
                            null
                        )
                );
                //Rank
                textOptions.Font = new Font(TorusRegular, 34);
                textOptions.Origin = new PointF(204, 138);
                SubPic.Mutate(
                    x =>
                        x.DrawText(
                            drawOptions,
                            textOptions,
                            $"#{Rank[i]}",
                            new SolidBrush(Color.ParseHex("#656b6d")),
                            null
                        )
                );
                var textMeasurePos =
                    204 + TextMeasurer.MeasureSize($"#{Rank[i]}", textOptions).Width + 5;

                //split
                textOptions.Origin = new PointF(textMeasurePos, 138);
                SubPic.Mutate(
                    x =>
                        x.DrawText(
                            drawOptions,
                            textOptions,
                            " | ",
                            new SolidBrush(Color.ParseHex("#656b6d")),
                            null
                        )
                );
                textMeasurePos =
                    textMeasurePos + TextMeasurer.MeasureSize(" | ", textOptions).Width + 5;

                //version
                title = "";
                foreach (char c in TBP[i].Beatmap!.Version!)
                {
                    title += c;
                    var m = TextMeasurer.MeasureSize(title, textOptions);
                    if (m.Width > 130)
                    {
                        title += "...";
                        break;
                    }
                }
                textOptions.Origin = new PointF(textMeasurePos, 138);
                SubPic.Mutate(
                    x =>
                        x.DrawText(
                            drawOptions,
                            textOptions,
                            title,
                            new SolidBrush(Color.ParseHex("#656b6d")),
                            null
                        )
                );
                textMeasurePos =
                    textMeasurePos + TextMeasurer.MeasureSize(title, textOptions).Width + 5;

                //split
                textOptions.Origin = new PointF(textMeasurePos, 138);
                SubPic.Mutate(
                    x =>
                        x.DrawText(
                            drawOptions,
                            textOptions,
                            " | ",
                            new SolidBrush(Color.ParseHex("#656b6d")),
                            null
                        )
                );
                textMeasurePos =
                    textMeasurePos + TextMeasurer.MeasureSize(" | ", textOptions).Width + 5;

                //bid
                textOptions.Origin = new PointF(textMeasurePos, 138);
                SubPic.Mutate(
                    x =>
                        x.DrawText(
                            drawOptions,
                            textOptions,
                            TBP[i].Beatmap!.BeatmapId.ToString(),
                            new SolidBrush(Color.ParseHex("#656b6d")),
                            null
                        )
                );
                textMeasurePos =
                    textMeasurePos
                    + TextMeasurer.MeasureSize(TBP[i].Beatmap!.BeatmapId.ToString(), textOptions).Width
                    + 5;

                //split
                textOptions.Origin = new PointF(textMeasurePos, 138);
                SubPic.Mutate(
                    x =>
                        x.DrawText(
                            drawOptions,
                            textOptions,
                            " | ",
                            new SolidBrush(Color.ParseHex("#656b6d")),
                            null
                        )
                );
                textMeasurePos =
                    textMeasurePos + TextMeasurer.MeasureSize(" | ", textOptions).Width + 5;

                //star
                textOptions.Origin = new PointF(textMeasurePos, 138);
                SubPic.Mutate(
                    x =>
                        x.DrawText(
                            drawOptions,
                            textOptions,
                            ppinfo1.ppInfo.star.ToString("0.##*"),
                            new SolidBrush(Color.ParseHex("#656b6d")),
                            null
                        )
                );
                textMeasurePos =
                    textMeasurePos
                    + TextMeasurer.MeasureSize(ppinfo1.ppInfo.star.ToString("0.##*"), textOptions).Width
                    + 5;

                //split
                textOptions.Origin = new PointF(textMeasurePos, 138);
                SubPic.Mutate(
                    x =>
                        x.DrawText(
                            drawOptions,
                            textOptions,
                            " | ",
                            new SolidBrush(Color.ParseHex("#656b6d")),
                            null
                        )
                );
                textMeasurePos =
                    textMeasurePos + TextMeasurer.MeasureSize(" | ", textOptions).Width + 5;

                //acc
                textOptions.Origin = new PointF(textMeasurePos, 138);
                SubPic.Mutate(
                    x =>
                        x.DrawText(
                            drawOptions,
                            textOptions,
                            TBP[i].Accuracy.ToString("0.##%"),
                            new SolidBrush(Color.ParseHex("#ffcd22")),
                            null
                        )
                );
                textMeasurePos =
                    textMeasurePos
                    + TextMeasurer.MeasureSize(TBP[i].Accuracy.ToString("0.##%"), textOptions).Width
                    + 5;

                //split
                textOptions.Origin = new PointF(textMeasurePos, 138);
                SubPic.Mutate(
                    x =>
                        x.DrawText(
                            drawOptions,
                            textOptions,
                            " | ",
                            new SolidBrush(Color.ParseHex("#656b6d")),
                            null
                        )
                );
                textMeasurePos =
                    textMeasurePos + TextMeasurer.MeasureSize(" | ", textOptions).Width + 5;

                //ranking
                textOptions.Origin = new PointF(textMeasurePos, 138);
                SubPic.Mutate(
                    x =>
                        x.DrawText(
                            drawOptions,
                            textOptions,
                            TBP[i].Rank!,
                            new SolidBrush(Color.ParseHex("#656b6d")),
                            null
                        )
                );

                //mods
                if (TBP[i].Mods!.Length > 0)
                {
                    var mods_pos_x = 1043;
                    if (TBP[i].Mods!.Length > 6)
                    {
                        //大于6个
                        foreach (var x in TBP[i].Mods!)
                        {
                            using var modicon = await Img.LoadAsync($"./work/mods_v2/2x/{x}.png");
                            modicon.Mutate(x => x.Resize(90, 90));
                            SubPic.Mutate(x => x.DrawImage(modicon, new Point(mods_pos_x, 48), 1));
                            mods_pos_x += 70 - (TBP[i].Mods!.Length - 7) * 9;
                        }
                    }
                    else if (TBP[i].Mods!.Length > 5)
                    {
                        //等于6个
                        foreach (var x in TBP[i].Mods!)
                        {
                            using var modicon = await Img.LoadAsync($"./work/mods_v2/2x/{x}.png");
                            modicon.Mutate(x => x.Resize(90, 90));
                            SubPic.Mutate(x => x.DrawImage(modicon, new Point(mods_pos_x, 48), 1));
                            mods_pos_x += 84;
                        }
                    }
                    else
                    {
                        //小于6个
                        foreach (var x in TBP[i].Mods!)
                        {
                            using var modicon = await Img.LoadAsync($"./work/mods_v2/2x/{x}.png");
                            modicon.Mutate(x => x.Resize(90, 90));
                            SubPic.Mutate(x => x.DrawImage(modicon, new Point(mods_pos_x, 48), 1));
                            mods_pos_x += 105;
                        }
                    }
                }

                //pp
                textOptions.Font = new Font(TorusRegular, 70);
                textOptions.HorizontalAlignment = HorizontalAlignment.Center;
                textOptions.Origin = new PointF(1790, 128);
                SubPic.Mutate(
                    x =>
                        x.DrawText(
                            drawOptions,
                            textOptions,
                            string.Format("{0:N0}pp", TBP[i].PP),
                            new SolidBrush(Color.ParseHex("#ff7bac")),
                            null
                        )
                );

                //draw
                image.Mutate(x => x.DrawImage(SubPic, new Point(0, 698 + (i - 1) * 186 + 1), 1));
            }
            //页尾
            using var FooterPic = await ReadImageRgba("./work/panelv2/score_list_footer.png");
            image.Mutate(
                x => x.DrawImage(FooterPic, new Point(0, 698 + (TBP.Count - 1) * 186 + 1), 1)
            );
            return image;
        }
    }
}