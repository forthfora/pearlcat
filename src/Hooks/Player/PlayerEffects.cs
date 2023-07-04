using RWCustom;
using UnityEngine;

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

        playerModule.CurrentPOEffect = combinedEffect;
    }

    public static void ApplyCombinedPOEffect(Player self, PlayerModule playerModule)
    {
        var effect = playerModule.CurrentPOEffect;
        var stats = self.slugcatStats;
        var baseStats = playerModule.BaseStats;
    
        if (PearlcatOptions.DisableMinorEffects.Value)
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


        var visibilityMult = PearlcatOptions.VisibilityMultiplier.Value / 100.0f;

        stats.loudnessFac = baseStats.loudnessFac * visibilityMult;
        stats.visualStealthInSneakMode = baseStats.visualStealthInSneakMode * visibilityMult;
        stats.generalVisibilityBonus = 0.4f * visibilityMult;


        switch (effect.MajorEffect)
        {
            case POEffect.MajorEffectType.SPEAR_CREATION:
                UpdateSpearCreation(self, playerModule);
                break;

            case POEffect.MajorEffectType.AGILITY:
                UpdateAgility(self, playerModule);
                break;

            case POEffect.MajorEffectType.REVIVE:
                UpdateRevive(self, playerModule);
                break;

            case POEffect.MajorEffectType.SHIELD:
                UpdateShield(self, playerModule);
                break;

            case POEffect.MajorEffectType.RAGE:
                UpdateRage(self, playerModule);
                break;

            case POEffect.MajorEffectType.CAMOFLAGUE:
                UpdateCamoflague(self, playerModule);
                break;
        } 
    }


    public static void UpdateSpearCreation(Player self, PlayerModule playerModule)
    {
        if (PearlcatOptions.DisableSpear.Value) return;
    }

    public static void UpdateAgility(Player self, PlayerModule playerModule)
    {
        if (PearlcatOptions.DisableAgility.Value) return;

        var abilityInput = self.IsAbilityKeybindPressed(playerModule);
        var wasAbilityInput = playerModule.WasAbilityInput;

        Plugin.Logger.LogWarning(abilityInput);
    
        if (abilityInput && !wasAbilityInput)
        {
            self.noGrabCounter = 5;

            Vector2 pos = self.firstChunk.pos;
            
            for (int i = 0; i < 8; i++)
                self.room.AddObject(new Explosion.ExplosionSmoke(pos, Custom.RNV() * 5f * Random.value, 1f));
         
            self.room.AddObject(new Explosion.ExplosionLight(pos, 160f, 1f, 3, Color.white));

            for (int j = 0; j < 10; j++)
            {
                Vector2 a = Custom.RNV();
                self.room.AddObject(new Spark(pos + a * Random.value * 40f, a * Mathf.Lerp(4f, 30f, Random.value), Color.white, null, 4, 18));
            }

            self.room.PlaySound(SoundID.Fire_Spear_Explode, pos, 0.3f + Random.value * 0.3f, 0.5f + Random.value * 2f);

            if (self.bodyMode == Player.BodyModeIndex.ZeroG || self.room.gravity == 0f || self.gravity == 0f)
            {
                float inputX = self.input[0].x;
                float num4 = self.input[0].y;

                while (inputX == 0f && num4 == 0f)
                {
                    inputX = ((double)Random.value <= 0.33) ? 0 : (((double)Random.value <= 0.5) ? 1 : -1);
                    num4 = ((double)Random.value <= 0.33) ? 0 : (((double)Random.value <= 0.5) ? 1 : -1);
                }

                self.bodyChunks[0].vel.x = 9f * inputX;
                self.bodyChunks[0].vel.y = 9f * num4;
                self.bodyChunks[1].vel.x = 8f * inputX;
                self.bodyChunks[1].vel.y = 8f * num4;
                self.pyroJumpCooldown = 150f;
                self.pyroJumpCounter++;
            }
            else
            {
                if (self.input[0].x != 0)
                {
                    self.bodyChunks[0].vel.y = Mathf.Min(self.bodyChunks[0].vel.y, 0f) + 8f;
                    self.bodyChunks[1].vel.y = Mathf.Min(self.bodyChunks[1].vel.y, 0f) + 7f;
                    self.jumpBoost = 6f;
                }

                if (self.input[0].x == 0 || self.input[0].y == 1)
                {
                    self.bodyChunks[0].vel.y = 11f;
                    self.bodyChunks[1].vel.y = 10f;
                    self.jumpBoost = 8f;
                }

                if (self.input[0].y == 1)
                {
                    self.bodyChunks[0].vel.x = 10f * self.input[0].x;
                    self.bodyChunks[1].vel.x = 8f * self.input[0].x;
                }
                else
                {
                    self.bodyChunks[0].vel.x = 15f * self.input[0].x;
                    self.bodyChunks[1].vel.x = 13f * self.input[0].x;
                }

                self.animation = Player.AnimationIndex.Flip;
                self.bodyMode = Player.BodyModeIndex.Default;
            }
        }
    }
    
    public static void UpdateRevive(Player self, PlayerModule playerModule)
    {
        if (PearlcatOptions.DisableRevive.Value) return;
    }
    
    public static void UpdateShield(Player self, PlayerModule playerModule)
    {
        if (PearlcatOptions.DisableShield.Value) return;
    }
    
    public static void UpdateRage(Player self, PlayerModule playerModule)
    {
        if (PearlcatOptions.DisableRage.Value) return;
    }

    public static void UpdateCamoflague(Player self, PlayerModule playerModule)
    {
        if (PearlcatOptions.DisableCamoflague.Value) return;
    }
}
