
using UnityEngine;

namespace Pearlcat;

public class Hooks_Fixes
{
    public static void ApplyHooks_Fixes()
    {
        On.Menu.PauseMenu.Singal += PauseMenu_Singal;

        Application.quitting += Application_quitting;
    }

    private static void Application_quitting()
    {
        Utils.LogPearlcatDebugInfo();
    }

    // Warp Fix
    private static void PauseMenu_Singal(On.Menu.PauseMenu.orig_Singal orig, Menu.PauseMenu self, Menu.MenuObject sender, string message)
    {
        orig(self, sender, message);

        if (self.game.IsWarpAllowed() && message.EndsWith("warp"))
        {
            Plugin.Logger.LogInfo("PEARLCAT WARP");

            foreach (var playerModule in self.game.GetAllPlayerData())
            {
                if (!playerModule.PlayerRef.TryGetTarget(out var player)) continue;

                Hooks_Player.ReleasePossession(player, playerModule);

                player.UpdateInventorySaveData(playerModule);

                for (int i = playerModule.Inventory.Count - 1; i >= 0; i--)
                {
                    var item = playerModule.Inventory[i];

                    player.RemoveFromInventory(item);
                     
                    if (player.abstractCreature.world.game.GetStorySession is StoryGameSession story)
                    {
                        story.RemovePersistentTracker(item);
                    }

                    item.destroyOnAbstraction = true;
                    item.Abstractize(item.pos);
                }


                playerModule.JustWarped = true;
            }
        }
    }
}
