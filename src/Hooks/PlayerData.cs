using IL.Menu.Remix.MixedUI;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using SlugBase;
using SlugBase.DataTypes;
using SlugBase.Features;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Color = UnityEngine.Color;
using Vector2 = UnityEngine.Vector2;

namespace TheSacrifice
{
    internal static partial class Hooks
    {
        private static ConditionalWeakTable<Player, PlayerModule> PlayerData = new ConditionalWeakTable<Player, PlayerModule>();

        private class PlayerModule
        {
            public WeakReference<Player> Player;

            // Sprites
            public int firstSprite;
            public int lastSprite;

            public int leftEar;
            public int rightEar;

            public int leftEarHighlight;
            public int rightEarHighlight;


            // Attached Objects
            public LightSource? activeObjectGlow;


            public PlayerModule(Player player)
            {
                Player = new WeakReference<Player>(player);

                InitSounds(player);
                InitColors(player);

                LoadTailTexture("tail");
            }



            // Fields
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



            // Tail
            public FAtlas? tailAtlas;

            public void LoadTailTexture(string textureName)
            {
                Texture2D? tailTexture = AssetLoader.GetTexture(textureName);
                if (tailTexture == null) return;

                if (!Player.TryGetTarget(out var player)) return;

                if (Futile.atlasManager.DoesContainElementWithName(textureName)) Futile.atlasManager.ActuallyUnloadAtlasOrImage(textureName);

                // Remap Colours
                MapAlphaToColor(tailTexture, 1.0f, BodyColor);
                MapAlphaToColor(tailTexture, 0.0f, Color.white);

                tailAtlas = Futile.atlasManager.LoadAtlasFromTexture(Plugin.MOD_ID + textureName + player.playerState.playerNumber, tailTexture, false);
            }


            public void RegenerateTail()
            {
                if (!Player.TryGetTarget(out var player)) return;

                if (player.graphicsModule == null) return;

                PlayerGraphics self = (PlayerGraphics)player.graphicsModule;
                TailSegment[] newTail = new TailSegment[6];

                newTail[0] = new TailSegment(self, 8.0f, 4.0f, null, 0.85f, 1.0f, 1.0f, true);
                newTail[1] = new TailSegment(self, 7.0f, 7.0f, newTail[0], 0.85f, 1.0f, 0.5f, true);
                newTail[2] = new TailSegment(self, 6.0f, 7.0f, newTail[1], 0.85f, 1.0f, 0.5f, true);
                newTail[3] = new TailSegment(self, 5.0f, 7.0f, newTail[2], 0.85f, 1.0f, 0.5f, true);
                newTail[4] = new TailSegment(self, 2.5f, 7.0f, newTail[3], 0.85f, 1.0f, 0.5f, true);
                newTail[5] = new TailSegment(self, 1.0f, 7.0f, newTail[4], 0.85f, 1.0f, 0.5f, true);

                for (var i = 0; i < self.tail.Length && i < self.tail.Length; i++)
                {
                    newTail[i].pos = self.tail[i].pos;
                    newTail[i].lastPos = self.tail[i].lastPos;
                    newTail[i].vel = self.tail[i].vel;
                    newTail[i].terrainContact = self.tail[i].terrainContact;
                    newTail[i].stretched = self.tail[i].stretched;
                }

                self.tail = newTail;

                // Generate the new body parts array, whilst attempting to preserve the existing indexes
                // The flaw with this is that if the new tail is shorter than the default, this will crash
                List<BodyPart> newBodyParts = self.bodyParts.ToList();
                List<int> oldTailSegmentIndexes = new List<int>();

                bool reachedOldTail = false;

                // Get existing indexes
                for (int i = 0; i <= newBodyParts.Count; i++)
                {
                    if (newBodyParts[i] is TailSegment)
                    {
                        reachedOldTail = true;
                        oldTailSegmentIndexes.Add(i);
                    }
                    else if (reachedOldTail)
                    {
                        break;
                    }
                }

                int tailSegmentIndex = 0;

                // Where possible, substitute the existing indexes with the new tail
                foreach (int i in oldTailSegmentIndexes)
                {
                    newBodyParts[i] = self.tail[tailSegmentIndex];
                    tailSegmentIndex++;
                }

                // For any remaining tail segments, append them to the end
                for (int i = tailSegmentIndex; i < self.tail.Length; i++)
                {
                    newBodyParts.Add(self.tail[tailSegmentIndex]);
                }

                self.bodyParts = newBodyParts.ToArray();

            }



            // Sounds
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



            // Colours
            public Color BodyColor;
            public Color EyesColor;

            public Color StaticEarHighlightColor;


            private void InitColors(Player player)
            {
                if (!SlugBaseCharacter.TryGet(Enums.Slugcat.Sacrifice, out var character)) return;

                if (!character.Features.TryGet(PlayerFeatures.CustomColors, out var customColors)) return;

                int playerNumber = !player.room.game.IsArenaSession && player.playerState.playerNumber == 0 ? -1 : player.playerState.playerNumber;

                SetColor(customColors, playerNumber, ref BodyColor, "Body");
                SetColor(customColors, playerNumber, ref EyesColor, "Eyes");

                SetColor(customColors, playerNumber, ref StaticEarHighlightColor, "EarHighlight");
            }

            private void SetColor(ColorSlot[] customColors, int playerNumber, ref Color color, string name)
            {
                var customColor = customColors.Where(customColor => customColor.Name == name).FirstOrDefault();
                if (customColor == null) return;

                color = customColor.GetColor(playerNumber);
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
