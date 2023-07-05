using System;
using System.Linq;

namespace Pearlcat;

public static partial class Hooks
{
    public static void ApplyHooks()
    {
        On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        On.RainWorld.PostModsInit += RainWorld_PostModsInit;

        // Core
        ApplySaveDataHooks();
        ApplyGameDataHooks();
        ApplyMenuHooks();

        // Player
        ApplyPlayerHooks();
        ApplyPlayerGraphicsHooks();
        ApplyPlayerObjectDataHooks();

        // World
        ApplyOracleHooks();
        ApplySoundHooks();
        ApplyWarpHooks();
    }


    public static bool isInit = false;

    private static void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        try
        {
            if (isInit) return;
            isInit = true;

            var mod = ModManager.ActiveMods.FirstOrDefault(mod => mod.id == Plugin.MOD_ID);

            Plugin.MOD_NAME = mod.name;
            Plugin.VERSION = mod.version;
            Plugin.AUTHORS = mod.authors;


            // Init Enums
            _ = Enums.General.Pearlcat;
            _ = Enums.Pearls.AS_PearlBlue;
            _ = Enums.Sounds.Pearlcat_PearlScroll;

            MachineConnector.SetRegisteredOI(Plugin.MOD_ID, PearlcatOptions.Instance);
            AssetLoader.LoadAssets();
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("OnModsInit:\n" + e.Message);
        }
        finally
        {
            orig(self);
        }
    }


    public static bool isPostInit = false;

    private static void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
    {
        try
        {
            if (isPostInit) return;
            isPostInit = true;

            POEffectManager.RegisterEffects();
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("PostModsInit:\n" + e.Message);
        }
        finally
        {
            orig(self);
        }
    }
}
