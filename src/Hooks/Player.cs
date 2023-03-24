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
        private static void ApplyPlayerHooks()
        {
            On.Player.ctor += Player_ctor;
            On.Player.Update += Player_Update;

            On.Player.GrabUpdate += Player_GrabUpdate;
            On.Player.NewRoom += Player_NewRoom;
            On.Player.Grabability += Player_Grabability;

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

        private static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);

            if (!IsCustomSlugcat(self)) return;

            PlayerData.Add(self, new PlayerModule(self));
        }

        private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);

            if (!IsCustomSlugcat(self)) return;

            if (!PlayerData.TryGetValue(self, out PlayerModule playerModule)) return;


            TryRealizeActiveObject(self);
            AbstractPhysicalObject? activeObject = GetRealizedActiveObject(self);

            if (activeObject == null) return;


            Vector2 targetPos = GetActiveObjectPos(self);
            activeObject.realizedObject.firstChunk.pos = targetPos;


            // Object glows when active
            if (playerModule.activeObjectGlow != null)
            {
                playerModule.activeObjectGlow.stayAlive = true;
                playerModule.activeObjectGlow.setPos = GetActiveObjectPos(self);
                playerModule.activeObjectGlow.setRad = 75.0f;
                playerModule.activeObjectGlow.setAlpha = 0.3f;

                if (playerModule.DynamicColors.Count > 0) playerModule.activeObjectGlow.color = playerModule.DynamicColors[0];

                if (playerModule.activeObjectGlow.slatedForDeletetion) playerModule.activeObjectGlow = null;
            }
            else
            {
                playerModule.activeObjectGlow = new LightSource(GetActiveObjectPos(self), false, Color.white, self);
                playerModule.activeObjectGlow.requireUpKeep = true;
                self.room.AddObject(playerModule.activeObjectGlow);
            }
        }


        private static void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
        {
            orig(self, eu);

            StoreObjectUpdate(self);

            TransferObjectUpdate(self);
        }

        private static void StoreObjectUpdate(Player self)
        {
            if (!PlayerData.TryGetValue(self, out var playerModule)) return;

            // Gather held storables
            AbstractPhysicalObject? heldStorable = null;

            for (int i = 0; i < self.grasps.Length; i++)
            {
                if (self.grasps[i] == null) continue;

                AbstractPhysicalObject heldObject = self.grasps[i].grabbed.abstractPhysicalObject;

                if (!IsObjectStorable(heldObject)) continue;

                heldStorable = heldObject;
                break;
            }


            if (!IsStoreKeybindPressed(self))
            {
                playerModule.transferObject = null;
                playerModule.canTransferObject = true;

                // Reenable normal actions
                playerModule.canSwallowOrRegurgitate = true;
                self.spearOnBack.interactionLocked = false;
                self.slugOnBack.interactionLocked = false;
                return;
            }

            // Prevent transfer immediately after one has taken place 
            if (!playerModule.canTransferObject) return;


            // Held items take priority
            playerModule.transferObject = heldStorable ?? GetRealizedActiveObject(self);

            // Disable normal actions
            playerModule.canSwallowOrRegurgitate = false;
            self.slugOnBack.interactionLocked = true;
            self.spearOnBack.interactionLocked = true;

            self.input[0].x = 0;
            self.input[0].y = 0;
        }

        private static void TransferObjectUpdate(Player self)
        {
            if (!PlayerData.TryGetValue(self, out PlayerModule? playerModule)) return;

            playerModule.storingObjectSound.Update();
            playerModule.retrievingObjectSound.Update();

            if (playerModule.transferObject == null)
            {
                ResetTransferObject(playerModule);
                return;
            }

            PlayerGraphics playerGraphics = (PlayerGraphics)self.graphicsModule;

            playerModule.transferObjectInitialPos ??= playerModule.transferObject.realizedObject.firstChunk.pos;

            playerModule.transferStacker++;
            bool puttingInStorage = playerModule.transferObject != GetStoredActiveObject(self);

            if (puttingInStorage)
            {
                int? targetHand = null;

                if (self.grasps.Length == 0) return;

                for (int i = 0; i < self.grasps.Length; i++)
                {
                    PhysicalObject graspedObject = self.grasps[i].grabbed;

                    if (graspedObject == playerModule.transferObject.realizedObject)
                    {
                        targetHand = i;
                        break;
                    }
                }

                if (targetHand == null) return;

                //playerModule.storingObjectSound.Volume = 1.0f;

                // Pearl to head
                playerModule.transferObject.realizedObject.firstChunk.pos = Vector2.Lerp(playerModule.transferObject.realizedObject.firstChunk.pos, GetActiveObjectPos(self), (float)playerModule.transferStacker / FramesToStoreObject);
                playerGraphics.hands[(int)targetHand].absoluteHuntPos = playerModule.transferObject.realizedObject.firstChunk.pos;
                playerGraphics.hands[(int)targetHand].reachingForObject = true;

                if (playerModule.transferStacker < FramesToStoreObject) return;

                //playerModule.storingObjectSound.Volume = 0.0f;
                self.room.PlaySound(Enums.Sounds.ObjectStored, self.firstChunk);

                StoreObject(self, playerModule.transferObject);
                DestroyRealizedActiveObject(self);
                DestroyTransferObject(playerModule);

                ActivateObjectInStorage(self, playerModule.abstractInventory.Count - 1);
                return;
            }

            // Hand to head

            //playerModule.retrievingObjectSound.Volume = 1.0f;

            playerGraphics.hands[self.FreeHand()].absoluteHuntPos = GetActiveObjectPos(self);
            playerGraphics.hands[self.FreeHand()].reachingForObject = true;

            if (playerModule.transferStacker < FramesToRetrieveObject) return;

            //playerModule.retrievingObjectSound.Volume = 0.0f;
            self.room.PlaySound(Enums.Sounds.ObjectRetrieved, self.firstChunk);

            RetrieveObject(self);
            DestroyTransferObject(playerModule);
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
                if (!PlayerData.TryGetValue(player, out PlayerModule playerModule)) return true;

                return playerModule.canSwallowOrRegurgitate;
            });

            c.Emit(OpCodes.Brfalse, dest);
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
    }
}
