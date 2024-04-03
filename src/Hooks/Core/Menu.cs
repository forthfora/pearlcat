using Menu;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static Pearlcat.Enums;

namespace Pearlcat;

public static partial class Hooks
{
    public static void ApplyMenuHooks()
    {
        On.Menu.SlugcatSelectMenu.SlugcatPage.ctor += SlugcatPage_ctor;

        On.Menu.MenuScene.ctor += MenuScene_ctor;
        On.Menu.MenuScene.Update += MenuScene_Update;

        On.Menu.Menu.Update += Menu_Update;

        On.Menu.SlugcatSelectMenu.Update += SlugcatSelectMenu_Update;

        new Hook(
            typeof(SlugcatSelectMenu.SlugcatPage).GetProperty(nameof(SlugcatSelectMenu.SlugcatPage.HasMark), BindingFlags.Instance | BindingFlags.Public).GetGetMethod(),
            typeof(Hooks).GetMethod(nameof(GetSlugcatPageHasMark), BindingFlags.Static | BindingFlags.Public)
        );

        new Hook(
            typeof(SlugcatSelectMenu.SlugcatPage).GetProperty(nameof(SlugcatSelectMenu.SlugcatPage.HasGlow), BindingFlags.Instance | BindingFlags.Public).GetGetMethod(),
            typeof(Hooks).GetMethod(nameof(GetSlugcatPageHasGlow), BindingFlags.Static | BindingFlags.Public)
        );

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

        //IL.Menu.SlugcatSelectMenu.StartGame += SlugcatSelectMenu_StartGame;
    }


    public static int MenuPearlAnimStacker { get; set; } = 0;

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
            self.visible = !Utils.IsMiraActive;
        }
        if (fileName == "Intro6_Mira")
        {
            self.visible = Utils.IsMiraActive;
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



    public delegate bool orig_SlugcatPageHasMark(SlugcatSelectMenu.SlugcatPage self);
    public static bool GetSlugcatPageHasMark(orig_SlugcatPageHasMark orig, SlugcatSelectMenu.SlugcatPage self)
    {
        var result = orig(self);

        if (self.slugcatNumber == Enums.Pearlcat)
            return true;

        return result;
    }

    public delegate bool orig_SlugcatPageHasGlow(SlugcatSelectMenu.SlugcatPage self);
    public static bool GetSlugcatPageHasGlow(orig_SlugcatPageHasGlow orig, SlugcatSelectMenu.SlugcatPage self)
    {
        var result = orig(self);

        if (self.slugcatNumber == Enums.Pearlcat)
            return true;

        return result;
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

            ModuleManager.MenuSceneData.Add(self, new(pearlColors, Pearls.RM_Pearlcat.GetDataPearlColor()));
        }
        else
        {
            var pearls = save.StoredPearlColors;

            if (pearls.Count > 11)
                pearls.RemoveRange(11, pearls.Count - 11);

            ModuleManager.MenuSceneData.Add(self, new(save.StoredPearlColors, save.ActivePearlColor));
        }
        
        foreach (var illustration in self.depthIllustrations)
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

    public static Color MenuPearlColorFilter(this Color color) => color;


    private static void MenuScene_Update(On.Menu.MenuScene.orig_Update orig, MenuScene self)
    {
        orig(self);

        foreach (var illustration in self.depthIllustrations)
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

            //if (Input.GetKeyDown("-"))
            //{
            //    if (illustration == self.depthIllustrations.First())
            //    {
            //        Plugin.Logger.LogWarning("------------------------------");
            //    }
                
            //    var fileName = Path.GetFileNameWithoutExtension(illustration.fileName);
            //    Plugin.Logger.LogWarning(fileName + " - " + illustration.pos);
            //}
        }
    }


    private static void UpdateSelectScreen(MenuScene self, MenuDepthIllustration illustration, MenuSceneModule menuSceneModule, MenuIllustrationModule illustrationModule)
    {
        var save = Utils.GetMiscProgression();
        var fileName = Path.GetFileNameWithoutExtension(illustration.fileName);

        if (illustrationModule.Index == -2)
        {
            var visible = true;

            if (fileName.Contains("Pearlpup"))
            {
                visible = save.HasPearlpup || (save.IsNewPearlcatSave && ModManager.MSC);
            }

            visible = visible && ((save.HasTrueEnding && fileName.Contains("trueend")) || (!save.HasTrueEnding && !fileName.Contains("trueend")));
            illustration.visible = visible;

            if (fileName.Contains("pupheart"))
            {
                UpdatePupHeartIllustration(self, illustration, illustrationModule);
            }

            return;
        }

        if (illustrationModule.Index == -1)
        {
            if (menuSceneModule.ActivePearlColor == null)
            {
                illustration.visible = false;
                return;
            }

            if (fileName == "pearlactivehalo")
            {
                illustration.sprite.SetAnchor(Vector2.one * 0.5f);
                illustration.sprite.scale = 0.3f;

                illustration.pos = menuSceneModule.ActivePearlPos;
                return;
            }


            var trueEndPos = new Vector2(690, 490);

            if (save.HasTrueEnding && illustrationModule.InitialPos != trueEndPos)
            {
                illustrationModule.InitialPos = trueEndPos;
                illustrationModule.SetPos = trueEndPos;
                illustrationModule.Vel = Vector2.zero;

                illustration.pos = trueEndPos;
            }


            var activePearlColor = menuSceneModule.ActivePearlColor;

            illustration.visible = true;
            illustration.color = (Color)activePearlColor;
            illustration.sprite.scale = 0.3f;


            var pos = illustration.pos;
            var spritePos = illustration.sprite.GetPosition();
            var mousePos = self.menu.mousePosition;

            var setPos = illustrationModule.SetPos;

            if (Custom.Dist(spritePos, mousePos) < 30.0f && Custom.Dist(pos, setPos) < 120.0f)
            {
                illustrationModule.Vel += (spritePos - mousePos).normalized * 2.0f;
            }

            var dir = (setPos - pos).normalized;
            var dist = Custom.Dist(setPos, pos);
            var speed = Custom.LerpMap(dist, 0.0f, 5.0f, 0.1f, 1.0f);

            illustrationModule.Vel *= Custom.LerpMap(illustrationModule.Vel.magnitude, 2.0f, 0.5f, 0.97f, 0.5f);
            illustrationModule.Vel += dir * speed;

            illustration.pos += illustrationModule.Vel;

            illustrationModule.SetPos = illustrationModule.InitialPos;
            illustrationModule.SetPos.y = illustrationModule.InitialPos.y + Mathf.Sin(MenuPearlAnimStacker / 500.0f) * 25.0f;

            menuSceneModule.ActivePearlPos = illustration.pos;

            return;
        }

        var pearlColors = menuSceneModule.PearlColors;

        var count = pearlColors.Count;
        var i = illustrationModule.Index;

        if (i >= count)
        {
            illustration.visible = false;
            return;
        }

        illustration.visible = true;

        var angleFrameAddition = 0.00675f;
        var radius = 150.0f;
        var origin = new Vector2(675, 400);

        if (save.HasTrueEnding)
        {
            radius = 130.0f;
            angleFrameAddition = 0.0045f;
            origin = new Vector2(675.0f, 350.0f);
        }

        var angle = (i * Mathf.PI * 2.0f / count) + angleFrameAddition * MenuPearlAnimStacker;

        var targetPos = new Vector2(origin.x + Mathf.Cos(angle) * radius, origin.y + Mathf.Sin(angle) * radius);
        illustration.pos = targetPos;

        illustration.sprite.scale = Custom.LerpMap(Mathf.Sin(angle), 0.0f, 1.0f, 0.3f, 0.25f);
        illustration.color = pearlColors[i].MenuPearlColorFilter();
    }

    private static void UpdateSleepScreen(MenuScene self, MenuDepthIllustration illustration, MenuSceneModule menuSceneModule, MenuIllustrationModule illustrationModule)
    {
        var save = Utils.GetMiscProgression();
        var fileName = Path.GetFileNameWithoutExtension(illustration.fileName);

        illustration.alpha = 1.0f;

        var pearlcatSad = (save.IsPearlpupSick || (!save.HasPearlpup && save.DidHavePearlpup)) && !save.HasTrueEnding;

        if (illustrationModule.Index == -2)
        {
            var visible = true;

            if (fileName == "pcat_nopup")
            {
                visible = !save.DidHavePearlpup;
            }
            else if (fileName == "pcat_withpup")
            {
                visible = !pearlcatSad && save.HasPearlpup;
            }
            else if (fileName == "pcat_sad")
            {
                visible = pearlcatSad;
            }
            else if (fileName == "pup")
            {
                visible = save.HasPearlpup && !save.IsPearlpupSick;
            }
            else if (fileName == "pup_sick")
            {
                visible = save.HasPearlpup && save.IsPearlpupSick;
            }
            else if (fileName == "scarf")
            {
                visible = pearlcatSad;
            }
            else if (fileName == "pup_drawings")
            {
                visible = save.HasPearlpup && !save.IsPearlpupSick;
            }
            else if (fileName == "sleep1")
            {
                if (pearlcatSad || save.HasPearlpup)
                {
                    illustration.pos = new(609, 27);
                }
            }

            var trueEndVisible = save.HasTrueEnding == fileName.Contains("trueend") || fileName.Contains("sleep1");

            illustration.visible = visible && trueEndVisible;

            if (fileName.Contains("pupheart"))
            {
                UpdatePupHeartIllustration(self, illustration, illustrationModule);
            }

            return;
        }

        if (illustrationModule.Index == -1)
        {

            if (fileName == "pearlactivehalo")
            {
                illustration.sprite.SetAnchor(Vector2.one * 0.5f);
                illustration.sprite.scale = 0.3f;
                illustration.visible = menuSceneModule.ActivePearlColor != null; 

                illustration.pos = menuSceneModule.ActivePearlPos;
                return;
            }

            var sadPos = new Vector2(870, 330);

            if (pearlcatSad && illustrationModule.InitialPos != sadPos)
            {
                illustrationModule.InitialPos = sadPos;
                illustrationModule.SetPos = sadPos;
                illustrationModule.Vel = Vector2.zero;

                illustration.pos = sadPos;
            }

            bool isPlaceholder = fileName == "pearlactiveplaceholder";
            var activePearlColor = Color.white;
            
            if (menuSceneModule.ActivePearlColor == null)
            {
                if (isPlaceholder && !save.HasTrueEnding)
                {
                    illustration.visible = true;
                }
                else
                {
                    illustration.visible = false;
                    return;
                }
            }
            else if (isPlaceholder)
            {
                illustration.visible = false;
                return;
            }
            else
            {
                activePearlColor = (Color)menuSceneModule.ActivePearlColor;
            }

            illustration.visible = true;
            illustration.color = MenuPearlColorFilter(activePearlColor);
            illustration.sprite.scale = isPlaceholder ? 1.0f : 0.3f;

            var pos = illustration.pos;
            var spritePos = illustration.sprite.GetPosition();
            var mousePos = self.menu.mousePosition;

            if (Custom.Dist(spritePos, mousePos) < 30.0f && Custom.Dist(pos, illustrationModule.SetPos) < 90.0f)
                illustrationModule.Vel += (spritePos - mousePos).normalized * 1.5f;


            var dir = (illustrationModule.SetPos - pos).normalized;
            var dist = Custom.Dist(illustrationModule.SetPos, pos);
            var speed = Custom.LerpMap(dist, 0.0f, 5.0f, 0.1f, 1.0f);

            illustrationModule.Vel *= Custom.LerpMap(illustrationModule.Vel.magnitude, 2.0f, 0.5f, 0.97f, 0.5f);
            illustrationModule.Vel += dir * speed;

            illustration.pos += illustrationModule.Vel;

            illustrationModule.SetPos.y = illustrationModule.InitialPos.y + Mathf.Sin(MenuPearlAnimStacker / 500.0f) * 25.0f;

            menuSceneModule.ActivePearlPos = illustration.pos;
            return;
        }

        var pearlColors = menuSceneModule.PearlColors;

        var count = pearlColors.Count;
        var i = illustrationModule.Index;

        if (i >= count)
        {
            illustration.visible = false;
            return;
        }

        illustration.visible = true;
        illustration.sprite.scale = 0.35f;
        illustration.color = pearlColors[i].MenuPearlColorFilter();

        illustration.pos.y = illustrationModule.InitialPos.y + Mathf.Sin((MenuPearlAnimStacker + i * 50.0f) / 50.0f) * 25.0f;
    }

    private static void UpdateSickScreen(MenuScene self, MenuDepthIllustration illustration, MenuSceneModule menuSceneModule, MenuIllustrationModule illustrationModule)
    {
        var fileName = Path.GetFileNameWithoutExtension(illustration.fileName);

        if (illustrationModule.Index == -2) return;

        if (illustrationModule.Index == -1)
        {
            if (menuSceneModule.ActivePearlColor == null)
            {
                illustration.visible = false;
                return;
            }

            if (fileName == "pearlactivehalo")
            {
                illustration.sprite.SetAnchor(Vector2.one * 0.5f);
                illustration.sprite.scale = 0.3f;

                illustration.pos = menuSceneModule.ActivePearlPos;
                return;
            }

            var activePearlColor = menuSceneModule.ActivePearlColor;

            illustration.visible = true;
            illustration.color = (Color)activePearlColor;
            illustration.sprite.scale = 0.3f;


            var pos = illustration.pos;
            var spritePos = illustration.sprite.GetPosition();
            var mousePos = self.menu.mousePosition;

            var setPos = illustrationModule.SetPos;

            if (Custom.Dist(spritePos, mousePos) < 30.0f && Custom.Dist(pos, setPos) < 120.0f)
                illustrationModule.Vel += (spritePos - mousePos).normalized * 2.0f;


            var dir = (setPos - pos).normalized;
            var dist = Custom.Dist(setPos, pos);
            var speed = Custom.LerpMap(dist, 0.0f, 5.0f, 0.1f, 1.0f);

            illustrationModule.Vel *= Custom.LerpMap(illustrationModule.Vel.magnitude, 2.0f, 0.5f, 0.97f, 0.5f);
            illustrationModule.Vel += dir * speed;

            illustration.pos += illustrationModule.Vel;

            illustrationModule.SetPos.y = illustrationModule.InitialPos.y + Mathf.Sin(MenuPearlAnimStacker / 500.0f) * 25.0f;
            menuSceneModule.ActivePearlPos = illustration.pos;
            return;
        }

        var pearlColors = menuSceneModule.PearlColors;

        var count = pearlColors.Count;
        var i = illustrationModule.Index;

        if (i >= count)
        {
            illustration.visible = false;
            return;
        }

        illustration.visible = true;

        var angleFrameAddition = 0.0045f;
        var radius = 90.0f;
        var origin = new Vector2(650, 490);

        var angle = (i * Mathf.PI * 2.0f / count) + angleFrameAddition * MenuPearlAnimStacker;

        var targetPos = new Vector2(origin.x + Mathf.Cos(angle) * radius * 1.7f, origin.y + Mathf.Sin(angle) * radius);
        illustration.pos = targetPos;

        illustration.sprite.scale = Custom.LerpMap(Mathf.Sin(angle), 1.0f, 0.0f, 0.2f, 0.3f);
        illustration.alpha = 1.0f;
        illustration.color = pearlColors[i].MenuPearlColorFilter();
    }

    private static void UpdateAscendedScreen(MenuScene self, MenuDepthIllustration illustration, MenuSceneModule menuSceneModule, MenuIllustrationModule illustrationModule)
    {
        if (illustrationModule.Index == -2)
        {
            var save = Utils.GetMiscProgression();
            var fileName = Path.GetFileNameWithoutExtension(illustration.fileName);

            var visible = true;

            if (fileName == "pup")
            {
                visible = save.AscendedWithPup;
            }

            visible = visible && ((save.HasTrueEnding && fileName.Contains("trueend")) || (!save.HasTrueEnding && !fileName.Contains("trueend")));
            illustration.visible = visible;
            return;
        }

        if (illustrationModule.Index == -1)
        {
            if (menuSceneModule.ActivePearlColor == null)
            {
                illustration.visible = false;
                return;
            }

            var fileName = Path.GetFileNameWithoutExtension(illustration.fileName);

            if (fileName == "pearlactivehalo")
            {
                illustration.sprite.SetAnchor(Vector2.one * 0.5f);
                illustration.sprite.scale = 0.3f;

                illustration.pos = menuSceneModule.ActivePearlPos;
                return;
            }

            var activePearlColor = menuSceneModule.ActivePearlColor;

            illustration.visible = true;
            illustration.color = ((Color)activePearlColor).AscendedPearlColorFilter();
            illustration.sprite.scale = 0.25f;
            illustration.alpha = 1.0f;

            var pos = illustration.pos;
            var spritePos = illustration.sprite.GetPosition();
            var mousePos = self.menu.mousePosition;

            if (Custom.Dist(spritePos, mousePos) < 30.0f && Custom.Dist(pos, illustrationModule.SetPos) < 120.0f)
                illustrationModule.Vel += (spritePos - mousePos).normalized * 2.0f;


            var dir = (illustrationModule.SetPos - pos).normalized;
            var dist = Custom.Dist(illustrationModule.SetPos, pos);
            var speed = Custom.LerpMap(dist, 0.0f, 5.0f, 0.1f, 1.0f);

            illustrationModule.Vel *= Custom.LerpMap(illustrationModule.Vel.magnitude, 2.0f, 0.5f, 0.97f, 0.5f);
            illustrationModule.Vel += dir * speed;

            illustration.pos += illustrationModule.Vel;

            illustrationModule.SetPos.y = illustrationModule.InitialPos.y + Mathf.Sin(MenuPearlAnimStacker / 500.0f) * 25.0f;
            menuSceneModule.ActivePearlPos = illustration.pos;
            return;
        }

        var pearlColors = menuSceneModule.PearlColors;

        var count = pearlColors.Count;
        var i = illustrationModule.Index;

        if (i >= count)
        {
            illustration.visible = false;
            return;
        }

        illustration.visible = true;

        var angleFrameAddition = 0.0045f;
        var radius = 90.0f;
        var origin = new Vector2(680, 360);

        var angle = (i * Mathf.PI * 2.0f / count) + angleFrameAddition * MenuPearlAnimStacker;

        var targetPos = new Vector2(origin.x + Mathf.Cos(angle) * radius * 2.0f, origin.y + Mathf.Sin(angle) * radius);
        illustration.pos = targetPos;

        illustration.sprite.scale = Custom.LerpMap(Mathf.Cos(angle) + Mathf.Sin(angle), 2.0f, 0.0f, 0.2f, 0.3f);
        illustration.alpha = 1.0f;
        illustration.color = pearlColors[i].AscendedPearlColorFilter();
        //illustration.color = Color.Lerp(pearlColors[i].MenuPearlColorFilter(), new Color32(207, 187, 101, 255), 0.4f);
    }


    public static Color AscendedPearlColorFilter(this Color color)
    {
        //return color;

        Color.RGBToHSV(Color.Lerp(color, Color.white, 0.3f), out var hue, out var sat, out var val);
        return Color.HSVToRGB(hue, sat, val);
    }
    
    private static void UpdatePupHeartIllustration(MenuScene self, MenuDepthIllustration illustration, MenuIllustrationModule illustrationModule)
    {
        var miscProg = Utils.GetMiscProgression();

        if (!miscProg.HasTrueEnding)
        {
            illustration.visible = false;
            return;
        }

        var fileName = Path.GetFileNameWithoutExtension(illustration.fileName);
        illustration.visible = true;

        var initialScale = 0.3f;
        var isCore = fileName == "pupheartcore";

        if (illustration.sprite.scale == 1.0f)
        {
            illustration.sprite.scale = initialScale;
            illustration.sprite.SetAnchor(new Vector2(0.5f, 0.5f));
        }

        var currentScale = illustration.sprite.scale;
        
        const int beatFrequency = 80;
        const int coreBeatOffset = 10;

        var beat = MenuPearlAnimStacker % beatFrequency == 0;
        var coreBeat = (MenuPearlAnimStacker - coreBeatOffset) % beatFrequency == 0;

        if (isCore)
        {
            if (coreBeat)
            {
                illustration.sprite.scale = 0.4f;
            }
            else
            {
                illustration.sprite.scale = Mathf.Lerp(currentScale, initialScale, 0.1f);
            }
        }
        else
        {
            if (beat)
            {
                illustration.sprite.scale = 0.45f;
                self.menu.PlaySound(Sounds.Pearlcat_Heartbeat, 0.0f, 0.3f, 1.0f);
            }
            else if (coreBeat)
            {
                illustration.sprite.scale = 0.4f;
            }
            else
            {
                illustration.sprite.scale = Mathf.Lerp(currentScale, initialScale, 0.1f);
            }
        }
    }



    public static string SecretPassword { get; set; } = "mira";
    public static int SecretIndex { get; set; } = 0;

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
        var miraSkipAvailable = !disableSave && Utils.IsMiraActive && isPearlcatPage && !self.restartChecked && !miscProg.HasTrueEnding && !miscProg.UnlockedMira && ModManager.MSC;

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
            var text = self.Translate("CANNOT PLAY") + "\n" + (miscProg.IsMSCSave ? "MSC" : "NON-MSC") + self.Translate(" SAVE");

            self.startButton.menuLabel.text = text;
        }


        var canSecretOccur = page is SlugcatSelectMenu.SlugcatPageNewGame && miscProg.IsSecretEnabled == miscProg.HasTrueEnding;

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

            if (Utils.MiraVersionWarning)
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
            if (Utils.MiraVersionWarning)
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



    public const string MIRA_SKIP_ID = "PEARLCAT_MIRA_SKIP";
    public static Color MiraMenuColor { get; } = Custom.hexToColor("9487c9");

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


    // Skip intro cutscene if secret (nvm I want people to see geahgeahg's work, they can already skip if they want lol)
    private static void SlugcatSelectMenu_StartGame(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(MoveType.After,
            x => x.MatchCallOrCallvirt<SlugcatSelectMenu>(nameof(SlugcatSelectMenu.ContinueStartedGame))
        );

        var dest = il.DefineLabel();

        c.GotoNext(MoveType.After,
            x => x.MatchLdstr("s"),
            x => x.MatchCallOrCallvirt<Input>(nameof(Input.GetKey)),
            x => x.MatchBrtrue(out dest)
        );

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Func<SlugcatSelectMenu, bool>>((self) =>
        {
            var save = Utils.GetMiscProgression();

            return save.IsSecretEnabled;
        });

        c.Emit(OpCodes.Brtrue, dest);
    }
}
