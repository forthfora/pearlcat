
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Pearlcat;

public static class ModCompat_Hooks
{
    public static void ApplyHooks()
    {
        On.Menu.PauseMenu.Singal += PauseMenu_Singal;
        On.DevInterface.TriggersPage.ctor += TriggersPage_ctor;
        Application.quitting += Application_quitting;
    }

    private static void Application_quitting()
    {
        Plugin.LogPearlcatDebugInfo();
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
                if (!playerModule.PlayerRef.TryGetTarget(out var player))
                {
                    continue;
                }

                Player_Helpers.ReleasePossession(player, playerModule);

                player.UpdateInventorySaveData(playerModule);

                for (var i = playerModule.Inventory.Count - 1; i >= 0; i--)
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

    // Fix for DevTools not displaying sounds (credit to Bro for the code)
    private static void TriggersPage_ctor(On.DevInterface.TriggersPage.orig_ctor orig, DevInterface.TriggersPage self, DevInterface.DevUI owner, string IDstring, DevInterface.DevUINode parentNode, string name)
    {
        orig(self, owner, IDstring, parentNode, name);

        List<string> songs = [];

        var files = AssetManager.ListDirectory("Music" + Path.DirectorySeparatorChar.ToString() + "Songs");

        foreach (var file in files)
        {
            var noExtension = Path.GetFileNameWithoutExtension(file);

            if (!songs.Contains(noExtension) && Path.GetExtension(file).ToLower() != ".meta")
            {
                songs.Add(noExtension);
            }
        }

        self.songNames = songs.ToArray();
    }
}
