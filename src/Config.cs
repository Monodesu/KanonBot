#pragma warning disable IDE1006 // 命名样式

using KanonBot.Serializer;
using Tomlyn.Model;

namespace KanonBot;
public class Config
{
    public static Base? inner;
    public class Mail : ITomlMetadataProvider
    {
        public string? smtpHost { get; set; }
        public int smtpPort { get; set; }
        public string? userName { get; set; }
        public string? passWord { get; set; }
        TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }

    }
    public class Database : ITomlMetadataProvider
    {
        public string? type { get; set; }
        public string? host { get; set; }
        public int port { get; set; }
        public string? db { get; set; }
        public string? user { get; set; }
        public string? password { get; set; }
        TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }

    }
    public class OSU : ITomlMetadataProvider
    {
        public int clientId { get; set; }
        public string? clientSecret { get; set; }
        TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }
    }
    public class OSS : ITomlMetadataProvider
    {
        public string? url { get; set; }
        public string? accessKeyId { get; set; }
        public string? accessKeySecret { get; set; }
        public string? endPoint { get; set; }
        public string? bucketName { get; set; }
        TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }

    }
    public class OneBot : ITomlMetadataProvider
    {
        public string? host { get; set; }
        public int port { get; set; }
        public int httpPort { get; set; }
        public int serverPort { get; set; }
        public long? managementGroup { get; set; }
        TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }
    }
    public class Guild : ITomlMetadataProvider
    {
        public bool sandbox { get; set; }
        public long appID { get; set; }
        public string? secret { get; set; }
        public string? token { get; set; }
        TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }
    }
    public class Base : ITomlMetadataProvider
    {
        public bool debug { get; set; }
        public OSU? osu { get; set; }
        public OneBot? onebot { get; set; }
        public Guild? guild { get; set; }
        public OSS? oss { get; set; }
        public Database? database { get; set; }
        public Mail? mail { get; set; }
        TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }
        public static Base Default()
        {
            return new Base()
            {
                debug = true,
                osu = new()
                {
                    clientId = 0,
                    clientSecret = ""
                },
                onebot = new()
                {
                    managementGroup = 0,
                    host = "localhost",
                    serverPort = 7700,
                    httpPort = 5700,
                    port = 6700
                },
                guild = new()
                {
                    appID = 0,
                    secret = "",
                    token = "",
                    sandbox = true
                },
                oss = new()
                {
                    url = "",
                    accessKeyId = "",
                    accessKeySecret = "",
                    endPoint = "",
                    bucketName = ""
                },
                database = new()
                {
                    type = "mysql",
                    host = "",
                    port = 3306,
                    db = "kanonbot",
                    user = "",
                    password = ""
                },
                mail = new()
                {
                    smtpHost = "localhost",
                    smtpPort = 587,
                    userName = "",
                    passWord = ""
                }
            };
        }
        public void save(string path)
        {
            using (var f = new StreamWriter(path))
            {
                f.Write(this.ToString());
            }
        }

        public override string ToString()
        {
            return Toml.Serialize(this);
        }

        public string ToJson()
        {
            return Json.Serialize(this);
        }
    }


    public static Base load(string path)
    {
        string c;
        using (var f = File.OpenText(path))
        {
            c = f.ReadToEnd();
        }
        return Toml.Deserialize<Base>(c);
    }
}