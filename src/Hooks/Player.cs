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

                if (playerModule.accentColors.Count > 0) playerModule.activeObjectGlow.color = playerModule.accentColors[0];

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
