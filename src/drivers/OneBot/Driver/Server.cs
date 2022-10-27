using System.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Net.WebSockets;
using Fleck;
using KanonBot.Message;
using KanonBot.Serializer;
using KanonBot.Event;
using Newtonsoft.Json;
using Serilog;
using KanonBot;

namespace KanonBot.Drivers;
public partial class OneBot
{

    public class Server : OneBot, IDriver
    {
        public class Socket : ISocket
        {
            public API api;
            IWebSocketConnection inner;
            public string? selfID { get; private set; }
            public Socket(IWebSocketConnection socket)
            {
                this.api = new(this);
                this.inner = socket;
                this.selfID = socket.ConnectionInfo.Headers["X-Self-ID"];
            }
            public void Send(string message)
            {
                this.inner.Send(message);
            }
            public IWebSocketConnectionInfo ConnectionInfo()
            {
                return this.inner.ConnectionInfo;
            }
            public void Close()
            {
                this.inner.Close();
            }
        }
        class Clients
        {
            private Dictionary<Guid, Socket> inner = new();
            private Mutex mut = new();

            public Socket? Get(Guid k) {
                return this.inner.GetValueOrDefault(k);
            }
            public void Set(Guid k, Socket v) {
                mut.WaitOne();
                this.inner.Add(k, v);
                mut.ReleaseMutex();
            }
            public Socket? Remove(Guid k) {
                Socket? s;
                mut.WaitOne();
                this.inner.Remove(k, out s);
                mut.ReleaseMutex();
                return s;
            }

        }
        Clients clients = new();
        WebSocketServer instance;
        public Server(string url)
        {
            var server = new WebSocketServer(url);
            server.RestartAfterListenError = true;
            Fleck.FleckLog.LogAction = (level, message, ex) =>
            {
                switch (level)
                {
                    case LogLevel.Debug:
                        // 不要debug
                        // Log.Debug($"[{OneBot.platform} Core] {message}", ex);
                        break;
                    case LogLevel.Error:
                        Log.Error($"[{OneBot.platform} Core] {message}", ex);
                        break;
                    case LogLevel.Warn:
                        Log.Warning($"[{OneBot.platform} Core] {message}", ex);
                        break;
                    default:
                        Log.Information($"[{OneBot.platform} Core] {message}", ex);
                        break;
                }
            };
            this.instance = server;
        }

        void SocketAction(IWebSocketConnection socket)
        {
            // 获取请求头数据
            // 数据验证失败后直接断开链接

            if (!socket.ConnectionInfo.Headers.TryGetValue("X-Client-Role", out string? role))
            {
                socket.Close();
                return;
            }

            if (role != "Universal")
            {
                socket.Close();
                return;
            }


            socket.OnError = (e) =>
            {
                this.Disconnect(socket.ConnectionInfo.Id);
                Log.Error($"[{OneBot.platform} Core] {socket.ConnectionInfo.ClientIpAddress}:{socket.ConnectionInfo.ClientPort} 连接异常断开");
            };
            socket.OnOpen = () =>
            {
                this.clients.Set(socket.ConnectionInfo.Id, new Socket(socket));
                Log.Information($"[{OneBot.platform} Core] 收到来自 {socket.ConnectionInfo.ClientIpAddress}:{socket.ConnectionInfo.ClientPort} 的连接");
            };
            socket.OnClose = () =>
            {
                this.Disconnect(socket.ConnectionInfo.Id);
                Log.Information($"[{OneBot.platform} Core] {socket.ConnectionInfo.ClientIpAddress}:{socket.ConnectionInfo.ClientPort} 连接断开");
            };
            socket.OnMessage = message => Task.Run(() =>
            {
                try
                {
                    var s = this.clients.Get(socket.ConnectionInfo.Id);
                    if (s != null)
                        this.Parse(message, s);
                    else
                        socket.Close();
                }
                catch (Exception ex)
                {
                    Log.Error("未捕获的异常 ↓\n{ex}", ex);
                    this.Disconnect(socket.ConnectionInfo.Id);
                }
            });
        }

        void Parse(string msg, Socket socket)
        {
            var m = Json.ToLinq(msg);
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
                                obj = (string?)m["message_type"] switch
                                {
                                    "private" => m.ToObject<Models.PrivateMessage>()!,
                                    "group" => m.ToObject<Models.GroupMessage>()!,
                                    _ => throw new NotSupportedException("未知的消息类型")
                                };
                            }
                            catch (JsonSerializationException)
                            {
                                // throw new NotSupportedException($"不支持的消息格式，请使用数组消息格式");
                                Log.Error("不支持的消息格式，请使用数组消息格式");
                                this.Disconnect(socket.ConnectionInfo().Id);
                                return;
                            }
                            var target = new Target
                            {
                                platform = Platform.OneBot,
                                account = socket.selfID,
                                msg = Message.Parse(obj.MessageList),
                                raw = obj,
                                socket = socket
                            };
                            this.msgAction?.Invoke(target);
                            break;

                        case "meta_event":
                            var metaEventType = (string?)m["meta_event_type"];
                            if (metaEventType == "heartbeat")
                            {
                                this.eventAction?.Invoke(socket, new HeartBeat((long)m["time"]!));
                            }
                            else if (metaEventType == "lifecycle")
                            {
                                this.eventAction?.Invoke(socket, new Ready((string)m["self_id"]!, Platform.OneBot));
                            }
                            else
                            {
                                this.eventAction?.Invoke(socket, new RawEvent(m));
                            }
                            break;

                        default:
                            this.eventAction?.Invoke(socket, new RawEvent(m));
                            break;
                    }
                }
                // 处理回执消息
                if (m["echo"] != null)
                {
                    socket.api.Echo(m.ToObject<Models.CQResponse>()!);
                }
            }
        }

        void Disconnect(Guid id)
        {
            var s = this.clients.Remove(id);
            s?.Close();
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

        public Task Start()
        {
            return Task.Run(() => this.instance.Start(SocketAction));
        }
        public void Dispose()
        {
            this.instance.Dispose();
        }

    }
}
