using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace Pearlcat;

public static partial class Hooks
{
    public static void ApplySlideShowHooks()
    {
        try
        {
            IL.RainWorldGame.GoToRedsGameOver += RainWorldGame_GoToRedsGameOver;
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("Slide Show Hooks IL Error: " + e + "\n" + e.StackTrace);
        }
    }

    private static void RainWorldGame_GoToRedsGameOver(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(MoveType.After,
            x => x.MatchCallOrCallvirt<PlayerProgression>(nameof(PlayerProgression.SaveWorldStateAndProgression)),
            x => x.MatchPop());

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