using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Img = SixLabors.ImageSharp.Image;

namespace KanonBot.Image.OSU
{
    public class Hexagram
    {
        public struct R8
        {
            public required double r,
                _8;
        }

        public struct HexagramInfo
        {
            public required int size,
                nodeCount,
                nodeMaxValue,
                sideLength,
                mode;
            public required float strokeWidth;
            public required SizeF nodesize;
            public Color abilityFillColor,
                abilityLineColor;
        }

        // 极坐标转直角坐标系
        public static PointF r82xy(R8 r8)
        {
            PointF xy = new()
            {
                X = (float)(r8.r * Math.Sin(r8._8 * Math.PI / 180)),
                Y = (float)(r8.r * Math.Cos(r8._8 * Math.PI / 180))
            };
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
                var r =
                    Math.Pow((multi[i] * Math.Pow(ppd[i], exp[i]) / hi.nodeMaxValue), 0.8)
                    * hi.size
                    / 2.0;
                if (hi.mode == 1 && r > 100.00)
                    r = 100.00;
                if (hi.mode == 2 && r > 395.00)
                    r = 395.00;
                if (hi.mode == 3 && r > 495.00)
                    r = 495.00;
                R8 r8 = new() { r = r, _8 = 360.0 / hi.nodeCount * i + 90 };
                var xy = r82xy(r8);
                xy.X += hi.size / 2;
                xy.Y += hi.size / 2;
                points[i] = xy;
                xy.X += hi.nodesize.Width / 10;
                xy.Y += hi.nodesize.Height / 10;
                image.Mutate(
                    x => x.Fill(hi.abilityLineColor, new EllipsePolygon(xy, hi.nodesize))
                );
            }
            image.Mutate(
                x =>
                    x.DrawPolygon(hi.abilityLineColor, hi.strokeWidth, points)
                        .FillPolygon(hi.abilityFillColor, points)
            );
            return image;
        }
    }
}
