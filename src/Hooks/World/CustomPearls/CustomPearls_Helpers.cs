using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using RWCustom;

namespace Pearlcat;

public static class CustomPearls_Helpers
{
    public static List<DataPearl.AbstractDataPearl.DataPearlType> CustomPearlTypes { get; } =
    [
        Enums.Pearls.RM_Pearlcat,

        Enums.Pearls.AS_PearlBlue,
        Enums.Pearls.AS_PearlRed,
        Enums.Pearls.AS_PearlYellow,
        Enums.Pearls.AS_PearlGreen,
        Enums.Pearls.AS_PearlBlack,

        Enums.Pearls.SS_Pearlcat,

        Enums.Pearls.Heart_Pearlpup,

        Enums.Pearls.CW_Pearlcat,
    ];

    public static Dictionary<DataPearl.AbstractDataPearl.DataPearlType, Color> CustomPearlHighlightColors { get; } = new()
    {
        { Enums.Pearls.RM_Pearlcat, Custom.hexToColor("ff0000") },

        { Enums.Pearls.AS_PearlBlue, Custom.hexToColor("ffffff") },
        { Enums.Pearls.AS_PearlRed, Custom.hexToColor("ffffff") },
        { Enums.Pearls.AS_PearlYellow, Custom.hexToColor("ffffff") },
        { Enums.Pearls.AS_PearlGreen, Custom.hexToColor("ffffff") },
        { Enums.Pearls.AS_PearlBlack, Custom.hexToColor("ffffff") },

        { Enums.Pearls.SS_Pearlcat, Custom.hexToColor("ff0000") },

        { Enums.Pearls.Heart_Pearlpup, Custom.hexToColor("ffffff") },

        { Enums.Pearls.CW_Pearlcat, Custom.hexToColor("7dbaff") },
    };

    public static Dictionary<DataPearl.AbstractDataPearl.DataPearlType, Color> CustomPearlMainColors { get; } = new()
    {
        { Enums.Pearls.RM_Pearlcat, Custom.hexToColor("622ffb") },

        { Enums.Pearls.AS_PearlBlue, Custom.hexToColor("42adf5") },
        { Enums.Pearls.AS_PearlRed, Custom.hexToColor("b32c1b") },
        { Enums.Pearls.AS_PearlYellow, Custom.hexToColor("fcf63f") },
        { Enums.Pearls.AS_PearlGreen, Custom.hexToColor("42c728") },
        { Enums.Pearls.AS_PearlBlack, Custom.hexToColor("121212") },

        { Enums.Pearls.SS_Pearlcat, Custom.hexToColor("ffffff") },

        { Enums.Pearls.Heart_Pearlpup, Custom.hexToColor("ffffff") },

        { Enums.Pearls.CW_Pearlcat, Custom.hexToColor("ffffff") },
    };


    public static bool IsCustomPearlConvo(this Conversation.ID convoID)
    {
        return CustomPearlTypes.Any(x => x.GetCustomPearlConvoId() == convoID);
    }

    public static bool IsCustomPearl(this DataPearl.AbstractDataPearl.DataPearlType pearlType)
    {
        return CustomPearlTypes.Contains(pearlType);
    }

    public static Conversation.ID GetCustomPearlConvoId(this DataPearl.AbstractDataPearl.DataPearlType pearlType)
    {
        return new(pearlType.value, true);
    }


    public static Color? GetCustomPearlHighlightColor(DataPearl.AbstractDataPearl.DataPearlType pearlType)
    {
        if (CustomPearlHighlightColors.TryGetValue(pearlType, out var color))
        {
            return color;
        }

        return null;
    }

    public static Color? GetCustomPearlMainColor(DataPearl.AbstractDataPearl.DataPearlType pearlType)
    {
        if (CustomPearlMainColors.TryGetValue(pearlType, out var color))
        {
            return color;
        }

        return null;
    }


    // Based on decompiled code from Conversation.LoadEventsFromFile - just changed to accept a string fileName parameter + remove encryption
    public static void LoadCustomEventsFromFile(this Conversation self, string fileName, SlugcatStats.Name? saveFile = null, bool oneRandomLine = false, int randomSeed = 0)
    {
        var targetLanguage = Utils.RainWorld.inGameTranslator.currentLanguage;
        string path;

        while (true)
        {
            var langDir = Utils.RainWorld.inGameTranslator.SpecificTextFolderDirectory(targetLanguage);

            var sepChar = Path.DirectorySeparatorChar;

            var sepString = sepChar.ToString();

            path = AssetManager.ResolveFilePath(langDir + sepString + fileName + ".txt");

            if (saveFile is not null)
            {
                var originalPath = path;

                var finalFileName = new string[6];

                finalFileName[0] = Utils.RainWorld.inGameTranslator.SpecificTextFolderDirectory(targetLanguage);

                sepChar = Path.DirectorySeparatorChar;

                finalFileName[1] = sepChar.ToString();
                finalFileName[2] = fileName;
                finalFileName[3] = "-";
                finalFileName[4] = saveFile.value;
                finalFileName[5] = ".txt";

                path = AssetManager.ResolveFilePath(string.Concat(finalFileName));

                if (!File.Exists(path))
                {
                    path = originalPath;
                }
            }

            if (!File.Exists(path))
            {
                Plugin.Logger.LogWarning("NOT FOUND " + path);
                if (targetLanguage != InGameTranslator.LanguageID.English)
                {
                    Plugin.Logger.LogWarning("RETRY WITH ENGLISH");
                    targetLanguage = InGameTranslator.LanguageID.English;
                }
                else
                {
                    break;
                }
            }
            else
            {
                goto ParseFile;
            }
        }

        return;

        ParseFile:

        var contents = File.ReadAllText(path, Encoding.UTF8);

        // Unnecessary encryption (thanks Joar)
        // if (str[0] != '0')
        // {
        //     str = Custom.xorEncrypt(str, 54 + fileName + (int)(ExtEnum<InGameTranslator.LanguageID>)self.interfaceOwner.rainWorld.inGameTranslator.currentLanguage * 7);
        // }

        var splitContents = Regex.Split(contents, "\r\n");

        try
        {
            // Unecessary verification check (thanks Joar)
            // if (Regex.Split(splitContents[0], "-")[1] != fileName)
            // {
            //     return;
            // }

            if (oneRandomLine)
            {
                var textEventList = new List<Conversation.TextEvent>();

                for (var index = 1; index < splitContents.Length; ++index)
                {
                    var instructions = LocalizationTranslator.ConsolidateLineInstructions(splitContents[index]);

                    if (instructions.Length == 3)
                    {
                        textEventList.Add(new Conversation.TextEvent(self,
                            int.Parse(instructions[0], NumberStyles.Any, CultureInfo.InvariantCulture), instructions[2],
                            int.Parse(instructions[1], NumberStyles.Any, CultureInfo.InvariantCulture)));
                    }
                    else if (instructions.Length == 1 && instructions[0].Length > 0)
                    {
                        textEventList.Add(new Conversation.TextEvent(self, 0, instructions[0], 0));
                    }
                }

                if (textEventList.Count <= 0)
                {
                    return;
                }

                // RANDOM
                var state = Random.state;
                Random.InitState(randomSeed);

                var textEvent = textEventList[Random.Range(0, textEventList.Count)];

                Random.state = state;

                self.events.Add(textEvent);
            }
            else
            {
                for (var i = 1; i < splitContents.Length; ++i)
                {
                    var instructions = LocalizationTranslator.ConsolidateLineInstructions(splitContents[i]);

                    if (instructions.Length == 3)
                    {
                        if (ModManager.MSC &&
                            !int.TryParse(instructions[1], NumberStyles.Any, CultureInfo.InvariantCulture, out _) &&
                            int.TryParse(instructions[2], NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                        {
                            self.events.Add(new Conversation.TextEvent(self,
                                int.Parse(instructions[0], NumberStyles.Any, CultureInfo.InvariantCulture), instructions[1],
                                int.Parse(instructions[2], NumberStyles.Any, CultureInfo.InvariantCulture)));
                        }
                        else
                        {
                            self.events.Add(new Conversation.TextEvent(self,
                                int.Parse(instructions[0], NumberStyles.Any, CultureInfo.InvariantCulture), instructions[2],
                                int.Parse(instructions[1], NumberStyles.Any, CultureInfo.InvariantCulture)));
                        }
                    }
                    else if (instructions.Length == 2)
                    {
                        if (instructions[0] == "SPECEVENT")
                        {
                            self.events.Add(new Conversation.SpecialEvent(self, 0, instructions[1]));
                        }
                        else if (instructions[0] == "PEBBLESWAIT")
                        {
                            self.events.Add(new SSOracleBehavior.PebblesConversation.PauseAndWaitForStillEvent(self, null,
                                int.Parse(instructions[1], NumberStyles.Any, CultureInfo.InvariantCulture)));
                        }
                    }
                    else if (instructions.Length == 1 && instructions[0].Length > 0)
                    {
                        self.events.Add(new Conversation.TextEvent(self, 0, instructions[0], 0));
                    }
                }
            }
        }
        catch
        {
            Plugin.Logger.LogWarning("TEXT ERROR");

            self.events.Add(new Conversation.TextEvent(self, 0, "TEXT ERROR", 100));
        }
    }
}
