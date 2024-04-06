using System;
using static DataPearl.AbstractDataPearl;

namespace Pearlcat;

public static partial class Hooks
{
    public static void ApplySaveDataHooks()
    {
        On.WinState.CycleCompleted += WinState_CycleCompleted;

        On.SaveState.LoadGame += SaveState_LoadGame;

        On.PlayerProgression.WipeSaveState += PlayerProgression_WipeSaveState;
        On.PlayerProgression.WipeAll += PlayerProgression_WipeAll;
    }

    // Assess and update save data at the end of the cycle
    private static void WinState_CycleCompleted(On.WinState.orig_CycleCompleted orig, WinState self, RainWorldGame game)
    {
        if (game.IsPearlcatStory())
        {
            try
            {
                UpdateSaveAfterCycle(game);
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError("ERROR UPDATING SAVE ON CYCLE COMPLETION:\n" + e);
            }
        }

        orig(self, game);
    }

    private static void UpdateSaveAfterCycle(RainWorldGame game)
    {
        var miscWorld = game.GetMiscWorld();
        var miscProg = Utils.GetMiscProgression();

        var saveState = game.GetStorySession.saveState;

        if (miscWorld == null) return;


        // Meta
        miscProg.IsNewPearlcatSave = false;
        miscProg.Ascended = saveState.deathPersistentSaveData.ascended;


        // Pearlpup
        if (miscProg.HasPearlpup)
        {
            miscProg.DidHavePearlpup = true;
        }

        if (!miscWorld.HasPearlpupWithPlayerDeadOrAlive)
        {
            miscWorld.PearlpupID = null;
        }


        // Menu Scene
        if (miscWorld.HasPearlpupWithPlayer && miscProg.IsPearlpupSick && !miscProg.JustAscended)
        {
            SlugBase.Assets.CustomScene.SetSelectMenuScene(saveState, Enums.Scenes.Slugcat_Pearlcat_Sick);
        }
        else if (saveState.deathPersistentSaveData.ascended)
        {
            SlugBase.Assets.CustomScene.SetSelectMenuScene(saveState, Enums.Scenes.Slugcat_Pearlcat_Ascended);
        }
        else
        {
            SlugBase.Assets.CustomScene.SetSelectMenuScene(saveState, Enums.Scenes.Slugcat_Pearlcat);
        }


        // Dreams
        if (miscWorld.CurrentDream != null && !miscWorld.PreviousDreams.Contains(miscWorld.CurrentDream))
        {
            miscWorld.PreviousDreams.Add(miscWorld.CurrentDream);
            miscWorld.CurrentDream = null;
        }


        // Pearl Colors
        miscProg.StoredPearlColors.Clear();
        miscProg.ActivePearlColor = null;

        var heartIsActive = false;

        if (miscWorld.Inventory.TryGetValue(0, out var inventory) && miscWorld.ActiveObjectIndex.TryGetValue(0, out var activeIndex))
        {
            for (int i = 0; i < inventory.Count; i++)
            {
                var item = inventory[i];
                var split = item.Split(new string[] { "<oA>" }, StringSplitOptions.None);

                if (split.Length < 5) continue;

                var potentialType = split[5];


                if (!ExtEnumBase.TryParse(typeof(DataPearlType), potentialType, false, out var type)) continue;

                if (type is not DataPearlType dataPearlType) continue;


                if (dataPearlType == Enums.Pearls.Heart_Pearlpup)
                {
                    heartIsActive = true;
                    continue;
                }


                var potentialPebblesColor = 0;

                if (dataPearlType == DataPearlType.PebblesPearl && split.Length >= 6 && int.TryParse(split[6], out var result))
                {
                    potentialPebblesColor = result;
                }

                if (i == activeIndex)
                {
                    miscProg.ActivePearlColor = dataPearlType.GetDataPearlColor(potentialPebblesColor);
                }
                else
                {
                    miscProg.StoredPearlColors.Add(dataPearlType.GetDataPearlColor(potentialPebblesColor));
                }
            }
        }

        if (heartIsActive && miscProg.StoredPearlColors.Count > 0)
        {
            miscProg.ActivePearlColor = miscProg.StoredPearlColors[0];
            miscProg.StoredPearlColors.RemoveAt(0);
        }
    }


    // Assess and update save data just before a cycle
    private static void SaveState_LoadGame(On.SaveState.orig_LoadGame orig, SaveState self, string str, RainWorldGame game)
    {
        orig(self, str, game);

        if (self.saveStateNumber == Enums.Pearlcat)
        {
            try
            {
                UpdateSaveBeforeCycle(self);
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError("ERROR UPDATING SAVE BEFORE CYCLE START:\n" + e);
            }
        }
    }

    private static void UpdateSaveBeforeCycle(SaveState self)
    {
        var miscWorld = self.miscWorldSaveData.GetMiscWorld();
        var miscProg = Utils.GetMiscProgression();

        // Meta
        miscProg.JustAscended = false;

        miscWorld.JustBeatAltEnd = false;
        miscWorld.JustMiraSkipped = false;


        if (miscProg.IsMiraSkipEnabled)
        {
            self.StartFromMira();

            miscProg.IsMiraSkipEnabled = false;
        }
        else if (miscProg.IsSecretEnabled)
        {
            self.StartFromMira();

            miscWorld.JustMiraSkipped = false;

            self.GiveTrueEnding();

            miscProg.IsSecretEnabled = false;
        }
    }


    private static void PlayerProgression_WipeSaveState(On.PlayerProgression.orig_WipeSaveState orig, PlayerProgression self, SlugcatStats.Name saveStateNumber)
    {
        var miscProg = Utils.GetMiscProgression();

        if (saveStateNumber == Enums.Pearlcat)
        {
            miscProg.ResetSave();
        }

        orig(self, saveStateNumber);
    }

    private static void PlayerProgression_WipeAll(On.PlayerProgression.orig_WipeAll orig, PlayerProgression self)
    {
        var miscProg = Utils.GetMiscProgression();

        miscProg.ResetSave();

        orig(self);
    }
}
