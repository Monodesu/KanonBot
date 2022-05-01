using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
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



// https://justsimplycode.com/2021/08/01/custom-json-converter-to-de-serialise-enum-description-value-to-enum-value/
public class JsonEnumConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        var enumType = Nullable.GetUnderlyingType(objectType) ?? objectType;
        return enumType.IsEnum;
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.Value is long)
            return Enum.ToObject(objectType, reader.Value);

        string description = (string)reader.Value;

        if (description is null) return null;

        foreach (var field in objectType.GetFields())
        {
            if (Attribute.GetCustomAttribute(field,
            typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
            {
                if (attribute.Description == description)
                    return field.GetValue(null);
            }
            else
            {
                if (field.Name == description)
                    return field.GetValue(null);
            }
        }

        throw new ArgumentException("Not found.", nameof(description));
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (string.IsNullOrEmpty(value.ToString()))
        {
            writer.WriteValue("");
            return;
        }
        writer.WriteValue(Utils.GetDesc(value));
    }
}