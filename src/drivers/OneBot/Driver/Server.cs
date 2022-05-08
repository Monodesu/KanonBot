using System.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public class Socket : ISocket {
            public API api;
            IWebSocketConnection socket;
            public Socket(IWebSocketConnection socket) {
                this.api = new(this);
                this.socket = socket;
            }
            public void Send(string message) {
                this.socket.Send(message);
            }
        }
        Dictionary<Guid, Socket> clients;
        WebSocketServer instance;
        public Server(string url)
        {
            var server = new WebSocketServer(url);
            server.RestartAfterListenError = true;

            this.instance = server;
            this.clients = new();
        }

        void SocketAction(IWebSocketConnection socket)
        {
            // 获取请求头数据
            // 数据验证失败后直接断开链接
            if (!socket.ConnectionInfo.Headers.TryGetValue("X-Self-ID", out string? selfId))
            {
                this.Disconnect(socket);
                return;
            }

            if (!socket.ConnectionInfo.Headers.TryGetValue("X-Client-Role", out string? role))
            {
                this.Disconnect(socket);
                return;
            }

            if (role != "Universal")
            {
                this.Disconnect(socket);
                return;
            }

            

            socket.OnOpen = () =>
            {
                this.clients.Add(socket.ConnectionInfo.Id, new Socket(socket));
                Log.Information($"连接[{socket.ConnectionInfo.ClientIpAddress}:{socket.ConnectionInfo.ClientPort}]");
            };
            socket.OnClose = () =>
            {
                this.Disconnect(socket);
                Log.Information($"断开[{socket.ConnectionInfo.ClientIpAddress}:{socket.ConnectionInfo.ClientPort}]");
            };
            socket.OnMessage = message => Task.Run(() =>
            {
                try
                {
                    this.Parse(message, this.clients[socket.ConnectionInfo.Id]);
                }
                catch (Exception ex) { 
                    Log.Error("未捕获的异常 ↓\n{ex}", ex);
                    socket.Close();
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
                            dynamic? obj;
                            try
                            {
                                obj = (string?)m["message_type"] switch
                                {
                                    "private" => m.ToObject<Models.PrivateMessage>(),
                                    "group" => m.ToObject<Models.GroupMessage>(),
                                    _ => throw new NotSupportedException("未知的消息类型")
                                };
                            }
                            catch (JsonSerializationException)
                            {
                                throw new NotSupportedException($"不支持的消息格式，请使用数组消息格式");
                            }
                            var target = new Target
                            {
                                msg = Message.Parse(obj!.MessageList),
                                raw = obj,
                                api = socket.api
                            };
                            this.msgAction(target);
                            break;

                        case "meta_event":
                            var metaEventType = (string?)m["meta_event_type"];
                            if (metaEventType == "heartbeat")
                                this.eventAction(socket, new HeartBeat((long)m["time"]!));
                            else if (metaEventType == "lifecycle")
                                this.eventAction(socket, new Lifecycle((string)m["self_id"]!, Platform.OneBot));
                            else
                                this.eventAction(socket, new RawEvent(m));

                            break;

                        default:
                            this.eventAction(socket, new RawEvent(m));
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

        void Disconnect(IWebSocketConnection socket)
        {
            socket.Close();
            this.clients.Remove(socket.ConnectionInfo.Id);
        }


        public IDriver onMessage(Action<Target> action)
        {
            this.msgAction = action;
            return this;
        }
        public IDriver onEvent(Action<ISocket, IEvent> action)
        {
            this.eventAction = action;
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
