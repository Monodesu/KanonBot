#pragma warning disable CS8618 // 非null 字段未初始化

using System.ComponentModel;
using KanonBot.Message;
using KanonBot.Serializer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NullValueHandling = Newtonsoft.Json.NullValueHandling;

// 部分参考 https://github.com/DeepOceanSoft/Sora

namespace KanonBot.Drivers;
public partial class OneBot
{
    public class Models
    {

        public class Anonymous
        {
            /// <summary>
            /// 匿名用户 flag
            /// </summary>
            [JsonProperty(PropertyName = "flag")]
            public string Flag { get; init; }

            /// <summary>
            /// 匿名用户 ID
            /// </summary>
            [JsonProperty(PropertyName = "id")]
            public long Id { get; init; }

            /// <summary>
            /// 匿名用户名称
            /// </summary>
            [JsonProperty(PropertyName = "name")]
            public string Name { get; init; }
        }

        public readonly struct Segment
        {
            /// <summary>
            /// 消息段类型
            /// </summary>
            [JsonProperty(PropertyName = "type")]
            [JsonConverter(typeof(JsonEnumConverter))]
            public Enums.SegmentType msgType { get; init; }

            /// <summary>
            /// 消息段JSON
            /// </summary>
            [JsonProperty(PropertyName = "data")]
            public JObject rawData { get; init; }
        }

        public struct SendMessage
        {
            [JsonProperty(PropertyName = "message_type")]
            [JsonConverter(typeof(JsonEnumConverter))]
            public Enums.MessageType MessageType { get; set; }
            [JsonProperty(PropertyName = "user_id")]
            public long? UserId { get; set; }
            [JsonProperty(PropertyName = "group_id")]
            public long? GroupId { get; set; }
            [JsonProperty(PropertyName = "message")]
            public List<Segment> Message { get; set; }
            [JsonProperty(PropertyName = "auto_escape")]
            public bool AutoEscape { get; set; }
        }

        public class CQRequest
        {

            [JsonProperty(PropertyName = "action")]
            [JsonConverter(typeof(JsonEnumConverter))]
            public Enums.Actions action { get; init; }

            [JsonProperty(PropertyName = "echo")]
            public Guid Echo { get; } = Guid.NewGuid();

            [JsonProperty(PropertyName = "params")]
            public dynamic Params { get; init; }
        }
        public class CQResponse
        {

            [JsonProperty(PropertyName = "status")]
            public string Status { get; init; }
            [JsonProperty(PropertyName = "retcode")]
            public int RetCode { get; init; }
            [JsonProperty(PropertyName = "echo")]
            public Guid Echo { get; init; }
            [JsonProperty(PropertyName = "data")]
            public JObject Data { get; init; }

        }

        public class CQGroupAddRequest
        {

            [JsonProperty(PropertyName = "sub_type")]
            [JsonConverter(typeof(JsonEnumConverter))]
            public Enums.GroupRequestType RequestType { get; init; }
            [JsonProperty(PropertyName = "approve")]
            public bool Approve { get; set; }

            [JsonProperty(PropertyName = "reason")]
            public string Reason { set; get; }
        }

        public class Sender
        {
            [JsonProperty(PropertyName = "role")]
            [JsonConverter(typeof(JsonEnumConverter))]
            public Enums.GroupRole Role { get; set; }
            [JsonProperty(PropertyName = "user_id")]
            public long UserId { get; set; }
            [JsonProperty(PropertyName = "area")]
            public string Area { get; set; }
            [JsonProperty(PropertyName = "card")]
            public string Aard { get; set; }
            [JsonProperty(PropertyName = "level")]
            public string Level { get; set; }
            [JsonProperty(PropertyName = "nickname")]
            public string NickName { get; set; }
            [JsonProperty(PropertyName = "sex")]
            public string Sex { get; set; }
            [JsonProperty(PropertyName = "age")]
            public int Age { get; set; }
        }

        public class CQEventBase
        {
            /// <summary>
            /// 事件发生的时间戳
            /// </summary>
            [JsonProperty(PropertyName = "time", NullValueHandling = NullValueHandling.Ignore)]
            public long? Time { get; set; }

            /// <summary>
            /// 收到事件的机器人 QQ 号
            /// </summary>
            [JsonProperty(PropertyName = "self_id", NullValueHandling = NullValueHandling.Ignore)]
            public long? SelfId { get; set; }

            /// <summary>
            /// 事件类型
            /// </summary>
            [JsonProperty(PropertyName = "post_type", NullValueHandling = NullValueHandling.Ignore)]
            public string? PostType { get; set; }
        }

        public class CQMessageEventBase : CQEventBase
        {
            /// <summary>
            /// 消息类型
            /// </summary>
            [JsonProperty(PropertyName = "message_type")]
            public string MessageType { get; set; }

            /// <summary>
            /// 消息子类型
            /// </summary>
            [JsonProperty(PropertyName = "sub_type")]
            public string SubType { get; set; }

            /// <summary>
            /// 消息 ID
            /// </summary>
            [JsonProperty(PropertyName = "message_id")]
            public int MessageId { get; set; }

            /// <summary>
            /// 发送者 QQ 号
            /// </summary>
            [JsonProperty(PropertyName = "user_id")]
            public long UserId { get; set; }

            /// <summary>
            /// 消息内容
            /// </summary>
            [JsonProperty(PropertyName = "message")]
            public List<Segment> MessageList { get; set; }

            /// <summary>
            /// 原始消息内容
            /// </summary>
            [JsonProperty(PropertyName = "raw_message")]
            public string RawMessage { get; set; }

            /// <summary>
            /// 字体
            /// </summary>
            [JsonProperty(PropertyName = "font")]
            public int Font { get; set; }
        }

        public class GroupMessage : CQMessageEventBase
        {
            /// <summary>
            /// 群号
            /// </summary>
            [JsonProperty(PropertyName = "group_id")]
            public long GroupId { get; set; }

            /// <summary>
            /// 匿名信息
            /// </summary>
            [JsonProperty(PropertyName = "anonymous", NullValueHandling = NullValueHandling.Ignore)]
            public Anonymous? Anonymous { get; set; }

            /// <summary>
            /// 发送人信息
            /// </summary>
            [JsonProperty(PropertyName = "sender")]
            public Sender SenderInfo { get; set; }

            /// <summary>
            /// 消息序号
            /// </summary>
            [JsonProperty(PropertyName = "message_seq")]
            public long MessageSequence { get; set; }
        }

        public class PrivateMessage : CQMessageEventBase
        {
            /// <summary>
            /// 发送人信息
            /// </summary>
            [JsonProperty(PropertyName = "sender")]
            public Sender SenderInfo { get; set; }
        }
    }
}
