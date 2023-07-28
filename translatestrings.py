import os
import re

from googletrans import Translator

ROOT_DIR = os.path.join(os.path.join(os.environ['USERPROFILE']), 'Desktop/Pearlcat/src') 
OUTPUT_DIR = os.path.join(os.path.join(os.environ['USERPROFILE']), 'Desktop/Pearlcat') 

EXCLUDE = ["PlayerObjectEffect", "PlayerObjectAnimator", "InventoryHUD", "BackgroundView", "Modules"]

translator = Translator()
strings = []

SRC = "en"
DEST = "es"

for subdir, dirs, files in os.walk(ROOT_DIR):
    for fileName in files:
        if not fileName.endswith(".cs"): continue

        filePath = os.path.join(subdir, fileName)
        if any(x in filePath for x in EXCLUDE): continue

        f = open(filePath, "r")

        contents = f.read()
        thisFileStrings = re.findall(r'(?<![LogWarning(])"(.*?)"', contents)

        strings += thisFileStrings

        f.close()

strings = list(filter(None, strings)) # remove empty
strings = list(filter(lambda x: not (x.startswith("_") or x.startswith(".")), strings)) # trim weird stuff
strings = [*set(strings)] # remove duplicates

# strings = [x.strip() for x in strings]

f = open(os.path.join(OUTPUT_DIR, "strings.txt"), "w", encoding='utf-8-sig')

for i in range(len(strings)):
    string = strings[i]
    
    output = string + "|" + translator.translate(string, src=SRC, dest=DEST).text + "\n"
    f.write(output)

    print("[" + str(i + 1) + " / " + str(len(strings)) + "]")

f.close()