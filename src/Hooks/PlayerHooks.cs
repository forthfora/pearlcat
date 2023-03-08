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
        private static void ApplyPlayerHooks()
        {
            On.Player.Update += Player_Update;

            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
            On.PlayerGraphics.ctor += PlayerGraphics_ctor;
            On.PlayerGraphics.PlayerObjectLooker.HowInterestingIsThisObject += PlayerObjectLooker_HowInterestingIsThisObject;

            On.Player.GrabUpdate += Player_GrabUpdate;
            On.Player.ShortCutColor += Player_ShortCutColor;
            On.Player.NewRoom += Player_NewRoom;
            On.Player.Grabability += Player_Grabability;
            On.Player.GraphicsModuleUpdated += Player_GraphicsModuleUpdated;

            On.Creature.SuckedIntoShortCut += Creature_SuckedIntoShortCut;
            On.Creature.Grab += Creature_Grab;

            try
            {
                IL.Player.GrabUpdate += Player_GrabUpdateIL;
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError(e);
            }
        }

        private static void Player_GraphicsModuleUpdated(On.Player.orig_GraphicsModuleUpdated orig, Player self, bool actuallyViewed, bool eu)
        {
            orig(self, actuallyViewed, eu);

            PlayerEx? playerEx;
            if (!PlayerData.TryGetValue(self, out playerEx)) return;

            List<AbstractPhysicalObject> inventory;
            if (!GameInventory.TryGetValue(self.room.game, out inventory)) return;

            if (playerEx.transferObject == null)
            {
                playerEx.transferStacker = 0;
                return;
            }

            playerEx.transferObjectInitialPos ??= playerEx.transferObject.realizedObject.firstChunk.pos;

            playerEx.transferStacker++;
            bool puttingInStorage = playerEx.transferObject != GetStoredActiveObject(self);

            if (puttingInStorage)
            {
                playerEx.transferObject.realizedObject.firstChunk.MoveFromOutsideMyUpdate(eu, Vector2.Lerp((Vector2)playerEx.transferObjectInitialPos, GetActiveObjectPos(self), (float)playerEx.transferStacker / FramesToStoreObject));

                if (playerEx.transferStacker < FramesToStoreObject) return;

                StoreObject(self, playerEx.transferObject);
                DestroyRealizedActiveObject(self);
                ActivateObjectInStorage(self, inventory.Count - 1);

                playerEx.transferObject = null;
                playerEx.transferObjectInitialPos = null;
                playerEx.transferStacker = 0;
                return;
            }

            if (playerEx.transferStacker < FramesToRetrieveObject) return;

            RetrieveObject(self);

            playerEx.transferObject = null;
            playerEx.transferObjectInitialPos = null;
            playerEx.transferStacker = 0;
        }

        private static Player.ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
        {
            if (IsRealizedActiveObject(obj.abstractPhysicalObject)) return Player.ObjectGrabability.CantGrab;

            return orig(self, obj);
        }

        private static void Player_NewRoom(On.Player.orig_NewRoom orig, Player self, Room newRoom)
        {
            DestroyRealizedActiveObject(self); 
            orig(self, newRoom);
        }

        private static ConditionalWeakTable<Player, PlayerEx> PlayerData = new ConditionalWeakTable<Player, PlayerEx>();
        private static ConditionalWeakTable<RainWorldGame, List<AbstractPhysicalObject>> GameInventory = new ConditionalWeakTable<RainWorldGame, List<AbstractPhysicalObject>>();

        private class PlayerEx
        {
            public readonly LightSource activeObjectGlow;

            public PlayerEx(Player player)
            {
                activeObjectGlow = new LightSource(GetActiveObjectPos(player), false, Color.white, player);
                activeObjectGlow.affectedByPaletteDarkness = 0.0f;
                activeObjectGlow.stayAlive = true;
                player.room.AddObject(activeObjectGlow);
            }

            public bool canSwallowOrRegurgitate = true;

            public AbstractPhysicalObject? realizedActiveObject = null;
            public int? selectedIndex = null;
            public int? predictedIndex = null;

            public List<Color> accentColors = new List<Color>();
            public float shortcutColorStacker = 0.0f;
            public int shortcutColorStackerDirection = 1;

            public int transferStacker = 0;
            public Vector2? transferObjectInitialPos = null;
            public AbstractPhysicalObject? transferObject = null;

        }


        private const int MaxStorageCount = 10;
        private const float FrameShortcutColorAddition = 0.003f;

        private const int FramesToStoreObject = 80;
        private const int FramesToRetrieveObject = 80;

        private static readonly Vector2 ActiveObjectBaseOffset = new Vector2(0.0f, 10.0f);


        private static bool IsCustomSlugcat(Player player) => player.SlugCatClass.ToString() == Plugin.SLUGCAT_ID;

        private static AbstractPhysicalObject? GetStoredActiveObject(Player player)
        {
            if (player.room == null) return null;

            List<AbstractPhysicalObject> inventory;
            if (!GameInventory.TryGetValue(player.room.game, out inventory)) return null;
           
            PlayerEx playerEx;
            if (!PlayerData.TryGetValue(player, out playerEx)) return null;

            if (playerEx.selectedIndex == null) return null;

            if (playerEx.selectedIndex >= inventory.Count) return null;

            return inventory[(int)playerEx.selectedIndex];
        }

        private static AbstractPhysicalObject? GetRealizedActiveObject(Player player)
        {
            PlayerEx playerEx;
            if (!PlayerData.TryGetValue(player, out playerEx)) return null;
            return playerEx.realizedActiveObject;
        }

        private static void TryRealizeActiveObject(Player player)
        {
            PlayerEx playerEx;
            if (!PlayerData.TryGetValue(player, out playerEx)) return;

            if (player.inShortcut) return;

            AbstractPhysicalObject? storedActiveObject = GetStoredActiveObject(player);

            if (storedActiveObject == null) return;

            if (playerEx.realizedActiveObject != null) return;

            AbstractPhysicalObject realizedActiveObject = CloneObject(player.room.world, storedActiveObject);

            //player.room.abstractRoom.AddEntity(realizedActiveObject);

            WorldCoordinate newWorldCoordinate = player.room.ToWorldCoordinate(GetActiveObjectPos(player));
            realizedActiveObject.pos = newWorldCoordinate;

            realizedActiveObject.RealizeInRoom();
            playerEx.realizedActiveObject = realizedActiveObject;

            if (realizedActiveObject.realizedObject == null) return;

            realizedActiveObject.realizedObject.CollideWithTerrain = false;
            realizedActiveObject.realizedObject.gravity = 0.0f;

            if (realizedActiveObject.realizedObject is Weapon weapon) weapon.rotationSpeed = 0.0f;

            playerEx.accentColors = GetObjectAccentColors(realizedActiveObject);

        }

        private static void DestroyRealizedActiveObject(Player player)
        {
            PlayerEx playerEx;
            if (!PlayerData.TryGetValue(player, out playerEx)) return;

            AbstractPhysicalObject? realizedActiveObject = playerEx.realizedActiveObject;
            realizedActiveObject?.realizedObject?.Destroy();
            realizedActiveObject?.Destroy();
            playerEx.realizedActiveObject = null;
        }

        private static Vector2 GetActiveObjectPos(Player player)
        {
            Vector2 pos;

            if (player.gravity == 0.0f)
            {
                pos = player.graphicsModule.bodyParts[6].pos + (ActiveObjectBaseOffset.magnitude * player.bodyChunks[0].Rotation);
                return pos;    
            }

            pos = player.graphicsModule.bodyParts[6].pos + ActiveObjectBaseOffset;
            pos.x += player.mainBodyChunk.vel.x * 1.0f;

            return pos;
        }

        private static void StoreObject(Player player, AbstractPhysicalObject abstractObject)
        {
            List<AbstractPhysicalObject> inventory;
            if (!GameInventory.TryGetValue(player.room.game, out inventory)) return;

            if (inventory.Count >= MaxStorageCount) return;

            inventory.Add(abstractObject);
            abstractObject.realizedObject?.Destroy();
        }

        private static void RetrieveObject(Player player)
        {
            if (player.FreeHand() <= -1) return;

            List<AbstractPhysicalObject> inventory;
            if (!GameInventory.TryGetValue(player.room.game, out inventory)) return;

            PlayerEx playerEx;
            if (!PlayerData.TryGetValue(player, out playerEx)) return;

            foreach (var a in inventory)
                Plugin.Logger.LogWarning(inventory.IndexOf(a) + " - " + a.type);

            AbstractPhysicalObject? activeObject = GetStoredActiveObject(player);

            if (activeObject == null) return;

            AbstractPhysicalObject objectForHand = CloneObject(player.room.world, activeObject);

            objectForHand.pos = player.abstractCreature.pos;
            player.room.abstractRoom.AddEntity(objectForHand);

            objectForHand.RealizeInRoom();

            inventory.Remove(activeObject);
            DestroyRealizedActiveObject(player);
            playerEx.realizedActiveObject = null;

            player.SlugcatGrab(objectForHand.realizedObject, player.FreeHand());
        }

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

        private static void SelectNextObject(Player player)
        {
            List<AbstractPhysicalObject> inventory;
            if (!GameInventory.TryGetValue(player.room.game, out inventory)) return;

            PlayerEx playerEx;
            if (!PlayerData.TryGetValue(player, out playerEx)) return;

            if (playerEx.predictedIndex == null) return;

            int startIndex = (int)playerEx.predictedIndex;
            List<int> selectedIndexes = new List<int>();

            foreach (PlayerEx ex in GetAllPlayerData(player.room.game))
            {
                if (ex.selectedIndex == null) continue;
                selectedIndexes.Add((int)ex.selectedIndex);
            }

            for (int i = startIndex + 1; i < inventory.Count; i++)
            {
                if (i == startIndex) break;

                if (i > inventory.Count)
                {
                    i = -1;
                    continue;
                }

                if (selectedIndexes.Contains(i)) continue;

                playerEx.predictedIndex = i;
                break;
            }

            Plugin.Logger.LogWarning($"selected next object ({playerEx.predictedIndex})");
        }

        private static void SelectPreviousObject(Player player)
        {
            List<AbstractPhysicalObject> inventory;
            if (!GameInventory.TryGetValue(player.room.game, out inventory)) return;

            PlayerEx playerEx;
            if (!PlayerData.TryGetValue(player, out playerEx)) return;

            if (playerEx.predictedIndex == null) return;

            int startIndex = (int)playerEx.predictedIndex;
            List<int> selectedIndexes = new List<int>();

            foreach (PlayerEx ex in GetAllPlayerData(player.room.game))
            {
                if (ex.selectedIndex == null) continue;
                selectedIndexes.Add((int)ex.selectedIndex);
            }

            for (int i = startIndex - 1; i < inventory.Count; i--)
            {
                if (i == startIndex) break;

                if (i < inventory.Count)
                {
                    i = inventory.Count;
                    continue;
                }

                if (selectedIndexes.Contains(i)) continue;

                playerEx.predictedIndex = i;
                break;
            }

            Plugin.Logger.LogWarning($"selected prev object ({playerEx.predictedIndex})");
        }

        private static void ActivateObjectInStorage(Player player, int objectIndex)
        {
            List<AbstractPhysicalObject> inventory;
            if (!GameInventory.TryGetValue(player.room.game, out inventory)) return;

            if (objectIndex >= inventory.Count) return;

            foreach (PlayerEx ex in GetAllPlayerData(player.room.game))
            {
                if (ex.selectedIndex == objectIndex) return;
            }

            PlayerEx playerEx;
            if (!PlayerData.TryGetValue(player, out playerEx)) return;

            DestroyRealizedActiveObject(player);
            playerEx.selectedIndex = objectIndex;
        }

        private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);

            if (!IsCustomSlugcat(self)) return;

            PlayerEx playerEx;
            if (!PlayerData.TryGetValue(self, out playerEx)) PlayerData.Add(self, playerEx = new PlayerEx(self));

            TryRealizeActiveObject(self);
            AbstractPhysicalObject? activeObject = GetRealizedActiveObject(self);

            if (activeObject == null || activeObject.realizedObject == null) return;

            Vector2 targetPos = GetActiveObjectPos(self);
            activeObject.realizedObject.firstChunk.pos = targetPos;
            playerEx.activeObjectGlow.pos = targetPos;
        }

        private static void Player_GrabUpdateIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            ILLabel dest = null!;

            // Allow disabling of ordinary swallowing mechanic
            c.GotoNext(MoveType.After,
                x => x.MatchLdcR4(0.5f),
                x => x.MatchBltUn(out _),
                x => x.MatchLdcI4(0),
                x => x.MatchStloc(1),
                x => x.MatchLdloc(1),   
                x => x.MatchBrfalse(out dest)
                );

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<Player, bool>>((player) =>
            {
                PlayerEx playerEx;
                if (!PlayerData.TryGetValue(player, out playerEx)) return true;

                return playerEx.canSwallowOrRegurgitate;
            });

            c.Emit(OpCodes.Brfalse, dest);
        }

        private static AbstractPhysicalObject CloneObject(World world, AbstractPhysicalObject originalObject) => SaveState.AbstractPhysicalObjectFromString(world, originalObject.ToString());

        private static void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
        {
            AbstractPhysicalObject? heldStorable = null;

            List<AbstractPhysicalObject> inventory;
            if (!GameInventory.TryGetValue(self.room.game, out inventory)) GameInventory.Add(self.room.game, new List<AbstractPhysicalObject>());

            PlayerEx? playerEx;
            if (!PlayerData.TryGetValue(self, out playerEx)) goto ORIG;

            playerEx.canSwallowOrRegurgitate = true;

            for (int i = 0; i < self.grasps.Length; i++)
            {
                if (inventory.Count >= MaxStorageCount) continue;

                if (self.grasps[i] == null) continue;

                AbstractPhysicalObject heldObject = self.grasps[i].grabbed.abstractPhysicalObject;

                if (heldObject.realizedObject == null) continue;

                if (!IsObjectStorable(heldObject)) continue;

                heldStorable = heldObject;
                break;
            }

            ORIG:
            orig(self, eu);

            if (playerEx == null || inventory == null) return;

            if (!IsStoreKeybindPressed(self))
            {
                playerEx.transferObject = null;
                return;
            }

            if (heldStorable == null)
            {
                playerEx.transferObject = GetStoredActiveObject(self);
                return;
            }

            // Store keybind held
            playerEx.canSwallowOrRegurgitate = false;
            playerEx.transferObject = heldStorable;
        }

        private static bool IsStoreKeybindPressed(Player player)
        {
            if (!Options.usesCustomStoreKeybind.Value && player.input[0].y == 1.0f && player.input[0].pckp) return true;

            return player.playerState.playerNumber switch
            {
                0 => Input.GetKey(Options.storeKeybindPlayer1.Value) || Input.GetKey(Options.storeKeybindKeyboard.Value),
                1 => Input.GetKey(Options.storeKeybindPlayer2.Value),
                2 => Input.GetKey(Options.storeKeybindPlayer3.Value),
                3 => Input.GetKey(Options.storeKeybindPlayer4.Value),

                _ => false
            };
        }

        private static bool IsDashKeybindPressed(Player player)
        {
            if (!Options.usesCustomDashKeybind.Value && player.input[0].jmp && player.input[0].pckp) return true;

            return player.playerState.playerNumber switch
            {
                0 => Input.GetKey(Options.dashKeybindPlayer1.Value) || Input.GetKey(Options.dashKeybindKeyboard.Value),
                1 => Input.GetKey(Options.dashKeybindPlayer2.Value),
                2 => Input.GetKey(Options.dashKeybindPlayer3.Value),
                3 => Input.GetKey(Options.dashKeybindPlayer4.Value),

                _ => false
            };
        }

        private static bool IsSwapKeybindPressed(Player player)
        {
            return player.playerState.playerNumber switch
            {
                0 => Input.GetKey(Options.swapKeybindPlayer1.Value) || Input.GetKey(Options.swapKeybindKeyboard.Value),
                1 => Input.GetKey(Options.swapKeybindPlayer2.Value),
                2 => Input.GetKey(Options.swapKeybindPlayer3.Value),
                3 => Input.GetKey(Options.swapKeybindPlayer4.Value),

                _ => false
            };
        }

        private static float PlayerObjectLooker_HowInterestingIsThisObject(On.PlayerGraphics.PlayerObjectLooker.orig_HowInterestingIsThisObject orig, PlayerGraphics.PlayerObjectLooker self, PhysicalObject obj)
        {
            AbstractPhysicalObject? activeObject = GetRealizedActiveObject(self.owner.player);

            if (obj != null && obj.abstractPhysicalObject == activeObject) return 0.0f;

            return orig(self, obj);
        }

        private static void PlayerGraphics_ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
        {
            orig(self, ow);

            if (!IsCustomSlugcat(self.player)) return;

            //self.tail[0] = new TailSegment(self, 8f, 2f, null, 0.85f, 1f, 1f, true);
            //self.tail[1] = new TailSegment(self, 6f, 3.5f, self.tail[0], 0.85f, 1f, 0.5f, true);
            //self.tail[2] = new TailSegment(self, 4f, 3.5f, self.tail[1], 0.85f, 1f, 0.5f, true);
            //self.tail[3] = new TailSegment(self, 2f, 3.5f, self.tail[2], 0.85f, 1f, 0.5f, true);
        }

        private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            if (!IsCustomSlugcat(self.player)) return;

            UpdateCustomPlayerSprite(sLeaser, 0, "Body", "body");
            UpdateCustomPlayerSprite(sLeaser, 1, "Hips", "hips");
            UpdateCustomPlayerSprite(sLeaser, 3, "Head", "head");
            UpdateCustomPlayerSprite(sLeaser, 4, "Legs", "legs");
            UpdateCustomPlayerSprite(sLeaser, 5, "Arm", "arm");
            UpdateCustomPlayerSprite(sLeaser, 9, "Face", "face");

            //// Determine which sprites map to which indexes
            //Plugin.Logger.LogWarning("sLeaser Sprites");
            //foreach (var sprite in sLeaser.sprites)
            //{
            //    Plugin.Logger.LogWarning(sprite.element.name + " : " + sLeaser.sprites.IndexOf(sprite));
            //}

            //Plugin.Logger.LogWarning("Body Chunks");
            //foreach (var bodyChunk in self.player.bodyChunks)
            //{
            //    Plugin.Logger.LogWarning(bodyChunk.pos + " : " + self.player.bodyChunks.IndexOf(bodyChunk));
            //}

            //Plugin.Logger.LogWarning("Body Parts");
            //foreach (var bodyPart in self.bodyParts)
            //{
            //    Plugin.Logger.LogWarning(bodyPart.pos + " : " + self.bodyParts.IndexOf(bodyPart));
            //}
        }

        private static void UpdateCustomPlayerSprite(RoomCamera.SpriteLeaser sLeaser, int spriteIndex, string toReplace, string atlasName)
        {
            FAtlas? atlas = AssetLoader.GetAtlas(atlasName);

            if (atlas != null)
            {
                string? name = sLeaser.sprites[spriteIndex]?.element?.name;

                if (name != null && name.StartsWith(toReplace) && atlas._elementsByName.TryGetValue(Plugin.MOD_ID + name, out FAtlasElement element))
                {
                    sLeaser.sprites[spriteIndex].element = element;
                }
            }
        }

        private static Color Player_ShortCutColor(On.Player.orig_ShortCutColor orig, Player self)
        {
            PlayerEx playerEx;
            if (!PlayerData.TryGetValue(self, out playerEx)) return orig(self);

            List<Color> colors = playerEx.accentColors;

            if (colors.Count == 0) return orig(self);

            playerEx.shortcutColorStacker += FrameShortcutColorAddition * playerEx.shortcutColorStackerDirection;

            if (playerEx.shortcutColorStackerDirection == 1 && playerEx.shortcutColorStacker > 1.0f)
            {
                playerEx.shortcutColorStackerDirection = -1;
                playerEx.shortcutColorStacker += FrameShortcutColorAddition * playerEx.shortcutColorStackerDirection;

            }
            else if (playerEx.shortcutColorStackerDirection == -1 && playerEx.shortcutColorStacker < 0.0f)
            {
                playerEx.shortcutColorStackerDirection = 1;
                playerEx.shortcutColorStacker += FrameShortcutColorAddition * playerEx.shortcutColorStackerDirection;
            }

            // https://gamedev.stackexchange.com/questions/98740/how-to-color-lerp-between-multiple-colors
            float scaledTime = playerEx.shortcutColorStacker * (colors.Count - 1);
            Color oldColor = colors[(int)scaledTime];

            int nextIndex = (int)(scaledTime + 1.0f);
            Color newColor = nextIndex >= colors.Count ? oldColor : colors[nextIndex];

            float newTime = scaledTime - Mathf.Floor(scaledTime);
            return Color.Lerp(oldColor, newColor, newTime);
        }

        private static void Creature_SuckedIntoShortCut(On.Creature.orig_SuckedIntoShortCut orig, Creature self, IntVector2 entrancePos, bool carriedByOther)
        {
            if (self is Player player) DestroyRealizedActiveObject(player);

            orig(self, entrancePos, carriedByOther);
        }

        private static bool Creature_Grab(On.Creature.orig_Grab orig, Creature self, PhysicalObject obj, int graspUsed, int chunkGrabbed, Creature.Grasp.Shareability shareability, float dominance, bool overrideEquallyDominant, bool pacifying)
        {
            if (self is Player && IsRealizedActiveObject(obj.abstractPhysicalObject)) return false;

            return orig(self, obj, graspUsed, chunkGrabbed, shareability, dominance, overrideEquallyDominant, pacifying);
        }

        private static bool IsRealizedActiveObject(AbstractPhysicalObject targetObject)
        {
            List<PlayerEx> allPlayerData = GetAllPlayerData(targetObject.world.game);
            if (allPlayerData.Any(playerEx => playerEx.realizedActiveObject == targetObject)) return true;
            return false;
        }
    }
}
