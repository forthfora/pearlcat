using Menu.Remix.MixedUI;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using static DataPearl.AbstractDataPearl;
using static Pearlcat.Enums;

namespace Pearlcat;

public sealed class ModOptions : OptionsTemplate
{
    public static ModOptions Instance { get; } = new();
    public static void RegisterOI()
    {
        if (MachineConnector.GetRegisteredOI(Plugin.MOD_ID) != Instance)
            MachineConnector.SetRegisteredOI(Plugin.MOD_ID, Instance);
    }


    public static readonly Color WarnRed = new(0.85f, 0.35f, 0.4f);

    #region Options

    public static Configurable<bool> PearlThreatMusic { get; } = Instance.config.Bind(nameof(PearlThreatMusic), false, new ConfigurableInfo(
        "When checked, most pearls (when active) will force the threat theme for all regions to the theme of the region they were originally from.", null, "",
        "Pearl Threat Music?"));

    public static Configurable<bool> DisableCosmetics { get; } = Instance.config.Bind(nameof(DisableCosmetics), false, new ConfigurableInfo(
        "When checked, Pearlcat's cosmetics will be disabled, intended to allow custom sprites via DMS. This does not include the pearls themselves.", null, "",
        "Disable Cosmetics?"));

    public static Configurable<bool> DisableTutorials { get; } = Instance.config.Bind(nameof(DisableTutorials), false, new ConfigurableInfo(
        "When checked, all tutorials will be disabled.", null, "",
        "Disable Tutorials?"));



    public static Configurable<bool> BadScavRep { get; } = Instance.config.Bind(nameof(BadScavRep), true, new ConfigurableInfo(
        "When checked, Pearlcat's reputation with the scavengers will be permanently the minimum, like Artificer.", null, "",
        "Bad Scav Rep?"));

    public static Configurable<int> MaxPearlCount { get; } = Instance.config.Bind(nameof(MaxPearlCount), 11, new ConfigurableInfo(
        "Maximum number of pearls that can be stored at once, including the active pearl. Default is 11. Hold and drag up or down to change.",
        new ConfigAcceptableRange<int>(1, 100), "",
        "Max Pearl Count"));
    
    public static Configurable<int> VisibilityMultiplier { get; } = Instance.config.Bind(nameof(VisibilityMultiplier), 100, new ConfigurableInfo(
        "Percentage multiplier on Pearlcat's general visibility, influences predator attraction. By default, Pearlcat is significantly more visible than even Hunter.",
        new ConfigAcceptableRange<int>(0, 300), "",
        "Visibility Multiplier"));


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



    public static Configurable<bool> InventoryOverride { get; } = Instance.config.Bind(nameof(InventoryOverride), false, new ConfigurableInfo(
        "When checked, sets the inventory to the specified numbers of coloured pearls below every cycle." +
        "\nDoes not save over the current inventory - it is returned to when unchecked.", null, "",
        "Inventory Override?"));

    public static Configurable<bool> StartingInventoryOverride { get; } = Instance.config.Bind(nameof(StartingInventoryOverride), false, new ConfigurableInfo(
        "When checked, overrides the starting pearls with the option below. Only effective at the start of a new game, unlike Inventory Override?", null, "",
        "Starting Inventory Override?"));


    public static Configurable<int> SpearPearlCount { get; } = Instance.config.Bind(nameof(SpearPearlCount), 1, new ConfigurableInfo(
        "Number of spear creation pearls (white). Effective only when Inventory Override? is checked.",
        new ConfigAcceptableRange<int>(0, 100), "",
        "Spear Pearl Count"));

    public static Configurable<int> RevivePearlCount { get; } = Instance.config.Bind(nameof(RevivePearlCount), 1, new ConfigurableInfo(
        "Number of revive pearls (green). Effective only when Inventory Override? is checked.",
        new ConfigAcceptableRange<int>(0, 100), "",
        "Revive Pearl Count"));

    public static Configurable<int> AgilityPearlCount { get; } = Instance.config.Bind(nameof(AgilityPearlCount), 1, new ConfigurableInfo(
        "Number of agility pearls (blue). Effective only when Inventory Override? is checked.",
        new ConfigAcceptableRange<int>(0, 100), "",
        "Agility Pearl Count"));

    public static Configurable<int> RagePearlCount { get; } = Instance.config.Bind(nameof(RagePearlCount), 1, new ConfigurableInfo(
        "Number of rage pearls (red). Effective only when Inventory Override? is checked.",
        new ConfigAcceptableRange<int>(0, 100), "",
        "Rage Pearl Count"));

    public static Configurable<int> ShieldPearlCount { get; } = Instance.config.Bind(nameof(ShieldPearlCount), 1, new ConfigurableInfo(
        "Number of shield pearls (yellow). Effective only when Inventory Override? is checked.",
        new ConfigAcceptableRange<int>(0, 100), "",
        "Shield Pearl Count"));

    public static Configurable<int> CamoPearlCount { get; } = Instance.config.Bind(nameof(CamoPearlCount), 1, new ConfigurableInfo(
        "Number of camo pearls (black). Effective only when Inventory Override? is checked.",
        new ConfigAcceptableRange<int>(0, 100), "",
        "Camo Pearl Count"));


    public static Configurable<string> StartShelterOverride { get; } = Instance.config.Bind(nameof(StartShelterOverride), "", new ConfigurableInfo(
        "Input a shelter name to have it override where Pearlcat starts a new game.", null, "", "Start Shelter Override"));

    #endregion

    #region Keybind Options

    public static Configurable<KeyCode> SwapLeftKeybind = Instance.config.Bind(nameof(SwapLeftKeybind), KeyCode.A, new ConfigurableInfo(
        "Keybind to swap to the stored pearl to the left. Limited to Player 1.", null, "", "Swap Left"));

    public static Configurable<KeyCode> SwapRightKeybind = Instance.config.Bind(nameof(SwapRightKeybind), KeyCode.D, new ConfigurableInfo(
        "Keybind to swap to the stored pearl to the right. Limited to Player 1.", null, "", "Swap Right"));



    public static Configurable<KeyCode> SwapKeybindKeyboard = Instance.config.Bind(nameof(SwapKeybindKeyboard), KeyCode.LeftAlt, new ConfigurableInfo(
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



    public static Configurable<bool> CustomSpearKeybind = Instance.config.Bind(nameof(CustomSpearKeybind), false, new ConfigurableInfo(
        "Prefer to use the custom keybinds below for spear creation.",
        null, "", "Custom Spear Keybind?"));

    public static Configurable<bool> CustomAgilityKeybind = Instance.config.Bind(nameof(CustomAgilityKeybind), false, new ConfigurableInfo(
        "Prefer to use the custom keybinds below for agility double jump.",
        null, "", "Custom Agility Keybind?"));


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

    public static Configurable<KeyCode> StoreKeybindKeyboard = Instance.config.Bind(nameof(StoreKeybindKeyboard), KeyCode.LeftControl, new ConfigurableInfo(
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

    public const int TAB_COUNT = 6;

    public override void Initialize()
    {
        base.Initialize();

        Tabs = new OpTab[TAB_COUNT];
        int tabIndex = -1;

        InitGeneral(ref tabIndex);
        
        InitAbilityInput(ref tabIndex);
        InitSwapInput(ref tabIndex);
        InitStoreInput(ref tabIndex);

        InitDifficulty(ref tabIndex);
        InitCheats(ref tabIndex);
    }


    private void InitGeneral(ref int tabIndex)
    {
        AddTab(ref tabIndex, "General");

        AddCheckBox(PearlThreatMusic);
        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddCheckBox(DisableTutorials);
        AddCheckBox(DisableCosmetics);
        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddNewLine(2);

        AddTextLabel("Special thanks to the following people!", bigText: true);
        DrawTextLabels(ref Tabs[tabIndex]);

        AddNewLine(1);

        AddTextLabel("NoirCatto - Floppy Ears");
        DrawTextLabels(ref Tabs[tabIndex]);

        AddNewLine(9);
        DrawBox(ref Tabs[tabIndex]);

        if (GetConfigurable(DisableCosmetics, out OpCheckBox checkBox))
            checkBox.colorEdge = WarnRed;

        if (GetConfigurable(DisableTutorials, out checkBox))
            checkBox.colorEdge = WarnRed;


        if (GetLabel(DisableCosmetics, out var label))
            label.color = WarnRed;

        if (GetLabel(DisableTutorials, out label))
            label.color = WarnRed;
    }

    private void InitCheats(ref int tabIndex)
    {
        AddTab(ref tabIndex, "Cheats");
        Tabs[tabIndex].colorButton = WarnRed;

        var warningText = "Intended for fun," +
            "\nmay change gameplay significantly!";
        
        AddTextLabel(warningText, bigText: true);
        DrawTextLabels(ref Tabs[tabIndex]);
        
        AddDragger(MaxPearlCount);
        DrawDraggers(ref Tabs[tabIndex]);


        var offset = new Vector2(100.0f, 109.0f);

        var startShelterOverride = new OpTextBox(StartShelterOverride, new Vector2(165.0f, 259.0f) + offset, 90.0f)
        {
            colorEdge = WarnRed,
            colorText = WarnRed,
        };

        var startShelterOverrideLabel = new OpLabel(new Vector2(230.0f, 210.0f) + offset, new Vector2(150f, 16.0f) + offset, StartShelterOverride.info.Tags[0].ToString())
        {
            color = WarnRed,
        };

        Tabs[tabIndex].AddItems(startShelterOverride, startShelterOverrideLabel);


        AddNewLine(1);

        AddCheckBox(InventoryOverride);
        AddCheckBox(StartingInventoryOverride);
        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddNewLine(1);

        AddDragger(AgilityPearlCount);
        AddDragger(CamoPearlCount);
        DrawDraggers(ref Tabs[tabIndex]);

        AddDragger(RagePearlCount);
        AddDragger(RevivePearlCount);
        DrawDraggers(ref Tabs[tabIndex]);

        AddDragger(ShieldPearlCount);
        AddDragger(SpearPearlCount);
        DrawDraggers(ref Tabs[tabIndex]);

        AddNewLine(2);
        DrawBox(ref Tabs[tabIndex]);


        if (GetLabel(warningText, out OpLabel label))
            label.color = WarnRed;

        if (GetLabel(InventoryOverride, out label))
            label.color = WarnRed;

        if (GetLabel(StartingInventoryOverride, out label))
            label.color = WarnRed;

        if (GetLabel(MaxPearlCount, out label))
            label.color = WarnRed;


        if (GetLabel(AgilityPearlCount, out label))
            label.color = Color.cyan;

        if (GetLabel(CamoPearlCount, out label))
            label.color = Color.grey;

        if (GetLabel(RagePearlCount, out label))
            label.color = Color.red;

        if (GetLabel(RevivePearlCount, out label))
            label.color = Color.green;

        if (GetLabel(ShieldPearlCount, out label))
            label.color = Color.yellow;

        if (GetLabel(SpearPearlCount, out label))
            label.color = Color.white;


        if (GetConfigurable(InventoryOverride, out OpCheckBox checkBox))
            checkBox.colorEdge = WarnRed;

        if (GetConfigurable(StartingInventoryOverride, out checkBox))
            checkBox.colorEdge = WarnRed;


        if (GetConfigurable(MaxPearlCount, out OpDragger dragger))
        {
            dragger.colorEdge = WarnRed;
            dragger.colorText = WarnRed;
        }
        if (GetConfigurable(AgilityPearlCount, out dragger))
        {
            dragger.colorEdge = Color.cyan;
            dragger.colorText = Color.cyan;
        }
        if (GetConfigurable(CamoPearlCount, out dragger))
        {
            dragger.colorEdge = Color.grey;
            dragger.colorText = Color.grey;
        }
        if (GetConfigurable(RagePearlCount, out dragger))
        {
            dragger.colorEdge = Color.red;
            dragger.colorText = Color.red;
        }
        if (GetConfigurable(RevivePearlCount, out dragger))
        {
            dragger.colorEdge = Color.green;
            dragger.colorText = Color.green;
        }
        if (GetConfigurable(ShieldPearlCount, out dragger))
        {
            dragger.colorEdge = Color.yellow;
            dragger.colorText = Color.yellow;
        }
        if (GetConfigurable(SpearPearlCount, out dragger))
        {
            dragger.colorEdge = Color.white;
            dragger.colorText = Color.white;
        }
    }

    private void InitDifficulty(ref int tabIndex)
    {
        AddTab(ref tabIndex, "Difficulty");
        Tabs[tabIndex].colorButton = WarnRed;

        var warningText = "Intended to make gameplay more challenging, may change gameplay significantly!";
        AddTextLabel(warningText, bigText: true);
        DrawTextLabels(ref Tabs[tabIndex]);
         

        AddCheckBox(DisableMinorEffects);
        AddCheckBox(BadScavRep);
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

        AddDragger(VisibilityMultiplier);
        DrawDraggers(ref Tabs[tabIndex]);

        AddNewLine(2);
        DrawBox(ref Tabs[tabIndex]);

        #region Color Changes

        if (GetLabel(warningText, out var label))
            label.color = WarnRed;


        if (GetLabel(DisableMinorEffects, out label))
            label.color = WarnRed;

        if (GetLabel(BadScavRep, out label))
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

        if (GetConfigurable(BadScavRep, out checkBox))
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


        if (GetLabel(VisibilityMultiplier, out label))
            label.color = WarnRed;

        if (GetConfigurable(VisibilityMultiplier, out OpDragger dragger))
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

        AddCheckBox(CustomSpearKeybind);
        AddCheckBox(CustomAgilityKeybind);
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


    public static List<DataPearlType> GetOverridenInventory(bool hasRM)
    {
        List<DataPearlType> pearls = new();

        for (int i = 0; i < AgilityPearlCount.Value; i++)
            pearls.Add(Pearls.AS_PearlBlue);

        for (int i = 0; i < ShieldPearlCount.Value; i++)
            pearls.Add(Pearls.AS_PearlYellow);

        for (int i = 0; i < RevivePearlCount.Value; i++)
            pearls.Add(Pearls.AS_PearlGreen);

        for (int i = 0; i < CamoPearlCount.Value; i++)
            pearls.Add(Pearls.AS_PearlBlack);

        for (int i = 0; i < RagePearlCount.Value; i++)
            pearls.Add(Pearls.AS_PearlRed);

        for (int i = 0; i < SpearPearlCount.Value; i++)
            pearls.Add(i == 0 && hasRM ? Pearls.RM_Pearlcat : DataPearlType.Misc);

        return pearls;
    }
}