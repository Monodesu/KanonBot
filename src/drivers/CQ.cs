using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.WebSockets;
using Websocket.Client;
using KanonBot.WebSocket;
using KanonBot.Message;

namespace KanonBot.Drivers
{
    public class CQ : IDriver
    {
        IWebsocketClient client;
        Action<Chain>? onMessage;
        public CQ(string url)
        {
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

            client.Name = "KanonBot";
            client.ReconnectTimeout = TimeSpan.FromSeconds(30);
            client.ErrorReconnectTimeout = TimeSpan.FromSeconds(30);
            client.ReconnectionHappened.Subscribe(info =>
            {
                Console.WriteLine($"Reconnection happened, type: {info.Type}, url: {client.Url}");
            });
            client.DisconnectionHappened.Subscribe(info =>
                Console.WriteLine($"Disconnection happened, type: {info.Type}"));

            client.MessageReceived.Subscribe(this.ParseMessage);

            this.client = client;
        }

        void ParseMessage(ResponseMessage msg)
        {
            this.onMessage!(new Chain().msg(msg.Text));
        }

        public IDriver SubscribeMessage(Action<Chain> action)
        {
            this.onMessage = action;
            return this;
        }

        public Task Connect()
        {
            return this.client.Start();
        }
    }
}