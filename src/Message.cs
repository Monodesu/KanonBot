using System.IO;
using System;

namespace KanonBot.Message;

public interface IMsg
{
    string toRaw();
}

public class RawMessage : IMsg
{
    public string value;
    public RawMessage(string msg)
    {
        this.value = msg;
    }

    public string toRaw()
    {
        return value;
    }
}

public class Image : IMsg
{
    public enum Type
    {
        file,
        base64,
        url
    }
    public Type t;
    public string value;
    public Image(string value, Type t)
    {
        this.value = value;
        this.t = t;
    }

    public string toRaw()
    {
        switch (this.t)
        {
            case Type.file:
                return $"<image:file:///{this.value}>";
            case Type.base64:
                return $"<image:base64:///{this.value}>";
            case Type.url:
                return $"<image:file:///{this.value}>";
        }
        // 保险
        return "";
    }
}

public class Chain
{
    List<IMsg> msgList;
    public Chain()
    {
        this.msgList = new();
    }

    public void append(IMsg n)
    {
        this.msgList.Add(n);
    }

    public Chain msg(string v)
    {
        this.append(new RawMessage(v));
        return this;
    }

    public Chain image(string v, Image.Type t)
    {
        this.append(new Image(v, t));
        return this;
    }

    public string build()
    {
        var raw = "";
        foreach (var item in this.msgList)
        {
            raw += item.toRaw();
        }
        return raw;
    }

    public override string ToString()
    {
        return this.build();
    }
}