using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace Pearlcat;

public static partial class Utils
{
    public static Color RWColorSafety(this Color color)
    {
        var hsl = Custom.RGB2HSL(color);

        var safeColor = Custom.HSL2RGB(hsl.x, hsl.y, Mathf.Clamp(hsl.z, 0.01f, 1.0f), color.a);

        return safeColor;
    }

    public static int TexUpdateInterval(this Player player)
    {
        var texUpdateInterval = 5;
        var quality = player.abstractCreature.world.game.rainWorld.options.quality;

        if (quality == Options.Quality.LOW)
        {
            texUpdateInterval = 20;
        }
        else if (quality == Options.Quality.MEDIUM)
        {
            texUpdateInterval = 10;
        }

        return texUpdateInterval;
    }

    public static void SetIfSame(this ref Color toSet, Color toCompare, Color newColor)
    {
        if (toSet == toCompare)
        {
            toSet = newColor;
        }
    }

    public static void MapAlphaToColor(this Texture2D texture, Dictionary<byte, Color> map)
    {
        var data = texture.GetPixelData<Color32>(0);

        for (int i = 0; i < data.Length; i++)
        {
            if (map.TryGetValue(data[i].a, out var targetColor))
            {
                data[i] = targetColor;
            }
        }

        texture.SetPixelData(data, 0);
        texture.Apply(false);
    }

    public static Color HSLToRGB(this Vector3 hsl)
    {
        return Custom.HSL2RGB(hsl.x, hsl.y, hsl.z);
    }
}