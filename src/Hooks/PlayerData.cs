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

                LoadTailTexture("tail");

                InitSounds(player);
                InitColors(player);
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

                MapTextureColor(tailTexture, Color.red, new Color(218, 236, 226));

                tailAtlas = Futile.atlasManager.LoadAtlasFromTexture(Plugin.MOD_ID + textureName + player.playerState.playerNumber, tailTexture, false);
            }


            public void RegenerateTail()
            {
                return;
                
                if (!Player.TryGetTarget(out var player)) return;

                if (player.graphicsModule == null) return;

                PlayerGraphics self = (PlayerGraphics)player.graphicsModule;

                TailSegment[] oldTail = self.tail;

                self.tail = new TailSegment[7];
                self.tail[0] = new TailSegment(self, 11.0f, 4.0f, null, 0.85f, 1.0f, 1.0f, true);
                self.tail[1] = new TailSegment(self, 8.0f, 7.0f, self.tail[0], 0.85f, 1.0f, 0.5f, true);
                self.tail[2] = new TailSegment(self, 9.0f, 7.0f, self.tail[1], 0.85f, 1.0f, 0.5f, true);
                self.tail[3] = new TailSegment(self, 7.0f, 7.0f, self.tail[2], 0.85f, 1.0f, 0.5f, true);
                self.tail[4] = new TailSegment(self, 6.0f, 4.0f, self.tail[3], 0.85f, 1.0f, 0.5f, true);
                self.tail[5] = new TailSegment(self, 4.0f, 2.0f, self.tail[4], 0.85f, 1.0f, 0.5f, false);
                self.tail[6] = new TailSegment(self, 1.0f, 1.0f, self.tail[5], 0.85f, 1.0f, 0.5f, false);

                for (var i = 0; i < self.tail.Length && i < oldTail.Length; i++)
                {
                    self.tail[i].pos = oldTail[i].pos;
                    self.tail[i].lastPos = oldTail[i].lastPos;
                    self.tail[i].vel = oldTail[i].vel;
                    self.tail[i].terrainContact = oldTail[i].terrainContact;
                    self.tail[i].stretched = oldTail[i].stretched;
                }

                List<BodyPart> oldBodyParts = self.bodyParts.ToList();
                oldBodyParts.RemoveAll(x => x is TailSegment);
                oldBodyParts.AddRange(self.tail);

                self.bodyParts = oldBodyParts.ToArray();

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
            public Color StaticEarHighlightColor;

            private void InitColors(Player player)
            {
                if (!SlugBaseCharacter.TryGet(Enums.Slugcat.Sacrifice, out var character)) return;

                if (!character.Features.TryGet(PlayerFeatures.CustomColors, out var customColors)) return;

                int playerNumber = !player.room.game.IsArenaSession && player.playerState.playerNumber == 0 ? -1 : player.playerState.playerNumber;

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
