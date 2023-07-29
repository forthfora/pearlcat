using RWCustom;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Color = UnityEngine.Color;

namespace Pearlcat;

public static partial class Hooks
{
    public static void ApplyPlayerObjectDataHooks()
    {
        On.DataPearl.DrawSprites += DataPearl_DrawSprites;

        On.Creature.Grab += Creature_Grab;

        On.PhysicalObject.Grabbed += PhysicalObject_Grabbed;
        On.ScavengerAI.CollectScore_PhysicalObject_bool += ScavengerAI_CollectScore_PhysicalObject_bool;

        On.PhysicalObject.Update += PhysicalObject_Update;
        On.DataPearl.Update += DataPearl_Update;

        On.AbstractPhysicalObject.Update += AbstractPhysicalObject_Update;
        On.AbstractPhysicalObject.UsesAPersistantTracker += AbstractPhysicalObject_UsesAPersistantTracker;
    }

    private static bool AbstractPhysicalObject_UsesAPersistantTracker(On.AbstractPhysicalObject.orig_UsesAPersistantTracker orig, AbstractPhysicalObject abs)
    {
        var result = orig(abs);

        if (abs.IsPlayerObject())
            return false;

        return result;
    }
    
    private static void AbstractPhysicalObject_Update(On.AbstractPhysicalObject.orig_Update orig, AbstractPhysicalObject self, int time)
    {
        orig(self, time);

        if (self.IsPlayerObject() && self.Room.world.game.GetStorySession is StoryGameSession session)
            session.RemovePersistentTracker(self);
    }


    public static ConditionalWeakTable<AbstractPhysicalObject, PlayerObjectModule> PlayerObjectData { get; } = new();

    public static bool TryGetModule(this AbstractPhysicalObject abstractObject, out PlayerObjectModule module)
    {
        if (PlayerObjectData.TryGetValue(abstractObject, out module))
            return true;

        module = null!;
        return false;
    }

    public static bool TryGetAddon(this AbstractPhysicalObject abstractObject, out ObjectAddon addon)
    {
        if (ObjectAddon.ObjectsWithAddon.TryGetValue(abstractObject, out addon))
            return true;

        addon = null!;
        return false;
    }


    public static void MarkAsPlayerObject(this AbstractPhysicalObject abstractObject)
    {
        var module = PlayerObjectData.GetValue(abstractObject, x => new PlayerObjectModule());

        if (module.IsCurrentlyStored) return;

        var physicalObject = abstractObject.realizedObject;
        if (abstractObject.realizedObject == null) return;

        module.IsCurrentlyStored = true;
        module.Gravity = physicalObject.gravity;

        module.CollideWithObjects = physicalObject.CollideWithObjects;
        module.CollideWithSlopes = physicalObject.CollideWithSlopes;
        module.CollideWithTerrain = physicalObject.CollideWithTerrain;

        if (physicalObject is DataPearl pearl)
            module.PearlGlimmerWait = pearl.glimmerWait;

        if (physicalObject is Weapon weapon)
            module.WeaponRotationSpeed = weapon.rotationSpeed;
    }

    public static void ClearAsPlayerObject(this AbstractPhysicalObject abstractObject)
    {
        if (!abstractObject.TryGetModule(out var module)) return;

        if (!module.IsCurrentlyStored) return;

        var physicalObject = abstractObject.realizedObject;
        if (physicalObject == null) return;

        module.IsCurrentlyStored = false;
        physicalObject.gravity = module.Gravity;

        physicalObject.CollideWithObjects = module.CollideWithObjects;
        physicalObject.CollideWithSlopes = module.CollideWithSlopes;
        physicalObject.CollideWithTerrain = module.CollideWithTerrain;

        if (physicalObject is DataPearl pearl)
            pearl.glimmerWait = module.PearlGlimmerWait;

        if (physicalObject is Weapon weapon)
            weapon.rotationSpeed = module.WeaponRotationSpeed;
    }


    private static bool Creature_Grab(On.Creature.orig_Grab orig, Creature self, PhysicalObject obj, int graspUsed, int chunkGrabbed, Creature.Grasp.Shareability shareability, float dominance, bool overrideEquallyDominant, bool pacifying)
    {
        var result = orig(self, obj, graspUsed, chunkGrabbed, shareability, dominance, overrideEquallyDominant, pacifying);

        if (obj.abstractPhysicalObject.IsPlayerObject())
            return false;
        
        if (obj is Player player && player.TryGetPearlcatModule(out var playerModule) && playerModule.ShieldActive && player.IsHostileToMe(self))
        {
            if (!(self is Centipede && playerModule.ShieldTimer > 0))
                DeflectEffect(self.room, self.DangerPos);

            self.Stun(10);
            playerModule.ActivateVisualShield();
            return false;
        }

        return result;
    }

    // extra grab prevention safety
    private static void PhysicalObject_Grabbed(On.PhysicalObject.orig_Grabbed orig, PhysicalObject self, Creature.Grasp grasp)
    {
        orig(self, grasp);

        if (self.abstractPhysicalObject.IsPlayerObject())
            grasp.Release();
    }

    private static int ScavengerAI_CollectScore_PhysicalObject_bool(On.ScavengerAI.orig_CollectScore_PhysicalObject_bool orig, ScavengerAI self, PhysicalObject obj, bool weaponFiltered)
    {
        var result = orig(self, obj, weaponFiltered);

        // weird nullref here
        if (obj?.abstractPhysicalObject != null && obj.abstractPhysicalObject.IsPlayerObject())
            return 0;

        return result;
    }


    private static void PhysicalObject_Update(On.PhysicalObject.orig_Update orig, PhysicalObject self, bool eu)
    {        
        orig(self, eu);

        if (!self.abstractPhysicalObject.TryGetModule(out var module)) return;


        if (module.CooldownTimer > 0)
        {
            module.CooldownTimer--;

            var effect = self.abstractPhysicalObject.GetPOEffect();

            if (module.CooldownTimer == 0 && effect.MajorEffect == POEffect.MajorEffectType.SHIELD)
            {
                var playerModule = self.room.game.GetAllPlayerData().FirstOrDefault(x => x.Inventory.Contains(self.abstractPhysicalObject));
                
                if (ModOptions.InventoryPings.Value)
                    playerModule.ShowHUD(80);
                
                module.InventoryFlash = true;

                self.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, self.firstChunk, false, 1.0f, 3.0f);
            }
        }

        if (!module.IsCurrentlyStored) return;

        self.gravity = 0.0f;

        self.CollideWithObjects = false;
        self.CollideWithSlopes = false;
        self.CollideWithTerrain = false;

        if (self is Weapon weapon)
            weapon.rotationSpeed = 0.0f;
    }

    private static void DataPearl_Update(On.DataPearl.orig_Update orig, DataPearl self, bool eu)
    {
        orig(self, eu);

        if (!self.abstractPhysicalObject.TryGetModule(out var module) || !module.IsCurrentlyStored) return;

        self.CollideWithObjects = false;
        self.CollideWithSlopes = false;
        self.CollideWithTerrain = false;

        self.glimmerWait = 40;
    }

    private static void DataPearl_DrawSprites(On.DataPearl.orig_DrawSprites orig, DataPearl self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        IDrawable_DrawSprites(self, sLeaser, rCam, timeStacker, camPos);

        // SWAP CONTAINERS?
        //if (!PlayerObjectData.TryGetValue(self, out var _))
        //{
        //    self.AddToContainer(sLeaser, rCam, null);
        //}
        //else
        //{
        //    self.AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Background"));
        //}
    }

    private static void IDrawable_DrawSprites(PhysicalObject self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (!self.abstractPhysicalObject.TryGetAddon(out var addon)) return;

        addon.ParentGraphics_DrawSprites(self, sLeaser, rCam, timeStacker, camPos);
    }


    public static bool IsPlayerObject(this AbstractPhysicalObject targetObject)
    {
        var playerData = GetAllPlayerData(targetObject.world.game);

        foreach (var playerModule in playerData)
            if (playerModule.Inventory.Any(abstractObject => abstractObject == targetObject))
                return true;

        return false;
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
        if (abstractObject is DataPearl.AbstractDataPearl dataPearl)
        {
            if (dataPearl is PebblesPearl.AbstractPebblesPearl pebblesPearl)
                return GetDataPearlColor(dataPearl.dataPearlType, pebblesPearl.color);

            return GetDataPearlColor(dataPearl.dataPearlType);
        }

        var symbolData = ItemSymbol.SymbolDataFromItem(abstractObject);

        if (symbolData == null)
            return Color.white;

        return ItemSymbol.ColorForItem(abstractObject.type, symbolData.Value.intData);
    }

    public static Color GetDataPearlColor(this DataPearl.AbstractDataPearl.DataPearlType type, int pebblesPearlColor = 0)
    {
        if (type == DataPearl.AbstractDataPearl.DataPearlType.PebblesPearl)
        {
            switch (Mathf.Abs(pebblesPearlColor))
            {
                case 1:
                    return new(0.7f, 0.7f, 0.7f);

                case 2:
                    if (pebblesPearlColor < 0)
                        return new(1f, 122f / 255f, 2f / 255f);

                    return new(0.01f, 0.01f, 0.01f);

                default:
                    if (pebblesPearlColor < 0)
                        return new(0f, 116f / 255f, 163f / 255f);

                    return new(1f, 122f / 255f, 2f / 255f);
            }
        }

        return DataPearl.UniquePearlMainColor(type);
    }


    public static Vector2 GetActiveObjectPos(this Player self, Vector2? overrideOffset = null)
    {
        if (!ActiveObjectOffset.TryGet(self, out var activeObjectOffset))
            activeObjectOffset = Vector2.zero;

        if (overrideOffset != null)
            activeObjectOffset = overrideOffset.Value;

        var playerGraphics = (PlayerGraphics)self.graphicsModule;

        var pos = playerGraphics.head.pos + activeObjectOffset;
        pos.x += self.mainBodyChunk.vel.x * 1.0f;

        if (self.TryGetPearlcatModule(out var playerModule) && playerModule.ShieldTimer > 0 || self.onBack != null)
            pos.y += self.onBack != null ? 40.0f : 30.0f;

        return pos;
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

        if (dist < 0.1f)
            firstChunk.pos = targetPos;
    }
}
