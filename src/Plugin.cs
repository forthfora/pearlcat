﻿using BepInEx;
using BepInEx.Logging;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Security;
using System.Linq;
using System;
using Newtonsoft.Json;
using UnityEngine;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: IgnoresAccessChecksTo("Assembly-CSharp")]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[module: UnverifiableCode]
#pragma warning restore CS0618 // Type or member is obsolete


namespace Pearlcat;

[BepInDependency("slime-cubed.slugbase")] // SlugBase

[BepInDependency("improved-input-config", BepInDependency.DependencyFlags.SoftDependency)] // Improved Input Config
[BepInDependency("lb-fgf-m4r-ik.chatoyant-waterfalls-but-real", BepInDependency.DependencyFlags.SoftDependency)] // Chasing Wind
[BepInDependency("henpemaz.rainmeadow", BepInDependency.DependencyFlags.SoftDependency)] // Rain Meadow

[BepInPlugin(MOD_ID, MOD_ID, "1.4.7")]
public class Plugin : BaseUnityPlugin
{
    public const string MOD_ID = "pearlcat";

    public static string MOD_NAME = "";
    public static string VERSION = "";
    public static string AUTHORS = "";

    public new static ManualLogSource Logger { get; private set; } = null!;

    public void OnEnable()
    {
        Logger = base.Logger;
        Hooks.ApplyInitHooks();
    }

    public void Update()
    {
        // Left Control + Left Shift + L
        // Produces a debug log to the console
        var input = Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.L);

        if (input)
        {
            LogPearlcatDebugInfo(true);
        }
    }

    public static void LogPearlcatDebugInfo(bool isWarning = false)
    {
        try
        {
            var rainWorld = RWCustom.Custom.rainWorld;

            var saveState = (rainWorld.processManager?.currentMainLoop as RainWorldGame)?.GetStorySession?.saveState;

            var pearlcatMiscProg = Utils.MiscProgression;
            var pearlcatMiscWorld = (rainWorld.processManager?.currentMainLoop as RainWorldGame)?.GetMiscWorld();

            var message =
                $"=======================\n" +
                $"PEARLCAT DEBUG INFO LOG\n" +
                $"=======================\n" +

                $"TIME: {DateTime.UtcNow}\n" +
                $"GAME VERSION: {RainWorld.GAME_VERSION_STRING}\n" +

                "\n" +

                $"PEARLCAT VERSION: {VERSION}\n" +
                $"SLUGBASE VERSION: {ModManager.ActiveMods.FirstOrDefault(x => x.id == "slime-cubed.slugbase")?.version ?? "NOT FOUND"}\n" +

                "\n" +

                $"CURRENT MAIN LOOP: {rainWorld.processManager?.currentMainLoop?.GetType()}\n" +
                $"MSC ACTIVE: {ModManager.MSC}\n" +
                $"REMIX ACTIVE: {ModManager.MMF}\n" +
                $"JOLLY ACTIVE: {ModManager.JollyCoop}\n" +

                "\n-------------------\n" +
                "PEARLCAT MISC PROGRESSION:\n" +
                JsonConvert.SerializeObject(pearlcatMiscProg, Formatting.Indented) +
                "\n";


            if (saveState is not null && pearlcatMiscWorld is not null)
            {
                message +=
                    $"\n-------------------\n" +
                    $"VANILLA SAVE STATE:\n" +
                    $"{nameof(SaveState.cycleNumber)}: {saveState.cycleNumber}\n" +
                    $"{nameof(SaveState.denPosition)}: {saveState.denPosition}\n" +
                    $"{nameof(SaveState.malnourished)}: {saveState.malnourished}\n" +

                    $"\n-------------------\n" +
                    $"PEARLCAT MISC WORLD:\n" +
                    JsonConvert.SerializeObject(pearlcatMiscWorld, Formatting.Indented) +
                    "\n";
            }
            else
            {
                message +=
                    $"\n-------------------\n" +
                    $"MISC WORLD NOT FOUND! (not in story session?)\n";
            }

            message +=
                $"\n-------------------\n" +
                $"FULL ACTIVE MODS LIST:\n";

            foreach (var mod in ModManager.ActiveMods)
            {
                var version = mod.version == "" ? "N/A" : $"v{mod.version}";

                message += $"> {mod.id} ({mod.name}) - {version}\n";
            }


            Debug.Log(message);

            if (isWarning)
            {
                Logger.LogWarning(message);
            }
            else
            {
                Logger.LogInfo(message);
            }
        }
        catch (Exception e)
        {
            Logger.LogError($"PEARLCAT LOG DEBUG INFO ERROR:\n{e}");
        }
    }
}
