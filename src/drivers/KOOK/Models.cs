#pragma warning disable CS8618 // 非null 字段未初始化

using Newtonsoft.Json;
using NullValueHandling = Newtonsoft.Json.NullValueHandling;

namespace KanonBot.Drivers;
public partial class Kook
{
    public class Models
    {
        public class MessageCreate
        {

            [JsonProperty(PropertyName = "type")]
            public Enums.MessageType MessageType { get; set; }

            [JsonProperty(PropertyName = "target_id")]
            public string TargetId { get; set; }

            [JsonProperty(PropertyName = "content")]
            public string Content { get; set; }

            [JsonProperty(PropertyName = "quote", NullValueHandling = NullValueHandling.Ignore)]
            public Guid? QuotedMessageId { get; set; }

            [JsonProperty(PropertyName = "nonce", NullValueHandling = NullValueHandling.Ignore)]
            public Guid? Nonce { get; set; }

            [JsonProperty(PropertyName = "temp_target_id", NullValueHandling = NullValueHandling.Ignore)]
            public string? EphemeralUserId { get; set; }

            public MessageCreate Clone()
            {
                var other = this.MemberwiseClone() as MessageCreate;
                return other!;
            }
        }
    }
}
