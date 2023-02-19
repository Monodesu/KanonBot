using System.Threading.Channels;
using libKook = Kook;
using libDiscord = Discord;
using Msg = KanonBot.Message;

namespace KanonBot.Drivers;
// 消息target封装
// 暂时还不知道怎么写
public class Target
{
    public static Atom<List<(Target, ChannelWriter<Target>)>> Waiters { get; set; } = Atom<List<(Target, ChannelWriter<Target>)>>(new());
    async public Task<Option<Target>> prompt(TimeSpan timeout)
    {
        var channel = Channel.CreateBounded<Target>(1);
        Waiters.Swap(l =>
        {
            l.Add((this, channel.Writer));
            return l;
        });
        var ret = await channel.Reader.ReadAsync().AsTask().TimeOut(timeout);
        Waiters.Swap(l =>
        {
            l.Remove((this, channel.Writer));
            return l;
        });
        return ret;
    }
    public required Msg.Chain msg { get; init; }

    // account和sender为用户ID字符串，可以是qq号，khl号，等等
    public required string? selfAccount { get; init; }
    public required string? sender { get; init; }
    public required Platform platform { get; init; }

    // 原平台消息结构
    public object? raw { get; init; }

    // 原平台接口
    public required ISocket socket { get; init; }


    public Task<bool> reply(string m)
    {
        return this.reply(new Msg.Chain().msg(m));
    }

    async public Task<bool> reply(Msg.Chain msgChain)
    {
        switch (this.socket!)
        {
            case Discord d:
                var discordRawMessage = this.raw as libDiscord.WebSocket.SocketMessage;
                try
                {
                    await d.api.SendMessage(discordRawMessage!.Channel, msgChain);
                }
                catch (Exception ex) { Log.Warning("发送Discord消息失败 ↓\n{ex}", ex); return false; }
                break;
            case Kook s:
                var KookRawMessage = this.raw as libKook.WebSocket.SocketMessage;
                try
                {
                    await s.api.SendChannelMessage(KookRawMessage!.Channel.Id.ToString(), msgChain, KookRawMessage.Id);
                }
                catch (Exception ex) { Log.Warning("发送Kook消息失败 ↓\n{ex}", ex); return false; }
                break;
            case Guild s:
                var GuildMessageData = (this.raw as Guild.Models.MessageData)!;
                try
                {
                    await s.api.SendMessage(GuildMessageData.ChannelID, new Guild.Models.SendMessageData()
                    {
                        MessageId = GuildMessageData.ID,
                        MessageReference = new() { MessageId = GuildMessageData.ID }
                    }.Build(msgChain));
                }
                catch (Exception ex) { Log.Warning("发送QQ频道消息失败 ↓\n{ex}", ex); return false; }
                break;
            case OneBot.Client s:
                switch (this.raw)
                {
                    case OneBot.Models.GroupMessage g:
                        if (s.api.SendGroupMessage(g.GroupId, msgChain).HasValue)
                        {
                            Log.Warning("发送QQ消息失败");
                            return false;
                        }
                        break;
                    case OneBot.Models.PrivateMessage p:
                        if (s.api.SendPrivateMessage(p.UserId, msgChain).HasValue)
                        {
                            Log.Warning("发送QQ消息失败");
                            return false;
                        }
                        break;
                    default:
                        break;
                }
                break;
            case OneBot.Server.Socket s:
                switch (this.raw)
                {
                    case OneBot.Models.GroupMessage g:
                        if (s.api.SendGroupMessage(g.GroupId, msgChain).HasValue)
                        {
                            return false;
                        }
                        break;
                    case OneBot.Models.PrivateMessage p:
                        if (s.api.SendPrivateMessage(p.UserId, msgChain).HasValue)
                        {
                            return false;
                        }
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
        return true;
    }

}
