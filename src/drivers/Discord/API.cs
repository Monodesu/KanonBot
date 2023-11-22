using System.IO;
using System.Net.Http;
using Discord;
using Discord.WebSocket;
using KanonBot.Message;
using Newtonsoft.Json.Linq;

namespace KanonBot.Drivers;

public partial class Discord
{
    // API 部分 * 包装 Driver
    public class API
    {
        private string AuthToken;

        public API(string authToken)
        {
            this.AuthToken = $"Bot {authToken}";
        }

        // IFlurlRequest http()
        // {
        //     return EndPoint.WithHeader("Authorization", this.AuthToken);
        // }

        async public Task SendMessage(ISocketMessageChannel channel, Chain msgChain)
        {
            foreach (var seg in msgChain.Iter())
            {
                switch (seg)
                {
                    case ImageSegment s:
                        switch (s.t)
                        {
                            case ImageSegment.Type.Base64:
                                var _uuid = Guid.NewGuid();
                                using (
                                    var _s = Utils.Byte2Stream(Convert.FromBase64String(s.value))
                                )
                                    await channel.SendFileAsync(
                                        _s,
                                        $"{_uuid}.jpg",
                                        embed: new EmbedBuilder
                                        {
                                            ImageUrl = $"attachment://{_uuid}.jpg"
                                        }.Build()
                                    );
                                break;
                            case ImageSegment.Type.File:
                                var __uuid = Guid.NewGuid();
                                using (var __s = Utils.LoadFile2ReadStream(s.value))
                                    await channel.SendFileAsync(
                                        __s,
                                        $"{__uuid}.jpg",
                                        embed: new EmbedBuilder
                                        {
                                            ImageUrl = $"attachment://{__uuid}.jpg"
                                        }.Build()
                                    );

                                break;
                            case ImageSegment.Type.Url:
                                var ___uuid = Guid.NewGuid();
                                using (var ___s = await s.value.GetStreamAsync())
                                    await channel.SendFileAsync(
                                        ___s,
                                        $"{___uuid}.jpg",
                                        embed: new EmbedBuilder
                                        {
                                            ImageUrl = $"attachment://{___uuid}.jpg"
                                        }.Build()
                                    );
                                break;
                            default:
                                break;
                        }
                        break;
                    case TextSegment s:
                        await channel.SendMessageAsync(s.value);
                        break;
                    case AtSegment s:
                        // 我不管，我就先不发送
                        break;
                    default:
                        throw new NotSupportedException("不支持的平台类型");
                }
            }
        }
    }
}
