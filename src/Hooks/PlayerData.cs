using IL.Menu.Remix.MixedUI;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using SlugBase;
using SlugBase.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace TheSacrifice
{
    internal static partial class Hooks
    {
        private static ConditionalWeakTable<Player, PlayerModule> PlayerData = new ConditionalWeakTable<Player, PlayerModule>();

        private class PlayerModule
        {
            public WeakReference<Player> Player;

            public int firstSprite;
            public int lastSprite;

            public int leftEarSprite;
            public int rightEarSprite;

            public LightSource? activeObjectGlow;

            public PlayerModule(Player player)
            {
                Player = new WeakReference<Player>(player);

                LoadTailTexture("tail");

                InitSounds(player);
                InitColors(player);
            }

            public List<AbstractPhysicalObject> inventory = new List<AbstractPhysicalObject>();

            public bool canSwallowOrRegurgitate = true;

            public AbstractPhysicalObject? realizedActiveObject = null;
            public int? selectedIndex = null;
            public int? predictedIndex = null;

            public List<Color> accentColors = new List<Color>();
            public float shortcutColorStacker = 0.0f;
            public int shortcutColorStackerDirection = 1;

            public AbstractPhysicalObject? transferObject = null;
            public Vector2? transferObjectInitialPos = null;
            public bool canTransferObject = true;
            public int transferStacker = 0;


            public FAtlas? tailAtlas;

            public void LoadTailTexture(string textureName)
            {
                Texture2D? tailTexture = AssetLoader.GetTexture(textureName);
                if (tailTexture == null) return;

                if (!Player.TryGetTarget(out var player)) return;

                if (Futile.atlasManager.DoesContainElementWithName(textureName)) Futile.atlasManager.ActuallyUnloadAtlasOrImage(textureName);

                tailAtlas = Futile.atlasManager.LoadAtlasFromTexture(Plugin.MOD_ID + textureName + player.playerState.playerNumber, tailTexture, false);
            }



            public DynamicSoundLoop storingObjectSound = null!;
            public DynamicSoundLoop retrievingObjectSound = null!;

            private void InitSounds(Player player)
            {
                storingObjectSound = new ChunkDynamicSoundLoop(player.bodyChunks[0]);
                storingObjectSound.sound = Enums.Sounds.StoringObject;
                storingObjectSound.destroyClipWhenDone = false;
                storingObjectSound.Volume = 0.0f;

                retrievingObjectSound = new ChunkDynamicSoundLoop(player.bodyChunks[0]);
                retrievingObjectSound.sound = Enums.Sounds.RetrievingObject;
                retrievingObjectSound.destroyClipWhenDone = false;
                retrievingObjectSound.Volume = 0.0f;
            }



            private void InitColors(Player player)
            {
                if (!SlugBaseCharacter.TryGet(Enums.Slugcat.Sacrifice, out var character)) return;

                if (!character.Features.TryGet(PlayerFeatures.CustomColors, out var customColors)) return;

                int playerNumber = !player.room.game.IsArenaSession && player.playerState.playerNumber == 0 ? -1 : player.playerState.playerNumber;
            }
        }


        // Constant Features
        private const int MaxStorageCount = 10;
        private const float FrameShortcutColorAddition = 0.003f;

        private const int FramesToStoreObject = 80;
        private const int FramesToRetrieveObject = 80;

        private static readonly Vector2 ActiveObjectBaseOffset = new Vector2(0.0f, 20.0f);



        private static List<PlayerModule> GetAllPlayerData(RainWorldGame game)
        {
            List<PlayerModule> allPlayerData = new List<PlayerModule>();
            List<AbstractCreature> players = game.Players;

            if (players == null) return allPlayerData;

            foreach (AbstractCreature creature in players)
            {
                if (creature.realizedCreature == null) continue;

                if (creature.realizedCreature is not Player player) continue;

                if (!PlayerData.TryGetValue(player, out PlayerModule playerModule)) continue;

                allPlayerData.Add(playerModule);
            }

            return allPlayerData;
        }

        private static bool IsCustomSlugcat(Player player) => player.SlugCatClass == Enums.Slugcat.Sacrifice;
    }
}
