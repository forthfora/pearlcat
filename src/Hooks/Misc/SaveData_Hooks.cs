using System;
using static DataPearl.AbstractDataPearl;

namespace Pearlcat;

public static class SaveData_Hooks
{
    public static void ApplyHooks()
    {
        On.WinState.CycleCompleted += WinState_CycleCompleted;

        On.SaveState.LoadGame += SaveState_LoadGame;

        On.PlayerProgression.WipeAll += PlayerProgression_WipeAll;
    }


    // Reset misc progression when the slot is reset
    private static void PlayerProgression_WipeAll(On.PlayerProgression.orig_WipeAll orig, PlayerProgression self)
    {
        var miscProg = Utils.GetMiscProgression();

        miscProg.ResetSave();

        orig(self);
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

        if (miscWorld == null)
        {
            return;
        }


        // Meta
        miscProg.IsMSCSave = ModManager.MSC;

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
            for (var i = 0; i < inventory.Count; i++)
            {
                var item = inventory[i];
                var split = item.Split(["<oA>"], StringSplitOptions.None);

                if (split.Length < 5)
                {
                    continue;
                }

                var potentialType = split[5];


                if (!ExtEnumBase.TryParse(typeof(DataPearlType), potentialType, false, out var type))
                {
                    continue;
                }

                if (type is not DataPearlType dataPearlType)
                {
                    continue;
                }


                var potentialPebblesColor = 0;

                if (dataPearlType == DataPearlType.PebblesPearl && split.Length >= 6 && int.TryParse(split[6], out var result))
                {
                    potentialPebblesColor = result;
                }


                if (dataPearlType == Enums.Pearls.Heart_Pearlpup)
                {
                    if (i == activeIndex)
                    {
                        heartIsActive = true;
                    }
                }
                else if (i == activeIndex)
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


        if (self.cycleNumber == 0)
        {
            SlugBase.Assets.CustomScene.SetSelectMenuScene(self, Enums.Scenes.Slugcat_Pearlcat);
            
            if (miscProg.IsSecretEnabled)
            {
                miscProg.ResetSave();

                miscProg.IsSecretEnabled = true;


                self.StartFromMira();
                miscWorld.JustMiraSkipped = false;

                self.GiveTrueEnding();
            }
            else if (miscProg.IsMiraSkipEnabled)
            {
                miscProg.ResetSave();

                miscProg.IsMiraSkipEnabled = true;
            }
            else
            {
                miscProg.ResetSave();
            }
        }


        if (miscProg.IsMiraSkipEnabled)
        {
            self.StartFromMira();

            miscProg.IsMiraSkipEnabled = false;
        }
    }
}
