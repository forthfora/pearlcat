import os
import re

from googletrans import Translator

ROOT_DIR = os.path.join(os.path.join(os.environ['USERPROFILE']), 'Desktop/Pearlcat/src') 
OUTPUT_DIR = os.path.join(os.path.join(os.environ['USERPROFILE']), 'Desktop/Pearlcat/text') 

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

def Translate(targetLang, preserveExisting, actuallyTranslate = True, printTranslatedLines = False):
    print("TRANSLATING: " + targetLang)

    strings = [
        "Pearlcat",
        "PEARLCAT",
        "The Pearlcat",
        "A curious scholar with the unusual ability to harness pearls to their advantage.<LINE><LINE>Configure inputs, difficulty, cheats and more via the Remix config!<LINE><LINE>More Slugcats is optional, but strongly recommended.",
        "A scholar of obscure origin, armed with an enigmatic energy and a thirst for knowledge.<LINE>Physically frail, but with pearls as your ally - what will you discover on your travels?",
        "Their curiosity insatiable, Pearlcat ventures out once more in<LINE>pursuit of expanding their collection",
        "Transit System"
    ]

    for subdir, dirs, files in os.walk(ROOT_DIR):
        for fileName in files:
            if not fileName.endswith(".cs"): continue

            filePath = os.path.join(subdir, fileName)

            f = open(filePath, "r")

            contents = f.read()
            thisFileStrings = re.findall('"(.*?)"', contents)

            strings += thisFileStrings

            f.close()

    # strings = list(filter(None, strings)) # remove empty
    # strings = list(filter(lambda x: not (x.startswith("_") or x.startswith(".")), strings)) # trim weird stuff
            
    strings = [*set(strings)] # remove duplicates

    # strings = [x.strip() for x in strings]

    existingStrings = {}
    output = os.path.join(OUTPUT_DIR, "text_{dest}/strings.txt".format(dest = langMap[targetLang]))

    if os.path.exists(output) and preserveExisting:
        f = open(output, "r", encoding='utf-8-sig')

        for line in f.readlines():
            lineSplit = line.split("|")
            lineSplit[1] = lineSplit[1].removesuffix("\n")

            existingStrings[lineSplit[0]] = lineSplit[1]

        f.close()

    os.makedirs(os.path.dirname(output), exist_ok = True)

    f = open(output, "w", encoding='utf-8-sig')

    for i in range(len(strings)):
        print("[" + str(i + 1) + " / " + str(len(strings)) + "]")
        
        try:
            string = strings[i] 
            
            if string in existingStrings:
                output = string + "|" + existingStrings[string] + "\n"
            
            else:
                translated = "NO_TRANSLATION" if not actuallyTranslate else translator.translate(string, src=SRC, dest=targetLang).text
                
                output = string + "|" + translated + "\n"

                if printTranslatedLines:
                    print(output)
            
            f.write(output)
        
        except Exception as e:
            print("TRANSLATION ERROR: ", e)

    f.close()



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