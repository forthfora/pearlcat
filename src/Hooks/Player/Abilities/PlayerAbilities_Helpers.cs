using UnityEngine;

namespace Pearlcat;

public static class PlayerAbilities_Helpers
{
    public static void UpdatePearlEffects(Player self, PlayerModule playerModule)
    {
        var combinedEffect = new PearlEffect();

        foreach (var playerPearl in playerModule.Inventory)
        {
            var effect = playerPearl.GetPearlEffect();
            var mult = playerPearl == playerModule.ActiveObject ? effect.ActiveMultiplier : 1.0f;

            combinedEffect.ThrowingSkill += effect.ThrowingSkill;

            combinedEffect.RunSpeedFac += effect.RunSpeedFac * mult;
            combinedEffect.CorridorClimbSpeedFac += effect.CorridorClimbSpeedFac * mult;
            combinedEffect.PoleClimbSpeedFac += effect.PoleClimbSpeedFac * mult;

            combinedEffect.LungsFac += effect.LungsFac * mult;
            combinedEffect.BodyWeightFac += effect.BodyWeightFac * mult;
        }

        if (playerModule.ActiveObject is not null)
        {
            var effect = playerModule.ActiveObject.GetPearlEffect();
            combinedEffect.MajorEffect = effect.MajorEffect;
        }

        playerModule.CurrentPearlEffect = combinedEffect;

        ApplyPearlEffects(self, playerModule);
    }

    public static void ApplyPearlEffects(Player self, PlayerModule playerModule)
    {
        var effect = playerModule.CurrentPearlEffect;
        var stats = self.slugcatStats;
        var baseStats = playerModule.BaseStats;

        if (ModOptions.DisableMinorEffects)
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

            stats.lungsFac = Mathf.Clamp(baseStats.lungsFac + effect.LungsFac, 0.01f, 2.5f);
            stats.runspeedFac = Mathf.Clamp(baseStats.runspeedFac + effect.RunSpeedFac, 0.5f, float.MaxValue);

            stats.corridorClimbSpeedFac = Mathf.Clamp(baseStats.corridorClimbSpeedFac + effect.CorridorClimbSpeedFac, 0.5f, float.MaxValue);
            stats.poleClimbSpeedFac = Mathf.Clamp(baseStats.poleClimbSpeedFac + effect.PoleClimbSpeedFac, 0.5f, float.MaxValue);
            stats.bodyWeightFac = Mathf.Clamp(baseStats.bodyWeightFac + effect.BodyWeightFac, 0.5f, float.MaxValue);
        }

        var visibilityMult = ModOptions.VisibilityMultiplier / 100.0f;

        stats.loudnessFac = baseStats.loudnessFac * visibilityMult;
        stats.visualStealthInSneakMode = baseStats.visualStealthInSneakMode * visibilityMult;
        stats.generalVisibilityBonus = 0.4f * visibilityMult;

        playerModule.DisabledEffects.Clear();


        PlayerAbilities_Helpers_SpearCreation.Update(self, playerModule, effect);
        PlayerAbilities_Helpers_Agility.Update(self, playerModule, effect);
        PlayerAbilities_Helpers_Revive.Update(self, playerModule, effect);
        PlayerAbilities_Helpers_Shield.Update(self, playerModule, effect);
        PlayerAbilities_Helpers_Rage.Update(self, playerModule, effect);
        PlayerAbilities_Helpers_Camouflage.Update(self, playerModule, effect);


        if (self.inVoidSea || !self.Consious || self.Sleeping || (self.controller is not null && ModCompat_Helpers.RainMeadow_IsMine(self.abstractPhysicalObject))) // Meadow uses the player controller, don't block in that case
        {
            return;
        }

        var activeObj = playerModule.ActiveObject;

        if (activeObj is null)
        {
            return;
        }

        var abilityInput = self.IsSentryKeybindPressed(playerModule);
        var wasAbilityInput = playerModule.WasSentryInput;

        if (abilityInput && !wasAbilityInput)
        {
            DeploySentry(self, activeObj);
        }
    }

    public static void DeploySentry(Player self, AbstractPhysicalObject activeObj)
    {
        if (!self.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        if (!activeObj.TryGetPlayerPearlModule(out var pearlModule))
        {
            return;
        }

        if (activeObj.IsHeartPearl() && playerModule.IsPossessingCreature)
        {
            Player_Helpers.ReleasePossession(self, playerModule);
        }
        else if (!pearlModule.IsReturningSentry)
        {
            if (!pearlModule.IsSentry)
            {
                pearlModule.IsSentry = true;
                self.room.AddObject(new PearlSentry(activeObj));
            }
            else
            {
                pearlModule.ReturnSentry(activeObj);
            }
        }
    }
}
