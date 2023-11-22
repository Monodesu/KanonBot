using LinqToDB;
using LinqToDB.Mapping;

namespace KanonBot.Database;

public static partial class Models
{
    [Table("chatbot")]
    public class ChatBot
    {
        [PrimaryKey]
        public int uid { get; set; }

        [Column]
        public string? botdefine { get; set; }

        [Column]
        public string? openaikey { get; set; }

        [Column]
        public string? organization { get; set; }
    }
}
