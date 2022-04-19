using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using Tomyn = Tomlyn.Toml;

namespace KanonBot.Serializer;
public static class Json
{
    public static string Serialize(object? self) => JsonConvert.SerializeObject(self, Settings.Json);
    public static T? Deserialize<T>(string json) => JsonConvert.DeserializeObject<T>(json, Settings.Json);
    public static JObject ToLinq(string json) => JObject.Parse(json);
}
public static class Toml
{
    public static string Serialize(object self) => Tomyn.FromModel(self);
    public static T Deserialize<T>(string toml) where T : class, new() => Tomyn.ToModel<T>(toml);
    public static JObject ToLinq(string json) => JObject.Parse(json);
}

internal static class Settings
{
    public static readonly JsonSerializerSettings Json = new JsonSerializerSettings
    {
        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
        DateParseHandling = DateParseHandling.None,
        Converters = {
            new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
        },
    };
}



