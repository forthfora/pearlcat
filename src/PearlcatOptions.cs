using System.Collections.Generic;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace Pearlcat;

// Based on the options script from SBCameraScroll by SchuhBaum
// https://github.com/SchuhBaum/SBCameraScroll/blob/Rain-World-v1.9/SourceCode/MainModOptions.cs
public class PearlcatOptions : OptionInterface
{
    public static PearlcatOptions instance = new();
    public const string AUTHORS_NAME = "forthbridge";

    #region Options

    public static Configurable<bool> pearlThreatMusic = instance.config.Bind("pearlThreatMusic", false, new ConfigurableInfo(
        "When checked, most pearls (when active) will change the threat theme for all regions to the theme of the region they are from.", null, "",
        "Pearl Threat Music"));

    #endregion

    #region Keybind Options

    public static Configurable<KeyCode> swapKeybindKeyboard = instance.config.Bind("swapKeybindKeyboard", KeyCode.LeftAlt, new ConfigurableInfo(
        "Keybind for Keyboard.", null, "", "Keyboard"));

    public static Configurable<KeyCode> swapKeybindPlayer1 = instance.config.Bind("swapKeybindPlayer1", KeyCode.Joystick1Button4, new ConfigurableInfo(
        "Keybind for Player 1.", null, "", "Player 1"));

    public static Configurable<KeyCode> swapKeybindPlayer2 = instance.config.Bind("swapKkeybindPlayer2", KeyCode.Joystick2Button4, new ConfigurableInfo(
        "Keybind for Player 2.", null, "", "Player 2"));

    public static Configurable<KeyCode> swapKeybindPlayer3 = instance.config.Bind("swapKkeybindPlayer3", KeyCode.Joystick3Button4, new ConfigurableInfo(
        "Keybind for Player 3.", null, "", "Player 3"));

    public static Configurable<KeyCode> swapKeybindPlayer4 = instance.config.Bind("swapKeybindPlayer4", KeyCode.Joystick4Button4, new ConfigurableInfo(
        "Keybind for Player 4.", null, "", "Player 4"));



    public static Configurable<bool> usesCustomDashKeybind = instance.config.Bind("usesCustomDashKeybind", false, new ConfigurableInfo(
        "Enables custom keybinds below, as opposed to the default (JUMP + PICKUP).",
        null, "", "Custom Keybind?"));

    public static Configurable<KeyCode> abilityKeybindKeyboard = instance.config.Bind("abilityKeybindKeyboard", KeyCode.LeftAlt, new ConfigurableInfo(
        "Keybind for Keyboard.", null, "", "Keyboard"));

    public static Configurable<KeyCode> abilityKeybindPlayer1 = instance.config.Bind("abilityKeybindPlayer1", KeyCode.Joystick1Button4, new ConfigurableInfo(
        "Keybind for Player 1.", null, "", "Player 1"));

    public static Configurable<KeyCode> abilityKeybindPlayer2 = instance.config.Bind("abilityKkeybindPlayer2", KeyCode.Joystick2Button4, new ConfigurableInfo(
        "Keybind for Player 2.", null, "", "Player 2"));

    public static Configurable<KeyCode> abilityKeybindPlayer3 = instance.config.Bind("abilityKkeybindPlayer3", KeyCode.Joystick3Button4, new ConfigurableInfo(
        "Keybind for Player 3.", null, "", "Player 3"));

    public static Configurable<KeyCode> abilityKeybindPlayer4 = instance.config.Bind("abilityKeybindPlayer4", KeyCode.Joystick4Button4, new ConfigurableInfo(
        "Keybind for Player 4.", null, "", "Player 4"));



    public static Configurable<bool> usesCustomStoreKeybind = instance.config.Bind("usesCustomStoreKeybind", false, new ConfigurableInfo(
        "Enables custom keybinds below, as opposed to the default (UP + PICKUP).",
        null, "", "Custom Keybind?"));

    public static Configurable<KeyCode> storeKeybindKeyboard = instance.config.Bind("storeKeybindKeyboard", KeyCode.LeftAlt, new ConfigurableInfo(
        "Keybind for Keyboard.", null, "", "Keyboard"));

    public static Configurable<KeyCode> storeKeybindPlayer1 = instance.config.Bind("storeKeybindPlayer1", KeyCode.Joystick1Button4, new ConfigurableInfo(
        "Keybind for Player 1.", null, "", "Player 1"));

    public static Configurable<KeyCode> storeKeybindPlayer2 = instance.config.Bind("storeKkeybindPlayer2", KeyCode.Joystick2Button4, new ConfigurableInfo(
        "Keybind for Player 2.", null, "", "Player 2"));

    public static Configurable<KeyCode> storeKeybindPlayer3 = instance.config.Bind("storeKkeybindPlayer3", KeyCode.Joystick3Button4, new ConfigurableInfo(
        "Keybind for Player 3.", null, "", "Player 3"));

    public static Configurable<KeyCode> storeKeybindPlayer4 = instance.config.Bind("storeKeybindPlayer4", KeyCode.Joystick4Button4, new ConfigurableInfo(
        "Keybind for Player 4.", null, "", "Player 4"));

    #endregion

    #region Parameters

    public readonly float spacing = 20f;
    public readonly float fontHeight = 20f;
    public readonly int numberOfCheckboxes = 2;
    public readonly float checkBoxSize = 60.0f;
    
    public float CheckBoxWithSpacing => checkBoxSize + 0.25f * spacing;
    
    
    public Vector2 marginX = new();
    public Vector2 pos = new();

    public readonly List<float> boxEndPositions = new();

    public readonly List<Configurable<bool>> checkBoxConfigurables = new();
    public readonly List<OpLabel> checkBoxesTextLabels = new();

    public readonly List<Configurable<string>> comboBoxConfigurables = new();
    public readonly List<List<ListItem>> comboBoxLists = new();
    public readonly List<bool> comboBoxAllowEmpty = new();
    public readonly List<OpLabel> comboBoxesTextLabels = new();

    public readonly List<Configurable<int>> sliderConfigurables = new();
    public readonly List<string> sliderMainTextLabels = new();
    public readonly List<OpLabel> sliderTextLabelsLeft = new();
    public readonly List<OpLabel> sliderTextLabelsRight = new();

    public readonly List<OpLabel> textLabels = new();
    
    #endregion


    public const int NUMBER_OF_TABS = 4;

    public override void Initialize()
    {
        base.Initialize();
        Tabs = new OpTab[NUMBER_OF_TABS];
        int tabIndex = -1;


        AddTab(ref tabIndex, "General");

        AddCheckBox(pearlThreatMusic, (string)pearlThreatMusic.info.Tags[0]);
        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddNewLine(18);
        DrawBox(ref Tabs[tabIndex]);

        #region Ability Input

        AddTab(ref tabIndex, "Ability Input");

        AddCheckBox(usesCustomDashKeybind, (string)usesCustomDashKeybind.info.Tags[0]);
        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddNewLine(3);

        DrawKeybinders(abilityKeybindKeyboard, ref Tabs[tabIndex]);
        AddNewLine(1);

        DrawKeybinders(abilityKeybindPlayer1, ref Tabs[tabIndex]);
        AddNewLine(1);

        DrawKeybinders(abilityKeybindPlayer2, ref Tabs[tabIndex]);
        AddNewLine(1);

        DrawKeybinders(abilityKeybindPlayer3, ref Tabs[tabIndex]);
        AddNewLine(1);

        DrawKeybinders(abilityKeybindPlayer4, ref Tabs[tabIndex]);

        AddNewLine(1);
        DrawBox(ref Tabs[tabIndex]);
        #endregion

        #region Swap Input
        AddTab(ref tabIndex, "Swap Input");

        AddNewLine(2);

        DrawKeybinders(swapKeybindKeyboard, ref Tabs[tabIndex]);
        AddNewLine(1);

        DrawKeybinders(swapKeybindPlayer1, ref Tabs[tabIndex]);
        AddNewLine(1);

        DrawKeybinders(swapKeybindPlayer2, ref Tabs[tabIndex]);
        AddNewLine(1);

        DrawKeybinders(swapKeybindPlayer3, ref Tabs[tabIndex]);
        AddNewLine(1);

        DrawKeybinders(swapKeybindPlayer4, ref Tabs[tabIndex]);

        AddNewLine(5);
        DrawBox(ref Tabs[tabIndex]);
        #endregion

        #region Store Input
        AddTab(ref tabIndex, "Store Input");

        AddCheckBox(usesCustomStoreKeybind, (string)usesCustomStoreKeybind.info.Tags[0]);
        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddNewLine(3);

        DrawKeybinders(storeKeybindKeyboard, ref Tabs[tabIndex]);
        AddNewLine(1);

        DrawKeybinders(storeKeybindPlayer1, ref Tabs[tabIndex]);
        AddNewLine(1);

        DrawKeybinders(storeKeybindPlayer2, ref Tabs[tabIndex]);
        AddNewLine(1);

        DrawKeybinders(storeKeybindPlayer3, ref Tabs[tabIndex]);
        AddNewLine(1);

        DrawKeybinders(storeKeybindPlayer4, ref Tabs[tabIndex]);

        AddNewLine(1);
        DrawBox(ref Tabs[tabIndex]);
        #endregion
    }


    #region UI Elements

    public void AddTab(ref int tabIndex, string tabName)
    {
        tabIndex++;
        Tabs[tabIndex] = new OpTab(this, tabName);
        InitializeMarginAndPos();

        AddNewLine();
        AddTextLabel(Plugin.MOD_NAME, bigText: true);
        DrawTextLabels(ref Tabs[tabIndex]);

        AddNewLine(0.5f);
        AddTextLabel("Version " + Plugin.VERSION, FLabelAlignment.Left);
        AddTextLabel("by " + AUTHORS_NAME, FLabelAlignment.Right);
        DrawTextLabels(ref Tabs[tabIndex]);

        AddNewLine();
        AddBox();
    }

    public void InitializeMarginAndPos()
    {
        marginX = new Vector2(50f, 550f);
        pos = new Vector2(50f, 600f);
    }

    public void AddNewLine(float spacingModifier = 1f)
    {
        pos.x = marginX.x; // left margin
        pos.y -= spacingModifier * spacing;
    }

    public void AddBox()
    {
        marginX += new Vector2(spacing, -spacing);
        boxEndPositions.Add(pos.y); // end position > start position
        AddNewLine();
    }

    public void DrawBox(ref OpTab tab)
    {
        marginX += new Vector2(-spacing, spacing);
        AddNewLine();

        float boxWidth = marginX.y - marginX.x;
        int lastIndex = boxEndPositions.Count - 1;

        tab.AddItems(new OpRect(pos, new Vector2(boxWidth, boxEndPositions[lastIndex] - pos.y)));
        boxEndPositions.RemoveAt(lastIndex);
    }

    public void DrawKeybinders(Configurable<KeyCode> configurable, ref OpTab tab)
    {
        string name = (string)configurable.info.Tags[0];

        tab.AddItems(
            new OpLabel(new Vector2(115.0f, pos.y), new Vector2(100f, 34f), name)
            {
                alignment = FLabelAlignment.Right,
                verticalAlignment = OpLabel.LabelVAlignment.Center,
                description = configurable.info?.description
            },
            new OpKeyBinder(configurable, new Vector2(235.0f, pos.y), new Vector2(146f, 30f), false)
        );

        AddNewLine(2);
    }

    public void AddCheckBox(Configurable<bool> configurable, string text)
    {
        checkBoxConfigurables.Add(configurable);
        checkBoxesTextLabels.Add(new OpLabel(new Vector2(), new Vector2(), text, FLabelAlignment.Left));
    }

    public void DrawCheckBoxes(ref OpTab tab) // changes pos.y but not pos.x
    {
        if (checkBoxConfigurables.Count != checkBoxesTextLabels.Count) return;

        float width = marginX.y - marginX.x;
        float elementWidth = (width - (numberOfCheckboxes - 1) * 0.5f * spacing) / numberOfCheckboxes;
        pos.y -= checkBoxSize;
        float _posX = pos.x;

        for (int checkBoxIndex = 0; checkBoxIndex < checkBoxConfigurables.Count; ++checkBoxIndex)
        {
            Configurable<bool> configurable = checkBoxConfigurables[checkBoxIndex];
            OpCheckBox checkBox = new(configurable, new Vector2(_posX, pos.y))
            {
                description = configurable.info?.description ?? ""
            };
            tab.AddItems(checkBox);
            _posX += CheckBoxWithSpacing;

            OpLabel checkBoxLabel = checkBoxesTextLabels[checkBoxIndex];
            checkBoxLabel.pos = new Vector2(_posX, pos.y + 2f);
            checkBoxLabel.size = new Vector2(elementWidth - CheckBoxWithSpacing, fontHeight);
            tab.AddItems(checkBoxLabel);

            if (checkBoxIndex < checkBoxConfigurables.Count - 1)
            {
                if ((checkBoxIndex + 1) % numberOfCheckboxes == 0)
                {
                    AddNewLine();
                    pos.y -= checkBoxSize;
                    _posX = pos.x;
                }
                else
                {
                    _posX += elementWidth - CheckBoxWithSpacing + 0.5f * spacing;
                }
            }
        }

        checkBoxConfigurables.Clear();
        checkBoxesTextLabels.Clear();
    }

    public void AddComboBox(Configurable<string> configurable, List<ListItem> list, string text, bool allowEmpty = false)
    {
        OpLabel opLabel = new(new Vector2(), new Vector2(0.0f, fontHeight), text, FLabelAlignment.Center, false);
        comboBoxesTextLabels.Add(opLabel);
        comboBoxConfigurables.Add(configurable);
        comboBoxLists.Add(list);
        comboBoxAllowEmpty.Add(allowEmpty);
    }

    public void DrawComboBoxes(ref OpTab tab)
    {
        if (comboBoxConfigurables.Count != comboBoxesTextLabels.Count) return;
        if (comboBoxConfigurables.Count != comboBoxLists.Count) return;
        if (comboBoxConfigurables.Count != comboBoxAllowEmpty.Count) return;

        float offsetX = (marginX.y - marginX.x) * 0.1f;
        float width = (marginX.y - marginX.x) * 0.4f;

        for (int comboBoxIndex = 0; comboBoxIndex < comboBoxConfigurables.Count; ++comboBoxIndex)
        {
            AddNewLine(1.25f);
            pos.x += offsetX;

            OpLabel opLabel = comboBoxesTextLabels[comboBoxIndex];
            opLabel.pos = pos;
            opLabel.size += new Vector2(width, 2f); // size.y is already set
            pos.x += width;

            Configurable<string> configurable = comboBoxConfigurables[comboBoxIndex];
            OpComboBox comboBox = new(configurable, pos, width, comboBoxLists[comboBoxIndex])
            {
                allowEmpty = comboBoxAllowEmpty[comboBoxIndex],
                description = configurable.info?.description ?? ""
            };
            tab.AddItems(opLabel, comboBox);

            // don't add a new line on the last element
            if (comboBoxIndex < comboBoxConfigurables.Count - 1)
            {
                AddNewLine();
                pos.x = marginX.x;
            }
        }

        comboBoxesTextLabels.Clear();
        comboBoxConfigurables.Clear();
        comboBoxLists.Clear();
        comboBoxAllowEmpty.Clear();
    }

    public void AddSlider(Configurable<int> configurable, string text, string sliderTextLeft = "", string sliderTextRight = "")
    {
        sliderConfigurables.Add(configurable);
        sliderMainTextLabels.Add(text);
        sliderTextLabelsLeft.Add(new OpLabel(new Vector2(), new Vector2(), sliderTextLeft, alignment: FLabelAlignment.Right)); // set pos and size when drawing
        sliderTextLabelsRight.Add(new OpLabel(new Vector2(), new Vector2(), sliderTextRight, alignment: FLabelAlignment.Left));
    }

    public void DrawSliders(ref OpTab tab)
    {
        if (sliderConfigurables.Count != sliderMainTextLabels.Count) return;
        if (sliderConfigurables.Count != sliderTextLabelsLeft.Count) return;
        if (sliderConfigurables.Count != sliderTextLabelsRight.Count) return;

        float width = marginX.y - marginX.x;
        float sliderCenter = marginX.x + 0.5f * width;
        float sliderLabelSizeX = 0.2f * width;
        float sliderSizeX = width - 2f * sliderLabelSizeX - spacing;

        for (int sliderIndex = 0; sliderIndex < sliderConfigurables.Count; ++sliderIndex)
        {
            AddNewLine(2f);

            OpLabel opLabel = sliderTextLabelsLeft[sliderIndex];
            opLabel.pos = new Vector2(marginX.x, pos.y + 5f);
            opLabel.size = new Vector2(sliderLabelSizeX, fontHeight);
            tab.AddItems(opLabel);

            Configurable<int> configurable = sliderConfigurables[sliderIndex];
            OpSlider slider = new(configurable, new Vector2(sliderCenter - 0.5f * sliderSizeX, pos.y), (int)sliderSizeX)
            {
                size = new Vector2(sliderSizeX, fontHeight),
                description = configurable.info?.description ?? ""
            };
            tab.AddItems(slider);

            opLabel = sliderTextLabelsRight[sliderIndex];
            opLabel.pos = new Vector2(sliderCenter + 0.5f * sliderSizeX + 0.5f * spacing, pos.y + 5f);
            opLabel.size = new Vector2(sliderLabelSizeX, fontHeight);
            tab.AddItems(opLabel);

            AddTextLabel(sliderMainTextLabels[sliderIndex]);
            DrawTextLabels(ref tab);

            if (sliderIndex < sliderConfigurables.Count - 1)
            {
                AddNewLine();
            }
        }

        sliderConfigurables.Clear();
        sliderMainTextLabels.Clear();
        sliderTextLabelsLeft.Clear();
        sliderTextLabelsRight.Clear();
    }

    public void AddTextLabel(string text, FLabelAlignment alignment = FLabelAlignment.Center, bool bigText = false)
    {
        float textHeight = (bigText ? 2f : 1f) * fontHeight;
        if (textLabels.Count == 0)
        {
            pos.y -= textHeight;
        }

        OpLabel textLabel = new(new Vector2(), new Vector2(20f, textHeight), text, alignment, bigText) // minimal size.x = 20f
        {
            autoWrap = true
        };
        textLabels.Add(textLabel);
    }

    public void DrawTextLabels(ref OpTab tab)
    {
        if (textLabels.Count == 0) return;

        float width = (marginX.y - marginX.x) / textLabels.Count;
        foreach (OpLabel textLabel in textLabels)
        {
            textLabel.pos = pos;
            textLabel.size += new Vector2(width - 20f, 0.0f);
            tab.AddItems(textLabel);
            pos.x += width;
        }

        pos.x = marginX.x;
        textLabels.Clear();
    }

    #endregion
}