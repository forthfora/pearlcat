using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Pearlcat;

using DataPearlType = DataPearl.AbstractDataPearl.DataPearlType;
using OnlineOptions = MeadowOnlineOptions;
using OI = ModOptionInterface;

public static class ModOptions
{
    // Rotund World
    [PublicAPI]
    public static Configurable<bool> DisableCosmetics => OI.DisableCosmetics;

    public static bool IsRemote => !ModCompat_Helpers.RainMeadow_IsLobbyOwner;

    public static List<DataPearlType> GetOverridenInventory(bool giveHalcyonPearl)
    {
        List<DataPearlType> pearls = [];

        for (var i = 0; i < AgilityPearlCount; i++)
        {
            pearls.Add(Enums.Pearls.AS_PearlBlue);
        }

        for (var i = 0; i < ShieldPearlCount; i++)
        {
            pearls.Add(Enums.Pearls.AS_PearlYellow);
        }

        for (var i = 0; i < RevivePearlCount; i++)
        {
            pearls.Add(Enums.Pearls.AS_PearlGreen);
        }

        for (var i = 0; i < CamoPearlCount; i++)
        {
            pearls.Add(Enums.Pearls.AS_PearlBlack);
        }

        for (var i = 0; i < RagePearlCount; i++)
        {
            pearls.Add(Enums.Pearls.AS_PearlRed);
        }

        for (var i = 0; i < SpearPearlCount; i++)
        {
            pearls.Add(i == 0 && giveHalcyonPearl ? Enums.Pearls.RM_Pearlcat : DataPearlType.Misc);
        }

        return pearls;
    }


    // OPTIONS
    public static bool PearlpupRespawn => !ModCompat_Helpers.RainMeadow_IsOnline && OI.PearlpupRespawn.Value; // TODO: maybe reenable it for meadow when pups are fixed

    public static bool DisableCosmetics_New => OI.DisableCosmetics.Value;
    public static bool DisableTutorials => ModCompat_Helpers.RainMeadow_IsOnline || OI.DisableTutorials.Value;


    // INVENTORY COSMETICS
    public static bool HidePearls => OI.HidePearls.Value;

    public static bool InventoryPings => OI.InventoryPings.Value;
    public static bool CompactInventoryHUD => OI.CompactInventoryHUD.Value;


    // MISC GAMEPLAY
    public static int MaxPearlCount => IsRemote ? OnlineOptions.MaxPearlCount : OI.MaxPearlCount.Value;

    public static string StartShelterOverride => IsRemote ? OnlineOptions.StartShelterOverride : OI.StartShelterOverride.Value;
    public static int VisibilityMultiplier => IsRemote ? OnlineOptions.VisibilityMultiplier : OI.VisibilityMultiplier.Value;
    public static bool EnableBackSpear => IsRemote ? OnlineOptions.EnableBackSpear : OI.EnableBackSpear.Value;
    public static bool PearlThreatMusic => OI.PearlThreatMusic.Value;


    // DISABLE ABILITIES
    public static bool DisableMinorEffects => IsRemote ? OnlineOptions.DisableMinorEffects : OI.DisableMinorEffects.Value;

    public static bool DisableSpear => IsRemote ? OnlineOptions.DisableSpear : OI.DisableSpear.Value;
    public static bool DisableRevive => IsRemote ? OnlineOptions.DisableRevive : OI.DisableRevive.Value;
    public static bool DisableAgility => IsRemote ? OnlineOptions.DisableAgility : OI.DisableAgility.Value;
    public static bool DisableRage => IsRemote ? OnlineOptions.DisableRage : OI.DisableRage.Value;
    public static bool DisableShield => IsRemote ? OnlineOptions.DisableShield : OI.DisableShield.Value;
    public static bool DisableCamoflague => IsRemote ? OnlineOptions.DisableCamoflague : OI.DisableCamoflague.Value;


    // INVENTORY OVERRIDE
    public static bool InventoryOverride => IsRemote ? OnlineOptions.InventoryOverride : OI.InventoryOverride.Value;
    public static bool StartingInventoryOverride => IsRemote ? OnlineOptions.StartingInventoryOverride : OI.StartingInventoryOverride.Value;

    // Starting inventory override specifically gives the host authority over the counts
    private static bool UseRemoteInventoryCounts => IsRemote && StartingInventoryOverride && !InventoryOverride;
    public static int SpearPearlCount => UseRemoteInventoryCounts ? OnlineOptions.SpearPearlCount : OI.SpearPearlCount.Value;
    public static int RevivePearlCount => UseRemoteInventoryCounts ? OnlineOptions.RevivePearlCount : OI.RevivePearlCount.Value;
    public static int AgilityPearlCount => UseRemoteInventoryCounts ? OnlineOptions.AgilityPearlCount : OI.AgilityPearlCount.Value;
    public static int RagePearlCount => UseRemoteInventoryCounts ? OnlineOptions.RagePearlCount : OI.RagePearlCount.Value;
    public static int ShieldPearlCount => UseRemoteInventoryCounts ? OnlineOptions.ShieldPearlCount : OI.ShieldPearlCount.Value;
    public static int CamoPearlCount => UseRemoteInventoryCounts ? OnlineOptions.CamoPearlCount : OI.CamoPearlCount.Value;


    // ABILITY CONFIG
    public static int ShieldRechargeTime => IsRemote ? OnlineOptions.ShieldRechargeTime : OI.ShieldRechargeTime.Value;
    public static int ShieldDuration => IsRemote ? OnlineOptions.ShieldDuration : OI.ShieldDuration.Value;

    public static float LaserDamage => IsRemote ? OnlineOptions.LaserDamage : OI.LaserDamage.Value;
    public static int LaserWindupTime => IsRemote ? OnlineOptions.LaserWindupTime : OI.LaserWindupTime.Value;
    public static int LaserRechargeTime => IsRemote ? OnlineOptions.LaserRechargeTime : OI.LaserRechargeTime.Value;

    public static bool OldRedPearlAbility => IsRemote ? OnlineOptions.OldRedPearlAbility : OI.OldRedPearlAbility.Value;


    // KEYBIND OPTIONS
    public static bool DisableImprovedInputConfig => OI.DisableImprovedInputConfig.Value;


    // SWAP
    public static KeyCode SwapLeftKeybind => OI.SwapLeftKeybind.Value;
    public static KeyCode SwapRightKeybind => OI.SwapRightKeybind.Value;

    public static KeyCode SwapKeybindKeyboard => OI.SwapKeybindKeyboard.Value;

    public static KeyCode SwapKeybindPlayer1 => OI.SwapKeybindPlayer1.Value;
    public static KeyCode SwapKeybindPlayer2 => OI.SwapKeybindPlayer2.Value;
    public static KeyCode SwapKeybindPlayer3 => OI.SwapKeybindPlayer3.Value;
    public static KeyCode SwapKeybindPlayer4 => OI.SwapKeybindPlayer4.Value;

    public static int SwapTriggerPlayer => OI.SwapTriggerPlayer.Value;


    // ABILITY
    public static KeyCode AbilityKeybindKeyboard => OI.AbilityKeybindKeyboard.Value;

    public static KeyCode AbilityKeybindPlayer1 => OI.AbilityKeybindPlayer1.Value;
    public static KeyCode AbilityKeybindPlayer2 => OI.AbilityKeybindPlayer2.Value;
    public static KeyCode AbilityKeybindPlayer3 => OI.AbilityKeybindPlayer3.Value;
    public static KeyCode AbilityKeybindPlayer4 => OI.AbilityKeybindPlayer4.Value;

    public static bool CustomSpearKeybind => OI.CustomSpearKeybind.Value;
    public static bool CustomAgilityKeybind => OI.CustomAgilityKeybind.Value;


    // STORE
    public static KeyCode StoreKeybindKeyboard => OI.StoreKeybindKeyboard.Value;

    public static KeyCode StoreKeybindPlayer1 => OI.StoreKeybindPlayer1.Value;
    public static KeyCode StoreKeybindPlayer2 => OI.StoreKeybindPlayer2.Value;
    public static KeyCode StoreKeybindPlayer3 => OI.StoreKeybindPlayer3.Value;
    public static KeyCode StoreKeybindPlayer4 => OI.StoreKeybindPlayer4.Value;

    public static bool UsesCustomStoreKeybind => OI.UsesCustomStoreKeybind.Value;


    // SENTRY
    public static KeyCode SentryKeybindKeyboard => OI.SentryKeybindKeyboard.Value;

    public static KeyCode SentryKeybindPlayer1 => OI.SentryKeybindPlayer1.Value;
    public static KeyCode SentryKeybindPlayer2 => OI.SentryKeybindPlayer2.Value;
    public static KeyCode SentryKeybindPlayer3 => OI.SentryKeybindPlayer3.Value;
    public static KeyCode SentryKeybindPlayer4 => OI.SentryKeybindPlayer4.Value;

    public static bool CustomSentryKeybind => OI.CustomSentryKeybind.Value;
}
