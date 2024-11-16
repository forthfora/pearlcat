
using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace Pearlcat;

public class ModCompat_HooksIL
{
    public static void ApplyHooks()
    {
        try
        {
            IL.DevInterface.SoundPage.ctor += SoundPage_ctor;
        }
        catch (Exception e)
        {
            e.LogHookException();
        }
    }

    // Fix for DevTools not displaying sounds (credit to Bro for the code)
    private static void SoundPage_ctor(ILContext il)
    {
        var c = new ILCursor(il);

        if (!c.TryGotoNext(MoveType.Before, x => x.MatchLdstr("soundeffects/ambient")))
        {
            throw new Exception("Goto Failed");
        }

        c.MoveAfterLabels();
        c.Emit(OpCodes.Ldstr, "loadedsoundeffects/ambient");
        c.Remove();
    }
}
