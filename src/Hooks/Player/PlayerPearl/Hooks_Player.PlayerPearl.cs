using System.Linq;
using RWCustom;
using UnityEngine;

namespace Pearlcat;

public static partial class Hooks_Player
{
    public static void ApplyHooks_Player_PlayerPearl()
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

    
    // Management
    public static void MarkAsPlayerObject(this AbstractPhysicalObject abstractObject)
    {
        var module = ModuleManager.PlayerPearlData.GetValue(abstractObject, x => new PlayerPearlModule());

        if (module.IsCurrentlyStored) return;

        var physicalObject = abstractObject.realizedObject;
        
        if (abstractObject.realizedObject == null) return;

        module.IsCurrentlyStored = true;
        module.Gravity = physicalObject.gravity;

        module.CollideWithObjects = physicalObject.CollideWithObjects;
        module.CollideWithSlopes = physicalObject.CollideWithSlopes;
        module.CollideWithTerrain = physicalObject.CollideWithTerrain;

        if (physicalObject is DataPearl pearl)
        {
            module.PearlGlimmerWait = pearl.glimmerWait;
        }

        if (physicalObject is Weapon weapon)
        {
            module.WeaponRotationSpeed = weapon.rotationSpeed;
        }
    }

    public static void ClearAsPlayerObject(this AbstractPhysicalObject abstractObject)
    {
        if (!abstractObject.TryGetPlayerPearlModule(out var module)) return;

        if (!module.IsCurrentlyStored) return;

        var physicalObject = abstractObject.realizedObject;
        if (physicalObject == null) return;

        module.IsCurrentlyStored = false;

        //physicalObject.gravity = module.Gravity;
        physicalObject.gravity = 1.0f; // yem

        physicalObject.CollideWithObjects = module.CollideWithObjects;
        physicalObject.CollideWithSlopes = module.CollideWithSlopes;
        physicalObject.CollideWithTerrain = module.CollideWithTerrain;

        if (physicalObject is DataPearl pearl)
        {
            pearl.glimmerWait = module.PearlGlimmerWait;
        }

        if (physicalObject is Weapon weapon)
        {
            weapon.rotationSpeed = module.WeaponRotationSpeed;
        }
    }

    
    private static bool AbstractPhysicalObject_UsesAPersistantTracker(On.AbstractPhysicalObject.orig_UsesAPersistantTracker orig, AbstractPhysicalObject abs)
    {
        var result = orig(abs);

        if (abs.IsPlayerPearl())
        {
            return false;
        }

        return result;
    }
    
    private static void AbstractPhysicalObject_Update(On.AbstractPhysicalObject.orig_Update orig, AbstractPhysicalObject self, int time)
    {
        orig(self, time);

        if (self.IsPlayerPearl() && self.Room.world.game.GetStorySession is StoryGameSession session)
        {
            session.RemovePersistentTracker(self);
        }
    }


    private static void PhysicalObject_Update(On.PhysicalObject.orig_Update orig, PhysicalObject self, bool eu)
    {        
        orig(self, eu);

        if (!self.abstractPhysicalObject.TryGetPlayerPearlModule(out var module)) return;


        if (module.CooldownTimer > 0)
        {
            var playerModule = self.room.game.GetAllPlayerData().FirstOrDefault(x => x.Inventory.Contains(self.abstractPhysicalObject));
            var effect = self.abstractPhysicalObject.GetPOEffect();
            
            if (effect.MajorEffect != PearlEffect.MajorEffectType.SHIELD || (playerModule != null && module.CooldownTimer != 0 && playerModule.PlayerRef.TryGetTarget(out var player) && player.airInLungs == 1.0f))
                module.CooldownTimer--;

            if (module.CooldownTimer == 0 && effect.MajorEffect == PearlEffect.MajorEffectType.SHIELD)
            {
                if (ModOptions.InventoryPings.Value)
                    playerModule?.ShowHUD(80);
                
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

    private static void PhysicalObject_Grabbed(On.PhysicalObject.orig_Grabbed orig, PhysicalObject self, Creature.Grasp grasp)
    {
        orig(self, grasp);

        if (self.abstractPhysicalObject.IsPlayerPearl())
            grasp.Release();
    }


    private static void DataPearl_Update(On.DataPearl.orig_Update orig, DataPearl self, bool eu)
    {
        orig(self, eu);

        if (!self.abstractPhysicalObject.TryGetPlayerPearlModule(out var module) || !module.IsCurrentlyStored) return;

        self.CollideWithObjects = false;
        self.CollideWithSlopes = false;
        self.CollideWithTerrain = false;

        self.glimmerWait = 40;
    }

    private static void DataPearl_DrawSprites(On.DataPearl.orig_DrawSprites orig, DataPearl self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        IDrawable_DrawSprites(self, sLeaser, rCam, timeStacker, camPos);

        // CW Unique Pearl Color Override
        if (self.AbstractPearl.dataPearlType == Enums.Pearls.CW_Pearlcat)
        {
            sLeaser.sprites[0].color = Custom.hexToColor("ffffff");
            sLeaser.sprites[1].color = Custom.hexToColor("1175d9");
        }
    }

    private static void IDrawable_DrawSprites(PhysicalObject self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        foreach (var sprite in sLeaser.sprites)
        {
            sprite.alpha = 1.0f;
        }

        if (!self.abstractPhysicalObject.TryGetPearlGraphicsModule(out var pearlGraphics)) return;

        pearlGraphics.ParentGraphics_DrawSprites(self, sLeaser, rCam, timeStacker, camPos);
    }


    // Creature Interaction
    private static bool Creature_Grab(On.Creature.orig_Grab orig, Creature self, PhysicalObject obj, int graspUsed, int chunkGrabbed, Creature.Grasp.Shareability shareability, float dominance, bool overrideEquallyDominant, bool pacifying)
    {
        var result = orig(self, obj, graspUsed, chunkGrabbed, shareability, dominance, overrideEquallyDominant, pacifying);

        if (obj.abstractPhysicalObject.IsPlayerPearl())
            return false;
        
        if (obj is Player player && player.TryGetPearlcatModule(out var playerModule) && playerModule.ShieldActive && player.IsHostileToMe(self))
        {
            if (!(self is Centipede && playerModule.ShieldTimer > 0))
            {
                DeflectEffect(self.room, self.DangerPos);
            }

            self.Stun(10);
            self.ReleaseGrasp(graspUsed);

            playerModule.ActivateVisualShield();
            return false;
        }

        return result;
    }

    private static int ScavengerAI_CollectScore_PhysicalObject_bool(On.ScavengerAI.orig_CollectScore_PhysicalObject_bool orig, ScavengerAI self, PhysicalObject obj, bool weaponFiltered)
    {
        var result = orig(self, obj, weaponFiltered);

        // weird nullref here
        if (obj?.abstractPhysicalObject != null && obj.abstractPhysicalObject.IsPlayerPearl())
            return 0;

        return result;
    }
}
