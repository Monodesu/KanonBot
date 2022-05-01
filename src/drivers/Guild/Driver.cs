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
using Flurl;
using Flurl.Http;
using Newtonsoft.Json.Linq;

namespace KanonBot.Drivers;
public partial class Guild : IDriver
{

    public static readonly Platform platform = Platform.Guild;
    IWebsocketClient client;
    Action<Target> msgAction;
    Action<IDriver, IEvent> eventAction;
    public Guild()
    {
        // 初始化变量

        var config = Config.inner!.guild!;
        var auth = $"Bot {config.appID}.{config.token}";

        this.msgAction = (t) => { };
        this.eventAction = (c, e) => { };

        // 获取频道ws地址

        var res = "https://sandbox.api.sgroup.qq.com"
            .AppendPathSegment("gateway")
            .WithHeader("Authorization", auth)
            .GetJsonAsync<JObject>()
            .Result;

        var url = res["url"]!.ToString();

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
            return client;
        });

        var client = new WebsocketClient(new Uri(url), factory);

        client.Name = "Guild";
        client.ReconnectTimeout = TimeSpan.FromSeconds(45);
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

        this.client = client;
    }

    void Parse(ResponseMessage msg)
    {
        var obj = Json.Deserialize<Models.EventBase<JObject>>(msg.Text)!;
        Log.Debug("收到消息: {@0} 数据: {1}", obj, obj.Data.ToString(Formatting.None));

    }

    public IDriver onMessage(Action<Target> action)
    {
        this.msgAction = action;
        return this;
    }
    public IDriver onEvent(Action<IDriver, IEvent> action)
    {
        this.eventAction = action;
        return this;
    }

    public void Send(string message)
    {
        this.client.Send(message);
    }

    public Task Connect()
    {
        return this.client.Start();
    }
}
