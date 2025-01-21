using System.IO;

namespace Pearlcat;

public static class AssetLoader
{
    public static string AtlasesDirPath => $"{Plugin.MOD_ID}_atlases";
    public static string SpritesDirPath => $"{Plugin.MOD_ID}_sprites";


    public static FAtlas? GetAtlas(string atlasName)
    {
        var atlasDirPath = Path.Combine(AtlasesDirPath, atlasName);

        if (Futile.atlasManager.DoesContainAtlas(atlasDirPath))
        {
            return Futile.atlasManager.LoadAtlas(atlasDirPath);
        }

        Plugin.Logger.LogError($"Atlas not found! ({atlasName})");
        return null;
    }


    public static void LoadAssets()
    {
        LoadAtlases(AtlasesDirPath);
        LoadSprites(SpritesDirPath);
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

            var spriteName = Path.GetFileNameWithoutExtension(filePath);
            var spriteFilePath = filePath.TrimEnd(Path.GetExtension(filePath));

            Futile.atlasManager.ActuallyLoadAtlasOrImage(spriteName, spriteFilePath + Futile.resourceSuffix, "");
        }

        foreach (var dirPath in AssetManager.ListDirectory(targetDirPath, true))
        {
            LoadSprites(dirPath);
        }
    }
}
