using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Pearlcat;

// https://medium.com/@altaf.navalur/serialize-deserialize-color-objects-in-unity-1731e580af94
public class JsonColorHandler : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return true;
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        try
        {
            var ok = ColorUtility.TryParseHtmlString("#" + reader.Value, out var loadedColor);

            if (!ok)
            {
                return null;
            }

            return loadedColor;
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"Failed to parse color: {objectType}\n{e}\n{e.StackTrace}");
            return null;
        }
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            return;
        }

        var val = ColorUtility.ToHtmlStringRGB((Color)value);
        writer.WriteValue(val);
    }
}
