using System;

namespace Pearlcat;

public static partial class Hooks
{
    public static void ApplyHooks()
    {
        On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        On.RainWorld.OnModsDisabled += RainWorld_OnModsDisabled;

        // Core
        ApplySaveLoadHooks();
        ApplyMenuHooks();

        // Player
        ApplyPlayerHooks();
        ApplyPlayerGraphicsHooks();
        ApplyPlayerObjectDataHooks();

        // World
        ApplyMusicHooks();
        ApplyOracleHooks();
    }

    public static bool isInit = false;

    public static void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        try
        {
            if (isInit) return;
            isInit = true;

            MachineConnector.SetRegisteredOI(Plugin.MOD_ID, Options.instance);

            Enums.RegisterEnums();
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

    public static void RainWorld_OnModsDisabled(On.RainWorld.orig_OnModsDisabled orig, RainWorld self, ModManager.Mod[] newlyDisabledMods)
    {
        try
        {
            Enums.UnregisterEnums();
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("OnModsDisabled:\n" + e.Message);
        }
        finally
        {
            orig(self, newlyDisabledMods);
        }
    }
}
