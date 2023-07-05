using MoreSlugcats;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Pearlcat;

public partial class Hooks
{
    // Fix up warp compatibility
    public static void ApplyWarpHooks()
    {
        On.Menu.PauseMenu.Singal += PauseMenu_Singal;

        //On.AbstractPhysicalObject.GetAllConnectedObjects += AbstractPhysicalObject_GetAllConnectedObjects;
    }

    public static readonly ConditionalWeakTable<RainWorldGame, StrongBox<bool>> JustWarpedData = new();

    private static void PauseMenu_Singal(On.Menu.PauseMenu.orig_Singal orig, Menu.PauseMenu self, Menu.MenuObject sender, string message)
    {
        orig(self, sender, message);

        if (WarpEnabled(self.game) && message.EndsWith("warp"))
        {
            Plugin.Logger.LogWarning("PEARLCAT WARP");

            foreach (var playerModule in self.game.GetAllPlayerData())
            {
                if (!playerModule.PlayerRef.TryGetTarget(out var player)) continue;

                player.UpdateInventorySaveData(playerModule);
                player.AbstractizeInventory();
            }

            orig(self, sender, message);

            var game = self.game;

            if (!JustWarpedData.TryGetValue(game, out var justWarped))
            {
                justWarped = new();
                JustWarpedData.Add(game, justWarped);
            }

            justWarped.Value = true;
        }
    }

    public static bool WarpEnabled(RainWorldGame game) => game.IsStorySession && (!ModManager.MSC || !game.rainWorld.safariMode);


    // Deprecated
    private static List<AbstractPhysicalObject> AbstractPhysicalObject_GetAllConnectedObjects(On.AbstractPhysicalObject.orig_GetAllConnectedObjects orig, AbstractPhysicalObject self)
    {
        var result = orig(self);

        if (self.realizedObject is not Player player) return result;

        if (!player.TryGetPearlcatModule(out var playerModule)) return result;

        result.AddRange(playerModule.Inventory);
        return result;
    }
}
