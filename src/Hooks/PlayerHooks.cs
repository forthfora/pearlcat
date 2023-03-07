using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using SlugBase.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using SlugBase.Features;

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
            On.Player.NewRoom += Player_NewRoom;
            On.Player.ShortCutColor += Player_ShortCutColor;

            try
            {
                IL.Player.GrabUpdate += Player_GrabUpdateIL;
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError(e);
            }
        }

        private static ConditionalWeakTable<Player, PlayerEx> PlayerData = new ConditionalWeakTable<Player, PlayerEx>();
        private static ConditionalWeakTable<RainWorldGame, List<AbstractPhysicalObject>> GameInventory = new ConditionalWeakTable<RainWorldGame, List<AbstractPhysicalObject>>();

        // Data container, handles values that must be unique to each player
        private class PlayerEx
        {
            public int? selectedIndex = null!;
            public bool canSwallowOrRegurgitate = true;
        }


        private const int MAX_STORAGE_COUNT = 10;
        
        // Base offset of the object relative to the player's head
        private static readonly Vector2 ACTIVE_OBJECT_BASE_OFFSET = new Vector2(0.0f, 10.0f);

        private const float ACTIVE_OBJECT_LERP_SPEED = 0.99f;


        private static bool IsCustomSlugcat(Player player) => player.SlugCatClass.ToString() == Plugin.SLUGCAT_ID;

        private static AbstractPhysicalObject? GetActiveObject(Player player)
        {
            List<AbstractPhysicalObject> inventory;
            if (!GameInventory.TryGetValue(player.room.game, out inventory)) return null;
           
            PlayerEx playerEx;
            if (!PlayerData.TryGetValue(player, out playerEx)) return null;

            if (playerEx.selectedIndex == null) return null;

            return inventory[(int)playerEx.selectedIndex];
        }

        private static void TryRealizeActiveObject(Player player)
        {
            if (player.inShortcut) return;

            AbstractPhysicalObject? activeObject = GetActiveObject(player);
            
            if (activeObject == null) return;

            WorldCoordinate newWorldCoordinate = player.room.ToWorldCoordinate(GetActiveObjectPos(player));
            activeObject.ChangeRooms(newWorldCoordinate);
            activeObject.pos = newWorldCoordinate;

            activeObject.RealizeInRoom();
            activeObject.realizedObject.CollideWithTerrain = false;
            activeObject.realizedObject.gravity = 0.0f;
        }

        private static void DestroyActiveObject(Player player)
        {
            AbstractPhysicalObject? activeObject = GetActiveObject(player);

            activeObject?.Destroy();
            activeObject?.realizedObject?.Destroy();
            activeObject = null!;
        }

        private static Vector2 GetActiveObjectPos(Player player)
        {
            Vector2 pos = player.graphicsModule.bodyParts[6].pos + ACTIVE_OBJECT_BASE_OFFSET;

            if (player.gravity == 0.0f) 
                pos = player.graphicsModule.bodyParts[6].pos + ACTIVE_OBJECT_BASE_OFFSET.magnitude * player.bodyChunks[0].Rotation;

            return pos;
        }

        // Attempts to add the supplied object to the inventory, returns true on success
        private static bool AddObjectToStorage(RainWorldGame game, AbstractPhysicalObject abstractObject)
        {
            List<AbstractPhysicalObject> inventory;
            if (!GameInventory.TryGetValue(game, out inventory)) return false;

            if (inventory.Count >= MAX_STORAGE_COUNT) return false;

            AbstractPhysicalObject objectToStore = CloneAbstractObject(game.world, abstractObject);
            inventory.Add(objectToStore);

            abstractObject.Destroy();
            abstractObject.realizedObject.Destroy();

            return true;
        }

        private static bool IsObjectStorable(AbstractPhysicalObject abstractObject)
        {
            if (abstractObject.type == AbstractPhysicalObject.AbstractObjectType.DataPearl) return true;

            if (abstractObject.type == AbstractPhysicalObject.AbstractObjectType.PebblesPearl) return true;

            if (abstractObject.type == AbstractPhysicalObject.AbstractObjectType.Rock) return true;


            // Downpour
            if (abstractObject.type == MoreSlugcats.MoreSlugcatsEnums.AbstractObjectType.HalcyonPearl) return true;

            if (abstractObject.type == MoreSlugcats.MoreSlugcatsEnums.AbstractObjectType.Spearmasterpearl) return true;

            return false;
        }

        private static AbstractPhysicalObject CloneAbstractObject(World world, AbstractPhysicalObject originalObject)
        {
            string copyString = originalObject.ToString();
            return SaveState.AbstractPhysicalObjectFromString(world, copyString);
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
            
        }

        private static void SelectPreviousObject(Player player)
        {

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

            playerEx.selectedIndex = objectIndex;
            DestroyActiveObject(player);
        }

        private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);

            if (!IsCustomSlugcat(self)) return;

            PlayerEx playerEx;
            if (!PlayerData.TryGetValue(self, out playerEx))
            {
                PlayerData.Add(self, playerEx = new PlayerEx());
            }

            TryRealizeActiveObject(self);
            AbstractPhysicalObject? activeObject = GetActiveObject(self);

            if (activeObject == null) return;

            Vector2 targetPos = GetActiveObjectPos(self);
            activeObject.realizedObject.firstChunk.pos = Vector2.Lerp(activeObject.realizedObject.firstChunk.pos, targetPos, ACTIVE_OBJECT_LERP_SPEED);
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

        private static void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, global::Player self, bool eu)
        {
            AbstractPhysicalObject? heldStorable = null;
            
            PlayerEx playerEx;
            if (!PlayerData.TryGetValue(self, out playerEx)) goto ORIG;

            List<AbstractPhysicalObject> inventory;
            if (!GameInventory.TryGetValue(self.room.game, out inventory)) goto ORIG;


            for (int i = 0; i < self.grasps.Length; i++)
            {
                if (inventory.Count >= MAX_STORAGE_COUNT) continue;
                    
                if (self.grasps[i] == null) continue;

                AbstractPhysicalObject heldObject = self.grasps[i].grabbed.abstractPhysicalObject;

                if (!IsObjectStorable(heldObject)) continue;

                heldStorable = heldObject;
                break;
            }
             
            playerEx.canSwallowOrRegurgitate = heldStorable == null;


            ORIG:
            orig(self, eu);


            if (playerEx == null) return;

            if (heldStorable == null) return;
            
            AddObjectToStorage(self.room.game, heldStorable);
        }

        private static void Player_NewRoom(On.Player.orig_NewRoom orig, global::Player self, Room newRoom)
        {
            orig(self, newRoom);

            if (!IsCustomSlugcat(self)) return;

            DestroyActiveObject(self);
        }

        private static float PlayerObjectLooker_HowInterestingIsThisObject(On.PlayerGraphics.PlayerObjectLooker.orig_HowInterestingIsThisObject orig, PlayerGraphics.PlayerObjectLooker self, PhysicalObject obj)
        {
            AbstractPhysicalObject? activeObject = GetActiveObject(self.owner.player);

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

        // lol
        private static uint colorStacker = 0;
        private const uint COLOR_STACKER_LIMIT = 400;

        private static Color Player_ShortCutColor(On.Player.orig_ShortCutColor orig, global::Player self)
        {
            colorStacker = colorStacker >= COLOR_STACKER_LIMIT ? 0 : colorStacker + 1;
            return Custom.HSL2RGB(colorStacker / (float)COLOR_STACKER_LIMIT, 1.0f, 0.5f);
        }
    }
}
