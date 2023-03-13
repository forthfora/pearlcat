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

            if (playerEx.activeObjectGlow != null)
            {
                playerEx.activeObjectGlow.stayAlive = true;
                playerEx.activeObjectGlow.setPos = GetActiveObjectPos(self);
                playerEx.activeObjectGlow.setRad = 75.0f;
                playerEx.activeObjectGlow.setAlpha = 0.3f;

                if (playerEx.accentColors.Count > 0) playerEx.activeObjectGlow.color = playerEx.accentColors[0];

                if (playerEx.activeObjectGlow.slatedForDeletetion) playerEx.activeObjectGlow = null;
            }
            else
            {
                playerEx.activeObjectGlow = new LightSource(GetActiveObjectPos(self), false, Color.white, self);
                playerEx.activeObjectGlow.requireUpKeep = true;
                self.room.AddObject(playerEx.activeObjectGlow);
            }
        }


        private static void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
        {
            AbstractPhysicalObject? heldStorable = null;

            List<AbstractPhysicalObject> inventory;
            if (!GameInventory.TryGetValue(self.room.game, out inventory)) GameInventory.Add(self.room.game, new List<AbstractPhysicalObject>());

            PlayerEx? playerEx;
            if (!PlayerData.TryGetValue(self, out playerEx)) goto ORIG;

            playerEx.canSwallowOrRegurgitate = true;

            // 
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
                playerEx.canTransferObject = true;
                return;
            }

            if (!playerEx.canTransferObject) return;

            if (self.FreeHand() == -1) return;

            if (heldStorable == null)
            {
                playerEx.transferObject = GetStoredActiveObject(self);
                return;
            }

            // Store keybind held
            playerEx.canSwallowOrRegurgitate = false;
            playerEx.transferObject = heldStorable;
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
