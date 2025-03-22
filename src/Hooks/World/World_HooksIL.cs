using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System.Reflection;

namespace Pearlcat;

public static class World_HooksIL
{
    public static void ApplyHooks()
    {
        try
        {
            _ = new Hook(
                typeof(RegionGate).GetProperty(nameof(RegionGate.MeetRequirement), BindingFlags.Instance | BindingFlags.Public)?.GetGetMethod(),
                typeof(World_HooksIL).GetMethod(nameof(GetRegionGateMeetRequirement), BindingFlags.Static | BindingFlags.NonPublic)
            );
        }
        catch (Exception e)
        {
            e.LogHookException();
        }

        try
        {
            _ = new Hook(
                typeof(StoryGameSession).GetProperty(nameof(StoryGameSession.slugPupMaxCount), BindingFlags.Instance | BindingFlags.Public)?.GetGetMethod(),
                typeof(World_HooksIL).GetMethod(nameof(GetStoryGameSessionSlugPupMaxCount), BindingFlags.Static | BindingFlags.NonPublic)
            );
        }
        catch (Exception e)
        {
            e.LogHookException();
        }

        try
        {
            IL.AbstractRoom.RealizeRoom += AbstractRoom_RealizeRoom;
        }
        catch (Exception e)
        {
            e.LogHookException();
        }

        try
        {
            IL.RainWorldGame.BeatGameMode += RainWorldGame_BeatGameMode;
        }
        catch (Exception e)
        {
            e.LogHookException();
        }

        try
        {
            IL.Region.GetFullRegionOrder += Region_GetFullRegionOrder;
        }
        catch (Exception e)
        {
            e.LogHookException();
        }
    }


    // Remove transit system from the Regions menu
    private static void Region_GetFullRegionOrder(ILContext il)
    {
        var c = new ILCursor(il);

        if (!c.TryGotoNext(MoveType.Before,
                x => x.MatchRet()))
        {
            throw new Exception("Goto Failed");
        }

        c.EmitDelegate<Func<List<string>, List<string>>>((list) =>
        {
            list.Remove("T1");

            return list;
        });
    }


    // Outer Expanse Ending
    private static void RainWorldGame_BeatGameMode(ILContext il)
    {
        var c = new ILCursor(il);

        if (!c.TryGotoNext(MoveType.After,
                x => x.MatchLdstr("OE_SEXTRA"),
                x => x.MatchStloc(0)))
        {
            throw new Exception("Goto Failed");
        }

        if (!c.TryGotoNext(MoveType.After,
                x => x.MatchStloc(1)))
        {
            throw new Exception("Goto Failed");
        }


        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldloc_0);
        c.EmitDelegate<Func<RainWorldGame, string, string>>((game, roomName) =>
        {
            if (game.GetStorySession.saveStateNumber != Enums.Pearlcat)
            {
                return roomName;
            }

            var deathSave = game.GetStorySession.saveState.deathPersistentSaveData;

            deathSave.karma = deathSave.karmaCap;

            var miscProg = Utils.MiscProgression;

            miscProg.IsPearlpupSick = true;
            miscProg.HasOEEnding = true;

            var miscWorld = game.GetMiscWorld();

            if (miscWorld is not null)
            {
                miscWorld.JustBeatAltEnd = true;
            }

            SlugBase.Assets.CustomScene.SetSelectMenuScene(game.GetStorySession.saveState, Enums.Scenes.Slugcat_Pearlcat_Sick);

            return "OE_SEXTRA";
        });

        c.Emit(OpCodes.Stloc_0);
    }


    // Make pup spawns guaranteed when the Pearlpup respawn cheat is enabled and pearlpup is missing
    private static void AbstractRoom_RealizeRoom(ILContext il)
    {
        var c = new ILCursor(il);

        if (!c.TryGotoNext(MoveType.After,
                x => x.MatchStloc(0))
           )
        {
            throw new Exception("Goto Failed");
        }

        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldarg_2);
        c.Emit(OpCodes.Ldloc_0);
        c.EmitDelegate<Func<AbstractRoom, RainWorldGame, int, int>>((_, game, num) =>
        {
            if (game.IsStorySession && game.StoryCharacter == Enums.Pearlcat)
            {
                var save = game.GetMiscWorld();
                var miscProg = Utils.MiscProgression;

                if (save?.PearlpupID is null && ModOptions.PearlpupRespawn && !miscProg.HasTrueEnding)
                {
                    return 0;
                }

                return int.MaxValue;
            }

            return num;
        });

        c.Emit(OpCodes.Stloc_0);
    }


    // Only let pups spawn if pearlpup is missing
    private static int GetStoryGameSessionSlugPupMaxCount(Func<StoryGameSession, int> orig, StoryGameSession self)
    {
        var result = orig(self);

        if (self.saveStateNumber != Enums.Pearlcat)
        {
            return result;
        }

        var save = self.saveState.miscWorldSaveData.GetMiscWorld();

        if (save is not null && save.PearlpupID is null)
        {
            return 1;
        }

        return 0;

    }


    // Unlock certain gates (Bitter Aerie, Metropolis)
    private static bool GetRegionGateMeetRequirement(Func<RegionGate, bool> orig, RegionGate self)
    {
        var result = orig(self);

        return self.IsGateOpenForPearlcat() || result;
    }
}
