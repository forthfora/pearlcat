using System;
using static DataPearl.AbstractDataPearl;

namespace Pearlcat;

public static partial class Hooks
{
    public static void ApplySaveDataHooks()
    {
        On.SaveState.SaveToString += SaveState_SaveToString;
        On.SaveState.LoadGame += SaveState_LoadGame;

        On.PlayerProgression.SaveToDisk += PlayerProgression_SaveToDisk;

        //On.PlayerProgression.WipeSaveState += PlayerProgression_WipeSaveState;
        On.PlayerProgression.WipeAll += PlayerProgression_WipeAll;
    }
    

    // Shouldn't be necessary
    //private static void PlayerProgression_WipeSaveState(On.PlayerProgression.orig_WipeSaveState orig, PlayerProgression self, SlugcatStats.Name saveStateNumber)
    //{
    //    var miscProg = Utils.GetMiscProgression();

    //    if (saveStateNumber == Enums.Pearlcat)
    //    {
    //        miscProg.ResetSave();
    //    }

    //    orig(self, saveStateNumber);
    //}

    private static void PlayerProgression_WipeAll(On.PlayerProgression.orig_WipeAll orig, PlayerProgression self)
    {
        var miscProg = Utils.GetMiscProgression();

        miscProg.ResetSave();

        orig(self);
    }


    private static bool PlayerProgression_SaveToDisk(On.PlayerProgression.orig_SaveToDisk orig, PlayerProgression self, bool saveCurrentState, bool saveMaps, bool saveMiscProg)
    {
        try
        {
            var miscWorld = self.currentSaveState?.miscWorldSaveData?.GetMiscWorld();
            var miscProg = Utils.GetMiscProgression();

            if (miscWorld != null && saveCurrentState && self.currentSaveState?.saveStateNumber == Enums.Pearlcat)
            {
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
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("PEARLCAT SAVE TO DISK EXCEPTION:\n" + e);
        }

        return orig(self, saveCurrentState, saveMaps, saveMiscProg);
    }

    private static string SaveState_SaveToString(On.SaveState.orig_SaveToString orig, SaveState self)
    {
        try
        {
            var miscWorld = self.miscWorldSaveData.GetMiscWorld();
            var miscProg = self.progression.miscProgressionData.GetMiscProgression();
            
            if (self.saveStateNumber == Enums.Pearlcat)
            {
                miscProg.IsNewPearlcatSave = false;
                miscProg.Ascended = self.deathPersistentSaveData.ascended;

                if (miscProg.HasPearlpup)
                {
                    miscProg.DidHavePearlpup = true;
                }

                if (miscWorld.HasPearlpupWithPlayer && miscProg.IsPearlpupSick && !miscProg.JustAscended)
                {
                    SlugBase.Assets.CustomScene.SetSelectMenuScene(self, Enums.Scenes.Slugcat_Pearlcat_Sick);
                }
                else if (self.deathPersistentSaveData.ascended)
                {
                    SlugBase.Assets.CustomScene.SetSelectMenuScene(self, Enums.Scenes.Slugcat_Pearlcat_Ascended);
                }
                else
                {
                    SlugBase.Assets.CustomScene.SetSelectMenuScene(self, Enums.Scenes.Slugcat_Pearlcat);
                }

                if (miscWorld.CurrentDream != null && !miscWorld.PreviousDreams.Contains(miscWorld.CurrentDream))
                {
                    miscWorld.PreviousDreams.Add(miscWorld.CurrentDream);
                    miscWorld.CurrentDream = null;
                }

                if (!miscWorld.HasPearlpupWithPlayerDeadOrAlive)
                {
                    miscWorld.PearlpupID = null;
                }
            }
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("PEARLCAT SAVE TO STRING EXCEPTION:\n" + e);
        }

        return orig(self);
    }

    private static void SaveState_LoadGame(On.SaveState.orig_LoadGame orig, SaveState self, string str, RainWorldGame game)
    {
        orig(self, str, game);

        var miscWorld = self.miscWorldSaveData.GetMiscWorld();
        var miscProg = self.progression.miscProgressionData.GetMiscProgression();

        if (self.saveStateNumber != Enums.Pearlcat) return;


        miscProg.JustAscended = false;
        miscWorld.JustMiraSkipped = false;

        if (self.cycleNumber == 0)
        {
            miscProg.ResetSave();
        }

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
}
