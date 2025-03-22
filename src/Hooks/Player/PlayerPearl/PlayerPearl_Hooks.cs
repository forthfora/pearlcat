using RWCustom;

namespace Pearlcat;

public static class PlayerPearl_Hooks
{
    public static void ApplyHooks()
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

        if (!self.IsPlayerPearl())
        {
            return;
        }

        if (self.world.game.GetStorySession is StoryGameSession session)
        {
            session.RemovePersistentTracker(self);
        }


        // Clean up if our abstract owner is gone (really only useful in Meadow for player disconnects as for Warp the room will be deleted too)
        var playerData = self.world.game.GetAllPearlcatModules();

        var hasOwner = playerData.Any(x => x.Inventory.Contains(self));

        if (!hasOwner)
        {
            self.realizedObject?.AbstractedEffect();

            self.realizedObject?.Destroy();
            self.Destroy();
        }
    }

    private static void PhysicalObject_Update(On.PhysicalObject.orig_Update orig, PhysicalObject self, bool eu)
    {        
        orig(self, eu);

        if (!self.abstractPhysicalObject.TryGetPlayerPearlModule(out var module))
        {
            return;
        }


        if (module.CooldownTimer > 0)
        {
            var playerModule = self.abstractPhysicalObject.world.game.GetAllPearlcatModules().FirstOrDefault(x => x.Inventory.Contains(self.abstractPhysicalObject));
            var effect = self.abstractPhysicalObject.GetPearlEffect();
            
            if (effect.MajorEffect != PearlEffect.MajorEffectType.Shield || (playerModule is not null && module.CooldownTimer != 0 && playerModule.PlayerRef is not null && playerModule.PlayerRef.airInLungs == 1.0f))
            {
                module.CooldownTimer--;
            }

            if (module.CooldownTimer == 0 && effect.MajorEffect == PearlEffect.MajorEffectType.Shield)
            {
                if (ModOptions.InventoryPings)
                {
                    playerModule?.ShowHUD(80);
                }

                module.InventoryFlash = true;

                self.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, self.firstChunk, false, 1.0f, 3.0f);
            }
        }

        if (!module.IsCurrentlyStored)
        {
            return;
        }

        self.gravity = 0.0f;

        self.CollideWithObjects = false;
        self.CollideWithSlopes = false;
        self.CollideWithTerrain = false;

        if (self is Weapon weapon)
        {
            weapon.rotationSpeed = 0.0f;
        }
    }

    private static void PhysicalObject_Grabbed(On.PhysicalObject.orig_Grabbed orig, PhysicalObject self, Creature.Grasp grasp)
    {
        orig(self, grasp);

        if (self.abstractPhysicalObject.IsPlayerPearl())
        {
            grasp.Release();
        }
    }


    private static void DataPearl_Update(On.DataPearl.orig_Update orig, DataPearl self, bool eu)
    {
        orig(self, eu);

        if (self.abstractPhysicalObject.TryGetPlayerPearlModule(out var module))
        {
            if (module.IsCurrentlyStored)
            {
                self.CollideWithObjects = false;
                self.CollideWithSlopes = false;
                self.CollideWithTerrain = false;

                self.glimmerWait = 40;
            }

            if (ModCompat_Helpers.RainMeadow_IsOnline)
            {
                var shouldSyncPos = !module.IsCurrentlyStored;

                if (module.IsSentry)
                {
                    shouldSyncPos = true;
                }
                else
                {
                    // Player or graphics is missing, could be due to latency, just let Meadow sync the pos in that case
                    if (!self.AbstractPearl.TryGetPlayerPearlOwner(out var player) || player.room is null || player.graphicsModule is null)
                    {
                        shouldSyncPos = true;
                    }
                }

                MeadowCompat.SetPosSynced(self.AbstractPearl, shouldSyncPos);
            }
        }

        if (self.abstractPhysicalObject.TryGetPearlGraphicsModule(out var graphics))
        {
            graphics.CheckIfShouldDestroy();
        }
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

        if (!self.abstractPhysicalObject.TryGetPearlGraphicsModule(out var pearlGraphics))
        {
            return;
        }

        pearlGraphics.ParentGraphics_DrawSprites(self, sLeaser, rCam, timeStacker, camPos);
    }


    // Creature Interaction
    private static bool Creature_Grab(On.Creature.orig_Grab orig, Creature self, PhysicalObject obj, int graspUsed, int chunkGrabbed, Creature.Grasp.Shareability shareability, float dominance, bool overrideEquallyDominant, bool pacifying)
    {
        var result = orig(self, obj, graspUsed, chunkGrabbed, shareability, dominance, overrideEquallyDominant, pacifying);

        if (obj.abstractPhysicalObject.IsPlayerPearl())
        {
            return false;
        }

        if (obj is Player player && player.TryGetPearlcatModule(out var playerModule) && playerModule.ShieldActive && player.IsHostileToMe(self))
        {
            if (!(self is Centipede && playerModule.ShieldTimer > 0))
            {
                self.room.DeflectEffect(self.DangerPos);
            }

            self.Stun(10);
            self.ReleaseGrasp(graspUsed);

            playerModule.ActivateVisualShield();
            return false;
        }

        return result;
    }

    private static int ScavengerAI_CollectScore_PhysicalObject_bool(On.ScavengerAI.orig_CollectScore_PhysicalObject_bool orig, ScavengerAI self, PhysicalObject? obj, bool weaponFiltered)
    {
        var result = orig(self, obj, weaponFiltered);

        // weird nullref here
        if (obj?.abstractPhysicalObject is not null && obj.abstractPhysicalObject.IsPlayerPearl())
        {
            return 0;
        }

        return result;
    }
}
