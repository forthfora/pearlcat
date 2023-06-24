using RWCustom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Pearlcat;

public static partial class Hooks
{
    public static void ApplyPlayerObjectDataHooks()
    {
        On.DataPearl.DrawSprites += DataPearl_DrawSprites;

        On.Creature.Grab += Creature_Grab;

        On.PhysicalObject.Update += PhysicalObject_Update;
        On.DataPearl.Update += DataPearl_Update;
    }


    public static readonly ConditionalWeakTable<PhysicalObject, PlayerObjectModule> PlayerObjectData = new();

    public static void MarkAsPlayerObject(this PhysicalObject physicalObject)
    {
        if (PlayerObjectData.TryGetValue(physicalObject, out _)) return;

        var playerObjectModule = new PlayerObjectModule()
        {
            gravity = physicalObject.gravity,

            collideWithObjects = physicalObject.CollideWithObjects,
            collideWithSlopes = physicalObject.CollideWithSlopes,
            collideWithTerrain = physicalObject.CollideWithTerrain,
        };

        if (physicalObject is DataPearl pearl)
            playerObjectModule.pearlGlimmerWait = pearl.glimmerWait;

        if (physicalObject is Weapon weapon)
            playerObjectModule.weaponRotationSpeed = weapon.rotationSpeed;


        PlayerObjectData.Add(physicalObject, playerObjectModule);
    }

    public static void ClearAsPlayerObject(this PhysicalObject physicalObject)
    {
        if (!PlayerObjectData.TryGetValue(physicalObject, out var playerObjectModule)) return;

        physicalObject.gravity = playerObjectModule.gravity;

        physicalObject.CollideWithObjects = playerObjectModule.collideWithObjects;
        physicalObject.CollideWithSlopes = playerObjectModule.collideWithSlopes;
        physicalObject.CollideWithTerrain = playerObjectModule.collideWithTerrain;

        if (physicalObject is DataPearl pearl)
            pearl.glimmerWait = playerObjectModule.pearlGlimmerWait;

        if (physicalObject is Weapon weapon)
            weapon.rotationSpeed = playerObjectModule.weaponRotationSpeed;


        PlayerObjectData.Remove(physicalObject);
    }


    // TODO: Fix scavs 'picking up' the pearls which breaks which room they're in
    public static bool Creature_Grab(On.Creature.orig_Grab orig, Creature self, PhysicalObject obj, int graspUsed, int chunkGrabbed, Creature.Grasp.Shareability shareability, float dominance, bool overrideEquallyDominant, bool pacifying)
    {
        if (obj.abstractPhysicalObject.IsPlayerObject())
            return false;

        return orig(self, obj, graspUsed, chunkGrabbed, shareability, dominance, overrideEquallyDominant, pacifying);
    }

    public static void PhysicalObject_Update(On.PhysicalObject.orig_Update orig, PhysicalObject self, bool eu)
    {        
        orig(self, eu);

        if (!PlayerObjectData.TryGetValue(self, out _)) return;

        self.gravity = 0.0f;

        self.CollideWithObjects = false;
        self.CollideWithSlopes = false;
        self.CollideWithTerrain = false;

        if (self is Weapon weapon)
            weapon.rotationSpeed = 0.0f;
    }

    public static void DataPearl_Update(On.DataPearl.orig_Update orig, DataPearl self, bool eu)
    {
        orig(self, eu);

        if (!PlayerObjectData.TryGetValue(self, out _)) return;

        self.CollideWithObjects = false;
        self.CollideWithSlopes = false;
        self.CollideWithTerrain = false;

        self.glimmerWait = 40;
    }



    public static void DataPearl_DrawSprites(On.DataPearl.orig_DrawSprites orig, DataPearl self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        IDrawable_DrawSprites(self, sLeaser, rCam, timeStacker, camPos);

        // TODO: SWAP CONTAINERS
        //if (!PlayerObjectData.TryGetValue(self, out var _))
        //{
        //    self.AddToContainer(sLeaser, rCam, null);
        //}
        //else
        //{
        //    self.AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Background"));
        //}
    }

    public static void IDrawable_DrawSprites(PhysicalObject self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (!ObjectAddon.ObjectsWithAddon.TryGetValue(self, out var addon)) return;

        addon.ParentGraphics_DrawSprites(self, sLeaser, rCam, timeStacker, camPos);
    }



    public static bool IsStorable(this AbstractPhysicalObject abstractObject)
    {
        if (abstractObject.type == AbstractPhysicalObject.AbstractObjectType.DataPearl) return true;

        if (abstractObject.type == AbstractPhysicalObject.AbstractObjectType.PebblesPearl) return true;
        
        if (abstractObject.type == MoreSlugcats.MoreSlugcatsEnums.AbstractObjectType.HalcyonPearl) return true;
        
        if (abstractObject.type == MoreSlugcats.MoreSlugcatsEnums.AbstractObjectType.Spearmasterpearl) return true;

        return false;
    }

    public static Color GetObjectColor(this AbstractPhysicalObject abstractObject)
    {
        var symbolData = ItemSymbol.SymbolDataFromItem(abstractObject);

        if (symbolData == null)
            return Color.white;

        return ItemSymbol.ColorForItem(abstractObject.type, symbolData.Value.intData);
    }

    public static void MoveToTargetPos(this AbstractPhysicalObject abstractObject, Player player, Vector2 targetPos)
    {
        if (!player.TryGetPearlcatModule(out var playerModule)) return;

        if (abstractObject.realizedObject == null) return;

        if (!MinFricSpeed.TryGet(player, out var minFricSpeed)) return;
        if (!MaxFricSpeed.TryGet(player, out var maxFricSpeed)) return;
        if (!MinFric.TryGet(player, out var minFric)) return;
        if (!MaxFric.TryGet(player, out var maxFric)) return;

        if (!CutoffDist.TryGet(player, out var cutoffDist)) return;
        if (!CutoffMinSpeed.TryGet(player, out var cutoffMinSpeed)) return;
        if (!CutoffMaxSpeed.TryGet(player, out var cutoffMaxSpeed)) return;
        if (!DazeMaxSpeed.TryGet(player, out var dazeMaxSpeed)) return;

        if (!MaxDist.TryGet(player, out var maxDist)) return;
        if (!MinSpeed.TryGet(player, out var minSpeed)) return;
        if (!MaxSpeed.TryGet(player, out var maxSpeed)) return;

        var firstChunk = abstractObject.realizedObject.firstChunk;
        var dir = (targetPos - firstChunk.pos).normalized;
        var dist = Custom.Dist(firstChunk.pos, targetPos);

        float speed = dist < cutoffDist ? Custom.LerpMap(dist, 0.0f, cutoffDist, cutoffMinSpeed, playerModule.IsDazed ? dazeMaxSpeed : cutoffMaxSpeed) : Custom.LerpMap(dist, cutoffDist, maxDist, minSpeed, maxSpeed);

        firstChunk.vel *= Custom.LerpMap(firstChunk.vel.magnitude, minFricSpeed, maxFricSpeed, minFric, maxFric);
        firstChunk.vel += dir * speed;
    }
}
