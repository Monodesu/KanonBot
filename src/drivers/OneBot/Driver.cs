using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.WebSockets;
using Websocket.Client;
using KanonBot.Message;
using KanonBot.Serializer;
using KanonBot.Event;
using Newtonsoft.Json;
using Serilog;
using KanonBot;

namespace KanonBot.Drivers;
public partial class OneBot
{
    public static readonly Platform platform = Platform.OneBot;
    event IDriver.MessageDelegate? msgAction;
    event IDriver.EventDelegate? eventAction;
    public OneBot()
    {
        // 初始化变量
    }



}
