using IL.Menu.Remix.MixedUI;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
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
        private static ConditionalWeakTable<Player, PlayerEx> PlayerData = new ConditionalWeakTable<Player, PlayerEx>();
        private static ConditionalWeakTable<RainWorldGame, List<AbstractPhysicalObject>> GameInventory = new ConditionalWeakTable<RainWorldGame, List<AbstractPhysicalObject>>();

        private class PlayerEx
        {
            public int firstSprite;
            public int lastSprite;

            public int leftEarSprite;
            public int rightEarSprite;

            public LightSource? activeObjectGlow;

            public PlayerEx(Player player)
            {

                InitSounds(player);
            }


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



            public DynamicSoundLoop storingObjectSound = null!;
            public DynamicSoundLoop retrievingObjectSound = null!;

            private void InitSounds(Player player)
            {
                storingObjectSound = new ChunkDynamicSoundLoop(player.bodyChunks[0]);
                storingObjectSound.sound = Enums.Sounds.StoringObject;
                storingObjectSound.Volume = 0.0f;

                retrievingObjectSound = new ChunkDynamicSoundLoop(player.bodyChunks[0]);
                retrievingObjectSound.sound = Enums.Sounds.RetrievingObject;
                retrievingObjectSound.Volume = 0.0f;
            }
        }


        // Constant Features
        private const int MaxStorageCount = 10;
        private const float FrameShortcutColorAddition = 0.003f;

        private const int FramesToStoreObject = 80;
        private const int FramesToRetrieveObject = 80;

        private static readonly Vector2 ActiveObjectBaseOffset = new Vector2(0.0f, 20.0f);



        private static List<PlayerEx> GetAllPlayerData(RainWorldGame game)
        {
            List<PlayerEx> allPlayerData = new List<PlayerEx>();
            List<AbstractCreature> players = game.Players;

            if (players == null) return allPlayerData;

            foreach (AbstractCreature creature in players)
            {
                if (creature.realizedCreature == null) continue;

                if (creature.realizedCreature is not Player player) continue;

                PlayerEx playerEx;
                if (!PlayerData.TryGetValue(player, out playerEx)) continue;

                allPlayerData.Add(playerEx);
            }

            return allPlayerData;
        }

        private static bool IsCustomSlugcat(Player player) => player.SlugCatClass == Enums.Slugcat.Sacrifice;
    }
}
