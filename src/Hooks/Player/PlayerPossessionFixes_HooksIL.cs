
using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Pearlcat;

public static class PlayerPossessionFixes_HooksIL
{
    public static void ApplyHooks()
    {
        try
        {
            IL.BigEel.JawsSnap += BigEel_JawsSnap;
        }
        catch (Exception e)
        {
            e.LogHookException();
        }
    }

    // Leviathan - prevent killing player when possessed
    private static void BigEel_JawsSnap(ILContext il)
    {
        var c = new ILCursor(il);

        if (!c.TryGotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt<BigEel>(nameof(BigEel.InBiteArea))))
        {
            throw new Exception("Goto Failed");
        }

        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldloc, 6);
        c.Emit(OpCodes.Ldloc, 7);
        c.Emit(OpCodes.Ldloc, 8);


        c.EmitDelegate<Func<BigEel, int, int, int, bool>>((self, j, k, l) =>
        {
            var bodyChunk = self.room.physicalObjects[j][k].bodyChunks[l];

            if (bodyChunk.owner is Player player && player.TryGetPearlcatModule(out var playerModule))
            {
                if (playerModule.PossessedCreature?.TryGetTarget(out var creature) == true &&
                    creature.realizedCreature == self)
                {
                    return false;
                }
            }

            return true;
        });

        c.Emit(OpCodes.And);
    }
}
