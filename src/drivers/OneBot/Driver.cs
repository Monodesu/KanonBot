using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using KanonBot;
using KanonBot.Event;
using KanonBot.Message;
using KanonBot.Serializer;
using Newtonsoft.Json;
using Serilog;
using Websocket.Client;

namespace KanonBot.Drivers;

public partial class OneBot
{
    public static readonly Platform platform = Platform.OneBot;
    event IDriver.MessageDelegate? msgAction;
    event IDriver.EventDelegate? eventAction;
}
