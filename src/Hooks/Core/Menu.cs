using System.Runtime.CompilerServices;

namespace Pearlcat;

public static partial class Hooks
{
    public static void ApplyMenuHooks()
    {
        On.Menu.MenuScene.ctor += MenuScene_ctor;
        On.Menu.MenuScene.Update += MenuScene_Update;

        On.Menu.SlugcatSelectMenu.Update += SlugcatSelectMenu_Update;

        On.HUD.HUD.InitSinglePlayerHud += HUD_InitSinglePlayerHud;
    }



    private static void HUD_InitSinglePlayerHud(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam)
    {
        orig(self, cam);

        self.AddPart(new InventoryHUD(self, self.fContainers[1]));
    }


    public static ConditionalWeakTable<Menu.MenuDepthIllustration, MenuIllustrationModule> MenuIllustrationData = new();

    public static void MenuScene_Update(On.Menu.MenuScene.orig_Update orig, Menu.MenuScene self)
    {
        orig(self);

        foreach(var illustration in self.depthIllustrations)
        {
            MenuIllustrationData.TryGetValue(illustration, out var menuIllustrationModule);
            menuIllustrationModule?.Update();
        }
    }

    public static void MenuScene_ctor(On.Menu.MenuScene.orig_ctor orig, Menu.MenuScene self, Menu.Menu menu, Menu.MenuObject owner, Menu.MenuScene.SceneID sceneID)
    {
        orig(self, menu, owner, sceneID);

        if (sceneID.value != "Slugcat_Pearlcat") return;

        foreach (var illustration in self.depthIllustrations)
        {
            if (illustration.fileName.EndsWith("_glow")) continue;

            if (MenuIllustrationData.TryGetValue(illustration, out _)) continue;

            MenuIllustrationData.Add(illustration, new MenuIllustrationModule(self, illustration));
        }
    }

    public static void SlugcatSelectMenu_Update(On.Menu.SlugcatSelectMenu.orig_Update orig, Menu.SlugcatSelectMenu self)
    {
        orig(self);

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
