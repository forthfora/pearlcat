using System.Collections.Generic;
using UnityEngine;
using static DataPearl.AbstractDataPearl;
using static Pearlcat.Enums;

namespace Pearlcat;

public sealed partial class ModOptions : OptionsTemplate
{
    public static ModOptions Instance { get; } = new();

    public static void RegisterOI()
    {
        if (MachineConnector.GetRegisteredOI(Plugin.MOD_ID) != Instance)
        {
            MachineConnector.SetRegisteredOI(Plugin.MOD_ID, Instance);
        }
    }

    public static List<DataPearlType> GetOverridenInventory(bool giveHalcyonPearl)
    {
        List<DataPearlType> pearls = [];

        for (var i = 0; i < AgilityPearlCount.Value; i++)
        {
            pearls.Add(Pearls.AS_PearlBlue);
        }

        for (var i = 0; i < ShieldPearlCount.Value; i++)
        {
            pearls.Add(Pearls.AS_PearlYellow);
        }

        for (var i = 0; i < RevivePearlCount.Value; i++)
        {
            pearls.Add(Pearls.AS_PearlGreen);
        }

        for (var i = 0; i < CamoPearlCount.Value; i++)
        {
            pearls.Add(Pearls.AS_PearlBlack);
        }

        for (var i = 0; i < RagePearlCount.Value; i++)
        {
            pearls.Add(Pearls.AS_PearlRed);
        }

        for (var i = 0; i < SpearPearlCount.Value; i++)
        {
            pearls.Add(i == 0 && giveHalcyonPearl ? Pearls.RM_Pearlcat : DataPearlType.Misc);
        }

        return pearls;
    }


    // OPTIONS
    public static Configurable<bool> PearlpupRespawn { get; } = Instance.config.Bind(nameof(PearlpupRespawn), false, new ConfigurableInfo(
        "When checked, Pearlpup will respawn in the next shelter on the following cycle whenever they are lost.", null, "",
        "Pearlpup Respawn?"));

    public static Configurable<bool> DisableCosmetics { get; } = Instance.config.Bind(nameof(DisableCosmetics), false, new ConfigurableInfo(
        "When checked, Pearlcat's cosmetics will be disabled, intended to allow custom sprites via DMS. This does not include the pearls themselves.", null, "",
        "Disable Cosmetics?"));

    public static Configurable<bool> DisableTutorials { get; } = Instance.config.Bind(nameof(DisableTutorials), false, new ConfigurableInfo(
        "When checked, all tutorials will be disabled.", null, "",
        "Disable Tutorials?"));


    // INVENTORY COSMETICS
    public static Configurable<bool> HidePearls { get; } = Instance.config.Bind(nameof(HidePearls), false, new ConfigurableInfo(
        "Hides the visuals of inactive pearls and turns you into... cat.", null, "",
        "Hide Pearls?"));

    public static Configurable<bool> InventoryPings { get; } = Instance.config.Bind(nameof(InventoryPings), false, new ConfigurableInfo(
        "When checked, some abilties will show the inventory when recharged or depleted.", null, "",
        "Inventory Pings?"));

    public static Configurable<bool> CompactInventoryHUD { get; } = Instance.config.Bind(nameof(CompactInventoryHUD), false, new ConfigurableInfo(
        "When checked, the inventory HUD will be replaced with a more compact version.", null, "",
        "Compact Inventory HUD?"));


    // MISC GAMEPLAY
    public static Configurable<int> MaxPearlCount { get; } = Instance.config.Bind(nameof(MaxPearlCount), 9, new ConfigurableInfo(
        "Maximum number of pearls that can be stored at once, including the active pearl. Default is 9. Hold and drag up or down to change.",
        new ConfigAcceptableRange<int>(1, int.MaxValue), "",
        "Max Pearl Count"));

    public static Configurable<string> StartShelterOverride { get; } = Instance.config.Bind(nameof(StartShelterOverride), "", new ConfigurableInfo(
        "Input a shelter name to have it override where Pearlcat starts a new game.", null, "", "Start Shelter Override"));

    public static Configurable<int> VisibilityMultiplier { get; } = Instance.config.Bind(nameof(VisibilityMultiplier), 100, new ConfigurableInfo(
        "Percentage multiplier on Pearlcat's general visibility, influences predator attraction. By default, Pearlcat is significantly more visible than even Hunter.",
        new ConfigAcceptableRange<int>(0, 300), "",
        "Visibility Multiplier"));

    public static Configurable<bool> EnableBackSpear { get; } = Instance.config.Bind(nameof(EnableBackSpear), false, new ConfigurableInfo(
        "When checked, enables Pearlcat to carry a backspear.", null, "",
        "Enable Backspear?"));

    public static Configurable<bool> PearlThreatMusic { get; } = Instance.config.Bind(nameof(PearlThreatMusic), false, new ConfigurableInfo(
        "When checked, most pearls (when active) will force the threat theme for all regions to the theme of the region they were originally from.", null, "",
        "Pearl Threat Music?"));


    // DISABLE ABILITIES
    public static Configurable<bool> DisableMinorEffects { get; } = Instance.config.Bind(nameof(DisableMinorEffects), false, new ConfigurableInfo(
        "When checked, pearls will no longer grant stat changes, active or otherwise, and base stats are set to be similar to Hunter.", null, "",
        "Disable Minor Effects?"));

    public static Configurable<bool> DisableSpear { get; } = Instance.config.Bind(nameof(DisableSpear), false, new ConfigurableInfo(
        "When checked, disables the spear creation effect granted by an active pearl.", null, "",
        "Disable Spear Effect?"));

    public static Configurable<bool> DisableRevive { get; } = Instance.config.Bind(nameof(DisableRevive), false, new ConfigurableInfo(
        "When checked, disables the revive effect granted by an active pearl.", null, "",
        "Disable Revive Effect?"));

    public static Configurable<bool> DisableAgility { get; } = Instance.config.Bind(nameof(DisableAgility), false, new ConfigurableInfo(
        "When checked, disables the agility effect granted by an active pearl.", null, "",
        "Disable Agility Effect?"));

    public static Configurable<bool> DisableRage { get; } = Instance.config.Bind(nameof(DisableRage), false, new ConfigurableInfo(
        "When checked, disables the rage effect granted by an active pearl.", null, "",
        "Disable Rage Effect?"));

    public static Configurable<bool> DisableShield { get; } = Instance.config.Bind(nameof(DisableShield), false, new ConfigurableInfo(
        "When checked, disables the shield effect granted by an active pearl.", null, "",
        "Disable Shield Effect?"));

    public static Configurable<bool> DisableCamoflague { get; } = Instance.config.Bind(nameof(DisableCamoflague), false, new ConfigurableInfo(
        "When checked, disables the camoflague effect granted by an active pearl.", null, "",
        "Disable Camoflague Effect?"));


    // INVENTORY OVERRIDE
    public static Configurable<bool> InventoryOverride { get; } = Instance.config.Bind(nameof(InventoryOverride), false, new ConfigurableInfo(
        "When checked, sets the inventory to the specified numbers of coloured pearls below every cycle. Does not save over the current inventory - it is returned to when unchecked.", null, "",
        "Inventory Override?"));

    public static Configurable<bool> StartingInventoryOverride { get; } = Instance.config.Bind(nameof(StartingInventoryOverride), false, new ConfigurableInfo(
        "When checked, overrides the starting pearls with the option below. Only effective at the start of a new game, unlike Inventory Override?", null, "",
        "Starting Inventory Override?"));


    public static Configurable<int> SpearPearlCount { get; } = Instance.config.Bind(nameof(SpearPearlCount), 1, new ConfigurableInfo(
        "Number of spear creation pearls (white). Effective only when Inventory Override? is checked.",
        new ConfigAcceptableRange<int>(0, int.MaxValue), "",
        "Spear Pearl Count"));

    public static Configurable<int> RevivePearlCount { get; } = Instance.config.Bind(nameof(RevivePearlCount), 1, new ConfigurableInfo(
        "Number of revive pearls (green). Effective only when Inventory Override? is checked.",
        new ConfigAcceptableRange<int>(0, int.MaxValue), "",
        "Revive Pearl Count"));

    public static Configurable<int> AgilityPearlCount { get; } = Instance.config.Bind(nameof(AgilityPearlCount), 1, new ConfigurableInfo(
        "Number of agility pearls (blue). Effective only when Inventory Override? is checked.",
        new ConfigAcceptableRange<int>(0, int.MaxValue), "",
        "Agility Pearl Count"));

    public static Configurable<int> RagePearlCount { get; } = Instance.config.Bind(nameof(RagePearlCount), 1, new ConfigurableInfo(
        "Number of rage pearls (red). Effective only when Inventory Override? is checked.",
        new ConfigAcceptableRange<int>(0, int.MaxValue), "",
        "Rage Pearl Count"));

    public static Configurable<int> ShieldPearlCount { get; } = Instance.config.Bind(nameof(ShieldPearlCount), 1, new ConfigurableInfo(
        "Number of shield pearls (yellow). Effective only when Inventory Override? is checked.",
        new ConfigAcceptableRange<int>(0, int.MaxValue), "",
        "Shield Pearl Count"));

    public static Configurable<int> CamoPearlCount { get; } = Instance.config.Bind(nameof(CamoPearlCount), 1, new ConfigurableInfo(
        "Number of camo pearls (black). Effective only when Inventory Override? is checked.",
        new ConfigAcceptableRange<int>(0, int.MaxValue), "",
        "Camo Pearl Count"));


    // ABILITY CONFIG
    public static Configurable<int> ShieldRechargeTime { get; } = Instance.config.Bind(nameof(ShieldRechargeTime), 1600, new ConfigurableInfo(
        "Time in frames the yellow pearl shield take to recharge after activating. Default 40 seconds.",
        new ConfigAcceptableRange<int>(40, 3200), "",
        "Shield Recharge Time"));

    public static Configurable<int> ShieldDuration { get; } = Instance.config.Bind(nameof(ShieldDuration), 60, new ConfigurableInfo(
        "Time in frames the yellow pearl shield lasts after activating. Default 1.5 seconds.",
        new ConfigAcceptableRange<int>(5, 300), "",
        "Shield Duration"));



    public static Configurable<float> LaserDamage { get; } = Instance.config.Bind(nameof(LaserDamage), 0.2f, new ConfigurableInfo(
        "Damage each red pearl's laser does per shot. Survivor spear damage = 1.0",
        new ConfigAcceptableRange<float>(0.0f, 3.0f), "",
        "Laser Damage (OLD)"));

    public static Configurable<int> LaserWindupTime { get; } = Instance.config.Bind(nameof(LaserWindupTime), 60, new ConfigurableInfo(
        "Time in frames for a red pearl's laser to fire after acquiring a target. Default 1.5 seconds.",
        new ConfigAcceptableRange<int>(5, 300), "",
        "Laser Windup TIme (OLD)"));

    public static Configurable<int> LaserRechargeTime { get; } = Instance.config.Bind(nameof(LaserRechargeTime), 60, new ConfigurableInfo(
        "Time in frames for a red pearl's laser to recharge after firing. Default 1.5 seconds.",
        new ConfigAcceptableRange<int>(5, 300), "",
        "Laser Recharge Time (OLD)"));


    public static Configurable<bool> OldRedPearlAbility { get; } = Instance.config.Bind(nameof(OldRedPearlAbility), false, new ConfigurableInfo(
        "Reverts to the old red pearl mechanics - auto targeting lasers.", null, "",
        "Old Red Pearl Ability?"));


    // KEYBIND OPTIONS
    public static Configurable<bool> DisableImprovedInputConfig { get; } = Instance.config.Bind(nameof(DisableImprovedInputConfig), false, new ConfigurableInfo(
        "When checked, disables improved input config support, reverting to Remix config. Exit and re-enter the Remix menu to take effect.", null, "",
        "Disable Improved Input Config?"));


    // SWAP
    public static Configurable<KeyCode> SwapLeftKeybind { get; } = Instance.config.Bind(nameof(SwapLeftKeybind), KeyCode.A, new ConfigurableInfo(
        "Keybind to swap to the stored pearl to the left. Limited to Player 1.", null, "", "Swap Left"));

    public static Configurable<KeyCode> SwapRightKeybind { get; } = Instance.config.Bind(nameof(SwapRightKeybind), KeyCode.D, new ConfigurableInfo(
        "Keybind to swap to the stored pearl to the right. Limited to Player 1.", null, "", "Swap Right"));


    public static Configurable<KeyCode> SwapKeybindKeyboard { get; } = Instance.config.Bind(nameof(SwapKeybindKeyboard), KeyCode.LeftAlt, new ConfigurableInfo(
        "Keybind for Keyboard.", null, "", "Keyboard"));

    public static Configurable<KeyCode> SwapKeybindPlayer1 { get; } = Instance.config.Bind(nameof(SwapKeybindPlayer1), KeyCode.Joystick1Button3, new ConfigurableInfo(
        "Keybind for Player 1.", null, "", "Player 1"));

    public static Configurable<KeyCode> SwapKeybindPlayer2 { get; } = Instance.config.Bind(nameof(SwapKeybindPlayer2), KeyCode.Joystick2Button3, new ConfigurableInfo(
        "Keybind for Player 2.", null, "", "Player 2"));

    public static Configurable<KeyCode> SwapKeybindPlayer3 { get; } = Instance.config.Bind(nameof(SwapKeybindPlayer3), KeyCode.Joystick3Button3, new ConfigurableInfo(
        "Keybind for Player 3.", null, "", "Player 3"));

    public static Configurable<KeyCode> SwapKeybindPlayer4 { get; } = Instance.config.Bind(nameof(SwapKeybindPlayer4), KeyCode.Joystick4Button3, new ConfigurableInfo(
        "Keybind for Player 4.", null, "", "Player 4"));


    public static Configurable<int> SwapTriggerPlayer { get; } = Instance.config.Bind(nameof(SwapTriggerPlayer), 1, new ConfigurableInfo(
        "Which player controller trigger swapping will apply to. 0 disables trigger swapping. Hold and drag up or down to change.",
        new ConfigAcceptableRange<int>(0, 4), "",
        "Trigger Player"));


    // ABILITY
    public static Configurable<KeyCode> AbilityKeybindKeyboard { get; } = Instance.config.Bind(nameof(AbilityKeybindKeyboard), KeyCode.C, new ConfigurableInfo(
        "Keybind for Keyboard.", null, "", "Ability KB"));

    public static Configurable<KeyCode> AbilityKeybindPlayer1 { get; } = Instance.config.Bind(nameof(AbilityKeybindPlayer1), KeyCode.Joystick1Button4, new ConfigurableInfo(
        "Keybind for Player 1.", null, "", "Ability P1"));

    public static Configurable<KeyCode> AbilityKeybindPlayer2 { get; } = Instance.config.Bind(nameof(AbilityKeybindPlayer2), KeyCode.Joystick2Button4, new ConfigurableInfo(
        "Keybind for Player 2.", null, "", "Ability P2"));

    public static Configurable<KeyCode> AbilityKeybindPlayer3 { get; } = Instance.config.Bind(nameof(AbilityKeybindPlayer3), KeyCode.Joystick3Button4, new ConfigurableInfo(
        "Keybind for Player 3.", null, "", "Ability P3"));

    public static Configurable<KeyCode> AbilityKeybindPlayer4 { get; } = Instance.config.Bind(nameof(AbilityKeybindPlayer4), KeyCode.Joystick4Button4, new ConfigurableInfo(
        "Keybind for Player 4.", null, "", "Ability P4"));


    public static Configurable<bool> CustomSpearKeybind { get; } = Instance.config.Bind(nameof(CustomSpearKeybind), false, new ConfigurableInfo(
        "Prefer to use the custom keybinds below for spear creation, instead of the default (GRAB)",
        null, "", "Custom Spear Keybind?"));

    public static Configurable<bool> CustomAgilityKeybind { get; } = Instance.config.Bind(nameof(CustomAgilityKeybind), false, new ConfigurableInfo(
        "Prefer to use the custom keybinds below for agility double jump, instead of the default (GRAB + JUMP)",
        null, "", "Custom Agility Keybind?"));


    // STORE
    public static Configurable<KeyCode> StoreKeybindKeyboard { get; } = Instance.config.Bind(nameof(StoreKeybindKeyboard), KeyCode.LeftControl, new ConfigurableInfo(
        "Keybind for Keyboard.", null, "", "Keyboard"));

    public static Configurable<KeyCode> StoreKeybindPlayer1 { get; } = Instance.config.Bind(nameof(StoreKeybindPlayer1), KeyCode.Joystick1Button6, new ConfigurableInfo(
        "Keybind for Player 1.", null, "", "Player 1"));

    public static Configurable<KeyCode> StoreKeybindPlayer2 { get; } = Instance.config.Bind(nameof(StoreKeybindPlayer2), KeyCode.Joystick2Button6, new ConfigurableInfo(
        "Keybind for Player 2.", null, "", "Player 2"));

    public static Configurable<KeyCode> StoreKeybindPlayer3 { get; } = Instance.config.Bind(nameof(StoreKeybindPlayer3), KeyCode.Joystick3Button6, new ConfigurableInfo(
        "Keybind for Player 3.", null, "", "Player 3"));

    public static Configurable<KeyCode> StoreKeybindPlayer4 { get; } = Instance.config.Bind(nameof(StoreKeybindPlayer4), KeyCode.Joystick4Button6, new ConfigurableInfo(
        "Keybind for Player 4.", null, "", "Player 4"));


    public static Configurable<bool> UsesCustomStoreKeybind { get; } = Instance.config.Bind(nameof(UsesCustomStoreKeybind), false, new ConfigurableInfo(
        "Enables custom keybinds below, as opposed to the default (UP + PICKUP).",
        null, "", "Custom Keybind?"));


    // SENTRY
    public static Configurable<KeyCode> SentryKeybindKeyboard { get; } = Instance.config.Bind(nameof(SentryKeybindKeyboard), KeyCode.C, new ConfigurableInfo(
        "Keybind for Keyboard.", null, "", "Sentry KB"));

    public static Configurable<KeyCode> SentryKeybindPlayer1 { get; } = Instance.config.Bind(nameof(SentryKeybindPlayer1), KeyCode.Joystick1Button4, new ConfigurableInfo(
        "Keybind for Player 1.", null, "", "Sentry P1"));

    public static Configurable<KeyCode> SentryKeybindPlayer2 { get; } = Instance.config.Bind(nameof(SentryKeybindPlayer2), KeyCode.Joystick2Button4, new ConfigurableInfo(
        "Keybind for Player 2.", null, "", "Sentry P2"));

    public static Configurable<KeyCode> SentryKeybindPlayer3 { get; } = Instance.config.Bind(nameof(SentryKeybindPlayer3), KeyCode.Joystick3Button4, new ConfigurableInfo(
        "Keybind for Player 3.", null, "", "Sentry P3"));

    public static Configurable<KeyCode> SentryKeybindPlayer4 { get; } = Instance.config.Bind(nameof(SentryKeybindPlayer4), KeyCode.Joystick4Button4, new ConfigurableInfo(
        "Keybind for Player 4.", null, "", "Sentry P4"));


    public static Configurable<bool> CustomSentryKeybind { get; } = Instance.config.Bind(nameof(CustomSentryKeybind), true, new ConfigurableInfo(
        "Prefer to use the custom keybinds for deploying sentry pearls, instead of the default (GRAB + JUMP + DOWN)",
        null, "", "Custom Sentry Keybind?"));
}
