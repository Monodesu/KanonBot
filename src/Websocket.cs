using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KanonBot.Message;

namespace KanonBot.WebSocket
{
    public interface IDriver
    {
        IDriver SubscribeMessage(Action<Chain> action);
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
}