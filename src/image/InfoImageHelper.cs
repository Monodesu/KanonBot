#pragma warning disable IDE0044 // 添加只读修饰符

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Drawing;
using SixLabors.Fonts;
using Microsoft.CodeAnalysis;

namespace KanonBot;
public class ImageOption
{
    public string? Name { get; set; }
    public string? Path { get; set; } //可以是URL
    public string? Size { get; set; }
}
public class RoundOption
{
    public string? Name { get; set; }
    public string? Size { get; set; }
    public int? Radius { get; set; }
}
public class DrawOption
{
    public string? Dest_name { get; set; }
    public string? Source_name { get; set; }
    public float? Alpha { get; set; }
    public string? Pos { get; set; }
}
public class DrawTextOption
{
    public string? Image_Name { get; set; }
    public string? Text { get; set; }
    public string? Font { get; set; }
    public string? Font_Color { get; set; }
    public float? Font_Size { get; set; } //px
    public string? Align { get; set; } //center left right
    public string? Pos { get; set; }
}
public class ResizeOption
{
    public string? Name { get; set; }
    public string? Size { get; set; }
}
public class KanonBotImage
{

    #region 变量
    // workingFileName 永远使用第一条调用Image命令行导入图像时所使用的图像名称
    private string workingFileName = "";
    RootCommand rootCommand = new();
    private Dictionary<string, Image> WorkList = new();
    Command imageCommand = new("Image");
    Command roundCommand = new("Round");
    Command drawCommand = new("Draw");
    Command drawtextCommand = new("DrawText");
    Command resizeCommand = new("Resize");
    #endregion

    public KanonBotImage()
    {
        string[] temp;
        #region Image
        imageCommand.Add(new Option<string>(new string[] { "--name", "-n" }));
        imageCommand.Add(new Option<string>(new string[] { "--path", "-p" }));
        imageCommand.Add(new Option<string>(new string[] { "--size", "-s" }));
        imageCommand.Handler = CommandHandler.Create<ImageOption>((imageOption) =>
        {
            if (imageOption.Name == null || imageOption.Path == null || imageOption.Size == null) return; // 拒绝处理参数不全的命令
            try { temp = imageOption.Size.Split('x'); } catch { return; }

            Image_Processor(imageOption.Name, imageOption.Path, int.Parse(temp[0]), int.Parse(temp[1]));
        });
        #endregion

        #region Round
        roundCommand.Add(new Option<string>(new string[] { "--name", "-n" }));
        roundCommand.Add(new Option<string>(new string[] { "--size", "-s" }));
        roundCommand.Add(new Option<int>(new string[] { "--radius", "-r" }));
        roundCommand.Handler = CommandHandler.Create<RoundOption>((roundOption) =>
        {
            if (roundOption.Name == null || roundOption.Size == null || roundOption.Radius == null) return; // 拒绝处理参数不全的命令
            try { temp = roundOption.Size.Split('x'); } catch { return; }

            Round_Processor(roundOption.Name, int.Parse(temp[0]), int.Parse(temp[1]), roundOption.Radius.Value);
        });
        #endregion

        #region Draw
        drawCommand.Add(new Option<string>(new string[] { "--dest_name", "-d" }));
        drawCommand.Add(new Option<string>(new string[] { "--source_name", "-s" }));
        drawCommand.Add(new Option<float>(new string[] { "--alpha", "-a" }));
        drawCommand.Add(new Option<string>(new string[] { "--pos", "-p" }));
        drawCommand.Handler = CommandHandler.Create<DrawOption>((drawOption) =>
        {
            if (drawOption.Dest_name == null || drawOption.Source_name == null || drawOption.Alpha == null || drawOption.Pos == null) return; // 拒绝处理参数不全的命令
            try { temp = drawOption.Pos.Split('x'); } catch { return; }

            Draw_Processor(drawOption.Dest_name, drawOption.Source_name, drawOption.Alpha.Value, int.Parse(temp[0]), int.Parse(temp[1]));
        });
        #endregion

        #region DrawText
        drawtextCommand.Add(new Option<string>(new string[] { "--image_name", "-n" }));
        drawtextCommand.Add(new Option<string>(new string[] { "--text", "-t" }));
        drawtextCommand.Add(new Option<string>(new string[] { "--font", "-f" }));
        drawtextCommand.Add(new Option<string>(new string[] { "--font_color", "-c" })); //HEX
        drawtextCommand.Add(new Option<float>(new string[] { "--font_size", "-s" }));  //font size (px)
        drawtextCommand.Add(new Option<string>(new string[] { "--align", "-a" })); //left right center
        drawtextCommand.Add(new Option<string>(new string[] { "--pos", "-p" }));
        drawtextCommand.Handler = CommandHandler.Create<DrawTextOption>((drawTextOption) =>
        {
            if (
            drawTextOption.Image_Name == null ||
            drawTextOption.Text == null ||
            drawTextOption.Font == null ||
            drawTextOption.Font_Color == null ||
            drawTextOption.Font_Size == null ||
            drawTextOption.Align == null ||
            drawTextOption.Pos == null) return; // 拒绝处理参数不全的命令
            try { temp = drawTextOption.Pos.Split('x'); } catch { return; }

            DrawText_Processor(
                drawTextOption.Text,
                drawTextOption.Image_Name,
                drawTextOption.Font,
                Color.ParseHex(drawTextOption.Font_Color),
                drawTextOption.Font_Size.Value,
                int.Parse(temp[0]),
                int.Parse(temp[1]),
                drawTextOption.Align);
        });
        #endregion

        #region Resize
        resizeCommand.Add(new Option<string>(new string[] { "--name", "-n" }));
        resizeCommand.Add(new Option<string>(new string[] { "--size", "-s" }));
        resizeCommand.Handler = CommandHandler.Create<ResizeOption>((resizeOption) =>
        {
            if (resizeOption.Name == null || resizeOption.Size == null) return; // 拒绝处理参数不全的命令
            try { temp = resizeOption.Size.Split('x'); } catch { return; }

            Resize_Processor(resizeOption.Name, int.Parse(temp[0]), int.Parse(temp[1]));
        });
        #endregion

        #region Invoke
        rootCommand.AddCommand(imageCommand);
        rootCommand.AddCommand(roundCommand);
        rootCommand.AddCommand(drawCommand);
        rootCommand.AddCommand(drawtextCommand);
        rootCommand.AddCommand(resizeCommand);
        #endregion
    }
    /// <summary>
    /// Image --name/-n --path/-p --size-s
    /// Image -n image1 -p https://localhost/1.png --size 100x100
    /// 
    /// Round --name/-n --size/-s --radius/-r
    /// Round -n image1 -s 100x100 -r 25
    /// 
    /// Draw --dest_name/d --source_name/s --alpha/-a --pos/-p
    /// Draw -d image1 -s image2 -a 1 -p 1x1
    /// 
    /// DrawText --image_name/-n --text/-t --font/-f --font_color/-c --font_size/-s --align/-a --pos/-p
    /// DrawText -n image1 -t "你好啊" -f "HarmonyOS_Sans_Medium" -c "#f47920" -a left -p 10x120
    /// 
    /// Resize --name/-n --size/-s
    /// Resize -n image1 -s 50x50
    /// </summary>
    /// <param name="commandline">传入的命令行</param>
    public void Parse(string commandline)
    {
        var args = CommandLineParser.SplitCommandLineIntoArguments(commandline, true).ToArray();
        if (args.Length == 0) return; //不处理空命令
        _ = rootCommand.InvokeAsync(args).Result;
    }

    /// <summary>
    /// 处理Image命令
    /// </summary>
    /// <param name="name">图像名</param>
    /// <param name="path">图像路径，使用网页图片链接，在传入值为new时，则新建一个空白的透明画布</param>
    /// <param name="width">图像宽度</param>
    /// <param name="height">图像高度</param>
    public void Image_Processor(string name, string path, int width, int height)
    {
        if (path.ToLower() == "new")
        {
            if (workingFileName == "") workingFileName = name;
            WorkList.Add(name, new Image<Rgba32>(width, height));
        }
        else
        {
            if (workingFileName == "") workingFileName = name;
            WorkList.Add(name, Image.Load(path));
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
    public void Round_Processor(string name, int width, int height, int radius)
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
    public void Draw_Processor(string dest_name, string source_name, float alpha, int pos_x, int pos_y)
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
    public void DrawText_Processor(string text, string name, string font, Color color, float size, int pos_x, int pos_y, string align)
    {
        //构建text options 与 draw options
        var fonts = new FontCollection();
        FontFamily ff = fonts.Add($"./work/fonts/{font}.ttf");

        var textOptions = new TextOptions(new Font(ff, size))
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

        IBrush brush = new SolidBrush(color); //IPen pen = new Pen(color, size);

        //构建图像
        WorkList[name].Mutate(x => x.DrawText(drawOptions, textOptions, text, brush, null));
    }

    /// <summary>
    /// 处理Resize命令
    /// </summary>
    /// <param name="name">目标图像名(Target)</param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void Resize_Processor(string name, int width, int height)
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
    public void Automatic_Displacement_ImageProcessor(string dest_name, string source_name, int pos_x, int pos_y, int value, float alpha, string direction) //LevelBar
    {
        var source_img_size = WorkList[source_name].Size();
        var point = Get_ADIP_Pos(pos_x, pos_y, source_img_size.Width, source_img_size.Height, value, direction);

        WorkList[dest_name].Mutate(x => x.DrawImage(WorkList[source_name], point, alpha));
    }

    /// <summary>
    /// 计算坐标
    /// </summary>
    private Point Get_ADIP_Pos(int pos_x, int pos_y, int width, int height, int value, string direction)
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
    ~KanonBotImage()
    {
        WorkList.Clear();
        return;
    }
}
static class BuildCornersClass
{
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

