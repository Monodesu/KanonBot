using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using KanonBot;

namespace KanonBot.Drivers;
// https://justsimplycode.com/2021/08/01/custom-json-converter-to-de-serialise-enum-description-value-to-enum-value/
public class EnumConverter : JsonConverter
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