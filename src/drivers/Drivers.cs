using KanonBot.Event;
using KanonBot.Serializer;


namespace KanonBot.Drivers;

public enum Platform
{
    Unknown,
    OneBot,
    Guild,
    KOOK,
    Discord,
    OSU
}

public interface IDriver
{
    delegate void MessageDelegate(Target target);
    delegate void EventDelegate(ISocket socket, IEvent kevent);
    IDriver onMessage(MessageDelegate action);
    IDriver onEvent(EventDelegate action);
    Task Start();
    void Dispose();
}
public interface ISocket
{
    string? selfID { get; }
    void Send(string message);
    void Send(Object obj) => Send(Json.Serialize(obj));
}

public interface IReply
{
    void Reply(Target target, Message.Chain msg);
}

public class Drivers
{
    List<IDriver> driverList;
    public Drivers()
    {
        this.driverList = new();
    }
    public Drivers append(IDriver n)
    {
        this.driverList.Add(n);
        return this;
    }

    public Drivers StartAll()
    {
        foreach (var driver in this.driverList)
            driver.Start();
        return this;
    }

    public void StopAll()
    {
        foreach (var driver in this.driverList)
            driver.Dispose();
    }
}
