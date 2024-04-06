import os

from googletrans import Translator

ROOT_DIR = os.path.join(os.path.join(os.environ['USERPROFILE']), 'Desktop/Pearlcat/text/text_eng') 
OUTPUT_DIR = os.path.join(os.path.join(os.environ['USERPROFILE']), 'Desktop/Pearlcat/text') 

translator = Translator()

SRC = "en"
DEST = "es"

langMap = {
    "fr": "fre",
    # "zh-CN": "chi",
    # "es": "spa",
    "ru": "rus",
    "pt": "por",
    "ko": "kor",
    "it": "ita",
    "de": "ger",
    "ja": "jap"
}

def Translate(targetLang):
    print("TRANSLATING: " + targetLang)

    output = os.path.join(OUTPUT_DIR, "text_{dest}".format(dest = langMap[targetLang]))
    os.makedirs(output, exist_ok=True)

    for subdir, dirs, files in os.walk(ROOT_DIR):
        for i in range(len(files)):
            fileName = files[i]
            if not fileName.endswith(".txt"): continue

            print("[" + str(i + 1) + " / " + str(len(files)) + "]" + " (" + fileName + ")")

            try:
                filePath = os.path.join(subdir, fileName)
                f = open(filePath, "r")

                contents = f.readlines()
                f.close()

                firstLine = contents[0]
                contents.pop(0)

                contents = ''.join(contents)
                translated = translator.translate(contents, src=SRC, dest=targetLang).text

                text = firstLine + translated

                f = open(os.path.join(output, fileName), "w", encoding='utf-8-sig')
                
                f.write(text)
                f.close()
            
            except:
                print("TRANSLATION ERROR")

for lang in langMap.keys():
    Translate(lang)