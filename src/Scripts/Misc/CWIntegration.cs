using CWStuff;

namespace Pearlcat;

public static class CWIntegration
{
    public static void Init()
    {
        On.SSOracleBehavior.SpecialEvent += SSOracleBehavior_SpecialEvent;
    }

    private static void SSOracleBehavior_SpecialEvent(On.SSOracleBehavior.orig_SpecialEvent orig, SSOracleBehavior self, string eventName)
    {
        orig(self, eventName);
    }
}
