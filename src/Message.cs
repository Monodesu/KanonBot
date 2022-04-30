using System.IO;
using System;

namespace KanonBot.Message;

public interface IMsgSegment
{
    string Build();
}

public class RawMessage : IMsgSegment
{
    public string value;
    public RawMessage(string msg)
    {
        this.value = msg;
    }

    public string Build()
    {
        return value;
    }
}
public class AtSegment : IMsgSegment
{
    public enum Platform
    {
        QQ
    }
    public Platform platform;
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
        File,
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
                return $"<image;file:///{this.value}>";
            case Type.Base64:
                return $"<image;base64:///{this.value}>";
            case Type.Url:
                return $"<image;file:///{this.value}>";
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

    public void append(IMsgSegment n)
    {
        this.msgList.Add(n);
    }

    public Chain msg(string v)
    {
        this.append(new RawMessage(v));
        return this;
    }

    public Chain image(string v, ImageSegment.Type t)
    {
        this.append(new ImageSegment(v, t));
        return this;
    }

    public List<IMsgSegment> GetList()
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
}