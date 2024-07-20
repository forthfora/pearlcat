using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Pearlcat;

// https://medium.com/@altaf.navalur/serialize-deserialize-color-objects-in-unity-1731e580af94
public class JsonColorHandler : JsonConverter
{
    public override bool CanConvert(Type objectType) => true;

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        try
        {
            ColorUtility.TryParseHtmlString("#" + reader.Value, out Color loadedColor);
            return loadedColor;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to parse color {objectType} : {ex.Message}");
            return null;
        }
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null) return;

        string val = ColorUtility.ToHtmlStringRGB((Color)value);
        writer.WriteValue(val);
    }
}
