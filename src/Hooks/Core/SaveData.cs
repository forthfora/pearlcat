using Newtonsoft.Json;
using SlugBase;
using SlugBase.SaveData;
using System;
using System.Collections.Generic;
using UnityEngine;
using static DataPearl.AbstractDataPearl;

namespace Pearlcat;

public static partial class Hooks
{
    public class SaveMiscWorld
    {
        public bool IsNewGame { get; set; } = true;

        public Dictionary<int, List<string>> Inventory { get; } = new();
        public Dictionary<int, int?> ActiveObjectIndex { get; } = new();

        public Dictionary<int, SpearModule> PearlSpears { get; } = new();
    }

    public class SaveDeathPersistent
    {
    }

    public class SaveMiscProgression
    {
        public bool IsNewSave { get; set; } = true;

        public List<DataPearlType> StoredPearlTypes { get; set; } = new();
        public DataPearlType? ActivePearlType { get; set; }
    }


    public static SaveMiscWorld GetMiscWorld(this RainWorldGame game) => GetMiscWorld(game.GetStorySession.saveState.miscWorldSaveData);
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
    }

    private static string SaveState_SaveToString(On.SaveState.orig_SaveToString orig, SaveState self)
    {
        var miscWorld = self.miscWorldSaveData.GetMiscWorld();
        var miscProg = self.progression.miscProgressionData.GetMiscProgression();

        miscWorld.IsNewGame = false;
        miscProg.IsNewSave = false;

        return orig(self);
    }

    private static void SaveState_LoadGame(On.SaveState.orig_LoadGame orig, SaveState self, string str, RainWorldGame game)
    {
        orig(self, str, game);

        var miscWorld = self.miscWorldSaveData.GetMiscWorld();
        var miscProg = self.progression.miscProgressionData.GetMiscProgression();

        miscProg.IsNewSave = miscWorld.IsNewGame;
    }


    // https://medium.com/@altaf.navalur/serialize-deserialize-color-objects-in-unity-1731e580af94
    public class ColorHandler : Newtonsoft.Json.JsonConverter
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
