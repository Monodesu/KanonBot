using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using KanonBot.Message;
using KanonBot.WebSocket;
using KanonBot.Serializer;
using Serilog;
namespace KanonBot.Drivers;
public partial class OneBot
{
    public class Message
    {
        public static List<Models.Segment> Build(Chain msgChain)
        {
            var ListSegment = new List<Models.Segment>();
            foreach (var msg in msgChain.GetList())
            {
                ListSegment.Add(
                    msg switch {
                        TextSegment text => new Models.Segment {
                            msgType = Enums.SegmentType.Text,
                            rawData = new JObject { { "text", text.value } }
                        },
                        ImageSegment image => new Models.Segment {
                            msgType = Enums.SegmentType.Image,
                            rawData = new JObject { { "file", image.value } }
                        },
                        AtSegment at => new Models.Segment {
                            msgType = Enums.SegmentType.At,
                            rawData = new JObject { { "qq", at.value } }
                        },
                        FaceSegment face => new Models.Segment {
                            msgType = Enums.SegmentType.Face,
                            rawData = new JObject { { "id", face.value } }
                        },
                        // 收到未知消息就转换为纯文本
                        _ => new Models.Segment {
                            msgType = Enums.SegmentType.Text,
                            rawData = new JObject { { "text", msg.Build() } }
                        }
                    }
                );
            }
            return ListSegment;
        }

        public static Chain Parse(List<Models.Segment> MessageList)
        {
            var chain = new Chain();
            foreach (var obj in MessageList)
            {
                chain.append(
                    obj.msgType switch {
                        Enums.SegmentType.Text => new TextSegment(obj.rawData["text"].ToString()),
                        Enums.SegmentType.Image => new ImageSegment(obj.rawData["file"].ToString(), ImageSegment.Type.File),
                        Enums.SegmentType.At => new AtSegment(obj.rawData["qq"].ToString(), Platform.OneBot),
                        Enums.SegmentType.Face => new FaceSegment(obj.rawData["id"].ToString()),
                        _ => new RawMessage(obj.msgType.ToString(), obj.rawData)
                    }
                );
            }
            return chain;
        }

    }
}