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
    internal static partial class Hooks
    {
        private static void ApplyCreatureHooks()
        {
            On.Creature.Grab += Creature_Grab;
            On.Creature.SuckedIntoShortCut += Creature_SuckedIntoShortCut;
        }

        private static void Creature_SuckedIntoShortCut(On.Creature.orig_SuckedIntoShortCut orig, Creature self, IntVector2 entrancePos, bool carriedByOther)
        {
            orig(self, entrancePos, carriedByOther);

            if (self != player) return;

            DestroyActiveObject();
        }

        private static bool Creature_Grab(On.Creature.orig_Grab orig, Creature self, PhysicalObject obj, int graspUsed, int chunkGrabbed, Creature.Grasp.Shareability shareability, float dominance, bool overrideEquallyDominant, bool pacifying)
        {
            if (activeObject != null && obj == activeObject.realizedObject) return false;

            return orig(self, obj, graspUsed, chunkGrabbed, shareability, dominance, overrideEquallyDominant, pacifying);
        }
    }
}
