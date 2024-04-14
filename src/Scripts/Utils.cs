using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using UnityEngine;
using RWCustom;
using static Conversation;
using static SSOracleBehavior;
using Random = UnityEngine.Random;
using static AbstractPhysicalObject;
using MoreSlugcats;

namespace Pearlcat;

public static class Utils
{
    public static bool IsMiraActive => ModManager.ActiveMods.Any(x => x.id == "mira");
    public static bool MiraVersionWarning => IsMiraActive;

    public static RainWorld RainWorld => Custom.rainWorld;
    public static Dictionary<string, FShader> Shaders => RainWorld.Shaders;
    public static InGameTranslator Translator => RainWorld.inGameTranslator;
    public static SaveMiscProgression GetMiscProgression() => RainWorld.GetMiscProgression();

    public static bool WarpEnabled(this RainWorldGame game) => game.IsStorySession && (!ModManager.MSC || !game.rainWorld.safariMode);

    public static bool IsHeartPearl(this AbstractPhysicalObject? obj) => obj is DataPearl.AbstractDataPearl dataPearl && dataPearl.IsHeartPearl();
    public static bool IsHeartPearl(this DataPearl dataPearl) => dataPearl.AbstractPearl.IsHeartPearl();
    public static bool IsHeartPearl(this DataPearl.AbstractDataPearl dataPearl) => dataPearl.dataPearlType == Enums.Pearls.Heart_Pearlpup;

    public static bool IsHalcyonPearl(this DataPearl dataPearl) => dataPearl.AbstractPearl.IsHalcyonPearl();
    public static bool IsHalcyonPearl(this DataPearl.AbstractDataPearl dataPearl) => dataPearl.dataPearlType == Enums.Pearls.RM_Pearlcat || dataPearl.dataPearlType == MoreSlugcatsEnums.DataPearlType.RM;


    public static void TryRevivePlayer(this Player self, PlayerModule playerModule)
    {
        if (playerModule.ReviveCount <= 0) return;

        if (self.room == null) return;

        //if (self.room == null || self.graphicsModule == null) return;

        //if (self.killTag?.creatureTemplate is CreatureTemplate template
        //    && (template.type == CreatureTemplate.Type.DaddyLongLegs || template.type == CreatureTemplate.Type.BrotherLongLegs
        //    || template.type == CreatureTemplate.Type.BigEel || template.type == MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs)) return;

        self.AllGraspsLetGoOfThisObject(true);

        self.room.DeflectEffect(self.firstChunk.pos);
        playerModule.ShieldTimer = 200;

        if (self.dead)
            self.RevivePlayer();

        else
            self.room.ReviveEffect(self.firstChunk.pos);

        playerModule.SetReviveCooldown(-1);
    }

    public static void RevivePlayer(this Player self)
    {
        self.Revive();

        self.abstractCreature.Room.world.game.cameras.First().hud.textPrompt.gameOverMode = false;
        self.playerState.permaDead = false;
        self.playerState.alive = true;

        self.exhausted = false;
        self.airInLungs = 1.0f;
        self.aerobicLevel = 0.0f;

        if (!self.TryGetPearlcatModule(out var playerModule)) return;

        playerModule.PickObjectAnimation(self);
    }

    public static void Revive(this Creature self)
    {
        //self.graphicsModule?.ReleaseAllInternallyContainedSprites();

        if (self.State is HealthState healthState)
        {
            healthState.health = 1.0f;
        }

        self.State.alive = true;

        self.dead = false;
        self.killTag = null;
        self.killTagCounter = 0;
        self.abstractCreature.abstractAI?.SetDestination(self.abstractCreature.pos);

        if (self is not Player)
        {
            self.Stun(100);
        }

        self.room.ReviveEffect(self.mainBodyChunk.pos);
    }


    public static int GraspsHasType(this Player self, AbstractObjectType type)
    {
        for (int i = 0; i < self.grasps.Length; i++)
        {
            Creature.Grasp? grasp = self.grasps[i];

            if (grasp == null) continue;

            if (grasp.grabbed.abstractPhysicalObject.type == type)
                return i;
        }

        return -1;
    }

    public static bool IsHostileToMe(this Creature self, Creature creature)
    {
        // trust no one, not even yourself?
        if (creature == self)
            return false;
        
        if (creature is Player pup && pup.IsPearlpup())
            return false;

        // possessed creature
        if (self is Player && creature.abstractCreature.controlled)
            return false;
        
        var AI = creature.abstractCreature.abstractAI?.RealAI;

        if (self is Player && AI is LizardAI or ScavengerAI or BigNeedleWormAI or DropBugAI or CicadaAI or InspectorAI)
        {
            var aggression = AI.CurrentPlayerAggression(self.abstractCreature);

            var rep = AI.tracker.RepresentationForCreature(self.abstractCreature, false);

            if (rep?.dynamicRelationship == null)
                return false;

            if (AI is LizardAI)
                return aggression > 0.0f;

            if (AI is ScavengerAI)
                return aggression > 0.5f;

            if (AI is BigNeedleWormAI)
                return aggression > 0.0f;

            if (AI is CicadaAI)
                return aggression > 0.0f;

            if (AI is DropBugAI)
                return true;

            if (AI is MoreSlugcats.InspectorAI)
                return aggression > 0.0f;

            return false;
        }

        if (self is Player && creature is Player player2 && !player2.isSlugpup)
        {
            var game = self.abstractCreature.world.game;

            if (game.IsArenaSession && game.GetArenaGameSession.GameTypeSetup.spearsHitPlayers)
                return true;
        }

        var myRelationship = self.abstractCreature.creatureTemplate.CreatureRelationship(self.abstractCreature.creatureTemplate);
        var creatureRelationship = creature.abstractCreature.creatureTemplate.CreatureRelationship(self.abstractCreature.creatureTemplate);

        return myRelationship.GoForKill || creatureRelationship.GoForKill;
    }

    public static bool InDeathpit(this Player self)
    {
        return self.mainBodyChunk.pos.y < -300.0f
            && (!self.room.water || self.room.waterInverted || self.room.defaultWaterLevel < -10)
            && (!self.Template.canFly || self.Stunned || self.dead) && self.room.deathFallGraphic != null;
    }


    public static void AddTextPrompt(this RainWorldGame game, string text, int wait, int time, bool darken = false, bool? hideHud = null)
    {
        hideHud ??= ModManager.MMF;
        game.cameras.First().hud.textPrompt.AddMessage(Translator.Translate(text), wait, time, darken, (bool)hideHud);
    }


    public static void LockAndHideShortcuts(this Room room)
    {
        room.LockShortcuts();
        room.HideShortcuts();
    }
    public static void UnlockAndShowShortcuts(this Room room)
    {
        room.UnlockShortcuts();
        room.ShowShortcuts();

        room.game.cameras.First().hud.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom);
    }


    public static void LockShortcuts(this Room room)
    {
        foreach (var shortcut in room.shortcutsIndex)
            if (!room.lockedShortcuts.Contains(shortcut))
                room.lockedShortcuts.Add(shortcut);
    }
    public static void UnlockShortcuts(this Room room)
    {
        room.lockedShortcuts.Clear();
    }


    public static void HideShortcuts(this Room room)
    {
        var rCam = room.game.cameras.First();

        if (rCam.room != room) return;

        var shortcutGraphics = rCam.shortcutGraphics;

        for (int i = 0; i < room.shortcuts.Length; i++)
            if (shortcutGraphics.entranceSprites.Length > i && shortcutGraphics.entranceSprites[i, 0] != null)
                shortcutGraphics.entranceSprites[i, 0].isVisible = false;
    }
    public static void ShowShortcuts(this Room room)
    {
        var rCam = room.game.cameras.First();

        if (rCam.room != room) return;

        var shortcutGraphics = rCam.shortcutGraphics;

        for (int i = 0; i < room.shortcuts.Length; i++)
            if (shortcutGraphics.entranceSprites[i, 0] != null)
                shortcutGraphics.entranceSprites[i, 0].isVisible = true;
    }


    public static void TryDream(this StoryGameSession storyGame, DreamsState.DreamID dreamId, bool isRecurringDream = false)
    {
        var miscWorld = storyGame.saveState.miscWorldSaveData.GetMiscWorld();

        if (miscWorld == null) return;

        var strId = dreamId.value;
        
        if (miscWorld.PreviousDreams.Contains(strId) && !isRecurringDream) return;
        
        miscWorld.CurrentDream = strId;
        SlugBase.Assets.CustomDreams.QueueDream(storyGame, dreamId);
    }


    public static Color RWColorSafety(this Color color)
    {
        var hsl = Custom.RGB2HSL(color);

        var safeColor = Custom.HSL2RGB(hsl.x, hsl.y, Mathf.Clamp(hsl.z, 0.01f, 1.0f), color.a);

        return safeColor;
    }

    public static int TexUpdateInterval(this Player player)
    {
        var texUpdateInterval = 5;
        var quality = player.abstractCreature.world.game.rainWorld.options.quality;

        if (quality == Options.Quality.LOW)
        {
            texUpdateInterval = 20;
        }
        else if (quality == Options.Quality.MEDIUM)
        {
            texUpdateInterval = 10;
        }

        return texUpdateInterval;
    }

    public static void SetIfSame(this ref Color toSet, Color toCompare, Color newColor)
    {
        if (toSet == toCompare)
        {
            toSet = newColor;
        }
    }

    public static void MapAlphaToColor(this Texture2D texture, Dictionary<byte, Color> map)
    {
        var data = texture.GetPixelData<Color32>(0);

        for (int i = 0; i < data.Length; i++)
        {
            if (map.TryGetValue(data[i].a, out var targetColor))
            {
                data[i] = targetColor;
            }
        }

        texture.SetPixelData(data, 0);
        texture.Apply(false);
    }

    public static Color HSLToRGB(this Vector3 hsl)
    {
        return Custom.HSL2RGB(hsl.x, hsl.y, hsl.z);
    }


    public static void LoadCustomEventsFromFile(this Conversation self, string fileName, SlugcatStats.Name? saveFile = null, bool oneRandomLine = false, int randomSeed = 0)
    {
        if (saveFile == null) { saveFile = self.currentSaveFile; }

        var languageID = Translator.currentLanguage;
        string text;
        for (; ; )
        {
            text = AssetManager.ResolveFilePath(Translator.SpecificTextFolderDirectory(languageID) + Path.DirectorySeparatorChar.ToString() + fileName + ".txt");
            if (saveFile != null)
            {
                string text2 = text;
                text = AssetManager.ResolveFilePath(string.Concat(new string[]
                {
                    Translator.SpecificTextFolderDirectory(languageID),
                    Path.DirectorySeparatorChar.ToString(),
                    fileName,
                    "-",
                    saveFile.value,
                    ".txt"
                }));
                if (!File.Exists(text))
                {
                    text = text2;
                }
            }
            if (File.Exists(text))
            {
                goto IL_117;
            }
            if (languageID == InGameTranslator.LanguageID.English)
            {
                break;
            }
            languageID = InGameTranslator.LanguageID.English;
        }
        return;

    IL_117:
        string text3 = File.ReadAllText(text, Encoding.UTF8);
        if (text3[0] != '0')
        {
            text3 = Custom.xorEncrypt(text3, 54 + fileName.GetHashCode() + (int)languageID * 7);
        }

        string[] array = Regex.Split(text3, "\r\n");
        try
        {

            if (oneRandomLine)
            {
                List<TextEvent> list = new List<TextEvent>();
                for (int i = 1; i < array.Length; i++)
                {
                    string[] array2 = LocalizationTranslator.ConsolidateLineInstructions(array[i]);
                    if (array2.Length == 3)
                    {
                        list.Add(new TextEvent(self, int.Parse(array2[0], NumberStyles.Any, CultureInfo.InvariantCulture), array2[2], int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture)));
                    }
                    else if (array2.Length == 1 && array2[0].Length > 0)
                    {
                        list.Add(new TextEvent(self, 0, array2[0], 0));
                    }
                }
                if (list.Count > 0)
                {
                    Random.State state = Random.state;
                    Random.InitState(randomSeed);
                    TextEvent item = list[Random.Range(0, list.Count)];
                    Random.state = state;
                    self.events.Add(item);
                }
            }
            else
            {
                for (int j = 1; j < array.Length; j++)
                {
                    string[] array3 = LocalizationTranslator.ConsolidateLineInstructions(array[j]);
                    if (array3.Length == 3)
                    {
                        if (ModManager.MSC && !int.TryParse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture, out int num) && int.TryParse(array3[2], NumberStyles.Any, CultureInfo.InvariantCulture, out int num2))
                        {
                            self.events.Add(new TextEvent(self, int.Parse(array3[0], NumberStyles.Any, CultureInfo.InvariantCulture), array3[1], int.Parse(array3[2], NumberStyles.Any, CultureInfo.InvariantCulture)));
                        }
                        else
                        {
                            self.events.Add(new TextEvent(self, int.Parse(array3[0], NumberStyles.Any, CultureInfo.InvariantCulture), array3[2], int.Parse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture)));
                        }
                    }
                    else if (array3.Length == 2)
                    {
                        if (array3[0] == "SPECEVENT")
                        {
                            self.events.Add(new SpecialEvent(self, 0, array3[1]));
                        }
                        else if (array3[0] == "PEBBLESWAIT")
                        {
                            self.events.Add(new PebblesConversation.PauseAndWaitForStillEvent(self, null, int.Parse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture)));
                        }
                    }
                    else if (array3.Length == 1 && array3[0].Length > 0)
                    {
                        self.events.Add(new TextEvent(self, 0, array3[0], 0));
                    }
                }
            }

        }
        catch
        {
            self.events.Add(new TextEvent(self, 0, "TEXT ERROR", 100));
        }
    }

    
    public static void GiveTrueEnding(this SaveState saveState)
    {
        if (saveState.saveStateNumber != Enums.Pearlcat) return;

        var miscProg = GetMiscProgression();
        var miscWorld = saveState.miscWorldSaveData.GetMiscWorld();
        
        if (miscWorld == null) return;


        miscProg.HasTrueEnding = true;
        miscProg.IsPearlpupSick = false;

        miscWorld.PebblesMeetCount = 0;

        SlugBase.Assets.CustomScene.SetSelectMenuScene(saveState, Enums.Scenes.Slugcat_Pearlcat);

        // So the tutorial scripts can be added again
        foreach (var regionState in saveState.regionStates)
        {
            regionState?.roomsVisited?.RemoveAll(x => x?.StartsWith("T1_") == true);
        }
    }

    public static void StartFromMira(this SaveState saveState)
    {
        if (saveState.saveStateNumber != Enums.Pearlcat) return;

        var miscProg = GetMiscProgression();
        var miscWorld = saveState.miscWorldSaveData.GetMiscWorld();
        var baseMiscWorld = saveState.miscWorldSaveData;

        if (miscWorld == null) return;


        miscProg.IsPearlpupSick = true;
        miscProg.HasOEEnding = true;
        miscProg.DidHavePearlpup = true;

        miscWorld.ShownFullInventoryTutorial = true;
        miscWorld.ShownSpearCreationTutorial = true;

        miscWorld.PebblesMeetCount = 3;
        miscWorld.MoonSickPupMeetCount = 1;
        miscWorld.PebblesMetSickPup = true;


        baseMiscWorld.SLOracleState.playerEncountersWithMark = 0;
        baseMiscWorld.SLOracleState.playerEncounters = 1;

        miscWorld.JustMiraSkipped = true;

        SlugBase.Assets.CustomScene.SetSelectMenuScene(saveState, Enums.Scenes.Slugcat_Pearlcat_Sick);
    }
}