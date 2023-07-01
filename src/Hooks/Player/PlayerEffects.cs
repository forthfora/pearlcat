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

        foreach (var playerObject in playerModule.AbstractInventory)
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

            combinedEffect.LoudnessFac += effect.LoudnessFac * mult;
            combinedEffect.GeneralVisibilityBonus += effect.GeneralVisibilityBonus * mult;
            combinedEffect.VisualStealthInSneakMode += effect.VisualStealthInSneakMode * mult;

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

            stats.loudnessFac = baseStats.loudnessFac + effect.LoudnessFac;
            stats.generalVisibilityBonus = baseStats.generalVisibilityBonus + effect.GeneralVisibilityBonus;
            stats.visualStealthInSneakMode = baseStats.visualStealthInSneakMode + effect.VisualStealthInSneakMode;

            playerModule.CanMaul = effect.MaulFac >= 1.0;
        }


        switch (effect.majorEffect)
        {
            case POEffect.MajorEffect.SPEAR_CREATION:
                UpdateSpearCreation(self, playerModule);
                break;

            case POEffect.MajorEffect.AGILITY:
                UpdateAgility(self, playerModule);
                break;

            case POEffect.MajorEffect.REVIVE:
                UpdateRevive(self, playerModule);
                break;

            case POEffect.MajorEffect.SHIELD:
                UpdateShield(self, playerModule);
                break;

            case POEffect.MajorEffect.RAGE:
                UpdateRage(self, playerModule);
                break;

            case POEffect.MajorEffect.CAMOFLAGUE:
                UpdateCamoflague(self, playerModule);
                break;
        } 
    }


    public static void UpdateSpearCreation(Player self, PlayerModule playerModule)
    {
        if (PearlcatOptions.DisableSpear.Value) return;

        var abilityInput = self.IsAbilityKeybindPressed();
        var wasAbilityInput = playerModule.WasAbilityInput;
    }

    public static void UpdateAgility(Player self, PlayerModule playerModule)
    {
        if (PearlcatOptions.DisableAgility.Value) return;
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
