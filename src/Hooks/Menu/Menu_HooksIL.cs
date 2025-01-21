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

        if (self.slugcatNumber == Enums.Pearlcat)
        {
            return self.slugcatImage?.sceneID == Enums.Scenes.Slugcat_Pearlcat;
        }

        return result;
    }


    private delegate bool orig_SlugcatPageHasGlow(SlugcatSelectMenu.SlugcatPage self);
    private static bool GetSlugcatPageHasGlow(orig_SlugcatPageHasGlow orig, SlugcatSelectMenu.SlugcatPage self)
    {
        var result = orig(self);

        if (self.slugcatNumber == Enums.Pearlcat)
        {
            return self.slugcatImage?.sceneID == Enums.Scenes.Slugcat_Pearlcat;
        }

        return result;
    }


    private static void SlugcatSelectMenu_StartGame(ILContext il)
    {
        var c = new ILCursor(il);

        if (!c.TryGotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt<SlugcatSelectMenu>(nameof(SlugcatSelectMenu.ContinueStartedGame))))
        {
            throw new Exception("Goto Failed");
        }

        var dest = il.DefineLabel();

        if (!c.TryGotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt<Input>(nameof(Input.GetKey))))
        {
            throw new Exception("Goto Failed");
        }

        if (!c.TryGotoNext(MoveType.After,
                x => x.MatchBrtrue(out dest)))
        {
            throw new Exception("Goto Failed");
        }

        c.Emit(OpCodes.Ldarg_0);

        c.EmitDelegate<Func<SlugcatSelectMenu, bool>>((_) =>
        {
            var save = Utils.MiscProgression;

            return save.IsSecretEnabled;
        });

        c.Emit(OpCodes.Brtrue, dest);
    }
}
