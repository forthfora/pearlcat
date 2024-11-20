import os
import re

from googletrans import Translator

ROOT_DIR = os.path.join(os.path.join(os.environ['USERPROFILE']), 'Desktop/pearlcat/src') 
OUTPUT_DIR = os.path.join(os.path.join(os.environ['USERPROFILE']), 'Desktop/pearlcat/assets/text') 

translator = Translator()

SRC = "en"

langMap = {
    "ru": "rus",
    "ko": "kor",
    "fr": "fre",
    "pt": "por",
    "it": "ita",
    "de": "ger",
    "ja": "jap",
    "zh-CN": "chi",
    "es": "spa"
}

excludedDirNames = [
    "obj",
    "bin",
    ".idea",
    "lib",

    "Player",
    "Pearlpup",
    "Modules",
    "PearlAnimator",
    "PearlEffect",
    "SaveData",
    "InventoryHUD",
    "BackgroundView",
]

excludedFileNames = [
    "Plugin.cs",
    "CWIntegration.cs",
    "AssetLoader.cs",
    "Utils.cs",
    "World_Helpers.cs",
    "CloakGraphics.cs",
    "Pearlcat.AssemblyInfo.cs",
    "OptionsTemplate.cs",
    "ModCompat_Hooks.cs"
]

excludedStrings = [
    "Geahgeah ",
    "Sidera ",
    "forthfora",

    "Noir ",
    "Lin ",
    "zbiotr ",
    "Kimi ",

    "TurtleMan27",
    "Balagaga",
    "Elliot",
    "Efi",
    "WillowWisp",
    "Lolight2",
    "mayhemmm",

    "Goto Failed",
    "DschockHorizontalRight",
    "T1_",
    "T1",
    "mira",
    "<oA>",
    "\\r\\n",
    "PearlcatMoon_",
    "PearlcatPebbles_",
    "Joystick[0-9]Button",
    "na_30 - distance",
    "TEXT ERROR",
    "\\n",
    ".meta",
    ".txt",
    "sleep1",
    "-",
    " || fileName ==",
    " || fileName == ",
    "OE_",
    "SS_T1_S01",
    "CC",
    "SS_AI",
    "soundeffects/ambient",
]


def Translate(targetLang, preserveExisting, actuallyTranslate = True, printTranslatedLines = False):
    print("TRANSLATING: " + targetLang)

    strings = [
        "Pearlcat",
        "PEARLCAT",
        "The Pearlcat",
        "A scholar of obscure origin, armed with an enigmatic energy and a thirst for knowledge.<LINE>Physically frail, but with pearls as your ally - what will you discover on your travels?",
        "Their curiosity insatiable, Pearlcat ventures out once more in<LINE>pursuit of expanding their collection",
        "Transit System",

        "Storage limit reached (",
        "): swap out a pearl, or change the limit in the Remix options",

        "Pearl spears will attempt to return to you after being thrown, if they are not stuck",
        "Hold (GRAB) with an active common pearl to convert it into a pearl spear",

        "Hold ("
        ") or (",
        ") with an active common pearl to convert it into a pearl spear",

        "Version",
        "by",

        "Skip to Mira Storyline"
    ]

    mappedStrings = {
        "pearlcat-name" : "Pearlcat",
        "pearlcat-description" : "A curious scholar with the unusual ability to harness pearls to their advantage.<LINE><LINE>Configure inputs, difficulty, cheats and more via the Remix config!<LINE><LINE>More Slugcats is optional, but strongly recommended.",
    }

    for x in mappedStrings.keys():
        strings.append(x)


    for subdir, dirs, files in os.walk(ROOT_DIR):
        for fileName in files:
            if not fileName.endswith(".cs"): continue

            if fileName in excludedFileNames: continue

            if any("\\" + x in subdir for x in excludedDirNames): continue

            filePath = os.path.join(subdir, fileName)

            f = open(filePath, "r")

            contents = f.read()

            regexConditions = [
                "(?<!LogError\()",
                "(?<!LogWarning\()",
                "(?<!LogInfo\()",
                "(?<!Texture\()",
                "(?<!hexToColor\()",
                "(?<!== )",
                "(?<!new\()",
                "(?<!Ldstr, )",
                "\"(.*?)\""
            ]

            pattern = ''.join(regexConditions)

            thisFileStrings = re.findall(pattern, contents)

            strings += thisFileStrings

            f.close()


    strings = [*set(strings)] # remove duplicates
    
    strings = [x for x in strings if x not in excludedStrings]

    strings = [x for x in strings if not str.isspace(x)] # remove empty

    strings = [x for x in strings if not all(c.isdigit() or c == '.' for c in x)] # remove strings that are just numbers

    strings = [x for x in strings if (not "{" in x and not "}" in x)] # has curly brace, so definitely not a display string

    strings = [x for x in strings if not x.startswith("pearlcat_")] # likely id, not a display string

    existingStrings = {}
    output = os.path.join(OUTPUT_DIR, "text_{dest}/strings.txt".format(dest = langMap[targetLang]))

    if os.path.exists(output) and preserveExisting:
        f = open(output, "r", encoding='utf-8-sig')

        for line in f.readlines():
            if str.isspace(line): continue

            lineSplit = line.split("|")
            lineSplit[1] = lineSplit[1].removesuffix("\n")

            existingStrings[lineSplit[0]] = lineSplit[1]

        f.close()

    os.makedirs(os.path.dirname(output), exist_ok = True)

    f = open(output, "w", encoding='utf-8-sig')

    preexistingLines = 0
    newlyTranslatedLines = 0

    for i in range(len(strings)):
        #print("[" + str(i + 1) + " / " + str(len(strings)) + "]")
        
        try:
            string = strings[i] 

            # Preserve an existing translation if the string has not changed            
            if string in existingStrings:
                output = string + "|" + existingStrings[string] + "\n\n"
                preexistingLines += 1

            else:
                key = string

                if string in mappedStrings.keys():
                    string = mappedStrings[string]

                translated = "NO_TRANSLATION" if not actuallyTranslate else translator.translate(string, src=SRC, dest=targetLang).text

                output = key + "|" + translated + "\n\n"

                output = re.sub("<line>", "<LINE>", output, flags=re.IGNORECASE)

                if printTranslatedLines:
                    print(output)

                newlyTranslatedLines += 1

            f.write(output)
        
        except Exception as e:
            print("TRANSLATION ERROR: ", e)

    f.close()

    print("Pre-existing lines:", preexistingLines)
    print("Newly translated lines:", newlyTranslatedLines)


toTranslate = {
    "ru",
    "ko",
    "fr",
    "pt",
    "it",
    "de",
    "ja",
    "zh-CN",
    "es",
}

preserveLangMap = [
    "ru",
    "ko",
    "fr",
    "pt",
    "it",
    "de",
    "ja",
    "zh-CN",
    "es",
]

dontTranslate = [
    # "ru",
    # "ko",
    # "fr",
    # "pt",
    # "it",
    # "de",
    # "ja",
    # "zh-CN",
    # "es",
]

for lang in toTranslate:
    Translate(lang, lang in preserveLangMap, lang not in dontTranslate, True)