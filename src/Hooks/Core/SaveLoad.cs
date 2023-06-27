using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;


namespace Pearlcat;

public static partial class Hooks
{
    // TODO: rewrite this whole load of garbage
    public class PearlcatSaveStateSaveData : SaveData
    {
        public override void ReadData(string e)
        {

        }

        public override void WriteData(ref string s)
        {

        }
    }


    public class PearlcatDeathPersistentSaveData : SaveData
    {
        public Dictionary<int, List<string>> RawInventoryData = new();
        public Dictionary<int, int> ActiveObjectIndex = new();

        public int MaxStorageCount = 11; 

        public bool MetPebbles = false;
        public bool MetMoon = false;


        public override void ReadData(string e)
        {
            ReadIntListString(e, ref RawInventoryData, nameof(RawInventoryData));
            ReadIntInt(e, ref ActiveObjectIndex, nameof(ActiveObjectIndex));

            ReadInt(e, ref MaxStorageCount, nameof(MaxStorageCount));

            ReadBool(e, ref MetPebbles, nameof(MetPebbles));
            ReadBool(e, ref MetMoon, nameof(MetMoon));
        }

        public override void WriteData(ref string s)
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
        public override void ReadData(string e)
        {

        }

        public override void WriteData(ref string s)
        {

        }
    }



    public static ConditionalWeakTable<SaveState, PearlcatSaveStateSaveData> SaveStateData = new();
    public static ConditionalWeakTable<DeathPersistentSaveData, PearlcatDeathPersistentSaveData> DeathPersistentData = new();
    public static ConditionalWeakTable<PlayerProgression.MiscProgressionData, PearlcatMiscProgressionSaveData> MiscProgressionData = new();

    public static bool GetSaveState(this RainWorldGame game, out PearlcatSaveStateSaveData saveState)
        => SaveStateData.TryGetValue(game.GetStorySession.saveState, out saveState);

    public static bool GetDeathPersistentData(this RainWorldGame game, out PearlcatDeathPersistentSaveData deathPersistentData)
        => DeathPersistentData.TryGetValue(game.GetStorySession.saveState.deathPersistentSaveData, out deathPersistentData);



    public static void ApplySaveLoadHooks()
    {
        On.DeathPersistentSaveData.ctor += DeathPersistentSaveData_ctor;
        On.DeathPersistentSaveData.SaveToString += DeathPersistentSaveData_SaveToString;
        On.DeathPersistentSaveData.FromString += DeathPersistentSaveData_FromString;
    }

    public static void DeathPersistentSaveData_ctor(On.DeathPersistentSaveData.orig_ctor orig, DeathPersistentSaveData self, SlugcatStats.Name slugcat)
    {
        orig(self, slugcat);

        if (!DeathPersistentData.TryGetValue(self, out _))
            DeathPersistentData.Add(self, new PearlcatDeathPersistentSaveData());
    }
    
    public static string DeathPersistentSaveData_SaveToString(On.DeathPersistentSaveData.orig_SaveToString orig, DeathPersistentSaveData self, bool saveAsIfPlayerDied, bool saveAsIfPlayerQuit)
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
    
    public static void DeathPersistentSaveData_FromString(On.DeathPersistentSaveData.orig_FromString orig, DeathPersistentSaveData self, string s)
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



    #region Data Parsing

    public static string SaveDataStart => Plugin.MOD_ID + "DataStart";
    public static string SaveDataEnd => Plugin.MOD_ID + "DataEnd";

    const char SEPARATOR_CHAR = '˥';
    const char SEPARATOR_CHAR_2 = '˦';
    const char SEPARATOR_CHAR_3 = '˧';

    const char EQUALITY_CHAR = '˨';
    const char EQUALITY_CHAR_2 = '˩';


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

        public abstract void ReadData(string e);

        public abstract void WriteData(ref string s);
    }



    // Primitives
    public static void WriteValue<TValue>(ref string s, ref TValue target, string targetName)
    {
        if (target == null) return;

        s += SEPARATOR_CHAR;

        s += targetName;
        s += EQUALITY_CHAR;
        s += target.ToString();
    }


    public static void ReadBool(string e, ref bool target, string targetName)
    {
        string[] keyValue = e.Split(EQUALITY_CHAR);

        if (keyValue.Length < 2) return;

        if (keyValue[0] != targetName) return;

        if (!bool.TryParse(keyValue[1], out var result)) return;

        target = result;
    }

    public static void ReadInt(string e, ref int target, string targetName)
    {
        string[] keyValue = e.Split(EQUALITY_CHAR);

        if (keyValue.Length < 2) return;

        if (keyValue[0] != targetName) return;

        if (!int.TryParse(keyValue[1], out var result)) return;

        target = result;
    }

    public static void ReadFloat(string e, ref float target, string targetName)
    {
        string[] keyValue = e.Split(EQUALITY_CHAR);

        if (keyValue.Length < 2) return;

        if (keyValue[0] != targetName) return;

        if (!float.TryParse(keyValue[1], out var result)) return;

        target = result;
    }

    public static void ReadString(string e, ref string target, string targetName)
    {
        string[] keyValue = e.Split(EQUALITY_CHAR);

        if (keyValue.Length < 2) return;

        if (keyValue[0] != targetName) return;

        target = keyValue[1];
    }



    // Dictionaries
    public static void WriteDict<TKey, TValue>(ref string s, ref Dictionary<TKey, TValue> target, string targetName)
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


    public static void ReadIntInt(string e, ref Dictionary<int, int> target, string targetName)
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
    public static void ReadIntListString(string e, ref Dictionary<int, List<string>> target, string targetName)
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

            string[] arrString = kvp[1].Split(SEPARATOR_CHAR_3);

            List<string> listString = arrString.ToList();
            listString.RemoveAll(string.IsNullOrWhiteSpace);

            target[key] = listString;
        }
    }

    public static void WriteIntListString(ref string s, ref Dictionary<int, List<string>> target, string targetName)
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
