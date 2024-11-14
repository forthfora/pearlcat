
namespace Pearlcat;

public static class PlayerPossessionFixes_Hooks
{
    public static void ApplyHooks()
    {
        On.Player.Stun += PlayerOnStun;
        On.Centipede.Shock += CentipedeOnShock;
    }

    private static void CentipedeOnShock(On.Centipede.orig_Shock orig, Centipede self, PhysicalObject shockobj)
    {


        orig(self, shockobj);
    }

    // Fix stunning player when possessing a creature
    private static void PlayerOnStun(On.Player.orig_Stun orig, Player self, int st)
    {
        if (self.TryGetPearlcatModule(out var playerModule))
        {
            if (playerModule.IsPossessingCreature) return;
        }

        orig(self, st);
    }
}
