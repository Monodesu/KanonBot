using KanonBot.Drivers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KanonBot.Message;

public interface IMsgSegment : IEquatable<IMsgSegment>
{
    string Build();
}

public class RawSegment : IMsgSegment, IEquatable<RawSegment>
{
    public Object value { get; set; }
    public string type { get; set; }

    public RawSegment(string type, Object value)
    {
        this.type = type;
        this.value = value;
    }

    public string Build()
    {
        return value switch
        {
            JObject j => $"<raw;{type}={j.ToString(Formatting.None)}>",
            _ => $"<raw;{type}={value}>",
        };
    }

    public bool Equals(RawSegment? other)
    {
        return other != null && this.type == other.type && this.value == other.value;
    }

    public bool Equals(IMsgSegment? other)
    {
        if (other is RawSegment r)
            return this.Equals(r);
        else
            return false;
    }
}

public class TextSegment : IMsgSegment, IEquatable<TextSegment>
{
    public string value { get; set; }

    public TextSegment(string msg)
    {
        this.value = msg;
    }

    public string Build()
    {
        return value.ToString();
    }

    public bool Equals(TextSegment? other)
    {
        return other != null && this.value == other.value;
    }

    public bool Equals(IMsgSegment? other)
    {
        if (other is TextSegment r)
            return this.Equals(r);
        else
            return false;
    }
}

public class EmojiSegment : IMsgSegment, IEquatable<EmojiSegment>
{
    public string value { get; set; }

    public EmojiSegment(string value)
    {
        this.value = value;
    }

    public string Build()
    {
        return $"<Face;id={value}>";
    }

    public bool Equals(EmojiSegment? other)
    {
        return other != null && this.value == other.value;
    }

    public bool Equals(IMsgSegment? other)
    {
        if (other is EmojiSegment r)
            return this.Equals(r);
        else
            return false;
    }
}

public class AtSegment : IMsgSegment, IEquatable<AtSegment>
{
    public Platform platform { get; set; }

    // all 表示全体成员
    public string value { get; set; }

    public AtSegment(string target, Platform platform)
    {
        this.value = target;
        this.platform = platform;
    }

    public string Build()
    {
        var platform = this.platform switch
        {
            Platform.OneBot => "qq",
            Platform.Guild => "gulid",
            Platform.Discord => "discord",
            Platform.KOOK => "kook",
            _ => "unknown",
        };
        return $"{platform}={value}";
    }

    public bool Equals(AtSegment? other)
    {
        return other != null && this.value == other.value && this.platform == other.platform;
    }

    public bool Equals(IMsgSegment? other)
    {
        if (other is AtSegment r)
            return this.Equals(r);
        else
            return false;
    }
}

public class ImageSegment : IMsgSegment, IEquatable<ImageSegment>
{
    public enum Type
    {
        File, // 如果是file就是文件地址
        Base64,
        Url
    }

    public Type t { get; set; }
    public string value { get; set; }

    public ImageSegment(string value, Type t)
    {
        this.value = value;
        this.t = t;
    }

    public string Build()
    {
        return this.t switch
        {
            Type.File => $"<image;file={this.value}>",
            Type.Base64 => $"<image;base64>",
            Type.Url => $"<image;url={this.value}>",
            // 保险
            _ => "",
        };
    }

    public bool Equals(ImageSegment? other)
    {
        return other != null && this.value == other.value && this.t == other.t;
    }

    public bool Equals(IMsgSegment? other)
    {
        if (other is ImageSegment r)
            return this.Equals(r);
        else
            return false;
    }
}

public class Chain : IEquatable<Chain>
{
    List<IMsgSegment> msgList { get; set; }

    public Chain()
    {
        this.msgList = new();
    }

    public static Chain FromList(List<IMsgSegment> list)
    {
        return new Chain { msgList = list };
    }

    public void Add(IMsgSegment n)
    {
        this.msgList.Add(n);
    }

    public Chain msg(string v)
    {
        this.Add(new TextSegment(v));
        return this;
    }

    public Chain at(string v, Platform p)
    {
        this.Add(new AtSegment(v, p));
        return this;
    }

    public Chain image(string v, ImageSegment.Type t)
    {
        this.Add(new ImageSegment(v, t));
        return this;
    }

    public IEnumerable<IMsgSegment> Iter()
    {
        return this.msgList.AsEnumerable();
    }

    public string Build()
    {
        var raw = "";
        foreach (var item in this.msgList)
        {
            raw += item.Build();
        }
        return raw;
    }

    public override string ToString()
    {
        return this.Build();
    }

    public int Length() => this.msgList.Count;

    public bool StartsWith(string s)
    {
        if (this.msgList.Count == 0)
            return false;
        else
            return this.msgList[0] is TextSegment t && t.value.StartsWith(s);
    }

    public bool StartsWith(AtSegment at)
    {
        if (this.msgList.Count == 0)
            return false;
        else
            return this.msgList[0] is AtSegment t
                && t.value == at.value
                && t.platform == at.platform;
    }

    public T? Find<T>()
        where T : class, IMsgSegment => this.msgList.Find(t => t is T) as T;

    public bool Equals(Chain? other)
    {
        if (other == null)
            return false;
        if (this.msgList.Count != other.msgList.Count)
            return false;
        for (int i = 0; i < this.msgList.Count; i++)
        {
            if (!this.msgList[i].Equals(other.msgList[i]))
                return false;
        }
        return true;
    }
}
