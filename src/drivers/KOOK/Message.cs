using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using KanonBot.Message;
using KanonBot.API;
using KanonBot.Serializer;
using Serilog;
using khl = KaiHeiLa;
using KaiHeiLa.WebSocket;

namespace KanonBot.Drivers;
public partial class KOOK
{
    static readonly string AtPattern = @"\(met\)(.*?)\(met\)";
    static readonly string AtAdminPattern = @"\(rol\)(.*?)\(rol\)";
    public class Message
    {
        public async static Task<List<Models.MessageCreate>> Build(API api, Chain msgChain)
        {
            var msglist = new List<Models.MessageCreate>();
            foreach (var seg in msgChain.ToList())
            {
                var req = new Models.MessageCreate();
                switch (seg)
                {
                    case ImageSegment s:
                        req.MessageType = Enums.MessageType.Image;
                        switch (s.t)
                        {
                            case ImageSegment.Type.Base64:
                                var _s = Utils.Byte2Stream(Convert.FromBase64String(s.value));
                                req.Content = await api.CreateAsset(_s);
                                break;
                            case ImageSegment.Type.File:
                                var __s = Utils.LoadFile2Stream(s.value);
                                req.Content = await api.CreateAsset(__s);
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
                            if (msglist[msglist.Count - 1].MessageType == Enums.MessageType.Text)
                            {
                                msglist[msglist.Count - 1].Content += s.value;
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
                            throw new NotSupportedException("????????????????????????");
                        if (msglist.Count > 0)
                        {
                            // ???????????????????????????????????? Text ???
                            if (msglist[msglist.Count - 1].MessageType == Enums.MessageType.Text)
                            {
                                msglist[msglist.Count - 1].Content += _at;
                                continue;
                            }
                        }
                        req.MessageType = Enums.MessageType.Text;
                        req.Content = _at;
                        break;
                    default:
                        throw new NotSupportedException("????????????????????????");
                }
                msglist.Add(req);
            }
            return msglist;
        }

        /// <summary>
        /// ?????????????????????????????????
        /// </summary>
        /// <param name="MessageData"></param>
        /// <returns></returns>
        public static Chain Parse(SocketMessage MessageData)
        {
            var chain = new Chain();
            // ?????? content
            var segList = new List<(Match m, IMsgSegment seg)>();
            RegexOptions options = RegexOptions.Multiline;
            foreach (Match m in Regex.Matches(MessageData.Content, AtPattern, options))
            {
                segList.Add((m, new AtSegment(m.Groups[1].Value, Platform.KOOK)));
            }
            foreach (Match m in Regex.Matches(MessageData.Content, AtAdminPattern, options))
            {
                segList.Add((m, new RawSegment("KOOK AT ADMIN", m.Groups[1].Value)));
            }
            var AddText = (string text) =>
            {
                var x = text.Trim();
                // ????????????attacment
                if (MessageData.Attachment != null)
                {
                    if (MessageData.Attachment.Type == khl.AttachmentType.Image)
                    {
                        // ???????????????????????????
                        chain.Add(new ImageSegment(MessageData.Attachment.Url, ImageSegment.Type.Url));
                        x = x.Replace(MessageData.Attachment.Url, "");
                    }
                }
                if (x.Length != 0)
                    chain.Add(new TextSegment(Utils.KOOKUnEscape(x)));
            };
            var pos = 0;
            segList.OrderBy(x => x.m.Index).ToList().ForEach(x =>
            {
                if (pos < x.m.Index)
                {
                    AddText(MessageData.Content.Substring(pos, x.m.Index - pos));
                }
                chain.Add(x.seg);
                pos = x.m.Index + x.m.Length;
            });
            if (pos < MessageData.Content.Length)
                AddText(MessageData.Content.Substring(pos));

            return chain;
        }

    }
}