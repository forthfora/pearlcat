using BepInEx;
using BepInEx.Logging;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Security;
using System.Linq;
using System;
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

[BepInPlugin(MOD_ID, MOD_ID, "1.3.4")]
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
            LogPearlcatDebugInfo();
        }
    }

    public static void LogPearlcatDebugInfo()
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
                $"CRS VERSION: {ModManager.ActiveMods.FirstOrDefault(x => x.id == "crs")?.version ?? "NOT FOUND"}\n" +
                $"MIRA VERSION: {ModManager.ActiveMods.FirstOrDefault(x => x.id == "mira")?.version ?? "NOT FOUND"}\n" +

                "\n" +

                $"CURRENT MAIN LOOP: {rainWorld.processManager?.currentMainLoop?.GetType()}\n" +
                $"MSC ACTIVE: {ModManager.MSC}\n" +
                $"REMIX ACTIVE: {ModManager.MMF}\n" +
                $"JOLLY ACTIVE: {ModManager.JollyCoop}\n" +

                $"\n-------------------\n" +
                $"PEARLCAT MISC PROGRESSION:\n" +
                $"{nameof(SaveMiscProgression.IsNewPearlcatSave)}: {pearlcatMiscProg.IsNewPearlcatSave}\n" +
                $"{nameof(SaveMiscProgression.IsMSCSave)}: {pearlcatMiscProg.IsMSCSave}\n" +

                "\n" +

                $"{nameof(SaveMiscProgression.StoredActivePearl)}: {pearlcatMiscProg.StoredActivePearl?.DataPearlType ?? "None"}\n" +
                $"{nameof(SaveMiscProgression.StoredNonActivePearls)}:\n{string.Join(",\n", pearlcatMiscProg.StoredNonActivePearls.Select(x => x.DataPearlType))}\n" +

                "\n" +

                $"{nameof(SaveMiscProgression.HasPearlpup)}: {pearlcatMiscProg.HasPearlpup}\n" +
                $"{nameof(SaveMiscProgression.HasDeadPearlpup)}: {pearlcatMiscProg.HasDeadPearlpup}\n" +
                $"{nameof(SaveMiscProgression.IsPearlpupSick)}: {pearlcatMiscProg.IsPearlpupSick}\n" +

                "\n" +

                $"{nameof(SaveMiscProgression.HasOEEnding)}: {pearlcatMiscProg.HasOEEnding}\n" +
                $"{nameof(SaveMiscProgression.JustAscended)}: {pearlcatMiscProg.JustAscended}\n" +
                $"{nameof(SaveMiscProgression.Ascended)}: {pearlcatMiscProg.Ascended}\n" +
                $"{nameof(SaveMiscProgression.AscendedWithPup)}: {pearlcatMiscProg.AscendedWithPup}\n" +

                "\n" +

                $"{nameof(SaveMiscProgression.DidHavePearlpup)}: {pearlcatMiscProg.DidHavePearlpup}\n" +
                $"{nameof(SaveMiscProgression.HasTrueEnding)}: {pearlcatMiscProg.HasTrueEnding}\n";


            if (saveState != null && pearlcatMiscWorld != null)
            {
                message +=
                    $"\n-------------------\n" +
                    $"VANILLA SAVE STATE:\n" +
                    $"{nameof(SaveState.cycleNumber)}: {saveState.cycleNumber}\n" +
                    $"{nameof(SaveState.denPosition)}: {saveState.denPosition}\n" +
                    $"{nameof(SaveState.malnourished)}: {saveState.malnourished}\n" +

                    $"\n-------------------\n" +
                    $"PEARLCAT MISC WORLD:\n" +
                    $"{nameof(SaveMiscWorld.Inventory)}:\n{string.Join("\n", pearlcatMiscWorld.Inventory.Select(x => $"{x.Key}:\n  {string.Join("\n  ", x.Value)}"))}\n" +

                    "\n" +

                    $"{nameof(SaveMiscWorld.ActiveObjectIndex)}:\n{string.Join("\n", pearlcatMiscWorld.ActiveObjectIndex)}\n" +


                    "\n" +

                    $"Pearl Spear Count: {pearlcatMiscWorld.PearlSpears.Keys.Count}\n" +

                    "\n" +

                    $"{nameof(SaveMiscWorld.PebblesMeetCount)}: {pearlcatMiscWorld.PebblesMeetCount}\n" +
                    $"{nameof(SaveMiscWorld.PebblesMetSickPup)}: {pearlcatMiscWorld.PebblesMetSickPup}\n" +
                    $"{nameof(SaveMiscWorld.MoonSickPupMeetCount)}: {pearlcatMiscWorld.MoonSickPupMeetCount}\n" +
                    $"{nameof(SaveMiscWorld.PearlIDsBroughtToPebbles)}: {string.Join(", ", pearlcatMiscWorld.PearlIDsBroughtToPebbles.Keys)}\n" +

                    "\n" +

                    $"{nameof(SaveMiscWorld.ShownSpearCreationTutorial)}: {pearlcatMiscWorld.ShownSpearCreationTutorial}\n" +
                    $"{nameof(SaveMiscWorld.ShownFullInventoryTutorial)}: {pearlcatMiscWorld.ShownFullInventoryTutorial}\n" +

                    "\n" +

                    $"{nameof(SaveMiscWorld.PearlpupID)}: {pearlcatMiscWorld.PearlpupID ?? -1}\n" +
                    $"{nameof(SaveMiscWorld.HasPearlpupWithPlayer)}: {pearlcatMiscWorld.HasPearlpupWithPlayer}\n" +

                    "\n" +


                    $"{nameof(SaveMiscWorld.JustBeatAltEnd)}: {pearlcatMiscWorld.JustBeatAltEnd}\n";
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

            Logger.LogWarning("START OF BEPINEX LOG");
            Logger.LogWarning(message);
        }
        catch (Exception e)
        {
            Logger.LogError("PEARLCAT LOG DEBUG INFO ERROR: \n" + e + "\n" + e.StackTrace);
        }
    }
}
