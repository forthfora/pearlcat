using UnityEngine;

namespace Pearlcat;

using OnlineOptions = MeadowOnlineOptions;
using OI = ModOptionInterface;

public static class ModOptions
{
    public static bool IsOnline => !ModCompat_Helpers.RainMeadow_IsLobbyOwner;

    // OPTIONS
    public static bool PearlpupRespawn => IsOnline ? OnlineOptions.PearlpupRespawn : OI.PearlpupRespawn.Value;

    public static bool DisableCosmetics => OI.DisableCosmetics.Value;

    public static bool DisableTutorials => IsOnline || OI.DisableTutorials.Value;


    // INVENTORY COSMETICS
    public static bool HidePearls => OI.HidePearls.Value;

    public static bool InventoryPings => OI.InventoryPings.Value;

    public static bool CompactInventoryHUD => OI.CompactInventoryHUD.Value;


    // MISC GAMEPLAY
    public static int MaxPearlCount => IsOnline ? OnlineOptions.MaxPearlCount : OI.MaxPearlCount;

    public static string StartShelterOverride => IsOnline ? "" : OI.StartShelterOverride.Value;

    public static int VisibilityMultiplier => IsOnline ? OnlineOptions.VisibilityMultiplier : OI.VisibilityMultiplier.Value;

    public static bool EnableBackSpear => IsOnline ? OnlineOptions.EnableBackSpear : OI.EnableBackSpear.Value;

    public static bool PearlThreatMusic => OI.PearlThreatMusic.Value;


    // DISABLE ABILITIES
    public static bool DisableMinorEffects => IsOnline ? OnlineOptions.DisableMinorEffects : OI.DisableMinorEffects.Value;

    public static bool DisableSpear => IsOnline ? OnlineOptions.DisableSpear : OI.DisableSpear.Value;

    public static bool DisableRevive => IsOnline ? OnlineOptions.DisableRevive : OI.DisableRevive.Value;

    public static bool DisableAgility => IsOnline ? OnlineOptions.DisableAgility : OI.DisableAgility.Value;

    public static bool DisableRage => IsOnline ? OnlineOptions.DisableRage : OI.DisableRage.Value;

    public static bool DisableShield => IsOnline ? OnlineOptions.DisableShield : OI.DisableShield.Value;

    public static bool DisableCamoflague => IsOnline ? OnlineOptions.DisableCamoflague : OI.DisableCamoflague.Value;


    // INVENTORY OVERRIDE
    public static bool InventoryOverride => IsOnline ? OnlineOptions.InventoryOverride : OI.InventoryOverride.Value;

    public static bool StartingInventoryOverride => IsOnline ? OnlineOptions.StartingInventoryOverride : OI.StartingInventoryOverride.Value;


    public static int SpearPearlCount => OI.SpearPearlCount.Value;

    public static int RevivePearlCount => OI.RevivePearlCount.Value;

    public static int AgilityPearlCount => OI.AgilityPearlCount.Value;

    public static int RagePearlCount => OI.RagePearlCount.Value;

    public static int ShieldPearlCount => OI.ShieldPearlCount.Value;

    public static int CamoPearlCount => OI.CamoPearlCount.Value;


    // ABILITY CONFIG
    public static int ShieldRechargeTime => IsOnline ? OnlineOptions.ShieldRechargeTime : OI.ShieldRechargeTime.Value;

    public static int ShieldDuration => IsOnline ? OnlineOptions.ShieldDuration : OI.ShieldDuration.Value;



    public static float LaserDamage => IsOnline ? OnlineOptions.LaserDamage : OI.LaserDamage.Value;

    public static int LaserWindupTime => IsOnline ? OnlineOptions.LaserWindupTime : OI.LaserWindupTime.Value;

    public static int LaserRechargeTime => IsOnline ? OnlineOptions.LaserRechargeTime : OI.LaserRechargeTime.Value;


    public static bool OldRedPearlAbility => IsOnline ? OnlineOptions.OldRedPearlAbility : OI.OldRedPearlAbility.Value;


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

    public static bool CustomSentryKeybind => OI.CustomAgilityKeybind.Value;
}
