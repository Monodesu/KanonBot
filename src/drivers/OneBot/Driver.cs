using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.WebSockets;
using Websocket.Client;
using KanonBot.WebSocket;
using KanonBot.Message;
using KanonBot.Serializer;
using KanonBot.Event;
using Newtonsoft.Json;
using Serilog;
using KanonBot;

namespace KanonBot.Drivers;
public partial class OneBot
{
    public class Driver : IDriver
    {
        public static readonly Platform platform = Platform.OneBot;
        IWebsocketClient client;
        Action<Target> msgAction;
        Action<IDriver, IEvent> eventAction;
        API api;
        public Driver(string url)
        {
            // 初始化变量

            this.msgAction = (t) => {};
            this.eventAction = (c, e) => {};
            this.api = new(this);

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
                //client.Options.SetRequestHeader("Origin", "xxx");
                return client;
            });

            var client = new WebsocketClient(new Uri(url), factory);

            client.Name = "OneBot";
            client.ReconnectTimeout = TimeSpan.FromSeconds(30);
            client.ErrorReconnectTimeout = TimeSpan.FromSeconds(30);
            // client.ReconnectionHappened.Subscribe(info =>
            //     Console.WriteLine($"Reconnection happened, type: {info.Type}, url: {client.Url}"));
            // client.DisconnectionHappened.Subscribe(info =>
            //     Console.WriteLine($"Disconnection happened, type: {info.Type}"));
            
            // 拿Tasks异步执行
            client.MessageReceived.Subscribe(msgAction => Task.Run(() => {
                try
                {
                    this.Parse(msgAction);
                }
                catch (System.Exception ex) { Log.Error("未捕获的异常 ↓\n{ex}", ex); }
            }));

            this.client = client;
        }

        void Parse(ResponseMessage msg)
        {
            var m = Json.ToLinq(msg.Text);
            if (m != null)
            {
                if (m["post_type"] != null)
                {
                    switch ((string?)m["post_type"])
                    {
                        case "message":
                            dynamic obj;
                            try
                            {
                                obj = (string?)m["message_type"] switch {
                                    "private" => m.ToObject<Models.PrivateMessage>(),
                                    "group" => m.ToObject<Models.GroupMessage>()
                                };
                            }
                            catch (JsonSerializationException ex)
                            {
                                throw new NotSupportedException($"不支持的消息格式，请使用数组消息格式");
                            }
                            var target = new Target{
                                msg = Message.Parse(obj.MessageList),
                                raw = obj,
                                api = this.api
                            };
                            this.msgAction(target);
                            break;

                        case "meta_event":
                            var metaEventType = (string?)m["meta_event_type"];
                            if (metaEventType == "heartbeat")
                                this.eventAction(this, new HeartBeat((long)m["time"]!));
                            else if (metaEventType == "lifecycle")
                                this.eventAction(this, new Lifecycle((string)m["self_id"]!, Platform.OneBot));
                            else
                                this.eventAction(this, new RawEvent(m));
                            
                            break;

                        default:
                            this.eventAction(this, new RawEvent(m));
                            break;
                    }
                }
                // 处理回执消息
                if (m["echo"] != null)
                {
                    this.api.Echo(m.ToObject<Models.CQResponse>());
                }
            }
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
            // if (this.eventAction == null || this.msgAction == null)
            // {
            //     throw new KanonError("no action assigned");
            // }
            return this.client.Start();
        }
    }
}
