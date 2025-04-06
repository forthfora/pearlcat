using Mono.Cecil.Cil;
using MonoMod.Cil;
using Music;

namespace Pearlcat;

public static class Sound_HooksIL
{
    public static void ApplyHooks()
    {
        try
        {
            IL.Music.ProceduralMusic.BuildThreatLayerPool += ProceduralMusic_BuildThreatLayerPool;
        }
        catch (Exception e)
        {
            e.LogHookException();
        }
    }

    private static void ProceduralMusic_BuildThreatLayerPool(ILContext il)
    {
        var c = new ILCursor(il);

        if (!c.TryGotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt<ProceduralMusic.ProceduralMusicInstruction.Track>(
                    nameof(ProceduralMusic.ProceduralMusicInstruction.Track.AllowedInSubRegion))))
        {
            throw new Exception("Goto Failed");
        }

        c.Emit(OpCodes.Ldloc_2);
        c.Emit(OpCodes.Ldloc, 4);
        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldfld, typeof(ProceduralMusic).GetField(nameof(ProceduralMusic.musicPlayer)));
        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Func<int, int, MusicPlayer, ProceduralMusic, bool>>((j, k, musicPlayer, self) =>
        {
            if (!ModOptions.PearlThreatMusic)
            {
                return false;
            }

            if (j >= self.instruction.layers.Count)
            {
                return false;
            }

            if (k >= self.instruction.layers[j].tracks.Count)
            {
                return false;
            }

            var track = self.instruction.layers[j].tracks[k];
            var module = musicPlayer.GetModule();

            return module.Subregion is not null && track.subRegions is not null && track.subRegions.Contains(module.Subregion);
        });

        c.Emit(OpCodes.Or);
    }
}
