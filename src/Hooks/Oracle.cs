namespace Pearlcat;

public static partial class Hooks
{
    private static void ApplyOracleHooks()
    {
        On.SSOracleBehavior.ctor += SSOracleBehavior_ctor;
        On.SSOracleBehavior.Update += SSOracleBehavior_Update;
    }

    private const string SS_ORACLE_ROOM = "SS_AI";
    private const string DM_ORACLE_ROOM = "DM_AI";


    private static void SSOracleBehavior_ctor(On.SSOracleBehavior.orig_ctor orig, SSOracleBehavior self, Oracle oracle)
    {
        orig(self, oracle);

    }

    private static void SSOracleBehavior_Update(On.SSOracleBehavior.orig_Update orig, SSOracleBehavior self, bool eu)
    {
        orig(self, eu);
    }
}
