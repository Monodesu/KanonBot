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
using Serilog.Events;
using Newtonsoft.Json.Linq;
using System.Timers;
using libKook = Kook;
using Kook.WebSocket;

namespace KanonBot.Drivers;
public partial class Kook : ISocket, IDriver
{
    public static readonly Platform platform = Platform.Guild;
    public string? selfID { get; private set; }
    KookSocketClient instance;
    event IDriver.MessageDelegate? msgAction;
    event IDriver.EventDelegate? eventAction;
    string token;
    public API api;
    public Kook(string token, string botID)
    {
        // 初始化变量
        this.token = token;
        this.selfID = botID;

        this.api = new(token);

        var client = new KookSocketClient();
        client.Log += LogAsync;

        // client.MessageUpdated += this.Parse;
        client.DirectMessageReceived += msg => Task.Run(() =>
        {
            try
            {
                this.Parse(msg);
            }
            catch (Exception ex) { Log.Error("未捕获的异常 ↓\n{ex}", ex); }
        });
        client.MessageReceived += msg => Task.Run(() =>
        {
            try
            {
                this.Parse(msg);
            }
            catch (Exception ex) { Log.Error("未捕获的异常 ↓\n{ex}", ex); }
        });
        client.Ready += () =>
        {
            // 连接成功
            return Task.CompletedTask;
        };

        this.instance = client;
    }
    private static async Task LogAsync(libKook.LogMessage message)
    {
        var severity = message.Severity switch
        {
            libKook.LogSeverity.Critical => LogEventLevel.Fatal,
            libKook.LogSeverity.Error => LogEventLevel.Error,
            libKook.LogSeverity.Warning => LogEventLevel.Warning,
            libKook.LogSeverity.Info => LogEventLevel.Information,
            libKook.LogSeverity.Verbose => LogEventLevel.Verbose,
            libKook.LogSeverity.Debug => LogEventLevel.Debug,
            _ => LogEventLevel.Information
        };
        Log.Write(severity, message.Exception, "[KOOK] [{Source}] {Message}", message.Source, message.Message);
        await Task.CompletedTask;
    }


    private void Parse(SocketMessage message)
    {
        // 过滤掉bot消息和系统消息
        if (message.Source != libKook.MessageSource.User)
        {
            this.eventAction?.Invoke(
                this,
                new RawEvent(message)
            );
        }
        else
        {
            this.msgAction?.Invoke(new Target()
            {
                platform = Platform.KOOK,
                sender = message.Author.Id.ToString(),
                selfAccount = this.selfID,
                msg = Message.Parse(message),
                raw = message,
                socket = this
            });
        }

    }
    private async Task ParseUpdateMessage(libKook.Cacheable<libKook.IMessage, Guid> before, SocketMessage after, ISocketMessageChannel channel)
    {
        var message = await before.GetOrDownloadAsync();
        Log.Debug($"{message} -> {after}");
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
        throw new NotSupportedException("不支持");
    }

    public async Task Start()
    {
        // return this.instance.Start();
        await this.instance.LoginAsync(libKook.TokenType.Bot, this.token);
        await this.instance.StartAsync();
    }

    public void Dispose()
    {
        this.instance.Dispose();
    }
}
