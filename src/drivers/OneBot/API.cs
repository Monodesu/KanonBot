using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using KanonBot.Message;
using KanonBot.Serializer;
using Serilog;
namespace KanonBot.Drivers;
public partial class OneBot
{
    // API 部分 * 包装 Driver
    public class API
    {
        ISocket socket;
        public API(ISocket socket)
        {
            this.socket = socket;
        }


        #region 消息收发
        public class RetCallback
        {
            public AutoResetEvent ResetEvent { get; } = new AutoResetEvent(false);
            public Models.CQResponse? Data { get; set; }
        }
        public Dictionary<Guid, RetCallback> CallbackList = new();
        public void Echo(Models.CQResponse res)
        {
            this.CallbackList[res.Echo].Data = res;
            this.CallbackList[res.Echo].ResetEvent.Set();
        }
        private Models.CQResponse Send(Models.CQRequest req)
        {
            this.CallbackList[req.Echo] = new RetCallback();    // 创建回调
            this.socket.Send(req);                              // 发送
            this.CallbackList[req.Echo].ResetEvent.WaitOne();   // 等待回调
            var ret = this.CallbackList[req.Echo].Data!;         // 获取回调
            this.CallbackList.Remove(req.Echo);                 // 移除回调
            return ret;
        }
        #endregion

        // 发送群消息
        public long? SendGroupMessage(long groupId, Chain msgChain)
        {
            var message = Message.Build(msgChain);
            var req = new Models.CQRequest
            {
                action = Enums.Actions.SendMsg,
                Params = new Models.SendMessage
                {
                    MessageType = Enums.MessageType.Group,
                    GroupId = groupId,
                    Message = message,
                    AutoEscape = false
                },
            };

            var res = this.Send(req);
            if (res.Status == "ok")
                return (long)res.Data["message_id"]!;
            else
                return null;
        }

        // 发送私聊消息
        public long? SendPrivateMessage(long userId, Chain msgChain)
        {
            var message = Message.Build(msgChain);
            var req = new Models.CQRequest
            {
                action = Enums.Actions.SendMsg,
                Params = new Models.SendMessage
                {
                    MessageType = Enums.MessageType.Private,
                    UserId = userId,
                    Message = message,
                    AutoEscape = false
                },
            };

            var res = this.Send(req);
            if (res.Status == "ok")
                return (long)res.Data["message_id"]!;
            else
                return null;
        }

    }
}