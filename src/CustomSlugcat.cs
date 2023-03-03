using Mono.Cecil.Cil;
using MonoMod.Cil;
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

        private List<DataPearl.AbstractDataPearl> storedPearls = new List<DataPearl.AbstractDataPearl>();

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
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
            On.PlayerGraphics.ctor += PlayerGraphics_ctor;
            On.Player.GrabUpdate += Player_GrabUpdate;
        
            try
            {
                IL.Player.GrabUpdate += Player_GrabUpdateIL;
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError(e);
            }
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

            DataPearl.AbstractDataPearl pearlToStore = new DataPearl.AbstractDataPearl(pearl.world, pearl.type, null, pearl.pos, pearl.ID, -1, -1, null, pearl.dataPearlType);
            storedPearls.Add(pearlToStore);

            pearl.Destroy();
            pearl.realizedObject.Destroy();
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
            //foreach (var sprite in sLeaser.sprites)
            //{
            //    Plugin.Logger.LogWarning(sprite.element.name + " : " + sLeaser.sprites.IndexOf(sprite));
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
