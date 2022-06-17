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
using Newtonsoft.Json.Linq;
using System.Timers;

namespace KanonBot.Drivers;
public partial class Kaiheila : ISocket, IDriver
{
    public static readonly Platform platform = Platform.Guild;
    public string? selfID { get; private set; }
    IWebsocketClient instance;
    event IDriver.MessageDelegate? msgAction;
    event IDriver.EventDelegate? eventAction;
    public API api;
    string AuthToken;
    Guid? SessionId;
    System.Timers.Timer heartbeatTimer = new();
    int lastSeq = 0;
    public Kaiheila(string token, string botID)
    {
        // 初始化变量


        this.api = new(token);

        // 获取ws

        var url = api.GetWebsocketUrl();

        // 初始化ws

        var factory = new Func<ClientWebSocket>(() =>
        {
            var client = new ClientWebSocket
            {
                Options =
                {
                        KeepAliveInterval = TimeSpan.FromSeconds(5),
                        // Proxy = ...
                        // ClientCertificates = ...

                }
            };
            client.Options.SetRequestHeader("Authorization", this.AuthToken);
            return client;
        });

        var client = new WebsocketClient(new Uri(null), factory);

        client.Name = "Kaiheila";
        client.ReconnectTimeout = null;
        client.ErrorReconnectTimeout = TimeSpan.FromSeconds(30);
        // client.ReconnectionHappened.Subscribe(info =>
        //     Console.WriteLine($"Reconnection happened, type: {info.Type}, url: {client.Url}"));
        // client.DisconnectionHappened.Subscribe(info =>
        //     Console.WriteLine($"Disconnection happened, type: {info.Type}"));

        // 拿Tasks异步执行
        client.MessageReceived.Subscribe(msgAction => Task.Run(() =>
        {
            try
            {
                this.Parse(msgAction);
            }
            catch (Exception ex) { Log.Error("未捕获的异常 ↓\n{ex}", ex); }
        }));

        this.instance = client;
    }


    void Parse(ResponseMessage msg)
    {
        var obj = Json.Deserialize<JToken>(msg.Text)!;
        Log.Debug(obj.ToString());
    }


    public IDriver onMessage(IDriver.MessageDelegate action)
    {
        this.msgAction += action;
        return this;
    }
    public IDriver onEvent(IDriver.EventDelegate action)
    {
        this.eventAction += action;
        return this;
    }

    public void Send(string message)
    {
        this.instance.Send(message);
    }

    public Task Start()
    {
        return this.instance.Start();
    }

    public void Dispose()
    {
        this.instance.Dispose();
    }
}
