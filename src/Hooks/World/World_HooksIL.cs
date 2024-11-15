using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
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
                x => x.MatchRet())
           )
        {
            return;
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

        if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdstr("OE_SEXTRA"),
                x => x.MatchStloc(0))
           )
        {
            return;
        }

        if (!c.TryGotoNext(MoveType.After,
                x => x.MatchStloc(1))
           )
        {
            return;
        }


        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldloc_0);
        c.EmitDelegate<Func<RainWorldGame, string, string>>((game, roomName) =>
        {
            if (game.GetStorySession.saveStateNumber == Enums.Pearlcat)
            {   
                var deathSave = game.GetStorySession.saveState.deathPersistentSaveData;
                deathSave.karma = deathSave.karmaCap;
                
                var miscProg = Utils.GetMiscProgression();


                miscProg.IsPearlpupSick = true;
                miscProg.HasOEEnding = true;


                var miscWorld = game.GetMiscWorld();

                if (miscWorld != null)
                {
                    miscWorld.JustBeatAltEnd = true;
                }


                SlugBase.Assets.CustomScene.SetSelectMenuScene(game.GetStorySession.saveState, Enums.Scenes.Slugcat_Pearlcat_Sick);
                

                Plugin.Logger.LogInfo("PEARLCAT OE ENDING");

                return "OE_SEXTRA";
            }

            return roomName;
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
            return;
        }

        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldarg_2);
        c.Emit(OpCodes.Ldloc_0);
        c.EmitDelegate<Func<AbstractRoom, RainWorldGame, int, int>>((self, game, num) =>
        {
            if (game.IsStorySession && game.StoryCharacter == Enums.Pearlcat)
            {
                var save = game.GetMiscWorld();
                var miscProg = Utils.GetMiscProgression();

                if (save?.PearlpupID == null && ModOptions.PearlpupRespawn.Value && !miscProg.HasTrueEnding)
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
    private delegate int orig_StoryGameSessionSlugPupMaxCount(StoryGameSession self);
    private static int GetStoryGameSessionSlugPupMaxCount(orig_StoryGameSessionSlugPupMaxCount orig, StoryGameSession self)
    {
        var result = orig(self);

        if (self.saveStateNumber != Enums.Pearlcat)
        {
            return result;
        }

        var save = self.saveState.miscWorldSaveData.GetMiscWorld();

        if (save != null && save.PearlpupID == null)
        {
            return 1;
        }

        return 0;

    }


    // Unlock certain gates (Bitter Aerie, Metropolis)
    private delegate bool orig_RegionGateMeetRequirement(RegionGate self);
    private static bool GetRegionGateMeetRequirement(orig_RegionGateMeetRequirement orig, RegionGate self)
    {
        var result = orig(self);

        return self.IsGateOpenForPearlcat() || result;
    }


    // Prevent Player Pearls being saved in the shelter 
    private static HUD.Map.ShelterMarker.ItemInShelterMarker.ItemInShelterData? Map_GetItemInShelterFromWorld(On.HUD.Map.orig_GetItemInShelterFromWorld orig, World world, int room, int index)
    {
        var result = orig(world, room, index);

        var abstractRoom = world.GetAbstractRoom(room);

        if (index < abstractRoom.entities.Count && abstractRoom.entities[index] is AbstractPhysicalObject abstractObject)
        {
            if (abstractObject.realizedObject != null && abstractObject.IsPlayerPearl())
            {
                return null;
            }
        }

        return result;
    }
}
