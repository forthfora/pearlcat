using MonoMod.RuntimeDetour;
using RWCustom;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Pearlcat;

public static partial class Hooks
{
    public static void ApplySSOraclePearlsHooks()
    {
        On.PebblesPearl.Update += PebblesPearl_Update;
        On.SSOracleBehavior.Update += SSOracleBehavior_Update;

        new Hook(
            typeof(PebblesPearl).GetProperty(nameof(PebblesPearl.NotCarried), BindingFlags.Instance | BindingFlags.Public).GetGetMethod(),
            typeof(Hooks).GetMethod(nameof(GetPebblesPearlNotCarried), BindingFlags.Static | BindingFlags.Public)
        );
    }


    private static void SSOracleBehavior_Update(On.SSOracleBehavior.orig_Update orig, SSOracleBehavior self, bool eu)
    {
        orig(self, eu);

        if (!self.oracle.room.game.IsPearlcatStory() || !self.IsPebbles()) return;

        if (self.timeSinceSeenPlayer == -1) return;

        if (self.getToWorking == 0.0f) return;

        var module = self.GetModule();

        var orbitSmall = new List<DataPearl>();
        var orbitMid = new List<DataPearl>();
        var orbitLarge = new List<DataPearl>();

        foreach (var updatable in self.oracle.room.updateList)
        {
            if (updatable is not DataPearl dataPearl) continue;

            if (dataPearl.grabbedBy.Count > 0) continue;

            if (dataPearl.abstractPhysicalObject.IsPlayerObject()) continue;

            if (dataPearl == module.PearlToRead || dataPearl == module.PearlBeingRead || dataPearl == module.PearlToReturn) continue;

            if (self.player != null && Custom.DistLess(self.player.firstChunk.pos, self.oracle.firstChunk.pos, 180.0f))
            {
                orbitLarge.Add(dataPearl);
                continue;
            }

            if (orbitSmall.Count < 5)
            {
                orbitSmall.Add(dataPearl);
            }
            else if (orbitMid.Count < 10)
            {
                orbitMid.Add(dataPearl);
            }
            else
            {
                orbitLarge.Add(dataPearl);
            }
        }

        var origin = new Vector2(490.0f, 340.0f);

        AnimatePebblesPearlOrbit(origin, 80.0f, 0.0002f, orbitSmall);
        AnimatePebblesPearlOrbit(origin, 180.0f, -0.00015f, orbitMid);
        AnimatePebblesPearlOrbit(origin, 265.0f, 0.0001f, orbitLarge);
    }

    private static void PebblesPearl_Update(On.PebblesPearl.orig_Update orig, PebblesPearl self, bool eu)
    {
        orig(self, eu);

        if (self.abstractPhysicalObject.IsPlayerObject())
        {
            self.abstractPhysicalObject.slatedForDeletion = false;
            self.label?.Destroy();
            return;
        }
    }


    public static int PearlAnimCounter { get; set; }

    public static void AnimatePebblesPearlOrbit(Vector2 origin, float radius, float angleFrameAddition, List<DataPearl> pearls)
    {
        for (int i = 0; i < pearls.Count; i++)
        {
            var dataPearl = pearls[i];

            var angle = i * (Mathf.PI * 2.0f / pearls.Count) + angleFrameAddition * PearlAnimCounter++;
            var targetPos = new Vector2(origin.x + Mathf.Cos(angle) * radius, origin.y + Mathf.Sin(angle) * radius);

            var oraclePearlDir = Custom.DirVec(dataPearl.firstChunk.pos, targetPos);
            var oraclePearlDist = Custom.Dist(targetPos, dataPearl.firstChunk.pos);

            dataPearl.firstChunk.vel = oraclePearlDir * Custom.LerpMap(oraclePearlDist, 200.0f, 10.0f, 15.0f, 1.0f);

            if (Custom.DistLess(dataPearl.firstChunk.pos, targetPos, 5.0f))
            {
                dataPearl.firstChunk.HardSetPosition(targetPos);
            }
        }
    }


    public delegate bool orig_PebblesPearlNotCarried(PebblesPearl self);

    public static bool GetPebblesPearlNotCarried(orig_PebblesPearlNotCarried orig, PebblesPearl self)
    {
        var result = orig(self);

        if (self.room.game.IsPearlcatStory())
        {
            if (self.oracle?.oracleBehavior is SSOracleBehavior behavior && behavior.timeSinceSeenPlayer >= 0)
            {
                return false;
            }
        }

        return result;
    }
}
