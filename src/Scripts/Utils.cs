using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using RWCustom;
using UnityEngine;

namespace Pearlcat;

public static class Utils
{
    public static RainWorld RainWorld => Custom.rainWorld;
    public static Dictionary<string, FShader> Shaders => RainWorld.Shaders;
    public static InGameTranslator Translator => RainWorld.inGameTranslator;
    public static SaveMiscProgression MiscProgression => RainWorld.GetMiscProgression();

    // Prevents pure black (which is transparent)
    public static Color RWColorSafety(this Color color)
    {
        var hsl = Custom.RGB2HSL(color);

        var safeColor = Custom.HSL2RGB(hsl.x, hsl.y, Mathf.Clamp(hsl.z, 0.01f, 1.0f), color.a);

        return safeColor;
    }

    public static void SetIfSame(this ref Color toSet, Color toCompare, Color newColor)
    {
        if (toSet == toCompare)
        {
            toSet = newColor;
        }
    }

    public static void LogHookException(this Exception e, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "")
    {
        Plugin.Logger.LogError($"Caught exception applying a hook! May not be fatal, but likely to cause issues." +
                               $"\nRelated to ({Path.GetFileNameWithoutExtension(filePath)}.{memberName}). Details:" +
                               $"\n{e}\n{e.StackTrace}");
    }

    public static string TrimEnd(this string source, string value)
    {
        if (!source.EndsWith(value))
        {
            return source;
        }

        return source.Remove(source.LastIndexOf(value, StringComparison.Ordinal));
    }
}
