using BepInEx.Logging;
using MonoMod.Cil;
using On.Music;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using UnityEngine;
using static SlugcatStats;

namespace TheSacrifice
{
    internal static partial class Hooks
    {
        public static void ApplyHooks()
        {
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            On.RainWorld.OnModsDisabled += RainWorld_OnModsDisabled;

            On.Menu.MenuScene.ctor += MenuScene_ctor;
            On.Menu.SlugcatSelectMenu.Update += SlugcatSelectMenu_Update; ;

            ApplyPlayerHooks();
        }

        private static void SlugcatSelectMenu_Update(On.Menu.SlugcatSelectMenu.orig_Update orig, Menu.SlugcatSelectMenu self)
        {
            orig(self);

            return;

            Music.MusicPlayer musicPlayer = self.manager.musicPlayer;

            if (self.slugcatPages[self.slugcatPageIndex].slugcatNumber.ToString() == Plugin.SLUGCAT_ID)
            {
                musicPlayer.RequestHalcyonSong("NA_19 - Halcyon Memories");

                MoreSlugcats.HalcyonSong? halcyonSong = null;
                if (musicPlayer.song is MoreSlugcats.HalcyonSong song && musicPlayer.song != null) halcyonSong = song;
                if (musicPlayer.nextSong is MoreSlugcats.HalcyonSong nextSong && musicPlayer.nextSong != null) halcyonSong = nextSong;

                if (halcyonSong != null)
                {
                    halcyonSong.volume = 0.5f;
                    halcyonSong.baseVolume = 0.5f;
                    halcyonSong.setVolume = 0.5f;
                    halcyonSong.droneVolume = 0.5f;
                }
            }
            else
            {
                musicPlayer.RequestIntroRollMusic();

                Music.IntroRollMusic? introRollSong = null;
                if (musicPlayer.song is Music.IntroRollMusic song && musicPlayer.song != null) introRollSong = song;
                if (musicPlayer.nextSong is Music.IntroRollMusic nextSong && musicPlayer.nextSong != null) introRollSong = nextSong;

                if (introRollSong != null)
                {
                    introRollSong.fadeOutRain = true;
                    introRollSong.StartPlaying();
                    introRollSong.StartMusic();
                }
            }
        }

        private static void MenuScene_ctor(On.Menu.MenuScene.orig_ctor orig, Menu.MenuScene self, Menu.Menu menu, Menu.MenuObject owner, Menu.MenuScene.SceneID sceneID)
        {
            orig(self, menu, owner, sceneID);

            Color color = UnityEngine.Random.ColorHSV(0.0f, 1.0f, 0.8f, 0.8f, 2.5f, 2.5f); ;

            Menu.MenuDepthIllustration? activePearl = self.depthIllustrations.Where(scene => scene.fileName == "activepearl").FirstOrDefault();
            if (activePearl != null) activePearl.color = color;

            Menu.MenuDepthIllustration? activePearlGlow = self.depthIllustrations.Where(scene => scene.fileName == "activepearl_glow").FirstOrDefault();
            if (activePearlGlow != null) activePearlGlow.color = color;
        }

        private static bool isInit = false;

        private static void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);

            if (isInit) return;
            isInit = true;

            MachineConnector.SetRegisteredOI(Plugin.MOD_ID, Options.instance);

            Enums.RegisterEnums();
            AssetLoader.LoadAssets();
        }

        private static void RainWorld_OnModsDisabled(On.RainWorld.orig_OnModsDisabled orig, RainWorld self, ModManager.Mod[] newlyDisabledMods)
        {
            orig(self, newlyDisabledMods);

            Enums.UnregisterEnums();
        }
    }
}
