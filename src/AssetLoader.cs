using RWCustom;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TheSacrifice
{
    internal static class AssetLoader
    {
        public const string ATLASES_DIRPATH = Plugin.MOD_ID + "_atlases";
        public const string SPRITES_DIRPATH = Plugin.MOD_ID + "_sprites";
        public const string TEXTURES_DIRPATH = Plugin.MOD_ID + "_textures";

        private static readonly Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

        public static string GetUniqueName(string name) => Plugin.MOD_ID + "_" + name;

        public static FAtlas? GetAtlas(string atlasName)
        {
            string uniqueName = GetUniqueName(atlasName);

            if (Futile.atlasManager.DoesContainAtlas(uniqueName)) return Futile.atlasManager.LoadAtlas(uniqueName);

            string atlasDirName = ATLASES_DIRPATH + Path.AltDirectorySeparatorChar + Plugin.MOD_ID + "_" + atlasName;

            if (!Futile.atlasManager.DoesContainAtlas(atlasDirName))
            {
                Plugin.Logger.LogError($"Atlas not found! ({uniqueName})");
                return null;
            }

            return Futile.atlasManager.LoadAtlas(atlasDirName);
        }

        public static Texture2D? GetTexture(string textureName)
        {
            if (!textures.ContainsKey(textureName)) return null;

            Texture2D originalTexture = textures[textureName];
            Texture2D? copiedTexture = new Texture2D(originalTexture.width, originalTexture.height, TextureFormat.ARGB32, false);
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
            foreach (string filePath in AssetManager.ListDirectory(ATLASES_DIRPATH))
            {
                if (Path.GetExtension(filePath) != ".txt") continue;

                string atlasName = Path.GetFileNameWithoutExtension(filePath);
                Futile.atlasManager.LoadAtlas(ATLASES_DIRPATH + Path.AltDirectorySeparatorChar + atlasName);
            }
        }

        // Loads individual PNG files into their own separate atlases
        private static void LoadSprites()
        {
            foreach (string filePath in AssetManager.ListDirectory(SPRITES_DIRPATH))
            {
                if (Path.GetExtension(filePath).ToLower() != ".png") continue;

                string atlasName = Path.GetFileNameWithoutExtension(filePath);

                Texture2D? texture = FileToTexture2D(filePath);
                if (texture == null) continue;

                Futile.atlasManager.LoadAtlasFromTexture(atlasName, texture, false);
            }
        }

        // Load individual PNG files into a dictionary of Texture2Ds
        public static void LoadTextures()
        {
            foreach (string filePath in AssetManager.ListDirectory(TEXTURES_DIRPATH))
            {
                if (Path.GetExtension(filePath).ToLower() != ".png") continue;

                string textureName = Path.GetFileNameWithoutExtension(filePath);

                Texture2D? texture = FileToTexture2D(filePath);
                if (texture == null) continue;

                textures.Add(textureName, texture);
            }
        }

        // https://answers.unity.com/questions/432655/loading-texture-file-from-pngjpg-file-on-disk.html
        private static Texture2D? FileToTexture2D(string filePath)
        {
            byte[] fileData = File.ReadAllBytes(filePath);

            Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, false)
            {
                anisoLevel = 1,
                filterMode = FilterMode.Point
            };

            if (!texture.LoadImage(fileData)) return null;

            return texture;
        }
    }
}
