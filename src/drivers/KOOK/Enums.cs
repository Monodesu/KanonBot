namespace KanonBot.Drivers;

public partial class Kook
{
    public class Enums
    {
        public enum MessageType
        {
            Text = 1,
            Image = 2,
            Video = 3,
            File = 4,
            Audio = 8,
            KMarkdown = 9,
            Card = 10,
            System = 255
        }
    }
}
