using Menu.Remix.MixedUI;

namespace Pearlcat;

public sealed partial class ModOptionInterface
{
    private const int TAB_COUNT = 7;

    public static Color WarnRed { get; } = new(0.85f, 0.35f, 0.4f);


    public override void Initialize()
    {
        base.Initialize();

        Tabs = new OpTab[TAB_COUNT];
        var tabIndex = -1;

        InitGeneral(ref tabIndex);

        InitAbilityInput(ref tabIndex);
        InitSwapInput(ref tabIndex);
        InitStoreInput(ref tabIndex);

        InitDifficulty(ref tabIndex);
        InitCheats(ref tabIndex);
        InitExtraCheats(ref tabIndex);
    }

    private void InitGeneral(ref int tabIndex)
    {
        AddTab(ref tabIndex, "General");


        // CREDITS
        AddAndDrawLargeDivider(ref Tabs[tabIndex]);

        AddTextLabel("CREDITS", bigText: true);
        DrawTextLabels(ref Tabs[tabIndex]);

        AddAndDrawLargeDivider(ref Tabs[tabIndex]);

        AddNewLine();

        AddTextLabel("Geahgeah " + Translate("- Artwork"), translate: false);
        AddTextLabel("Sidera " + Translate("- Dialogue, SFX"), translate: false);
        AddTextLabel("Noir " + Translate("- Floppy Ears, Scarf"), translate: false);
        DrawTextLabels(ref Tabs[tabIndex]);

        AddNewLine();

        AddTextLabel("Kimi " + Translate("- Additional Artwork"), translate: false);
        AddTextLabel("Lin " + Translate("- Chinese Translation"), translate: false);
        AddTextLabel("zbiotr " + Translate("- Spanish Translation"), translate: false);
        DrawTextLabels(ref Tabs[tabIndex]);

        AddNewLine();


        // PLAYTESTERS
        AddNewLine();
        AddAndDrawLargeDivider(ref Tabs[tabIndex]);

        AddTextLabel("PLAYTESTERS", bigText: true);
        DrawTextLabels(ref Tabs[tabIndex]);

        AddAndDrawLargeDivider(ref Tabs[tabIndex]);

        AddNewLine();

        AddTextLabel("TurtleMan27", translate: false);
        AddTextLabel("Elliot", translate: false);
        AddTextLabel("Balagaga", translate: false);
        DrawTextLabels(ref Tabs[tabIndex]);

        AddNewLine();

        AddTextLabel("Efi", translate: false);
        AddTextLabel("WillowWisp", translate: false);
        AddTextLabel("Lolight2", translate: false);
        AddTextLabel("mayhemmm", translate: false);
        DrawTextLabels(ref Tabs[tabIndex]);


        // OPTIONS
        AddNewLine(0.5f);

        AddCheckBox(PearlThreatMusic);
        AddCheckBox(CompactInventoryHUD);
        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddCheckBox(DisableTutorials);
        AddCheckBox(DisableCosmetics);
        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddNewLine(0.5f);


        DrawBox(ref Tabs[tabIndex]);


        if (GetConfigurable(DisableCosmetics, out OpCheckBox checkBox))
        {
            checkBox.colorEdge = WarnRed;
        }

        if (GetConfigurable(DisableTutorials, out checkBox))
        {
            checkBox.colorEdge = WarnRed;
        }


        if (GetLabel(DisableCosmetics, out var label))
        {
            label.color = WarnRed;
        }

        if (GetLabel(DisableTutorials, out label))
        {
            label.color = WarnRed;
        }
    }

    private void InitAbilityInput(ref int tabIndex)
    {
        AddTab(ref tabIndex, "Ability Input");

        AddCheckBox(CustomSpearKeybind);
        AddCheckBox(CustomAgilityKeybind);
        DrawCheckBoxes(ref Tabs[tabIndex]);

        if (!ModCompat_Helpers.IsIICActive)
        {
            AddNewLine(0.75f);
            AddAndDrawLargeDivider(ref Tabs[tabIndex]);
            AddNewLine(0.25f);

            AddNewLine(2);

            var abilityOffset = new Vector2(-100.0f, 0.0f);
            var sentryOffset = new Vector2(140.0f, 0.0f);

            AddAndDrawKeybinder(AbilityKeybindKeyboard, ref Tabs[tabIndex], abilityOffset, false);
            AddAndDrawKeybinder(SentryKeybindKeyboard, ref Tabs[tabIndex], sentryOffset);
            AddNewLine();

            AddAndDrawKeybinder(AbilityKeybindPlayer1, ref Tabs[tabIndex], abilityOffset, false);
            AddAndDrawKeybinder(SentryKeybindPlayer1, ref Tabs[tabIndex], sentryOffset);
            AddNewLine();

            AddAndDrawKeybinder(AbilityKeybindPlayer2, ref Tabs[tabIndex], abilityOffset, false);
            AddAndDrawKeybinder(SentryKeybindPlayer2, ref Tabs[tabIndex], sentryOffset);
            AddNewLine();

            AddAndDrawKeybinder(AbilityKeybindPlayer3, ref Tabs[tabIndex], abilityOffset, false);
            AddAndDrawKeybinder(SentryKeybindPlayer3, ref Tabs[tabIndex], sentryOffset);
            AddNewLine();

            AddAndDrawKeybinder(AbilityKeybindPlayer4, ref Tabs[tabIndex], abilityOffset, false);
            AddAndDrawKeybinder(SentryKeybindPlayer4, ref Tabs[tabIndex], sentryOffset);

            AddNewLine(-2);

            AddNewLine();
            AddAndDrawLargeDivider(ref Tabs[tabIndex]);
            AddNewLine(-1);
        }


        AddCheckBox(CustomSentryKeybind);

        if (ModCompat_Helpers.IsModEnabled_ImprovedInputConfig)
        {
            AddCheckBox(DisableImprovedInputConfig);
        }

        DrawCheckBoxes(ref Tabs[tabIndex]);


        if (ModCompat_Helpers.IsIICActive)
        {
            AddNewLine();
            AddAndDrawLargeDivider(ref Tabs[tabIndex]);
            AddNewLine(-1);

            AddNewLine(6);

            AddTextLabel("Improved Input Config is active!", bigText: true);
            DrawTextLabels(ref Tabs[tabIndex]);

            AddTextLabel("Edit keybinds through the normal input menu.");
            DrawTextLabels(ref Tabs[tabIndex]);

            AddNewLine(6);
        }

        DrawBox(ref Tabs[tabIndex]);


        if (GetLabel(DisableImprovedInputConfig, out var label))
        {
            label.color = WarnRed;
        }

        if (GetConfigurable(DisableImprovedInputConfig, out OpCheckBox checkBox))
        {
            checkBox.colorEdge = WarnRed;
        }
    }

    private void InitSwapInput(ref int tabIndex)
    {
        AddTab(ref tabIndex, "Swap Input");

        if (ModCompat_Helpers.IsIICActive)
        {
            AddNewLine(8);

            AddTextLabel("Improved Input Config is active!", bigText: true);
            DrawTextLabels(ref Tabs[tabIndex]);

            AddTextLabel("Edit keybinds through the normal input menu.");
            DrawTextLabels(ref Tabs[tabIndex]);

            AddNewLinesUntilEnd();
        }
        else
        {
            AddDragger(SwapTriggerPlayer);
            DrawDraggers(ref Tabs[tabIndex], offsetX: 150.0f);

            AddNewLine();
            AddAndDrawLargeDivider(ref Tabs[tabIndex]);
            AddNewLine(-1);

            AddNewLine(4);

            AddAndDrawKeybinder(SwapLeftKeybind, ref Tabs[tabIndex]);
            AddAndDrawKeybinder(SwapRightKeybind, ref Tabs[tabIndex]);

            AddNewLine(-1);
            AddAndDrawLargeDivider(ref Tabs[tabIndex]);
            AddNewLine(2.5f);

            AddAndDrawKeybinder(SwapKeybindKeyboard, ref Tabs[tabIndex]);
            AddAndDrawKeybinder(SwapKeybindPlayer1, ref Tabs[tabIndex]);
            AddAndDrawKeybinder(SwapKeybindPlayer2, ref Tabs[tabIndex]);
            AddAndDrawKeybinder(SwapKeybindPlayer3, ref Tabs[tabIndex]);
            AddAndDrawKeybinder(SwapKeybindPlayer4, ref Tabs[tabIndex]);

            AddNewLine(-1.5f);
        }

        DrawBox(ref Tabs[tabIndex]);
    }

    private void InitStoreInput(ref int tabIndex)
    {
        AddTab(ref tabIndex, "Store Input");

        AddCheckBox(UsesCustomStoreKeybind);
        DrawCheckBoxes(ref Tabs[tabIndex], 150.0f);

        AddNewLine();
        AddAndDrawLargeDivider(ref Tabs[tabIndex]);
        AddNewLine(-1);

        if (ModCompat_Helpers.IsIICActive)
        {
            AddNewLine(7);

            AddTextLabel("Improved Input Config is active!", bigText: true);
            DrawTextLabels(ref Tabs[tabIndex]);

            AddTextLabel("Edit keybinds through the normal input menu.");
            DrawTextLabels(ref Tabs[tabIndex]);

            AddNewLinesUntilEnd();
        }
        else
        {
            AddNewLine(4);

            AddAndDrawKeybinder(StoreKeybindKeyboard, ref Tabs[tabIndex]);
            AddNewLine();

            AddAndDrawKeybinder(StoreKeybindPlayer1, ref Tabs[tabIndex]);
            AddNewLine();

            AddAndDrawKeybinder(StoreKeybindPlayer2, ref Tabs[tabIndex]);
            AddNewLine();

            AddAndDrawKeybinder(StoreKeybindPlayer3, ref Tabs[tabIndex]);
            AddNewLine();

            AddAndDrawKeybinder(StoreKeybindPlayer4, ref Tabs[tabIndex]);
        }

        DrawBox(ref Tabs[tabIndex]);
    }

    private void InitDifficulty(ref int tabIndex)
    {
        AddTab(ref tabIndex, "Difficulty");
        Tabs[tabIndex].colorButton = WarnRed;

        var warningText = "Intended to make gameplay more challenging, may change gameplay significantly!";
        AddTextLabel(warningText, bigText: true);
        DrawTextLabels(ref Tabs[tabIndex]);


        AddNewLine();
        AddAndDrawLargeDivider(ref Tabs[tabIndex], color: WarnRed);
        AddNewLine(-1);


        AddCheckBox(InventoryPings);
        AddCheckBox(HidePearls);
        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddCheckBox(DisableMinorEffects);
        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddNewLine();
        AddAndDrawLargeDivider(ref Tabs[tabIndex], color: WarnRed);
        AddNewLine(-1);


        AddCheckBox(DisableAgility);
        AddCheckBox(DisableCamoflague);
        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddCheckBox(DisableRage);
        AddCheckBox(DisableRevive);
        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddCheckBox(DisableShield);
        AddCheckBox(DisableSpear);
        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddNewLine();
        AddAndDrawLargeDivider(ref Tabs[tabIndex], color: WarnRed);
        AddNewLine(-1);

        AddDragger(VisibilityMultiplier);
        DrawDraggers(ref Tabs[tabIndex]);

        AddNewLine(-3);

        AddCheckBox(OldRedPearlAbility);
        DrawCheckBoxes(ref Tabs[tabIndex], 235.0f);

        AddNewLine();
        DrawBox(ref Tabs[tabIndex]);


        if (GetLabel(warningText, out var label))
        {
            label.color = WarnRed;
        }


        if (GetLabel(DisableMinorEffects, out label))
        {
            label.color = WarnRed;
        }

        if (GetLabel(DisableAgility, out label))
        {
            label.color = Color.cyan;
        }

        if (GetLabel(DisableCamoflague, out label))
        {
            label.color = Color.grey;
        }

        if (GetLabel(DisableRage, out label))
        {
            label.color = Color.red;
        }

        if (GetLabel(DisableRevive, out label))
        {
            label.color = Color.green;
        }

        if (GetLabel(DisableShield, out label))
        {
            label.color = Color.yellow;
        }

        if (GetLabel(DisableSpear, out label))
        {
            label.color = Color.white;
        }


        if (GetConfigurable(DisableMinorEffects, out OpCheckBox checkBox))
        {
            checkBox.colorEdge = WarnRed;
        }

        if (GetConfigurable(DisableAgility, out checkBox))
        {
            checkBox.colorEdge = Color.cyan;
        }

        if (GetConfigurable(DisableCamoflague, out checkBox))
        {
            checkBox.colorEdge = Color.grey;
        }

        if (GetConfigurable(DisableRage, out checkBox))
        {
            checkBox.colorEdge = Color.red;
        }

        if (GetConfigurable(DisableRevive, out checkBox))
        {
            checkBox.colorEdge = Color.green;
        }

        if (GetConfigurable(DisableShield, out checkBox))
        {
            checkBox.colorEdge = Color.yellow;
        }

        if (GetConfigurable(DisableSpear, out checkBox))
        {
            checkBox.colorEdge = Color.white;
        }

        if (GetLabel(VisibilityMultiplier, out label))
        {
            label.color = WarnRed;
        }

        if (GetConfigurable(VisibilityMultiplier, out OpDragger dragger))
        {
            dragger.colorEdge = WarnRed;
            dragger.colorText = WarnRed;
        }


        if (GetLabel(OldRedPearlAbility, out label))
        {
            label.color = Color.red;
        }

        if (GetConfigurable(OldRedPearlAbility, out checkBox))
        {
            checkBox.colorEdge = Color.red;
        }
    }

    private void InitCheats(ref int tabIndex)
    {
        AddTab(ref tabIndex, "Cheats");
        Tabs[tabIndex].colorButton = WarnRed;

        var warningText = Translate("Intended for fun, may change gameplay significantly!");

        AddTextLabel(warningText, bigText: true);
        DrawTextLabels(ref Tabs[tabIndex]);


        AddNewLine();
        AddAndDrawLargeDivider(ref Tabs[tabIndex], color: WarnRed);
        AddNewLine(-1);


        AddDragger(MaxPearlCount);
        DrawDraggers(ref Tabs[tabIndex]);

        var offset = new Vector2(100.0f, 109.0f);

        var startShelterOverride = new OpTextBox(StartShelterOverride, new Vector2(165.0f, 259.0f) + offset, 90.0f)
        {
            colorEdge = WarnRed,
            colorText = WarnRed,
        };

        var startShelterOverrideLabel = new OpLabel(new Vector2(230.0f, 210.0f) + offset,
            new Vector2(150f, 16.0f) + offset, Translate(StartShelterOverride.info.Tags[0].ToString()))
        {
            color = WarnRed,
        };

        Tabs[tabIndex].AddItems(startShelterOverride, startShelterOverrideLabel);


        AddCheckBox(PearlpupRespawn);
        AddCheckBox(EnableBackSpear);
        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddNewLine();
        AddAndDrawLargeDivider(ref Tabs[tabIndex], color: WarnRed);
        AddNewLine(-1);

        // lazy fix
        AddCheckBox(InventoryOverride);
        AddCheckBox(StartingInventoryOverride);
        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddDragger(AgilityPearlCount);
        AddDragger(CamoPearlCount);
        DrawDraggers(ref Tabs[tabIndex]);

        AddDragger(RagePearlCount);
        AddDragger(RevivePearlCount);
        DrawDraggers(ref Tabs[tabIndex]);

        AddDragger(ShieldPearlCount);
        AddDragger(SpearPearlCount);
        DrawDraggers(ref Tabs[tabIndex]);

        AddNewLine();
        DrawBox(ref Tabs[tabIndex]);


        if (GetLabel(warningText, out var label))
        {
            label.color = WarnRed;
        }

        if (GetLabel(InventoryOverride, out label))
        {
            label.color = WarnRed;
        }

        if (GetLabel(StartingInventoryOverride, out label))
        {
            label.color = WarnRed;
        }

        if (GetLabel(MaxPearlCount, out label))
        {
            label.color = WarnRed;
        }

        if (GetLabel(PearlpupRespawn, out label))
        {
            label.color = WarnRed;
        }

        if (GetLabel(EnableBackSpear, out label))
        {
            label.color = WarnRed;
        }

        if (GetLabel(AgilityPearlCount, out label))
        {
            label.color = Color.cyan;
        }

        if (GetLabel(CamoPearlCount, out label))
        {
            label.color = Color.grey;
        }

        if (GetLabel(RagePearlCount, out label))
        {
            label.color = Color.red;
        }

        if (GetLabel(RevivePearlCount, out label))
        {
            label.color = Color.green;
        }

        if (GetLabel(ShieldPearlCount, out label))
        {
            label.color = Color.yellow;
        }

        if (GetLabel(SpearPearlCount, out label))
        {
            label.color = Color.white;
        }


        if (GetConfigurable(InventoryOverride, out OpCheckBox checkBox))
        {
            checkBox.colorEdge = WarnRed;
        }

        if (GetConfigurable(StartingInventoryOverride, out checkBox))
        {
            checkBox.colorEdge = WarnRed;
        }

        if (GetConfigurable(PearlpupRespawn, out checkBox))
        {
            checkBox.colorEdge = WarnRed;
        }

        if (GetConfigurable(EnableBackSpear, out checkBox))
        {
            checkBox.colorEdge = WarnRed;
        }


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

    private void InitExtraCheats(ref int tabIndex)
    {
        AddTab(ref tabIndex, "Extra Cheats");
        Tabs[tabIndex].colorButton = WarnRed;

        var text = Translate("All times here are in frames.<LINE>40 frames = 1 second.");
        AddTextLabel(text);
        DrawTextLabels(ref Tabs[tabIndex]);

        AddNewLine();

        AddIntSlider(ShieldRechargeTime, sliderTextLeft: "40", sliderTextRight: "3600");
        AddIntSlider(ShieldDuration, sliderTextLeft: "5", sliderTextRight: "300");

        if (OldRedPearlAbility.Value)
        {
            AddIntSlider(LaserWindupTime, sliderTextLeft: "5", sliderTextRight: "300");
            AddIntSlider(LaserRechargeTime, sliderTextLeft: "5", sliderTextRight: "300");
            AddFloatSlider(LaserDamage, sliderTextLeft: "0.0", sliderTextRight: "3.0");

            DrawIntSliders(ref Tabs[tabIndex]);
            DrawFloatSliders(ref Tabs[tabIndex]);
        }
        else
        {
            DrawIntSliders(ref Tabs[tabIndex]);

            AddNewLine(11);
        }


        AddNewLine();

        if (GetLabel(text, out var label))
        {
            label.color = WarnRed;
        }

        if (GetConfigurable(ShieldRechargeTime, out OpSlider slider))
        {
            slider.colorEdge = slider.colorLine = Color.yellow;
        }

        if (GetConfigurable(ShieldDuration, out slider))
        {
            slider.colorEdge = slider.colorLine = Color.yellow;
        }


        if (OldRedPearlAbility.Value)
        {
            if (GetConfigurable(LaserWindupTime, out slider))
            {
                slider.colorEdge = slider.colorLine = Color.red;
            }

            if (GetConfigurable(LaserRechargeTime, out slider))
            {
                slider.colorEdge = slider.colorLine = Color.red;
            }

            if (GetConfigurable(LaserDamage, out OpFloatSlider floatSlider))
            {
                floatSlider.colorEdge = floatSlider.colorLine = Color.red;
            }
        }

        DrawBox(ref Tabs[tabIndex]);
    }
}
