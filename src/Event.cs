using System.IO;
using System;
using Newtonsoft.Json.Linq;

namespace KanonBot.Event;

public interface IEvent
{
    string ToString();
}

public class RawEvent : IEvent
{
    public JObject value;
    public RawEvent(JObject e)
    {
        this.value = e;
    }

    public override string ToString()
    {
        return value.ToString();
    }
}

public class MessageEvent : IEvent
{
    public Message.Chain value;
    public MessageEvent(Message.Chain message)
    {
        this.value = message;
    }

    public override string ToString()
    {
        return value.ToString();
    }
}

public class HeartBeat : IEvent
{
    public DateTime value;
    public HeartBeat(long timestamp)
    {
        this.value = Utils.TimeStampSecToDateTime(timestamp).ToLocalTime();
    }

    public override string ToString()
    {
        return $"<heartbeat;time:{this.value.ToString()}>";
    }
}
