#pragma warning disable IDE1006 // 命名样式

using KanonBot.Serializer;
using Tomlyn.Model;

namespace KanonBot.Config;

public class Mail : ITomlMetadataProvider
{
    public string? smtp_host { get; set; }
    public int smtp_port { get; set; }
    public string? username { get; set; }
    public string? password { get; set; }
    TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }

}
public class Database : ITomlMetadataProvider
{
    public string? type { get; set; } = "mysql";
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
    public string? accessKeyId { get; set; }
    public string? accessKeySecret { get; set; }
    public string? endpoint { get; set; }
    public string? bucketName { get; set; }
    TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }

}
public class CQhttp : ITomlMetadataProvider
{
    public string? host { get; set; }
    public int port { get; set; }
    public int httpPort { get; set; }
    public string? managementGroup { get; set; }
    TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }
}
public class Config : ITomlMetadataProvider
{
    public static Config? inner;
    public bool debug { get; set; }
    public OSU? osu { get; set; }
    public CQhttp? cqhttp { get; set; }
    public OSS? oss { get; set; }
    public Database? database { get; set; }
    public Mail? mail { get; set; }
    TomlPropertiesMetadata? ITomlMetadataProvider.PropertiesMetadata { get; set; }
    public static Config Default() {
        return new Config() {
            debug = true,
            osu = new() {
                clientId = 0,
                clientSecret = ""
            },
            cqhttp = new() {
                managementGroup = "localhost",
                host = "localhost",
                httpPort = 5700,
                port = 6700
            },
            oss = new() {
                accessKeyId = "",
                accessKeySecret = "",
                endpoint = "",
                bucketName = ""
            },
            database = new() {
                type = "mysql",
                host = "",
                port = 3306,
                db = "kanonbot",
                user = "",
                password = ""
            },
            mail = new() {
                smtp_host = "localhost",
                smtp_port = 587,
                username = "",
                password = ""
            }
        };
    } 

    public static Config load(string path) {
        string c;
        using (var f = File.OpenText(path))
        {
            c = f.ReadToEnd();
        }
        return Toml.Deserialize<Config>(c);
    }

    public void save(string path) {
        using (var f = new StreamWriter(path))
        {
            f.Write(this.ToString());
        }
    }

    public override string ToString() {
        return Toml.Serialize(this);
    }

    public string ToJson() {
        return Json.Serialize(this);
    }
}
