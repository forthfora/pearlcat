using System;
using Newtonsoft.Json;
using RainMeadow;

namespace Pearlcat;

public static class ModCompat_RainMeadow_Hooks
{
    public static void ApplyHooks()
    {
        On.Player.Update += PlayerOnUpdate;
    }

    private static void PlayerOnUpdate(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);

        if (!self.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        if (!self.IsLocal())
        {
            return;
        }

        // Sync Inputs
        playerModule.MeadowInput.Store = self.IsStoreKeybindPressed(playerModule);

        playerModule.MeadowInput.Swap = self.IsSwapKeybindPressed();
        playerModule.MeadowInput.SwapLeft = self.IsSwapLeftInput();
        playerModule.MeadowInput.SwapRight = self.IsSwapRightInput();

        playerModule.MeadowInput.Ability = self.IsCustomAbilityKeybindPressed();
        playerModule.MeadowInput.Semtry = self.IsSentryKeybindPressed(playerModule);

        playerModule.MeadowInput.Agility = self.IsAgilityKeybindPressed(playerModule);
        playerModule.MeadowInput.SpearCreation = self.IsSpearCreationKeybindPressed(playerModule);
        playerModule.MeadowInput.Revive = self.IsReviveKeybindPressed(playerModule);

        ModCompat_RainMeadow_Helpers.RPC_ReceivePearlcatInput(self, playerModule);
    }
}
