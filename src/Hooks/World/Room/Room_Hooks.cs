using static Pearlcat.World_Helpers;
using static Pearlcat.Room_Helpers;

namespace Pearlcat;

public static class Room_Hooks
{
    public static void ApplyHooks()
    {
        On.Room.Loaded += Room_Loaded;
        On.Room.Update += Room_Update;
        On.RoomSpecificScript.AddRoomSpecificScript += RoomSpecificScript_AddRoomSpecificScript;
        On.ShelterDoor.DrawSprites += ShelterDoor_DrawSprites;
        On.ShelterDoor.DoorGraphic.DrawSprites += DoorGraphic_DrawSprites;
        On.AboveCloudsView.ctor += AboveCloudsView_ctor;
    }

    private static void RoomSpecificScript_AddRoomSpecificScript(On.RoomSpecificScript.orig_AddRoomSpecificScript orig, Room room)
    {
        orig(room);

        var roomName = room.roomSettings.name;

        AddRoomSpecificScript_SS(room, roomName);

        AddRoomSpecificScript_T1(room, roomName);
    }

    private static void Room_Update(On.Room.orig_Update orig, Room self)
    {
        orig(self);

        var miscProg = Utils.MiscProgression;

        if (self.roomSettings.name == "T1_END")
        {
            // Outside train wind effect
            foreach (var updatable in self.updateList)
            {
                if (updatable is not PhysicalObject physicalObject)
                {
                    continue;
                }

                if (physicalObject is not Player player)
                {
                    continue;
                }

                List<Player.BodyModeIndex> exemptBodyModes =
                [
                    Player.BodyModeIndex.Crawl,
                    Player.BodyModeIndex.ClimbIntoShortCut,
                    Player.BodyModeIndex.CorridorClimb,
                ];

                var target = player.canJump == 0 ? 1.0f : 0.85f;

                if (!player.TryGetPearlcatModule(out var playerModule))
                {
                    continue;
                }

                if (playerModule.EarL is null || playerModule.EarR is null)
                {
                    continue;
                }

                foreach (var earSegment in playerModule.EarL)
                {
                    earSegment.vel.x += target * 1.25f;
                }

                foreach (var earSegment in playerModule.EarR)
                {
                    earSegment.vel.x += target * 1.25f;
                }

                if (player.graphicsModule is not PlayerGraphics graphics)
                {
                    continue;
                }

                foreach (var tailSegment in graphics.tail)
                {
                    tailSegment.vel.x += target * 1.25f;
                }


                if (!exemptBodyModes.Contains(player.bodyMode))
                {
                    foreach (var bodyChunk in player.bodyChunks)
                    {
                        bodyChunk.vel.x += target;
                    }
                }
            }

            // TrueEnd changes the music that plays ontop of the train
            if (miscProg.HasTrueEnding)
            {
                if (self.roomSettings.triggers.FirstOrDefault(x => x is SpotTrigger) is SpotTrigger spotTrigger)
                {
                    if (spotTrigger.tEvent is MusicEvent musicEvent)
                    {
                        musicEvent.songName = "na_30 - distance";
                    }
                }
            }
        }

        if (TrainViewRooms.Contains(self.roomSettings.name))
        {
            // Train view screen shake
            var intensity = self.roomSettings.name == "T1_END" ? 0.15f : 0.1f;
            self.ScreenMovement(null, Vector2.right * 3.0f, intensity);

            // Nighttime Palette
            if (miscProg.HasTrueEnding)
            {
                foreach (var camera in self.game.cameras)
                {
                    if (camera.paletteA != 303)
                    {
                        camera.ChangeMainPalette(303);
                    }
                }
            }
        }
    }


    private static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
    {
        orig(self);

        if (TrainViewRooms.Contains(self.roomSettings.name))
        {
            self.AddObject(new TrainView(self));
        }
    }

    // Hide door sprites for the train shelter
    private static void DoorGraphic_DrawSprites(On.ShelterDoor.DoorGraphic.orig_DrawSprites orig, ShelterDoor.DoorGraphic self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (self.myShelter.room.roomSettings.name == "T1_S01")
        {
            foreach (var sprite in sLeaser.sprites)
            {
                sprite.isVisible = false;
            }
        }
    }

    private static void ShelterDoor_DrawSprites(On.ShelterDoor.orig_DrawSprites orig, ShelterDoor self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (self.room.roomSettings.name == "T1_S01")
        {
            foreach (var sprite in sLeaser.sprites)
            {
                sprite.isVisible = false;
            }
        }
    }

    // Reset this here instead, better for compat
    private static void AboveCloudsView_ctor(On.AboveCloudsView.orig_ctor orig, AboveCloudsView self, Room room, RoomSettings.RoomEffect effect)
    {
        if (Shader.GetGlobalFloat(TrainView.WindDir) == TrainView.TRAIN_WIND_DIR)
        {
            Shader.SetGlobalFloat(TrainView.WindDir, ModManager.MSC ? -1.0f : 1.0f);
        }

        orig(self, room, effect);
    }
}
