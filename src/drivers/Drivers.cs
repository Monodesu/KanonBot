using KanonBot.Event;
using KanonBot.Serializer;


namespace KanonBot.Drivers;

public enum Platform
{
    Unknown,
    OneBot,
    Guild,
    KOOK
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

public class Drivers
{
    List<IDriver> driverList;
    public Drivers()
    {
        this.driverList = new();
    }
    public void append(IDriver n)
    {
        this.driverList.Add(n);
    }

    public void StartAll()
    {
        foreach (var driver in this.driverList)
            driver.Start();
    }

    public void StopAll()
    {
        foreach (var driver in this.driverList)
            driver.Dispose();
    }
}
