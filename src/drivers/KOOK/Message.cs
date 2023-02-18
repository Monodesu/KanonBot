using System.Linq;
using System.Text.RegularExpressions;
using KanonBot.Message;
using libKook = Kook;
using Kook.WebSocket;

namespace KanonBot.Drivers;

public partial class Kook
{
    static readonly string AtPattern = @"\(met\)(.*?)\(met\)";
    static readonly string AtAdminPattern = @"\(rol\)(.*?)\(rol\)";

    public class Message
    {
        public async static Task<List<Models.MessageCreate>> Build(API api, Chain msgChain)
        {
            var msglist = new List<Models.MessageCreate>();
            foreach (var seg in msgChain.Iter())
            {
                var req = new Models.MessageCreate();
                switch (seg)
                {
                    case ImageSegment s:
                        req.MessageType = Enums.MessageType.Image;
                        switch (s.t)
                        {
                            case ImageSegment.Type.Base64:
                                using (
                                    var _s = Utils.Byte2Stream(Convert.FromBase64String(s.value))
                                )
                                {
                                    req.Content = await api.CreateAsset(_s);
                                }
                                break;
                            case ImageSegment.Type.File:
                                using (var __s = Utils.LoadFile2ReadStream(s.value))
                                {
                                    req.Content = await api.CreateAsset(__s);
                                }
                                break;
                            case ImageSegment.Type.Url:
                                req.Content = s.value;
                                break;
                            default:
                                break;
                        }
                        break;
                    case TextSegment s:
                        if (msglist.Count > 0)
                        {
                            if (msglist[^1].MessageType == Enums.MessageType.Text)
                            {
                                msglist[^1].Content += s.value;
                                continue;
                            }
                        }
                        req.MessageType = Enums.MessageType.Text;
                        req.Content = s.value;
                        break;
                    case AtSegment s:
                        string _at;
                        if (s.platform == Platform.KOOK)
                            _at = $"(met){s.value}(met)";
                        else
                            throw new NotSupportedException("不支持的平台类型");
                        if (msglist.Count > 0)
                        {
                            // 将一类文字消息合并起来到 Text 中
                            if (msglist[^1].MessageType == Enums.MessageType.Text)
                            {
                                msglist[^1].Content += _at;
                                continue;
                            }
                        }
                        req.MessageType = Enums.MessageType.Text;
                        req.Content = _at;
                        break;
                    default:
                        throw new NotSupportedException("不支持的平台类型");
                }
                msglist.Add(req);
            }
            return msglist;
        }

        /// <summary>
        /// 解析部分附件只支持图片
        /// </summary>
        /// <param name="MessageData"></param>
        /// <returns></returns>
        public static Chain Parse(SocketMessage MessageData)
        {
            var chain = new Chain();
            // 处理 content
            var segList = new List<(Match m, IMsgSegment seg)>();
            RegexOptions options = RegexOptions.Multiline;
            foreach (Match m in Regex.Matches(MessageData.Content, AtPattern, options).AsEnumerable())
            {
                segList.Add((m, new AtSegment(m.Groups[1].Value, Platform.KOOK)));
            }
            foreach (Match m in Regex.Matches(MessageData.Content, AtAdminPattern, options).AsEnumerable())
            {
                segList.Add((m, new RawSegment("KOOK AT ADMIN", m.Groups[1].Value)));
            }
            var AddText = (string text) =>
            {
                var x = text.Trim();
                // 匹配一下attacment
                foreach (var Attachment in MessageData.Attachments)
                {
                    if (Attachment.Type == libKook.AttachmentType.Image)
                    {
                        // 添加图片，删除文本
                        chain.Add(new ImageSegment(Attachment.Url, ImageSegment.Type.Url));
                        x = x.Replace(Attachment.Url, "");
                    }
                }
                if (x.Length != 0)
                    chain.Add(new TextSegment(Utils.KOOKUnEscape(x)));
            };
            var pos = 0;
            segList
                .OrderBy(x => x.m.Index)
                .ToList()
                .ForEach(x =>
                {
                    if (pos < x.m.Index)
                    {
                        AddText(MessageData.Content[pos..x.m.Index]);
                    }
                    chain.Add(x.seg);
                    pos = x.m.Index + x.m.Length;
                });
            if (pos < MessageData.Content.Length)
                AddText(MessageData.Content[pos..]);

            return chain;
        }
    }
}
