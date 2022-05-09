using System.IO;
using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using KanonBot.Drivers;

namespace KanonBot.Message;

public interface IMsgSegment
{
    string Build();
}

public class RawSegment : IMsgSegment
{
    public Object value;
    public string type;
    public RawSegment(string type, Object value)
    {
        this.type = type;
        this.value = value;
    }

    public string Build()
    {
        return value switch {
            JObject j => $"<raw;{type}={j.ToString(Formatting.None)}>",
            _ => $"<raw;{type}={value}>",
        };
    }
}
public class TextSegment : IMsgSegment
{
    public string value;
    public TextSegment(string msg)
    {
        this.value = msg;
    }

    public string Build()
    {
        return value.ToString();
    }
}
public class EmojiSegment : IMsgSegment
{
    public string value;
    public EmojiSegment(string value)
    {
        this.value = value;
    }

    public string Build()
    {
        return $"<Face;id={value}>";
    }
}
public class AtSegment : IMsgSegment
{
    public Platform platform;
    // all 表示全体成员
    public string value;
    public AtSegment(string target, Platform platform)
    {
        this.value = target;
        this.platform = platform;
    }

    public string Build()
    {
        return $"<at;{platform.ToString()}={value}>";
    }
}

public class ImageSegment : IMsgSegment
{
    public enum Type
    {
        File,   // 如果是file就是文件地址
        Base64,
        Url
    }
    public Type t;
    public string value;
    public ImageSegment(string value, Type t)
    {
        this.value = value;
        this.t = t;
    }

    public string Build()
    {
        switch (this.t)
        {
            case Type.File:
                return $"<image;file={this.value}>";
            case Type.Base64:
                return $"<image;base64>";
            case Type.Url:
                return $"<image;url={this.value}>";
        }
        // 保险
        return "";
    }
}

public class Chain
{
    List<IMsgSegment> msgList;
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

    public Chain image(string v, ImageSegment.Type t)
    {
        this.Add(new ImageSegment(v, t));
        return this;
    }

    public List<IMsgSegment> ToList()
    {
        return this.msgList;
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
            return this.msgList[0] is AtSegment t && t.value == at.value && t.platform == at.platform;
    }
}