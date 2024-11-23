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

        On.HUD.Map.GetItemInShelterFromWorld += Map_GetItemInShelterFromWorld;
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

        if (!sceneID.value.Contains("Pearlcat"))
        {
            return;
        }
       
        var miscProg = Utils.MiscProgression;

        if (ModOptions.InventoryOverride.Value || (miscProg.IsNewPearlcatSave && ModOptions.StartingInventoryOverride.Value))
        {
            var pearls = ModOptions.GetOverridenInventory(true);
            var activePearl = pearls.FirstOrDefault();

            var displayLimit = GetPearlDisplayLimit(sceneID);

            if (pearls.Count > displayLimit)
            {
                pearls.RemoveRange(displayLimit, pearls.Count - displayLimit);
            }

            pearls.Remove(activePearl);

            ModuleManager.MenuSceneData.Add(self, new(pearls.PearlTypeToStoredData(), activePearl?.PearlTypeToStoredData()));
        }
        else if (miscProg.IsNewPearlcatSave)
        {
            List<DataPearl.AbstractDataPearl.DataPearlType> pearls =
            [
                Pearls.AS_PearlBlue,
                Pearls.AS_PearlYellow,
                Pearls.AS_PearlRed,
                Pearls.AS_PearlGreen,
                Pearls.AS_PearlBlack,
            ];

            var activePearl = Pearls.RM_Pearlcat;

            // Replace the active pearl with one of the pearl colors (+ remove it from the list)
            if (miscProg.HasTrueEnding)
            {
                activePearl = Pearls.AS_PearlRed;
                pearls.Remove(activePearl);
            }

            ModuleManager.MenuSceneData.Add(self, new(pearls.PearlTypeToStoredData(), activePearl.PearlTypeToStoredData()));
        }
        else
        {
            var pearls = miscProg.StoredNonActivePearls;
            var displayLimit = GetPearlDisplayLimit(sceneID);

            if (pearls.Count > displayLimit)
            {
                pearls.RemoveRange(displayLimit, pearls.Count - displayLimit);
            }

            ModuleManager.MenuSceneData.Add(self, new(miscProg.StoredNonActivePearls, miscProg.StoredActivePearl));
        }


        if (!self.TryGetModule(out var module))
        {
            return;
        }

        var illustrationFolder = Path.Combine("illustrations", "pearlcat_menupearls");
        var appendTag = "";
        var flatMode = self.flatMode;

        if (sceneID == Scenes.Slugcat_Pearlcat_Ascended)
        {
            appendTag = "_(ascendscene)";
        }

        // Non-Active Pearls
        for (var i = 0; i < module.NonActivePearls.Count; i++)
        {
            var pearlData = module.NonActivePearls[i];

            var illustration = flatMode ? new MenuIllustration(self.menu, self, illustrationFolder, GetPearlIllustration(pearlData.DataPearlType, appendTag), Vector2.zero, false, false)
                : new MenuDepthIllustration(self.menu, self, illustrationFolder, GetPearlIllustration(pearlData.DataPearlType, appendTag), Vector2.zero, -1.0f, MenuDepthIllustration.MenuShader.Basic);

            self.AddIllustration(illustration);

            var isUnique = GetUniquePearlIllustration(pearlData.DataPearlType) is not null;

            illustration.GetModule().Init(illustration, MenuIllustrationModule.IllustrationType.PearlNonActive, i, isUnique);
        }

        // Active Pearl
        if (module.ActivePearl is not null)
        {
            var illustration = flatMode ? new MenuIllustration(self.menu, self, illustrationFolder, GetPearlIllustration(module.ActivePearl.DataPearlType, appendTag), Vector2.zero, false, false)
                : new MenuDepthIllustration(self.menu, self, illustrationFolder, GetPearlIllustration(module.ActivePearl.DataPearlType, appendTag), Vector2.zero, -1.0f, MenuDepthIllustration.MenuShader.Basic);

            self.AddIllustration(illustration);

            var isUnique = GetUniquePearlIllustration(module.ActivePearl.DataPearlType) is not null;

            illustration.GetModule().Init(illustration, MenuIllustrationModule.IllustrationType.PearlActive, hasUniquePearlIllustration: isUnique);


            var haloIllustration = flatMode ? new MenuIllustration(self.menu, self, illustrationFolder, "halo" + appendTag, Vector2.zero, false, false)
                : new MenuDepthIllustration(self.menu, self, illustrationFolder, "halo" + appendTag, Vector2.zero, -1.0f, MenuDepthIllustration.MenuShader.Basic);

            self.AddIllustration(haloIllustration);

            haloIllustration.GetModule().Init(haloIllustration, MenuIllustrationModule.IllustrationType.PearlActiveHalo);
        }

        // Placeholder for when Pearlcat sleeps with no pearls stored
        if (sceneID == Scenes.Slugcat_Pearlcat_Sleep && !miscProg.HasTrueEnding && miscProg.StoredActivePearl is null)
        {
            var illustration = flatMode ? new MenuIllustration(self.menu, self, illustrationFolder, "placeholder", Vector2.zero, false, false)
                : new MenuDepthIllustration(self.menu, self, illustrationFolder, "placeholder", Vector2.zero, -1.0f, MenuDepthIllustration.MenuShader.Basic);

            self.AddIllustration(illustration);

            illustration.GetModule().Init(illustration, MenuIllustrationModule.IllustrationType.PearlPlaceHolder);
        }

        // Pearlpup heart
        if (miscProg.HasTrueEnding)
        {
            if (sceneID == Scenes.Slugcat_Pearlcat_Sleep || sceneID == Scenes.Slugcat_Pearlcat)
            {
                var heartCoreIllustration = flatMode ?new MenuIllustration(self.menu, self, illustrationFolder, "heartcore", Vector2.zero, false, false)
                    : new MenuDepthIllustration(self.menu, self, illustrationFolder, "heartcore", Vector2.zero, -1.0f, MenuDepthIllustration.MenuShader.Basic);

                self.AddIllustration(heartCoreIllustration);

                heartCoreIllustration.GetModule().Init(heartCoreIllustration, MenuIllustrationModule.IllustrationType.PearlHeartCore);

                var heartIllustration = flatMode ? new MenuIllustration(self.menu, self, illustrationFolder, "heart", Vector2.zero, false, false)
                    : new MenuDepthIllustration(self.menu, self, illustrationFolder, "heart", Vector2.zero, -1.0f, MenuDepthIllustration.MenuShader.Basic);

                self.AddIllustration(heartIllustration);

                heartIllustration.GetModule().Init(heartIllustration, MenuIllustrationModule.IllustrationType.PearlHeart);
            }
        }

        foreach (var a in self.depthIllustrations.Concat(self.flatIllustrations))
        {
            Plugin.Logger.LogWarning(Path.GetFileNameWithoutExtension(a.fileName));
        }
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
        var color = ModOptions.InventoryOverride.Value ? ModOptions.GetOverridenInventory(true).FirstOrDefault()?.GetDataPearlColor() : save.IsNewPearlcatSave ? Pearls.RM_Pearlcat.GetDataPearlColor() : save.StoredActivePearl?.GetPearlColor();

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

    // Mira Skip, Secret, Version Warning, MSC/Non MSC Warning
    private static void SlugcatSelectMenu_Update(On.Menu.SlugcatSelectMenu.orig_Update orig, SlugcatSelectMenu self)
    {
        orig(self);

        var page = self.slugcatPages[self.slugcatPageIndex];
        var module = self.GetModule();
        var miraSkipCheckbox = module.MiraCheckbox;

        var miscProg = Utils.MiscProgression;
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


        if (!isPearlcatPage)
        {
            return;
        }

        if (disableSave)
        {
            //self.startButton.buttonBehav.greyedOut = true; // found issues with this, so don't restrict incase of false detection

            self.startButton.fillTime = 240.0f;

            var text = self.Translate("WARNING") + "\n" + (miscProg.IsMSCSave ? self.Translate("MSC").Replace(" ", "\n") : self.Translate("NON-MSC").Replace(" ", "\n")) + self.Translate(" SAVE");

            self.startButton.menuLabel.text = text;
        }


        var canSecretOccur = page is SlugcatSelectMenu.SlugcatPageNewGame && miscProg.IsSecretEnabled == miscProg.HasTrueEnding; // MSC not technically required

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
                regionLabel.text = Custom.ReplaceLineDelimeters(self.Translate("VERSION WARNING<LINE>Mira Installation requires a more recent version of Pearlcat! Please update..."));
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
                newGamePage.infoLabel.text = Custom.ReplaceLineDelimeters(self.Translate("Mira Installation requires a more recent version of Pearlcat! Please update..."));
            }
            else if (miscProg.IsSecretEnabled)
            {
                newGamePage.difficultyLabel.text = self.Translate("PEARLPUP");
                newGamePage.infoLabel.text = Custom.ReplaceLineDelimeters(self.Translate("WIP - no new ending yet!<LINE>If you find any bugs, please report them to forthfora!"));
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


    // Mira Skip
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
        var save = Utils.MiscProgression;
        
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
            var save = Utils.MiscProgression;
            return save.IsMiraSkipEnabled;
        }

        return result;
    }

    private static void SlugcatSelectMenu_UpdateStartButtonText(On.Menu.SlugcatSelectMenu.orig_UpdateStartButtonText orig, SlugcatSelectMenu self)
    {
        var save = Utils.MiscProgression;

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
            var save = Utils.MiscProgression;
            
            if (save.IsMiraSkipEnabled)
            {
                return MiraMenuColor;
            }
        }

        return result;
    }


    // Refresh IIC Keybinds
    private static void InputOptionsMenu_ctor(On.Menu.InputOptionsMenu.orig_ctor orig, InputOptionsMenu self, ProcessManager manager)
    {
        if (ModCompat_Helpers.IsModEnabled_ImprovedInputConfig)
        {
            Input_Helpers.InitIICKeybinds();
        }

        orig(self, manager);
    }


    // Prevent player pearls being displayed to the map in a shelter
    private static HUD.Map.ShelterMarker.ItemInShelterMarker.ItemInShelterData? Map_GetItemInShelterFromWorld(On.HUD.Map.orig_GetItemInShelterFromWorld orig, World world, int room, int index)
    {
        var result = orig(world, room, index);

        var abstractRoom = world.GetAbstractRoom(room);

        if (index < abstractRoom.entities.Count && abstractRoom.entities[index] is AbstractPhysicalObject abstractObject)
        {
            if (abstractObject.realizedObject != null && abstractObject.IsPlayerPearl())
            {
                return null;
            }
        }

        return result;
    }
}
