using BepInEx.Logging;
using MonoMod.Cil;
using On.Music;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using UnityEngine;

namespace TheSacrifice
{
    internal static partial class Hooks
    {
        public static void ApplyHooks()
        {
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            On.RainWorld.OnModsDisabled += RainWorld_OnModsDisabled;

            On.Menu.MenuScene.ctor += MenuScene_ctor;
            On.Menu.SlugcatSelectMenu.Update += SlugcatSelectMenu_Update;

            ApplySaveLoadHooks();

            ApplyPlayerHooks();
            ApplyPlayerGraphicsHooks();
            
            ApplyMenuHooks();
            ApplyOracleHooks();
        }

        private static bool isInit = false;

        private static void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);

            if (isInit) return;
            isInit = true;

            MachineConnector.SetRegisteredOI(Plugin.MOD_ID, Options.instance);

            Enums.RegisterEnums();
            AssetLoader.LoadAssets();
        }

        private static void RainWorld_OnModsDisabled(On.RainWorld.orig_OnModsDisabled orig, RainWorld self, ModManager.Mod[] newlyDisabledMods)
        {
            orig(self, newlyDisabledMods);

            Enums.UnregisterEnums();
        }
    }
}
