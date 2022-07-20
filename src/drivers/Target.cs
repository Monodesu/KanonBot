using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Msg = KanonBot.Message;
using Serilog;

namespace KanonBot.Drivers;
// 消息target封装
// 暂时还不知道怎么写
public class Target
{
    public Msg.Chain msg { get; set; } = new();

    // 这里的account是一个字符串，可以是qq号，discord号，等等
    // 之后将会自动解析成猫猫独有的账户类型以统一管理
    public Platform platform { get; set; }
    public string? account { get; set; }

    // 原平台消息结构
    public object? raw { get; set; }

    // 原平台接口
    public ISocket? socket { get; set; }

    public bool reply(string m)
    {
        return this.reply(new Msg.Chain().msg(m));
    }

    public bool reply(Msg.Chain msgChain)
    {
        switch (this.socket!)
        {
            case KOOK s:
                var rawMessage = (this.raw as KaiHeiLa.WebSocket.SocketMessage);
                try
                {
                    s.api.SendChannelMessage(rawMessage!.Channel.Id.ToString(), msgChain, rawMessage.Id).Wait();
                }
                catch (Exception ex) { Log.Warning("发送KOOK消息失败 ↓\n{ex}", ex); return false; }
                break;
            case Guild s:
                var GuildMessageData = (this.raw as Guild.Models.MessageData)!;
                try
                {
                    s.api.SendMessage(GuildMessageData.ChannelID, new Guild.Models.SendMessageData() {
                        MessageId = GuildMessageData.ID,
                        MessageReference = new() { MessageId = GuildMessageData.ID }
                    }.Build(msgChain)).Wait();
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