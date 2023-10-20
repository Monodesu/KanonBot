using Microsoft.VisualBasic.CompilerServices;
//using KanonBot.API;
using KanonBot.Message;

namespace KanonBot.Drivers;

public partial class FakeSocket : ISocket, IReply
{
    public required Action<string>? action;
    public string? selfID => throw new NotImplementedException();

    public void Send(string message)
    {
        action?.Invoke(message);
    }

    public void Reply(Target target, Message.Chain msg)
    {
        foreach (var s in msg.Iter())
        {
            switch (s)
            {
                case ImageSegment i:
                    var url = i.t switch
                    {
                        ImageSegment.Type.Base64
                            => Utils.Byte2File(
                                $"./work/tmp/{Guid.NewGuid()}.png",
                                Convert.FromBase64String(i.value)
                            ),
                        ImageSegment.Type.Url => i.value,
                        ImageSegment.Type.File => i.value,
                        _ => throw new ArgumentException("不支持的图片类型")
                    };
                    this.Send($"image;{url}");
                    break;
                default:
                    this.Send(s.Build());
                    break;
            }
        }
    }
}
