using Menu.Remix.MixedUI;
using System.Linq;
using UnityEngine;

namespace Pearlcat;

public sealed class PearlcatOptions : OptionsTemplate
{
    public static readonly PearlcatOptions Instance = new();

    #region Options

    public static Configurable<bool> PearlThreatMusic = Instance.config.Bind(nameof(PearlThreatMusic), true, new ConfigurableInfo(
        "When checked, most pearls (when active) will force the threat theme for all regions to the theme of the region they were originally from.", null, "",
        "Pearl Threat Music?"));

    public static Configurable<bool> DisableCosmetics = Instance.config.Bind(nameof(DisableCosmetics), false, new ConfigurableInfo(
    "When checked, Pearlcat's cosmetics will be disabled, intended to allow custom sprites via DMS. This does not include the pearls themselves.", null, "",
    "Disable Cosmetics?"));

    #endregion

    #region Keybind Options

    public static Configurable<KeyCode> SwapLeftKeybind = Instance.config.Bind(nameof(SwapLeftKeybind), KeyCode.A, new ConfigurableInfo(
        "Keybind to swap to the stored pearl to the left. Limited to Player 1.", null, "", "Swap Left"));

    public static Configurable<KeyCode> SwapRightKeybind = Instance.config.Bind(nameof(SwapRightKeybind), KeyCode.D, new ConfigurableInfo(
        "Keybind to swap to the stored pearl to the right. Limited to Player 1.", null, "", "Swap Right"));



    public static Configurable<KeyCode> SwapKeybindKeyboard = Instance.config.Bind(nameof(SwapKeybindKeyboard), KeyCode.S, new ConfigurableInfo(
        "Keybind for Keyboard.", null, "", "Keyboard"));

    public static Configurable<KeyCode> SwapKeybindPlayer1 = Instance.config.Bind(nameof(SwapKeybindPlayer1), KeyCode.Joystick1Button4, new ConfigurableInfo(
        "Keybind for Player 1.", null, "", "Player 1"));

    public static Configurable<KeyCode> SwapKeybindPlayer2 = Instance.config.Bind(nameof(SwapKeybindPlayer2), KeyCode.Joystick2Button4, new ConfigurableInfo(
        "Keybind for Player 2.", null, "", "Player 2"));

    public static Configurable<KeyCode> SwapKeybindPlayer3 = Instance.config.Bind(nameof(SwapKeybindPlayer3), KeyCode.Joystick3Button4, new ConfigurableInfo(
        "Keybind for Player 3.", null, "", "Player 3"));

    public static Configurable<KeyCode> SwapKeybindPlayer4 = Instance.config.Bind(nameof(SwapKeybindPlayer4), KeyCode.Joystick4Button4, new ConfigurableInfo(
        "Keybind for Player 4.", null, "", "Player 4"));

    public static Configurable<int> SwapTriggerPlayer = Instance.config.Bind(nameof(SwapTriggerPlayer), 1, new ConfigurableInfo(
        "Which player controller trigger swapping will apply to. Hold and drag up or down to change.",
        new ConfigAcceptableRange<int>(1, 4), "",
        "Trigger Player"));



    public static Configurable<bool> UsesCustomAbilityKeybind = Instance.config.Bind(nameof(UsesCustomAbilityKeybind), false, new ConfigurableInfo(
        "Enables custom keybinds below, as opposed to the default (JUMP + PICKUP).",
        null, "", "Custom Keybind?"));

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

    public static Configurable<KeyCode> StoreKeybindPlayer1 = Instance.config.Bind(nameof(StoreKeybindPlayer1), KeyCode.Joystick1Button4, new ConfigurableInfo(
        "Keybind for Player 1.", null, "", "Player 1"));

    public static Configurable<KeyCode> StoreKeybindPlayer2 = Instance.config.Bind(nameof(StoreKeybindPlayer2), KeyCode.Joystick2Button4, new ConfigurableInfo(
        "Keybind for Player 2.", null, "", "Player 2"));

    public static Configurable<KeyCode> StoreKeybindPlayer3 = Instance.config.Bind(nameof(StoreKeybindPlayer3), KeyCode.Joystick3Button4, new ConfigurableInfo(
        "Keybind for Player 3.", null, "", "Player 3"));

    public static Configurable<KeyCode> StoreKeybindPlayer4 = Instance.config.Bind(nameof(StoreKeybindPlayer4), KeyCode.Joystick4Button4, new ConfigurableInfo(
        "Keybind for Player 4.", null, "", "Player 4"));

    #endregion

    public const int TAB_COUNT = 4;

    public override void Initialize()
    {
        base.Initialize();

        Tabs = new OpTab[TAB_COUNT];
        int tabIndex = -1;

        InitGeneral(ref tabIndex);
        
        InitAbilityInput(ref tabIndex);
        InitSwapInput(ref tabIndex);
        InitStoreInput(ref tabIndex);
    }


    private void InitGeneral(ref int tabIndex)
    {
        AddTab(ref tabIndex, "General");

        AddCheckBox(PearlThreatMusic);
        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddNewLine(13);

        AddCheckBox(DisableCosmetics);
        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddNewLine(1);
        DrawBox(ref Tabs[tabIndex]);

        var checkBox = (OpCheckBox)Tabs[0].items.Where(item => item is OpCheckBox checkBox && checkBox.cfgEntry == DisableCosmetics).FirstOrDefault();
        var label = (OpLabel)Tabs[0].items.Where(item => item is OpLabel label && label.text == DisableCosmetics.info.Tags[0].ToString()).FirstOrDefault();

        checkBox.colorEdge = Color.red;
        label.color = Color.red;
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

        AddNewLine(2);

        DrawKeybinders(SwapLeftKeybind, ref Tabs[tabIndex]);
        DrawKeybinders(SwapRightKeybind, ref Tabs[tabIndex]);

        AddNewLine(-2);

        AddDragger(SwapTriggerPlayer);
        DrawDraggers(ref Tabs[tabIndex]);

        AddNewLine(3);

        DrawKeybinders(SwapKeybindKeyboard, ref Tabs[tabIndex]);
        DrawKeybinders(SwapKeybindPlayer1, ref Tabs[tabIndex]);
        DrawKeybinders(SwapKeybindPlayer2, ref Tabs[tabIndex]);
        DrawKeybinders(SwapKeybindPlayer3, ref Tabs[tabIndex]);
        DrawKeybinders(SwapKeybindPlayer4, ref Tabs[tabIndex]);

        AddNewLine(1);
        DrawBox(ref Tabs[tabIndex]);
    }

    private void InitAbilityInput(ref int tabIndex)
    {
        AddTab(ref tabIndex, "Ability Input");

        AddCheckBox(UsesCustomAbilityKeybind);
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