using RWCustom;
using System.IO;
using UnityEngine;

namespace TheSacrifice
{
    internal static class AssetLoader
    {
        public const string SPRITES_DIRPATH = Plugin.MOD_ID + "_sprites";
        public const string ATLASES_DIRPATH = Plugin.MOD_ID + "_atlases";

        public static string GetUniqueName(string name) => Plugin.MOD_ID + "_" + name;

        public static FAtlas? GetAtlas(string atlasName)
        {
            string uniqueName = GetUniqueName(atlasName);

            string spriteDirName = SPRITES_DIRPATH + Path.AltDirectorySeparatorChar + Plugin.MOD_ID + "_" + atlasName;

            if (!Futile.atlasManager.DoesContainAtlas(spriteDirName))
            {
                string atlasDirName = ATLASES_DIRPATH + Path.AltDirectorySeparatorChar + Plugin.MOD_ID + "_" + atlasName;

                if (!Futile.atlasManager.DoesContainAtlas(atlasDirName))
                {
                    Plugin.Logger.LogError($"Atlas not found! ({uniqueName})");
                    return null;
                }

                return Futile.atlasManager.LoadAtlas(atlasDirName);
            }

            return Futile.atlasManager.LoadAtlas(spriteDirName);
        }


        public static void LoadAssets()
        {
            LoadSprites();
            LoadAtlases();
        }

        // Loads individual PNG files into their own separate atlases
        private static void LoadSprites()
        {
            foreach (string filePath in AssetManager.ListDirectory(SPRITES_DIRPATH))
            {
                // https://answers.unity.com/questions/432655/loading-texture-file-from-pngjpg-file-on-disk.html
                byte[] fileData = File.ReadAllBytes(filePath);
                Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, false)
                {
                    anisoLevel = 1,
                    filterMode = FilterMode.Point
                };
                texture.LoadImage(fileData);

                Futile.atlasManager.LoadAtlasFromTexture(GetUniqueName(Path.GetFileNameWithoutExtension(filePath)), texture, false);
            }
        }
        
        // Loads complete atlases 
        private static void LoadAtlases()
        {
            foreach (string filePath in AssetManager.ListDirectory(ATLASES_DIRPATH))
            {
                if (Path.GetExtension(filePath) != ".txt") continue;

                string atlasName = Path.GetFileNameWithoutExtension(filePath);
                Futile.atlasManager.LoadAtlas(ATLASES_DIRPATH + Path.AltDirectorySeparatorChar + atlasName);
            }
        }

    }
}
