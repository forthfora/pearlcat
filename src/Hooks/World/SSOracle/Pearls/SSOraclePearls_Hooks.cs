using RWCustom;
using static Pearlcat.SSOraclePearls_Helpers;

namespace Pearlcat;

public static class SSOraclePearls_Hooks
{
    public static void ApplyHooks()
    {
        On.PebblesPearl.Update += PebblesPearl_Update;
        On.SSOracleBehavior.Update += SSOracleBehavior_Update;
    }


    private static void SSOracleBehavior_Update(On.SSOracleBehavior.orig_Update orig, SSOracleBehavior self, bool eu)
    {
        orig(self, eu);

        if (!self.oracle.room.game.IsPearlcatStory() || !self.IsPebbles())
        {
            return;
        }

        if (self.timeSinceSeenPlayer == -1)
        {
            return;
        }

        if (self.getToWorking == 0.0f)
        {
            return;
        }

        var module = self.GetModule();

        var orbitSmall = new List<DataPearl>();
        var orbitMid = new List<DataPearl>();
        var orbitLarge = new List<DataPearl>();

        foreach (var updatable in self.oracle.room.updateList)
        {
            if (updatable is not DataPearl dataPearl)
            {
                continue;
            }

            if (dataPearl.grabbedBy.Count > 0)
            {
                continue;
            }

            if (!ModCompat_Helpers.RainMeadow_IsMine(dataPearl.abstractPhysicalObject))
            {
                continue;
            }

            if (dataPearl.abstractPhysicalObject.IsPlayerPearl())
            {
                continue;
            }

            if (dataPearl == module.PearlToRead || dataPearl == module.PearlBeingRead || dataPearl == module.PearlToReturn)
            {
                continue;
            }

            if (self.player is not null && Custom.DistLess(self.player.firstChunk.pos, self.oracle.firstChunk.pos, 180.0f))
            {
                orbitLarge.Add(dataPearl);
                continue;
            }

            if (orbitSmall.Count < 5)
            {
                orbitSmall.Add(dataPearl);
            }
            else if (orbitMid.Count < 10)
            {
                orbitMid.Add(dataPearl);
            }
            else
            {
                orbitLarge.Add(dataPearl);
            }
        }

        var origin = new Vector2(490.0f, 340.0f);

        AnimatePebblesPearlOrbit(origin, 80.0f, 0.0002f, orbitSmall);
        AnimatePebblesPearlOrbit(origin, 180.0f, -0.00015f, orbitMid);
        AnimatePebblesPearlOrbit(origin, 265.0f, 0.0001f, orbitLarge);
    }

    private static void PebblesPearl_Update(On.PebblesPearl.orig_Update orig, PebblesPearl self, bool eu)
    {
        orig(self, eu);

        if (!self.abstractPhysicalObject.IsPlayerPearl())
        {
            return;
        }

        self.abstractPhysicalObject.slatedForDeletion = false;
        self.label?.Destroy();
    }
}
