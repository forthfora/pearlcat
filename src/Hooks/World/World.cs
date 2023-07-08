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
    }

    public static List<string> TrainViewRooms = new()
    {
        "T1_END",
        "T1_START",
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
            var intensity = self.roomSettings.name == "T1_END" ? 0.15f : 0.025f;
            //self.game.cameras[0].ScreenMovement(null, Vector2.right * 3.0f, intensity);
        }

        if (self.roomSettings.name == "T1_END")
        {
            foreach (var updatable in self.updateList)
            {
                if (updatable is not PhysicalObject physicalObject) continue;

                if (physicalObject is not Player player) continue;

                if (player.bodyMode != Player.BodyModeIndex.Crawl)
                    player.firstChunk.vel.x += player.canJump == 0 ?  1.0f : 0.5f;
            }
        }
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
