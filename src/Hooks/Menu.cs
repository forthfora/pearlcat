using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;

namespace Pearlcat;

public static partial class Hooks
{
    public static void ApplyMenuHooks()
    {
        On.Menu.MenuScene.ctor += MenuScene_ctor;
        On.Menu.MenuScene.Update += MenuScene_Update;

        On.Menu.SlugcatSelectMenu.Update += SlugcatSelectMenu_Update;
    }

    private static ConditionalWeakTable<Menu.MenuDepthIllustration, MenuIllustrationEx> MenuIllustrationData = new();

    private static void MenuScene_Update(On.Menu.MenuScene.orig_Update orig, Menu.MenuScene self)
    {
        orig(self);

        foreach(var illustration in self.depthIllustrations)
        {
            MenuIllustrationData.TryGetValue(illustration, out var menuIllustrationEx);
            menuIllustrationEx?.Update();
        }
    }

    private static void MenuScene_ctor(On.Menu.MenuScene.orig_ctor orig, Menu.MenuScene self, Menu.Menu menu, Menu.MenuObject owner, Menu.MenuScene.SceneID sceneID)
    {
        orig(self, menu, owner, sceneID);

        if (sceneID.value != "Slugcat_Pearlcat") return;

        foreach (var illustration in self.depthIllustrations)
        {
            if (illustration.fileName.EndsWith("_glow")) continue;

            if (MenuIllustrationData.TryGetValue(illustration, out _)) continue;

            MenuIllustrationData.Add(illustration, new MenuIllustrationEx(self, illustration));
        }
    }

    private class MenuIllustrationEx
    {
        public bool isMenuIllustrationInit = false;
        public readonly string name;

        public readonly WeakReference<Menu.MenuDepthIllustration> illustrationRef;
        public readonly WeakReference<Menu.MenuScene> menuScene;

        public readonly WeakReference<Menu.MenuDepthIllustration>? glowRef = null!;

        public MenuIllustrationEx(Menu.MenuScene menuScene, Menu.MenuDepthIllustration illustration)
        {
            name = illustration.fileName;
            this.illustrationRef = new WeakReference<Menu.MenuDepthIllustration>(illustration);
            this.menuScene = new WeakReference<Menu.MenuScene>(menuScene);

            glowRef = new WeakReference<Menu.MenuDepthIllustration>(menuScene.depthIllustrations.FirstOrDefault(illustration => illustration.fileName == name + "_glow"));
        }

        public Vector2 pos = Vector2.zero;
        public Color color = Color.white;

        public int animationStacker = 0;

        public AnimationCurve curve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
        private const int framesToCycle = 150;

        public Vector2 maxPos;
        public Vector2 minPos;
        public int dir = 1;

        public void Update()
        {
            illustrationRef.TryGetTarget(out var illustration);
            if (illustration == null) return;

            Menu.MenuDepthIllustration glow = null!;
            glowRef?.TryGetTarget(out glow);

            if (name.Contains("pearl"))
                UpdatePearl(illustration, glow);

            isMenuIllustrationInit = true;
        }

        private void UpdatePearl(Menu.MenuDepthIllustration illustration, Menu.MenuDepthIllustration glow)
        {
            if (glow == null) return;

            if (!isMenuIllustrationInit)
            {
                illustration.color = Random.ColorHSV(0.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f);
                glow.color = name.StartsWith("activepearl") ? Color.red : Color.blue;
                

                maxPos = illustration.pos + new Vector2(0.0f, 35.0f * Mathf.InverseLerp(6.0f, 1.0f, illustration.depth));
                minPos = illustration.pos + new Vector2(0.0f, -35.0f * Mathf.InverseLerp(6.0f, 1.0f, illustration.depth));

                if (char.IsDigit(name.Last()))
                {
                    animationStacker += name.Last() * 40;
                    animationStacker %= framesToCycle;
                }
            }

            UpdateLinearMovement(illustration, glow);
        }

        private void UpdateLinearMovement(Menu.MenuDepthIllustration illustration, Menu.MenuDepthIllustration glow)
        {
            Vector2 targetPos = Vector2.Lerp(minPos, maxPos, curve.Evaluate((float)animationStacker / framesToCycle));

            illustration.pos = targetPos;
            glow.pos = targetPos;

            if (animationStacker + dir > framesToCycle || animationStacker + dir < 0) dir *= -1;
            animationStacker += dir;
        }
    }

    private static void SlugcatSelectMenu_Update(On.Menu.SlugcatSelectMenu.orig_Update orig, Menu.SlugcatSelectMenu self)
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
