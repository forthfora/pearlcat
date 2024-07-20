
namespace Pearlcat;

public static partial class Hooks
{
    public static void ApplyPlayerPossessionFixes()
    {
        // Fix stunning player when possessing a creature
        On.Player.Stun += (orig, self, st) =>
        {
            if (self.TryGetPearlcatModule(out var playerModule))
            {
                if (playerModule.IsPossessingCreature) return;
            }

            orig(self, st);
        }; 
    }
}
