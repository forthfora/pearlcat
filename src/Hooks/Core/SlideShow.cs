using Menu;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using static Menu.MenuScene;

namespace Pearlcat;

public static partial class Hooks
{
    public static void ApplySlideShowHooks()
    {
        IL.RainWorldGame.ExitToVoidSeaSlideShow += RainWorldGame_ExitToVoidSeaSlideShow;
        IL.Menu.SlugcatSelectMenu.StartGame += SlugcatSelectMenu_StartGame;

        IL.Menu.SlideShow.ctor += SlideShow_ctor;
    }

    private static void SlideShow_ctor(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(MoveType.After,
            x => x.MatchStfld<SlideShow>(nameof(SlideShow.playList)));

        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldarg_2);
        c.EmitDelegate<Action<SlideShow, SlideShow.SlideShowID>>((self, id) =>
        {
            if (id == Enums.SlideShows.PearlcatIntro)
            {            
                if (self.manager.musicPlayer != null)
                {
                    self.waitForMusic = "BM_SS_DOOR";
                    self.stall = true;
                    self.manager.musicPlayer.MenuRequestsSong(self.waitForMusic, 1.5f, 10f);
                }

                self.playList.Add(new SlideShow.Scene(Enums.SlideShows.Pearlcat_Outro_1, self.ConvertTime(0, 2, 0), self.ConvertTime(0, 4, 0), self.ConvertTime(0, 12, 0)));
                self.playList.Add(new SlideShow.Scene(Enums.SlideShows.Pearlcat_Outro_2, self.ConvertTime(0, 15, 0), self.ConvertTime(0, 17, 0), self.ConvertTime(0, 25, 0)));

                foreach (var scene in self.playList)
                {
                    scene.startAt -= 1.1f;
                    scene.fadeInDoneAt -= 1.1f;
                    scene.fadeOutStartAt -= 1.1f;
                }
                self.processAfterSlideShow = ProcessManager.ProcessID.Game;
            }
            else if (id == Enums.SlideShows.PearlcatOutro)
            {
                if (self.manager.musicPlayer != null)
                {
                    self.waitForMusic = "RW_Outro_Theme";
                    self.stall = true;
                    self.manager.musicPlayer.MenuRequestsSong(self.waitForMusic, 1.5f, 10f);
                }

                self.playList.Add(new SlideShow.Scene(SceneID.Empty, 0f, 0f, 0f));
                self.playList.Add(new SlideShow.Scene(SceneID.Outro_1_Left_Swim, self.ConvertTime(0, 1, 20), self.ConvertTime(0, 5, 0), self.ConvertTime(0, 17, 0)));
                self.playList.Add(new SlideShow.Scene(SceneID.Outro_2_Up_Swim, self.ConvertTime(0, 21, 0), self.ConvertTime(0, 25, 0), self.ConvertTime(0, 37, 0)));

                self.playList.Add(new SlideShow.Scene(Enums.SlideShows.Pearlcat_Outro_1, self.ConvertTime(0, 41, 10), self.ConvertTime(0, 45, 20), self.ConvertTime(0, 46, 60)));
                self.playList.Add(new SlideShow.Scene(Enums.SlideShows.Pearlcat_Outro_2, self.ConvertTime(0, 48, 20), self.ConvertTime(0, 51, 0), self.ConvertTime(0, 55, 0)));


                foreach (var scene in self.playList)
                {
                    scene.startAt -= 1.1f;
                    scene.fadeInDoneAt -= 1.1f;
                    scene.fadeOutStartAt -= 1.1f;
                }
                self.processAfterSlideShow = ProcessManager.ProcessID.Credits;
            }
        });
    }


    private static void SlugcatSelectMenu_StartGame(ILContext il)
    {
        var c = new ILCursor(il);

        if (c.TryGotoNext(MoveType.After,
            i => i.MatchLdfld<MainLoopProcess>(nameof(MainLoopProcess.manager)),
            i => i.MatchLdsfld<ProcessManager.ProcessID>(nameof(ProcessManager.ProcessID.Game))))
        {
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate<Func<ProcessManager.ProcessID, SlugcatSelectMenu, SlugcatStats.Name, ProcessManager.ProcessID>>((id, self, character) =>
            {
                if (character == Enums.Pearlcat)
                {
                    self.manager.nextSlideshow = Enums.SlideShows.PearlcatIntro;
                    return ProcessManager.ProcessID.SlideShow;
                }

                return id;
            });

        }
    }

    private static void RainWorldGame_ExitToVoidSeaSlideShow(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(MoveType.After,
            x => x.MatchLdsfld<SlideShow.SlideShowID>(nameof(SlideShow.SlideShowID.WhiteOutro)));

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Func<SlideShow.SlideShowID, RainWorldGame, SlideShow.SlideShowID>>((id, self) =>
        {
            if (self.StoryCharacter == Enums.Pearlcat)
                return Enums.SlideShows.PearlcatOutro;

            return id;
        });
    }
}