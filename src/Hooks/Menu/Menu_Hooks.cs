using Menu;
using RWCustom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
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


    private static void StoryGameStatisticsScreen_AddBkgIllustration(On.Menu.StoryGameStatisticsScreen.orig_AddBkgIllustration orig, StoryGameStatisticsScreen self)
    {

        if (RainWorld.lastActiveSaveSlot == Enums.Pearlcat)
        {
            var save = Utils.GetMiscProgression();

            var sceneID = Scenes.Slugcat_Pearlcat_Statistics_Ascended;
            
            if (!save.JustAscended)
            {
                sceneID = Scenes.Slugcat_Pearlcat_Statistics_Sick;
            }

            self.scene = new InteractiveMenuScene(self, self.pages[0], sceneID);
            self.pages[0].subObjects.Add(self.scene);

            Plugin.Logger.LogInfo("PEARLCAT STATISTICS SCREEN: " + sceneID);
            return;
        }

        orig(self);
    }

    private static void MenuIllustration_Update(On.Menu.MenuIllustration.orig_Update orig, MenuIllustration self)
    {
        orig(self);

        if (!self.fileName.Contains("pearlcat")) return;

        var miscProg = Utils.GetMiscProgression();
        var fileName = Path.GetFileNameWithoutExtension(self.fileName);

        // INTRO
        if (fileName == "Intro6")
        {
            self.visible = !ModCompat_Helpers.IsModEnabled_MiraInstallation;
        }
        if (fileName == "Intro6_Mira")
        {
            self.visible = ModCompat_Helpers.IsModEnabled_MiraInstallation;
        }

        // VOID SEA ENDING
        if (fileName == "Outro3_1" || fileName == "Outro2_1")
        {
            self.visible = miscProg.HasPearlpup;
        }

        // OUTER EXPANSE ENDING
        if (fileName == "AltOutro10_1")
        {
            if (self.alpha == 1.0f)
            {
                self.alpha = 0.0f;
            }

            self.alpha = Mathf.Lerp(self.alpha, 0.99f, 0.015f);
        }
    }

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


    private static void MenuScene_ctor(On.Menu.MenuScene.orig_ctor orig, MenuScene self, Menu.Menu menu, MenuObject owner, MenuScene.SceneID sceneID)
    {
        orig(self, menu, owner, sceneID);
       
        var save = Utils.GetMiscProgression();

        if (ModOptions.InventoryOverride.Value || (save.IsNewPearlcatSave && ModOptions.StartingInventoryOverride.Value))
        {
            var pearls = ModOptions.GetOverridenInventory(true);
            var activePearl = pearls.FirstOrDefault();

            if (pearls.Count > 11)
                pearls.RemoveRange(11, pearls.Count - 11);

            pearls.Remove(activePearl);

            List<Color> pearlColors = new();

            foreach (var pearl in pearls)
                pearlColors.Add(pearl.GetDataPearlColor());

            ModuleManager.MenuSceneData.Add(self, new(pearlColors, activePearl?.GetDataPearlColor()));
        }
        else if (save.IsNewPearlcatSave)
        {
            List<Color> pearlColors = new()
            {
                Pearls.AS_PearlBlue.GetDataPearlColor(),
                Pearls.AS_PearlYellow.GetDataPearlColor(),
                Pearls.AS_PearlRed.GetDataPearlColor(),
                Pearls.AS_PearlGreen.GetDataPearlColor(),
                Pearls.AS_PearlBlack.GetDataPearlColor(),
            };
            
            var activeColor = Pearls.RM_Pearlcat.GetDataPearlColor();

            if (save.HasTrueEnding)
            {
                activeColor = Pearls.AS_PearlRed.GetDataPearlColor();
                pearlColors.Remove((activeColor));
            }

            ModuleManager.MenuSceneData.Add(self, new(pearlColors, activeColor));
        }
        else
        {
            var pearls = save.StoredPearlColors;

            if (pearls.Count > 11)
                pearls.RemoveRange(11, pearls.Count - 11);

            ModuleManager.MenuSceneData.Add(self, new(save.StoredPearlColors, save.ActivePearlColor));
        }


        var illustrations = self.flatIllustrations.Concat(self.depthIllustrations);
        
        foreach (var illustration in illustrations)
        {
            var fileName = Path.GetFileNameWithoutExtension(illustration.fileName);

            var index = -2;

            if (fileName.Contains("pearl"))
            {
                var indexString = fileName.Replace("pearl", "");

                if (!int.TryParse(indexString, out index))
                {
                    if (fileName == "pearlactive" || fileName == "pearlactiveplaceholder" || fileName == "pearlactivehalo")
                    {
                        index = -1;
                    }
                }
            }

            ModuleManager.MenuIllustrationData.Add(illustration, new(illustration, index));
        }
    }

    private static void MenuScene_Update(On.Menu.MenuScene.orig_Update orig, MenuScene self)
    {
        orig(self);

        var illustrations = self.flatMode ? self.flatIllustrations : self.depthIllustrations.ConvertAll(x => (MenuIllustration)x);

        foreach (var illustration in illustrations)
        {
            if (!ModuleManager.MenuSceneData.TryGetValue(self, out var menuSceneModule)) continue;

            if (!ModuleManager.MenuIllustrationData.TryGetValue(illustration, out var illustrationModule)) continue;

            if (self.sceneID == Scenes.Slugcat_Pearlcat)
            {
                UpdateSelectScreen(self, illustration, menuSceneModule, illustrationModule);
            }
            else if (self.sceneID == Scenes.Slugcat_Pearlcat_Sleep)
            {
                UpdateSleepScreen(self, illustration, menuSceneModule, illustrationModule);
            }
            else if (self.sceneID == Scenes.Slugcat_Pearlcat_Ascended)
            {
                UpdateAscendedScreen(self, illustration, menuSceneModule, illustrationModule);
            }
            else if (self.sceneID == Scenes.Slugcat_Pearlcat_Sick)
            {
                UpdateSickScreen(self, illustration, menuSceneModule, illustrationModule);
            }
        }
    }


    private static void SlugcatPage_ctor(On.Menu.SlugcatSelectMenu.SlugcatPage.orig_ctor orig, SlugcatSelectMenu.SlugcatPage self, Menu.Menu menu, MenuObject owner, int pageIndex, SlugcatStats.Name slugcatNumber)
    {
        orig(self, menu, owner, pageIndex, slugcatNumber);

        if (slugcatNumber != Enums.Pearlcat) return;

        var save = Utils.GetMiscProgression();
        var color = ModOptions.InventoryOverride.Value ? ModOptions.GetOverridenInventory(true).FirstOrDefault().GetDataPearlColor() : save.IsNewPearlcatSave ? Pearls.RM_Pearlcat.GetDataPearlColor() : save.ActivePearlColor;

        // screw pebbles pearls you get ORANGE    
        self.effectColor = color ?? Color.white;

        self.markOffset = save.HasPearlpup ? new(0.0f, 50.0f) : new(20.0f, 50.0f);

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

    private static void SlugcatSelectMenu_Update(On.Menu.SlugcatSelectMenu.orig_Update orig, SlugcatSelectMenu self)
    {
        orig(self);

        var page = self.slugcatPages[self.slugcatPageIndex];
        var module = self.GetModule();
        var miraSkipCheckbox = module.MiraCheckbox;

        var miscProg = Utils.GetMiscProgression();
        var disableSave = !miscProg.IsNewPearlcatSave && miscProg.IsMSCSave != ModManager.MSC && !self.restartChecked;

        var isPearlcatPage = page.slugcatNumber == Enums.Pearlcat;
        var miraSkipAvailable = !disableSave && ModCompat_Helpers.IsModEnabled_MiraInstallation && isPearlcatPage && !self.restartChecked && !miscProg.HasTrueEnding && !miscProg.UnlockedMira && ModManager.MSC;

        if (miraSkipAvailable)
        {
            miraSkipCheckbox.pos = Vector2.Lerp(miraSkipCheckbox.pos, module.CheckboxUpPos, 0.2f);
            miraSkipCheckbox.buttonBehav.greyedOut = false;
            miraSkipCheckbox.selectable = true;
        }
        else
        {
            miraSkipCheckbox.pos = Vector2.Lerp(miraSkipCheckbox.pos, module.CheckboxDownPos, 0.2f);
            miraSkipCheckbox.buttonBehav.greyedOut = true;
            miraSkipCheckbox.selectable = false;
            miraSkipCheckbox.Checked = false;
        }


        if (!isPearlcatPage) return;

        if (disableSave)
        {
            //self.startButton.buttonBehav.greyedOut = true; // found issues with this, so don't restrict incase of false detection

            self.startButton.fillTime = 240.0f;

            var text = self.Translate("WARNING") + "\n" + (miscProg.IsMSCSave ? self.Translate("MSC").Replace(" ", "\n") : self.Translate("NON-MSC").Replace(" ", "\n")) + self.Translate(" SAVE");

            self.startButton.menuLabel.text = text;
        }


        var canSecretOccur = page is SlugcatSelectMenu.SlugcatPageNewGame && miscProg.IsSecretEnabled == miscProg.HasTrueEnding && (ModManager.MSC || miscProg.IsSecretEnabled);

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

        if (page is SlugcatSelectMenu.SlugcatPageContinue continuePage && module.OriginalRegionLabelText != null)
        {
            var regionLabel = continuePage.regionLabel;

            if (ModCompat_Helpers.ShowMiraVersionWarning)
            {
                regionLabel.text = Custom.ReplaceLineDelimeters(self.Translate("VERSION WARNING<LINE>Mira Installation requires Pearlcat version 1.3.0 or above! Please update..."));
            }
            else if (miscProg.IsMiraSkipEnabled)
            {
                regionLabel.text = Custom.ReplaceLineDelimeters(self.Translate("Begin at the start of the Mira storyline...<LINE>The world will be preserved, and pearls will carry over!"));

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
            if (ModCompat_Helpers.ShowMiraVersionWarning)
            {
                newGamePage.difficultyLabel.text = self.Translate("VERSION WARNING");
                newGamePage.infoLabel.text = Custom.ReplaceLineDelimeters(self.Translate("Mira Installation requires Pearlcat version 1.3.0 or above! Please update..."));
            }
            else if (miscProg.IsSecretEnabled)
            {
                newGamePage.difficultyLabel.text = self.Translate("PEARLPUP");
                newGamePage.infoLabel.text = Custom.ReplaceLineDelimeters(self.Translate("WIP - no new ending yet!<LINE>If you find any bugs, please report them to forthbridge!"));
            }
            else
            {
                var infoLabel = newGamePage.infoLabel;

                if (miscProg.IsMiraSkipEnabled)
                {
                    infoLabel.text = Custom.ReplaceLineDelimeters(self.Translate("Begin at the start of the Mira storyline...<LINE>The world will be preserved, and pearls will carry over!"));
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


    private static Color CheckBox_MyColor(On.Menu.CheckBox.orig_MyColor orig, CheckBox self, float timeStacker)
    {
        var result = orig(self, timeStacker);

        if (self.IDString == MIRA_SKIP_ID)
        {
            return MiraMenuColor;
        }

        return result;
    }

    private static void SlugcatSelectMenu_SetChecked(On.Menu.SlugcatSelectMenu.orig_SetChecked orig, SlugcatSelectMenu self, CheckBox box, bool c)
    {
        var save = Utils.GetMiscProgression();
        
        if (box.IDString == MIRA_SKIP_ID)
        {
            save.IsMiraSkipEnabled = c;
            self.UpdateStartButtonText();
            return;
        }

        orig(self, box, c);
    }

    private static bool SlugcatSelectMenu_GetChecked(On.Menu.SlugcatSelectMenu.orig_GetChecked orig, SlugcatSelectMenu self, CheckBox box)
    {
        var result = orig(self, box);
        
        if (box.IDString == MIRA_SKIP_ID)
        {
            var save = Utils.GetMiscProgression();
            return save.IsMiraSkipEnabled;
        }

        return result;
    }

    private static void SlugcatSelectMenu_UpdateStartButtonText(On.Menu.SlugcatSelectMenu.orig_UpdateStartButtonText orig, SlugcatSelectMenu self)
    {
        var save = Utils.GetMiscProgression();

        if (save.IsMiraSkipEnabled)
        {
            self.startButton.fillTime = 240.0f;
            self.startButton.menuLabel.text = self.Translate("FIND MIRA...");
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
            var save = Utils.GetMiscProgression();
            
            if (save.IsMiraSkipEnabled)
            {
                return MiraMenuColor;
            }
        }

        return result;
    }


    private static void InputOptionsMenu_ctor(On.Menu.InputOptionsMenu.orig_ctor orig, InputOptionsMenu self, ProcessManager manager)
    {
        if (ModCompat_Helpers.IsModEnabled_ImprovedInputConfig)
        {
            Input_Helpers.InitIICKeybinds();
        }

        orig(self, manager);
    }
}
