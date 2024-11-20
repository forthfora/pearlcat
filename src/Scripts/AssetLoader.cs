using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Pearlcat;

public static class AssetLoader
{
    public static TextureFormat TextureFormat { get; set; } = TextureFormat.RGBA32;

    public static string AtlasesDirPath { get; set; } = $"{Plugin.MOD_ID}_atlases";
    public static string TexturesDirPath { get; set; } = $"{Plugin.MOD_ID}_textures";
    public static string SpritesDirPath { get; set; } = $"{Plugin.MOD_ID}_sprites";

    public static Dictionary<string, Texture2D> LoadedTextures { get; } = new();

    public static FAtlas? GetAtlas(string atlasName)
    {
        if (Futile.atlasManager.DoesContainAtlas(atlasName))
        {
            return Futile.atlasManager.LoadAtlas(atlasName);
        }

        var atlasDirName = Path.Combine(AtlasesDirPath, atlasName);

        if (Futile.atlasManager.DoesContainAtlas(atlasDirName))
        {
            return Futile.atlasManager.LoadAtlas(atlasDirName);
        }

        Plugin.Logger.LogError($"Atlas not found! ({atlasName})");
        return null;
    }

    public static Texture2D? GetTexture(string textureId)
    {
        if (!LoadedTextures.TryGetValue(textureId, out var texture))
        {
            return null;
        }

        return texture;
    }


    public static void LoadAssets()
    {
        LoadAtlases(AtlasesDirPath);
        LoadSprites(SpritesDirPath);
        LoadTextures(TexturesDirPath);
    }

    private static void LoadAtlases(string targetDirPath)
    {
        foreach (var filePath in AssetManager.ListDirectory(targetDirPath))
        {
            if (Path.GetExtension(filePath).ToLower() != ".txt")
            {
                continue;
            }

            var atlasFileName = Path.GetFileNameWithoutExtension(filePath);
            var atlasPath = Path.Combine(targetDirPath, atlasFileName);

            Futile.atlasManager.LoadAtlas(atlasPath);
            Plugin.Logger.LogWarning(atlasPath);
        }

        foreach (var dirPath in AssetManager.ListDirectory(targetDirPath, true))
        {
            LoadAtlases(dirPath);
        }
    }

    private static void LoadSprites(string targetDirPath)
    {
        foreach (var filePath in AssetManager.ListDirectory(targetDirPath))
        {
            if (Path.GetExtension(filePath).ToLower() != ".png")
            {
                continue;
            }

            var spriteFileName = Path.GetFileNameWithoutExtension(filePath);

            var texture = FileToTexture2D(filePath);

            if (texture == null)
            {
                continue;
            }

            Futile.atlasManager.LoadAtlasFromTexture(spriteFileName, texture, false);
        }

        foreach (var dirPath in AssetManager.ListDirectory(targetDirPath, true))
        {
            LoadSprites(dirPath);
        }
    }

    private static void LoadTextures(string targetDirPath)
    {
        foreach (var filePath in AssetManager.ListDirectory(targetDirPath))
        {
            if (Path.GetExtension(filePath).ToLower() != ".png")
            {
                continue;
            }

            var textureFileName = Path.GetFileNameWithoutExtension(filePath);

            var texture = FileToTexture2D(filePath);

            if (texture == null)
            {
                continue;
            }

            // Doesn't use Futile's manager, so can use the file name directly
            var textureId = textureFileName;

            LoadedTextures.Add(textureId, texture);
        }

        foreach (var dirPath in AssetManager.ListDirectory(targetDirPath, true))
        {
            LoadTextures(dirPath);
        }
    }

    // https://answers.unity.com/questions/432655/loading-texture-file-from-pngjpg-file-on-disk.html
    private static Texture2D? FileToTexture2D(string filePath)
    {
        var fileData = File.ReadAllBytes(filePath);

        var texture = new Texture2D(0, 0, TextureFormat, false)
        {
            anisoLevel = 0,
            filterMode = FilterMode.Point,
        };

        if (!texture.LoadImage(fileData))
        {
            return null;
        }

        return texture;
    }
}
