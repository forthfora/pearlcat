using Mono.Cecil.Cil;
using MonoMod.Cil;
using Music;
using System;
using System.Linq;

namespace Pearlcat;

public static class Sound_HooksIL
{
    public static void ApplyHooks()
    {
        try
        {
            IL.Music.ProceduralMusic.Reset += ProceduralMusic_Reset;
        }
        catch (Exception e)
        {
            e.LogHookException();
        }
    }

    private static void ProceduralMusic_Reset(ILContext il)
    {
        var c = new ILCursor(il);

        if (!c.TryGotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt<ProceduralMusic.ProceduralMusicInstruction.Track>(
                    nameof(ProceduralMusic.ProceduralMusicInstruction.Track.AllowedInSubRegion)))
           ) return;

        c.Emit(OpCodes.Ldloc_2);
        c.Emit(OpCodes.Ldloc, 4);
        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldfld, typeof(ProceduralMusic).GetField(nameof(ProceduralMusic.musicPlayer)));
        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Func<int, int, MusicPlayer, ProceduralMusic, bool>>((j, k, musicPlayer, self) =>
        {
            var track = self.instruction.layers[j].tracks[k];
            var module = musicPlayer.GetModule();

            return module.Subregion != null && track.subRegions != null && track.subRegions.Contains(module.Subregion);
        });

        c.Emit(OpCodes.Or);
    }
}
