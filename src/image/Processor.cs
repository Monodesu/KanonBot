#pragma warning disable IDE0044 // 添加只读修饰符
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Drawing;
using Img = SixLabors.ImageSharp.Image;
using SixLabors.Fonts;

namespace KanonBot.Image;

public class Processor
{


    #region 变量
    // workingFileName 永远使用第一条调用Image命令行导入图像时所使用的图像名称
    private string workingFileName = "";
    private Dictionary<string, Img> WorkList = new();

    #endregion

    /// <summary>
    /// 处理Image命令
    /// </summary>
    /// <param name="name">图像名</param>
    /// <param name="path">图像路径，使用网页图片链接，在传入值为new时，则新建一个空白的透明画布</param>
    /// <param name="width">图像宽度</param>
    /// <param name="height">图像高度</param>
    public void Image(string name, string path, int width, int height)
    {
        if (path.ToLower() == "new")
        {
            if (workingFileName == "") workingFileName = name;
            WorkList.Add(name, new Image<Rgba32>(width, height));
        }
        else
        {
            if (workingFileName == "") workingFileName = name;
            WorkList.Add(name, Img.Load(path));
            WorkList[name].Mutate(x => x.Resize(width, height));
        }
    }

    /// <summary>
    /// 处理Round命令
    /// </summary>
    /// <param name="name">目标图像名(Target)</param>
    /// <param name="width">图像宽度</param>
    /// <param name="height">图像高度</param>
    /// <param name="radius">圆角半径</param>
    public void Round(string name, int width, int height, int radius)
    {
        WorkList[name].Mutate(x => x.Resize(width, height).RoundCorner(new Size(width, height), radius));
    }

    /// <summary>
    /// 处理Draw命令
    /// 将一个图像覆盖至另一个图像上
    /// </summary>
    /// <param name="dest_name">目标图像名(Target)</param>
    /// <param name="source_name">原始图像名</param>
    /// <param name="alpha">透明度</param>
    /// <param name="pos_x"></param>
    /// <param name="pos_y"></param>
    public void Draw(string dest_name, string source_name, float alpha, int pos_x, int pos_y)
    {
        WorkList[dest_name].Mutate(x => x.DrawImage(WorkList[source_name], new Point(pos_x, pos_y), alpha));
    }

    /// <summary>
    /// 处理DrawText命令
    /// </summary>
    /// <param name="text"></param>
    /// <param name="name">目标图像名(Target)</param>
    /// <param name="font"></param>
    /// <param name="color">HEX(#ffffff)</param>
    /// <param name="size">17px</param>
    /// <param name="pos_x"></param>
    /// <param name="pos_y"></param>
    /// <param name="align">left right center</param>
    public void DrawText(string text, string name, string font, Color color, float size, int pos_x, int pos_y, string align)
    {
        //构建text options 与 draw options
        var fonts = new FontCollection();
        FontFamily ff = fonts.Add($"./work/fonts/{font}");

        var textOptions = new RichTextOptions(new Font(ff, size))
        {
            VerticalAlignment = VerticalAlignment.Bottom,
            Origin = new Point(pos_x, pos_y),
        };

        //获取对齐方式
        switch (align)
        {
            case "left":
                textOptions.HorizontalAlignment = HorizontalAlignment.Left;
                break;
            case "right":
                textOptions.HorizontalAlignment = HorizontalAlignment.Right;
                break;
            case "center":
                textOptions.HorizontalAlignment = HorizontalAlignment.Center;
                break;
            default:
                return; //拒绝处理
        }

        var drawOptions = new DrawingOptions
        {
            GraphicsOptions = new GraphicsOptions
            {
                Antialias = true
            }
        };

        var brush = new SolidBrush(color); //IPen pen = new Pen(color, size);

        //构建图像
        WorkList[name].Mutate(x => x.DrawText(drawOptions, textOptions, text, brush, null));
    }

    /// <summary>
    /// 处理Resize命令
    /// </summary>
    /// <param name="name">目标图像名(Target)</param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void Resize(string name, int width, int height)
    {
        WorkList[name].Mutate(x => x.Resize(new Size(width, height)));
    }

    /// <summary>
    /// 
    /// *****代码需测试*****
    /// 
    /// 处理自动位移图像，用于自定义等级进度条的绘制
    /// pos_x与pos_y必须位于绘制点的起始位置
    /// direction指定了是从左到右绘制，从右到左绘制，从下到上绘制，还是从上到下绘制
    /// 要注意的是，无论direction是什么，在测试绘制时，都需要在代码中按照100%位移结束后的位置设定pos
    /// </summary>
    /// <param name="dest_name">目标图像名(Target)</param>
    /// <param name="source_name">原始图像名</param>
    /// <param name="pos_x"></param>
    /// <param name="pos_y"></param>
    /// <param name="value">进度，从0到100</param>
    /// <param name="direction">LR RL UD DU(left to right / up to down)</param>
    public void AutomaticDisplacementImage(string dest_name, string source_name, int pos_x, int pos_y, int value, float alpha, string direction) //LevelBar
    {
        var source_img_size = WorkList[source_name].Size;
        var point = Get_ADIP_Pos(pos_x, pos_y, source_img_size.Width, source_img_size.Height, value, direction);

        WorkList[dest_name].Mutate(x => x.DrawImage(WorkList[source_name], point, alpha));
    }

    /// <summary>
    /// 计算坐标
    /// </summary>
    private static Point Get_ADIP_Pos(int pos_x, int pos_y, int width, int height, int value, string direction)
    {
        Point point = new();
        switch (direction)
        {
            default: //LR
                point.X = pos_x - width * (value / 100);
                point.Y = pos_y;
                break;
            case "RL":
                point.X = (pos_x + width) - width * (value / 100);
                point.Y = pos_y;
                break;
            case "UD":
                point.X = pos_x;
                point.Y = pos_y - height * (value / 100);
                break;
            case "DU":
                point.X = pos_x;
                point.Y = (pos_y + height) - height * (value / 100);
                break;
        }
        return point;
    }

    /// <summary>
    /// 将结果保存到文件
    /// </summary>
    /// <param name="name">目标图像名(Target)</param>
    /// <param name="path">本地路径</param>
    public void SaveAsFile(string path)
    {
        WorkList[workingFileName].SaveAsPng(path);
    }

    /// <summary>
    /// 获取工作图片
    /// </summary>
    public Img GetWorkingImage()
    {
        return WorkList[workingFileName];
    }

    ~Processor()
    {
        WorkList.Clear();
        return;
    }
}

static class BuildCornersClass
{
    //分别设置四个角的圆角程度
    #region buildCorners_Part

    public static IImageProcessingContext ApplyRoundedCorners_Part(this IImageProcessingContext ctx,
        float cornerRadiusLT, float cornerRadiusRT, float cornerRadiusLB, float cornerRadiusRB)
    {
        Size size = ctx.GetCurrentSize();
        IPathCollection corners = BuildCorners_Part(size.Width, size.Height, cornerRadiusLT, cornerRadiusRT, cornerRadiusLB, cornerRadiusRB);

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

    public static IImageProcessingContext RoundCorner_Parts(this IImageProcessingContext processingContext, Size size,
        float cornerRadiusLT, float cornerRadiusRT, float cornerRadiusLB, float cornerRadiusRB)
    {
        return processingContext.Resize(new SixLabors.ImageSharp.Processing.ResizeOptions
        {
            Size = size,
            Mode = ResizeMode.Crop
        }).ApplyRoundedCorners_Part(cornerRadiusLT, cornerRadiusRT, cornerRadiusLB, cornerRadiusRB);
    }


    public static IPathCollection BuildCorners_Part(int imageWidth, int imageHeight,
         float cornerRadiusLT, float cornerRadiusRT, float cornerRadiusLB, float cornerRadiusRB)
    {
        //CREARE SQUARE
        var rectLT = new RectangularPolygon(-0.5f, -0.5f, cornerRadiusLT, cornerRadiusLT);
        var rectRT = new RectangularPolygon(-0.5f, -0.5f, cornerRadiusRT, cornerRadiusRT);
        var rectLB = new RectangularPolygon(-0.5f, -0.5f, cornerRadiusLB, cornerRadiusLB);
        var rectRB = new RectangularPolygon(-0.5f, -0.5f, cornerRadiusRB, cornerRadiusRB);

        float rightPos, bottomPos;
        //TOP LEFT
        IPath cornerTopLeft = rectLT.Clip(new EllipsePolygon(cornerRadiusLT - 0.5f, cornerRadiusLT - 0.5f, cornerRadiusLT));

        //TOP RIGHT
        IPath cornerTopRight = rectRT.Clip(new EllipsePolygon(cornerRadiusRT - 0.5f, cornerRadiusRT - 0.5f, cornerRadiusRT));
        rightPos = imageWidth - cornerTopRight.Bounds.Width + 1;
        cornerTopRight = cornerTopRight.RotateDegree(90).Translate(rightPos, 0);

        //BOTTOM LEFT
        IPath cornerBottomLeft = rectLB.Clip(new EllipsePolygon(cornerRadiusLB - 0.5f, cornerRadiusLB - 0.5f, cornerRadiusLB));
        bottomPos = imageHeight - cornerBottomLeft.Bounds.Height + 1;
        cornerBottomLeft = cornerBottomLeft.RotateDegree(-90).Translate(0, bottomPos);

        //BOTTOM RIGHT
        IPath cornerBottomRight = rectRB.Clip(new EllipsePolygon(cornerRadiusRB - 0.5f, cornerRadiusRB - 0.5f, cornerRadiusRB));
        rightPos = imageWidth - cornerBottomRight.Bounds.Width + 1;
        bottomPos = imageHeight - cornerBottomRight.Bounds.Height + 1;
        cornerBottomRight = cornerBottomRight.RotateDegree(180).Translate(rightPos, bottomPos);

        return new PathCollection(cornerTopLeft, cornerBottomLeft, cornerTopRight, cornerBottomRight);
    }
    #endregion

    #region BuildCorners
    // This method can be seen as an inline implementation of an `IImageProcessor`:
    // (The combination of `IImageOperations.Apply()` + this could be replaced with an `IImageProcessor`)
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
        return processingContext.Resize(new SixLabors.ImageSharp.Processing.ResizeOptions
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

