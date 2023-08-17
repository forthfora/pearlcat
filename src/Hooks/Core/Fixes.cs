using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pearlcat;

public partial class Hooks
{
    public static void ApplyFixesHooks()
    {
        On.Menu.PauseMenu.Singal += PauseMenu_Singal;
        On.ModManager.ModApplyer.ctor += ModApplyer_ctor;

        //On.AbstractPhysicalObject.GetAllConnectedObjects += AbstractPhysicalObject_GetAllConnectedObjects;
    }

    public static bool IsSplitScreenCoopActive => ModManager.ActiveMods.Any(x => x.id == "henpemaz_splitscreencoop");
    public static bool IsMergeFixActive => ModManager.ActiveMods.Any(x => x.id == "bro.mergefix");
    public static bool WarpEnabled(RainWorldGame game) => game.IsStorySession && (!ModManager.MSC || !game.rainWorld.safariMode);
    

    // WARP
    private static void PauseMenu_Singal(On.Menu.PauseMenu.orig_Singal orig, Menu.PauseMenu self, Menu.MenuObject sender, string message)
    {
        orig(self, sender, message);

        if (WarpEnabled(self.game) && message.EndsWith("warp"))
        {
            Plugin.Logger.LogInfo("PEARLCAT WARP");

            foreach (var playerModule in self.game.GetAllPlayerData())
            {
                if (!playerModule.PlayerRef.TryGetTarget(out var player)) continue;

                player.UpdateInventorySaveData(playerModule);

                for (int i = playerModule.Inventory.Count - 1; i >= 0; i--)
                {
                    var item = playerModule.Inventory[i];

                    player.RemoveFromInventory(item);

                    item.destroyOnAbstraction = true;
                    item.Abstractize(item.pos);
                }

                playerModule.JustWarped = true;
            }
        }
    }
    // Deprecated
    private static List<AbstractPhysicalObject> AbstractPhysicalObject_GetAllConnectedObjects(On.AbstractPhysicalObject.orig_GetAllConnectedObjects orig, AbstractPhysicalObject self)
    {
        var result = orig(self);

        if (self.realizedObject is not Player player) return result;

        if (!player.TryGetPearlcatModule(out var playerModule)) return result;

        result.AddRange(playerModule.Inventory);
        return result;
    }


    // MERGEFIX
    private static void ModApplyer_ctor(On.ModManager.ModApplyer.orig_ctor orig, ModManager.ModApplyer self, ProcessManager manager, List<bool> pendingEnabled, List<int> pendingLoadOrder)
    {
        orig(self, manager, pendingEnabled, pendingLoadOrder);

        if (IsMergeFixActive) return;

        self.pendingLoadOrder.Reverse(); //flip it back so that the values line up with the mods
        int highest = self.pendingLoadOrder.Max((int t) => t); //find the highest mod
        self.pendingLoadOrder = self.pendingLoadOrder.Select(t => highest - t).ToList(); //invert the real load order
    }


    // SPLIT SCREEN
    public static Vector2 GetSplitScreenHUDOffset(this RoomCamera rCam, int camIndex)
    {
        var splitScreenOffset = Vector2.zero;

        // some reflection required
        if (IsSplitScreenCoopActive)
        {
            var loadedAsms = AppDomain.CurrentDomain.GetAssemblies().ToList();

            foreach (var asm in loadedAsms)
            {
                var nameInfo = asm.GetName();
                if (nameInfo.Name != "SplitScreen Co-op") continue;

                var type = asm.GetType("SplitScreenCoop.SplitScreenCoop");
                if (type == null) break;

                var methodInfo = type.GetMethod("GetSplitScreenHudOffset");
                if (methodInfo == null) break;

                var instance = UnityEngine.Object.FindObjectOfType(type);
                if (instance == null) break;

                var args = new object[] { rCam, camIndex };
                splitScreenOffset = (Vector2)methodInfo.Invoke(instance, args);
                break;
            }
        }

        return splitScreenOffset;
    }
}
