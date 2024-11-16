using RWCustom;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pearlcat;

public static class Creatures_Hooks
{
    public static void ApplyHooks()
    {
        On.KingTusks.Tusk.ShootUpdate += Tusk_ShootUpdate;
        On.KingTusks.Tusk.Update += Tusk_Update;

        On.BigNeedleWorm.Swish += BigNeedleWorm_Swish;

        On.SporePlant.AttachedBee.Update += AttachedBee_Update;

        On.WormGrass.WormGrassPatch.Update += WormGrassPatch_Update;
        On.WormGrass.Worm.Attached += Worm_Attached;

        On.DaddyTentacle.Update += DaddyTentacle_Update;
        On.DaddyLongLegs.Update += DaddyLongLegs_Update;

        On.ScavengerAI.CollectScore_PhysicalObject_bool += ScavengerAI_CollectScore_PhysicalObject_bool1;

        On.TempleGuardAI.ThrowOutScore += TempleGuardAI_ThrowOutScore;

        On.Leech.Attached += Leech_Attached;

        On.Creature.SafariControlInputUpdate += Creature_SafariControlInputUpdate;
        On.ArtificialIntelligence.VisualContact_BodyChunk += ArtificialIntelligence_VisualContact_BodyChunk;

        On.KingTusks.Tusk.ShootUpdate += Tusk_ShootUpdate;
        On.KingTusks.Tusk.Update += Tusk_Update;

        On.BigNeedleWorm.Swish += BigNeedleWorm_Swish;

        On.SporePlant.AttachedBee.Update += AttachedBee_Update;

        On.WormGrass.WormGrassPatch.Update += WormGrassPatch_Update;
        On.WormGrass.Worm.Attached += Worm_Attached;

        On.DaddyTentacle.Update += DaddyTentacle_Update;
        On.DaddyLongLegs.Update += DaddyLongLegs_Update;

        On.ScavengerAI.CollectScore_PhysicalObject_bool += ScavengerAI_CollectScore_PhysicalObject_bool1;

        On.TempleGuardAI.ThrowOutScore += TempleGuardAI_ThrowOutScore;

        On.Leech.Attached += Leech_Attached;
    }


    // Leech
    private static void Leech_Attached(On.Leech.orig_Attached orig, Leech self)
    {
        orig(self);

        if (self.grasps.FirstOrDefault()?.grabbed is not Player player)
        {
            return;
        }

        if (!player.TryGetPearlcatModule(out var module))
        {
            return;
        }


        if (module.ShieldActive)
        {
            module.ActivateVisualShield();
        }

        if (module.ShieldTimer > 0)
        {
            self.Stun(80);
            self.LoseAllGrasps();
        }
    }


    // Temple Guard
    private static float TempleGuardAI_ThrowOutScore(On.TempleGuardAI.orig_ThrowOutScore orig, TempleGuardAI self, Tracker.CreatureRepresentation crit)
    {
        var result = orig(self, crit);

        if (crit.representedCreature?.realizedCreature is Player player)
        {
            if (player.IsPearlpup())
            {
                return 0.0f;
            }

            if (player.TryGetPearlcatModule(out var playerModule) && playerModule.IsAdultPearlpup)
            {
                return 500f / (self.ProtectExitDistance(crit.BestGuessForPosition().Tile) + (crit.TicksSinceSeen / 2f));
            }
        }

        return result;
    }


    // Scavenger
    private static int ScavengerAI_CollectScore_PhysicalObject_bool1(On.ScavengerAI.orig_CollectScore_PhysicalObject_bool orig, ScavengerAI self, PhysicalObject? obj, bool weaponFiltered)
    {
        var result = orig(self, obj, weaponFiltered);

        if (obj?.abstractPhysicalObject is AbstractSpear spear && spear.TryGetSpearModule(out _))
        {
            return 12;
        }

        return result;
    }


    // Daddy Long Legs
    private static void DaddyLongLegs_Update(On.DaddyLongLegs.orig_Update orig, DaddyLongLegs self, bool eu)
    {
        orig(self, eu);

        var killedPlayers = new List<Player>();

        for (var i = self.eatObjects.Count - 1; i >= 0; i--)
        {
            var eatObj = self.eatObjects[i];

            if (eatObj.chunk.owner is not Player player)
            {
                continue;
            }

            if (!player.TryGetPearlcatModule(out var module))
            {
                continue;
            }

            if (module.ReviveCount <= 0 && module.ShieldTimer <= 0)
            {
                continue;
            }

            if (eatObj.progression < 0.25f)
            {
                continue;
            }

            if (module.ShieldTimer <= 0 && !killedPlayers.Contains(player))
            {
                killedPlayers.Add(player);
                player.Die();
            }

            eatObj.progression = 0.0f;

            self.digestingCounter = 0;
            self.Stun(20);

            player.ChangeCollisionLayer(1);
            self.eatObjects.Remove(eatObj);
        }
    }

    private static void DaddyTentacle_Update(On.DaddyTentacle.orig_Update orig, DaddyTentacle self)
    {
        orig(self);

        var grabbedPlayer = self.grabChunk?.owner as Player;

        if ((grabbedPlayer != null || self.grabChunk?.owner == grabbedPlayer?.slugOnBack?.slugcat) && grabbedPlayer?.TryGetPearlcatModule(out var playerModule) == true && playerModule.ShieldActive)
        {
            playerModule.ActivateVisualShield();

            if (playerModule.ShieldTimer > 0)
            {
                if (self.grabChunk != null)
                {
                    self.room.DeflectEffect(self.grabChunk.pos);
                }

                self.stun = 100;
            }
        }
    }


    // Noodlefly
    private static void BigNeedleWorm_Swish(On.BigNeedleWorm.orig_Swish orig, BigNeedleWorm self)
    {
        orig(self);

        foreach (var crit in self.abstractCreature.Room.world.game.Players)
        {

            if (crit.realizedCreature is not Player player)
            {
                continue;
            }

            if (!player.TryGetPearlcatModule(out var playerModule))
            {
                continue;
            }

            if (!playerModule.ShieldActive)
            {
                continue;
            }

            if (self.impaleChunk == null || self.impaleChunk.owner != player)
            {
                continue;
            }

            self.swishCounter = 0;
            self.firstChunk.vel = Vector2.zero;

            self.impaleChunk = null;

            self.Stun(40);

            self.room.DeflectEffect(self.firstChunk.pos);
            playerModule.ActivateVisualShield();
        }
    }


    // King Vulture
    private static void Tusk_ShootUpdate(On.KingTusks.Tusk.orig_ShootUpdate orig, KingTusks.Tusk self, float speed)
    {
        orig(self, speed);

        if (self.mode != KingTusks.Tusk.Mode.ShootingOut)
        {
            return;
        }

        foreach (var crit in self.vulture.abstractCreature.Room.world.game.Players)
        {
            if (crit.realizedCreature is not Player player)
            {
                continue;
            }

            if (!player.TryGetPearlcatModule(out var playerModule))
            {
                continue;
            }

            if (!playerModule.ShieldActive)
            {
                continue;
            }


            var pos = self.chunkPoints[0, 0] + self.shootDir * (20.0f + speed);

            if (!Custom.DistLess(player.firstChunk.pos, pos, 50.0f))
            {
                continue;
            }

            self.mode = KingTusks.Tusk.Mode.Dangling;

            self.room.DeflectEffect(pos);

            self.head.pos += Custom.DirVec(self.head.pos, self.chunkPoints[1, 0]) * 100f;
            self.head.vel += Custom.DirVec(self.head.pos, self.chunkPoints[1, 0]) * 100f;

            self.chunkPoints[0, 2] = self.shootDir * speed * 0.4f;
            self.chunkPoints[1, 2] = self.shootDir * speed * 0.6f;

            var rand = Custom.RNV();
            self.chunkPoints[0, 0] += rand * 4f;
            self.chunkPoints[0, 2] += rand * 6f;
            self.chunkPoints[1, 0] -= rand * 4f;
            self.chunkPoints[1, 2] -= rand * 6f;

            playerModule.ActivateVisualShield();
        }
    }

    private static void Tusk_Update(On.KingTusks.Tusk.orig_Update orig, KingTusks.Tusk self)
    {
        orig(self);

        if (self.impaleChunk != null && self.impaleChunk.owner is Player impaledPlayer)
        {
            if (impaledPlayer.TryGetPearlcatModule(out var playerModule) && playerModule.ShieldTimer > 0)
            {
                self.mode = KingTusks.Tusk.Mode.Dangling;
            }
        }
    }


    // Worm Grass
    private static void WormGrassPatch_Update(On.WormGrass.WormGrassPatch.orig_Update orig, WormGrass.WormGrassPatch self)
    {
        orig(self);

        for (var i = self.trackedCreatures.Count - 1; i >= 0; i--)
        {
            var trackedCreature = self.trackedCreatures[i];
            if (trackedCreature.creature is not Player player)
            {
                continue;
            }

            if (!player.TryGetPearlcatModule(out var playerModule))
            {
                continue;
            }

            if (playerModule.ShieldTimer > 0)
            {
                self.trackedCreatures.Remove(trackedCreature);
            }
        }
    }

    private static void Worm_Attached(On.WormGrass.Worm.orig_Attached orig, WormGrass.Worm self)
    {
        orig(self);

        if (self.attachedChunk?.owner is not Player player)
        {
            return;
        }

        if (!player.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }


        var playerPull = self.patch?.trackedCreatures?.FirstOrDefault(x => x.creature == self.attachedChunk.owner);

        if (playerPull == null)
        {
            return;
        }

        if (!playerModule.ShieldActive)
        {
            return;
        }

        self.room.DeflectEffect(self.pos);
        playerModule.ActivateVisualShield();

        if (playerModule.ShieldTimer > 0)
        {
            self.attachedChunk = null;
        }
    }


    // Paincone
    private static void AttachedBee_Update(On.SporePlant.AttachedBee.orig_Update orig, SporePlant.AttachedBee self, bool eu)
    {
        orig(self, eu);

        if (self.attachedChunk?.owner is not Player player)
        {
            return;
        }

        if (!player.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        if (playerModule.ShieldActive)
        {
            playerModule.ActivateVisualShield();
        }

        if (playerModule.ShieldTimer > 0)
        {
            self.BreakStinger();
            self.stingerOut = false;
        }
    }


    // Possession
    private static void Creature_SafariControlInputUpdate(On.Creature.orig_SafariControlInputUpdate orig, Creature self, int playerIndex)
    {
        foreach (var module in self.abstractCreature.world.game.GetAllPlayerData())
        {
            if (module.PossessedCreature?.TryGetTarget(out var target) == true && target == self.abstractCreature && module.PlayerRef.TryGetTarget(out var player))
            {
                playerIndex = player.playerState.playerNumber;
            }
        }

        orig(self, playerIndex);
    }

    private static bool ArtificialIntelligence_VisualContact_BodyChunk(On.ArtificialIntelligence.orig_VisualContact_BodyChunk orig, ArtificialIntelligence self, BodyChunk chunk)
    {
        if (chunk.owner is Player player && player.TryGetPearlcatModule(out var playerModule))
        {
            if (playerModule.PossessedCreature?.TryGetTarget(out var target) == true)
            {
                if (target == self.creature)
                {
                    return false;
                }
            }
        }

        return orig(self, chunk);    
    }
}
