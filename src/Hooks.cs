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
    internal static class Hooks
    {
        public static void ApplyHooks()
        {
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            On.RainWorld.OnModsDisabled += RainWorld_OnModsDisabled;

            On.Player.ctor += Player_ctor;
            On.Player.Destroy += Player_Destroy;
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

        private static void Player_Destroy(On.Player.orig_Destroy orig, Player self)
        {
            orig(self);

            for (int i = customSlugcats.Count; i > 0; i--)
            {
                if (customSlugcats[i].player == self)
                {
                    customSlugcats.RemoveAt(i);
                }
            }
        }

        public static List<CustomSlugcat> customSlugcats = new List<CustomSlugcat>();

        private static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);

            if (self.SlugCatClass.value != Plugin.SLUGCAT_NAME) return;

            CustomSlugcat customSlugcat = new CustomSlugcat(self);
            customSlugcats.Add(customSlugcat);
        }
    }
}
