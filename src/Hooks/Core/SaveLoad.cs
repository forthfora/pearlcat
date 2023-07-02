using SlugBase.SaveData;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static DataPearl.AbstractDataPearl;

namespace Pearlcat;

public static partial class Hooks
{
    [Serializable]
    public class PearlcatMiscWorld
    {
        public bool IsNewGame { get; set; } = true;

        public Dictionary<int, List<string>> Inventory { get; set; } = new();
        public Dictionary<int, int?> ActiveObjectIndex { get; set; } = new();
    }

    [Serializable]
    public class PearlcatDeathPersistent
    {
    }

    [Serializable]
    public class PearlcatMiscProgression
    {
        public bool IsNewSave { get; set; } = true;

        public List<DataPearlType> StoredPearlTypes { get; set; } = new();
        public DataPearlType? ActivePearlType { get; set; }
    }


    public static readonly ConditionalWeakTable<MiscWorldSaveData, PearlcatMiscWorld> MiscWorldData = new();
    public static readonly ConditionalWeakTable<DeathPersistentSaveData, PearlcatDeathPersistent> DeathPersistentData = new();
    public static readonly ConditionalWeakTable<PlayerProgression.MiscProgressionData, PearlcatMiscProgression> MiscProgressionData = new();

    public static PearlcatMiscWorld GetMiscWorld(this RainWorldGame game) => GetMiscWorld(game.GetStorySession.saveState.miscWorldSaveData);
    public static PearlcatDeathPersistent GetDeathPersistent(this RainWorldGame game) => GetDeathPersistent(game.GetStorySession.saveState.deathPersistentSaveData);
    public static PearlcatMiscProgression GetMiscProgression(this RainWorldGame game) => GetMiscProgression(game.GetStorySession.saveState.progression.miscProgressionData);

    public static PearlcatMiscWorld GetMiscWorld(this MiscWorldSaveData miscWorld)
    {
        if (!MiscWorldData.TryGetValue(miscWorld, out var saveData))
        {
            saveData = new();
            MiscWorldData.Add(miscWorld, saveData);
        }
        return saveData;
    }
    public static PearlcatDeathPersistent GetDeathPersistent(this DeathPersistentSaveData deathPersistent)
    {
        if (!DeathPersistentData.TryGetValue(deathPersistent, out var saveData))
        {
            saveData = new();
            DeathPersistentData.Add(deathPersistent, saveData);
        }
        return saveData;
    }
    public static PearlcatMiscProgression GetMiscProgression(this PlayerProgression.MiscProgressionData miscProg)
    {
        if (!MiscProgressionData.TryGetValue(miscProg, out var saveData))
        {
            saveData = new();
            MiscProgressionData.Add(miscProg, saveData);
        }
        return saveData;
    }


    public static void ApplySaveDataHooks()
    {
        On.SaveState.SaveToString += SaveState_SaveToString;
        On.SaveState.LoadGame += SaveState_LoadGame;
    }

    private static string SaveState_SaveToString(On.SaveState.orig_SaveToString orig, SaveState self)
    {
        var miscWorld = self.miscWorldSaveData.GetMiscWorld();
        miscWorld.IsNewGame = false;

        self.miscWorldSaveData.GetSlugBaseData().Set(Plugin.MOD_ID, self.miscWorldSaveData.GetMiscWorld());


        var miscProg = self.progression.miscProgressionData.GetMiscProgression();
        miscProg.IsNewSave = false;

        self.progression.miscProgressionData.GetSlugBaseData().Set(Plugin.MOD_ID, miscProg);
        
        
        self.deathPersistentSaveData.GetSlugBaseData().Set(Plugin.MOD_ID, self.deathPersistentSaveData.GetDeathPersistent());

        return orig(self);
    }

    private static void SaveState_LoadGame(On.SaveState.orig_LoadGame orig, SaveState self, string str, RainWorldGame game)
    {
        orig(self, str, game);

        var miscWorld = self.miscWorldSaveData;

        if (miscWorld.GetSlugBaseData().TryGet(Plugin.MOD_ID, out PearlcatMiscWorld miscWorldData))
        {
            if (MiscWorldData.TryGetValue(miscWorld, out _))
                MiscWorldData.Remove(miscWorld);

            MiscWorldData.Add(miscWorld, miscWorldData);
        }

        var deathPersistent = self.deathPersistentSaveData;

        if (deathPersistent.GetSlugBaseData().TryGet(Plugin.MOD_ID, out PearlcatDeathPersistent deathPersistentData))
        {
            if (DeathPersistentData.TryGetValue(deathPersistent, out _))
                DeathPersistentData.Remove(deathPersistent);

            DeathPersistentData.Add(deathPersistent, deathPersistentData);
        }

        LoadMiscProgData(self.progression.miscProgressionData);
    }

    // Useful in place of mining for save data
    public static void LoadMiscProgData(PlayerProgression.MiscProgressionData miscProg)
    {
        if (miscProg.GetSlugBaseData().TryGet(Plugin.MOD_ID, out PearlcatMiscProgression miscProgData))
        {
            if (MiscProgressionData.TryGetValue(miscProg, out _))
                MiscProgressionData.Remove(miscProg);

            MiscProgressionData.Add(miscProg, miscProgData);
        }
    }
}
