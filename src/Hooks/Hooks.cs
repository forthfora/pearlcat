using System;
using System.Linq;

namespace Pearlcat;

public static partial class Hooks
{
    public static void ApplyHooks()
    {
        // Core
        ApplySaveDataHooks();
        ApplyMenuHooks();

        // Player
        ApplyPlayerHooks();
        ApplyPlayerGraphicsHooks();
        ApplyPlayerObjectDataHooks();

        // World
        ApplyWorldHooks();
        ApplyOracleHooks();
        ApplySoundHooks();
        ApplyWarpHooks();
    }

    public static void ApplyInit()
    {
        On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        On.RainWorld.PostModsInit += RainWorld_PostModsInit;

        On.ModManager.RefreshModsLists += ModManager_RefreshModsLists;
    }

    public static bool IsInit { get; private set; } = false;

    private static void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        try
        {
            if (IsInit) return;
            IsInit = true;

            ApplyHooks();

            var mod = ModManager.ActiveMods.FirstOrDefault(mod => mod.id == Plugin.MOD_ID);

            Plugin.MOD_NAME = mod.name;
            Plugin.VERSION = mod.version;
            Plugin.AUTHORS = mod.authors;


            // Init Enums
            _ = Enums.General.Pearlcat;
            _ = Enums.Pearls.AS_PearlBlue;
            _ = Enums.Sounds.Pearlcat_PearlScroll;

            MachineConnector.SetRegisteredOI(Plugin.MOD_ID, ModOptions.Instance);
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

    public static bool IsPostInit { get; private set; } = false;

    private static void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
    {
        try
        {
            if (IsPostInit) return;
            IsPostInit = true;

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

    // fix the stupid remix load order issues
    private static void ModManager_RefreshModsLists(On.ModManager.orig_RefreshModsLists orig, RainWorld rainWorld)
    {
        orig(rainWorld);

        if (!ModManager.MSC) return;

        var modIndex = ModManager.ActiveMods.FindIndex(mod => mod.id == Plugin.MOD_ID);
        if (modIndex == -1) return;
        
        var mod = ModManager.ActiveMods[modIndex];
        
        var mscIndex = ModManager.ActiveMods.FindIndex(mod => mod.id == "moreslugcats");
        if (mscIndex == -1) return;

        if (modIndex > mscIndex) return;

        ModManager.ActiveMods.Remove(mod);
        ModManager.ActiveMods.Insert(mscIndex + 1, mod);
    }
}
