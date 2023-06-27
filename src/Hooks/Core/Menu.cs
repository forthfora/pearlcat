using Menu;
using RWCustom;
using SlugBase.DataTypes;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using static DataPearl.AbstractDataPearl;

namespace Pearlcat;

public static partial class Hooks
{
    public static void ApplyMenuHooks()
    {
        On.Menu.SlugcatSelectMenu.SlugcatPage.ctor += SlugcatPage_ctor;

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



    public static ConditionalWeakTable<MenuDepthIllustration, MenuPearlModule> MenuIllustrationData = new();

    private static void SlugcatPage_ctor(On.Menu.SlugcatSelectMenu.SlugcatPage.orig_ctor orig, SlugcatSelectMenu.SlugcatPage self, Menu.Menu menu, MenuObject owner, int pageIndex, SlugcatStats.Name slugcatNumber)
    {
        orig(self, menu, owner, pageIndex, slugcatNumber);

        if (slugcatNumber != Enums.General.Pearlcat) return;

        self.effectColor = ItemSymbol.ColorForItem(AbstractPhysicalObject.AbstractObjectType.DataPearl, DataPearlType.HI.index);
    }

    public static void MenuScene_Update(On.Menu.MenuScene.orig_Update orig, MenuScene self)
    {
        orig(self);

        // HACK: remove this once migrated to the new save system
        List<Color> pearlColors = new();
        Color? activePearlColor = ItemSymbol.ColorForItem(AbstractPhysicalObject.AbstractObjectType.DataPearl, DataPearlType.HI.index);

        for (int i = 0; i < 10; i++)
        {
            var type = i switch
            {
                0 => DataPearlType.SL_moon,
                1 => DataPearlType.SL_bridge,
                2 => DataPearlType.LF_bottom,
                3 => DataPearlType.CC,
                4 => DataPearlType.GW,
                5 => DataPearlType.DS,
                6 => DataPearlType.SH,
                7 => DataPearlType.Misc,
                8 => DataPearlType.UW,
                _ => DataPearlType.SB_filtration,
            };

            var color = ItemSymbol.ColorForItem(AbstractPhysicalObject.AbstractObjectType.DataPearl, type.index);
            pearlColors.Add(color);
        }


        foreach (var illustration in self.depthIllustrations)
        {
            if (!MenuIllustrationData.TryGetValue(illustration, out var pearlModule)) continue;

            if (pearlModule.index == -1)
            {
                if (activePearlColor == null)
                {
                    illustration.visible = false;
                }
                else
                {
                    illustration.visible = true;
                    illustration.color = MenuPearlColorFilter((Color)activePearlColor);
                    illustration.sprite.scale = 0.3f;
                }

                var pos = illustration.pos;
                var spritePos = illustration.sprite.GetPosition();
                var mousePos = self.menu.mousePosition;
                var mouseVel = (self.menu.mousePosition - self.menu.lastMousePos).magnitude;
                // Custom.LerpMap(mouseVel, 0.0f, 100.0f, 1.0f, 6.0f);

                if (Custom.Dist(spritePos, mousePos) < 30.0f && Custom.Dist(pos, pearlModule.initialPos) < 120.0f)
                    pearlModule.vel += (spritePos - mousePos).normalized * 2.0f;


                var dir = (pearlModule.initialPos - pos).normalized;
                var dist = Custom.Dist(pearlModule.initialPos, pos);
                var speed = Custom.LerpMap(dist, 0.0f, 5.0f, 0.1f, 1.0f);

                pearlModule.vel *= Custom.LerpMap(pearlModule.vel.magnitude, 2.0f, 0.5f, 0.97f, 0.5f);
                pearlModule.vel += dir * speed;

                illustration.pos += pearlModule.vel;
                continue;
            }

            var count = pearlColors.Count;
            var i = pearlModule.index;

            if (i >= pearlColors.Count)
            {
                illustration.visible = false;
                continue;
            }

            illustration.visible = true;

            var angleFrameAddition = 0.00075f;
            var radius = 120.0f;
            var origin = new Vector2(680, 400);

            var angle = (i * Mathf.PI * 2.0f / count) + angleFrameAddition * MenuPearlAnimStacker;

            Vector2 targetPos = new(origin.x + Mathf.Cos(angle) * radius, origin.y + Mathf.Sin(angle) * radius * 1.25f);
            illustration.pos = targetPos;

            illustration.sprite.scale = Custom.LerpMap(Mathf.Cos(angle), 0.0f, 1.0f, 0.2f, 0.35f);
            illustration.color = MenuPearlColorFilter(pearlColors[i]);
        }

        MenuPearlAnimStacker++;
    }

    public static Color MenuPearlColorFilter(Color color) => color; 

    public static int MenuPearlAnimStacker = 0;

    public static void MenuScene_ctor(On.Menu.MenuScene.orig_ctor orig, MenuScene self, Menu.Menu menu, MenuObject owner, MenuScene.SceneID sceneID)
    {
        orig(self, menu, owner, sceneID);

        if (sceneID.value != "Slugcat_Pearlcat") return;

        MenuPearlAnimStacker = 0;
        
        foreach (var illustration in self.depthIllustrations)
        {
            var fileName = Path.GetFileNameWithoutExtension(illustration.fileName);

            if (fileName.Contains("pearl"))
            {
                var indexString = fileName.Replace("pearl", "");

                if (!int.TryParse(indexString, out var index))
                {
                    if (fileName == "pearlactive")
                        index = -1;

                    else
                        continue;
                }

                MenuIllustrationData.Add(illustration, new MenuPearlModule(illustration, index));
            }
        }
    }

    public static void SlugcatSelectMenu_Update(On.Menu.SlugcatSelectMenu.orig_Update orig, SlugcatSelectMenu self)
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
