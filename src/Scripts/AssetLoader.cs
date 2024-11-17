using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Pearlcat;

public static class AssetLoader
{
    private const string ATLASES_DIRPATH = Plugin.MOD_ID + "_atlases";
    private const string SPRITES_DIRPATH = Plugin.MOD_ID + "_sprites";
    private const string TEXTURES_DIRPATH = Plugin.MOD_ID + "_textures";

    private const TextureFormat TEXTURE_FORMAT = TextureFormat.RGBA32;

    public static Dictionary<string, Texture2D> Textures { get; } = new();
    public static string GetUniqueName(string name)
    {
        return Plugin.MOD_ID + "_" + name;
    }


    public static FAtlas? GetAtlas(string atlasName)
    {
        var uniqueName = GetUniqueName(atlasName);

        if (Futile.atlasManager.DoesContainAtlas(uniqueName))
        {
            return Futile.atlasManager.LoadAtlas(uniqueName);
        }


        var atlasDirName = ATLASES_DIRPATH + Path.AltDirectorySeparatorChar + Plugin.MOD_ID + "_" + atlasName;

        if (Futile.atlasManager.DoesContainAtlas(atlasDirName))
        {
            return Futile.atlasManager.LoadAtlas(atlasDirName);
        }


        Plugin.Logger.LogError($"Atlas not found! ({uniqueName})");
        return null;

    }

    public static Texture2D? GetTexture(string textureName)
    {
        if (!Textures.ContainsKey(textureName))
        {
            return null;
        }

        var originalTexture = Textures[textureName];

        var copiedTexture = new Texture2D(originalTexture.width, originalTexture.height, TEXTURE_FORMAT, false);
        
        Graphics.CopyTexture(originalTexture, copiedTexture);

        return copiedTexture;
    }


    public static void LoadAssets()
    {
        LoadAtlases();
        LoadSprites();
        LoadTextures();
    }

    // Loads complete atlases 
    private static void LoadAtlases()
    {
        foreach (var filePath in AssetManager.ListDirectory(ATLASES_DIRPATH))
        {
            if (Path.GetExtension(filePath) != ".txt")
            {
                continue;
            }

            var atlasName = Path.GetFileNameWithoutExtension(filePath);
            
            Futile.atlasManager.LoadAtlas(ATLASES_DIRPATH + Path.AltDirectorySeparatorChar + atlasName);
        }
    }

    // Loads individual PNG files into their own separate atlases
    private static void LoadSprites()
    {
        foreach (var filePath in AssetManager.ListDirectory(SPRITES_DIRPATH))
        {
            if (Path.GetExtension(filePath).ToLower() != ".png")
            {
                continue;
            }

            var atlasName = Path.GetFileNameWithoutExtension(filePath);

            var texture = FileToTexture2D(filePath);
            
            if (texture == null)
            {
                continue;
            }

            Futile.atlasManager.LoadAtlasFromTexture(atlasName, texture, false);
        }
    }

    // Load individual PNG files into a dictionary of Texture2Ds
    private static void LoadTextures()
    {
        foreach (var filePath in AssetManager.ListDirectory(TEXTURES_DIRPATH))
        {
            if (Path.GetExtension(filePath).ToLower() != ".png")
            {
                continue;
            }

            var textureName = Path.GetFileNameWithoutExtension(filePath);

            var texture = FileToTexture2D(filePath);
            
            if (texture == null)
            {
                continue;
            }

            Textures.Add(textureName, texture);
        }
    }


    // https://answers.unity.com/questions/432655/loading-texture-file-from-pngjpg-file-on-disk.html
    private static Texture2D? FileToTexture2D(string filePath)
    {
        var fileData = File.ReadAllBytes(filePath);

        var texture = new Texture2D(0, 0, TEXTURE_FORMAT, false)
        {
            anisoLevel = 0,
            filterMode = FilterMode.Point,
        };

        return texture.LoadImage(fileData) ? texture : null;
    }
}
