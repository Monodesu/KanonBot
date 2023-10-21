using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
//using KanonBot.API;
using Kook;
using LanguageExt;
using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
//using static KanonBot.LegacyImage.Draw;
using Color = SixLabors.ImageSharp.Color;
using Img = SixLabors.ImageSharp.Image;
using Org.BouncyCastle.Cms;
using System.Reactive.Subjects;

namespace KanonBot;

public static partial class Utils
{
    private static RandomNumberGenerator rng = RandomNumberGenerator.Create();

    public static byte[] GenerateRandomBytes(int length)
    {
        byte[] randomBytes = new byte[length];
        rng.GetBytes(randomBytes);
        return randomBytes;
    }


    public static async Task<Option<T>> TimeOut<T>(this Task<T> task, TimeSpan delay)
    {
        var timeOutTask = Task.Delay(delay); // 设定超时任务
        var doing = await Task.WhenAny(task, timeOutTask); // 返回任何一个完成的任务
        if (doing == timeOutTask) // 如果超时任务先完成了 就返回none
            return None;
        return Some<T>(await task);
    }

    public static int TryGetConsoleWidth()
    {
        try
        {
            return Console.WindowWidth;
        }
        catch
        {
            return 80;
        }
    } // 获取失败返回80

    public static Option<(String, String)> SplitKvp(String msg)
    {
        if (msg.Filter((c) => c == '=').Count() == 1)
        {
            var p = msg.Split('=');

            var (k, v) = (p[0], p[1]);
            if (string.IsNullOrWhiteSpace(k) || string.IsNullOrWhiteSpace(v))
                return None;
            return Some((k, v));
        }
        return None;
    }

    public static string? GetObjectDescription(Object value)
    {
        foreach (var field in value.GetType().GetFields())
        {
            // 获取object的类型，并遍历获取DescriptionAttribute
            // 提取出匹配的那个
            if (
                Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute))
                is DescriptionAttribute attribute
            )
            {
                if (field.GetValue(null)?.Equals(value) ?? false)
                    return attribute.Description;
            }
        }
        return null;
    }

    public static List<T> Slice<T>(this List<T> myList, int startIndex, int endIndex)
    {
        return myList.Skip(startIndex).Take(endIndex - startIndex + 1).ToList();
    }

    public static Stream Byte2Stream(byte[] buffer)
    {
        var stream = new MemoryStream(buffer);
        //设置 stream 的 position 为流的开始
        stream.Seek(0, SeekOrigin.Begin);
        return stream;
    }

    public static string Byte2File(string fileName, byte[] buffer)
    {
        using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
        {
            fs.Write(buffer, 0, buffer.Length);
        }
        return Path.GetFullPath(fileName);;
    }

    public static Stream LoadFile2ReadStream(string filePath)
    {
        var fs = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite
        );
        return fs;
    }

    public static async Task<byte[]> LoadFile2Byte(string filePath)
    {
        using var fs = LoadFile2ReadStream(filePath);
        byte[] bt = new byte[fs.Length];
        var mem = new Memory<Byte>(bt);
        await fs.ReadAsync(mem);
        fs.Close();
        return mem.ToArray();
    }

    public static string GetDesc(object? value)
    {
        FieldInfo? fieldInfo = value!.GetType().GetField(value.ToString()!);
        if (fieldInfo == null)
            return string.Empty;
        DescriptionAttribute[] attributes = (DescriptionAttribute[])
            fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Length > 0 ? attributes[0].Description : string.Empty;
    }

    public static DateTimeOffset TimeStampMilliToDateTime(int timeStamp)
    {
        return DateTimeOffset.FromUnixTimeMilliseconds(timeStamp);
    }

    public static DateTimeOffset TimeStampSecToDateTime(long timeStamp)
    {
        return DateTimeOffset.FromUnixTimeSeconds(timeStamp);
    }

    public static string Dict2String(Dictionary<String, Object> dict)
    {
        var lines = dict.Select(kvp => kvp.Key + ": " + kvp.Value.ToString());
        return string.Join(Environment.NewLine, lines);
    }

    public static double log1p(double x) =>
        Math.Abs(x) > 1e-4 ? Math.Log(1.0 + x) : (-0.5 * x + 1.0) * x;

    public static string KOOKUnEscape(string str)
    {
        str = str.Replace("\\\\n", "\\n");
        str = str.Replace("\\(", "(");
        str = str.Replace("\\)", ")");
        return str;
    }

    public static string KOOKEscape(string str)
    {
        str = str.Replace("\\n", "\\\\n");
        str = str.Replace("(", "\\(");
        str = str.Replace(")", "\\)");
        return str;
    }

    public static string GuildUnEscape(string str)
    {
        str = str.Replace("&amp;", "&");
        str = str.Replace("&lt;", "<");
        str = str.Replace("&gt;", ">");
        return str;
    }

    public static string GuildEscape(string str)
    {
        str = str.Replace("&", "&amp;");
        str = str.Replace("<", "&lt;");
        str = str.Replace(">", "&gt;");
        return str;
    }

    public static string CQUnEscape(string str)
    {
        str = str.Replace("&amp;", "&");
        str = str.Replace("&#91;", "[");
        str = str.Replace("&#93;", "]");
        return str;
    }

    public static string CQEscape(string str)
    {
        str = str.Replace("&", "&amp;");
        str = str.Replace("[", "&#91;");
        str = str.Replace("]", "&#93;");
        return str;
    }

    public static string RandomStr(int length, bool URLparameter = false)
    {
        string str = "";
        str += "0123456789";
        str += "abcdefghijklmnopqrstuvwxyz";
        str += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        if (!URLparameter)
            str += "!_-@#$%+^&()[]'~`";
        StringBuilder sb = new();
        for (int i = 0; i < length; i++)
        {
            byte[] randomBytes = GenerateRandomBytes(100);
            int randomIndex = randomBytes[i] % str.Length;
            sb.Append(str[randomIndex]);
        }
        return sb.ToString();
    }

    public static string RandomRedemptionCode()
    {
        StringBuilder sb = new();
        string str = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        for (int o = 0; o < 7; o++)
        {
            for (int i = 0; i < 7; i++)
            {
                byte[] randomBytes = GenerateRandomBytes(255);
                int randomIndex = randomBytes[i] % str.Length;
                sb.Append(str[randomIndex]);
            }
            if (o < 6) sb.Append('-');
        }
        return sb.ToString();
    }

    public static string Duration2String(long duration)
    {
        long day,
            hour,
            minute,
            second;
        day = duration / 86400;
        duration %= 86400;
        hour = duration / 3600;
        duration %= 3600;
        minute = duration / 60;
        second = duration % 60;
        return $"{day}d {hour}h {minute}m {second}s";
    }

    public static string Duration2StringWithoutSec(long duration)
    {
        long day,
            hour,
            minute,
            second;
        day = duration / 86400;
        duration %= 86400;
        hour = duration / 3600;
        duration %= 3600;
        minute = duration / 60;
        second = duration % 60;
        return $"{day}d {hour}h {minute}m";
    }

    public static string Duration2TimeString(long duration)
    {
        long hour,
            minute,
            second;
        hour = duration / 3600;
        duration %= 3600;
        minute = duration / 60;
        second = duration % 60;
        if (hour > 0)
            return $"{hour}:{minute:00}:{second:00}";
        return $"{minute}:{second:00}";
    }

    public static string Duration2TimeString_ForScoreV3(long duration)
    {
        long hour,
            minute,
            second;
        hour = duration / 3600;
        duration %= 3600;
        minute = duration / 60;
        second = duration % 60;
        if (hour > 0)
            return $"{hour}H,{minute:00}M,{second:00}S";
        return $"{minute}M,{second:00}S";
    }

    // 全角转半角
    public static string ToDBC(string input)
    {
        char[] c = input.ToCharArray();
        for (int i = 0; i < c.Length; i++)
        {
            if (c[i] == 12288)
            {
                c[i] = (char)32;
                continue;
            }
            if (c[i] > 65280 && c[i] < 65375)
                c[i] = (char)(c[i] - 65248);
        }
        return new String(c);
    }

    [GeneratedRegex(@"^http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?$")]
    private static partial Regex UrlRegex();
    public static bool IsUrl(string str)
    {
        try
        {
            return UrlRegex().IsMatch(str);
        }
        catch (Exception)
        {
            return false;
        }
    }

    [GeneratedRegex(@"([a-zA-Z0-9_\.\-])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,5})+")]
    private static partial Regex EmailRegex();
    public static bool IsMailAddr(string str)
    {
        if (EmailRegex().IsMatch(str))
            return true;
        return false;
    }

    public static string GetTimeStamp(bool isMillisec)
    {
        TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        if (!isMillisec)
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        else
            return Convert.ToInt64(ts.TotalMilliseconds).ToString();
    }

    public static string HideMailAddr(string mailAddr)
    {
        try
        {
            var t1 = mailAddr.Split('@');
            string[] t2 = new string[t1[0].Length];
            for (int i = 0; i < t1[0].Length; i++)
            {
                t2[i] = "*";
            }
            t2[0] = t1[0][0].ToString();
            t2[t1[0].Length - 1] = t1[0][^1].ToString();
            string ret = "";
            foreach (string s in t2)
            {
                ret += s;
            }
            ret += "@";
            t2 = new string[t1[1].Length];
            for (int i = 0; i < t1[1].Length; i++)
            {
                t2[i] = "*";
            }
            t2[0] = t1[1][0].ToString();
            t2[t1[1].Length - 1] = t1[1][^1].ToString();
            t2[t1[1].IndexOf(".")] = ".";
            foreach (string s in t2)
            {
                ret += s;
            }
            return ret;
        }
        catch
        {
            return mailAddr;
        }
    }

    public static void SendDebugMail(string mailto, string body)
    {
        var mailContent = new Mail.MailContent(new List<string> { mailto }, $"KanonBot 错误自动上报 - 发生于 {DateTime.Now}", body, false);
        try
        {
            Mail.Send(mailContent);
        }
        catch { }
    }

    public static void SendMail(string mailto, string subject, string body, bool isBodyHtml)
    {
        var mailContent = new Mail.MailContent(new List<string> { mailto }, subject, body, isBodyHtml);
        try
        {
            Mail.Send(mailContent);
        }
        catch { }
    }

    async public static Task<Image<Rgba32>> ReadImageRgba(string path)
    {
        using var img = await Img.LoadAsync(path);
        return img.CloneAs<Rgba32>();
    }

    async public static Task<(Image<Rgba32>, IImageFormat)> ReadImageRgbaWithFormat(string path)
    {
        using var s = Utils.LoadFile2ReadStream(path);
        var img = await Img.LoadAsync<Rgba32>(s);
        return (img, img.Metadata.DecodedImageFormat!);
    }

    public static double ToLinear(double color) =>
        color <= 0.04045 ? color / 12.92 : Math.Pow((color + 0.055) / 1.055, 0.8);

    public static Vector4 ToLinear(this Vector4 colour) =>
        new(
            (float)ToLinear(colour.X),
            (float)ToLinear(colour.Y),
            (float)ToLinear(colour.Z),
            colour.W
        );

    public static Color ToColor(this Vector4 colour) =>
        Color.FromRgba(
            (byte)(colour.X * 255),
            (byte)(colour.Y * 255),
            (byte)(colour.Z * 255),
            (byte)(colour.W * 255)
        );

    public static Vector4 ValueAt(
        double time,
        Vector4 startColour,
        Vector4 endColour,
        double startTime,
        double endTime
    )
    {
        if (startColour == endColour)
            return startColour;

        double current = time - startTime;
        double duration = endTime - startTime;

        if (duration == 0 || current == 0)
            return startColour;

        var startLinear = startColour.ToLinear();
        var endLinear = endColour.ToLinear();

        float t = Math.Max(0, Math.Min(1, (float)(current / duration)));

        return new Vector4(
            startLinear.X + t * (endLinear.X - startLinear.X),
            startLinear.Y + t * (endLinear.Y - startLinear.Y),
            startLinear.Z + t * (endLinear.Z - startLinear.Z),
            startLinear.W + t * (endLinear.W - startLinear.W)
        );
    }

    public static Vector4 SampleFromLinearGradient(
        IReadOnlyList<(float position, Vector4 colour)> gradient,
        float point
    )
    {
        if (point < gradient[0].position)
            return gradient[0].colour;

        for (int i = 0; i < gradient.Count - 1; i++)
        {
            var startStop = gradient[i];
            var endStop = gradient[i + 1];

            if (point >= endStop.position)
                continue;

            return ValueAt(
                point,
                startStop.colour,
                endStop.colour,
                startStop.position,
                endStop.position
            );
        }

        return gradient[^1].colour;
    }

    static public Color ForStarDifficulty(double starDifficulty) =>
        SampleFromLinearGradient(
                new[]
                {
                    (0.1f, Rgba32.ParseHex("#aaaaaa").ToVector4()),
                    (0.1f, Rgba32.ParseHex("#4290fb").ToVector4()),
                    (1.25f, Rgba32.ParseHex("#4fc0ff").ToVector4()),
                    (2.0f, Rgba32.ParseHex("#4fffd5").ToVector4()),
                    (2.5f, Rgba32.ParseHex("#7cff4f").ToVector4()),
                    (3.3f, Rgba32.ParseHex("#f6f05c").ToVector4()),
                    (4.2f, Rgba32.ParseHex("#ff8068").ToVector4()),
                    (4.9f, Rgba32.ParseHex("#ff4e6f").ToVector4()),
                    (5.8f, Rgba32.ParseHex("#c645b8").ToVector4()),
                    (6.7f, Rgba32.ParseHex("#6563de").ToVector4()),
                    (7.7f, Rgba32.ParseHex("#18158e").ToVector4()),
                    (9.0f, Rgba32.ParseHex("#000000").ToVector4()),
                },
                (float)Math.Round(starDifficulty, 2, MidpointRounding.AwayFromZero)
            )
            .ToColor();

    public static int RandomNum(int min, int max)
    {
        var r = new Random(
            DateTime.Now.Millisecond
                + DateTime.Now.Second
                + DateTime.Now.Minute
                + DateTime.Now.Microsecond
                + DateTime.Now.Nanosecond
        );
        return r.Next(min, max);
    }
}
