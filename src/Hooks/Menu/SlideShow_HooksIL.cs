using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace Pearlcat;

public static class SlideShow_HooksIL
{
    public static void ApplyHooks()
    {
        try
        {
            IL.RainWorldGame.GoToRedsGameOver += RainWorldGame_GoToRedsGameOver;
        }
        catch (Exception e)
        {
            e.LogHookException();
        }
    }

    private static void RainWorldGame_GoToRedsGameOver(ILContext il)
    {
        var c = new ILCursor(il);

        if (c.TryGotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt<PlayerProgression>(nameof(PlayerProgression.SaveWorldStateAndProgression)),
                x => x.MatchPop())
           ) return;

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Action<RainWorldGame>>((self) =>
        {
            if (self.GetStorySession?.saveStateNumber != Enums.Pearlcat) return;

            self.manager.statsAfterCredits = true;
            self.manager.nextSlideshow = Enums.Scenes.Pearlcat_AltOutro;
            self.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.SlideShow);
        });
    }
}
