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
        public class PayloadBase<T>
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
            public Enums.EventType? Type { get; set; }
            
            /// <summary>
            /// 事件内容
            /// </summary>
            [JsonProperty(PropertyName = "d")]
            public T Data { get; set; }

            /// <summary>
            /// s 下行消息都会有一个序列号，标识消息的唯一性，客户端需要再发送心跳的时候，携带客户端收到的最新的s。
            /// </summary>
            [JsonProperty(PropertyName = "s", NullValueHandling = NullValueHandling.Ignore)]
            public int? Seq { get; set; }

        }

        public class IdentityData
        {
            /// <summary>
            /// token 是创建机器人的时候分配的，格式为Bot {appid}.{app_token}
            /// </summary>
            [JsonProperty(PropertyName = "token")]
            public string Token { get; set; }

            /// <summary>
            /// intents 是此次连接所需要接收的事件
            /// </summary>
            [JsonProperty(PropertyName = "intents")]
            public Enums.Intent Intents { get; set; }

            /// <summary>
            /// shard 该参数是用来进行水平分片的。该参数是个拥有两个元素的数组。例如：[0,4]，代表分为四个片，当前链接是第 0 个片，业务稍后应该继续建立 shard 为[1,4],[2,4],[3,4]的链接，才能完整接收事件。
            /// </summary>
            [JsonProperty(PropertyName = "shard")]
            public int[] Shard { get; set; }

            /// <summary>
            /// properties 目前无实际作用，可以按照自己的实际情况填写，也可以留空
            /// </summary>
            [JsonProperty(PropertyName = "properties")]
            public Properties Prop { get; set; } = new Properties();
            
            public class Properties {
                // 自动获取当前运行系统类型
                [JsonProperty(PropertyName = "$os")]
                public string Os { get; set; } = Environment.OSVersion.Platform.ToString();
                [JsonProperty(PropertyName = "$browser")]
                public string Browser { get; set; } = "KanonBot";
                [JsonProperty(PropertyName = "$device")]
                public string Device { get; set; } = "KanonBot";
            }
        }
    }
}