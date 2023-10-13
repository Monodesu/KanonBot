#pragma warning disable IDE0044 // 添加只读修饰符
using System.CommandLine;
using SixLabors.ImageSharp;
using Microsoft.CodeAnalysis;
using Img = SixLabors.ImageSharp.Image;
using CommandLine;

namespace KanonBot.Image;

[Verb("Image")]
class ImageOptions
{
    [Option('n', "name", Required = true)]
    public string? Name { get; set; }
    [Option('p', "path", Required = true)]
    public string? Path { get; set; } //可以是URL
    [Option('s', "size", Required = true)]
    public string? Size { get; set; }
}

[Verb("Round")]
class RoundOptions
{
    [Option('n', "name", Required = true)]
    public string? Name { get; set; }
    [Option('s', "size", Required = true)]
    public string? Size { get; set; }
    [Option('r', "radius", Required = true)]
    public int? Radius { get; set; }
}

[Verb("Draw")]
class DrawOptions
{
    [Option('d', "dest_name", Required = true)]
    public string? Dest_name { get; set; }
    [Option('s', "source_name", Required = true)]
    public string? Source_name { get; set; }
    [Option('a', "alpha", Required = true)]
    public float? Alpha { get; set; }
    [Option('p', "pos", Required = true)]
    public string? Pos { get; set; }
}

[Verb("DrawText")]
class DrawTextOptions
{
    [Option('n', "image_name", Required = true)]
    public string? Image_Name { get; set; }
    [Option('t', "text", Required = true)]
    public string? Text { get; set; }
    [Option('f', "font", Required = true)]
    public string? Font { get; set; }
    [Option('c', "font_color", Required = true)]
    public string? Font_Color { get; set; }
    [Option('s', "font_size", Required = true)]
    public float? Font_Size { get; set; } //px
    [Option('a', "align", Required = true)]
    public string? Align { get; set; } //center left right
    [Option('p', "pos", Required = true)]
    public string? Pos { get; set; }
}

[Verb("Resize")]
class ResizeOptions
{
    [Option('n', "name", Required = true)]
    public string? Name { get; set; }
    [Option('s', "size", Required = true)]
    public string? Size { get; set; }
}

public class Helper
{
    #region 变量
    // workingFileName 永远使用第一条调用Image命令行导入图像时所使用的图像名称
    private Processor processor;

    #endregion

    public Helper()
    {
        this.processor = new();
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
    public bool Parse(string commandline)
    {
        var args = CommandLineParser.SplitCommandLineIntoArguments(commandline, true).ToArray();
        if (args.Length == 0) return false; //不处理空命令

        return CommandLine.Parser.Default.ParseArguments<ImageOptions, RoundOptions, DrawOptions, DrawTextOptions, ResizeOptions>(args)
            .MapResult(
                (ImageOptions opts) =>
                {
                    var temp = opts.Size!.Split('x');
                    processor.Image(opts.Name!, opts.Path!, int.Parse(temp[0]), int.Parse(temp[1]));
                    return true;
                },
                (RoundOptions opts) =>
                {
                    var temp = opts.Size!.Split('x');
                    processor.Round(opts.Name!, int.Parse(temp[0]), int.Parse(temp[1]), opts.Radius!.Value);
                    return true;
                },
                (DrawOptions opts) =>
                {
                    var temp = opts.Pos!.Split('x');
                    processor.Draw(opts.Dest_name!, opts.Source_name!, opts.Alpha!.Value, int.Parse(temp[0]), int.Parse(temp[1]));
                    return true;
                },
                (DrawTextOptions opts) =>
                {
                    var temp = opts.Pos!.Split('x');
                    processor.DrawText(
                        opts.Text!,
                        opts.Image_Name!,
                        opts.Font!,
                        Color.ParseHex(opts.Font_Color!),
                        opts.Font_Size!.Value,
                        int.Parse(temp[0]),
                        int.Parse(temp[1]),
                        opts.Align!
                    );
                    return true;
                },
                (ResizeOptions opts) =>
                {
                    var temp = opts.Size!.Split('x');
                    processor.Resize(opts.Name!, int.Parse(temp[0]), int.Parse(temp[1]));
                    return true;
                },
                errs => false
            );
    }

    public Img Build() { return processor.GetWorkingImage(); }
}
