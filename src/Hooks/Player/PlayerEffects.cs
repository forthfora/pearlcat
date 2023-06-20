
using UnityEngine;

namespace Pearlcat;

public static partial class Hooks
{
    public static void ApplyPlayerEffectsHooks()
    {
        
    }

    public static void UpdateCombinedPOEffect(Player self, PearlcatModule playerModule)
    {
        POEffect combinedEffect = new();

        foreach (var playerObject in playerModule.abstractInventory)
        {
            var effect = playerObject.GetPOEffect();
            var mult = playerObject == playerModule.ActiveObject ? effect.activeMultiplier : 1.0f;

            if (self.Malnourished)
                mult *= 0.75f;

            combinedEffect.runSpeedFac += effect.runSpeedFac * mult;
            combinedEffect.corridorClimbSpeedFac += effect.corridorClimbSpeedFac * mult;
            combinedEffect.poleClimbSpeedFac += effect.poleClimbSpeedFac * mult;

            combinedEffect.throwingSkill += effect.throwingSkill * mult;
            combinedEffect.lungsFac += effect.lungsFac * mult;
            combinedEffect.bodyWeightFac += effect.bodyWeightFac * mult;

            combinedEffect.loudnessFac += effect.loudnessFac * mult;
            combinedEffect.generalVisibilityBonus += effect.generalVisibilityBonus * mult;
            combinedEffect.visualStealthInSneakMode += effect.visualStealthInSneakMode * mult;
        }

        playerModule.currentPOEffect = combinedEffect;
    }

    public static void ApplyCombinedPOEffect(Player self, PearlcatModule playerModule)
    {
        var effect = playerModule.currentPOEffect;
        var stats = self.slugcatStats;
        var baseStats = playerModule.baseStats;

        stats.lungsFac = baseStats.lungsFac + effect.lungsFac;
        stats.throwingSkill = (int)Mathf.Clamp(baseStats.throwingSkill + effect.throwingSkill, 0, 2);
        stats.runspeedFac = baseStats.runspeedFac + effect.runSpeedFac;

        stats.corridorClimbSpeedFac = baseStats.corridorClimbSpeedFac + effect.corridorClimbSpeedFac;
        stats.poleClimbSpeedFac = baseStats.poleClimbSpeedFac + effect.poleClimbSpeedFac;
        stats.bodyWeightFac = baseStats.bodyWeightFac + effect.bodyWeightFac;

        stats.loudnessFac = baseStats.loudnessFac + effect.loudnessFac;
        stats.generalVisibilityBonus = baseStats.generalVisibilityBonus + effect.generalVisibilityBonus;
        stats.visualStealthInSneakMode = baseStats.visualStealthInSneakMode + effect.visualStealthInSneakMode;
    }

}
