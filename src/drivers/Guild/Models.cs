#pragma warning disable CS8618 // 非null 字段未初始化

using Newtonsoft.Json;
using System.ComponentModel;
using KanonBot.Message;
using KanonBot.Serializer;
using Newtonsoft.Json.Linq;


namespace KanonBot.Drivers;
public partial class Guild
{
    public class Models
    {
        public class EventBase<T>
        {
            /// <summary>
            /// 操作码
            /// </summary>
            [JsonProperty(PropertyName = "op")]
            public Enums.OperationCode Operation { get; set; }

            /// <summary>
            /// 事件类型
            /// </summary>
            [JsonProperty(PropertyName = "t", NullValueHandling = NullValueHandling.Ignore)]
            [JsonConverter(typeof(JsonEnumConverter))]
            public Enums.EventType? EventType { get; set; }
            
            /// <summary>
            /// 事件内容
            /// </summary>
            [JsonProperty(PropertyName = "d")]
            public T Data { get; set; }

            /// <summary>
            /// s 下行消息都会有一个序列号，标识消息的唯一性，客户端需要再发送心跳的时候，携带客户端收到的最新的s。
            /// </summary>
            [JsonProperty(PropertyName = "s", NullValueHandling = NullValueHandling.Ignore)]
            public string? Echo { get; set; }

        }
    }
}