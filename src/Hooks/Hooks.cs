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
        ApplySlideShowHooks();

        // Player
        ApplyPlayerHooks();
        ApplyPlayerGraphicsHooks();
        ApplyPlayerObjectDataHooks();

        ApplyPearlpupHooks();
        ApplyPearlpupGraphicsHooks();

        // World
        ApplyWorldHooks();
        ApplySoundHooks();
        ApplyWarpHooks();

        ApplySSOracleHooks();
        ApplySLOracleHooks();
    }

    public static void ApplyInit()
    {
        On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        On.RainWorld.PostModsInit += RainWorld_PostModsInit;

        // deprecated
        //On.ModManager.RefreshModsLists += ModManager_RefreshModsLists;
    }

    public static bool IsInit { get; private set; } = false;

    private static void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        try
        {
            ModOptions.RegisterOI();

            if (IsInit) return;
            IsInit = true;

            ApplyHooks();

            var mod = ModManager.ActiveMods.FirstOrDefault(mod => mod.id == Plugin.MOD_ID);

            Plugin.MOD_NAME = mod.name;
            Plugin.VERSION = mod.version;
            Plugin.AUTHORS = mod.authors;


            // Init Enums
            _ = Enums.Pearlcat;

            _ = Enums.SSOracle.Pearlcat_SSActionGeneral;
            _ = Enums.Pearls.RM_Pearlcat;
            _ = Enums.Sounds.Pearlcat_PearlScroll;

            _ = Enums.SlideShows.PearlcatIntro;

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

    private static void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
    {
        try
        {
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

        try
        {
            if (!ModManager.MSC) return;

            var modIndex = ModManager.ActiveMods.FindIndex(mod => mod.id == Plugin.MOD_ID);
            if (modIndex == -1) return;
        
            var mod = ModManager.ActiveMods[modIndex];
        
            var mscIndex = ModManager.ActiveMods.FindIndex(mod => mod.id == "moreslugcats");
            if (mscIndex == -1) return;

            if (modIndex > mscIndex) return;

            ModManager.ActiveMods.Remove(mod);
            ModManager.ActiveMods.Insert(mscIndex, mod);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("ModManager.RefreshModsLists:\n" + e.Message);
        }
    }
}
