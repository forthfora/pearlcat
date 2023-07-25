using Newtonsoft.Json;
using SlugBase.SaveData;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static DataPearl.AbstractDataPearl;

namespace Pearlcat;

public static partial class Hooks
{
    public class SaveMiscWorld
    {
        public bool IsNewGame { get; set; } = true;
        public bool IsPearlcatStory { get; set; }

        public Dictionary<int, List<string>> Inventory { get; } = new();
        public Dictionary<int, int?> ActiveObjectIndex { get; } = new();

        public Dictionary<int, SpearModule> PearlSpears { get; } = new();

        public int PebblesMeetCount { get; set; }
        public Dictionary<int, int> PearlIDsBroughtToPebbles { get; } = new();
        public int UniquePearlsBroughtToPebbles => PearlIDsBroughtToPebbles.Keys.Count;

        public bool ShownFullInventoryTutorial { get; set; }
        public bool ShownSpearCreationTutorial { get; set; }
    }

    public class SaveDeathPersistent
    {
    }

    public class SaveMiscProgression
    {
        public bool IsNewPearlcatSave { get; set; } = true;
        public bool IsMSCSave { get; set; }

        public List<DataPearlType> StoredPearlTypes { get; } = new();
        public DataPearlType? ActivePearlType { get; set; }
    }


    public static SaveMiscWorld? GetMiscWorld(this RainWorldGame game) => game.IsStorySession ? GetMiscWorld(game.GetStorySession.saveState.miscWorldSaveData) : null;
    public static SaveMiscWorld GetMiscWorld(this MiscWorldSaveData data)
    {
        if (!data.GetSlugBaseData().TryGet(Plugin.MOD_ID, out SaveMiscWorld save))
            data.GetSlugBaseData().Set(Plugin.MOD_ID, save = new());

        return save;
    }

    public static SaveDeathPersistent GetDeathPersistent(this RainWorldGame game) => GetDeathPersistent(game.GetStorySession.saveState.deathPersistentSaveData);
    public static SaveDeathPersistent GetDeathPersistent(this DeathPersistentSaveData data)
    {
        if (!data.GetSlugBaseData().TryGet(Plugin.MOD_ID, out SaveDeathPersistent save))
            data.GetSlugBaseData().Set(Plugin.MOD_ID, save = new());

        return save;
    }

    public static SaveMiscProgression GetMiscProgression(this RainWorld rainWorld) => GetMiscProgression(rainWorld.progression.miscProgressionData);
    public static SaveMiscProgression GetMiscProgression(this RainWorldGame game) => GetMiscProgression(game.GetStorySession.saveState.progression.miscProgressionData);
    public static SaveMiscProgression GetMiscProgression(this PlayerProgression.MiscProgressionData data)
    {
        if (!data.GetSlugBaseData().TryGet(Plugin.MOD_ID, out SaveMiscProgression save))
            data.GetSlugBaseData().Set(Plugin.MOD_ID, save = new());

        return save;
    }

    
    public static void ApplySaveDataHooks()
    {
        On.SaveState.SaveToString += SaveState_SaveToString;
        On.SaveState.LoadGame += SaveState_LoadGame;

        On.PlayerProgression.SaveToDisk += PlayerProgression_SaveToDisk;
    }

    private static bool PlayerProgression_SaveToDisk(On.PlayerProgression.orig_SaveToDisk orig, PlayerProgression self, bool saveCurrentState, bool saveMaps, bool saveMiscProg)
    {
        var miscWorld = self.currentSaveState?.miscWorldSaveData?.GetMiscWorld();
        var miscProg = self.miscProgressionData?.GetMiscProgression();


        if (miscWorld != null && miscProg != null && saveCurrentState && miscWorld.IsPearlcatStory)
        {
            miscProg.StoredPearlTypes.Clear();
            miscProg.ActivePearlType = null;

            if (miscWorld.Inventory.TryGetValue(0, out var inventory) && miscWorld.ActiveObjectIndex.TryGetValue(0, out var activeIndex))
            {
                for (int i = 0; i < inventory.Count; i++)
                {
                    string? item = inventory[i];

                    var split = item.Split(new string[] { "<oA>" }, StringSplitOptions.None);

                    if (split.Length < 5) continue;

                    var tryToParse = split[5];


                    if (!ExtEnumBase.TryParse(typeof(DataPearlType), tryToParse, false, out var type)) continue;

                    if (type is not DataPearlType dataPearlType) continue;

                    if (i == activeIndex)
                        miscProg.ActivePearlType = dataPearlType;

                    else
                        miscProg.StoredPearlTypes.Add(dataPearlType);
                }
            }
        }

        return orig(self, saveCurrentState, saveMaps, saveMiscProg);
    }

    private static string SaveState_SaveToString(On.SaveState.orig_SaveToString orig, SaveState self)
    {
        var miscWorld = self.miscWorldSaveData.GetMiscWorld();
        var miscProg = self.progression.miscProgressionData.GetMiscProgression();

        miscProg.IsNewPearlcatSave = false;
        miscWorld.IsNewGame = false;

        return orig(self);
    }

    private static void SaveState_LoadGame(On.SaveState.orig_LoadGame orig, SaveState self, string str, RainWorldGame game)
    {
        orig(self, str, game);

        var miscWorld = self.miscWorldSaveData.GetMiscWorld();
        var miscProg = self.progression.miscProgressionData.GetMiscProgression();


        miscWorld.IsPearlcatStory = self.saveStateNumber == Enums.Pearlcat;
        miscProg.IsMSCSave = ModManager.MSC;

        if (miscWorld.IsPearlcatStory)
        {
            miscProg.IsNewPearlcatSave = miscWorld.IsNewGame;
        }
    }


    // https://medium.com/@altaf.navalur/serialize-deserialize-color-objects-in-unity-1731e580af94
    public class ColorHandler : JsonConverter
    {
        public override bool CanConvert(Type objectType) => true;

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            try
            {
                ColorUtility.TryParseHtmlString("#" + reader.Value, out Color loadedColor);
                return loadedColor;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse color {objectType} : {ex.Message}");
                return null;
            }
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null) return;

            string val = ColorUtility.ToHtmlStringRGB((Color)value);
            writer.WriteValue(val);
        }
    }
}
