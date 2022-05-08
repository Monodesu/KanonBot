using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Msg = KanonBot.Message;

namespace KanonBot.Drivers;
// 消息target封装
// 暂时还不知道怎么写
public class Target
{
    public Msg.Chain msg = new();

    // 这里的account是一个字符串，可以是qq号，discord号，等等
    // 之后将会自动解析成猫猫独有的账户类型以统一管理
    // public string account = String.Empty;

    // 原平台消息结构
    public object? raw;

    // 原平台接口
    public ISocket? socket;
    public bool reply(Msg.Chain msgChain)
    {
        switch (this.socket!)
        {
            case Guild s:
                var GuildMessageData = (this.raw as Guild.Models.MessageData)!;
                try
                {
                    var res = s.api.SendMessage(GuildMessageData.ChannelID, new Guild.Models.SendMessageData() {
                        MessageId = GuildMessageData.ID,
                        MessageReference = new() { MessageId = GuildMessageData.ID }
                    }.Build(msgChain)).Result;
                }
                catch
                {
                    return false;
                }
                break;
            case OneBot.Client s:
                switch (this.raw)
                {
                    case OneBot.Models.GroupMessage g:
                        if (s.api.SendGroupMessage(g.GroupId, msgChain) == -1)
                        {
                            return false;
                        }
                        break;
                    case OneBot.Models.PrivateMessage p:
                        if (s.api.SendPrivateMessage(p.UserId, msgChain) == -1)
                        {
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
                        if (s.api.SendGroupMessage(g.GroupId, msgChain) == -1)
                        {
                            return false;
                        }
                        break;
                    case OneBot.Models.PrivateMessage p:
                        if (s.api.SendPrivateMessage(p.UserId, msgChain) == -1)
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