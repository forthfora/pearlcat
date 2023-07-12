using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pearlcat;

public partial class Hooks
{
    public static void ApplyWorldHooks()
    {
        On.HUD.Map.GetItemInShelterFromWorld += Map_GetItemInShelterFromWorld;
        On.RegionState.AdaptRegionStateToWorld += RegionState_AdaptRegionStateToWorld;

        On.Room.Loaded += Room_Loaded;
        On.Room.Update += Room_Update;

        On.RoomSpecificScript.AddRoomSpecificScript += RoomSpecificScript_AddRoomSpecificScript;

        On.ShelterDoor.DrawSprites += ShelterDoor_DrawSprites;
        On.ShelterDoor.DoorGraphic.DrawSprites += DoorGraphic_DrawSprites;
    }

    private static void RoomSpecificScript_AddRoomSpecificScript(On.RoomSpecificScript.orig_AddRoomSpecificScript orig, Room room)
    {
        orig(room);

        if (room.game.IsStorySession && room.game.GetStorySession.saveState.saveStateNumber == Enums.General.Pearlcat
            && room.abstractRoom.firstTimeRealized && room.game.GetStorySession.saveState.cycleNumber == 0
            && room.game.GetStorySession.saveState.denPosition == "T1_START")
        {
            room.AddObject(new PearlcatStart(room));
        }
    }

    public static List<string> TrainViewRooms = new()
    {
        "T1_START",
        "T1_CAR1",
        "T1_CAREND",
        "T1_END",
        "T1_S01",
    };

    private static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
    {
        orig(self);

        if (TrainViewRooms.Contains(self.roomSettings.name))
            self.AddObject(new TrainView(self));
    }

    private static void Room_Update(On.Room.orig_Update orig, Room self)
    {
        orig(self);

        if (TrainViewRooms.Contains(self.roomSettings.name))
        {
            var intensity = self.roomSettings.name == "T1_END" ? 0.15f : 0.1f;
            self.ScreenMovement(null, Vector2.right * 3.0f, intensity);
        }
        else
        {
            Shader.SetGlobalFloat("_windDir", ModManager.MSC ? -1f : 1f);
        }

        if (self.roomSettings.name == "T1_END")
        {
            foreach (var updatable in self.updateList)
            {
                if (updatable is not PhysicalObject physicalObject) continue;

                if (physicalObject is not Player player) continue;

                List<Player.BodyModeIndex> exemptBodyModes = new()
                {
                    Player.BodyModeIndex.Crawl,
                    Player.BodyModeIndex.ClimbIntoShortCut,
                    Player.BodyModeIndex.CorridorClimb,
                };
                
                var target = player.canJump == 0 ? 1.5f : 1.1f;
               
                if (!player.TryGetPearlcatModule(out var playerModule)) continue;

                if (playerModule.EarL == null || playerModule.EarR == null) continue;

                foreach (var earSegment in playerModule.EarL)
                    earSegment.vel.x += target * 1.25f;

                foreach (var earSegment in playerModule.EarR)
                    earSegment.vel.x += target * 1.25f;

                if (player.graphicsModule is not PlayerGraphics graphics) continue;

                foreach (var tailSegment in graphics.tail)
                    tailSegment.vel.x += target * 1.25f;


                if (!exemptBodyModes.Contains(player.bodyMode))
                    foreach (var bodyChunk in player.bodyChunks)
                        bodyChunk.vel.x += target;
            }
        }
    }


    private static void DoorGraphic_DrawSprites(On.ShelterDoor.DoorGraphic.orig_DrawSprites orig, ShelterDoor.DoorGraphic self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (self.myShelter.room.roomSettings.name == "T1_S01")
            foreach (var sprite in sLeaser.sprites)
                sprite.isVisible = false;
    }

    private static void ShelterDoor_DrawSprites(On.ShelterDoor.orig_DrawSprites orig, ShelterDoor self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (self.room.roomSettings.name == "T1_S01")
            foreach (var sprite in sLeaser.sprites)
                sprite.isVisible = false;
    }

    private static void RegionState_AdaptRegionStateToWorld(On.RegionState.orig_AdaptRegionStateToWorld orig, RegionState self, int playerShelter, int activeGate)
    {
        for (int i = 0; i < self.world.NumberOfRooms; i++)
        {
            var abstractRoom = self.world.GetAbstractRoom(self.world.firstRoomIndex + i);

            for (int j = abstractRoom.entities.Count - 1; j >= 0; j--)
            {
                var entity = abstractRoom.entities[j];

                if (entity is not AbstractPhysicalObject abstractObject) continue;

                if (abstractObject.IsPlayerObject())
                    abstractRoom.RemoveEntity(entity);
            }
        }

        orig(self, playerShelter, activeGate);
    }

    // Prevent Player Pearls being saved in the shelter 
    private static HUD.Map.ShelterMarker.ItemInShelterMarker.ItemInShelterData? Map_GetItemInShelterFromWorld(On.HUD.Map.orig_GetItemInShelterFromWorld orig, World world, int room, int index)
    {
        var result = orig(world, room, index);

        var abstractRoom = world.GetAbstractRoom(room);

        if (index < abstractRoom.entities.Count && abstractRoom.entities[index] is AbstractPhysicalObject abstractObject)
            if (abstractObject.realizedObject != null && abstractObject.IsPlayerObject())
                return null;

        return result;
    }
}
