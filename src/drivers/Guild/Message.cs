using System.Text.RegularExpressions;
using KanonBot.Message;

//using KanonBot.API;

namespace KanonBot.Drivers;

public partial class Guild
{
    public partial class Message
    {
        [GeneratedRegex(@"<@!?(.*?)> ?", RegexOptions.Multiline)]
        private static partial Regex AtPattern();

        [GeneratedRegex(@"<emoji:(.*?)>", RegexOptions.Multiline)]
        private static partial Regex EmojiPattern();

        [GeneratedRegex(@"<#(.*?)>", RegexOptions.Multiline)]
        private static partial Regex ChannelPattern();

        [GeneratedRegex(@"@everyone", RegexOptions.Multiline)]
        private static partial Regex AtEveryonePattern();

        public static Models.SendMessageData Build(Models.SendMessageData data, Chain msgChain)
        {
            var content = String.Empty;
            foreach (var msg in msgChain.Iter())
            {
                var tmp = msg switch
                {
                    TextSegment text => Utils.GuildEscape(text.value),
                    EmojiSegment face => $"<emoji:{face.value}>",
                    AtSegment at
                        => at.platform switch
                        {
                            Platform.Guild
                                => at.value switch
                                {
                                    "all" => "@everyone",
                                    _ => $"<@!{at.value}>"
                                },
                            _ => throw new ArgumentException("不支持的平台类型")
                        },
                    ImageSegment => null, // 图片不在此处处理
                    _ => msg.Build(),
                };
                content += tmp ?? String.Empty;

                if (msg is ImageSegment image)
                {
                    data.ImageUrl = image.t switch
                    {
                        //ImageSegment.Type.Base64 => Ali.PutFile(Convert.FromBase64String(image.value), "jpg", true),
                        ImageSegment.Type.Url
                            => image.value,
                        //ImageSegment.Type.File => Ali.PutFile(Utils.LoadFile2Byte(image.value).Result, "jpg", true),
                        _ => throw new ArgumentException("不支持的图片类型")
                    };
                }
            }
            data.Content = content;
            return data;
        }

        public static Chain Parse(Models.MessageData MessageData)
        {
            // 先处理 content
            var segList = new List<(Match m, IMsgSegment seg)>();
            foreach (Match m in AtPattern().Matches(MessageData.Content).Cast<Match>())
            {
                segList.Add((m, new AtSegment(m.Groups[1].Value, Platform.Guild)));
            }
            foreach (Match m in EmojiPattern().Matches(MessageData.Content).Cast<Match>())
            {
                segList.Add((m, new EmojiSegment(m.Groups[1].Value)));
            }
            foreach (Match m in ChannelPattern().Matches(MessageData.Content).Cast<Match>())
            {
                segList.Add((m, new RawSegment("Channel", m.Groups[1].Value)));
            }
            foreach (Match m in AtEveryonePattern().Matches(MessageData.Content).Cast<Match>())
            {
                segList.Add((m, new AtSegment("all", Platform.Guild)));
            }
            var chain = new Chain();
            var AddText = (string text) =>
            {
                var x = text.Trim();
                if (x.Length != 0)
                    chain.Add(new TextSegment(Utils.GuildUnEscape(x)));
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

            // 然后处理 Attachments
            if (MessageData.Attachments != null)
            {
                foreach (var attachment in MessageData.Attachments)
                {
                    if (attachment["content_type"]?.ToString() == "image/jpeg")
                    {
                        chain.Add(
                            new ImageSegment($"https://{attachment["url"]}", ImageSegment.Type.Url)
                        );
                    }
                }
            }
            return chain;
        }
    }
}
