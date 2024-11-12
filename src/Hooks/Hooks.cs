using System;
using System.Linq;
using UnityEngine;

namespace Pearlcat;

public static partial class Hooks
{
    public static void ApplyHooks()
    {
        // Misc
        Hooks_SaveData.ApplyHooks_SaveData();
        Hooks_Menu.ApplyHooks_Menu();
        Hooks_SlideShow.ApplyHooks_SlideShow();
        Hooks_Fixes.ApplyHooks_Fixes();

        Hooks_Player.ApplyHooks_Player();

        Hooks_PearlpupNPC.ApplyHooks_PearlpupNPC();

        Hooks_World.ApplyHooks_World();
    }


    public static bool IsInit { get; private set; }

    public static void ApplyInit()
    {
        On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        On.RainWorld.PostModsInit += RainWorld_PostModsInit;
    }

    private static void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        try
        {
            ModOptions.RegisterOI();

            if (IsInit) return;
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


            // Init Enums
            _ = Enums.Pearlcat;

            _ = Enums.SSOracle.Pearlcat_SSActionGeneral;
            _ = Enums.Pearls.RM_Pearlcat;
            _ = Enums.Sounds.Pearlcat_PearlScroll;

            _ = Enums.Scenes.Slugcat_Pearlcat;

            Enums.Dreams.RegisterDreams();


            // Init Assets
            Utils.LoadAssets();


            // Init Soft Dependencies
            if (Utils.IsModEnabled_ImprovedInputConfig)
            {
                Hooks_Input.InitIICKeybinds();
            }

            if (Utils.IsModEnabled_ChasingWind)
            {
                Utils.InitCWIntegration();
            }


            // Apply Hooks
            ApplyHooks();


            // Startup Log
            var initMessage = $"PEARLCAT SAYS HELLO FROM INIT! (VERSION: {Plugin.VERSION})";

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
            PearlEffectManager.RegisterEffects();
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



    // There are only here for backwards compatability, I'm pretty sure another mod used this at one point or another

    // Gate Scanner (?)
    public static bool TryGetPearlcatModule(Player player, out PlayerModule playerModule) => player.TryGetPearlcatModule(out playerModule);

    // Pups+
    public static bool IsPearlpup(this Player player) => player.abstractCreature.IsPearlpup();
}
