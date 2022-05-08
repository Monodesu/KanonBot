using System.IO;
using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using KanonBot.Drivers;

namespace KanonBot.Event;

public interface IEvent
{
    string ToString();
}

public class RawEvent : IEvent
{
    public object value;
    public RawEvent(object e)
    {
        this.value = e;
    }

    public override string ToString()
    {
        return value switch {
            JObject j => j.ToString(Formatting.None),
            _ => $"{value}",
        };
    }
}


public class Ready : IEvent
{
    public string selfId;
    public Platform platform;
    public Ready(string selfId, Platform platform)
    {
        this.selfId = selfId;
        this.platform = platform;
    }

    public override string ToString()
    {
        return $"<ready;selfId={this.selfId},platform={this.platform}>";
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
        return $"<heartbeat;time={this.value.ToString()}>";
    }
}
