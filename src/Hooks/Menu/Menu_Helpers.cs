using System.Collections.Generic;
using Menu;
using RWCustom;
using System.IO;
using UnityEngine;

namespace Pearlcat;

using IllustrationType = MenuIllustrationModule.IllustrationType;
using Scenes = Enums.Scenes;

public static class Menu_Helpers
{
    public static int MenuPearlAnimStacker { get; set; }

    public static string SecretPassword { get; set; } = "mira";
    public static int SecretIndex { get; set; }


    public const string MIRA_SKIP_ID = "PEARLCAT_MIRA_SKIP";
    public static Color MiraMenuColor { get; } = Custom.hexToColor("9487c9");


    // Allows tags to be specified in an illustration's filename to disable / enable them under specific conditions
    public static void UpdateIllustrationConditionTags(MenuIllustration self)
    {
        var miscProg = Utils.MiscProgression;
        var fileName = Path.GetFileNameWithoutExtension(self.fileName);

        var visible = true;

        // If Pearlpup is alive and with the player
        if (fileName.HasConditionTag("pup", out var c))
        {
            visible &= miscProg.HasPearlpup == c;
        }

        // If true ending achieved
        if (fileName.HasConditionTag("trueend", out c))
        {
            visible &= miscProg.HasTrueEnding == c;
        }

        // If Pearlpup is sick
        if (fileName.HasConditionTag("sick", out c))
        {
            visible &= miscProg.IsPearlpupSick == c;
        }

        // Had pearlpup and lost them or pearlpup is sick
        if (fileName.HasConditionTag("sad", out c))
        {
            visible &= (miscProg.IsPearlpupSick || (!miscProg.HasPearlpup && miscProg.DidHavePearlpup && !miscProg.HasTrueEnding)) == c;
        }

        self.visible = visible;
    }

    public static bool HasConditionTag(this string fileName, string tag, out bool requiredCondition)
    {
        if (fileName.Contains($"({tag})"))
        {
            requiredCondition = true;
            return true;
        }

        if (fileName.Contains($"(!{tag})"))
        {
            requiredCondition = false;
            return true;
        }

        requiredCondition = false;
        return false;
    }


    // Applies behavior specific to a given scenen
    public static void UpdateIllustrationSpecificBehavior(MenuIllustration self)
    {
        var fileName = Path.GetFileNameWithoutExtension(self.fileName);

        // Outer Expanse Ending - fade this scene to black
        if (fileName == "AltOutro10_1")
        {
            if (self.alpha == 1.0f)
            {
                self.alpha = 0.0f;
            }

            self.alpha = Mathf.Lerp(self.alpha, 0.99f, 0.015f);
        }
    }


    // Update dynamic pearls on menu scenes
    public static void UpdateDynamicMenuSceneIllustration(MenuScene self, MenuIllustration illustration, MenuSceneModule menuSceneModule, MenuIllustrationModule illustrationModule)
    {
        if (self.sceneID == Scenes.Slugcat_Pearlcat)
        {
            UpdateSelectScreenIllustration(self, illustration, menuSceneModule, illustrationModule);
        }

        if (self.sceneID == Scenes.Slugcat_Pearlcat_Sleep)
        {
            UpdateSleepScreenIllustration(self, illustration, menuSceneModule, illustrationModule);
        }

        if (self.sceneID == Scenes.Slugcat_Pearlcat_Ascended)
        {
            UpdateAscendedScreenIllustration(self, illustration, menuSceneModule, illustrationModule);
        }

        if (self.sceneID == Scenes.Slugcat_Pearlcat_Sick)
        {
            UpdateSickScreenIllustration(self, illustration, menuSceneModule, illustrationModule);
        }

        if (illustrationModule.Type == IllustrationType.PearlHeart)
        {
            UpdatePupHeartIllustration(self, illustration);
        }
    }

    public static void UpdateSelectScreenIllustration(MenuScene self, MenuIllustration illustration, MenuSceneModule menuSceneModule, MenuIllustrationModule illustrationModule)
    {
        var miscProg = Utils.MiscProgression;

        if (illustrationModule.Type == IllustrationType.PearlActive)
        {
            var trueEndPos = new Vector2(690, 490);

            if (miscProg.HasTrueEnding && illustrationModule.InitialPos != trueEndPos)
            {
                illustrationModule.InitialPos = trueEndPos;
                illustrationModule.SetPos = trueEndPos;
                illustrationModule.Vel = Vector2.zero;

                illustration.pos = trueEndPos;
            }


            illustration.visible = true;
            illustration.color = menuSceneModule.ActivePearl?.GetPearlColor() ?? Color.white;
            illustration.sprite.scale = 0.3f;

            var pos = illustration.pos;
            var spritePos = illustration.sprite.GetPosition();
            var mousePos = self.menu.mousePosition;

            var setPos = illustrationModule.SetPos;

            if (Custom.Dist(spritePos, mousePos) < 30.0f && Custom.Dist(pos, setPos) < 120.0f)
            {
                illustrationModule.Vel += (spritePos - mousePos).normalized * 2.0f;
            }

            var dir = (setPos - pos).normalized;
            var dist = Custom.Dist(setPos, pos);
            var speed = Custom.LerpMap(dist, 0.0f, 5.0f, 0.1f, 1.0f);

            illustrationModule.Vel *= Custom.LerpMap(illustrationModule.Vel.magnitude, 2.0f, 0.5f, 0.97f, 0.5f);
            illustrationModule.Vel += dir * speed;

            illustration.pos += illustrationModule.Vel;

            illustrationModule.SetPos = illustrationModule.InitialPos;
            illustrationModule.SetPos.y = illustrationModule.InitialPos.y + Mathf.Sin(MenuPearlAnimStacker / 500.0f) * 25.0f;

            menuSceneModule.ActivePearlPos = illustration.pos;
            return;
        }

        if (illustrationModule.Type == IllustrationType.PearlActiveHalo)
        {
            illustration.sprite.SetAnchor(Vector2.one * 0.5f);
            illustration.sprite.scale = 0.3f;
            illustration.pos = menuSceneModule.ActivePearlPos;
            return;
        }

        if (illustrationModule.Type == IllustrationType.PearlNonActive)
        {
            var pearls = menuSceneModule.NonActivePearls;

            var count = pearls.Count;
            var i = illustrationModule.NonActivePearlIndex;

            if (i >= count)
            {
                illustration.visible = false;
                return;
            }

            illustration.visible = true;

            var angleFrameAddition = 0.00675f;
            var radius = 150.0f;
            var origin = new Vector2(675, 400);

            if (miscProg.HasTrueEnding)
            {
                radius = 130.0f;
                angleFrameAddition = 0.0045f;
                origin = new Vector2(675.0f, 350.0f);
            }

            var angle = (i * Mathf.PI * 2.0f / count) + angleFrameAddition * MenuPearlAnimStacker;

            var targetPos = new Vector2(origin.x + Mathf.Cos(angle) * radius, origin.y + Mathf.Sin(angle) * radius);
            illustration.pos = targetPos;

            illustration.sprite.scale = Custom.LerpMap(Mathf.Sin(angle), -1.0f, 1.0f, 0.35f, 0.25f);
            illustration.color = pearls[i].GetPearlColor();
            return;
        }
    }

    public static void UpdateSleepScreenIllustration(MenuScene self, MenuIllustration illustration, MenuSceneModule menuSceneModule, MenuIllustrationModule illustrationModule)
    {
        var miscProg = Utils.MiscProgression;
        var fileName = Path.GetFileNameWithoutExtension(illustration.fileName);

        illustration.alpha = 1.0f;

        var pearlcatSad = (miscProg.IsPearlpupSick || (!miscProg.HasPearlpup && miscProg.DidHavePearlpup && !miscProg.HasTrueEnding));

        if (illustrationModule.Type == IllustrationType.Default)
        {
            // Shift the grass over so it doesn't cover pearlpup
            if (fileName == "1")
            {
                if (pearlcatSad || miscProg.HasPearlpup)
                {
                    illustration.pos = new(609, 27);
                }
            }
            return;
        }

        if (illustrationModule.Type == IllustrationType.PearlActive || illustrationModule.Type == IllustrationType.PearlPlaceHolder)
        {
            var sadPos = new Vector2(870, 330);

            if (pearlcatSad && illustrationModule.InitialPos != sadPos)
            {
                illustrationModule.InitialPos = sadPos;
                illustrationModule.SetPos = sadPos;
                illustrationModule.Vel = Vector2.zero;

                illustration.pos = sadPos;
            }

            var isPlaceholder = illustrationModule.Type == IllustrationType.PearlPlaceHolder;

            if (isPlaceholder)
            {
                illustration.visible = !miscProg.HasTrueEnding;
            }

            var color = menuSceneModule.ActivePearl?.GetPearlColor() ?? Color.white;

            illustration.visible = true;
            illustration.color = color;
            illustration.sprite.scale = isPlaceholder ? 1.0f : 0.3f;

            var pos = illustration.pos;
            var spritePos = illustration.sprite.GetPosition();
            var mousePos = self.menu.mousePosition;

            if (Custom.Dist(spritePos, mousePos) < 30.0f && Custom.Dist(pos, illustrationModule.SetPos) < 90.0f)
            {
                illustrationModule.Vel += (spritePos - mousePos).normalized * 1.5f;
            }

            var dir = (illustrationModule.SetPos - pos).normalized;
            var dist = Custom.Dist(illustrationModule.SetPos, pos);
            var speed = Custom.LerpMap(dist, 0.0f, 5.0f, 0.1f, 1.0f);

            illustrationModule.Vel *= Custom.LerpMap(illustrationModule.Vel.magnitude, 2.0f, 0.5f, 0.97f, 0.5f);
            illustrationModule.Vel += dir * speed;

            illustration.pos += illustrationModule.Vel;

            illustrationModule.SetPos.y = illustrationModule.InitialPos.y + Mathf.Sin(MenuPearlAnimStacker / 500.0f) * 25.0f;

            menuSceneModule.ActivePearlPos = illustration.pos;
            return;
        }

        if (illustrationModule.Type == IllustrationType.PearlActiveHalo)
        {
            illustration.sprite.SetAnchor(Vector2.one * 0.5f);
            illustration.sprite.scale = 0.3f;
            illustration.pos = menuSceneModule.ActivePearlPos;
            return;
        }

        if (illustrationModule.Type == IllustrationType.PearlActiveHalo)
        {
            var pearls = menuSceneModule.NonActivePearls;

            var count = pearls.Count;
            var i = illustrationModule.NonActivePearlIndex;

            if (i >= count)
            {
                illustration.visible = false;
                return;
            }

            illustration.visible = true;
            illustration.sprite.scale = 0.35f;
            illustration.color = pearls[i].GetPearlColor();

            illustration.pos.y = illustrationModule.InitialPos.y + Mathf.Sin((MenuPearlAnimStacker + i * 50.0f) / 50.0f) * 25.0f;
            return;
        }
    }

    public static void UpdateSickScreenIllustration(MenuScene self, MenuIllustration illustration, MenuSceneModule menuSceneModule, MenuIllustrationModule illustrationModule)
    {
        if (illustrationModule.Type == IllustrationType.PearlActive)
        {
            var activePearlColor = menuSceneModule.ActivePearl?.GetPearlColor() ?? Color.white;

            illustration.visible = true;
            illustration.color = (Color)activePearlColor;
            illustration.sprite.scale = 0.3f;


            var pos = illustration.pos;
            var spritePos = illustration.sprite.GetPosition();
            var mousePos = self.menu.mousePosition;

            var setPos = illustrationModule.SetPos;

            if (Custom.Dist(spritePos, mousePos) < 30.0f && Custom.Dist(pos, setPos) < 120.0f)
            {
                illustrationModule.Vel += (spritePos - mousePos).normalized * 2.0f;
            }


            var dir = (setPos - pos).normalized;
            var dist = Custom.Dist(setPos, pos);
            var speed = Custom.LerpMap(dist, 0.0f, 5.0f, 0.1f, 1.0f);

            illustrationModule.Vel *= Custom.LerpMap(illustrationModule.Vel.magnitude, 2.0f, 0.5f, 0.97f, 0.5f);
            illustrationModule.Vel += dir * speed;

            illustration.pos += illustrationModule.Vel;

            illustrationModule.SetPos.y = illustrationModule.InitialPos.y + Mathf.Sin(MenuPearlAnimStacker / 500.0f) * 25.0f;
            menuSceneModule.ActivePearlPos = illustration.pos;
            return;
        }

        if (illustrationModule.Type == IllustrationType.PearlActiveHalo)
        {
            illustration.sprite.SetAnchor(Vector2.one * 0.5f);
            illustration.sprite.scale = 0.3f;
            illustration.pos = menuSceneModule.ActivePearlPos;
            return;
        }

        if (illustrationModule.Type == IllustrationType.PearlNonActive)
        {
            var pearls = menuSceneModule.NonActivePearls;

            var count = pearls.Count;
            var i = illustrationModule.NonActivePearlIndex;

            if (i >= count)
            {
                illustration.visible = false;
                return;
            }

            illustration.visible = true;

            var angleFrameAddition = 0.0045f;
            var radius = 90.0f;
            var origin = new Vector2(650, 490);

            var angle = (i * Mathf.PI * 2.0f / count) + angleFrameAddition * MenuPearlAnimStacker;

            var targetPos = new Vector2(origin.x + Mathf.Cos(angle) * radius * 1.7f, origin.y + Mathf.Sin(angle) * radius);
            illustration.pos = targetPos;

            illustration.sprite.scale = Custom.LerpMap(Mathf.Sin(angle), 1.0f, -1.0f, 0.2f, 0.3f);
            illustration.alpha = 1.0f;
            illustration.color = pearls[i].GetPearlColor();
        }
    }

    public static void UpdateAscendedScreenIllustration(MenuScene self, MenuIllustration illustration, MenuSceneModule menuSceneModule, MenuIllustrationModule illustrationModule)
    {
        if (illustrationModule.Type == IllustrationType.PearlActive)
        {
            var activePearlColor = menuSceneModule.ActivePearl;

            illustration.visible = true;
            illustration.color = (activePearlColor?.GetPearlColor() ?? Color.white).GetAscendScenePearlColor();
            illustration.sprite.scale = 0.25f;
            illustration.alpha = 1.0f;

            var pos = illustration.pos;
            var spritePos = illustration.sprite.GetPosition();
            var mousePos = self.menu.mousePosition;

            if (Custom.Dist(spritePos, mousePos) < 30.0f && Custom.Dist(pos, illustrationModule.SetPos) < 120.0f)
            {
                illustrationModule.Vel += (spritePos - mousePos).normalized * 2.0f;
            }


            var dir = (illustrationModule.SetPos - pos).normalized;
            var dist = Custom.Dist(illustrationModule.SetPos, pos);
            var speed = Custom.LerpMap(dist, 0.0f, 5.0f, 0.1f, 1.0f);

            illustrationModule.Vel *= Custom.LerpMap(illustrationModule.Vel.magnitude, 2.0f, 0.5f, 0.97f, 0.5f);
            illustrationModule.Vel += dir * speed;

            illustration.pos += illustrationModule.Vel;

            illustrationModule.SetPos.y = illustrationModule.InitialPos.y + Mathf.Sin(MenuPearlAnimStacker / 500.0f) * 25.0f;
            menuSceneModule.ActivePearlPos = illustration.pos;
            return;
        }

        if (illustrationModule.Type == IllustrationType.PearlActiveHalo)
        {
            illustration.sprite.SetAnchor(Vector2.one * 0.5f);
            illustration.sprite.scale = 0.3f;
            illustration.pos = menuSceneModule.ActivePearlPos;
            return;
        }

        if (illustrationModule.Type == IllustrationType.PearlNonActive)
        {
            var pearls = menuSceneModule.NonActivePearls;

            var count = pearls.Count;
            var i = illustrationModule.NonActivePearlIndex;

            if (i >= count)
            {
                illustration.visible = false;
                return;
            }

            illustration.visible = true;

            var angleFrameAddition = 0.0045f;
            var radius = 90.0f;
            var origin = new Vector2(680, 360);

            var angle = (i * Mathf.PI * 2.0f / count) + angleFrameAddition * MenuPearlAnimStacker;

            var targetPos = new Vector2(origin.x + Mathf.Cos(angle) * radius * 2.0f, origin.y + Mathf.Sin(angle) * radius);
            illustration.pos = targetPos;

            illustration.sprite.scale = Custom.LerpMap(Mathf.Cos(angle) + Mathf.Sin(angle), 2.0f, 0.0f, 0.2f, 0.3f);
            illustration.alpha = 1.0f;
            illustration.color = pearls[i].GetPearlColor().GetAscendScenePearlColor();
        }
    }


    public static void DetermineIllustrationPosDepthLayer(MenuIllustration illustration, MenuScene menuScene, MenuIllustrationModule menuIllustrationModule)
    {
        var sceneId = menuScene.sceneID;
        var illustrationType = menuIllustrationModule.Type;

        if (sceneId == Scenes.Slugcat_Pearlcat)
        {

        }   
    }

    public static void UpdatePupHeartIllustration(MenuScene self, MenuIllustration illustration)
    {
        var miscProg = Utils.MiscProgression;

        if (!miscProg.HasTrueEnding)
        {
            illustration.visible = false;
            return;
        }

        var fileName = Path.GetFileNameWithoutExtension(illustration.fileName);
        illustration.visible = true;

        var initialScale = 0.3f;
        var isCore = fileName == "pearlcat_menupearl_heartcore";

        if (illustration.sprite.scale == 1.0f)
        {
            illustration.sprite.scale = initialScale;
            illustration.sprite.SetAnchor(new Vector2(0.5f, 0.5f));
        }

        var currentScale = illustration.sprite.scale;

        const int beatFrequency = 80;
        const int coreBeatOffset = 10;

        var beat = MenuPearlAnimStacker % beatFrequency == 0;
        var coreBeat = (MenuPearlAnimStacker - coreBeatOffset) % beatFrequency == 0;

        if (isCore)
        {
            if (coreBeat)
            {
                illustration.sprite.scale = 0.4f;
            }
            else
            {
                illustration.sprite.scale = Mathf.Lerp(currentScale, initialScale, 0.1f);
            }
        }
        else
        {
            if (beat)
            {
                illustration.sprite.scale = 0.45f;

                if (self.menu is SlugcatSelectMenu menu)
                {
                    var page = menu.slugcatPages[menu.slugcatPageIndex];

                    if (page.slugcatNumber == Enums.Pearlcat)
                    {
                        self.menu.PlaySound(Enums.Sounds.Pearlcat_Heartbeat, 0.0f, 0.3f, 1.0f);
                    }
                }
            }
            else if (coreBeat)
            {
                illustration.sprite.scale = 0.4f;
            }
            else
            {
                illustration.sprite.scale = Mathf.Lerp(currentScale, initialScale, 0.1f);
            }
        }
    }


    public static SaveMiscProgression.StoredPearlData? PearlTypeToStoredData(this DataPearl.AbstractDataPearl.DataPearlType? dataPearlType)
    {
        if (dataPearlType is null)
        {
            return null;
        }

        return new SaveMiscProgression.StoredPearlData
        {
            DataPearlType = dataPearlType.value,
        };
    }

    public static List<SaveMiscProgression.StoredPearlData> PearlTypeToStoredData(this List<DataPearl.AbstractDataPearl.DataPearlType> pearlTypeList)
    {
        var storedDataList = new List<SaveMiscProgression.StoredPearlData>();

        foreach (var dataPearlType in pearlTypeList)
        {
            var pearlData = dataPearlType.PearlTypeToStoredData();

            if (pearlData is null)
            {
                continue;
            }

            storedDataList.Add(pearlData);
        }

        return storedDataList;
    }


    public static string GetPearlIllustration(string menuPearlType, string appendTag)
    {
        var uniqueIllustration = GetUniquePearlIllustration(menuPearlType);

        if (uniqueIllustration is not null)
        {
            return uniqueIllustration;
        }

        var randState = Random.state;
        Random.InitState(menuPearlType.GetHashCode());

        var index = Random.Range(0, 10);

        var illustration = $"pearlcat_menupearl_{index}{appendTag}";

        Random.state = randState;

        return illustration;
    }

    public static string? GetUniquePearlIllustration(string menuPearlType)
    {
        if (menuPearlType == "RM_Pearlcat" || menuPearlType == "RM")
        {
            return "unique_rm_pearlcat";
        }

        if (menuPearlType == "SS_Pearlcat")
        {
            return "unique_ss_pearlcat";
        }

        if (menuPearlType == "CW_Pearlcat")
        {
            return "unique_cw_pearlcat";
        }

        return null;
    }


    public static Color GetAscendScenePearlColor(this Color color)
    {
        Color.RGBToHSV(Color.Lerp(color, Color.white, 0.3f), out var hue, out var sat, out var val);

        return Color.HSVToRGB(hue, sat, val);
    }

    public static int GetPearlDisplayLimit(MenuScene.SceneID sceneID)
    {
        if (sceneID == Scenes.Slugcat_Pearlcat || sceneID == Scenes.Slugcat_Pearlcat_Ascended || sceneID == Scenes.Slugcat_Pearlcat_Sick)
        {
            return 100;
        }

        return 11;
    }
}
