using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Msg = KanonBot.Message;
using KanonBot.Event;
using KanonBot.Serializer;


namespace KanonBot.Drivers;

public enum Platform
{
    Unknown,
    OneBot,
    Guild
}

public interface IDriver
{
    IDriver onMessage(Action<Target> action);
    IDriver onEvent(Action<IDriver, IEvent> action);
    void Send(string message);
    void Send(Object obj) => Send(Json.Serialize(obj));
    Task Connect();
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
        var tasks = new Task[this.driverList.Count];
        for (int i = 0; i < this.driverList.Count; i++)
            tasks[i] = driverList[i].Connect();
        Task.WaitAll(tasks);
    }
}