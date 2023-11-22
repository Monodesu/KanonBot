using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace KanonBot.Image
{
    public static class BuildCornersClass
    {
        private static IImageProcessingContext ApplyRoundedCorners(
            this IImageProcessingContext ctx,
            float cornerRadius
        )
        {
            Size size = ctx.GetCurrentSize();
            IPathCollection corners = BuildCorners(size.Width, size.Height, cornerRadius);

            ctx.SetGraphicsOptions(
                new GraphicsOptions()
                {
                    Antialias = true,
                    AlphaCompositionMode = PixelAlphaCompositionMode.DestOut // enforces that any part of this shape that has color is punched out of the background
                }
            );

            // mutating in here as we already have a cloned original
            // use any color (not Transparent), so the corners will be clipped
            foreach (var c in corners)
            {
                ctx = ctx.Fill(Color.Red, c);
            }
            return ctx;
        }

        public static IImageProcessingContext RoundCorner(
            this IImageProcessingContext processingContext,
            Size size,
            float cornerRadius
        )
        {
            return processingContext
                .Resize(new ResizeOptions { Size = size, Mode = ResizeMode.Crop })
                .ApplyRoundedCorners(cornerRadius);
        }

        private static IPathCollection BuildCorners(
            int imageWidth,
            int imageHeight,
            float cornerRadius
        )
        {
            // first create a square
            var rect = new RectangularPolygon(-0.5f, -0.5f, cornerRadius, cornerRadius);

            // then cut out of the square a circle so we are left with a corner
            IPath cornerTopLeft = rect.Clip(
                new EllipsePolygon(cornerRadius - 0.5f, cornerRadius - 0.5f, cornerRadius)
            );

            // corner is now a corner shape positions top left
            //lets make 3 more positioned correctly, we can do that by translating the original around the center of the image

            float rightPos = imageWidth - cornerTopLeft.Bounds.Width + 1;
            float bottomPos = imageHeight - cornerTopLeft.Bounds.Height + 1;

            // move it across the width of the image - the width of the shape
            IPath cornerTopRight = cornerTopLeft.RotateDegree(90).Translate(rightPos, 0);
            IPath cornerBottomLeft = cornerTopLeft.RotateDegree(-90).Translate(0, bottomPos);
            IPath cornerBottomRight = cornerTopLeft
                .RotateDegree(180)
                .Translate(rightPos, bottomPos);

            return new PathCollection(
                cornerTopLeft,
                cornerBottomLeft,
                cornerTopRight,
                cornerBottomRight
            );
        }

        public static IImageProcessingContext ApplyRoundedCorners_Part(
            this IImageProcessingContext ctx,
            float cornerRadiusLT,
            float cornerRadiusRT,
            float cornerRadiusLB,
            float cornerRadiusRB
        )
        {
            Size size = ctx.GetCurrentSize();
            IPathCollection corners = BuildCorners_Part(
                size.Width,
                size.Height,
                cornerRadiusLT,
                cornerRadiusRT,
                cornerRadiusLB,
                cornerRadiusRB
            );

            ctx.SetGraphicsOptions(
                new GraphicsOptions()
                {
                    Antialias = true,
                    AlphaCompositionMode = PixelAlphaCompositionMode.DestOut // enforces that any part of this shape that has color is punched out of the background
                }
            );

            // mutating in here as we already have a cloned original
            // use any color (not Transparent), so the corners will be clipped
            foreach (var c in corners)
            {
                ctx = ctx.Fill(Color.Red, c);
            }
            return ctx;
        }

        public static IImageProcessingContext RoundCorner_Parts(
            this IImageProcessingContext processingContext,
            Size size,
            float cornerRadiusLT,
            float cornerRadiusRT,
            float cornerRadiusLB,
            float cornerRadiusRB
        )
        {
            return processingContext
                .Resize(
                    new SixLabors.ImageSharp.Processing.ResizeOptions
                    {
                        Size = size,
                        Mode = ResizeMode.Crop
                    }
                )
                .ApplyRoundedCorners_Part(
                    cornerRadiusLT,
                    cornerRadiusRT,
                    cornerRadiusLB,
                    cornerRadiusRB
                );
        }

        private static IPathCollection BuildCorners_Part(
            int imageWidth,
            int imageHeight,
            float cornerRadiusLT,
            float cornerRadiusRT,
            float cornerRadiusLB,
            float cornerRadiusRB
        )
        {
            //CREARE SQUARE
            var rectLT = new RectangularPolygon(-0.5f, -0.5f, cornerRadiusLT, cornerRadiusLT);
            var rectRT = new RectangularPolygon(-0.5f, -0.5f, cornerRadiusRT, cornerRadiusRT);
            var rectLB = new RectangularPolygon(-0.5f, -0.5f, cornerRadiusLB, cornerRadiusLB);
            var rectRB = new RectangularPolygon(-0.5f, -0.5f, cornerRadiusRB, cornerRadiusRB);

            float rightPos,
                bottomPos;
            //TOP LEFT
            IPath cornerTopLeft = rectLT.Clip(
                new EllipsePolygon(cornerRadiusLT - 0.5f, cornerRadiusLT - 0.5f, cornerRadiusLT)
            );

            //TOP RIGHT
            IPath cornerTopRight = rectRT.Clip(
                new EllipsePolygon(cornerRadiusRT - 0.5f, cornerRadiusRT - 0.5f, cornerRadiusRT)
            );
            rightPos = imageWidth - cornerTopRight.Bounds.Width + 1;
            cornerTopRight = cornerTopRight.RotateDegree(90).Translate(rightPos, 0);

            //BOTTOM LEFT
            IPath cornerBottomLeft = rectLB.Clip(
                new EllipsePolygon(cornerRadiusLB - 0.5f, cornerRadiusLB - 0.5f, cornerRadiusLB)
            );
            bottomPos = imageHeight - cornerBottomLeft.Bounds.Height + 1;
            cornerBottomLeft = cornerBottomLeft.RotateDegree(-90).Translate(0, bottomPos);

            //BOTTOM RIGHT
            IPath cornerBottomRight = rectRB.Clip(
                new EllipsePolygon(cornerRadiusRB - 0.5f, cornerRadiusRB - 0.5f, cornerRadiusRB)
            );
            rightPos = imageWidth - cornerBottomRight.Bounds.Width + 1;
            bottomPos = imageHeight - cornerBottomRight.Bounds.Height + 1;
            cornerBottomRight = cornerBottomRight.RotateDegree(180).Translate(rightPos, bottomPos);

            return new PathCollection(
                cornerTopLeft,
                cornerBottomLeft,
                cornerTopRight,
                cornerBottomRight
            );
        }
    }
}
