
namespace Pearlcat;

public static class PlayerPossessionFixes_Hooks
{
    public static void ApplyHooks()
    {
        // Fix stunning player when possessing a creature
        On.Player.Stun += OnPlayerOnStun;
    }

    private static void OnPlayerOnStun(On.Player.orig_Stun orig, Player self, int st)
    {
        if (self.TryGetPearlcatModule(out var playerModule))
        {
            if (playerModule.IsPossessingCreature) return;
        }

        orig(self, st);
    }
}
