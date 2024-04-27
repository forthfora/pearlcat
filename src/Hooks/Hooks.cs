using System;
using System.Linq;
using UnityEngine;

namespace Pearlcat;

public static partial class Hooks
{
    public static void ApplyHooks()
    {
        // Core
        ApplySaveDataHooks();
        ApplyMenuHooks();
        ApplySlideShowHooks();
        ApplyFixesHooks();

        // Player
        ApplyPlayerHooks();
        ApplyPlayerGraphicsHooks();
        ApplyPlayerObjectManagementHooks();

        // Pearlpup
        ApplyPearlpupHooks();
        ApplyPearlpupGraphicsHooks();
        ApplyPearlpupIllnessHooks();
        ApplyPearlpupPearlHooks();

        // World
        ApplyWorldHooks();
        ApplyWorldCreatureHooks();
        ApplySoundHooks();

        ApplySSOracleHooks();
        ApplySSOracleConvoHooks();
        ApplySSOraclePearlsHooks();

        ApplySLOracleHooks();
    }

    public static void ApplyInit()
    {
        On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        On.RainWorld.PostModsInit += RainWorld_PostModsInit;
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

            _ = Enums.Scenes.Slugcat_Pearlcat;

            Enums.Dreams.RegisterDreams();

            AssetLoader.LoadAssets();

            // Only init this if IIC is installed
            if (IsImprovedInputInstalled)
            {
                InitIICKeybinds();
            }

            if (Utils.IsCWActive)
            {
                Utils.InitCWIntegration();
            }

            var initMessage = "PEARLCAT SAYS HELLO FROM INIT! (VERSION: " + Plugin.VERSION + ")";

            Debug.Log(initMessage);

            Plugin.Logger.LogInfo("START OF BEPINEX LOG");
            Plugin.Logger.LogInfo(initMessage);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("OnModsInit:\n" + e.Message + "\n" + e.StackTrace);
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
            Plugin.Logger.LogError("PostModsInit:\n" + e.Message + "\n" + e.StackTrace);
        }
        finally
        {
            orig(self);
        }
    }
}
