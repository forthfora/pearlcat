using Menu.Remix.MixedUI;
using UnityEngine;

namespace Pearlcat;

public sealed class ModOptions : OptionsTemplate
{
    public static readonly ModOptions Instance = new();

    public static readonly Color WarnRed = new(0.85f, 0.35f, 0.4f);

    #region Options

    public static Configurable<bool> PearlThreatMusic = Instance.config.Bind(nameof(PearlThreatMusic), true, new ConfigurableInfo(
        "When checked, most pearls (when active) will force the threat theme for all regions to the theme of the region they were originally from.", null, "",
        "Pearl Threat Music?"));

    public static Configurable<bool> DisableCosmetics = Instance.config.Bind(nameof(DisableCosmetics), false, new ConfigurableInfo(
        "When checked, Pearlcat's cosmetics will be disabled, intended to allow custom sprites via DMS. This does not include the pearls themselves.", null, "",
        "Disable Cosmetics?"));

    public static Configurable<bool> LowStartingReputation = Instance.config.Bind(nameof(LowStartingReputation), true, new ConfigurableInfo(
        "When checked, Pearlcat's starting reputation with many creatures will be low.", null, "",
        "Low Starting Reputation?"));

    public static Configurable<int> MaxPearlCount = Instance.config.Bind(nameof(MaxPearlCount), 11, new ConfigurableInfo(
        "Maximum number of pearls that can be stored at once, including the active pearl. Default is 11. Hold and drag up or down to change.",
        new ConfigAcceptableRange<int>(1, 100), "",
        "Max Pearl Count"));
    
    public static Configurable<int> VisibilityMultiplier = Instance.config.Bind(nameof(VisibilityMultiplier), 100, new ConfigurableInfo(
        "Percentage multiplier on Pearlcat's general visibility, influences predator attraction. By default, Pearlcat is significantly more visible than even Hunter.",
        new ConfigAcceptableRange<int>(0, 300), "",
        "Visibility Multiplier"));


    public static Configurable<bool> DisableMinorEffects = Instance.config.Bind(nameof(DisableMinorEffects), false, new ConfigurableInfo(
        "When checked, pearls will no longer grant stat changes, active or otherwise, and base stats are set to be similar to Hunter.", null, "",
        "Disable Minor Effects?"));

    public static Configurable<bool> DisableSpear = Instance.config.Bind(nameof(DisableSpear), false, new ConfigurableInfo(
        "When checked, disables the spear creation effect granted by an active pearl.", null, "",
        "Disable Spear Effect?"));

    public static Configurable<bool> DisableRevive = Instance.config.Bind(nameof(DisableRevive), false, new ConfigurableInfo(
        "When checked, disables the revive effect granted by an active pearl.", null, "",
        "Disable Revive Effect?"));

    public static Configurable<bool> DisableAgility = Instance.config.Bind(nameof(DisableAgility), false, new ConfigurableInfo(
        "When checked, disables the agility effect granted by an active pearl.", null, "",
        "Disable Agility Effect?"));

    public static Configurable<bool> DisableRage = Instance.config.Bind(nameof(DisableRage), false, new ConfigurableInfo(
        "When checked, disables the rage effect granted by an active pearl.", null, "",
        "Disable Rage Effect?"));

    public static Configurable<bool> DisableShield = Instance.config.Bind(nameof(DisableShield), false, new ConfigurableInfo(
        "When checked, disables the shield effect granted by an active pearl.", null, "",
        "Disable Shield Effect?"));

    public static Configurable<bool> DisableCamoflague = Instance.config.Bind(nameof(DisableCamoflague), false, new ConfigurableInfo(
        "When checked, disables the camoflague effect granted by an active pearl.", null, "",
        "Disable Camoflague Effect?"));


    #endregion

    #region Keybind Options

    public static Configurable<KeyCode> SwapLeftKeybind = Instance.config.Bind(nameof(SwapLeftKeybind), KeyCode.A, new ConfigurableInfo(
        "Keybind to swap to the stored pearl to the left. Limited to Player 1.", null, "", "Swap Left"));

    public static Configurable<KeyCode> SwapRightKeybind = Instance.config.Bind(nameof(SwapRightKeybind), KeyCode.D, new ConfigurableInfo(
        "Keybind to swap to the stored pearl to the right. Limited to Player 1.", null, "", "Swap Right"));



    public static Configurable<KeyCode> SwapKeybindKeyboard = Instance.config.Bind(nameof(SwapKeybindKeyboard), KeyCode.LeftControl, new ConfigurableInfo(
        "Keybind for Keyboard.", null, "", "Keyboard"));

    public static Configurable<KeyCode> SwapKeybindPlayer1 = Instance.config.Bind(nameof(SwapKeybindPlayer1), KeyCode.Joystick1Button3, new ConfigurableInfo(
        "Keybind for Player 1.", null, "", "Player 1"));

    public static Configurable<KeyCode> SwapKeybindPlayer2 = Instance.config.Bind(nameof(SwapKeybindPlayer2), KeyCode.Joystick2Button3, new ConfigurableInfo(
        "Keybind for Player 2.", null, "", "Player 2"));

    public static Configurable<KeyCode> SwapKeybindPlayer3 = Instance.config.Bind(nameof(SwapKeybindPlayer3), KeyCode.Joystick3Button3, new ConfigurableInfo(
        "Keybind for Player 3.", null, "", "Player 3"));

    public static Configurable<KeyCode> SwapKeybindPlayer4 = Instance.config.Bind(nameof(SwapKeybindPlayer4), KeyCode.Joystick4Button3, new ConfigurableInfo(
        "Keybind for Player 4.", null, "", "Player 4"));

    public static Configurable<int> SwapTriggerPlayer = Instance.config.Bind(nameof(SwapTriggerPlayer), 1, new ConfigurableInfo(
        "Which player controller trigger swapping will apply to. Hold and drag up or down to change.",
        new ConfigAcceptableRange<int>(1, 4), "",
        "Trigger Player"));



    public static Configurable<bool> PreferCustomAbilityKeybind = Instance.config.Bind(nameof(PreferCustomAbilityKeybind), false, new ConfigurableInfo(
        "Prefer to use the custom keybinds below, as opposed to special binds in some cases, such as (JUMP + PICKUP) for Agiltiy.",
        null, "", "Prefer Custom Keybind?"));

    public static Configurable<KeyCode> AbilityKeybindKeyboard = Instance.config.Bind(nameof(AbilityKeybindKeyboard), KeyCode.C, new ConfigurableInfo(
        "Keybind for Keyboard.", null, "", "Keyboard"));

    public static Configurable<KeyCode> AbilityKeybindPlayer1 = Instance.config.Bind(nameof(AbilityKeybindPlayer1), KeyCode.Joystick1Button4, new ConfigurableInfo(
        "Keybind for Player 1.", null, "", "Player 1"));

    public static Configurable<KeyCode> AbilityKeybindPlayer2 = Instance.config.Bind(nameof(AbilityKeybindPlayer2), KeyCode.Joystick2Button4, new ConfigurableInfo(
        "Keybind for Player 2.", null, "", "Player 2"));

    public static Configurable<KeyCode> AbilityKeybindPlayer3 = Instance.config.Bind(nameof(AbilityKeybindPlayer3), KeyCode.Joystick3Button4, new ConfigurableInfo(
        "Keybind for Player 3.", null, "", "Player 3"));

    public static Configurable<KeyCode> AbilityKeybindPlayer4 = Instance.config.Bind(nameof(AbilityKeybindPlayer4), KeyCode.Joystick4Button4, new ConfigurableInfo(
        "Keybind for Player 4.", null, "", "Player 4"));



    public static Configurable<bool> UsesCustomStoreKeybind = Instance.config.Bind(nameof(UsesCustomStoreKeybind), false, new ConfigurableInfo(
        "Enables custom keybinds below, as opposed to the default (UP + PICKUP).",
        null, "", "Custom Keybind?"));

    public static Configurable<KeyCode> StoreKeybindKeyboard = Instance.config.Bind(nameof(StoreKeybindKeyboard), KeyCode.LeftAlt, new ConfigurableInfo(
        "Keybind for Keyboard.", null, "", "Keyboard"));

    public static Configurable<KeyCode> StoreKeybindPlayer1 = Instance.config.Bind(nameof(StoreKeybindPlayer1), KeyCode.Joystick1Button6, new ConfigurableInfo(
        "Keybind for Player 1.", null, "", "Player 1"));

    public static Configurable<KeyCode> StoreKeybindPlayer2 = Instance.config.Bind(nameof(StoreKeybindPlayer2), KeyCode.Joystick2Button6, new ConfigurableInfo(
        "Keybind for Player 2.", null, "", "Player 2"));

    public static Configurable<KeyCode> StoreKeybindPlayer3 = Instance.config.Bind(nameof(StoreKeybindPlayer3), KeyCode.Joystick3Button6, new ConfigurableInfo(
        "Keybind for Player 3.", null, "", "Player 3"));

    public static Configurable<KeyCode> StoreKeybindPlayer4 = Instance.config.Bind(nameof(StoreKeybindPlayer4), KeyCode.Joystick4Button6, new ConfigurableInfo(
        "Keybind for Player 4.", null, "", "Player 4"));

    #endregion

    public const int TAB_COUNT = 5;

    public override void Initialize()
    {
        base.Initialize();

        Tabs = new OpTab[TAB_COUNT];
        int tabIndex = -1;

        InitGeneral(ref tabIndex);
        
        InitAbilityInput(ref tabIndex);
        InitSwapInput(ref tabIndex);
        InitStoreInput(ref tabIndex);

        InitAccessibility(ref tabIndex);
    }


    private void InitGeneral(ref int tabIndex)
    {
        AddTab(ref tabIndex, "General");

        AddCheckBox(PearlThreatMusic);
        AddCheckBox(DisableCosmetics);
        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddNewLine(2);

        AddTextLabel("Special thanks to the following people!", bigText: true);
        DrawTextLabels(ref Tabs[tabIndex]);

        AddNewLine(1);

        AddTextLabel("NoirCatto - Floppy Ears");
        DrawTextLabels(ref Tabs[tabIndex]);

        AddNewLine(12);
        DrawBox(ref Tabs[tabIndex]);

        if (GetConfigurable(DisableCosmetics, out OpCheckBox checkBox))
            checkBox.colorEdge = WarnRed;

        if (GetLabel(DisableCosmetics, out var label))
            label.color = WarnRed;
    }

    private void InitAccessibility(ref int tabIndex)
    {
        AddTab(ref tabIndex, "Accessibility");
        Tabs[tabIndex].colorButton = WarnRed;

        var warningText = "Be warned the following may change gameplay significantly!";
        AddTextLabel(warningText, bigText: true);
        DrawTextLabels(ref Tabs[tabIndex]);
         

        AddCheckBox(DisableMinorEffects);
        AddCheckBox(LowStartingReputation);
        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddNewLine(1);

        AddCheckBox(DisableAgility);
        AddCheckBox(DisableCamoflague);
        DrawCheckBoxes(ref Tabs[tabIndex]);
        
        AddCheckBox(DisableRage);
        AddCheckBox(DisableRevive);
        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddCheckBox(DisableShield);
        AddCheckBox(DisableSpear);
        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddNewLine(1);

        AddDragger(MaxPearlCount);
        AddDragger(VisibilityMultiplier);
        DrawDraggers(ref Tabs[tabIndex]);

        AddNewLine(2);
        DrawBox(ref Tabs[tabIndex]);

        #region Color Changes

        if (GetLabel(warningText, out var label))
            label.color = WarnRed;


        if (GetLabel(DisableMinorEffects, out label))
            label.color = WarnRed;

        if (GetLabel(LowStartingReputation, out label))
            label.color = WarnRed;

        if (GetLabel(DisableAgility, out label))
            label.color = Color.cyan;

        if (GetLabel(DisableCamoflague, out label))
            label.color = Color.grey;

        if (GetLabel(DisableRage, out label))
            label.color = Color.red;

        if (GetLabel(DisableRevive, out label))
            label.color = Color.green;

        if (GetLabel(DisableShield, out label))
            label.color = Color.yellow;

        if (GetLabel(DisableSpear, out label))
            label.color = Color.white;


        if (GetConfigurable(DisableMinorEffects, out OpCheckBox checkBox))
            checkBox.colorEdge = WarnRed;

        if (GetConfigurable(LowStartingReputation, out checkBox))
            checkBox.colorEdge = WarnRed;

        if (GetConfigurable(DisableAgility, out checkBox))
            checkBox.colorEdge = Color.cyan;

        if (GetConfigurable(DisableCamoflague, out checkBox))
            checkBox.colorEdge = Color.grey;

        if (GetConfigurable(DisableRage, out checkBox))
            checkBox.colorEdge = Color.red;

        if (GetConfigurable(DisableRevive, out checkBox))
            checkBox.colorEdge = Color.green;

        if (GetConfigurable(DisableShield, out checkBox))
            checkBox.colorEdge = Color.yellow;

        if (GetConfigurable(DisableSpear, out checkBox))
            checkBox.colorEdge = Color.white;


        if (GetLabel(MaxPearlCount, out label))
            label.color = WarnRed;

        if (GetConfigurable(MaxPearlCount, out OpDragger dragger))
        {
            dragger.colorEdge = WarnRed;
            dragger.colorText = WarnRed;
        }


        if (GetLabel(VisibilityMultiplier, out label))
            label.color = WarnRed;

        if (GetConfigurable(VisibilityMultiplier, out dragger))
        {
            dragger.colorEdge = WarnRed;
            dragger.colorText = WarnRed;
        }

        #endregion
    }

    private void InitStoreInput(ref int tabIndex)
    {
        AddTab(ref tabIndex, "Store Input");

        AddCheckBox(UsesCustomStoreKeybind);
        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddNewLine(3);

        DrawKeybinders(StoreKeybindKeyboard, ref Tabs[tabIndex]);
        AddNewLine(1);

        DrawKeybinders(StoreKeybindPlayer1, ref Tabs[tabIndex]);
        AddNewLine(1);

        DrawKeybinders(StoreKeybindPlayer2, ref Tabs[tabIndex]);
        AddNewLine(1);

        DrawKeybinders(StoreKeybindPlayer3, ref Tabs[tabIndex]);
        AddNewLine(1);

        DrawKeybinders(StoreKeybindPlayer4, ref Tabs[tabIndex]);

        AddNewLine(1);
        DrawBox(ref Tabs[tabIndex]);
    }

    private void InitSwapInput(ref int tabIndex)
    {
        AddTab(ref tabIndex, "Swap Input");

        AddDragger(SwapTriggerPlayer);
        DrawDraggers(ref Tabs[tabIndex]);

        AddNewLine(3);

        DrawKeybinders(SwapLeftKeybind, ref Tabs[tabIndex]);
        DrawKeybinders(SwapRightKeybind, ref Tabs[tabIndex]);

        AddNewLine(2);

        DrawKeybinders(SwapKeybindKeyboard, ref Tabs[tabIndex]);
        DrawKeybinders(SwapKeybindPlayer1, ref Tabs[tabIndex]);
        DrawKeybinders(SwapKeybindPlayer2, ref Tabs[tabIndex]);
        DrawKeybinders(SwapKeybindPlayer3, ref Tabs[tabIndex]);
        DrawKeybinders(SwapKeybindPlayer4, ref Tabs[tabIndex]);

        AddNewLine(-1);
        DrawBox(ref Tabs[tabIndex]);
    }

    private void InitAbilityInput(ref int tabIndex)
    {
        AddTab(ref tabIndex, "Ability Input");

        AddCheckBox(PreferCustomAbilityKeybind);
        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddNewLine(3);

        DrawKeybinders(AbilityKeybindKeyboard, ref Tabs[tabIndex]);
        AddNewLine(1);

        DrawKeybinders(AbilityKeybindPlayer1, ref Tabs[tabIndex]);
        AddNewLine(1);

        DrawKeybinders(AbilityKeybindPlayer2, ref Tabs[tabIndex]);
        AddNewLine(1);

        DrawKeybinders(AbilityKeybindPlayer3, ref Tabs[tabIndex]);
        AddNewLine(1);

        DrawKeybinders(AbilityKeybindPlayer4, ref Tabs[tabIndex]);

        AddNewLine(1);
        DrawBox(ref Tabs[tabIndex]);
    }
}