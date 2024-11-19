using Menu;
using RWCustom;
using System.IO;
using UnityEngine;
using static Pearlcat.Enums;

namespace Pearlcat;

public static class Menu_Helpers
{
    public static int MenuPearlAnimStacker { get; set; }

    public static string SecretPassword { get; set; } = "mira";
    public static int SecretIndex { get; set; }

    public static Color MiraMenuColor { get; } = Custom.hexToColor("9487c9");

    public const string MIRA_SKIP_ID = "PEARLCAT_MIRA_SKIP";


    public static Color MenuPearlColorFilter(this Color color)
    {
        return color;
    }

    public static void UpdateSelectScreen(MenuScene self, MenuIllustration illustration, MenuSceneModule menuSceneModule, MenuIllustrationModule illustrationModule)
    {
        var save = Utils.MiscProgression;
        var fileName = Path.GetFileNameWithoutExtension(illustration.fileName);

        if (illustrationModule.Index == -2)
        {
            var visible = true;

            if (fileName == "flat")
            {
                visible = save.HasPearlpup || (save.IsNewPearlcatSave && ModManager.MSC);
            }
            else if (fileName == "flat_nopup")
            {
                visible = !save.HasPearlpup || (save.IsNewPearlcatSave && !ModManager.MSC);
            }


            if (fileName.Contains("Pearlpup"))
            {
                visible = save.HasPearlpup || (save.IsNewPearlcatSave && ModManager.MSC);
            }

            visible = visible && ((save.HasTrueEnding && fileName.Contains("trueend")) ||
                                  (!save.HasTrueEnding && !fileName.Contains("trueend")));
            illustration.visible = visible;

            if (fileName.Contains("pupheart"))
            {
                UpdatePupHeartIllustration(self, illustration);
            }

            return;
        }

        if (illustrationModule.Index == -1)
        {
            if (menuSceneModule.ActivePearlColor == null)
            {
                illustration.visible = false;
                return;
            }

            if (fileName == "pearlactivehalo")
            {
                illustration.sprite.SetAnchor(Vector2.one * 0.5f);
                illustration.sprite.scale = 0.3f;

                illustration.pos = menuSceneModule.ActivePearlPos;
                return;
            }


            var trueEndPos = new Vector2(690, 490);

            if (save.HasTrueEnding && illustrationModule.InitialPos != trueEndPos)
            {
                illustrationModule.InitialPos = trueEndPos;
                illustrationModule.SetPos = trueEndPos;
                illustrationModule.Vel = Vector2.zero;

                illustration.pos = trueEndPos;
            }


            var activePearlColor = menuSceneModule.ActivePearlColor;

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

            illustrationModule.SetPos = illustrationModule.InitialPos;
            illustrationModule.SetPos.y = illustrationModule.InitialPos.y + Mathf.Sin(MenuPearlAnimStacker / 500.0f) * 25.0f;

            menuSceneModule.ActivePearlPos = illustration.pos;

            return;
        }

        var pearlColors = menuSceneModule.PearlColors;

        var count = pearlColors.Count;
        var i = illustrationModule.Index;

        if (i >= count)
        {
            illustration.visible = false;
            return;
        }

        illustration.visible = true;

        var angleFrameAddition = 0.00675f;
        var radius = 150.0f;
        var origin = new Vector2(675, 400);

        if (save.HasTrueEnding)
        {
            radius = 130.0f;
            angleFrameAddition = 0.0045f;
            origin = new Vector2(675.0f, 350.0f);
        }

        var angle = (i * Mathf.PI * 2.0f / count) + angleFrameAddition * MenuPearlAnimStacker;

        var targetPos = new Vector2(origin.x + Mathf.Cos(angle) * radius, origin.y + Mathf.Sin(angle) * radius);
        illustration.pos = targetPos;

        illustration.sprite.scale = Custom.LerpMap(Mathf.Sin(angle), -1.0f, 1.0f, 0.35f, 0.25f);
        illustration.color = pearlColors[i].MenuPearlColorFilter();
    }

    public static void UpdateSleepScreen(MenuScene self, MenuIllustration illustration, MenuSceneModule menuSceneModule, MenuIllustrationModule illustrationModule)
    {
        var save = Utils.MiscProgression;
        var fileName = Path.GetFileNameWithoutExtension(illustration.fileName);

        illustration.alpha = 1.0f;

        var pearlcatSad = (save.IsPearlpupSick || (!save.HasPearlpup && save.DidHavePearlpup)) && !save.HasTrueEnding;

        if (illustrationModule.Index == -2)
        {
            var visible = true;

            // Flat
            if (fileName == "flat")
            {
                visible = !pearlcatSad && save.HasPearlpup;
            }
            else if (fileName == "flat_sad")
            {
                visible = pearlcatSad;
            }
            else if (fileName == "flat_sick")
            {
                visible = save.HasPearlpup && save.IsPearlpupSick;
            }
            else if (fileName == "flat_nopup")
            {
                visible = !save.DidHavePearlpup;
            }

            // Depth
            if (fileName == "pcat_nopup")
            {
                visible = !save.DidHavePearlpup;
            }
            else if (fileName == "pcat_withpup")
            {
                visible = !pearlcatSad && save.HasPearlpup;
            }
            else if (fileName == "pcat_sad")
            {
                visible = pearlcatSad;
            }
            else if (fileName == "pup")
            {
                visible = save.HasPearlpup && !save.IsPearlpupSick;
            }
            else if (fileName == "pup_sick")
            {
                visible = save.HasPearlpup && save.IsPearlpupSick;
            }
            else if (fileName == "scarf")
            {
                visible = pearlcatSad;
            }
            else if (fileName == "pup_drawings")
            {
                visible = save.HasPearlpup && !save.IsPearlpupSick;
            }
            else if (fileName == "sleep1")
            {
                if (pearlcatSad || save.HasPearlpup)
                {
                    illustration.pos = new(609, 27);
                }
            }

            var trueEndVisible = save.HasTrueEnding == fileName.Contains("trueend") || fileName.Contains("sleep1");

            illustration.visible = visible && trueEndVisible;

            if (fileName.Contains("pupheart"))
            {
                UpdatePupHeartIllustration(self, illustration);
            }

            return;
        }

        if (illustrationModule.Index == -1)
        {

            if (fileName == "pearlactivehalo")
            {
                illustration.sprite.SetAnchor(Vector2.one * 0.5f);
                illustration.sprite.scale = 0.3f;
                illustration.visible = menuSceneModule.ActivePearlColor != null;

                illustration.pos = menuSceneModule.ActivePearlPos;
                return;
            }

            var sadPos = new Vector2(870, 330);

            if (pearlcatSad && illustrationModule.InitialPos != sadPos)
            {
                illustrationModule.InitialPos = sadPos;
                illustrationModule.SetPos = sadPos;
                illustrationModule.Vel = Vector2.zero;

                illustration.pos = sadPos;
            }

            var isPlaceholder = fileName == "pearlactiveplaceholder";
            var activePearlColor = Color.white;

            if (menuSceneModule.ActivePearlColor == null)
            {
                if (isPlaceholder && !save.HasTrueEnding)
                {
                    illustration.visible = true;
                }
                else
                {
                    illustration.visible = false;
                    return;
                }
            }
            else if (isPlaceholder)
            {
                illustration.visible = false;
                return;
            }
            else
            {
                activePearlColor = (Color)menuSceneModule.ActivePearlColor;
            }

            illustration.visible = true;
            illustration.color = MenuPearlColorFilter(activePearlColor);
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

            illustrationModule.SetPos.y =
                illustrationModule.InitialPos.y + Mathf.Sin(MenuPearlAnimStacker / 500.0f) * 25.0f;

            menuSceneModule.ActivePearlPos = illustration.pos;
            return;
        }

        var pearlColors = menuSceneModule.PearlColors;

        var count = pearlColors.Count;
        var i = illustrationModule.Index;

        if (i >= count)
        {
            illustration.visible = false;
            return;
        }

        illustration.visible = true;
        illustration.sprite.scale = 0.35f;
        illustration.color = pearlColors[i].MenuPearlColorFilter();

        illustration.pos.y = illustrationModule.InitialPos.y +
                             Mathf.Sin((MenuPearlAnimStacker + i * 50.0f) / 50.0f) * 25.0f;
    }

    public static void UpdateSickScreen(MenuScene self, MenuIllustration illustration, MenuSceneModule menuSceneModule, MenuIllustrationModule illustrationModule)
    {
        var fileName = Path.GetFileNameWithoutExtension(illustration.fileName);

        if (illustrationModule.Index == -2)
        {
            return;
        }

        if (illustrationModule.Index == -1)
        {
            if (menuSceneModule.ActivePearlColor == null)
            {
                illustration.visible = false;
                return;
            }

            if (fileName == "pearlactivehalo")
            {
                illustration.sprite.SetAnchor(Vector2.one * 0.5f);
                illustration.sprite.scale = 0.3f;

                illustration.pos = menuSceneModule.ActivePearlPos;
                return;
            }

            var activePearlColor = menuSceneModule.ActivePearlColor;

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

        var pearlColors = menuSceneModule.PearlColors;

        var count = pearlColors.Count;
        var i = illustrationModule.Index;

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
        illustration.color = pearlColors[i].MenuPearlColorFilter();
    }

    public static void UpdateAscendedScreen(MenuScene self, MenuIllustration illustration, MenuSceneModule menuSceneModule, MenuIllustrationModule illustrationModule)
    {
        if (illustrationModule.Index == -2)
        {
            var save = Utils.MiscProgression;
            var fileName = Path.GetFileNameWithoutExtension(illustration.fileName);

            var visible = true;

            if (fileName == "pup")
            {
                visible = save.AscendedWithPup;
            }

            visible = visible && ((save.HasTrueEnding && fileName.Contains("trueend")) ||
                                  (!save.HasTrueEnding && !fileName.Contains("trueend")));
            illustration.visible = visible;
            return;
        }

        if (illustrationModule.Index == -1)
        {
            if (menuSceneModule.ActivePearlColor == null)
            {
                illustration.visible = false;
                return;
            }

            var fileName = Path.GetFileNameWithoutExtension(illustration.fileName);

            if (fileName == "pearlactivehalo")
            {
                illustration.sprite.SetAnchor(Vector2.one * 0.5f);
                illustration.sprite.scale = 0.3f;

                illustration.pos = menuSceneModule.ActivePearlPos;
                return;
            }

            var activePearlColor = menuSceneModule.ActivePearlColor;

            illustration.visible = true;
            illustration.color = ((Color)activePearlColor).AscendedPearlColorFilter();
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

        var pearlColors = menuSceneModule.PearlColors;

        var count = pearlColors.Count;
        var i = illustrationModule.Index;

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
        illustration.color = pearlColors[i].AscendedPearlColorFilter();
        //illustration.color = Color.Lerp(pearlColors[i].MenuPearlColorFilter(), new Color32(207, 187, 101, 255), 0.4f);
    }


    public static Color AscendedPearlColorFilter(this Color color)
    {
        //return color;

        Color.RGBToHSV(Color.Lerp(color, Color.white, 0.3f), out var hue, out var sat, out var val);
        return Color.HSVToRGB(hue, sat, val);
    }

    private static void UpdatePupHeartIllustration(MenuScene self, MenuIllustration illustration)
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
        var isCore = fileName == "pupheartcore";

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
                        self.menu.PlaySound(Sounds.Pearlcat_Heartbeat, 0.0f, 0.3f, 1.0f);
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
}
