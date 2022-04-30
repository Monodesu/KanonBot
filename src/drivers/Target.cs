using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Msg = KanonBot.Message;
using KanonBot.WebSocket;

namespace KanonBot.Drivers;
// 消息target封装
// 暂时还不知道怎么写
public class Target
{
    public Msg.Chain msg;

    // 这里的account是一个字符串，可以是qq号，discord号，等等
    // 之后将会自动解析成猫猫独有的账户类型以统一管理
    public string account;
    // 原平台结构
    public dynamic raw;

    // 原平台接口
    public dynamic api;
    public void reply(Msg.Chain msgChain)
    {

    }
    
}