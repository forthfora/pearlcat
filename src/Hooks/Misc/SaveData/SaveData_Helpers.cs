namespace Pearlcat;

public static class SaveData_Helpers
{
    public static void GiveTrueEnding(this SaveState saveState)
    {
        if (saveState.saveStateNumber != Enums.Pearlcat)
        {
            return;
        }

        var miscProg = Utils.MiscProgression;
        var miscWorld = saveState.miscWorldSaveData.GetMiscWorld();

        if (miscWorld == null)
        {
            return;
        }


        miscProg.HasTrueEnding = true;
        miscProg.IsPearlpupSick = false;

        miscWorld.PebblesMeetCount = 0;

        SlugBase.Assets.CustomScene.SetSelectMenuScene(saveState, Enums.Scenes.Slugcat_Pearlcat);

        // So the tutorial scripts can be added again
        foreach (var regionState in saveState.regionStates)
        {
            regionState?.roomsVisited?.RemoveAll(x => x?.StartsWith("T1_") == true);
        }
    }

    public static void StartFromMira(this SaveState saveState)
    {
        if (saveState.saveStateNumber != Enums.Pearlcat)
        {
            return;
        }

        var miscProg = Utils.MiscProgression;
        var miscWorld = saveState.miscWorldSaveData.GetMiscWorld();
        var baseMiscWorld = saveState.miscWorldSaveData;

        if (miscWorld == null)
        {
            return;
        }


        miscProg.IsPearlpupSick = true;
        miscProg.HasOEEnding = true;
        miscProg.DidHavePearlpup = true;

        miscWorld.ShownFullInventoryTutorial = true;
        miscWorld.ShownSpearCreationTutorial = true;

        miscWorld.PebblesMeetCount = 3;
        miscWorld.MoonSickPupMeetCount = 1;
        miscWorld.PebblesMetSickPup = true;

        baseMiscWorld.SLOracleState.playerEncountersWithMark = 0;
        baseMiscWorld.SLOracleState.playerEncounters = 1;

        miscWorld.JustMiraSkipped = true;

        SlugBase.Assets.CustomScene.SetSelectMenuScene(saveState, Enums.Scenes.Slugcat_Pearlcat_Sick);
    }
}
