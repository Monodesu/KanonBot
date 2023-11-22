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
using static KanonBot.Image.OSU.OsuInfoPanelV1;
using static KanonBot.Image.OSU.OsuResourceHelper;
using static KanonBot.Image.OSU.ResourceRegistrar;
using Img = SixLabors.ImageSharp.Image;
using ResizeOptions = SixLabors.ImageSharp.Processing.ResizeOptions;

namespace KanonBot.Image.OSU
{
    public static class OsuPPPVs
    {
        public static async Task<Img> Draw(PPVSPanelData data)
        {
            var ppvsImg = await Img.LoadAsync("work/legacy/ppvs.png");
            Hexagram.HexagramInfo hi =
                new()
                {
                    nodeCount = 6,
                    nodeMaxValue = 12000,
                    size = 1134,
                    sideLength = 791,
                    mode = 2,
                    strokeWidth = 6f,
                    nodesize = new SizeF(15f, 15f)
                };
            // hi.abilityLineColor = Color.ParseHex("#FF7BAC");
            var multi = new double[6] { 14.1, 69.7, 1.92, 19.8, 0.588, 3.06 };
            var exp = new double[6] { 0.769, 0.596, 0.953, 0.8, 1.175, 0.993 };
            var u1d = new int[6];
            u1d[0] = (int)data.u1!.AccuracyTotal;
            u1d[1] = (int)data.u1.FlowAimTotal;
            u1d[2] = (int)data.u1.JumpAimTotal;
            u1d[3] = (int)data.u1.PrecisionTotal;
            u1d[4] = (int)data.u1.SpeedTotal;
            u1d[5] = (int)data.u1.StaminaTotal;
            var u2d = new int[6];
            u2d[0] = (int)data.u2!.AccuracyTotal;
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
                using var tmp1 = Hexagram.Draw(u1d, multi, exp, hi);
                ppvsImg.Mutate(x => x.DrawImage(tmp1, new Point(0, -120), 1));
                hi.abilityFillColor = Color.FromRgba(41, 171, 226, 50);
                hi.abilityLineColor = Color.FromRgba(41, 171, 226, 255);
                using var tmp2 = Hexagram.Draw(u2d, multi, exp, hi);
                ppvsImg.Mutate(x => x.DrawImage(tmp2, new Point(0, -120), 1));
            }
            else
            {
                hi.abilityFillColor = Color.FromRgba(41, 171, 226, 50);
                hi.abilityLineColor = Color.FromRgba(41, 171, 226, 255);
                using var tmp1 = Hexagram.Draw(u2d, multi, exp, hi);
                ppvsImg.Mutate(x => x.DrawImage(tmp1, new Point(0, -120), 1));
                hi.abilityFillColor = Color.FromRgba(255, 123, 172, 50);
                hi.abilityLineColor = Color.FromRgba(255, 123, 172, 255);
                using var tmp2 = Hexagram.Draw(u1d, multi, exp, hi);
                ppvsImg.Mutate(x => x.DrawImage(tmp2, new Point(0, -120), 1));
            }

            // text
            var drawOptions = new DrawingOptions
            {
                GraphicsOptions = new GraphicsOptions { Antialias = true }
            };

            // 打印用户名
            var font = new Font(avenirLTStdMedium, 36);
            var textOptions = new RichTextOptions(font)
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Left,
                Origin = new PointF(808, 888)
            };
            var color = Color.ParseHex("#999999");
            ppvsImg.Mutate(
                x => x.DrawText(drawOptions, textOptions, data.u1Name!, new SolidBrush(color), null)
            );
            textOptions.Origin = new PointF(264, 888);
            ppvsImg.Mutate(
                x => x.DrawText(drawOptions, textOptions, data.u2Name!, new SolidBrush(color), null)
            );

            // 打印每个用户数据
            var y_offset = new int[6] { 1485, 1150, 1066, 1234, 1318, 1403 }; // pp+数据的y轴坐标
            font = new Font(avenirLTStdMedium, 32);
            textOptions = new RichTextOptions(font)
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Left,
            };
            for (var i = 0; i < u1d.Length; i++)
            {
                textOptions.Origin = new PointF(664, y_offset[i]);
                ppvsImg.Mutate(
                    x =>
                        x.DrawText(
                            drawOptions,
                            textOptions,
                            u1d[i].ToString(),
                            new SolidBrush(color),
                            null
                        )
                );
            }
            textOptions.Origin = new PointF(664, 980);
            ppvsImg.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        data.u1.PerformanceTotal.ToString("0.##"),
                        new SolidBrush(color),
                        null
                    )
            );
            for (var i = 0; i < u2d.Length; i++)
            {
                textOptions.Origin = new PointF(424, y_offset[i]);
                ppvsImg.Mutate(
                    x =>
                        x.DrawText(
                            drawOptions,
                            textOptions,
                            u2d[i].ToString(),
                            new SolidBrush(color),
                            null
                        )
                );
            }
            textOptions.Origin = new PointF(424, 980);
            ppvsImg.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        data.u2.PerformanceTotal.ToString("0.##"),
                        new SolidBrush(color),
                        null
                    )
            );

            // 打印数据差异
            var diffPoint = 960;
            color = Color.ParseHex("#ffcd22");
            textOptions.Origin = new PointF(diffPoint, 980);
            ppvsImg.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        string.Format(
                            "{0:0}",
                            (data.u2.PerformanceTotal - data.u1.PerformanceTotal)
                        ),
                        new SolidBrush(color),
                        null
                    )
            );
            textOptions.Origin = new PointF(diffPoint, 1066);
            ppvsImg.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        (u2d[2] - u1d[2]).ToString(),
                        new SolidBrush(color),
                        null
                    )
            );
            textOptions.Origin = new PointF(diffPoint, 1150);
            ppvsImg.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        (u2d[1] - u1d[1]).ToString(),
                        new SolidBrush(color),
                        null
                    )
            );
            textOptions.Origin = new PointF(diffPoint, 1234);
            ppvsImg.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        (u2d[3] - u1d[3]).ToString(),
                        new SolidBrush(color),
                        null
                    )
            );
            textOptions.Origin = new PointF(diffPoint, 1318);
            ppvsImg.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        (u2d[4] - u1d[4]).ToString(),
                        new SolidBrush(color),
                        null
                    )
            );
            textOptions.Origin = new PointF(diffPoint, 1403);
            ppvsImg.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        (u2d[5] - u1d[5]).ToString(),
                        new SolidBrush(color),
                        null
                    )
            );
            textOptions.Origin = new PointF(diffPoint, 1485);
            ppvsImg.Mutate(
                x =>
                    x.DrawText(
                        drawOptions,
                        textOptions,
                        (u2d[0] - u1d[0]).ToString(),
                        new SolidBrush(color),
                        null
                    )
            );

            using var title = await Img.LoadAsync($"work/legacy/ppvs_title.png");
            ppvsImg.Mutate(x => x.DrawImage(title, new Point(0, 0), 1));

            return ppvsImg;
        }
    }
}
