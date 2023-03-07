using BepInEx.Logging;
using MonoMod.Cil;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using UnityEngine;
using static SlugcatStats;

namespace TheSacrifice
{
    internal static partial class Hooks
    {
        public static void ApplyHooks()
        {
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            On.RainWorld.OnModsDisabled += RainWorld_OnModsDisabled;

            On.SaveState.SaveToString += SaveState_SaveToString;
            On.SaveState.LoadGame += SaveState_LoadGame;
        }

        private static void SaveState_LoadGame(On.SaveState.orig_LoadGame orig, SaveState self, string str, RainWorldGame game)
        {
            orig(self, str, game);

            self.stor
        }

        private static string SaveState_SaveToString(On.SaveState.orig_SaveToString orig, SaveState self)
        {
            throw new NotImplementedException();
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
