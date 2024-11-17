using System;

namespace Pearlcat;

public static class PearlpupIllness_Hooks
{
    public static void ApplyHooks()
    {
        On.RedsIllness.RedsIllnessEffect.CanShowPlayer += RedsIllnessEffect_CanShowPlayer;

        On.RedsIllness.AddFood += RedsIllness_AddFood;
        On.RedsIllness.AddQuarterFood += RedsIllness_AddQuarterFood;
    }


    private static void RedsIllness_AddFood(On.RedsIllness.orig_AddFood orig, RedsIllness self, int i)
    {
        if (!self.player.IsPearlpup())
        {
            orig(self, i);
            return;
        }

        var foodFac = 1.0f;
        var num = Math.Min(i * foodFac, self.player.slugcatStats.maxFood - self.player.playerState.foodInStomach);

        self.totFoodCounter += num / foodFac;
        self.floatFood += num;
        self.UpdateFood();
    }

    private static void RedsIllness_AddQuarterFood(On.RedsIllness.orig_AddQuarterFood orig, RedsIllness self)
    {
        if (!self.player.IsPearlpup())
        {
            orig(self);
            return;
        }

        var foodFac = 1.0f;
        var num = Math.Min(0.25f * foodFac, self.player.slugcatStats.maxFood - self.player.playerState.foodInStomach);

        self.totFoodCounter += num / foodFac;
        self.floatFood += num;
        self.UpdateFood();
    }

    private static bool RedsIllnessEffect_CanShowPlayer(On.RedsIllness.RedsIllnessEffect.orig_CanShowPlayer orig, Player player)
    {
        var result = orig(player);

        if (player.IsPearlpup())
        {
            return false;
        }

        return result;
    }
}
