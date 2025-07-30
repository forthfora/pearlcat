﻿using static DataPearl.AbstractDataPearl;

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
        var miscProg = Utils.MiscProgression;

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
                Plugin.Logger.LogError($"ERROR UPDATING SAVE DATA ON CYCLE COMPLETION:\n{e}");
            }
        }

        orig(self, game);
    }

    private static void UpdateSaveAfterCycle(RainWorldGame game)
    {
        var miscWorld = game.GetMiscWorld();

        if (miscWorld is null)
        {
            return;
        }

        var miscProg = Utils.MiscProgression;
        var saveState = game.GetStorySession.saveState;


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
        if (miscWorld.CurrentDream is not null && !miscWorld.PreviousDreams.Contains(miscWorld.CurrentDream))
        {
            miscWorld.PreviousDreams.Add(miscWorld.CurrentDream);
            miscWorld.CurrentDream = null;
        }


        // Pearl Colors
        miscProg.StoredNonActivePearls.Clear();
        miscProg.StoredActivePearl = null;

        var firstPearlcat = game.GetFirstPearlcat();
        var inventoryPearlcatId = (firstPearlcat?.realizedCreature as Player)?.playerState.playerNumber ?? -1;

        if (ModCompat_Helpers.RainMeadow_IsOnline)
        {
            // Get the pearlcat we're using if we're online
            var myPearlcat = game.GetAllPearlcats().FirstOrDefault(ModCompat_Helpers.RainMeadow_IsMine);

            if (myPearlcat is not null)
            {
                inventoryPearlcatId = ModCompat_Helpers.RainMeadow_GetOwnerIdOrNull(myPearlcat) ?? -1;
            }
            else
            {
                inventoryPearlcatId = -1;
            }
        }

        if (miscWorld.Inventory.TryGetValue(inventoryPearlcatId, out var inventory) && miscWorld.ActiveObjectIndex.TryGetValue(inventoryPearlcatId, out var activeIndex))
        {
            void AddInvalidPearl(int i)
            {
                if (i == activeIndex)
                {
                    miscProg.StoredActivePearl = new()
                    {
                        DataPearlType = "Pearlcat_Invalid",
                    };
                }
                else
                {
                    var menuPearlData = new SaveMiscProgression.StoredPearlData()
                    {
                        DataPearlType = "Pearlcat_Invalid",
                    };

                    miscProg.StoredNonActivePearls.Add(menuPearlData);
                }
            }

            for (var i = 0; i < inventory.Count; i++)
            {
                var item = inventory[i];
                var split = item.Split(["<oA>"], StringSplitOptions.None);

                if (split.Length < 5)
                {
                    AddInvalidPearl(i);
                    continue;
                }

                var possibleType = split[5];

                if (!ExtEnumBase.TryParse(typeof(DataPearlType), possibleType, false, out var type))
                {
                    AddInvalidPearl(i);
                    continue;
                }

                if (type is not DataPearlType dataPearlType)
                {
                    AddInvalidPearl(i);
                    continue;
                }


                var pebblesPearlType = 0;

                if (dataPearlType == DataPearlType.PebblesPearl && split.Length >= 6 && int.TryParse(split[6], out var result))
                {
                    pebblesPearlType = result;
                }


                if (i == activeIndex)
                {
                    miscProg.StoredActivePearl = new()
                    {
                        DataPearlType = dataPearlType.value,
                        PebblesPearlType = pebblesPearlType,
                    };
                }
                else
                {
                    var menuPearlData = new SaveMiscProgression.StoredPearlData()
                    {
                        DataPearlType = dataPearlType.value,
                        PebblesPearlType = pebblesPearlType,
                    };

                    miscProg.StoredNonActivePearls.Add(menuPearlData);
                }
            }
        }

        // Heart is displayed in its own unique way, so don't show it as the active pearl
        if (miscProg.StoredActivePearl?.DataPearlType == Enums.Pearls.Heart_Pearlpup.value && miscProg.StoredNonActivePearls.Count > 0)
        {
            miscProg.StoredActivePearl = miscProg.StoredNonActivePearls[0];
            miscProg.StoredNonActivePearls.RemoveAt(0);
        }
    }


    // Assess and update save data just before a cycle
    private static void SaveState_LoadGame(On.SaveState.orig_LoadGame orig, SaveState self, string str, RainWorldGame game)
    {
        orig(self, str, game);

        if (self.saveStateNumber != Enums.Pearlcat)
        {
            return;
        }

        try
        {
            UpdateSaveBeforeCycle(self);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"ERROR UPDATING SAVE BEFORE CYCLE START:\n{e}");
        }
    }

    private static void UpdateSaveBeforeCycle(SaveState self)
    {
        var miscWorld = self.miscWorldSaveData.GetMiscWorld();
        var miscProg = Utils.MiscProgression;

        // Meta
        miscProg.JustAscended = false;
        miscWorld.JustBeatAltEnd = false;
        miscWorld.JustStorySkipped = false;


        // New save
        if (self.cycleNumber == 0)
        {
            SlugBase.Assets.CustomScene.SetSelectMenuScene(self, Enums.Scenes.Slugcat_Pearlcat);
            
            if (miscProg.IsSecretEnabled)
            {
                miscProg.ResetSave();

                miscProg.IsSecretEnabled = true;


                self.SkipStory();

                miscWorld.JustStorySkipped = false;


                self.GiveTrueEnding();
            }
            else if (miscProg.IsStorySkipEnabled)
            {
                miscProg.ResetSave();

                miscProg.IsStorySkipEnabled = true;
            }
            else
            {
                miscProg.ResetSave();
            }
        }

        if (miscProg.IsStorySkipEnabled)
        {
            self.SkipStory();

            miscProg.IsStorySkipEnabled = false;
        }
    }
}
