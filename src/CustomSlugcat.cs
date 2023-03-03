using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TheSacrifice
{
    internal class CustomSlugcat
    {
        public readonly Player player;

        private const int MAX_PEARL_STORAGE_COUNT = 10;

        // These are abstract definitions of pearls, meant to store their data only
        private List<DataPearl.AbstractDataPearl> storedPearls = new List<DataPearl.AbstractDataPearl>();
        private DataPearl.AbstractDataPearl? selectedPearl = null!;

        // Pearls that are actually realized in the world
        private DataPearl.AbstractDataPearl? activePearl = null!;
        private DataPearl.AbstractDataPearl? heldPearl = null!;

        private bool canSwallowOrRegurgitate = true;

        public CustomSlugcat(Player player)
        {
            this.player = player;

            ApplyHooks();
        }

        private void ApplyHooks()
        {
            On.Player.Update += Player_Update;
            
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
            On.PlayerGraphics.ctor += PlayerGraphics_ctor;
            On.PlayerGraphics.PlayerObjectLooker.HowInterestingIsThisObject += PlayerObjectLooker_HowInterestingIsThisObject;

            On.Player.GrabUpdate += Player_GrabUpdate;
            On.Creature.Grab += Creature_Grab;

            On.Creature.SuckedIntoShortCut += Creature_SuckedIntoShortCut;
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

        private void Player_NewRoom(On.Player.orig_NewRoom orig, Player self, Room newRoom)
        {
            orig(self, newRoom);

            if (self != player) return;

            DestroyActivePearl();
        }

        private float PlayerObjectLooker_HowInterestingIsThisObject(On.PlayerGraphics.PlayerObjectLooker.orig_HowInterestingIsThisObject orig, PlayerGraphics.PlayerObjectLooker self, PhysicalObject obj)
        {
            if (obj != null && obj.abstractPhysicalObject == activePearl) return 0.0f;

            return orig(self, obj);
        }


        // lol
        private uint colorStacker = 0;
        private const uint COLOR_STACKER_LIMIT = 400;

        private Color Player_ShortCutColor(On.Player.orig_ShortCutColor orig, Player self)
        {
            colorStacker = colorStacker >= COLOR_STACKER_LIMIT ? 0 : colorStacker + 1;
            return Custom.HSL2RGB(colorStacker / (float)COLOR_STACKER_LIMIT, 1.0f, 0.5f);
        }

        private void Creature_SuckedIntoShortCut(On.Creature.orig_SuckedIntoShortCut orig, Creature self, IntVector2 entrancePos, bool carriedByOther)
        {
            orig(self, entrancePos, carriedByOther);

            if (self != player) return;

            DestroyActivePearl();
        }

        private void DestroyActivePearl()
        {
            activePearl?.Destroy();
            activePearl?.realizedObject.Destroy();
            activePearl = null!;
        }

        private bool Creature_Grab(On.Creature.orig_Grab orig, Creature self, PhysicalObject obj, int graspUsed, int chunkGrabbed, Creature.Grasp.Shareability shareability, float dominance, bool overrideEquallyDominant, bool pacifying)
        {
            if (activePearl != null && obj == activePearl.realizedObject) return false;

            return orig(self, obj, graspUsed, chunkGrabbed, shareability, dominance, overrideEquallyDominant, pacifying);
        }

        private Vector2 GetActivePearlPos()
        {
            Vector2 pos = player.graphicsModule.bodyParts[6].pos + activePearlOffset;

            if (player.gravity == 0.0f)
            {
                pos = player.graphicsModule.bodyParts[6].pos + activePearlOffset.magnitude * player.bodyChunks[0].Rotation;
            }

            return pos;
        }

        private const float PEARL_SPEED = 0.99f;
        private readonly Vector2 activePearlOffset = new Vector2(0.0f, 10.0f);

        private void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);

            if (player != self) return;

            RealizeActivePearl();

            if (activePearl == null) return;

            Vector2 targetPos = GetActivePearlPos();
            activePearl.realizedObject.firstChunk.pos = Vector2.Lerp(activePearl.realizedObject.firstChunk.pos, targetPos, PEARL_SPEED);
        }

        private void RealizeActivePearl()
        {
            if (activePearl != null || storedPearls.Count == 0) return;

            if (player.inShortcut) return;

            if (selectedPearl == null) return;

            activePearl = CloneAbstractDataPearl(selectedPearl);

            WorldCoordinate newWorldCoordinate = player.room.ToWorldCoordinate(GetActivePearlPos());
            activePearl.pos = newWorldCoordinate;

            activePearl.RealizeInRoom();
            activePearl.realizedObject.CollideWithTerrain = false;
            activePearl.realizedObject.gravity = 0.0f;
            
            //Plugin.Logger.LogWarning($"Active pearl realized at position ({activePearl.pos})!");
        }

        private void Player_GrabUpdateIL(ILContext il)
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
            c.EmitDelegate<Func<Player, bool>>((player) => canSwallowOrRegurgitate);

            c.Emit(OpCodes.Brfalse, dest);
        }

        private void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
        {
            if (player == self)
            {
                canSwallowOrRegurgitate = true;
                heldPearl = null;

                // Check if holding a pearl and if so get the pearl
                // The right hand takes priority
                for (int i = 0; i < self.grasps.Length; i++)
                {
                    if (self.grasps[i] == null) continue;

                    AbstractPhysicalObject heldObject = self.grasps[i].grabbed.abstractPhysicalObject;

                    // Several different types of pearls
                    if (heldObject.type != AbstractPhysicalObject.AbstractObjectType.DataPearl
                        && heldObject.type != AbstractPhysicalObject.AbstractObjectType.PebblesPearl
                        && heldObject.type != MoreSlugcats.MoreSlugcatsEnums.AbstractObjectType.HalcyonPearl) 
                        continue;

                    heldPearl = (DataPearl.AbstractDataPearl)self.grasps[i].grabbed.abstractPhysicalObject;
                    break;
                }

                if (heldPearl != null)
                {
                    if (storedPearls.Count < MAX_PEARL_STORAGE_COUNT)
                    {
                        canSwallowOrRegurgitate = false;
                    }
                }
            }
            
            orig(self, eu);

            if (player != self) return;

            if (heldPearl == null) return;
            
            AddPearlToStorage(heldPearl);
        }

        private void AddPearlToStorage(DataPearl.AbstractDataPearl pearl)
        {
            if (storedPearls.Count >= MAX_PEARL_STORAGE_COUNT) return;

            DataPearl.AbstractDataPearl pearlToStore = CloneAbstractDataPearl(pearl);
            storedPearls.Add(pearlToStore);

            pearl.Destroy();
            pearl.realizedObject.Destroy();

            selectedPearl = pearlToStore;
        }

        private void PlayerGraphics_ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
        {
            orig(self, ow);

            if (player != self.player) return;

            //self.tail[0] = new TailSegment(self, 8f, 2f, null, 0.85f, 1f, 1f, true);
            //self.tail[1] = new TailSegment(self, 6f, 3.5f, self.tail[0], 0.85f, 1f, 0.5f, true);
            //self.tail[2] = new TailSegment(self, 4f, 3.5f, self.tail[1], 0.85f, 1f, 0.5f, true);
            //self.tail[3] = new TailSegment(self, 2f, 3.5f, self.tail[2], 0.85f, 1f, 0.5f, true);
        }

        private DataPearl.AbstractDataPearl CloneAbstractDataPearl(DataPearl.AbstractDataPearl originalPearl) => new DataPearl.AbstractDataPearl(originalPearl.world, originalPearl.type, null, originalPearl.pos, originalPearl.ID, -1, -1, null, originalPearl.dataPearlType);

        private void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            if (player != self.player) return;

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

        private void UpdateCustomPlayerSprite(RoomCamera.SpriteLeaser sLeaser, int spriteIndex, string toReplace, string atlasName)
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
    }
}
