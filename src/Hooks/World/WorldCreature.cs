
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Pearlcat;

public static partial class Hooks
{
    public static void ApplyWorldCreatureHooks()
    {
        On.KingTusks.Tusk.ShootUpdate += Tusk_ShootUpdate;
        On.KingTusks.Tusk.Update += Tusk_Update;

        //On.DartMaggot.ShotUpdate += DartMaggot_ShotUpdate;
        On.BigNeedleWorm.Swish += BigNeedleWorm_Swish;

        new Hook(
            typeof(RegionGate).GetProperty(nameof(RegionGate.MeetRequirement), BindingFlags.Instance | BindingFlags.Public).GetGetMethod(),
            typeof(Hooks).GetMethod(nameof(GetRegionGateMeetRequirement), BindingFlags.Static | BindingFlags.Public)
        );

        new Hook(
            typeof(StoryGameSession).GetProperty(nameof(StoryGameSession.slugPupMaxCount), BindingFlags.Instance | BindingFlags.Public).GetGetMethod(),
            typeof(Hooks).GetMethod(nameof(GetStoryGameSessionSlugPupMaxCount), BindingFlags.Static | BindingFlags.Public)
        );

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

        try
        {
            IL.BigEel.JawsSnap += BigEel_JawsSnap;

            IL.Lizard.SpearStick += Lizard_SpearStick;
            IL.Lizard.Violence += Lizard_Violence;
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("WorldCreature IL Error: \n" + e);
        }
    }



    // Bypass lizard armor if deflected
    // This is so it actually deals damage
    private static void Lizard_Violence(ILContext il)
    {
        var c = new ILCursor(il);
        
        var dest = c.DefineLabel();

        // Grab dest after so these don't count these as mouth shots
        c.GotoNext(MoveType.Before,
            x => x.MatchLdarga(2),
            x => true,
            x => x.MatchCallOrCallvirt<Lizard>(nameof(Lizard.HitInMouth)),
            x => x.MatchBrfalse(out dest));

        // Make this deal damage
        c.GotoPrev(MoveType.Before,
            x => x.MatchLdarga(2),
            x => true,
            x => x.MatchCallOrCallvirt<Lizard>(nameof(Lizard.HitHeadShield)),
            x => x.MatchBrfalse(out _));

        // Ldarg 0 on the stack
        c.Emit(OpCodes.Ldarg_1); // BodyChunk
        c.EmitDelegate<Func<Lizard, BodyChunk, bool>>((self, bodyChunk) =>
        {
            if (bodyChunk.owner is not Spear weapon) return false;

            var playerData = self.abstractCreature?.world?.game?.GetAllPlayerData();

            if (playerData == null) return false;

            // wow
            foreach (var module in playerData)
            {
                foreach (var item in module.Inventory)
                {
                    if (item.TryGetPOModule(out var poModule))
                    {
                        if (poModule.VisitedObjects.TryGetValue(weapon, out _))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        });

        c.Emit(OpCodes.Brtrue, dest); // dest skips the HitHead check

        c.Emit(OpCodes.Ldarg_0); // return the value we consumed
    }

    // This is so the spear actually embeds
    private static void Lizard_SpearStick(ILContext il)
    {
        var c = new ILCursor(il);

        var dest = c.DefineLabel();

        c.GotoNext(MoveType.Before,
            x => x.MatchCallOrCallvirt<Lizard>(nameof(Lizard.HitHeadShield)),
            x => x.MatchBrfalse(out dest));

        c.GotoPrev(MoveType.After,
            x => x.MatchLdarg(3));

        c.Emit(OpCodes.Pop);

        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldarg_1); // Weapon
        c.EmitDelegate<Func<Lizard, Weapon, bool>>((self, weapon) =>
        {
            var playerData = self.abstractCreature?.world?.game?.GetAllPlayerData();

            if (playerData == null) return false;

            // wow
            foreach (var module in playerData)
            {
                foreach (var item in module.Inventory)
                {
                    if (item.TryGetPOModule(out var poModule))
                    {
                        if (poModule.VisitedObjects.TryGetValue(weapon, out _))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        });



        //c.Emit(OpCodes.Dup); // need 2 bools: the delegate will consume 1 and the branch the other

        //c.Emit(OpCodes.Ldarg_0);
        //c.Emit(OpCodes.Ldarg_3); // Body Chunk
        //c.EmitDelegate<Func<bool, Lizard, BodyChunk, BodyChunk>>((wasRedPearlWeapon, self, bodyChunk) =>
        //{
        //    if (wasRedPearlWeapon)
        //    {
        //        // Prevent hitting head directly, this won't actually deal damage unless it's considered a mouth shot (which means unwanted additional checks)
        //        if (bodyChunk.index == 0 || bodyChunk.index == 1)
        //        {
        //            if (self.bodyChunks.Length >= 3)
        //            {
        //                bodyChunk = self.bodyChunks[UnityEngine.Random.Range(2, self.bodyChunks.Length - 1)];
        //            }
        //        }
        //    }

        //    return bodyChunk;
        //});

        //c.Emit(OpCodes.Starg, 3);


        c.Emit(OpCodes.Brtrue, dest); // branch past HitHead check

        c.Emit(OpCodes.Ldarg, 3); // return the value we popped at the start
    }


    // Leech
    private static void Leech_Attached(On.Leech.orig_Attached orig, Leech self)
    {
        orig(self);

        if (self.grasps.FirstOrDefault()?.grabbed is not Player player) return;

        if (!player.TryGetPearlcatModule(out var module)) return;


        if (module.ShieldActive)
            module.ActivateVisualShield();

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


    // Leviathan
    private static void BigEel_JawsSnap(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(MoveType.After,
            x => x.MatchLdsfld<SoundID>(nameof(SoundID.Leviathan_Crush_NPC)));

        c.GotoPrev(MoveType.After,
            x => x.MatchConvI4(),
            x => x.MatchBlt(out _));

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Action<BigEel>>((self) =>
        {
            var didRevive = false;

            for (int i = self.clampedObjects.Count - 1; i >= 0; i--)
            {
                var clampedObj = self.clampedObjects[i];
                var obj = clampedObj.chunk?.owner;

                if (obj == null) continue;

                if (obj is not Player player) continue;

                if (!player.TryGetPearlcatModule(out var playerModule)) continue;

                if (playerModule.ReviveCount <= 0) continue;

                if (player.graphicsModule != null)
                    self.graphicsModule.ReleaseSpecificInternallyContainedObjectSprites(player);

                foreach (var item in playerModule.PostDeathInventory)
                {
                    if (item == obj.abstractPhysicalObject)
                        self.clampedObjects.Remove(clampedObj);

                    var graphics = item.realizedObject?.graphicsModule;

                    if (graphics == null) continue;

                    self.graphicsModule.ReleaseSpecificInternallyContainedObjectSprites(graphics);
                }

                self.Stun(100);

                self.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, player.firstChunk, false, 2.0f, 0.5f);
                self.room.AddObject(new ShockWave(player.firstChunk.pos, 700f, 0.6f, 90, false));

                didRevive = true;
            }

            if (didRevive)
                self.clampedObjects.Clear();

            //foreach (var item in self.clampedObjects)
            //    Plugin.Logger.LogWarning(item?.chunk?.owner?.GetType());
        });
    }


    // Scavenger
    private static int ScavengerAI_CollectScore_PhysicalObject_bool1(On.ScavengerAI.orig_CollectScore_PhysicalObject_bool orig, ScavengerAI self, PhysicalObject obj, bool weaponFiltered)
    {
        var result = orig(self, obj, weaponFiltered);

        if (obj?.abstractPhysicalObject is AbstractSpear spear && spear.TryGetSpearModule(out _))
            return 12;

        return result;
    }


    // Daddy Long Legs
    private static void DaddyLongLegs_Update(On.DaddyLongLegs.orig_Update orig, DaddyLongLegs self, bool eu)
    {
        orig(self, eu);

        var killedPlayers = new List<Player>();

        for (int i = self.eatObjects.Count - 1; i >= 0; i--)
        {
            var eatObj = self.eatObjects[i];

            if (eatObj.chunk.owner is not Player player) continue;

            if (!player.TryGetPearlcatModule(out var module)) continue;

            if (module.ReviveCount <= 0 && module.ShieldTimer <= 0) continue;

            if (eatObj.progression < 0.25f) continue;

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
                    self.room.DeflectEffect(self.grabChunk.pos);

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

            if (crit.realizedCreature is not Player player) continue;

            if (!player.TryGetPearlcatModule(out var playerModule)) continue;

            if (!playerModule.ShieldActive) continue;

            if (self.impaleChunk == null || self.impaleChunk.owner != player) continue;

            self.swishCounter = 0;
            self.firstChunk.vel = Vector2.zero;

            self.impaleChunk = null;

            self.Stun(40);

            self.room.DeflectEffect(self.firstChunk.pos);
            playerModule.ActivateVisualShield();
        }
    }

    private static void DartMaggot_ShotUpdate(On.DartMaggot.orig_ShotUpdate orig, DartMaggot self)
    {
        orig(self);

        foreach (var crit in self.room.world.game.Players)
        {
            if (crit.realizedCreature is not Player player) continue;

            if (!player.TryGetPearlcatModule(out var playerModule)) continue;

            if (!playerModule.ShieldActive) continue;

            if (!Custom.DistLess(player.firstChunk.pos, self.firstChunk.pos, 50.0f)) continue;

            self.mode = DartMaggot.Mode.Free;

            self.firstChunk.vel = Vector2.zero;

            self.room.DeflectEffect(self.firstChunk.pos);
            playerModule.ActivateVisualShield();
        }
    }


    // King Vulture
    private static void Tusk_ShootUpdate(On.KingTusks.Tusk.orig_ShootUpdate orig, KingTusks.Tusk self, float speed)
    {
        orig(self, speed);

        if (self.mode != KingTusks.Tusk.Mode.ShootingOut) return;

        foreach (var crit in self.vulture.abstractCreature.Room.world.game.Players)
        {
            if (crit.realizedCreature is not Player player) continue;

            if (!player.TryGetPearlcatModule(out var playerModule)) continue;

            if (!playerModule.ShieldActive) continue;


            var pos = self.chunkPoints[0, 0] + self.shootDir * (20.0f + speed);

            if (!Custom.DistLess(player.firstChunk.pos, pos, 50.0f)) continue;

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
            if (impaledPlayer.TryGetPearlcatModule(out var playerModule) && playerModule.ShieldTimer > 0)
                self.mode = KingTusks.Tusk.Mode.Dangling;
    }


    // Worm Grass
    private static void WormGrassPatch_Update(On.WormGrass.WormGrassPatch.orig_Update orig, WormGrass.WormGrassPatch self)
    {
        orig(self);

        for (int i = self.trackedCreatures.Count - 1; i >= 0; i--)
        {
            var trackedCreature = self.trackedCreatures[i];
            if (trackedCreature.creature is not Player player) continue;

            if (!player.TryGetPearlcatModule(out var playerModule)) continue;

            if (playerModule.ShieldTimer > 0)
                self.trackedCreatures.Remove(trackedCreature);
        }
    }

    private static void Worm_Attached(On.WormGrass.Worm.orig_Attached orig, WormGrass.Worm self)
    {
        orig(self);

        if (self.attachedChunk?.owner is not Player player) return;

        if (!player.TryGetPearlcatModule(out var playerModule)) return;

        if (self.patch?.trackedCreatures == null) return;


        var playerPull = self.patch.trackedCreatures.FirstOrDefault(x => x.creature == self.attachedChunk.owner);

        if (playerPull == null) return;

        if (!playerModule.ShieldActive) return;

        DeflectEffect(self.room, self.pos);
        playerModule.ActivateVisualShield();

        if (playerModule.ShieldTimer > 0)
            self.attachedChunk = null;
    }


    // Paincone
    private static void AttachedBee_Update(On.SporePlant.AttachedBee.orig_Update orig, SporePlant.AttachedBee self, bool eu)
    {
        orig(self, eu);

        if (self.attachedChunk?.owner is not Player player) return;

        if (!player.TryGetPearlcatModule(out var playerModule)) return;

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


    // Possesssion
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
                if (target == self.creature) return false;
            }
        }

        return orig(self, chunk);    
    }
}
