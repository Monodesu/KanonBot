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
using khl = KaiHeiLa;
using KaiHeiLa.WebSocket;

namespace KanonBot.Drivers;
public partial class KOOK : ISocket, IDriver
{
    public static readonly Platform platform = Platform.Guild;
    public string? selfID { get; private set; }
    KaiHeiLaSocketClient instance;
    event IDriver.MessageDelegate? msgAction;
    event IDriver.EventDelegate? eventAction;
    string token;
    public API api;
    public KOOK(string token, string botID)
    {
        // 初始化变量
        this.token = token;
        this.selfID = botID;

        this.api = new(token);

        var client = new KaiHeiLaSocketClient();
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
    private static async Task LogAsync(khl.LogMessage message)
    {
        var severity = message.Severity switch
        {
            khl.LogSeverity.Critical => LogEventLevel.Fatal,
            khl.LogSeverity.Error => LogEventLevel.Error,
            khl.LogSeverity.Warning => LogEventLevel.Warning,
            khl.LogSeverity.Info => LogEventLevel.Information,
            khl.LogSeverity.Verbose => LogEventLevel.Verbose,
            khl.LogSeverity.Debug => LogEventLevel.Debug,
            _ => LogEventLevel.Information
        };
        Log.Write(severity, message.Exception, "[KOOK] [{Source}] {Message}", message.Source, message.Message);
        await Task.CompletedTask;
    }


    private void Parse(SocketMessage message)
    {
        // 过滤掉bot消息和系统消息
        if (message.Source != khl.MessageSource.User)
            return;

        this.msgAction?.Invoke(new Target()
        {
            platform = Platform.KOOK,
            account = this.selfID,
            msg = Message.Parse(message),
            raw = message,
            socket = this
        });
    }
    private async Task ParseUpdateMessage(khl.Cacheable<khl.IMessage, Guid> before, SocketMessage after, ISocketMessageChannel channel)
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
        await this.instance.LoginAsync(khl.TokenType.Bot, this.token);
        await this.instance.StartAsync();
    }

    public void Dispose()
    {
        this.instance.Dispose();
    }
}
