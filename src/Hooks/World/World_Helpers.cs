using System;
using MoreSlugcats;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Random = UnityEngine.Random;

namespace Pearlcat;

using Dreams = Enums.Dreams;

public static class World_Helpers
{
    public static List<string> TrainViewRooms { get; } =
    [
        "T1_START",
        "T1_CAR0",
        "T1_CAR1",
        "T1_CAR2",
        "T1_CAR3",
        "T1_CAREND",
        "T1_END",
        "T1_S01",
    ];

    public static bool IsGateOpenForPearlcat(this RegionGate gate)
    {
        var roomName = gate.room?.roomSettings?.name;

        if (gate.room is null || roomName is null)
        {
            return false;
        }

        if (!gate.room.game.IsPearlcatStory())
        {
            return false;
        }


        if (roomName == "GATE_UW_LC")
        {
            return true;
        }

        if (roomName == "GATE_SL_MS")
        {
            return true;
        }


        return false;
    }


    // Shortcut Management
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
        {
            if (!room.lockedShortcuts.Contains(shortcut))
            {
                room.lockedShortcuts.Add(shortcut);
            }
        }
    }
    public static void UnlockShortcuts(this Room room)
    {
        room.lockedShortcuts.Clear();
    }

    public static void HideShortcuts(this Room room)
    {
        var rCam = room.game.cameras.First();

        if (rCam.room != room)
        {
            return;
        }

        var shortcutGraphics = rCam.shortcutGraphics;

        for (var i = 0; i < room.shortcuts.Length; i++)
        {
            if (shortcutGraphics.entranceSprites.Length <= i)
            {
                continue;
            }

            if (shortcutGraphics.entranceSprites[i, 0] is not null)
            {
                shortcutGraphics.entranceSprites[i, 0].isVisible = false;
            }
        }
    }
    public static void ShowShortcuts(this Room room)
    {
        var rCam = room.game.cameras.First();

        if (rCam.room != room)
        {
            return;
        }

        var shortcutGraphics = rCam.shortcutGraphics;

        for (var i = 0; i < room.shortcuts.Length; i++)
        {
            if (shortcutGraphics.entranceSprites.Length <= i)
            {
                continue;
            }

            if (shortcutGraphics.entranceSprites[i, 0] is not null)
            {
                shortcutGraphics.entranceSprites[i, 0].isVisible = true;
            }
        }
    }


    // Pearl ID
    public static bool IsHeartPearl(this AbstractPhysicalObject? obj)
    {
        return obj is DataPearl.AbstractDataPearl dataPearl && dataPearl.IsHeartPearl();
    }

    public static bool IsHeartPearl(this DataPearl dataPearl)
    {
        return dataPearl.AbstractPearl.IsHeartPearl();
    }

    public static bool IsHeartPearl(this DataPearl.AbstractDataPearl dataPearl)
    {
        return dataPearl.dataPearlType == Enums.Pearls.Heart_Pearlpup;
    }

    public static bool IsHalcyonPearl(this DataPearl dataPearl)
    {
        return dataPearl.AbstractPearl.IsHalcyonPearl();
    }

    public static bool IsHalcyonPearl(this DataPearl.AbstractDataPearl dataPearl)
    {
        return dataPearl.dataPearlType == Enums.Pearls.RM_Pearlcat || (ModManager.MSC && dataPearl.dataPearlType == MoreSlugcatsEnums.DataPearlType.RM);
    }


    // Dreams
    public static List<DreamsState.DreamID> GetDreamPool(RainWorldGame self, SaveMiscWorld miscWorld)
    {
        List<DreamsState.DreamID> dreamPool = [];

        var storyGame = self.GetStorySession;
        var save = self.GetStorySession.saveState;
        var miscProg = Utils.MiscProgression;

        if (miscProg.HasTrueEnding) // Adult Pearlpup has recurring nightmares
        {
            var randomDream = Random.Range(0.0f, 1.0f) < 0.1f;

            // Also don't dream on the train
            if (randomDream && save.denPosition != "T1_S01")
            {
                dreamPool.Add(Dreams.Dream_Pearlcat_Sick);
                dreamPool.Add(Dreams.Dream_Pearlcat_Pearlpup);
            }
        }
        else
        {
            if (miscWorld.HasPearlpupWithPlayerDeadOrAlive)
            {
                var randomDream = save.cycleNumber > 4 && Random.Range(0.0f, 1.0f) < 0.15f;

                if (randomDream)
                {
                    if (miscProg.IsPearlpupSick && !storyGame.HasDreamt(Dreams.Dream_Pearlcat_Sick))
                    {
                        dreamPool.Add(Dreams.Dream_Pearlcat_Sick);
                    }
                    else if (!storyGame.HasDreamt(Dreams.Dream_Pearlcat_Pearlpup))
                    {
                        dreamPool.Add(Dreams.Dream_Pearlcat_Pearlpup);
                    }
                }
            }

            // Pearlcat will have recurring nightmares if she loses her pup
            if (!miscWorld.HasPearlpupWithPlayerDeadOrAlive && miscProg.DidHavePearlpup)
            {
                var randomDream = Random.Range(0.0f, 1.0f) < 0.1f;

                if (randomDream)
                {
                    dreamPool.Add(Dreams.Dream_Pearlcat_Pearlpup);
                    dreamPool.Add(Dreams.Dream_Pearlcat_Sick);
                }
            }

            // for reaching a full inventory of pearls (default size)
            if (!ModOptions.InventoryOverride && miscProg.StoredNonActivePearls.Count >= 8 && !storyGame.HasDreamt(Dreams.Dream_Pearlcat_Scholar))
            {
                dreamPool.Add(Dreams.Dream_Pearlcat_Scholar);
            }
        }

        return dreamPool;
    }

    public static void TryDream(this StoryGameSession storyGame, DreamsState.DreamID dreamId, bool onlyIfNew)
    {
        var miscWorld = storyGame.saveState.miscWorldSaveData.GetMiscWorld();

        if (miscWorld is null)
        {
            return;
        }

        var strId = dreamId.value;

        if (storyGame.HasDreamt(dreamId) && onlyIfNew)
        {
            return;
        }

        miscWorld.CurrentDream = strId;
        SlugBase.Assets.CustomDreams.QueueDream(storyGame, dreamId);
    }

    public static bool HasDreamt(this StoryGameSession storyGame, DreamsState.DreamID dreamId)
    {
        var miscWorld = storyGame.saveState.miscWorldSaveData.GetMiscWorld();

        if (miscWorld is null)
        {
            return false;
        }

        var strId = dreamId.value;

        return miscWorld.PreviousDreams.Contains(strId);
    }

    // Misc
    public static bool IsPearlcatStory(this RainWorldGame? game)
    {
        return game?.StoryCharacter == Enums.Pearlcat;
    }

    public static bool IsSingleplayer(this Player player)
    {
        return player.abstractCreature.world.game.Players.Count == 1;
    }

    public static int GetFirstPearlcatIndex(this RainWorldGame? game)
    {
        if (game is null)
        {
            return -1;
        }

        for (var i = 0; i < game.Players.Count; i++)
        {
            var abstractCreature = game.Players[i];

            if (abstractCreature.realizedCreature is not Player player)
            {
                continue;
            }

            if (player.IsPearlcat())
            {
                return i;
            }
        }

        return -1;
    }

    public static void AddTextPrompt(this RainWorldGame game, string text, int wait, int time, bool darken = false, bool? hideHud = null)
    {
        hideHud ??= ModManager.MMF;

        game.cameras.First().hud.textPrompt.AddMessage(Utils.Translator.Translate(text), wait, time, darken, (bool)hideHud);
    }
}
