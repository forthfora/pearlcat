using System;
using System.Collections.Generic;
using Menu;
using RWCustom;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Pearlcat;

using IllustrationType = MenuIllustrationModule.IllustrationType;
using Pearls = Enums.Pearls;
using Scenes = Enums.Scenes;

public static class Menu_Helpers
{
    public static int MenuPearlAnimStacker { get; set; }
    public static List<MenuScene.SceneID> MenuPearlScenes { get; } =
    [
        Scenes.Slugcat_Pearlcat,
        Scenes.Slugcat_Pearlcat_Ascended,
        Scenes.Slugcat_Pearlcat_Sick,

        Scenes.Slugcat_Pearlcat_Sleep,

        Scenes.Dream_Pearlcat_Pebbles,
        Scenes.Dream_Pearlcat_Moon,
    ];

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

        // If Pearlpup is alive and with the player (or a new save)
        if (fileName.HasConditionTag("pup", out var c))
        {
            visible &= (miscProg.HasPearlpup || miscProg.HasDeadPearlpup || (miscProg.IsNewPearlcatSave && ModManager.MSC)) == c;
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

        // Had pearlpup and lost them, pearlpup is in the shelter but dead, or pearlpup is sick
        if (fileName.HasConditionTag("sad", out c))
        {
            visible &= (miscProg.IsPearlpupSick || miscProg.HasDeadPearlpup || (!miscProg.HasPearlpup && miscProg.DidHavePearlpup)) == c;
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


    // Applies behavior specific to a given illustration
    public static void UpdateIllustrationSpecificBehavior(MenuIllustration self)
    {
        var filePath = self.fileName;

        // Outer Expanse Ending - fade this scene to black
        if (filePath.EndsWith(Path.Combine("pearlcat_outro_sick", "10", "1")) || filePath.EndsWith(Path.Combine("pearlcat_outro_sick", "10", "flat_1")))
        {
            if (self.alpha == 1.0f)
            {
                self.alpha = 0.0f;
            }

            self.alpha = Mathf.Lerp(self.alpha, 0.99f, 0.015f);
        }
    }


    // Initialize dynamic pearls on menu scenes
    public static void InitMenuPearls(MenuScene self, MenuScene.SceneID sceneID)
    {
        if (!MenuPearlScenes.Contains(sceneID))
        {
            return;
        }

        var miscProg = Utils.MiscProgression;

        if (sceneID == Scenes.Dream_Pearlcat_Moon)
        {
            ModuleManager.MenuSceneData.Add(self, new([], miscProg.StoredActivePearl));
        }
        else if (sceneID == Scenes.Dream_Pearlcat_Pebbles)
        {
            List<SaveMiscProgression.StoredPearlData> pearls = [];

            var randState = Random.state;
            Random.InitState((int)DateTime.Now.Ticks);

            for (var i = 0; i < 10; i++)
            {
                var pebblesPearlType = Random.Range(0, 3);

                var pearlData = new SaveMiscProgression.StoredPearlData()
                {
                    DataPearlType = DataPearl.AbstractDataPearl.DataPearlType.PebblesPearl.value,
                    PebblesPearlType = pebblesPearlType,
                };

                pearls.Add(pearlData);
            }

            Random.state = randState;

            ModuleManager.MenuSceneData.Add(self, new(pearls, miscProg.StoredActivePearl));
        }
        else if (ModOptions.InventoryOverride || (miscProg.IsNewPearlcatSave && ModOptions.StartingInventoryOverride))
        {
            var pearls = ModOptions.GetOverridenInventory(true);
            var activePearl = pearls.FirstOrDefault();

            var displayLimit = GetPearlDisplayLimit(sceneID);

            if (pearls.Count > displayLimit)
            {
                pearls.RemoveRange(displayLimit, pearls.Count - displayLimit);
            }

            pearls.Remove(activePearl);

            ModuleManager.MenuSceneData.Add(self, new(pearls.PearlTypeToStoredData(), activePearl?.PearlTypeToStoredData()));
        }
        else if (miscProg.IsNewPearlcatSave)
        {
            List<DataPearl.AbstractDataPearl.DataPearlType> pearls =
            [
                Pearls.AS_PearlBlue,
                Pearls.AS_PearlYellow,
                Pearls.AS_PearlRed,
                Pearls.AS_PearlGreen,
                Pearls.AS_PearlBlack,
            ];

            var activePearl = Pearls.RM_Pearlcat;

            // Replace the active pearl with one of the pearl colors (+ remove it from the list)
            if (miscProg.HasTrueEnding)
            {
                activePearl = Pearls.AS_PearlRed;
                pearls.Remove(activePearl);
            }

            ModuleManager.MenuSceneData.Add(self, new(pearls.PearlTypeToStoredData(), activePearl.PearlTypeToStoredData()));
        }
        else
        {
            List<SaveMiscProgression.StoredPearlData> pearls = [];
            var displayLimit = GetPearlDisplayLimit(sceneID);

            for (var i = 0; i < miscProg.StoredNonActivePearls.Count; i++)
            {
                var pearlData = miscProg.StoredNonActivePearls[i];

                if (i >= displayLimit)
                {
                    break;
                }

                pearls.Add(pearlData);
            }

            ModuleManager.MenuSceneData.Add(self, new(pearls, miscProg.StoredActivePearl));
        }
    }

    public static void InitMenuPearlIllustrations(MenuScene self, MenuScene.SceneID sceneID)
    {
        if (!self.TryGetModule(out var module))
        {
            return;
        }

        var miscProg = Utils.MiscProgression;

        var illustrationFolder = Path.Combine("illustrations", "pearlcat_menupearls");
        var appendTag = "";
        var flatMode = self.flatMode;

        if (sceneID == Scenes.Slugcat_Pearlcat_Ascended)
        {
            appendTag = "_(ascendscene)";
        }

        // Called First => Last, Nearest Layer => Furthest Layer

        // Active Pearl
        if (module.ActivePearl is not null)
        {
            var haloIllustration = flatMode ? new MenuIllustration(self.menu, self, illustrationFolder, "halo" + appendTag, Vector2.zero, false, true)
                : new MenuDepthIllustration(self.menu, self, illustrationFolder, "halo" + appendTag, Vector2.zero, -1.0f, MenuDepthIllustration.MenuShader.Basic);

            self.AddIllustration(haloIllustration);

            haloIllustration.GetModule().Init(haloIllustration, IllustrationType.PearlActiveHalo);


            var illustration = flatMode ? new MenuIllustration(self.menu, self, illustrationFolder, GetPearlIllustration(module.ActivePearl.DataPearlType, appendTag), Vector2.zero, false, true)
                : new MenuDepthIllustration(self.menu, self, illustrationFolder, GetPearlIllustration(module.ActivePearl.DataPearlType, appendTag), Vector2.zero, -1.0f, MenuDepthIllustration.MenuShader.Basic);

            self.AddIllustration(illustration);

            var isUnique = GetUniquePearlIllustration(module.ActivePearl.DataPearlType) is not null;

            illustration.GetModule().Init(illustration, IllustrationType.PearlActive, hasUniquePearlIllustration: isUnique);
        }

        // Pearlpup heart
        if (miscProg.HasTrueEnding)
        {
            if (sceneID == Scenes.Slugcat_Pearlcat_Sleep || sceneID == Scenes.Slugcat_Pearlcat)
            {
                var heartIllustration = flatMode ? new MenuIllustration(self.menu, self, illustrationFolder, "heart", Vector2.zero, false, true)
                    : new MenuDepthIllustration(self.menu, self, illustrationFolder, "heart", Vector2.zero, -1.0f, MenuDepthIllustration.MenuShader.Basic);

                self.AddIllustration(heartIllustration);

                heartIllustration.GetModule().Init(heartIllustration, IllustrationType.PearlHeart);


                var heartCoreIllustration = flatMode ? new MenuIllustration(self.menu, self, illustrationFolder, "heartcore", Vector2.zero, false, true)
                    : new MenuDepthIllustration(self.menu, self, illustrationFolder, "heartcore", Vector2.zero, -1.0f, MenuDepthIllustration.MenuShader.Basic);

                self.AddIllustration(heartCoreIllustration);

                heartCoreIllustration.GetModule().Init(heartCoreIllustration, IllustrationType.PearlHeartCore);
            }
        }

        // Placeholder for when Pearlcat sleeps with no pearls stored
        if (sceneID == Scenes.Slugcat_Pearlcat_Sleep && !miscProg.IsNewPearlcatSave && !miscProg.HasTrueEnding && miscProg.StoredActivePearl is null)
        {
            var illustration = flatMode ? new MenuIllustration(self.menu, self, illustrationFolder, "placeholder", Vector2.zero, false, true)
                : new MenuDepthIllustration(self.menu, self, illustrationFolder, "placeholder", Vector2.zero, -1.0f, MenuDepthIllustration.MenuShader.Basic);

            self.AddIllustration(illustration);

            illustration.GetModule().Init(illustration, IllustrationType.PearlPlaceHolder);
        }

        // Non-Active Pearls
        for (var i = 0; i < module.NonActivePearls.Count; i++)
        {
            var pearlData = module.NonActivePearls[i];

            var illustration = flatMode
                ? new MenuIllustration(self.menu, self, illustrationFolder, GetPearlIllustration(pearlData.DataPearlType, appendTag), Vector2.zero, false, true)
                : new MenuDepthIllustration(self.menu, self, illustrationFolder, GetPearlIllustration(pearlData.DataPearlType, appendTag), Vector2.zero, -1.0f, MenuDepthIllustration.MenuShader.Basic);

            self.AddIllustration(illustration);

            var isUnique = GetUniquePearlIllustration(pearlData.DataPearlType) is not null;

            illustration.GetModule().Init(illustration, IllustrationType.PearlNonActive, i, isUnique);
        }
    }


    // Update dynamic pearls on menu scenes
    public static void UpdateDynamicMenuSceneIllustration(MenuScene self, MenuIllustration illustration, MenuSceneModule menuSceneModule, MenuIllustrationModule illustrationModule)
    {
        if (self.sceneID == Scenes.Slugcat_Pearlcat)
        {
            UpdateSelectScreenIllustration(self, illustration, menuSceneModule, illustrationModule);
        }
        else if (self.sceneID == Scenes.Slugcat_Pearlcat_Sleep)
        {
            UpdateSleepScreenIllustration(self, illustration, menuSceneModule, illustrationModule);
        }
        else if (self.sceneID == Scenes.Slugcat_Pearlcat_Ascended)
        {
            UpdateAscendedScreenIllustration(self, illustration, menuSceneModule, illustrationModule);
        }
        else if (self.sceneID == Scenes.Slugcat_Pearlcat_Sick)
        {
            UpdateSickScreenIllustration(self, illustration, menuSceneModule, illustrationModule);
        }
        else if (self.sceneID == Scenes.Dream_Pearlcat_Pebbles)
        {
            UpdatePebblesDreamIllustration(self, illustration, menuSceneModule, illustrationModule);
        }
        else if (self.sceneID == Scenes.Dream_Pearlcat_Moon)
        {
            UpdateMoonDreamIllustration(self, illustration, menuSceneModule, illustrationModule);
        }


        if (illustrationModule.Type == IllustrationType.PearlHeart || illustrationModule.Type == IllustrationType.PearlHeartCore)
        {
            AnimateMenuPearl_Heart(self, illustration, illustrationModule);
        }

        if (illustrationModule.Type == IllustrationType.PearlActiveHalo)
        {
            illustration.sprite.SetAnchor(Vector2.one * 0.5f);
            illustration.sprite.scale = 0.3f;
            illustration.pos = menuSceneModule.ActivePearlPos;
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

            UpdateMenuPearl_Active(self, illustration, menuSceneModule, illustrationModule);
        }
        else if (illustrationModule.Type == IllustrationType.PearlNonActive)
        {
            var origin = new Vector2(660, 400);
            var angleFrameAddition = 0.0055f;

            var radius = 80.0f;
            var radiusXYRatio = 2.0f;

            Func<float, float> scaleFunc = angle => Custom.LerpMap(Mathf.Sin(angle), -1.0f, 1.0f, 0.3f, 0.2f);
            Func<Color, Color> colorFunc = color => color;

            if (miscProg.HasTrueEnding)
            {
                origin = new Vector2(675.0f, 350.0f);
                angleFrameAddition = 0.0045f;
                radius = 130.0f;
                radiusXYRatio = 1.0f;
            }

            AnimateMenuPearl_Orbit(illustration, menuSceneModule, illustrationModule, angleFrameAddition, origin, radius, radiusXYRatio, scaleFunc, colorFunc);
        }

        if (miscProg.HasTrueEnding && self.owner is SlugcatSelectMenu.SlugcatPage slugcatPage)
        {
            // TODO: True end select screen
            slugcatPage.markOffset = new Vector2(0.0f, 80.0f);
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
            if (fileName == "1_(!trueend)")
            {
                if (pearlcatSad || miscProg.HasPearlpup)
                {
                    illustration.pos = new(609, 27);
                }
            }
        }
        else if (illustrationModule.Type == IllustrationType.PearlActive || illustrationModule.Type == IllustrationType.PearlPlaceHolder)
        {
            var sadPos = new Vector2(870, 350);

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

            UpdateMenuPearl_Active(self, illustration, menuSceneModule, illustrationModule);
        }
        else if (illustrationModule.Type == IllustrationType.PearlNonActive)
        {
            var scale = 0.35f;

            float YFunc(int i) => Mathf.Sin((MenuPearlAnimStacker + i * 50.0f) / 50.0f) * 25.0f;

            AnimateMenuPearl_Float(illustration, menuSceneModule, illustrationModule, scale, YFunc);
        }
    }

    public static void UpdateSickScreenIllustration(MenuScene self, MenuIllustration illustration, MenuSceneModule menuSceneModule, MenuIllustrationModule illustrationModule)
    {
        if (illustrationModule.Type == IllustrationType.PearlActive)
        {
            illustration.visible = true;
            illustration.color = menuSceneModule.ActivePearl?.GetPearlColor() ?? Color.white;
            illustration.sprite.scale = 0.3f;

            UpdateMenuPearl_Active(self, illustration, menuSceneModule, illustrationModule);
        }
        else if (illustrationModule.Type == IllustrationType.PearlNonActive)
        {
            var origin = new Vector2(685, 490);
            var angleFrameAddition = 0.0045f;

            var radius = 90.0f;
            var radiusXYRatio = 1.7f;

            float ScaleFunc(float angle) => Custom.LerpMap(Mathf.Sin(angle), 1.0f, -1.0f, 0.2f, 0.3f);
            Color ColorFunc(Color color) => color;

            AnimateMenuPearl_Orbit(illustration, menuSceneModule, illustrationModule, angleFrameAddition, origin, radius, radiusXYRatio, ScaleFunc, ColorFunc);
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

            UpdateMenuPearl_Active(self, illustration, menuSceneModule, illustrationModule);
        }
        else if (illustrationModule.Type == IllustrationType.PearlNonActive)
        {
            var origin = new Vector2(685, 360);
            var angleFrameAddition = 0.0045f;

            var radius = 90.0f;
            var radiusXYRatio = 2.0f;

            float ScaleFunc(float angle) => Custom.LerpMap(Mathf.Cos(angle) + Mathf.Sin(angle), 2.0f, 0.0f, 0.2f, 0.3f);
            Color ColorFunc(Color color) => color.GetAscendScenePearlColor();

            AnimateMenuPearl_Orbit(illustration, menuSceneModule, illustrationModule, angleFrameAddition, origin, radius, radiusXYRatio, ScaleFunc, ColorFunc);
        }
    }

    public static void UpdatePebblesDreamIllustration(MenuScene self, MenuIllustration illustration, MenuSceneModule menuSceneModule, MenuIllustrationModule illustrationModule)
    {
        if (illustrationModule.Type == IllustrationType.PearlActive)
        {
            illustration.visible = true;
            illustration.sprite.scale = 0.35f;
            illustration.color = menuSceneModule.ActivePearl?.GetPearlColor() ?? Color.white;

            illustration.pos.y = illustrationModule.InitialPos.y + Mathf.Sin(MenuPearlAnimStacker / 50.0f) * 25.0f;

            menuSceneModule.ActivePearlPos = illustration.pos;
        }
        else if (illustrationModule.Type == IllustrationType.PearlNonActive)
        {
            var origin = new Vector2(680, 605);
            var angleFrameAddition = 0.0015f;

            var radius = 450.0f;
            var radiusXYRatio = 1.0f;

            float ScaleFunc(float _) => 0.3f;
            Color ColorFunc(Color color) => color;

            AnimateMenuPearl_Orbit(illustration, menuSceneModule, illustrationModule, angleFrameAddition, origin, radius, radiusXYRatio, ScaleFunc, ColorFunc);
        }
    }

    public static void UpdateMoonDreamIllustration(MenuScene self, MenuIllustration illustration, MenuSceneModule menuSceneModule, MenuIllustrationModule illustrationModule)
    {
        if (illustrationModule.Type == IllustrationType.Default)
        {
            var fileName = Path.GetFileNameWithoutExtension(illustration.fileName);
            var scale = 1.0f;

            if (fileName == "3")
            {
                float YFunc(int i) => Mathf.Sin((MenuPearlAnimStacker) / 50.0f) * 25.0f;

                AnimateMenuPearl_Float(illustration, menuSceneModule, illustrationModule, scale, YFunc, false);
            }
            else if (fileName == "4")
            {
                float YFunc(int i) => Mathf.Sin((MenuPearlAnimStacker + 50.0f) / 50.0f) * 25.0f;

                AnimateMenuPearl_Float(illustration, menuSceneModule, illustrationModule, scale, YFunc, false);
            }
        }
        else if (illustrationModule.Type == IllustrationType.PearlActive)
        {
            illustration.visible = true;
            illustration.sprite.scale = 0.3f;
            illustration.color = Color.Lerp(menuSceneModule.ActivePearl?.GetPearlColor() ?? Color.white, Color.white, 0.5f);

            illustration.pos.y = illustrationModule.InitialPos.y + Mathf.Sin(MenuPearlAnimStacker / 80.0f) * 15.0f;

            menuSceneModule.ActivePearlPos = illustration.pos;
        }
        else if (illustrationModule.Type == IllustrationType.PearlActiveHalo)
        {
            illustration.visible = false;
        }
    }


    // Menu Pearl Animations
    public static void UpdateMenuPearl_Active(MenuScene self, MenuIllustration illustration, MenuSceneModule menuSceneModule, MenuIllustrationModule illustrationModule)
    {
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
    }

    public static void AnimateMenuPearl_Float(MenuIllustration illustration, MenuSceneModule menuSceneModule, MenuIllustrationModule illustrationModule, float scale, Func<int, float> yFunc, bool usesIndex = true)
    {
        var pearls = menuSceneModule.NonActivePearls;

        var i = illustrationModule.NonActivePearlIndex;

        if (usesIndex)
        {
            var count = pearls.Count;

            if (i >= count)
            {
                illustration.visible = false;
                return;
            }

            illustration.color = pearls[i].GetPearlColor();
        }

        illustration.visible = true;
        illustration.sprite.scale = scale;

        illustration.pos.y = illustrationModule.InitialPos.y + yFunc(i);
    }

    private static void AnimateMenuPearl_Orbit(MenuIllustration illustration, MenuSceneModule menuSceneModule, MenuIllustrationModule illustrationModule, float angleFrameAddition, Vector2 origin, float radius, float radiusXYRatio, Func<float, float> scaleFunc, Func<Color, Color> colorFunc)
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

        var angle = (i * Mathf.PI * 2.0f / count) + angleFrameAddition * MenuPearlAnimStacker;

        var targetPos = new Vector2(origin.x + Mathf.Cos(angle) * radius * radiusXYRatio, origin.y + Mathf.Sin(angle) * radius);
        illustration.pos = targetPos;

        illustration.sprite.scale = scaleFunc(angle);
        illustration.alpha = 1.0f;
        illustration.color = colorFunc(pearls[i].GetPearlColor());
    }

    public static void AnimateMenuPearl_Heart(MenuScene self, MenuIllustration illustration, MenuIllustrationModule module)
    {
        illustration.visible = true;

        var initialScale = 0.3f;
        var isCore = module.Type == IllustrationType.PearlHeartCore;

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
            illustration.sprite.scale = coreBeat ? 0.4f : Mathf.Lerp(currentScale, initialScale, 0.1f);
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


    // Set the positions, depths, and layers of the dynamic pearls
    public static void DetermineIllustrationPosDepthLayer(MenuIllustration i, MenuScene menuScene, MenuIllustrationModule menuIllustrationModule)
    {
        var sceneId = menuScene.sceneID;
        var type = menuIllustrationModule.Type;
        var index = menuIllustrationModule.NonActivePearlIndex;

        var miscProg = Utils.MiscProgression;

        if (sceneId == Scenes.Slugcat_Pearlcat)
        {
            if (type == IllustrationType.PearlNonActive)
            {
                i.SetDepth(2.0f);

                if (miscProg.HasTrueEnding)
                {
                    i.LayerInFrontOf("(trueend)"); // TODO: True end select screen
                    i.SetDepth(20.0f);
                }
                else
                {
                    if (menuScene.flatMode)
                    {
                        i.LayerInFrontOf("flat_(!trueend)_3");
                    }
                    else
                    {
                        i.LayerInFrontOf("(!trueend)_3");
                    }
                }
                return;
            }

            if (type == IllustrationType.PearlActive)
            {
                i.SetPosition(645, 540);
                i.SetDepth(2.3f);
                i.LayerInFrontOf("(!trueend)_1_(pup)");

                if (miscProg.HasTrueEnding)
                {
                    i.SetDepth(20.0f);
                }
                return;
            }

            if (type == IllustrationType.PearlActiveHalo)
            {
                i.SetDepth(2.0f);
                i.LayerInFrontOf("(!trueend)_1_(pup)");

                if (miscProg.HasTrueEnding)
                {
                    i.SetDepth(20.0f);
                }
                return;
            }

            if (type == IllustrationType.PearlHeart || type == IllustrationType.PearlHeartCore)
            {
                i.SetPosition(770, 500);
                i.SetDepth(20.0f);
                i.LayerInFrontOf("(trueend)"); // TODO: True end select screen
                return;
            }

            return;
        }

        if (sceneId == Scenes.Slugcat_Pearlcat_Sleep)
        {
            if (type == IllustrationType.PearlNonActive)
            {
                var pos = index switch
                {
                    0 => new(789, 472),
                    1 => new(870, 558),
                    2 => new(1193, 492),
                    3 => new(1077, 648),
                    4 => new(952, 647),

                    5 => new(805, 327),
                    6 => new(1106, 405),
                    7 => new(1218, 330),
                    8 => new(1005, 275),
                    9 => new(1213, 599),

                    _ => Vector2.zero,
                };
                i.SetPosition(pos.x, pos.y);

                i.SetDepth(2.3f);

                if (index <= 4)
                {
                    i.LayerInFrontOf("pearlpup_(pup)_(!sad)");
                }
                else
                {
                    i.LayerInFrontOf("pearlcat_(!trueend)_(!pup)_(!sad)");
                }
                return;
            }

            if (type == IllustrationType.PearlActive)
            {
                i.SetPosition(911, 450);
                i.SetDepth(2.3f);
                i.LayerInFrontOf("pearlcat_(!trueend)_(!pup)_(!sad)");
                return;
            }

            if (type == IllustrationType.PearlPlaceHolder)
            {
                i.SetPosition(900, 425);
                i.SetDepth(2.3f);
                i.LayerInFrontOf("pearlcat_(!trueend)_(!pup)_(!sad)");
                return;
            }

            if (type == IllustrationType.PearlActiveHalo)
            {
                i.SetDepth(2.1f);
                i.LayerInFrontOf("pearlcat_(!trueend)_(!pup)_(!sad)");
                return;
            }

            if (type == IllustrationType.PearlHeart || type == IllustrationType.PearlHeartCore)
            {
                i.SetPosition(1000, 400);
                i.SetDepth(2.3f);
                i.LayerInFrontOf("pearlpup_(trueend)");
                return;
            }

            return;
        }

        if (sceneId == Scenes.Slugcat_Pearlcat_Sick)
        {
            if (type == IllustrationType.PearlNonActive)
            {
                i.SetDepth(2.0f);
                i.LayerInFrontOf("pearlcat");
                return;
            }

            if (type == IllustrationType.PearlActive)
            {
                i.SetPosition(685, 630);
                i.SetDepth(2.3f);
                i.LayerInFrontOf("pearlcat");
                return;
            }

            if (type == IllustrationType.PearlActiveHalo)
            {
                i.SetDepth(2.1f);
                i.LayerInFrontOf("pearlcat");
                return;
            }

            return;
        }

        if (sceneId == Scenes.Slugcat_Pearlcat_Ascended)
        {
            if (type == IllustrationType.PearlNonActive)
            {
                i.SetDepth(2.0f);
                i.LayerInFrontOf("pearlcatbody");
                return;
            }

            if (type == IllustrationType.PearlActive)
            {
                i.SetPosition(685, 600);
                i.SetDepth(2.3f);
                i.LayerInFrontOf("pearlcathead");
                return;
            }

            if (type == IllustrationType.PearlActiveHalo)
            {
                i.SetDepth(2.0f);
                i.LayerInFrontOf("pearlcathead");
                return;
            }

            return;
        }

        if (sceneId == Scenes.Dream_Pearlcat_Pebbles)
        {
            if (type == IllustrationType.PearlNonActive)
            {
                i.SetDepth(2.0f);
                i.LayerInFrontOf("2");
                return;
            }

            if (type == IllustrationType.PearlActive)
            {
                i.SetPosition(700, 530);
                i.SetDepth(2.3f);
                i.LayerInFrontOf("2");
                return;
            }

            if (type == IllustrationType.PearlActiveHalo)
            {
                i.SetDepth(2.2f);
                i.LayerInFrontOf("2");
                return;
            }

            return;
        }

        if (sceneId == Scenes.Dream_Pearlcat_Moon)
        {
            if (type == IllustrationType.PearlActive)
            {
                i.SetPosition(900, 475);
                i.SetDepth(7.0f);
                i.LayerInFrontOf("5");
                return;
            }
        }
    }

    public static void LayerInFrontOf(this MenuIllustration illustration, string target)
    {
        if (illustration.owner is not MenuScene menuScene)
        {
            return;
        }

        var illustrations = menuScene.flatIllustrations.Concat(menuScene.depthIllustrations).ToList();

        var targetIllustration = illustrations.FirstOrDefault(x => x.fileName.EndsWith(Path.DirectorySeparatorChar + target));

        if (targetIllustration is null)
        {
            // the normal scenes are missing in flatmode so don't wanna cause uncessary errors, probably fine as we don't need to layer in flatmode usually
            if (!menuScene.flatMode)
            {
                Plugin.Logger.LogError($"Failed to layer menu scene! Not fatal, but indicates a target layer is named incorrectly and needs fixing.\nScene: {menuScene.sceneID.value}, Missing Target: {target}");
            }
            return;
        }

        illustration.sprite.MoveToBack();

        illustration.sprite.MoveInFrontOfOtherNode(targetIllustration.sprite);

        // i dunno if changing the index actually matters but makes it easier to see where stuff is
        var targetIndex = illustrations.IndexOf(targetIllustration);

        illustrations.Insert(targetIndex, illustration);
    }

    public static void SetPosition(this MenuIllustration illustration, float x, float y)
    {
        var pos = new Vector2(x, y);
        var module = illustration.GetModule();

        illustration.pos = pos;
        module.InitialPos = pos;
        module.SetPos = pos;
    }

    public static void SetDepth(this MenuIllustration illustration, float depth)
    {
        if (illustration is MenuDepthIllustration depthIllustration)
        {
            depthIllustration.depth = depth;
        }
    }


    // Convert a given pearl type enum to its corresponding serialized pearl data (as would be stored in misc progression)
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


    // Get the illustration file name for a given pearl type, randomised based on the ID, or a unique one for certain special types of pearls
    public static string GetPearlIllustration(string menuPearlType, string appendTag)
    {
        var uniqueIllustration = GetUniquePearlIllustration(menuPearlType);

        if (uniqueIllustration is not null)
        {
            return uniqueIllustration;
        }

        var randState = Random.state;

        if (menuPearlType == "Misc" || menuPearlType == "Misc2" || menuPearlType == "PebblesPearl")
        {
            Random.InitState((int)DateTime.Now.Ticks);
        }
        else
        {
            Random.InitState(menuPearlType.GetHashCode());
        }

        var index = Random.Range(0, 10);

        var illustration = $"{index}{appendTag}";

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

        if (menuPearlType == "BigGoldenPearl")
        {
            return "unique_biggoldenpearl";
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

        return 10;
    }
}
