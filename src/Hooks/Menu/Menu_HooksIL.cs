using Menu;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;
using UnityEngine;

namespace Pearlcat;

public static class Menu_HooksIL
{
    public static void ApplyHooks()
    {
        try
        {
            _ = new Hook(
                typeof(SlugcatSelectMenu.SlugcatPage).GetProperty(nameof(SlugcatSelectMenu.SlugcatPage.HasMark), BindingFlags.Instance | BindingFlags.Public)?.GetGetMethod(),
                typeof(Menu_HooksIL).GetMethod(nameof(GetSlugcatPageHasMark), BindingFlags.Static | BindingFlags.NonPublic)
            );
        }
        catch (Exception e)
        {
            e.LogHookException();
        }

        try
        {
            _ = new Hook(
                typeof(SlugcatSelectMenu.SlugcatPage).GetProperty(nameof(SlugcatSelectMenu.SlugcatPage.HasGlow), BindingFlags.Instance | BindingFlags.Public)?.GetGetMethod(),
                typeof(Menu_HooksIL).GetMethod(nameof(GetSlugcatPageHasGlow), BindingFlags.Static | BindingFlags.NonPublic)
            );
        }
        catch (Exception e)
        {
            e.LogHookException();
        }

        try
        {
            IL.Menu.SlugcatSelectMenu.StartGame += SlugcatSelectMenu_StartGame;
        }
        catch (Exception e)
        {
            e.LogHookException();
        }
    }


    private delegate bool orig_SlugcatPageHasMark(SlugcatSelectMenu.SlugcatPage self);
    private static bool GetSlugcatPageHasMark(orig_SlugcatPageHasMark orig, SlugcatSelectMenu.SlugcatPage self)
    {
        var result = orig(self);

        return self.slugcatNumber == Enums.Pearlcat || result;
    }


    private delegate bool orig_SlugcatPageHasGlow(SlugcatSelectMenu.SlugcatPage self);
    private static bool GetSlugcatPageHasGlow(orig_SlugcatPageHasGlow orig, SlugcatSelectMenu.SlugcatPage self)
    {
        var result = orig(self);

        return self.slugcatNumber == Enums.Pearlcat || result;
    }


    private static void SlugcatSelectMenu_StartGame(ILContext il)
    {
        var c = new ILCursor(il);

        if (!c.TryGotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt<SlugcatSelectMenu>(nameof(SlugcatSelectMenu.ContinueStartedGame)))
           ) return;

        var dest = il.DefineLabel();

        if (!c.TryGotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt<Input>(nameof(Input.GetKey)))
           ) return;

        if (!c.TryGotoNext(MoveType.After,
                x => x.MatchBrtrue(out dest))
           ) return;

        c.Emit(OpCodes.Ldarg_0);

        c.EmitDelegate<Func<SlugcatSelectMenu, bool>>((self) =>
        {
            var save = Utils.GetMiscProgression();

            return save.IsSecretEnabled;
        });

        c.Emit(OpCodes.Brtrue, dest);
    }
}
