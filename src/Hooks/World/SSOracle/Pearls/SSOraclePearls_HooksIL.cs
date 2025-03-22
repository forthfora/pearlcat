using MonoMod.RuntimeDetour;
using System.Reflection;

namespace Pearlcat;

public static class SSOraclePearls_HooksIL
{
    public static void ApplyHooks()
    {
        try
        {
            _ = new Hook(
                typeof(PebblesPearl).GetProperty(nameof(PebblesPearl.NotCarried), BindingFlags.Instance | BindingFlags.Public)?.GetGetMethod(),
                typeof(SSOraclePearls_HooksIL).GetMethod(nameof(GetPebblesPearlNotCarried), BindingFlags.Static | BindingFlags.NonPublic)
            );
        }
        catch (Exception e)
        {
            e.LogHookException();
        }
    }


    private static bool GetPebblesPearlNotCarried(Func<PebblesPearl, bool> orig, PebblesPearl self)
    {
        var result = orig(self);

        if (self.abstractPhysicalObject.world.game.IsPearlcatStory())
        {
            if (self.oracle?.oracleBehavior is SSOracleBehavior behavior && behavior.timeSinceSeenPlayer >= 0)
            {
                return false;
            }
        }

        return result;
    }
}
