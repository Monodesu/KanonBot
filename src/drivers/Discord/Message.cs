using System.Text.RegularExpressions;
using Discord.WebSocket;
using KanonBot.Message;

namespace KanonBot.Drivers;

public partial class Discord
{
    [GeneratedRegex(@"\(met\)(.*?)\(met\)", RegexOptions.Multiline)]
    private static partial Regex AtPattern();
    [GeneratedRegex(@"\(rol\)(.*?)\(rol\)", RegexOptions.Multiline)]
    private static partial Regex AtAdminPattern();

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
            foreach (Match m in AtPattern().Matches(MessageData.Content).Cast<Match>())
            {
                segList.Add((m, new AtSegment(m.Groups[1].Value, Platform.KOOK)));
            }
            foreach (Match m in AtAdminPattern().Matches(MessageData.Content).Cast<Match>())
            {
                segList.Add((m, new RawSegment("DISCORD AT ADMIN", m.Groups[1].Value)));
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
