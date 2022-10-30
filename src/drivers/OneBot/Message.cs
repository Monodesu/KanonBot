using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using KanonBot.Message;
using KanonBot.API;
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
            foreach (var msg in msgChain.ToList())
            {
                ListSegment.Add(
                    msg switch {
                        TextSegment text => new Models.Segment {
                            msgType = Enums.SegmentType.Text,
                            rawData = new JObject { { "text", text.value } }
                        },
                        ImageSegment image => new Models.Segment {
                            msgType = Enums.SegmentType.Image,
                            rawData = image.t switch {
                                ImageSegment.Type.Base64 => new JObject { { "file", $"base64://{image.value}" } },
                                ImageSegment.Type.Url => new JObject { { "file", image.value } },
                                ImageSegment.Type.File => new JObject { { "file", Ali.PutFile(Utils.LoadFile2ReadStream(image.value), "jpg") } }, // 这里还有缺陷，如果图片上传失败的话，还是会尝试发送
                                _ => throw new ArgumentException("不支持的图片类型")
                            }
                        },
                        AtSegment at => at.platform switch {
                            Platform.OneBot => new Models.Segment {
                                msgType = Enums.SegmentType.At,
                                rawData = new JObject { { "qq", at.value } }
                            },
                            _ => throw new ArgumentException("不支持的平台类型")
                        },
                        EmojiSegment face => new Models.Segment {
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
                chain.Add(
                    obj.msgType switch {
                        Enums.SegmentType.Text => new TextSegment(obj.rawData["text"]!.ToString()),
                        Enums.SegmentType.Image => obj.rawData.ContainsKey("url") ? new ImageSegment(obj.rawData["url"]!.ToString(), ImageSegment.Type.Url) : new ImageSegment(obj.rawData["file"]!.ToString(), ImageSegment.Type.File),
                        Enums.SegmentType.At => new AtSegment(obj.rawData["qq"]!.ToString(), Platform.OneBot),
                        Enums.SegmentType.Face => new EmojiSegment(obj.rawData["id"]!.ToString()),
                        _ => new RawSegment(obj.msgType.ToString(), obj.rawData)
                    }
                );
            }
            return chain;
        }

    }
}