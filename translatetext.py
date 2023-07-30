import os
import re

from googletrans import Translator

ROOT_DIR = os.path.join(os.path.join(os.environ['USERPROFILE']), 'Desktop/Pearlcat/text/text_eng') 
OUTPUT_DIR = os.path.join(os.path.join(os.environ['USERPROFILE']), 'Desktop/Pearlcat/text') 

translator = Translator()
strings = []

SRC = "en"
DEST = "es"

langMap = {
    "fr": "fre",
    "cn-zh": "chi",
    "es": "spa",
    "ru": "rus",
    "pt": "por",
    "ko": "kor",
    "it": "ita",
    "de": "ger",
    "jp": "jap"
}

output = os.path.join(OUTPUT_DIR, "text_{dest}".format(dest = langMap[DEST]))
os.makedirs(os.path.dirname(output))

for subdir, dirs, files in os.walk(ROOT_DIR):
    for fileName in files:
        if not fileName.endswith(".txt"): continue

        filePath = os.path.join(subdir, fileName)
        f = open(filePath, "r")

        contents = f.read()
        f.close()

        f = open(os.path.join(output, fileName), "w", encoding='utf-8-sig')

        f.write(translator.translate(contents, src=SRC, dest=DEST).text)
        f.close()
        break