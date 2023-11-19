using Menu;
using MonoMod.RuntimeDetour;
using RWCustom;
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
    }



    private static void StoryGameStatisticsScreen_AddBkgIllustration(On.Menu.StoryGameStatisticsScreen.orig_AddBkgIllustration orig, StoryGameStatisticsScreen self)
    {

        if (RainWorld.lastActiveSaveSlot == Enums.Pearlcat)
        {
            var save = self.manager.rainWorld.GetMiscProgression();

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

        var save = self.menu.manager.rainWorld.GetMiscProgression();
        var fileName = Path.GetFileNameWithoutExtension(self.fileName);

        if (fileName == "Intro3_3" || fileName == "Intro5_3")
        {
            self.visible = ModManager.MSC;
        }
        else if (fileName == "Outro3_1" || fileName == "Outro2_1")
        {
            self.visible = save.HasPearlpup;
        }
        else if (fileName == "AltOutro10_1")
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
       
        var save = menu.manager.rainWorld.GetMiscProgression();

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

        MenuPearlAnimStacker = 0;
        
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

    public static int MenuPearlAnimStacker { get; set; } = 0;

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

            //if (Input.GetKey("-"))
            //{
            //    var fileName = Path.GetFileNameWithoutExtension(illustration.fileName);

            //    if (illustration.depth > 4.8f)
            //        Plugin.Logger.LogWarning("-----------------------------------------");

            //    Plugin.Logger.LogWarning(fileName + " - " + illustration.pos);
            //}
        }

        if (self.menu is not SlugcatSelectMenu)
        {
            MenuPearlAnimStacker += 1; // hacky
        }
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

        var angleFrameAddition = 0.0005f;
        var radius = 90.0f;
        var origin = new Vector2(670, 470);

        var angle = (i * Mathf.PI * 2.0f / count) + angleFrameAddition * MenuPearlAnimStacker;

        var targetPos = new Vector2(origin.x + Mathf.Cos(angle) * radius * 1.7f, origin.y + Mathf.Sin(angle) * radius);
        illustration.pos = targetPos;

        illustration.sprite.scale = Custom.LerpMap(Mathf.Sin(angle), 1.0f, 0.0f, 0.2f, 0.3f);
        illustration.alpha = 1.0f;
        illustration.color = pearlColors[i].MenuPearlColorFilter();
    }

    private static void UpdateSleepScreen(MenuScene self, MenuDepthIllustration illustration, MenuSceneModule menuSceneModule, MenuIllustrationModule illustrationModule)
    {
        var save = self.menu.manager.rainWorld.GetMiscProgression();
        var fileName = Path.GetFileNameWithoutExtension(illustration.fileName);

        illustration.alpha = 1.0f;

        if (illustrationModule.Index == -2)
        {
            if (save.HasTrueEnding)
            {
                if (!fileName.Contains("pupadult"))
                {
                    illustration.visible = false;
                }

                return;
            }

            if (fileName == "sweat")
            {
                illustration.visible = menuSceneModule.ActivePearlColor == null;
            }
            else if (fileName == "slugcat")
            {
                illustration.visible = !save.IsPearlpupSick;
            }
            else if (fileName == "slugcatsick")
            {
                illustration.visible = save.IsPearlpupSick;
            }
            else if (fileName == "pup")
            {
                illustration.visible = save.HasPearlpup && !save.IsPearlpupSick;
            }
            else if (fileName == "pupsick")
            {
                illustration.visible = save.HasPearlpup && save.IsPearlpupSick;
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

            bool isPlaceholder = fileName == "pearlactiveplaceholder";
            var activePearlColor = Color.white;
            
            if (menuSceneModule.ActivePearlColor == null)
            {
                if (isPlaceholder)
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

    private static void UpdateSelectScreen(MenuScene self, MenuDepthIllustration illustration, MenuSceneModule menuSceneModule, MenuIllustrationModule illustrationModule)
    {
        var save = self.menu.manager.rainWorld.GetMiscProgression();
        var fileName = Path.GetFileNameWithoutExtension(illustration.fileName);

        if (illustrationModule.Index == -2)
        {
            if (save.HasTrueEnding)
            {
                if (!fileName.Contains("pupadult"))
                {
                    illustration.visible = false;
                }

                return;
            }

            if (fileName == "pup")
            {
                illustration.visible = save.HasPearlpup;
            }

            if (save.HasPearlpup)
            {
                if (fileName == "body" || fileName == "tail" || fileName == "legs")
                {
                    illustration.pos.x = illustrationModule.InitialPos.x - 20.0f;
                }
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

            var activePearlColor = menuSceneModule.ActivePearlColor;

            illustration.visible = true;
            illustration.color = (Color)activePearlColor;
            illustration.sprite.scale = 0.3f;


            var pos = illustration.pos;
            var spritePos = illustration.sprite.GetPosition();
            var mousePos = self.menu.mousePosition;
            // var mouseVel = (self.menu.mousePosition - self.menu.lastMousePos).magnitude;
            // Custom.LerpMap(mouseVel, 0.0f, 100.0f, 1.0f, 6.0f);

            var setPos = illustrationModule.SetPos - Vector2.right * (save.HasPearlpup ? 30.0f : 0.0f);

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

            if (save.HasTrueEnding)
            {
                illustrationModule.SetPos += new Vector2(100.0f, 170.0f);
            }

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

        var angleFrameAddition = 0.00075f;
        var radius = 120.0f;
        var origin = new Vector2(680, 400);

        if (save.HasPearlpup)
        {
            origin.x -= 20.0f;
        }

        if (save.HasTrueEnding)
        {
            radius = 130.0f;
            angleFrameAddition = 0.0005f;
        }

        var angle = (i * Mathf.PI * 2.0f / count) + angleFrameAddition * MenuPearlAnimStacker;

        Vector2 targetPos = new(origin.x + Mathf.Cos(angle) * radius, origin.y + Mathf.Sin(angle) * radius * 1.25f);
        illustration.pos = targetPos;

        illustration.sprite.scale = Custom.LerpMap(Mathf.Cos(angle), 0.0f, 1.0f, 0.2f, 0.35f);
        illustration.color = pearlColors[i].MenuPearlColorFilter();
    }

    private static void UpdateAscendedScreen(MenuScene self, MenuDepthIllustration illustration, MenuSceneModule menuSceneModule, MenuIllustrationModule illustrationModule)
    {
        if (illustrationModule.Index == -2) return;

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
            illustration.color = (Color)activePearlColor * Custom.HSL2RGB(1.0f, 0.2f, 1.0f);
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

        var angleFrameAddition = 0.0005f;
        var radius = 90.0f;
        var origin = new Vector2(650, 360);

        var angle = (i * Mathf.PI * 2.0f / count) + angleFrameAddition * MenuPearlAnimStacker;

        var targetPos = new Vector2(origin.x + Mathf.Cos(angle) * radius * 1.7f, origin.y + Mathf.Sin(angle) * radius);
        illustration.pos = targetPos;

        illustration.sprite.scale = Custom.LerpMap(Mathf.Sin(angle), 1.0f, 0.0f, 0.2f, 0.3f);
        illustration.alpha = 1.0f;
        illustration.color = pearlColors[i].MenuPearlColorFilter();
        //illustration.color = Color.Lerp(pearlColors[i].MenuPearlColorFilter(), new Color32(207, 187, 101, 255), 0.4f);
    }


    private static void SlugcatPage_ctor(On.Menu.SlugcatSelectMenu.SlugcatPage.orig_ctor orig, SlugcatSelectMenu.SlugcatPage self, Menu.Menu menu, MenuObject owner, int pageIndex, SlugcatStats.Name slugcatNumber)
    {
        orig(self, menu, owner, pageIndex, slugcatNumber);

        if (slugcatNumber != Enums.Pearlcat) return;

        var save = menu.manager.rainWorld.GetMiscProgression();
        var color = ModOptions.InventoryOverride.Value ? ModOptions.GetOverridenInventory(true).FirstOrDefault().GetDataPearlColor() : save.IsNewPearlcatSave ? Pearls.RM_Pearlcat.GetDataPearlColor() : save.ActivePearlColor;

        // screw pebbles pearls you get ORANGE    
        self.effectColor = color ?? Color.white;

        self.markOffset = save.HasPearlpup ? new(0.0f, 50.0f) : new(20.0f, 50.0f);
    }

    private static void SlugcatSelectMenu_Update(On.Menu.SlugcatSelectMenu.orig_Update orig, SlugcatSelectMenu self)
    {
        orig(self);

        MenuPearlAnimStacker += 9; // only reason this is not 1 is cause i was stupid earlier in development and it would be a PITA to fix it otherwise lol

        var page = self.slugcatPages[self.slugcatPageIndex];

        if (page.slugcatNumber == Enums.Pearlcat)
        {
            var save = self.manager.rainWorld.progression.miscProgressionData.GetMiscProgression();
            var disableSave = !save.IsNewPearlcatSave && save.IsMSCSave != ModManager.MSC && !self.restartChecked;

            if (disableSave)
            {
                self.startButton.buttonBehav.greyedOut = true;
                var text = "CANNOT PLAY" + "\n" + (save.IsMSCSave ? "MSC" : "NON-MSC") + " SAVE";

                self.startButton.menuLabel.text = text;
            }
        }

        
        //Music.MusicPlayer musicPlayer = self.manager.musicPlayer;

        //if (self.slugcatPages[self.slugcatPageIndex].slugcatNumber.ToString() == Plugin.SLUGCAT_ID)
        //{
        //    musicPlayer.RequestHalcyonSong("NA_19 - Halcyon Memories");

        //    MoreSlugcats.HalcyonSong? halcyonSong = null;
        //    if (musicPlayer.song is MoreSlugcats.HalcyonSong song && musicPlayer.song != null) halcyonSong = song;
        //    if (musicPlayer.nextSong is MoreSlugcats.HalcyonSong nextSong && musicPlayer.nextSong != null) halcyonSong = nextSong;

        //    if (halcyonSong != null)
        //    {
        //        halcyonSong.volume = 0.5f;
        //        halcyonSong.baseVolume = 0.5f;
        //        halcyonSong.setVolume = 0.5f;
        //        halcyonSong.droneVolume = 0.5f;
        //    }
        //}
        //else
        //{
        //    musicPlayer.RequestIntroRollMusic();

        //    Music.IntroRollMusic? introRollSong = null;
        //    if (musicPlayer.song is Music.IntroRollMusic song && musicPlayer.song != null) introRollSong = song;
        //    if (musicPlayer.nextSong is Music.IntroRollMusic nextSong && musicPlayer.nextSong != null) introRollSong = nextSong;

        //    if (introRollSong != null)
        //    {
        //        introRollSong.StartPlaying();
        //        introRollSong.StartMusic();
        //    }
        //}
    }
}
