using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using KanonBot.Message;
using KanonBot.WebSocket;
using KanonBot.Serializer;
namespace KanonBot.Drivers;
public partial class CQ
{
    // API 部分 * 包装 Driver
    public class API
    {
        IDriver driver;
        public API(IDriver driver)
        {
            this.driver = driver;
        }

        // 发送群消息
        public long SendGroupMessage(long groupId, Chain msgChain)
        {
            var message = Message.Build(msgChain);
            var req = new Model.CQRequest
            {
                action = Enums.Actions.SendMsg,
                Params = new Model.SendMessage
                {
                    MessageType = Enums.MessageType.Group,
                    GroupId = groupId,
                    Message = message,
                    AutoEscape = false
                },
            };

            this.driver.Send(Json.Serialize(req));
            return 0;
        }

        // 发送私聊消息
        public long SendPrivateMessage(long userId, Chain msgChain)
        {
            var message = Message.Build(msgChain);
            var req = new Model.CQRequest
            {
                action = Enums.Actions.SendMsg,
                Params = new Model.SendMessage
                {
                    MessageType = Enums.MessageType.Private,
                    UserId = userId,
                    Message = message,
                    AutoEscape = false
                },
            };

            this.driver.Send(req);
            return 0;
        }

    }
}