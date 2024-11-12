using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using RWCustom;
using static Conversation;
using static SSOracleBehavior;
using Random = UnityEngine.Random;

namespace Pearlcat;

public static partial class Utils
{
    public static RainWorld RainWorld => Custom.rainWorld;
    public static Dictionary<string, FShader> Shaders => RainWorld.Shaders;
    public static InGameTranslator Translator => RainWorld.inGameTranslator;
    public static SaveMiscProgression GetMiscProgression() => RainWorld.GetMiscProgression();


    // Method taken from CRS to parse custom oracle conversations from a file
    // I think this is based on decompiled code, could really use some cleaning up, does work tho
    public static void LoadCustomEventsFromFile(this Conversation self, string fileName, SlugcatStats.Name? saveFile = null, bool oneRandomLine = false, int randomSeed = 0)
    {
        if (saveFile == null) { saveFile = self.currentSaveFile; }

        var languageID = Translator.currentLanguage;
        string text;
        for (; ; )
        {
            text = AssetManager.ResolveFilePath(Translator.SpecificTextFolderDirectory(languageID) + Path.DirectorySeparatorChar.ToString() + fileName + ".txt");
            if (saveFile != null)
            {
                string text2 = text;
                text = AssetManager.ResolveFilePath(string.Concat(new string[]
                {
                    Translator.SpecificTextFolderDirectory(languageID),
                    Path.DirectorySeparatorChar.ToString(),
                    fileName,
                    "-",
                    saveFile.value,
                    ".txt"
                }));
                if (!File.Exists(text))
                {
                    text = text2;
                }
            }
            if (File.Exists(text))
            {
                goto IL_117;
            }
            if (languageID == InGameTranslator.LanguageID.English)
            {
                break;
            }
            languageID = InGameTranslator.LanguageID.English;
        }
        return;

    IL_117:
        string text3 = File.ReadAllText(text, Encoding.UTF8);
        if (text3[0] != '0')
        {
            text3 = Custom.xorEncrypt(text3, 54 + fileName.GetHashCode() + (int)languageID * 7);
        }

        string[] array = Regex.Split(text3, "\r\n");
        try
        {

            if (oneRandomLine)
            {
                List<TextEvent> list = new List<TextEvent>();
                for (int i = 1; i < array.Length; i++)
                {
                    string[] array2 = LocalizationTranslator.ConsolidateLineInstructions(array[i]);
                    if (array2.Length == 3)
                    {
                        list.Add(new TextEvent(self, int.Parse(array2[0], NumberStyles.Any, CultureInfo.InvariantCulture), array2[2], int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture)));
                    }
                    else if (array2.Length == 1 && array2[0].Length > 0)
                    {
                        list.Add(new TextEvent(self, 0, array2[0], 0));
                    }
                }
                if (list.Count > 0)
                {
                    Random.State state = Random.state;
                    Random.InitState(randomSeed);
                    TextEvent item = list[Random.Range(0, list.Count)];
                    Random.state = state;
                    self.events.Add(item);
                }
            }
            else
            {
                for (int j = 1; j < array.Length; j++)
                {
                    string[] array3 = LocalizationTranslator.ConsolidateLineInstructions(array[j]);
                    if (array3.Length == 3)
                    {
                        if (ModManager.MSC && !int.TryParse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture, out int num) && int.TryParse(array3[2], NumberStyles.Any, CultureInfo.InvariantCulture, out int num2))
                        {
                            self.events.Add(new TextEvent(self, int.Parse(array3[0], NumberStyles.Any, CultureInfo.InvariantCulture), array3[1], int.Parse(array3[2], NumberStyles.Any, CultureInfo.InvariantCulture)));
                        }
                        else
                        {
                            self.events.Add(new TextEvent(self, int.Parse(array3[0], NumberStyles.Any, CultureInfo.InvariantCulture), array3[2], int.Parse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture)));
                        }
                    }
                    else if (array3.Length == 2)
                    {
                        if (array3[0] == "SPECEVENT")
                        {
                            self.events.Add(new SpecialEvent(self, 0, array3[1]));
                        }
                        else if (array3[0] == "PEBBLESWAIT")
                        {
                            self.events.Add(new PebblesConversation.PauseAndWaitForStillEvent(self, null, int.Parse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture)));
                        }
                    }
                    else if (array3.Length == 1 && array3[0].Length > 0)
                    {
                        self.events.Add(new TextEvent(self, 0, array3[0], 0));
                    }
                }
            }

        }
        catch
        {
            self.events.Add(new TextEvent(self, 0, "TEXT ERROR", 100));
        }
    }
}
