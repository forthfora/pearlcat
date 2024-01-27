using BepInEx;
using BepInEx.Logging;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Security;
using static Pearlcat.Hooks;
using System.Linq;
using System;
using UnityEngine;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: IgnoresAccessChecksTo("Assembly-CSharp")]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[module: UnverifiableCode]
#pragma warning restore CS0618 // Type or member is obsolete


namespace Pearlcat;

[BepInDependency("slime-cubed.slugbase")]
[BepInDependency("com.rainworldgame.garrakx.crs.mod")]
[BepInDependency("improved-input-config", BepInDependency.DependencyFlags.SoftDependency)]
[BepInPlugin(MOD_ID, MOD_ID, "1.0.0")]

public class Plugin : BaseUnityPlugin
{
    public const string MOD_ID = "pearlcat";
    public const string SLUGCAT_ID = "Pearlcat";

    public static string MOD_NAME = "";
    public static string VERSION = "";
    public static string AUTHORS = "";

    public static new ManualLogSource Logger { get; private set; } = null!;

    public void OnEnable()
    {
        Logger = base.Logger;
        ApplyInit();
    }

    public void Update()
    {
        var input = Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.L);

        if (input)
        {
            LogPearlcatDebugInfo(RWCustom.Custom.rainWorld);
        }
    }

    public void LogPearlcatDebugInfo(RainWorld rainWorld)
    {
        try
        {
            var miscProg = rainWorld.progression.miscProgressionData;
            var saveState = (rainWorld.processManager?.currentMainLoop as RainWorldGame)?.GetStorySession?.saveState;

            var pearlcatMiscProg = Utils.GetMiscProgression();
            var pearlcatMiscWorld = (rainWorld.processManager?.currentMainLoop as RainWorldGame)?.GetMiscWorld();

            var message =
                $"!PEARLCAT DEBUG INFO LOG!\n" +

                $"TIME: {DateTime.UtcNow}\n" +
                $"GAME VERSION: {RainWorld.GAME_VERSION_STRING}\n" +

                "\n" +

                $"PEARLCAT VERSION: {Plugin.VERSION}\n" +
                $"SLUGBASE VERSION: {ModManager.ActiveMods.FirstOrDefault(x => x.id == "slime-cubed.slugbase")?.version ?? "NOT FOUND"}\n" +
                $"CRS VERSION: {ModManager.ActiveMods.FirstOrDefault(x => x.id == "crs")?.version ?? "NOT FOUND"}\n" +
                $"MERGEFIX VERSION: {ModManager.ActiveMods.FirstOrDefault(x => x.id == "bro.mergefix")?.version ?? "NOT FOUND"}\n" +

                "\n" +

                $"CURRENT MAIN LOOP: {rainWorld.processManager?.currentMainLoop?.GetType()}\n" +
                $"MSC: {ModManager.MSC}\n" +
                $"REMIX: {ModManager.MMF}\n" +
                $"JOLLY: {ModManager.JollyCoop}\n" +

                $"\n-------------------\n" +
                $"PEARLCAT MISC PROGRESSION:\n" +
                $"{nameof(SaveMiscProgression.IsNewPearlcatSave)}: {pearlcatMiscProg.IsNewPearlcatSave}\n" +
                $"{nameof(SaveMiscProgression.IsMSCSave)}: {pearlcatMiscProg.IsMSCSave}\n" +

                "\n" +

                $"{nameof(SaveMiscProgression.ActivePearlColor)}: {pearlcatMiscProg.ActivePearlColor}\n" +
                $"{nameof(SaveMiscProgression.StoredPearlColors)}:\n{string.Join(",\n", pearlcatMiscProg.StoredPearlColors)}\n" +

                "\n" +

                $"{nameof(SaveMiscProgression.HasPearlpup)}: {pearlcatMiscProg.HasPearlpup}\n" +
                $"{nameof(SaveMiscProgression.IsPearlpupSick)}: {pearlcatMiscProg.IsPearlpupSick}\n" +

                "\n" +

                $"{nameof(SaveMiscProgression.HasOEEnding)}: {pearlcatMiscProg.HasOEEnding}\n" +
                $"{nameof(SaveMiscProgression.JustAscended)}: {pearlcatMiscProg.JustAscended}\n" +
                $"{nameof(SaveMiscProgression.Ascended)}: {pearlcatMiscProg.Ascended}\n" +


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
                    $"{nameof(SaveMiscWorld.IsNewGame)}: {pearlcatMiscWorld.IsNewGame}\n" +
                    $"{nameof(SaveMiscWorld.IsPearlcatStory)}: {pearlcatMiscWorld.IsPearlcatStory}\n" +

                    "\n" +

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

            Debug.Log(message);

            Logger.LogWarning("START OF BEPINEX LOG");
            Logger.LogWarning(message);
        }
        catch (Exception e)
        {
            Logger.LogError("PEARLCAT LOG DEBUG INFO ERROR: \n" + e);
        }
    }
}
