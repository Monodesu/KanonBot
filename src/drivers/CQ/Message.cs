using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using KanonBot.Message;
using KanonBot.WebSocket;
using KanonBot.Serializer;
namespace KanonBot.Drivers;
public partial class CQ
{
    public class Message
    {
        public static List<Model.Segment> Build(Chain msgChain)
        {
            var ListSegment = new List<Model.Segment>();
            foreach (var msg in msgChain.GetList())
            {
                ListSegment.Add(
                    msg switch {
                        RawMessage raw => new Model.Segment {
                            msgType = Enums.SegmentType.Text,
                            rawData = new JObject { { "text", raw.value } }
                        },
                        ImageSegment image => new Model.Segment {
                            msgType = Enums.SegmentType.Image,
                            rawData = new JObject { { "file", image.value } }
                        },
                        AtSegment at => new Model.Segment {
                            msgType = Enums.SegmentType.At,
                            rawData = new JObject { { "qq", at.value } }
                        },
                        _ => throw new NotImplementedException()
                    }
                );
            }
            return ListSegment;
        }

        public static Chain Parse(JToken[] msg)
        {
            var chain = new Chain();
            foreach (var s in msg)
            {
                var obj = s.ToObject<Model.Segment>();
                chain.append(
                    obj.msgType switch {
                        Enums.SegmentType.Text => new RawMessage(obj.rawData["text"].ToString()),
                        Enums.SegmentType.Image => new ImageSegment(obj.rawData["file"].ToString(), ImageSegment.Type.File),
                        Enums.SegmentType.At => new AtSegment(obj.rawData["qq"].ToString(), AtSegment.Platform.QQ),
                        _ => throw new NotImplementedException()
                    }
                );
            }
            return chain;
        }

    }
}