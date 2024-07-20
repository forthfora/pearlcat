import os

from googletrans import Translator

#ROOT_DIR = os.path.join(os.path.join(os.environ['USERPROFILE']), 'Desktop/Pearlcat/text/text_eng') 
#OUTPUT_DIR = os.path.join(os.path.join(os.environ['USERPROFILE']), 'Desktop/Pearlcat/text') 
ROOT_DIR = os.path.join(os.path.join(os.environ['USERPROFILE']), 'Desktop/Pearlcat/cw_text/Text_Eng') 
OUTPUT_DIR = os.path.join(os.path.join(os.environ['USERPROFILE']), 'Desktop/Pearlcat/cw_text') 

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

                translatedList = []

                for line in contents:
                    if line == '\n':
                        translatedList.append(line)
                        continue
                    
                    # If the file uses the standard '0-1' first line format, we should skip that
                    # if line == contents[0]:
                    #    translatedList.append(line)
                    #    continue

                    # Skip special events
                    if line.startswith("SPECIAL :"):
                        translatedList.append(line)
                        continue

                    translated = translator.translate(line, src=SRC, dest=targetLang).text

                    translatedList.append(translated + '\n')

                text = ''.join(translatedList)

                f = open(os.path.join(output, fileName), "w", encoding='utf-8-sig')
                
                f.write(text)
                f.close()
            
            except Exception as e:
                print("TRANSLATION ERROR: ", e)

for lang in langMap.keys():
    Translate(lang)