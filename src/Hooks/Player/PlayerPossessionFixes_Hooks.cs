namespace Pearlcat;

public static class PlayerPossessionFixes_Hooks
{
    public static void ApplyHooks()
    {
        On.Player.Stun += PlayerOnStun;
        On.Player.Update += PlayerOnUpdate;
        On.Centipede.Shock += CentipedeOnShock;
        On.ArtificialIntelligence.VisualContact_BodyChunk += ArtificialIntelligenceOnVisualContact_BodyChunk;
    }


    // Make player copy possessed creature's lung capacity
    private static void PlayerOnUpdate(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);

        if (!self.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        if (playerModule.PossessedCreature is null || !playerModule.PossessedCreature.TryGetTarget(out var creature))
        {
            return;
        }

        if (creature?.realizedCreature is not AirBreatherCreature airBreather)
        {
            self.airInLungs = 1.0f;
        }
        else
        {
            self.airInLungs = airBreather.lungs;
        }
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
