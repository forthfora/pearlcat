import os
import re

from googletrans import Translator

ROOT_DIR = os.path.join(os.path.join(os.environ['USERPROFILE']), 'Desktop/Pearlcat/src') 
OUTPUT_DIR = os.path.join(os.path.join(os.environ['USERPROFILE']), 'Desktop/Pearlcat/text') 

translator = Translator()

SRC = "en"
DEST = "es"

langMap = {
    "fr": "fre",
    "zh-CN": "chi",
    "es": "spa",
    "ru": "rus",
    "pt": "por",
    "ko": "kor",
    "it": "ita",
    "de": "ger",
    "ja": "jap"
}

def Translate(targetLang):
    print("TRANSLATING: " + targetLang)

    strings = [
        "Pearlcat",
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

    output = os.path.join(OUTPUT_DIR, "text_{dest}/strings.txt".format(dest = langMap[targetLang]))
    os.makedirs(os.path.dirname(output), exist_ok=True)

    f = open(output, "w", encoding='utf-8-sig')

    for i in range(len(strings)):
        print("[" + str(i + 1) + " / " + str(len(strings)) + "]")
        try:
            string = strings[i]
            
            output = string + "|" + translator.translate(string, src=SRC, dest=targetLang).text + "\n"
            f.write(output)
        
        except:
            print("TRANSLATION ERROR")

    f.close()

for lang in langMap.keys():
    Translate(lang)