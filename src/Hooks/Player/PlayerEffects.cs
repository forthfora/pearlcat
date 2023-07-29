﻿using MoreSlugcats;
using RWCustom;
using SlugBase.Features;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Pearlcat.POEffect;
using Random = UnityEngine.Random;

namespace Pearlcat;

public static partial class Hooks
{
    public static void UpdateCombinedPOEffect(Player self, PlayerModule playerModule)
    {
        POEffect combinedEffect = new()
        {
            JumpHeightFac = 1.0f,
            RollSpeedFac = 1.0f,
            SlideSpeedFac = 1.0f
        };

        foreach (var playerObject in playerModule.Inventory)
        {
            var effect = playerObject.GetPOEffect();
            var mult = playerObject == playerModule.ActiveObject ? effect.ActiveMultiplier : 1.0f;


            combinedEffect.ThrowingSkill += effect.ThrowingSkill;

            combinedEffect.RunSpeedFac += effect.RunSpeedFac * mult;
            combinedEffect.CorridorClimbSpeedFac += effect.CorridorClimbSpeedFac * mult;
            combinedEffect.PoleClimbSpeedFac += effect.PoleClimbSpeedFac * mult;

            combinedEffect.LungsFac += effect.LungsFac * mult;
            combinedEffect.BodyWeightFac += effect.BodyWeightFac * mult;

            combinedEffect.JumpHeightFac += effect.JumpHeightFac * mult;
            combinedEffect.RollSpeedFac += effect.RollSpeedFac * mult;
            combinedEffect.SlideSpeedFac += effect.SlideSpeedFac * mult;
        }
        
        if (playerModule.ActiveObject != null)
        {
            var effect = playerModule.ActiveObject.GetPOEffect();
            combinedEffect.MajorEffect = effect.MajorEffect;
        }

        playerModule.CurrentPOEffect = combinedEffect;
    }

    public static void ApplyCombinedPOEffect(Player self, PlayerModule playerModule)
    {
        var effect = playerModule.CurrentPOEffect;
        var stats = self.slugcatStats;
        var baseStats = playerModule.BaseStats;
    
        if (ModOptions.DisableMinorEffects.Value)
        {
            if (!self.Malnourished)
            {
                stats.throwingSkill = 2;
                stats.runspeedFac = 1.2f;
                stats.corridorClimbSpeedFac = 1.2f;
                stats.poleClimbSpeedFac = 1.25f;
            }
            else
            {
                stats.throwingSkill = 0;
                stats.runspeedFac = 0.875f;
                stats.corridorClimbSpeedFac = 0.86f;
                stats.poleClimbSpeedFac = 0.8f;
            }
        }
        else
        {
            stats.throwingSkill = (int)Mathf.Clamp(baseStats.throwingSkill + effect.ThrowingSkill, 0, 2);

            stats.lungsFac = Mathf.Clamp(baseStats.lungsFac + effect.LungsFac, 0.75f, float.MaxValue);
            stats.runspeedFac = Mathf.Clamp(baseStats.runspeedFac + effect.RunSpeedFac, 0.75f, float.MaxValue);

            stats.corridorClimbSpeedFac = Mathf.Clamp(baseStats.corridorClimbSpeedFac + effect.CorridorClimbSpeedFac, 0.75f, float.MaxValue);
            stats.poleClimbSpeedFac = Mathf.Clamp(baseStats.poleClimbSpeedFac + effect.PoleClimbSpeedFac, 0.75f, float.MaxValue);
            stats.bodyWeightFac = Mathf.Clamp(baseStats.bodyWeightFac + effect.BodyWeightFac, 0.75f, float.MaxValue);
        }

        var visibilityMult = ModOptions.VisibilityMultiplier.Value / 100.0f;

        stats.loudnessFac = baseStats.loudnessFac * visibilityMult;
        stats.visualStealthInSneakMode = baseStats.visualStealthInSneakMode * visibilityMult;
        stats.generalVisibilityBonus = 0.4f * visibilityMult;

        UpdateSpearCreation(self, playerModule, effect);
        UpdateAgility(self, playerModule, effect);
        UpdateRevive(self, playerModule, effect);
        UpdateShield(self, playerModule, effect);
        UpdateRage(self, playerModule, effect);
        UpdateCamoflague(self, playerModule, effect);

        //if (playerModule.PearlpupRef?.TryGetTarget(out var pup) == true && pup.dangerGrasp != null && pup.graphicsModule != null)
        //{
        //    stats.throwingSkill = 2;
        //    stats.runspeedFac *= 1.5f;
        //    stats.corridorClimbSpeedFac *= 1.7f;
        //    stats.poleClimbSpeedFac *= 1.7f;
        //}

        //Plugin.Logger.LogWarning(
        //    "\nThrowSkill " + stats.throwingSkill +
        //    "\nLungsFac " + stats.lungsFac +
        //    "\nRunspeedFac " + stats.runspeedFac +
        //    "\nCorridorClimb " + stats.corridorClimbSpeedFac +
        //    "\nPoleClimb " + stats.poleClimbSpeedFac);
    }


    public static void UpdateSpearCreation(Player self, PlayerModule playerModule, POEffect effect)
    {
        if (ModOptions.DisableSpear.Value || self.inVoidSea) return;

        var spearCreationTime = 100;
        playerModule.SpearLerp = Custom.LerpMap(playerModule.SpearTimer, 5, spearCreationTime, 0.0f, 1.0f);

        playerModule.ForceLockSpearOnBack = false;

        if (effect.MajorEffect != MajorEffectType.SPEAR_CREATION)
        {
            playerModule.SpearTimer = 0;
            playerModule.SpearDelay = 0;
            return;
        }

        if (playerModule.SpearCount <= 0) return;

        playerModule.ForceLockSpearOnBack = self.spearOnBack.HasASpear != playerModule.WasSpearOnBack || spearCreationTime < 20;

        var abilityInput = self.IsSpearCreationKeybindPressed(playerModule) && !self.IsStoreKeybindPressed(playerModule)
            && !self.grasps.Any(x => x?.grabbed is Creature creature && creature.dead && PlayerFeatures.Diet.TryGet(self, out var diet) && diet.GetFoodMultiplier(creature) > 0) ;

        var holdingSpear = self.GraspsHasType(AbstractPhysicalObject.AbstractObjectType.Spear) >= 0;

        //Plugin.Logger.LogWarning(self.eatCounter);

        if (abilityInput && (self.spearOnBack.interactionLocked || (!holdingSpear && !self.spearOnBack.HasASpear)) && !(holdingSpear && self.spearOnBack.HasASpear) && !(self.spearOnBack.HasASpear && self.onBack != null))
        {
            playerModule.ForceLockSpearOnBack = true;

            if (playerModule.SpearDelay > 10)
            {
                playerModule.BlockInput = true;
                playerModule.SpearTimer++;
                self.Blink(5);

                if (playerModule.SpearTimer > spearCreationTime)
                {
                    playerModule.SpearTimer = 0;

                    var abstractSpear = new AbstractSpear(self.room.world, null, self.room.GetWorldCoordinate(self.mainBodyChunk.pos), self.room.game.GetNewID(), false);
                    self.room.abstractRoom.AddEntity(abstractSpear);
                    abstractSpear.pos = self.abstractCreature.pos;
                    abstractSpear.RealizeInRoom();

                    var save = self.abstractCreature.Room.world.game.GetMiscWorld();
                    var spearModule = new SpearModule(playerModule.ActiveColor);

                    save?.PearlSpears.Add(abstractSpear.ID.number, spearModule);

                    if (holdingSpear || self.onBack != null)
                        self.spearOnBack.SpearToBack((Spear)abstractSpear.realizedObject);

                    else
                        self.SlugcatGrab(abstractSpear.realizedObject, self.FreeHand());


                    ConnectEffect(playerModule.ActiveObject?.realizedObject, abstractSpear.realizedObject.firstChunk.pos);

                    self.room?.PlaySound(Enums.Sounds.Pearlcat_SpearEquip, self.firstChunk, false, 1.0f, Random.Range(1.2f, 1.5f));

                    if (playerModule.ActiveObject != null)
                    {
                        var activeObj = playerModule.ActiveObject;
                        self.RemoveFromInventory(playerModule.ActiveObject);

                        activeObj.destroyOnAbstraction = true;
                        activeObj.Abstractize(activeObj.pos);
                    }
                }
            }
            else
            {
                playerModule.SpearDelay++;
            }
        }
        else
        {
            if (playerModule.SpearTimer > spearCreationTime / 2.0f)
                self.room?.AddObject(new ShockWave(playerModule.ActiveObject!.realizedObject.firstChunk.pos, 30.0f, 0.5f, 6));

            playerModule.SpearTimer = 0;
            playerModule.SpearDelay = 0;
        }
    }

    public static void UpdateAgility(Player self, PlayerModule playerModule, POEffect effect)
    {
        if (playerModule.AgilityOveruseTimer > 0)
            playerModule.AgilityOveruseTimer--;

        if (ModOptions.DisableAgility.Value || self.inVoidSea) return;

        var maxOveruse = 120;

        var velocityMult = Custom.LerpMap(playerModule.AgilityCount, 1, 5, 1.0f, 0.75f);
        velocityMult *= Custom.LerpMap(playerModule.AgilityOveruseTimer, 40, maxOveruse, 1.0f, 0.7f);


        var abilityInput = self.IsAgilityKeybindPressed(playerModule);
        var wasAbilityInput = playerModule.WasAgilityInput;

        bool canUseAbility = playerModule.AgilityCount > 0 && playerModule.AgilityOveruseTimer < maxOveruse
            && self.canJump <= 0 && !(self.eatMeat >= 20 || self.maulTimer >= 15)
            && self.Consious && self.bodyMode != Player.BodyModeIndex.Crawl
            && self.bodyMode != Player.BodyModeIndex.CorridorClimb && self.bodyMode != Player.BodyModeIndex.ClimbIntoShortCut
            && self.animation != Player.AnimationIndex.HangFromBeam && self.animation != Player.AnimationIndex.ClimbOnBeam
            && self.bodyMode != Player.BodyModeIndex.WallClimb
            && self.animation != Player.AnimationIndex.AntlerClimb && self.animation != Player.AnimationIndex.VineGrab
            && self.animation != Player.AnimationIndex.ZeroGPoleGrab && self.onBack == null;

        if (abilityInput && !wasAbilityInput && canUseAbility)
        {
            var agilityObject = playerModule.SetAgilityCooldown(-1);

            self.noGrabCounter = 5;
            var pos = self.firstChunk.pos;

            self.room.AddObject(new Explosion.ExplosionLight(pos, 160f, 1f, 3, Color.white));

            for (int j = 0; j < 10; j++)
            {
                var randVec = Custom.RNV();
                self.room.AddObject(new Spark(pos + randVec * Random.value * 40f, randVec * Mathf.Lerp(4f, 30f, Random.value), Color.white, null, 4, 18));
            }

            self.room.PlaySound(SoundID.Fire_Spear_Explode, pos, 0.15f + Random.value * 0.15f, 0.5f + Random.value * 2f);


            if (self.bodyMode == Player.BodyModeIndex.ZeroG || self.room.gravity == 0f || self.gravity == 0f || self.bodyMode == Player.BodyModeIndex.Swimming)
            {
                float inputX = self.input[0].x;
                float randVariation = self.input[0].y;

                while (inputX == 0f && randVariation == 0f)
                {
                    inputX = ((double)Random.value <= 0.33) ? 0 : (((double)Random.value <= 0.5) ? 1 : -1);
                    randVariation = ((double)Random.value <= 0.33) ? 0 : (((double)Random.value <= 0.5) ? 1 : -1);
                }

                self.bodyChunks[0].vel.x = 9f * inputX * velocityMult;
                self.bodyChunks[0].vel.y = 9f * randVariation * velocityMult;
                self.bodyChunks[1].vel.x = 8f * inputX * velocityMult;
                self.bodyChunks[1].vel.y = 8f * randVariation * velocityMult;
            }
            else
            {
                if (self.input[0].x != 0)
                {
                    self.bodyChunks[0].vel.y = Mathf.Min(self.bodyChunks[0].vel.y, 0f) + 8f * velocityMult;
                    self.bodyChunks[1].vel.y = Mathf.Min(self.bodyChunks[1].vel.y, 0f) + 7f * velocityMult;
                    self.jumpBoost = 6f;
                }

                if (self.input[0].x == 0 || self.input[0].y == 1)
                {
                    self.bodyChunks[0].vel.y = 16f * velocityMult;
                    self.bodyChunks[1].vel.y = 15f * velocityMult;
                    self.jumpBoost = 8f;
                }

                if (self.input[0].y == 1)
                {
                    self.bodyChunks[0].vel.x = 10f * self.input[0].x * velocityMult;
                    self.bodyChunks[1].vel.x = 8f * self.input[0].x * velocityMult;
                }
                else
                {
                    self.bodyChunks[0].vel.x = 15f * self.input[0].x;
                    self.bodyChunks[1].vel.x = 13f * self.input[0].x;
                }

                self.animation = Player.AnimationIndex.Flip;
                self.bodyMode = Player.BodyModeIndex.Default;
            }

            var targetPos = self.firstChunk.pos + self.firstChunk.vel * -10.0f;

            if (agilityObject != null)
                self.ConnectEffect(targetPos, GetObjectColor(agilityObject));

            playerModule.AgilityOveruseTimer += (int)Custom.LerpMap(playerModule.AgilityOveruseTimer, 0, 80, 40, 60);
        }

        bool isAnim = self.animation == Player.AnimationIndex.HangFromBeam || self.animation == Player.AnimationIndex.ClimbOnBeam
            || self.bodyMode == Player.BodyModeIndex.WallClimb || self.animation == Player.AnimationIndex.AntlerClimb
            || self.animation == Player.AnimationIndex.VineGrab || self.animation == Player.AnimationIndex.ZeroGPoleGrab
            || self.bodyMode == Player.BodyModeIndex.Swimming;

        // FREAKING NULL REF
        if (isAnim || self.canJump > 0 || !self.Consious || self.Stunned
            || ((self.bodyMode == Player.BodyModeIndex.ZeroG)
            && (self.wantToJump == 0 || !self.input[0].pckp)))
        {
            playerModule.ResetAgilityCooldown(30);
        }

        var overuse = playerModule.AgilityOveruseTimer;
        var overuseDisplayCount = overuse < 20 ? 0 : (int)Custom.LerpMap(overuse, 20, maxOveruse, 1, 5, 1.5f);

        for (int i = 0; i < overuseDisplayCount; i++)
        {
            if (Random.value < 0.25f)
                self.room.AddObject(new Explosion.ExplosionSmoke(self.mainBodyChunk.pos, Custom.RNV() * 2f * Random.value, 1f));

            if (Random.value < 0.5f)
                self.room.AddObject(new Spark(self.mainBodyChunk.pos, Custom.RNV(), Color.white, null, 4, 8));

            if (overuse > 90 && Random.value < 0.03f)
                self.ConnectEffect(self.mainBodyChunk.pos + Custom.RNV() * 80.0f, playerModule.ActiveColor);
        }

        if (overuse > maxOveruse && !self.Stunned)
        {
            self.room.PlaySound(SoundID.Fire_Spear_Explode, self.mainBodyChunk.pos, 0.3f + Random.value * 0.15f, 0.25f + Random.value * 1.5f);
            self.Stun(60);
        }
    }
    
    public static void UpdateRevive(Player self, PlayerModule playerModule, POEffect effect)
    {
        if (ModOptions.DisableRevive.Value || self.inVoidSea) return;

        if (playerModule.ActiveObject == null || !PlayerObjectData.TryGetValue(playerModule.ActiveObject, out var poModule)) return;

        var abilityInput = self.IsReviveKeybindPressed(playerModule);

        if (effect.MajorEffect != MajorEffectType.REVIVE || !abilityInput)
        {
            playerModule.ReviveTimer = 0;
            return;
        }

        if (poModule.CooldownTimer != 0) return;

        bool shouldResetRevive = true;

        foreach (var grasp in self.grasps)
        {
            if (grasp?.grabbed is not Creature creature) continue;

            // maybe i dunno
            if (!creature.dead && !creature.State.dead && !(creature is Player deadPlayer && (deadPlayer.playerState.dead || deadPlayer.playerState.permaDead))) continue;

            self.Blink(5);

            if (playerModule.ReviveTimer % 3 == 0)
                playerModule.ActiveObject.realizedObject.ConnectEffect(creature.firstChunk.pos);

            if (playerModule.ReviveTimer > 200)
            {
                poModule.CooldownTimer = -1;

                if (creature is Player player)
                    player.RevivePlayer();
                
                else
                    creature.Revive();  
            }

            shouldResetRevive = false;
            playerModule.ReviveTimer++;
            playerModule.BlockInput = true;
            break;
        }

        if (shouldResetRevive)
            playerModule.ReviveTimer = 0;
        
        else
            playerModule.ReviveTimer++;
    }

    public static void UpdateShield(Player self, PlayerModule playerModule, POEffect effect)
    {
        if (playerModule.ShieldTimer > 0)
        {
            self.AllGraspsLetGoOfThisObject(false);
            playerModule.ShieldTimer--;

            playerModule.ShieldAlpha = Mathf.Lerp(playerModule.ShieldAlpha, 1.0f, 0.25f);
            playerModule.ShieldScale = Mathf.Lerp(playerModule.ShieldScale, 6.0f, 0.4f);
            
            if (playerModule.ShieldTimer % 3 == 0)
            {
                foreach (var item in playerModule.Inventory)
                {
                    var itemEffect = item.GetPOEffect();

                    if (!item.TryGetModule(out var module)) continue;

                    if (module.CooldownTimer != 0) continue;

                    if (itemEffect.MajorEffect == MajorEffectType.SHIELD)
                        item.realizedObject.ConnectEffect(self.firstChunk.pos);
                }
            }

            if (playerModule.ShieldTimer == 0)
                self.room?.PlaySound(Enums.Sounds.Pearlcat_ShieldOff, self.firstChunk);
        }
        else
        {
            playerModule.ShieldAlpha = Mathf.Lerp(playerModule.ShieldAlpha, 0.0f, 0.25f);
            playerModule.ShieldScale = Mathf.Lerp(playerModule.ShieldScale, 0.0f, 0.4f);
        }

        if (self.room == null) return;
        
        var roomObjects = self.room.physicalObjects;
        bool didDeflect = false;

        if (playerModule.ShieldActive)
        {
            for (int i = roomObjects.Length - 1; i >= 0; i--)
            {
                List<PhysicalObject>? roomObject = roomObjects[i];
                
                for (int j = roomObject.Count - 1; j >= 0; j--)
                {
                    PhysicalObject? physicalObject = roomObject[j];
             
                    if (physicalObject is not Weapon weapon) continue;

                    if (weapon.thrownBy == self) continue;

                    if (weapon.thrownBy is Player playerThrownBy && (!self.room.game.rainWorld.options.friendlyFire || playerThrownBy.onBack == self)) continue;

                    if (weapon.mode == Weapon.Mode.Thrown && Custom.Dist(weapon.firstChunk.pos, self.firstChunk.pos) < 50.0f)
                    {
                        weapon.ChangeMode(Weapon.Mode.Free);
                        weapon.SetRandomSpin();
                        weapon.firstChunk.vel *= -0.2f;

                        weapon.room.DeflectEffect(weapon.firstChunk.pos);
                        didDeflect = true;
                    }
                }
            }
        }

        if (didDeflect)
            playerModule.ActivateVisualShield();
    }
    
    public static void UpdateRage(Player self, PlayerModule playerModule, POEffect effect)
    {
        var shootTime = 60;
        var shootDamage = 0.2f;

        var ragePearlCounter = 0;

        foreach (var item in playerModule.Inventory)
        {
            if (!item.TryGetModule(out var module)) continue;

            var itemEffect = item.GetPOEffect();

            if (itemEffect.MajorEffect != MajorEffectType.RAGE) continue;

            module.LaserLerp = 0.0f;

            if (effect.MajorEffect != MajorEffectType.RAGE || playerModule.RageTarget == null || !playerModule.RageTarget.TryGetTarget(out _))
                module.LaserTimer = shootTime + ragePearlCounter * 5;

            ragePearlCounter++;
        }

        if (ModOptions.DisableRage.Value || self.inVoidSea) return;

        if (effect.MajorEffect != MajorEffectType.RAGE) return;

        if (self.room == null) return;

        if (!self.Consious) return;


        var playerRoom = self.room;
        
        // search for target
        if (playerModule.RageTarget == null || !playerModule.RageTarget.TryGetTarget(out var target))
        {
            Creature? bestTarget = null;
            var shortestDist = float.MaxValue;

            foreach (var roomObject in playerRoom.physicalObjects)
            {
                foreach (var physicalObject in roomObject)
                {
                    if (physicalObject is not Creature creature) continue;

                    if (creature is Cicada) continue;

                    if (creature is Centipede centipede && centipede.Small) continue;

                    if (!self.IsHostileToMe(creature) && !(self.room.roomSettings.name == "T1_CAR2" && creature is Fly)) continue;


                    if (creature.dead) continue;

                    if (creature.VisibilityBonus < 0.0f) continue;


                    var dist = Custom.Dist(creature.mainBodyChunk.pos, self.firstChunk.pos);

                    if (dist > 400.0f) continue;

                    if (dist > shortestDist) continue;


                    if (!self.room.VisualContact(self.mainBodyChunk.pos, creature.mainBodyChunk.pos)) continue;

                    shortestDist = dist;
                    bestTarget = creature;
                }
            }

            if (bestTarget != null)
            {
                playerModule.RageTarget = new(bestTarget);

                ragePearlCounter = 0;

                if (bestTarget is Spider)
                {
                    foreach (var item in playerModule.Inventory)
                    {
                        if (!item.TryGetModule(out var module)) continue;

                        var itemEffect = item.GetPOEffect();

                        if (itemEffect.MajorEffect != MajorEffectType.RAGE) continue;

                        module.LaserTimer = 7 + 3 * ragePearlCounter;
                        ragePearlCounter++;
                    }
                }
                    
            }
        }
        else
        {
            // ensure target is still valid
            bool invalidTarget = false;

            if (!Custom.DistLess(target.mainBodyChunk.pos, self.mainBodyChunk.pos, 500.0f))
                invalidTarget = true;

            if (target.room != self.room)
                invalidTarget = true;

            if (target.dead)
                invalidTarget = true;

            if (!self.room.VisualContact(self.mainBodyChunk.pos, target.mainBodyChunk.pos))
                invalidTarget = true;


            if (invalidTarget)
                playerModule.RageTarget = null;
        }


        if (playerModule.RageTarget == null || !playerModule.RageTarget.TryGetTarget(out target)) return;

        foreach (var item in playerModule.Inventory)
        {
            if (!item.TryGetModule(out var module)) continue;

            if (!item.TryGetAddon(out var addon)) continue;


            var itemEffect = item.GetPOEffect();

            if (itemEffect.MajorEffect != MajorEffectType.RAGE) continue;

            if (module.CooldownTimer > 0)
            {
                module.LaserTimer = shootTime;
                continue;
            }

            if (module.LaserTimer <= 0)
            {
                module.CooldownTimer = shootTime;

                var targetPos = target.mainBodyChunk.pos;

                // shoot laser
                self.room.PlaySound(SoundID.Bomb_Explode, targetPos, 0.8f, Random.Range(0.7f, 1.3f));
                self.room.AddObject(new LightningMachine.Impact(targetPos, 0.5f, addon.SymbolColor, true));

                self.room.AddObject(new ShockWave(targetPos, 30.0f, 0.4f, 5, false));
                self.room.AddObject(new ExplosionSpikes(self.room, targetPos, 5, 20.0f, 10, 20.0f, 20.0f, addon.SymbolColor));

                target.Violence(self.firstChunk, null, self.mainBodyChunk, null, Creature.DamageType.Explosion, shootDamage, 5.0f);
            }
            else
            {
                module.LaserTimer--;
            }

            module.LaserLerp = Custom.LerpMap(module.LaserTimer, shootTime, 0, 0.0f, 1.0f);
        }
    }

    public static void UpdateCamoflague(Player self, PlayerModule playerModule, POEffect effect)
    {
        if (ModOptions.DisableCamoflague.Value || self.inVoidSea) return;

        var camera = self.abstractCreature.world.game.cameras[0];
        List<Color> samples = new()
        {
            camera.PixelColorAtCoordinate(self.firstChunk.pos),

            camera.PixelColorAtCoordinate(self.firstChunk.pos + new Vector2(10.0f, 10.0f)),
            camera.PixelColorAtCoordinate(self.firstChunk.pos + new Vector2(-10.0f, -10.0f)),
            camera.PixelColorAtCoordinate(self.firstChunk.pos + new Vector2(10.0f, -10.0f)),
            camera.PixelColorAtCoordinate(self.firstChunk.pos + new Vector2(-10.0f, 10.0f)),

            camera.PixelColorAtCoordinate(self.firstChunk.pos + new Vector2(0.0f, 10.0f)),
            camera.PixelColorAtCoordinate(self.firstChunk.pos + new Vector2(10.0f, 0.0f)),
            camera.PixelColorAtCoordinate(self.firstChunk.pos + new Vector2(0.0f, -10.0f)),
            camera.PixelColorAtCoordinate(self.firstChunk.pos + new Vector2(-10.0f, 0.0f)),
        };

        var totalColor = Color.black;

        foreach (var color in samples)
            totalColor += color;

        playerModule.CamoColor = totalColor / samples.Count;


        var camoSpeed = Custom.LerpMap(playerModule.CamoCount, 1, 5, 0.001f, 0.01f);
        var camoMaxMoveSpeed = Custom.LerpMap(playerModule.CamoCount, 1, 5, 2.0f, 10.0f);

        bool shouldCamo = (((self.canJump > 0  || self.bodyMode == Player.BodyModeIndex.ClimbingOnBeam || self.bodyMode == Player.BodyModeIndex.CorridorClimb) 
            && self.firstChunk.vel.magnitude < camoMaxMoveSpeed) || self.bodyMode == Player.BodyModeIndex.Crawl)
            && effect.MajorEffect == MajorEffectType.CAMOFLAGUE && playerModule.StoreObjectTimer <= 0;

        var prevCamo = playerModule.CamoLerp;

        playerModule.CamoLerp = shouldCamo ? Custom.LerpAndTick(playerModule.CamoLerp, 1.0f, 0.1f, camoSpeed) : Custom.LerpAndTick(playerModule.CamoLerp, 0.0f, 0.1f, camoSpeed);

        if (shouldCamo && prevCamo < 0.9f && playerModule.CamoLerp > 0.9f)
        {
            self.room?.PlaySound(Enums.Sounds.Pearlcat_PearlAbstract, self.firstChunk, false, 0.5f, Random.Range(0.8f, 1.2f));
        }
        else if (!shouldCamo && prevCamo > 0.9 && playerModule.CamoLerp < 0.9f)
        {
            self.room?.PlaySound(Enums.Sounds.Pearlcat_PearlRealize, self.firstChunk, false, 0.7f, Random.Range(0.8f, 1.2f));
        }
    }
}
