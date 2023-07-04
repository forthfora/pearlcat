
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Pearlcat;

public partial class Hooks
{
    // Fix up warp compatibility
    public static void ApplyWarpHooks()
    {
        On.AbstractPhysicalObject.GetAllConnectedObjects += AbstractPhysicalObject_GetAllConnectedObjects;
        On.Menu.PauseMenu.WarpSignal += PauseMenu_WarpSignal;
    }


    public static readonly ConditionalWeakTable<RainWorldGame, StrongBox<bool>> JustWarpedData = new();

    private static void PauseMenu_WarpSignal(On.Menu.PauseMenu.orig_WarpSignal orig, Menu.PauseMenu self, Menu.MenuObject sender, string message)
    {
        orig(self, sender, message);

        var game = self.game;

        if (!JustWarpedData.TryGetValue(game, out var justWarped))
        {
            justWarped = new();
            JustWarpedData.Add(game, justWarped);
        }

        justWarped.Value = true;
    }

    private static List<AbstractPhysicalObject> AbstractPhysicalObject_GetAllConnectedObjects(On.AbstractPhysicalObject.orig_GetAllConnectedObjects orig, AbstractPhysicalObject self)
    {
        var result = orig(self);

        if (self.realizedObject is not Player player) return result;

        if (!player.TryGetPearlcatModule(out var playerModule)) return result;

        result.AddRange(playerModule.Inventory);
        return result;
    }
}
