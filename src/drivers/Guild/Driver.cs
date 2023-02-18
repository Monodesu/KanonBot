using System.Net.WebSockets;
using Websocket.Client;
using KanonBot.Serializer;
using KanonBot.Event;
using Newtonsoft.Json.Linq;
namespace KanonBot.Drivers;
public partial class Guild : ISocket, IDriver
{
    public static readonly Platform platform = Platform.Guild;
    public string? selfID { get; private set; }
    IWebsocketClient instance;
    event IDriver.MessageDelegate? msgAction;
    event IDriver.EventDelegate? eventAction;
    public API api;
    string AuthToken;
    Guid? SessionId;
    Enums.Intent intents;
    System.Timers.Timer heartbeatTimer = new();
    int lastSeq = 0;
    public Guild(long appID, string token, Enums.Intent intents, bool sandbox = false)
    {
        // 初始化变量

        this.AuthToken = $"Bot {appID}.{token}";
        this.intents = intents;

        this.api = new(AuthToken, sandbox);

        // 获取频道ws地址

        var url = api.GetWebsocketUrl().Result;

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

        var client = new WebsocketClient(new Uri(url), factory)
        {
            Name = "Guild",
            ReconnectTimeout = null,
            ErrorReconnectTimeout = TimeSpan.FromSeconds(30)
        };
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

    void Dispatch<T>(Models.PayloadBase<T> obj)
    {
        switch (obj.Type)
        {
            case Enums.EventType.Ready:
                var readyData = (obj.Data as JObject)?.ToObject<Models.ReadyData>();
                this.SessionId = readyData!.SessionId;
                this.selfID = readyData.User.ID;
                Log.Information("鉴权成功 {@0}", readyData);
                this.eventAction?.Invoke(this, new Ready(readyData.User.ID, Platform.Guild));
                break;
            case Enums.EventType.AtMessageCreate:
                var MessageData = (obj.Data as JObject)?.ToObject<Models.MessageData>();
                this.msgAction?.Invoke(new Target() {
                    platform = Platform.Guild,
                    sender = MessageData!.Author.ID,
                    selfAccount = this.selfID,
                    msg = Message.Parse(MessageData!),
                    raw = MessageData,
                    socket = this
                });
                break;
            case Enums.EventType.Resumed:
                // 恢复连接成功
                // 不做任何事
                break;
            default:
                this.eventAction?.Invoke(this, new RawEvent(obj));
                break;
        }
    }

    void Parse(ResponseMessage msg)
    {
        var obj = Json.Deserialize<Models.PayloadBase<JToken>>(msg.Text)!;
        // Log.Debug("收到消息: {@0} 数据: {1}", obj, obj.Data?.ToString(Formatting.None) ?? null);

        if (obj.Seq != null)
            this.lastSeq = obj.Seq.Value;   // 存储最后一次seq

        switch (obj.Operation)
        {
            case Enums.OperationCode.Dispatch:
                this.Dispatch(obj);
                break;
            case Enums.OperationCode.Hello:
                var heartbeatInterval = (obj.Data as JObject)!["heartbeat_interval"]!.Value<int>();

                SetHeartBeatTicker(heartbeatInterval);  // 设置心跳定时器

                this.Send(this.SessionId switch {
                    null => Json.Serialize(new Models.PayloadBase<Models.IdentityData> {    // 鉴权
                    Operation = Enums.OperationCode.Identify,
                    Data = new Models.IdentityData{
                        Token = this.AuthToken,
                        Intents = this.intents,
                        Shard = new int[] { 0, 1 },
                    }
                    }),
                    not null => Json.Serialize(new Models.PayloadBase<Models.ResumeData> {    // 鉴权
                    Operation = Enums.OperationCode.Resume,
                    Data = new Models.ResumeData{
                        Token = this.AuthToken,
                        SessionId = this.SessionId.Value,
                        Seq = this.lastSeq,
                    }
                    })
                });
                break;
            case Enums.OperationCode.Reconnect:
                this.instance.Reconnect();    // 重连
                break;
            case Enums.OperationCode.InvalidSession:
                this.Dispose();      // 销毁客户端
                throw new KanonError("无效的session，需要重新鉴权");
            case Enums.OperationCode.HeartbeatACK:
                // 无需处理
                break;
            default:
                break;
        }

    }

    void SetHeartBeatTicker(int interval)
    {
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
        // Log.Debug("Sending heartbeat..");   // log（仅测试）
        var j = Json.Serialize(new Models.PayloadBase<Models.IdentityData> {
            Operation = Enums.OperationCode.Heartbeat,
            Seq = this.lastSeq
        });

        this.Send(j);
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
