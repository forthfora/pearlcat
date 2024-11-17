using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using RWCustom;
using static Conversation;
using static SSOracleBehavior;
using Random = UnityEngine.Random;

namespace Pearlcat;

public static class SSOracleConversation_Helpers
{
    // Determines the first lines of dialogue before anything else Pebbles says
    public static void PebblesPearlIntro(this PebblesConversation self)
    {
        var module = self.owner.GetModule();
        var save = self.owner.oracle.room.game.GetMiscWorld();

        if (save == null)
        {
            return;
        }


        if (module.WasPearlAlreadyRead)
        {
            switch (Random.Range(0, 4))
            {
                case 0:
                    self.events.Add(new TextEvent(self, 0, self.owner.Translate("I already read this one. I can read it again, I suppose."), 10));
                    break;

                case 1:
                    self.events.Add(new TextEvent(self, 0, self.owner.Translate("This one again? I have better things to do... but..."), 10));
                    break;

                case 2:
                    self.events.Add(new TextEvent(self, 0, self.owner.Translate("Let us see what- oh, again?"), 10));
                    break;

                default:
                    self.events.Add(new TextEvent(self, 0, self.owner.Translate("I remember this one... don't you? Well..."), 10));
                    break;
            }
        }
        else
        {
            switch (save.UniquePearlsBroughtToPebbles)
            {
                case 1:
                    self.events.Add(new TextEvent(self, 0, self.owner.Translate("Something new to read... it has been too long..."), 10));
                    break;

                case 2:
                    self.events.Add(new TextEvent(self, 0, self.owner.Translate("Another? And I shall read this one to you as well..."), 10));
                    break;

                case 3:
                    self.events.Add(new TextEvent(self, 0, self.owner.Translate("And a third? So it is..."), 10));
                    break;

                case 4:
                    self.events.Add(new TextEvent(self, 0, self.owner.Translate("Four! Well, if you insist..."), 10));
                    break;

                case 5:
                    self.events.Add(new TextEvent(self, 0, self.owner.Translate("So curious..."), 10));
                    break;

                default:
                    switch (Random.Range(0, 4))
                    {
                        case 0:
                            self.events.Add(new TextEvent(self, 0, self.owner.Translate("Another one? I shouldn't be surprised. Let's see..."), 10));
                            break;

                        case 1:
                            self.events.Add(new TextEvent(self, 0, self.owner.Translate("Ah, yet another one? You are even better at finding these than I imagined..."), 10));
                            break;

                        case 2:
                            self.events.Add(new TextEvent(self, 0, self.owner.Translate("Read this one too? Yes, yes, here it is..."), 10));
                            break;

                        default:
                            self.events.Add(new TextEvent(self, 0, self.owner.Translate("Something else new? Allow me to see..."), 10));
                            break;
                    }
                    break;
            }
        }
    }


    // Method taken from CRS to parse custom oracle conversations from a file
    // I think this is based on decompiled code, could really use some cleaning up, does work tho
    public static void LoadCustomEventsFromFile(this Conversation self, string fileName, SlugcatStats.Name? saveFile = null, bool oneRandomLine = false, int randomSeed = 0)
    {
        if (saveFile == null) { saveFile = self.currentSaveFile; }

        var languageID = Utils.Translator.currentLanguage;
        string text;
        for (; ; )
        {
            text = AssetManager.ResolveFilePath(Utils.Translator.SpecificTextFolderDirectory(languageID) + Path.DirectorySeparatorChar + fileName + ".txt");
            if (saveFile != null)
            {
                var text2 = text;
                text = AssetManager.ResolveFilePath(string.Concat([
                    Utils.Translator.SpecificTextFolderDirectory(languageID),
                    Path.DirectorySeparatorChar.ToString(),
                    fileName,
                    "-",
                    saveFile.value,
                    ".txt"
                ]));
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
        var text3 = File.ReadAllText(text, Encoding.UTF8);
        if (text3[0] != '0')
        {
            text3 = Custom.xorEncrypt(text3, 54 + fileName.GetHashCode() + (int)languageID * 7);
        }

        var array = Regex.Split(text3, "\r\n");
        try
        {

            if (oneRandomLine)
            {
                var list = new List<TextEvent>();
                for (var i = 1; i < array.Length; i++)
                {
                    var array2 = LocalizationTranslator.ConsolidateLineInstructions(array[i]);
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
                    var state = Random.state;
                    Random.InitState(randomSeed);
                    var item = list[Random.Range(0, list.Count)];
                    Random.state = state;
                    self.events.Add(item);
                }
            }
            else
            {
                for (var j = 1; j < array.Length; j++)
                {
                    var array3 = LocalizationTranslator.ConsolidateLineInstructions(array[j]);
                    if (array3.Length == 3)
                    {
                        if (ModManager.MSC && !int.TryParse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture, out _) && int.TryParse(array3[2], NumberStyles.Any, CultureInfo.InvariantCulture, out _))
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
