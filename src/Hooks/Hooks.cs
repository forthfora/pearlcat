using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Pearlcat;

public static class Hooks
{
    public static bool IsInit { get; private set; }

    public static void ApplyInitHooks()
    {
        On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        On.RainWorld.PostModsInit += RainWorld_PostModsInit;
    }

    public static void ApplyHooks()
    {
        // Misc
        ModCompat_Hooks.ApplyHooks();
        ModCompat_HooksIL.ApplyHooks();

        SaveData_Hooks.ApplyHooks();

        Sound_Hooks.ApplyHooks();
        Sound_HooksIL.ApplyHooks();


        // Menu
        Menu_Hooks.ApplyHooks();
        Menu_HooksIL.ApplyHooks();

        SlideShow_HooksIL.ApplyHooks();


        // Pearlpup
        Pearlpup_Hooks.ApplyHooks();

        PearlpupGraphics_Hooks.ApplyHooks();

        PearlpupIllness_Hooks.ApplyHooks();


        // Player
        Player_Hooks.ApplyHooks();
        Player_HooksIL.ApplyHooks();

        PlayerGraphics_Hooks.ApplyHooks();

        PlayerPossessionFixes_Hooks.ApplyHooks();
        PlayerPossessionFixes_HooksIL.ApplyHooks();

        PlayerPearl_Hooks.ApplyHooks();

        PlayerHeartPearl_Hooks.ApplyHooks();


        // World
        World_Hooks.ApplyHooks();
        World_HooksIL.ApplyHooks();

        Creatures_Hooks.ApplyHooks();
        Creatures_HooksIL.ApplyHooks();

        CustomPearls_Hooks.ApplyHooks();

        SLOracle_Hooks.ApplyHooks();

        SSOracle_Hooks.ApplyHooks();

        SSOracleConversation_Hooks.ApplyHooks();
        SSOracleConversation_HooksIL.ApplyHooks();

        SSOraclePearls_Hooks.ApplyHooks();
        SSOraclePearls_HooksIL.ApplyHooks();
    }


    private static void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        try
        {
            ModOptions.RegisterOI();

            if (IsInit)
            {
                return;
            }

            IsInit = true;


            // Init Info
            var mod = ModManager.ActiveMods.FirstOrDefault(mod => mod.id == Plugin.MOD_ID);

            if (mod is null)
            {
                Plugin.Logger.LogError($"Failed to initialize: ID '{Plugin.MOD_ID}' wasn't found in the active mods list!");
                return;
            }

            Plugin.MOD_NAME = mod.name;
            Plugin.VERSION = mod.version;
            Plugin.AUTHORS = mod.authors;


            // Init
            Enums.InitEnums();

            AssetLoader.LoadAssets();

            ModCompat_Helpers.InitModCompat();

            ApplyHooks();


            // Startup Log
            var initMessage = $"PEARLCAT SAYS HELLO FROM INIT! (VERSION: {Plugin.VERSION})";

            Debug.Log(initMessage);

            Plugin.Logger.LogInfo("START OF BEPINEX LOG");
            Plugin.Logger.LogInfo(initMessage);
        }
        catch (Exception e)
        {
            e.LogHookException();
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
            PearlEffectManager.RegisterEffects();
        }
        catch (Exception e)
        {
            e.LogHookException();
        }
        finally
        {
            orig(self);
        }
    }


    // These are only here for backwards compatability - mods rely on these methods that have now been renamed or moved

    // Rotund World
    [UsedImplicitly]
    public static bool TryGetPearlcatModule(Player player, out PlayerModule playerModule)
    {
        return player.TryGetPearlcatModule(out playerModule);
    }

    // Gate Scanner
    [UsedImplicitly]
    public static bool IsPlayerObject(AbstractPhysicalObject abstractPhysicalObject)
    {
        return abstractPhysicalObject.IsPlayerPearl();
    }

    // Pups+
    [UsedImplicitly]
    public static bool IsPearlpup(Player player)
    {
        return player.abstractCreature.IsPearlpup();
    }

    // PupBase
    [UsedImplicitly]
    public static bool IsPearlpup(AbstractCreature abstractCreature)
    {
        return abstractCreature.IsPearlpup();
    }
}
