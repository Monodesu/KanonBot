using System.Text.RegularExpressions;
using Discord.WebSocket;
using KanonBot.Message;

namespace KanonBot.Drivers;

public partial class Discord
{
    static readonly string AtPattern = @"\(met\)(.*?)\(met\)";
    static readonly string AtAdminPattern = @"\(rol\)(.*?)\(rol\)";

    public class Message
    {
     

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
                // 匹配一下attacment
                foreach (var Attachment in MessageData.Attachments)
                {
                    if (Attachment.ContentType == "image")
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
