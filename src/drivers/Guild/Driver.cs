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
using System.Timers;

namespace KanonBot.Drivers;
public partial class Guild : IDriver
{

    public static readonly string DefaultEndPoint = "https://api.sgroup.qq.com";
    public static readonly string SandboxEndPoint = "https://sandbox.api.sgroup.qq.com";
    public static readonly Platform platform = Platform.Guild;
    IWebsocketClient client;
    Action<Target> msgAction;
    Action<IDriver, IEvent> eventAction;
    string authToken;
    Enums.Intent intents;
    System.Timers.Timer heartbeatTimer = new();
    int lastSeq = 0;
    public Guild(long appID, string token, Enums.Intent intents, bool sandbox = false)
    {
        // 初始化变量

        this.authToken = $"Bot {appID}.{token}";
        this.intents = intents;

        this.msgAction = (t) => { };
        this.eventAction = (c, e) => { };

        // 获取频道ws地址

        var res = (sandbox ? SandboxEndPoint : DefaultEndPoint)
            .AppendPathSegments("gateway", "bot")
            .WithHeader("Authorization", this.authToken)
            .GetJsonAsync<JObject>()
            .Result;

        Log.Debug("Guild.Driver.Init {0}", res.ToString(Formatting.None));

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
            client.Options.SetRequestHeader("Authorization", this.authToken);
            return client;
        });

        var client = new WebsocketClient(new Uri(url), factory);

        client.Name = "Guild";
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

        this.client = client;
    }

    void Parse(ResponseMessage msg)
    {
        var obj = Json.Deserialize<Models.PayloadBase<JToken>>(msg.Text)!;
        Log.Debug("收到消息: {@0} 数据: {1}", obj, obj.Data.ToString(Formatting.None));

        if (obj.Seq != null)
            this.lastSeq = obj.Seq.Value;   // 存储最后一次seq

        switch (obj.Operation)
        {
            case Enums.OperationCode.Hello:
                var heartbeatInterval = (obj.Data as JObject)!["heartbeat_interval"]!.Value<int>();
                
                SetHeartBeatTicker(heartbeatInterval);  // 设置心跳定时器
            
                var j = Json.Serialize(new Models.PayloadBase<Models.IdentityData> {    // 鉴权
                    Operation = Enums.OperationCode.Identify,
                    Data = new Models.IdentityData{
                        Token = this.authToken,
                        Intents = this.intents,
                        Shard = new int[] { 0, 1 },
                    }
                });
                Log.Debug(j);
                
                this.Send(j);
                break;
            default:
                break;
        }
        
    }

    void SetHeartBeatTicker(int interval)
    {
        HeartBeatTicker();
        this.heartbeatTimer = new System.Timers.Timer(interval);    // 初始化定时器
        this.heartbeatTimer.Elapsed += (s, e) =>
        {
            HeartBeatTicker();
        };
        this.heartbeatTimer.AutoReset = true;   // 设置定时器是否重复触发
        this.heartbeatTimer.Enabled = true;  // 启动定时器
    }

    void HeartBeatTicker()
    {
        Log.Debug("Sending heartbeat..");   // log（仅测试）
        var j = Json.Serialize(new Models.PayloadBase<Models.IdentityData> {
            Operation = Enums.OperationCode.Heartbeat,
            Seq = this.lastSeq
        });

        this.Send(j);
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
