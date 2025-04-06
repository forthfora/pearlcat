using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Pearlcat;

public static class SSOracleConversation_HooksIL
{
    public static void ApplyHooks()
    {
        try
        {
            IL.SSOracleBehavior.Update += SSOracleBehavior_UpdateIL;
        }
        catch (Exception e)
        {
            e.LogHookException();
        }
    }

    private static void SSOracleBehavior_UpdateIL(ILContext il)
    {
        var c = new ILCursor(il);

        if (!c.TryGotoNext(MoveType.After,
                x => x.MatchLdstr("Yes, help yourself. They are not edible.")))
        {
            throw new Exception("Goto Failed");
        }

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Func<string, SSOracleBehavior, string>>((origText, self) =>
        {
            if (self.oracle.room.game.IsPearlcatStory() && self.IsPebbles())
            {
                var miscProg = Utils.MiscProgression;

                if (miscProg.HasTrueEnding)
                {
                    return self.Translate("...ah... are those still a fascination to you? You really are no different from your mother...");
                }

                return self.Translate("...oh? Take them, the data they contain is worthless to me. I suppose they'd be far more useful to you...");
            }

            return origText;
        });
    }
}
