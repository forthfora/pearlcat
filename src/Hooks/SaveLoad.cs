using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;


namespace Pearlcat;

public static partial class Hooks
{
    public class PearlcatSaveStateSaveData : SaveData
    {
        protected override void ReadData(string e)
        {

        }

        protected override void WriteData(ref string s)
        {

        }
    }


    public class PearlcatDeathPersistentSaveData : SaveData
    {
        public Dictionary<int, List<string>> RawInventoryData = new();

        public Dictionary<int, int> ActiveObjectIndex = new();

        public int MaxStorageCount = 10; 

        public bool MetPebbles = false;
        public bool MetMoon = false;


        protected override void ReadData(string e)
        {
            ReadIntListString(e, ref RawInventoryData, nameof(RawInventoryData));
            ReadIntInt(e, ref ActiveObjectIndex, nameof(ActiveObjectIndex));

            ReadInt(e, ref MaxStorageCount, nameof(MaxStorageCount));

            ReadBool(e, ref MetPebbles, nameof(MetPebbles));
            ReadBool(e, ref MetMoon, nameof(MetMoon));
        }

        protected override void WriteData(ref string s)
        {
            WriteIntListString(ref s, ref RawInventoryData, nameof(RawInventoryData));
            WriteDict(ref s, ref ActiveObjectIndex, nameof(ActiveObjectIndex));

            WriteValue(ref s, ref MaxStorageCount, nameof(MaxStorageCount));

            WriteValue(ref s, ref MetPebbles, nameof(MetPebbles));
            WriteValue(ref s, ref MetMoon, nameof(MetMoon));
        }
    }


    public class PearlcatMiscProgressionSaveData : SaveData
    {
        protected override void ReadData(string e)
        {

        }

        protected override void WriteData(ref string s)
        {

        }
    }



    public static ConditionalWeakTable<SaveState, PearlcatSaveStateSaveData> SaveStateData = new();

    public static ConditionalWeakTable<DeathPersistentSaveData, PearlcatDeathPersistentSaveData> DeathPersistentData = new();

    public static ConditionalWeakTable<PlayerProgression.MiscProgressionData, PearlcatMiscProgressionSaveData> MiscProgressionData = new();


    private static void ApplySaveLoadHooks()
    {
        On.SaveState.ctor += SaveState_ctor;
        On.SaveState.SaveToString += SaveState_SaveToString;
        On.SaveState.LoadGame += SaveState_LoadGame;

        On.DeathPersistentSaveData.ctor += DeathPersistentSaveData_ctor;
        On.DeathPersistentSaveData.SaveToString += DeathPersistentSaveData_SaveToString;
        On.DeathPersistentSaveData.FromString += DeathPersistentSaveData_FromString;

        On.PlayerProgression.MiscProgressionData.ctor += MiscProgressionData_ctor;
        On.PlayerProgression.MiscProgressionData.ToString += MiscProgressionData_ToString;
        On.PlayerProgression.MiscProgressionData.FromString += MiscProgressionData_FromString;
    }



    private static void SaveState_ctor(On.SaveState.orig_ctor orig, SaveState self, SlugcatStats.Name saveStateNumber, PlayerProgression progression)
    {
        orig(self, saveStateNumber, progression);

        if (!SaveStateData.TryGetValue(self, out _))
            SaveStateData.Add(self, new PearlcatSaveStateSaveData());
    }

    private static string SaveState_SaveToString(On.SaveState.orig_SaveToString orig, SaveState self)
    {
        if (SaveStateData.TryGetValue(self, out var saveData))
        {
            int? saveDataPos = null;

            for (int i = 0; i < self.unrecognizedSaveStrings.Count; i++)
            {
                if (self.unrecognizedSaveStrings[i].StartsWith(SaveDataStart))
                    saveDataPos = i;
            }

            if (saveDataPos == null)
                self.unrecognizedSaveStrings.Add(saveData.DataToString());

            else
                self.unrecognizedSaveStrings[(int)saveDataPos] = saveData.DataToString();
        }

        return orig(self);
    }

    private static void SaveState_LoadGame(On.SaveState.orig_LoadGame orig, SaveState self, string str, RainWorldGame game)
    {
        orig(self, str, game);

        if (!SaveStateData.TryGetValue(self, out var saveData)) return;

        int? saveDataPos = null;

        for (int i = 0; i < self.unrecognizedSaveStrings.Count; i++)
        {
            if (self.unrecognizedSaveStrings[i].StartsWith(SaveDataStart))
                saveDataPos = i;
        }

        if (saveDataPos == null) return;

        saveData.DataFromString(self.unrecognizedSaveStrings[(int)saveDataPos]);
    }



    private static void DeathPersistentSaveData_ctor(On.DeathPersistentSaveData.orig_ctor orig, DeathPersistentSaveData self, SlugcatStats.Name slugcat)
    {
        orig(self, slugcat);

        if (!DeathPersistentData.TryGetValue(self, out _))
            DeathPersistentData.Add(self, new PearlcatDeathPersistentSaveData());
    }
    
    private static string DeathPersistentSaveData_SaveToString(On.DeathPersistentSaveData.orig_SaveToString orig, DeathPersistentSaveData self, bool saveAsIfPlayerDied, bool saveAsIfPlayerQuit)
    {
        if (DeathPersistentData.TryGetValue(self, out var saveData))
        {
            int? saveDataPos = null;

            for (int i = 0; i < self.unrecognizedSaveStrings.Count; i++)
            {
                if (self.unrecognizedSaveStrings[i].StartsWith(SaveDataStart))
                    saveDataPos = i;
            }

            if (saveDataPos == null)
                self.unrecognizedSaveStrings.Add(saveData.DataToString());

            else
                self.unrecognizedSaveStrings[(int)saveDataPos] = saveData.DataToString();
        }

        return orig(self, saveAsIfPlayerDied, saveAsIfPlayerQuit);
    }
    
    private static void DeathPersistentSaveData_FromString(On.DeathPersistentSaveData.orig_FromString orig, DeathPersistentSaveData self, string s)
    {
        orig(self, s);

        if (!DeathPersistentData.TryGetValue(self, out var saveData)) return;

        int? saveDataPos = null;

        for (int i = 0; i < self.unrecognizedSaveStrings.Count; i++)
        {
            if (self.unrecognizedSaveStrings[i].StartsWith(SaveDataStart))
                saveDataPos = i;
        }

        if (saveDataPos == null) return;

        saveData.DataFromString(self.unrecognizedSaveStrings[(int)saveDataPos]);
    }



    private static void MiscProgressionData_ctor(On.PlayerProgression.MiscProgressionData.orig_ctor orig, PlayerProgression.MiscProgressionData self, PlayerProgression owner)
    {
        orig(self, owner);

        if (!MiscProgressionData.TryGetValue(self, out _))
            MiscProgressionData.Add(self, new PearlcatMiscProgressionSaveData());
    }

    private static string MiscProgressionData_ToString(On.PlayerProgression.MiscProgressionData.orig_ToString orig, PlayerProgression.MiscProgressionData self)
    {
        if (MiscProgressionData.TryGetValue(self, out var saveData))
        {
            int? saveDataPos = null;

            for (int i = 0; i < self.unrecognizedSaveStrings.Count; i++)
            {
                if (self.unrecognizedSaveStrings[i].StartsWith(SaveDataStart))
                    saveDataPos = i;
            }

            if (saveDataPos == null)
                self.unrecognizedSaveStrings.Add(saveData.DataToString());

            else
                self.unrecognizedSaveStrings[(int)saveDataPos] = saveData.DataToString();
        }

        return orig(self);
    }

    private static void MiscProgressionData_FromString(On.PlayerProgression.MiscProgressionData.orig_FromString orig, PlayerProgression.MiscProgressionData self, string s)
    {
        orig(self, s);

        if (!MiscProgressionData.TryGetValue(self, out var saveData)) return;

        int? saveDataPos = null;

        for (int i = 0; i < self.unrecognizedSaveStrings.Count; i++)
        {
            if (self.unrecognizedSaveStrings[i].StartsWith(SaveDataStart))
                saveDataPos = i;
        }

        if (saveDataPos == null) return;

        saveData.DataFromString(self.unrecognizedSaveStrings[(int)saveDataPos]);
    }



    #region Data Parsing

    public static string SaveDataStart => Plugin.MOD_ID + "DataStart";
    public static string SaveDataEnd => Plugin.MOD_ID + "DataEnd";

    const char SEPARATOR_CHAR = '/';
    const char SEPARATOR_CHAR_2 = ',';
    const char SEPARATOR_CHAR_3 = '.';

    const char EQUALITY_CHAR = '=';
    const char EQUALITY_CHAR_2 = ':';


    public abstract class SaveData
    {
        public void DataFromString(string s)
        {
            string[] data = s.Split(SEPARATOR_CHAR);

            for (int i = 0; i < data.Length; i++)
            {
                string entry = data[i];

                if (entry == SaveDataEnd) break;

                ReadData(entry);
            }

            Plugin.Logger.LogWarning("LOADED:\n" + s);
        }

        public string DataToString()
        {
            string s = SaveDataStart;

            WriteData(ref s);

            s += SEPARATOR_CHAR;
            s += SaveDataEnd;

            Plugin.Logger.LogWarning("SAVED:\n" + s);

            return s;
        }

        protected abstract void ReadData(string e);

        protected abstract void WriteData(ref string s);
    }



    // Primitives
    private static void WriteValue<TValue>(ref string s, ref TValue target, string targetName)
    {
        if (target == null) return;

        s += SEPARATOR_CHAR;

        s += targetName;
        s += EQUALITY_CHAR;
        s += target.ToString();
    }


    private static void ReadBool(string e, ref bool target, string targetName)
    {
        string[] keyValue = e.Split(EQUALITY_CHAR);

        if (keyValue.Length < 2) return;

        if (keyValue[0] != targetName) return;

        if (!bool.TryParse(keyValue[1], out var result)) return;

        target = result;
    }

    private static void ReadInt(string e, ref int target, string targetName)
    {
        string[] keyValue = e.Split(EQUALITY_CHAR);

        if (keyValue.Length < 2) return;

        if (keyValue[0] != targetName) return;

        if (!int.TryParse(keyValue[1], out var result)) return;

        target = result;
    }

    private static void ReadFloat(string e, ref float target, string targetName)
    {
        string[] keyValue = e.Split(EQUALITY_CHAR);

        if (keyValue.Length < 2) return;

        if (keyValue[0] != targetName) return;

        if (!float.TryParse(keyValue[1], out var result)) return;

        target = result;
    }

    private static void ReadString(string e, ref string target, string targetName)
    {
        string[] keyValue = e.Split(EQUALITY_CHAR);

        if (keyValue.Length < 2) return;

        if (keyValue[0] != targetName) return;

        target = keyValue[1];
    }



    // Dictionaries
    private static void WriteDict<TKey, TValue>(ref string s, ref Dictionary<TKey, TValue> target, string targetName)
    {
        s += SEPARATOR_CHAR;

        s += targetName;
        s += EQUALITY_CHAR;

        foreach (var kvp in target)
        {
            if (kvp.Key == null || kvp.Value == null) continue;

            s += kvp.Key.ToString();
            s += EQUALITY_CHAR_2;
            s += kvp.Value.ToString();

            s += SEPARATOR_CHAR_2;
        }
    }


    private static void ReadIntInt(string e, ref Dictionary<int, int> target, string targetName)
    {
        string[] keyValue = e.Split(EQUALITY_CHAR);

        if (keyValue.Length < 2) return;

        if (keyValue[0] != targetName) return;

        string[] intIntPair = keyValue[1].Split(SEPARATOR_CHAR_2);

        foreach (string intIntString in intIntPair)
        {
            var kvp = intIntString.Split(EQUALITY_CHAR_2);

            if (kvp.Length < 2) continue;


            if (!int.TryParse(kvp[0], out var key)) continue;

            if (!int.TryParse(kvp[1], out var value)) continue;

            target[key] = value;
        }
    }



    // Complex
    private static void ReadIntListString(string e, ref Dictionary<int, List<string>> target, string targetName)
    {
        string[] keyValue = e.Split(EQUALITY_CHAR);

        if (keyValue.Length < 2) return;

        if (keyValue[0] != targetName) return;

        string[] intListPair = keyValue[1].Split(SEPARATOR_CHAR_2);

        foreach (string intListString in intListPair)
        {
            var kvp = intListString.Split(EQUALITY_CHAR_2);

            if (kvp.Length < 2) continue;


            if (!int.TryParse(kvp[0], out var key)) continue;

            string[] listString = kvp[1].Split(SEPARATOR_CHAR_3);
            target[key] = listString.ToList();
        }
    }

    private static void WriteIntListString(ref string s, ref Dictionary<int, List<string>> target, string targetName)
    {
        s += SEPARATOR_CHAR;

        s += targetName;
        s += EQUALITY_CHAR;

        foreach (var kvp in target)
        {
            if (kvp.Value == null) continue;

            s += kvp.Key.ToString();
            s += EQUALITY_CHAR_2;

            foreach (var value in kvp.Value)
            {
                s += value;
                s += SEPARATOR_CHAR_3;
            }

            s += SEPARATOR_CHAR_2;
        }
    }

    #endregion
}
