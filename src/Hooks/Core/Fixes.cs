using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Pearlcat;

public partial class Hooks
{
    public static void ApplyFixesHooks()
    {
        On.Menu.PauseMenu.Singal += PauseMenu_Singal;

        On.DevInterface.TriggersPage.ctor += TriggersPage_ctor;

        Application.quitting += Application_quitting;

        // (this already has a try catch)
        IL.DevInterface.SoundPage.ctor += SoundPage_ctor;
    }


    private static void Application_quitting()
    {
        Plugin.LogPearlcatDebugInfo();
    }

    // Warp
    private static void PauseMenu_Singal(On.Menu.PauseMenu.orig_Singal orig, Menu.PauseMenu self, Menu.MenuObject sender, string message)
    {
        orig(self, sender, message);

        if (self.game.WarpEnabled() && message.EndsWith("warp"))
        {
            Plugin.Logger.LogInfo("PEARLCAT WARP");

            foreach (var playerModule in self.game.GetAllPlayerData())
            {
                if (!playerModule.PlayerRef.TryGetTarget(out var player)) continue;

                ReleasePossession(player, playerModule);

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


    // fix for dev tools w/ custom ambient SFX, thanks Bro
    private static void TriggersPage_ctor(On.DevInterface.TriggersPage.orig_ctor orig, DevInterface.TriggersPage self, DevInterface.DevUI owner, string IDstring, DevInterface.DevUINode parentNode, string name)
    {
        orig(self, owner, IDstring, parentNode, name);

        List<string> songs = new();

        string[] files = AssetManager.ListDirectory("Music" + Path.DirectorySeparatorChar.ToString() + "Songs");

        foreach (string file in files)
        {
            string noExtension = Path.GetFileNameWithoutExtension(file);

            if (!songs.Contains(noExtension) && Path.GetExtension(file).ToLower() != ".meta")
                songs.Add(noExtension);
        }

        self.songNames = songs.ToArray();
    }

    private static void SoundPage_ctor(ILContext il)
    {
        var c = new ILCursor(il);

        if (c.TryGotoNext(MoveType.Before, x => x.MatchLdstr("soundeffects/ambient")))
        {
            c.MoveAfterLabels();
            c.Emit(OpCodes.Ldstr, "loadedsoundeffects/ambient");
            c.Remove();
        }
    }
}
