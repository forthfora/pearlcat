using Menu;
using RWCustom;
using static Pearlcat.Enums;

using static Pearlcat.Menu_Helpers;

namespace Pearlcat;

public static class Menu_Hooks
{
    public static void ApplyHooks()
    {
        On.Menu.SlugcatSelectMenu.SlugcatPage.ctor += SlugcatPage_ctor;

        On.Menu.MenuScene.ctor += MenuScene_ctor;
        On.Menu.MenuScene.Update += MenuScene_Update;

        On.Menu.Menu.Update += Menu_Update;

        On.Menu.SlugcatSelectMenu.Update += SlugcatSelectMenu_Update;

        On.HUD.HUD.InitSinglePlayerHud += HUD_InitSinglePlayerHud;
        On.HUD.HUD.InitSafariHud += HUD_InitSafariHud;
        On.ArenaGameSession.AddHUD += ArenaGameSession_AddHUD;

        On.Menu.MenuIllustration.Update += MenuIllustration_Update;
        On.Menu.StoryGameStatisticsScreen.AddBkgIllustration += StoryGameStatisticsScreen_AddBkgIllustration;

        On.Menu.CheckBox.MyColor += CheckBox_MyColor;
        On.Menu.SlugcatSelectMenu.SetChecked += SlugcatSelectMenu_SetChecked;
        On.Menu.SlugcatSelectMenu.GetChecked += SlugcatSelectMenu_GetChecked;

        On.Menu.SlugcatSelectMenu.UpdateStartButtonText += SlugcatSelectMenu_UpdateStartButtonText;
        On.Menu.HoldButton.MyColor += HoldButton_MyColor;

        On.Menu.InputOptionsMenu.ctor += InputOptionsMenu_ctor;
    }


    private static void Menu_Update(On.Menu.Menu.orig_Update orig, Menu.Menu self)
    {
        orig(self);

        MenuPearlAnimStacker++;
    }


    // Statistics screen background illustration
    private static void StoryGameStatisticsScreen_AddBkgIllustration(On.Menu.StoryGameStatisticsScreen.orig_AddBkgIllustration orig, StoryGameStatisticsScreen self)
    {
        if (RainWorld.lastActiveSaveSlot == Enums.Pearlcat)
        {
            var save = Utils.MiscProgression;

            var sceneID = Scenes.Slugcat_Pearlcat_Statistics_Ascended;
            
            if (!save.JustAscended)
            {
                sceneID = Scenes.Slugcat_Pearlcat_Statistics_Sick;
            }

            self.scene = new InteractiveMenuScene(self, self.pages[0], sceneID);
            self.pages[0].subObjects.Add(self.scene);
            return;
        }

        orig(self);
    }


    // Illustration specific conditions
    private static void MenuIllustration_Update(On.Menu.MenuIllustration.orig_Update orig, MenuIllustration self)
    {
        orig(self);

        // Restrict to pearlcat's directory so we don't mess with other mod's stuff
        if (!self.fileName.Contains("pearlcat"))
        {
            return;
        }

        UpdateIllustrationConditionTags(self);

        UpdateIllustrationSpecificBehavior(self);
    }


    // Add Inventory HUD
    private static void ArenaGameSession_AddHUD(On.ArenaGameSession.orig_AddHUD orig, ArenaGameSession self)
    {
        orig(self);

        var hud = self.game.cameras[0].hud;

        hud.AddPart(new InventoryHUD(hud, hud.fContainers[1]));
    }

    private static void HUD_InitSafariHud(On.HUD.HUD.orig_InitSafariHud orig, HUD.HUD self, RoomCamera cam)
    {
        orig(self, cam);

        self.AddPart(new InventoryHUD(self, self.fContainers[1]));
    }

    private static void HUD_InitSinglePlayerHud(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam)
    {
        orig(self, cam);

        self.AddPart(new InventoryHUD(self, self.fContainers[1]));
    }


    // Initialize menu scenes with dynamic pearls
    private static void MenuScene_ctor(On.Menu.MenuScene.orig_ctor orig, MenuScene self, Menu.Menu menu, MenuObject owner, MenuScene.SceneID sceneID)
    {
        orig(self, menu, owner, sceneID);

        if (sceneID is null)
        {
            return;
        }

        if (!sceneID.value.Contains("Pearlcat"))
        {
            return;
        }

        InitMenuPearls(self, sceneID);

        InitMenuPearlIllustrations(self, sceneID);
    }

    private static void MenuScene_Update(On.Menu.MenuScene.orig_Update orig, MenuScene self)
    {
        orig(self);

        var illustrations = self.flatIllustrations.Concat(self.depthIllustrations.ConvertAll(x => (MenuIllustration)x));

        foreach (var illustration in illustrations)
        {
            if (!self.TryGetModule(out var menuSceneModule))
            {
                continue;
            }

            var illustrationModule = illustration.GetModule();

            UpdateDynamicMenuSceneIllustration(self, illustration, menuSceneModule, illustrationModule);
        }
    }


    // Player Select Screen Mark Color, Secret Toggle
    private static void SlugcatPage_ctor(On.Menu.SlugcatSelectMenu.SlugcatPage.orig_ctor orig, SlugcatSelectMenu.SlugcatPage self, Menu.Menu menu, MenuObject owner, int pageIndex, SlugcatStats.Name slugcatNumber)
    {
        orig(self, menu, owner, pageIndex, slugcatNumber);

        if (slugcatNumber != Enums.Pearlcat)
        {
            return;
        }

        var save = Utils.MiscProgression;
        var color = ModOptions.InventoryOverride ? ModOptions.GetOverridenInventory(true).FirstOrDefault()?.GetDataPearlColor() : save.IsNewPearlcatSave ? Pearls.RM_Pearlcat.GetDataPearlColor() : save.StoredActivePearl?.GetPearlColor(true);

        self.effectColor = color ?? Color.white;

        if (SecretIndex == SecretPassword.Length)
        {
            save.IsSecretEnabled = !save.IsSecretEnabled;
            save.HasTrueEnding = save.IsSecretEnabled;

            if (self.menu is SlugcatSelectMenu selectMenu)
            {
                selectMenu.slugcatPageIndex = selectMenu.indexFromColor(Enums.Pearlcat);
            }
        }

        SecretIndex = 0;
    }

    // Story Skip, Secret, Version Warning, MSC/Non MSC Warning
    private static void SlugcatSelectMenu_Update(On.Menu.SlugcatSelectMenu.orig_Update orig, SlugcatSelectMenu self)
    {
        orig(self);

        var page = self.slugcatPages[self.slugcatPageIndex];
        var module = self.GetModule();
        var storySkipCheckbox = module.StoryCheckbox;

        var miscProg = Utils.MiscProgression;
        var disableSave = !miscProg.IsNewPearlcatSave && miscProg.IsMSCSave != ModManager.MSC && !self.restartChecked && !ModCompat_Helpers.RainMeadow_IsOnline;

        var isPearlcatPage = page.slugcatNumber == Enums.Pearlcat;
        var storySkipAvailable = !disableSave && ModCompat_Helpers.IsModEnabled_TheWell && isPearlcatPage && !self.restartChecked && !miscProg.HasTrueEnding && !miscProg.UnlockedTrueEnding && ModManager.MSC;

        if (storySkipAvailable)
        {
            storySkipCheckbox.pos = Vector2.Lerp(storySkipCheckbox.pos, module.CheckboxUpPos, 0.2f);
            storySkipCheckbox.buttonBehav.greyedOut = false;
            storySkipCheckbox.selectable = true;
        }
        else
        {
            storySkipCheckbox.pos = Vector2.Lerp(storySkipCheckbox.pos, module.CheckboxDownPos, 0.2f);
            storySkipCheckbox.buttonBehav.greyedOut = true;
            storySkipCheckbox.selectable = false;
            storySkipCheckbox.Checked = false;
        }


        if (!isPearlcatPage)
        {
            return;
        }

        if (disableSave)
        {
            //self.startButton.buttonBehav.greyedOut = true; // found issues with this, so don't restrict incase of false detection

            self.startButton.fillTime = 120.0f;
            self.startButton.warningMode = true;

            var text = self.Translate("WARNING") + "\n" + (miscProg.IsMSCSave ? self.Translate("MSC").Replace(" ", "\n") : self.Translate("NON-MSC").Replace(" ", "\n")) + self.Translate(" SAVE");

            self.startButton.menuLabel.text = text;
        }
        else
        {
            self.UpdateStartButtonText();
        }


        var canSecretOccur = page is SlugcatSelectMenu.SlugcatPageNewGame && miscProg.IsSecretEnabled == miscProg.HasTrueEnding && !ModCompat_Helpers.RainMeadow_IsOnline; // MSC not technically required

        if (SecretIndex >= SecretPassword.Length)
        {
            self.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.SlugcatSelect);
        }
        else if (canSecretOccur)
        {
            if (Input.anyKey)
            {
                if (Input.GetKey(SecretPassword[SecretIndex].ToString()))
                {
                    SecretIndex++;
                }
                else if (SecretIndex == 0 || !Input.GetKey(SecretPassword[SecretIndex - 1].ToString()))
                {
                    SecretIndex = 0;
                }
            }
        }

        if (page is SlugcatSelectMenu.SlugcatPageContinue continuePage && module.OriginalRegionLabelText is not null)
        {
            var regionLabel = continuePage.regionLabel;

            if (ModCompat_Helpers.ShowTheWellVersionWarning)
            {
                regionLabel.text = Custom.ReplaceLineDelimeters(self.Translate("VERSION WARNING<LINE>'The Well' requires a more recent version of Pearlcat! Please update..."));

                self.startButton.fillTime = 120.0f;
                self.startButton.warningMode = true;
            }
            else if (miscProg.IsStorySkipEnabled)
            {
                regionLabel.text = Custom.ReplaceLineDelimeters(self.Translate("Begin at the start of 'The Well' storyline...<LINE>The world will be preserved, and pearls will carry over!"));

                if (miscProg.IsMSCSave && !miscProg.HasPearlpup)
                {
                    regionLabel.text += self.Translate(" Pearlpup will be revived.");
                }
            }
            else
            {
                regionLabel.text = module.OriginalRegionLabelText;
            }
        }
        else if (page is SlugcatSelectMenu.SlugcatPageNewGame newGamePage)
        {
            if (ModCompat_Helpers.ShowTheWellVersionWarning)
            {
                newGamePage.difficultyLabel.text = self.Translate("VERSION WARNING");
                newGamePage.infoLabel.text = Custom.ReplaceLineDelimeters(self.Translate("'The Well' requires a more recent version of Pearlcat! Please update..."));

                self.startButton.fillTime = 120.0f;
                self.startButton.warningMode = true;
            }
            else if (miscProg.IsSecretEnabled)
            {
                newGamePage.difficultyLabel.text = self.Translate("PEARLPUP");
                newGamePage.infoLabel.text = Custom.ReplaceLineDelimeters(self.Translate("WIP - no new ending yet!<LINE>If you find any bugs, please report them to forthfora!"));
            }
            else
            {
                var infoLabel = newGamePage.infoLabel;

                if (miscProg.IsStorySkipEnabled)
                {
                    infoLabel.text = Custom.ReplaceLineDelimeters(self.Translate("Begin at the start of the 'The Well' storyline...<LINE>The world will be preserved, and pearls will carry over!"));
                }
                else
                {
                    if (SlugBase.SlugBaseCharacter.TryGet(Enums.Pearlcat, out var registry))
                    {
                        infoLabel.text = Custom.ReplaceLineDelimeters(self.Translate(registry.Description));
                    }
                }
            }
        }
    }


    // Story Skip
    private static Color CheckBox_MyColor(On.Menu.CheckBox.orig_MyColor orig, CheckBox self, float timeStacker)
    {
        var result = orig(self, timeStacker);

        if (self.IDString == STORY_SKIP_ID)
        {
            return StorySkipMenuColor;
        }

        return result;
    }

    private static void SlugcatSelectMenu_SetChecked(On.Menu.SlugcatSelectMenu.orig_SetChecked orig, SlugcatSelectMenu self, CheckBox box, bool c)
    {
        var save = Utils.MiscProgression;
        
        if (box.IDString == STORY_SKIP_ID)
        {
            save.IsStorySkipEnabled = c;
            self.UpdateStartButtonText();
            return;
        }

        orig(self, box, c);
    }

    private static bool SlugcatSelectMenu_GetChecked(On.Menu.SlugcatSelectMenu.orig_GetChecked orig, SlugcatSelectMenu self, CheckBox box)
    {
        var result = orig(self, box);
        
        if (box.IDString == STORY_SKIP_ID)
        {
            var save = Utils.MiscProgression;
            return save.IsStorySkipEnabled;
        }

        return result;
    }

    private static void SlugcatSelectMenu_UpdateStartButtonText(On.Menu.SlugcatSelectMenu.orig_UpdateStartButtonText orig, SlugcatSelectMenu self)
    {
        var save = Utils.MiscProgression;

        if (save.IsStorySkipEnabled)
        {
            self.startButton.fillTime = 240.0f;
            self.startButton.menuLabel.text = self.Translate("SKIP");
            return;
        }

        orig(self);
    }

    private static Color HoldButton_MyColor(On.Menu.HoldButton.orig_MyColor orig, HoldButton self, float timeStacker)
    {
        var result = orig(self, timeStacker);

        // If the misc prog check is outside here, it will break it lol
        if (self.signalText == "START")
        {
            var save = Utils.MiscProgression;
            
            if (save.IsStorySkipEnabled)
            {
                return StorySkipMenuColor;
            }
        }

        return result;
    }


    // Refresh IIC Keybinds
    private static void InputOptionsMenu_ctor(On.Menu.InputOptionsMenu.orig_ctor orig, InputOptionsMenu self, ProcessManager manager)
    {
        if (ModCompat_Helpers.IsModEnabled_ImprovedInputConfig)
        {
            ModCompat_Helpers.RefreshIICConfigs();
        }

        orig(self, manager);
    }
}
