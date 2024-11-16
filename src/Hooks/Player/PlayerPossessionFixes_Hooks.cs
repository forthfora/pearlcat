
namespace Pearlcat;

public static class PlayerPossessionFixes_Hooks
{
    public static void ApplyHooks()
    {
        On.Player.Stun += PlayerOnStun;
        On.Centipede.Shock += CentipedeOnShock;
        On.ArtificialIntelligence.VisualContact_BodyChunk += ArtificialIntelligenceOnVisualContact_BodyChunk;
    }


    // Hide player's true body from creature AI when possessing
    private static bool ArtificialIntelligenceOnVisualContact_BodyChunk(On.ArtificialIntelligence.orig_VisualContact_BodyChunk orig, ArtificialIntelligence self, BodyChunk chunk)
    {
        if (chunk.owner is Player player && player.TryGetPearlcatModule(out var module))
        {
            if (module.IsPossessingCreature)
            {
                return false;
            }
        }

        return orig(self, chunk);
    }


    // Fix for possessed centipedes shocking the player
    private static void CentipedeOnShock(On.Centipede.orig_Shock orig, Centipede self, PhysicalObject shockobj)
    {
        if (shockobj is Player player && player.TryGetPearlcatModule(out var module))
        {
            if (module.PossessedCreature?.TryGetTarget(out var creature) == true && creature?.realizedCreature == self)
            {
                return;
            }
        }

        orig(self, shockobj);
    }


    // Fix stunning player when possessing a creature
    private static void PlayerOnStun(On.Player.orig_Stun orig, Player self, int st)
    {
        if (self.TryGetPearlcatModule(out var playerModule))
        {
            if (playerModule.IsPossessingCreature)
            {
                return;
            }
        }

        orig(self, st);
    }
}
