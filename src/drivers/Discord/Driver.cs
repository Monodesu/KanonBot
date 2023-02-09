using Discord;
using Discord.WebSocket;
using KanonBot.Event;
using Serilog.Events;

namespace KanonBot.Drivers;
public partial class Discord : ISocket, IDriver
{
    public static readonly Platform platform = Platform.Discord;
    public string? selfID { get; private set; }
    DiscordSocketClient instance;
    event IDriver.MessageDelegate? msgAction;
    event IDriver.EventDelegate? eventAction;
    string token;
    public API api;
    public Discord(string token, string botID)
    {
        // 初始化变量
        this.token = token;
        this.selfID = botID;

        this.api = new(token);

        var client = new DiscordSocketClient();
        client.Log += LogAsync;

        // client.MessageUpdated += this.Parse;
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
    private static async Task LogAsync(LogMessage message)
    {
        var severity = message.Severity switch
        {
            LogSeverity.Critical => LogEventLevel.Fatal,
            LogSeverity.Error => LogEventLevel.Error,
            LogSeverity.Warning => LogEventLevel.Warning,
            LogSeverity.Info => LogEventLevel.Information,
            LogSeverity.Verbose => LogEventLevel.Verbose,
            LogSeverity.Debug => LogEventLevel.Debug,
            _ => LogEventLevel.Information
        };
        Log.Write(severity, message.Exception, "[Discord] [{Source}] {Message}", message.Source, message.Message);
        await Task.CompletedTask;
    }


    private void Parse(SocketMessage message)
    {
        // 过滤掉bot消息和系统消息
        if (message.Source != MessageSource.User)
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
        await this.instance.LoginAsync(TokenType.Bot, this.token);
        await this.instance.StartAsync();
    }

    public void Dispose()
    {
        this.instance.Dispose();
    }
}
