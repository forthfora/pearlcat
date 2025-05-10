global using UnityEngine;
global using Random = UnityEngine.Random;

using BepInEx;
using BepInEx.Logging;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Security;
using BepInEx.Bootstrap;
using Newtonsoft.Json;

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

[BepInPlugin(MOD_ID, MOD_ID, "1.5.1")]
public class Plugin : BaseUnityPlugin
{
    public const string MOD_ID = "pearlcat";

    public static string ModName { get; set; } = "";
    public static string Version { get; set; }= "";
    public static string Author { get; set; } = "";

    public new static ManualLogSource Logger { get; private set; } = null!;

    public void OnEnable()
    {
        Logger = base.Logger;
        Hooks.ApplyInitHooks();

        if (Chainloader.PluginInfos.Keys.Any(x => x == "com.dual.improved-input-config"))
        {
            try
            {
                // Needs a buffer method as there are statics in the IICCompat class which reference the DLL
                ModCompat_Helpers.InitIICCompat();
            }
            catch (Exception e)
            {
                Logger.LogError($"Error initializing Improved Input Config compat:\n{e}");
            }
        }
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

                $"PEARLCAT VERSION: {Version}\n" +
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
