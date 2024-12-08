using MonoMod.RuntimeDetour;
using System.Reflection;

namespace Pearlcat;

public static class SSOraclePearls_HooksIL
{
    public static void ApplyHooks()
    {
        _ = new Hook(
            typeof(PebblesPearl).GetProperty(nameof(PebblesPearl.NotCarried), BindingFlags.Instance | BindingFlags.Public)?.GetGetMethod(),
            typeof(SSOraclePearls_HooksIL).GetMethod(nameof(GetPebblesPearlNotCarried), BindingFlags.Static | BindingFlags.NonPublic)
        );
    }


    private delegate bool orig_PebblesPearlNotCarried(PebblesPearl self);
    private static bool GetPebblesPearlNotCarried(orig_PebblesPearlNotCarried orig, PebblesPearl self)
    {
        var result = orig(self);

        if (self.room.game.IsPearlcatStory())
        {
            if (self.oracle?.oracleBehavior is SSOracleBehavior behavior && behavior.timeSinceSeenPlayer >= 0)
            {
                return false;
            }
        }

        return result;
    }
}
