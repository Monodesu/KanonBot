using System.ComponentModel;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using KanonBot.API;
using Kook;
using LanguageExt;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.Crmf;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static KanonBot.LegacyImage.Draw;
using Color = SixLabors.ImageSharp.Color;

namespace KanonBot;

public static class Utils
{
    public static async Task<Option<T>> TimeOut<T>(this Task<T> task, TimeSpan delay)
    {
        var timeOutTask = Task.Delay(delay); // 设定超时任务
        var doing = await Task.WhenAny(task, timeOutTask); // 返回任何一个完成的任务
        if (doing == timeOutTask)// 如果超时任务先完成了 就返回none
            return None;
        return Some<T>(await task);
    }
    public static int TryGetConsoleWidth() { try { return Console.WindowWidth; } catch { return 80; } } // 获取失败返回80
    public static string? GetObjectDescription(Object value)
    {
        foreach (var field in value.GetType().GetFields())
        {
            // 获取object的类型，并遍历获取DescriptionAttribute
            // 提取出匹配的那个
            if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
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
        Stream stream = new MemoryStream(buffer);
        //设置 stream 的 position 为流的开始
        stream.Seek(0, SeekOrigin.Begin);
        return stream;
    }

    public static Stream LoadFile2ReadStream(string filePath)
    {
        FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return fs;
    }

    public static async Task<byte[]> LoadFile2Byte(string filePath)
    {
        using (var fs = LoadFile2ReadStream(filePath))
        {
            byte[] bt = new byte[fs.Length];
            await fs.ReadAsync(bt, 0, bt.Length);
            fs.Close();
            return bt;
        }
    }

    public static string GetDesc(object? value)
    {
        FieldInfo? fieldInfo = value!.GetType().GetField(value.ToString()!);
        if (fieldInfo == null) return string.Empty;
        DescriptionAttribute[] attributes = (DescriptionAttribute[])fieldInfo
           .GetCustomAttributes(typeof(DescriptionAttribute), false);
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
    public static double log1p(double x)
        => Math.Abs(x) > 1e-4 ? Math.Log(1.0 + x) : (-0.5 * x + 1.0) * x;

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
        Random r = new Random(DateTime.Now.Millisecond + DateTime.Now.Second + DateTime.Now.Minute);
        string s = "", str = "";
        str += "0123456789";
        str += "abcdefghijklmnopqrstuvwxyz";
        str += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        if (!URLparameter) str += "!_-@#$%+^&()[]'~`";
        for (int i = 0; i < length; i++)
        {
            s += str.Substring(r.Next(0, str.Length - 1), 1);
        }
        return s;
    }

    public static string Duration2String(long duration)
    {
        long day, hour, minute, second;
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
        long day, hour, minute, second;
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
        long hour, minute, second;
        hour = duration / 3600;
        duration %= 3600;
        minute = duration / 60;
        second = duration % 60;
        if (hour > 0)
            return $"{hour}:{minute.ToString("00")}:{second.ToString("00")}";
        return $"{minute}:{second.ToString("00")}";
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

    public static string ParseAt(string input)
    {
        string pattern = @"@.*?#(\d*)";
        RegexOptions options = RegexOptions.Multiline;
        var matches = Regex.Matches(input, pattern, options);
        foreach (Match m in matches.Reverse())
        {
            input = input.Remove(m.Index, m.Value.Length);
            input = input.Insert(m.Index, $"kaiheila={m.Groups[1].Value}");
        }
        pattern = @"\[CQ:at,qq=(\d*)\]";
        matches = Regex.Matches(input, pattern, options);
        foreach (Match m in matches.Reverse())
        {
            input = input.Remove(m.Index, m.Value.Length);
            input = input.Insert(m.Index, $"qq={m.Groups[1].Value}");
        }
        return input;
    }

    public static bool IsUrl(string str)
    {
        try
        {
            string Url = @"^http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?$";
            return Regex.IsMatch(str, Url);
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static string newYearMessage()
    {
        Random r = new();
        int i = r.Next(1, 26);
        switch (i)
        {
            case 1:
                return "新的一年要保持一个好心态，去年的留下的遗憾，要在今年努力争取改变！";
            case 2:
                return "(  )";
            case 3:
                return "新的一年要特别注意健康状况喔！";
            case 4:
                return "遗憾的事情假如已无法改变，那就勇敢地接受，不要后悔，要继续前进才是！";
            case 5:
                return "新的一年也要去勇敢地追求梦想呢~";
            case 6:
                return "遗憾不是尽了全力没有成功，而是可能成功却没尽全力。新的一年调整好自己，";
            case 7:
                return "把2021的失意揉碎。2022年好好生活，慢慢相遇，保持热爱。";
            case 8:
                return "愿岁并谢，与长友兮。";
            case 9:
                return "今年你没有压岁钱！";
            case 10:
                return "用一句话告别2021，你会说些什么呢？（    ）";
            case 11:
                return "さよならがあんたに捧ぐ愛の言葉 ——さよならべいべ(藤井风)";
            case 12:
                return "You're just too good to be true, Can't take my eyes off of you.";
            case 13:
                return "希望你的未来一片光明≥v≤";
            case 14:
                return "或许大家不再和过去认识的一些朋友联系了，但还是会记得那一段美好的时光不是吗？";
            case 15:
                return "啊就是不听话！！！就是想放假！！！";
            case 16:
                return "没太多计划，不知道要去哪，那就这样一直向前，什么都不管啦！";
            case 17:
                return "唱一首心爱的人喜欢的歌曲给自己听好吗";
            case 18:
                return "给心爱的人唱一首自己喜欢的歌吧！";
            case 19:
                return "没有说得出口的话，没有做得出来的事，或是最终没有争取到的人，那都没有关系。与其痛苦，不如坦然接受，继续向前。";
            case 20:
                return "Darling darling 今晚 一定要喝 只要 有你在 就够了 继续 反覆着 那痛苦快乐 不完美 的人生 才动人 —— 八三夭";
            case 21:
                return "在过去的一年里，最打动你的一件事是（   ）";
            case 22:
                return "朋友之间的抱歉，如果可以好好说出来的话，如今也不会像个傻瓜一样充满后悔与遗憾了吧";
            case 23:
                return "愿你不卑不亢不自叹，一生热爱不遗憾。";
            case 24:
                return "今年，对自己温柔一些。好吗？";
            case 25:
                return "对2021年的自己说一声辛苦了！";
            case 26:
                return "答应我，不要再玩OSU了！TAT";
            default:
                break;
        }
        return "";
    }

    public static bool IsMailAddr(string str)
    {
        string emailStr = @"([a-zA-Z0-9_\.\-])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,5})+";
        Regex emailReg = new(emailStr);
        if (emailReg.IsMatch(str))
            return true;
        return false;
    }

    public static string GetTimeStamp(bool isMillisec)
    {
        TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        if (!isMillisec) return Convert.ToInt64(ts.TotalSeconds).ToString();
        else return Convert.ToInt64(ts.TotalMilliseconds).ToString();
    }

    public static string HideMailAddr(string mailAddr)
    {
        try
        {
            var t1 = mailAddr.Split('@');
            string[] t2 = new string[t1[0].Length];
            for (int i = 0; i < t1[0].Length; i++) { t2[i] = "*"; }
            t2[0] = t1[0][0].ToString();
            t2[t1[0].Length - 1] = t1[0][t1[0].Length - 1].ToString();
            string ret = "";
            foreach (string s in t2) { ret += s; }
            ret += "@";
            t2 = new string[t1[1].Length];
            for (int i = 0; i < t1[1].Length; i++) { t2[i] = "*"; }
            t2[0] = t1[1][0].ToString();
            t2[t1[1].Length - 1] = t1[1][t1[1].Length - 1].ToString();
            t2[t1[1].IndexOf(".")] = ".";
            foreach (string s in t2) { ret += s; }
            return ret;
        }
        catch { return mailAddr; }
    }

    public static void SendDebugMail(string mailto, string body)
    {
        Mail.MailStruct ms = new()
        {
            MailTo = Array(mailto),
            Subject = $"KanonBot 错误自动上报 - 发生于 {DateTime.Now}",
            Body = body,
            IsBodyHtml = false
        };
        try
        {
            Mail.Send(ms);
        }
        catch { }
    }
    public static void SendMail(string mailto, string title, string body, bool isBodyHtml)
    {
        Mail.MailStruct ms = new()
        {
            MailTo = Array(mailto),
            Subject = title,
            Body = body,
            IsBodyHtml = isBodyHtml
        };
        try
        {
            Mail.Send(ms);
        }
        catch { }
    }

    public static double ToLinear(double color) => color <= 0.04045 ? color / 12.92 : Math.Pow((color + 0.055) / 1.055, 2.4);

    public static Vector4 ToLinear(this Vector4 colour) =>
            new Vector4(
                (float)ToLinear(colour.X),
                (float)ToLinear(colour.Y),
                (float)ToLinear(colour.Z),
                colour.W);
    public static Color ToColor(this Vector4 colour) =>
            Color.FromRgba(
                (byte)(colour.X * 255),
                (byte)(colour.Y * 255),
                (byte)(colour.Z * 255),
                (byte)(colour.W * 255)
            );

    public static Vector4 ValueAt(double time, Vector4 startColour, Vector4 endColour, double startTime, double endTime)
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


    public static Vector4 SampleFromLinearGradient(IReadOnlyList<(float position, Vector4 colour)> gradient, float point)
    {
        if (point < gradient[0].position)
            return gradient[0].colour;

        for (int i = 0; i < gradient.Count - 1; i++)
        {
            var startStop = gradient[i];
            var endStop = gradient[i + 1];

            if (point >= endStop.position)
                continue;

            return ValueAt(point, startStop.colour, endStop.colour, startStop.position, endStop.position);
        }

        return gradient[^1].colour;
    }

    static public Color ForStarDifficulty(double starDifficulty) => SampleFromLinearGradient(new[]
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
    }, (float)Math.Round(starDifficulty, 2, MidpointRounding.AwayFromZero)).ToColor();
}

