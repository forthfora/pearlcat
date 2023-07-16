namespace Pearlcat;

public static partial class Hooks
{
    public static void ApplyOracleHooks()
    {
        On.SSOracleBehavior.ctor += SSOracleBehavior_ctor;
        On.SSOracleBehavior.Update += SSOracleBehavior_Update;
    }

    public const string SS_ORACLE_ROOM = "SS_AI";


    public static void SSOracleBehavior_ctor(On.SSOracleBehavior.orig_ctor orig, SSOracleBehavior self, Oracle oracle)
    {
        orig(self, oracle);

    }

    public static void SSOracleBehavior_Update(On.SSOracleBehavior.orig_Update orig, SSOracleBehavior self, bool eu)
    {
        orig(self, eu);
    }
}
