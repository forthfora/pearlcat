using RWCustom;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using static Pearlcat.POEffect;

namespace Pearlcat;

public static partial class Hooks
{
    public static void ApplyPlayerEffectsHooks()
    {
    }

    public static void UpdateCombinedPOEffect(Player self, PlayerModule playerModule)
    {
        POEffect combinedEffect = new();

        foreach (var playerObject in playerModule.Inventory)
        {
            var effect = playerObject.GetPOEffect();
            var mult = playerObject == playerModule.ActiveObject ? effect.ActiveMultiplier : 1.0f;

            if (self.Malnourished)
                mult *= 0.75f;

            combinedEffect.RunSpeedFac += effect.RunSpeedFac * mult;
            combinedEffect.CorridorClimbSpeedFac += effect.CorridorClimbSpeedFac * mult;
            combinedEffect.PoleClimbSpeedFac += effect.PoleClimbSpeedFac * mult;

            combinedEffect.ThrowingSkill += effect.ThrowingSkill * mult;
            combinedEffect.LungsFac += effect.LungsFac * mult;
            combinedEffect.BodyWeightFac += effect.BodyWeightFac * mult;

            combinedEffect.MaulFac += effect.MaulFac * mult;
            combinedEffect.SpearPullFac += effect.SpearPullFac * mult;
            combinedEffect.BackSpearFac += effect.BackSpearFac * mult;
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
        }
        else
        {
            stats.lungsFac = baseStats.lungsFac + effect.LungsFac;
            stats.throwingSkill = (int)Mathf.Clamp(baseStats.throwingSkill + effect.ThrowingSkill, 0, 2);
            stats.runspeedFac = baseStats.runspeedFac + effect.RunSpeedFac;

            stats.corridorClimbSpeedFac = baseStats.corridorClimbSpeedFac + effect.CorridorClimbSpeedFac;
            stats.poleClimbSpeedFac = baseStats.poleClimbSpeedFac + effect.PoleClimbSpeedFac;
            stats.bodyWeightFac = baseStats.bodyWeightFac + effect.BodyWeightFac;

            playerModule.CanMaul = effect.MaulFac >= 1.0;
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
    }


    public static void UpdateSpearCreation(Player self, PlayerModule playerModule, POEffect effect)
    {
        var spearCreationTime = 40.0f;
        playerModule.SpearLerp = Custom.LerpMap(playerModule.SpearTimer, 10, spearCreationTime, 0.0f, 1.0f);
        
        if (ModOptions.DisableSpear.Value) return;

        if (playerModule.ActiveObject == null || !PlayerObjectData.TryGetValue(playerModule.ActiveObject, out var poModule)) return;


        if (effect.MajorEffect != MajorEffectType.SPEAR_CREATION)
        {
            playerModule.SpearTimer = 0;
            return;
        }

        if (poModule.CooldownTimer > 0) return;


        var abilityInput = self.IsAbilityKeybindPressed(playerModule);

        var holdingSpear = self.GraspsHasType(AbstractPhysicalObject.AbstractObjectType.Spear) >= 0;
    
        if (abilityInput && (!self.spearOnBack.HasASpear || !holdingSpear))
        {
            playerModule.SpearTimer++;
            self.Blink(5);

            if (playerModule.SpearTimer > spearCreationTime)
            {
                playerModule.SpearTimer = 0;
                poModule.CooldownTimer = 100;

                var abstractSpear = new AbstractSpear(self.room.world, null, self.room.GetWorldCoordinate(self.mainBodyChunk.pos), self.room.game.GetNewID(), false);
                self.room.abstractRoom.AddEntity(abstractSpear);
                abstractSpear.pos = self.abstractCreature.pos;
                abstractSpear.RealizeInRoom();

                var save = self.abstractCreature.Room.world.game.GetMiscWorld();
                var spearModule = new SpearModule()
                {
                    Color = playerModule.ActiveColor,
                };

                save.PearlSpears.Add(abstractSpear.ID.number, spearModule);

                if (holdingSpear)
                {
                    self.spearOnBack.SpearToBack((Spear)abstractSpear.realizedObject);
                }
                else
                {
                    self.SlugcatGrab(abstractSpear.realizedObject, self.FreeHand());
                }

                ConnectEffect(playerModule.ActiveObject.realizedObject, abstractSpear.realizedObject.firstChunk.pos);
            }
        }
        else
        {
            playerModule.SpearTimer = 0;
        }
    }

    public static Color SpearColorFilter(this Color color) => color * Custom.HSL2RGB(1.0f, 0.7f, 1.5f);


    public static void UpdateAgility(Player self, PlayerModule playerModule, POEffect effect)
    {
        if (ModOptions.DisableAgility.Value) return;

        if (playerModule.ActiveObject == null || !PlayerObjectData.TryGetValue(playerModule.ActiveObject, out var poModule)) return;

        if (effect.MajorEffect != MajorEffectType.AGILITY) return;

        var abilityInput = self.IsDoubleJumpKeybindPressed(playerModule);
        var wasAbilityInput = playerModule.WasDJInput;

        poModule.CooldownTimer = poModule.UsedAgility ? 40 : 0;
        
        bool canUseAbility = !poModule.UsedAgility
            && self.canJump <= 0 && !(self.eatMeat >= 20 || self.maulTimer >= 15)
            && self.Consious && self.bodyMode != Player.BodyModeIndex.Crawl
            && self.bodyMode != Player.BodyModeIndex.CorridorClimb && self.bodyMode != Player.BodyModeIndex.ClimbIntoShortCut
            && self.animation != Player.AnimationIndex.HangFromBeam && self.animation != Player.AnimationIndex.ClimbOnBeam
            && self.bodyMode != Player.BodyModeIndex.WallClimb && self.bodyMode != Player.BodyModeIndex.Swimming
            && self.animation != Player.AnimationIndex.AntlerClimb && self.animation != Player.AnimationIndex.VineGrab
            && self.animation != Player.AnimationIndex.ZeroGPoleGrab && self.onBack == null;

        if (abilityInput && !wasAbilityInput && canUseAbility)
        {
            poModule.CooldownTimer = 20;

            poModule.UsedAgility = true;
            self.noGrabCounter = 5;
            var pos = self.firstChunk.pos;

            self.room.AddObject(new Explosion.ExplosionLight(pos, 160f, 1f, 3, Color.white));

            for (int j = 0; j < 10; j++)
            {
                Vector2 a = Custom.RNV();
                self.room.AddObject(new Spark(pos + a * Random.value * 40f, a * Mathf.Lerp(4f, 30f, Random.value), Color.white, null, 4, 18));
            }

            self.room.PlaySound(SoundID.Fire_Spear_Explode, pos, 0.15f + Random.value * 0.15f, 0.5f + Random.value * 2f);

            var velocityMult = 1.0f;

            if (self.bodyMode == Player.BodyModeIndex.ZeroG || self.room.gravity == 0f || self.gravity == 0f)
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

            if (playerModule.ActiveObject != null)
                self.ConnectEffect(targetPos, GetObjectColor(playerModule.ActiveObject));
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
            poModule.UsedAgility = false;
        }
    }
    
    public static void UpdateRevive(Player self, PlayerModule playerModule, POEffect effect)
    {
        if (ModOptions.DisableRevive.Value) return;

        if (playerModule.ActiveObject == null || !PlayerObjectData.TryGetValue(playerModule.ActiveObject, out var poModule)) return;

        var abilityInput = self.IsAbilityKeybindPressed(playerModule);

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

            if (!creature.dead) continue;


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
            break;
        }

        if (shouldResetRevive)
            playerModule.ReviveTimer = 0;
        
        else
            playerModule.ReviveTimer++;
    }

    public static void UpdateShield(Player self, PlayerModule playerModule, POEffect effect)
    {
        if (ModOptions.DisableShield.Value) return;

        if (playerModule.ActiveObject == null || !PlayerObjectData.TryGetValue(playerModule.ActiveObject, out var poModule)) return;

        if (playerModule.ShieldTimer > 0)
        {
            playerModule.ShieldTimer--;

            playerModule.ShieldAlpha = Mathf.Lerp(playerModule.ShieldAlpha, 1.0f, 0.25f);
            playerModule.ShieldScale = Mathf.Lerp(playerModule.ShieldScale, 6.0f, 0.4f);
            
            if (playerModule.ShieldTimer % 3 == 0)
                playerModule.ActiveObject.realizedObject.ConnectEffect(self.firstChunk.pos);

            var roomObjects = self.room.physicalObjects;

            foreach (var roomObject in roomObjects)
            {
                foreach (var physicalObject in roomObject)
                {
                    if (physicalObject is not Weapon weapon) continue;

                    if (weapon.thrownBy == self) continue;

                    if (weapon.mode == Weapon.Mode.Thrown && Custom.Dist(weapon.firstChunk.pos, self.firstChunk.pos) < 50.0f)
                    {
                        weapon.ChangeMode(Weapon.Mode.Free);
                        weapon.SetRandomSpin();
                        weapon.firstChunk.vel *= -0.2f;

                        weapon.room.DeflectEffect(weapon.firstChunk.pos);
                        playerModule.ReduceShieldTimer();
                    }
                }
            }
        }
        else
        {
            playerModule.ShieldAlpha = Mathf.Lerp(playerModule.ShieldAlpha, 0.0f, 0.25f);
            playerModule.ShieldScale = Mathf.Lerp(playerModule.ShieldScale, 0.0f, 0.4f);
        }


        if (effect.MajorEffect != MajorEffectType.SHIELD)
        {
            if (playerModule.ShieldTimer > 80)
                playerModule.ShieldTimer = 80;

            return;
        }
        
        var abilityInput = self.IsAbilityKeybindPressed(playerModule);
        var wasAbilityInput = playerModule.WasAbilityInput;


        if (abilityInput && !wasAbilityInput && poModule.CooldownTimer == 0)
        {
            playerModule.ShieldTimer = 200;
            poModule.CooldownTimer = playerModule.ShieldTimer + 300;
        }
    }
    
    public static void UpdateRage(Player self, PlayerModule playerModule, POEffect effect)
    {
        if (ModOptions.DisableRage.Value) return;

        if (effect.MajorEffect != MajorEffectType.RAGE) return;
    }

    public static void UpdateCamoflague(Player self, PlayerModule playerModule, POEffect effect)
    {
        if (ModOptions.DisableCamoflague.Value) return;

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

        
        bool shouldCamo = ((self.canJump > 0 && self.firstChunk.vel.magnitude < 2.0f)
            || self.bodyMode == Player.BodyModeIndex.Crawl) && effect.MajorEffect == MajorEffectType.CAMOFLAGUE
            && playerModule.StoreObjectTimer <= 0;

        playerModule.CamoLerp = shouldCamo ? Custom.LerpAndTick(playerModule.CamoLerp, 1.0f, 0.1f, 0.001f) : Custom.LerpAndTick(playerModule.CamoLerp, 0.0f, 0.1f, 0.001f);
    }

    Type
}
