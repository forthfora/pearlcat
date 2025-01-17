namespace Pearlcat;

public static class Room_Helpers
{
    public static void AddRoomSpecificScript_SS(Room room, string roomName)
    {
        if (ModCompat_Helpers.IsModEnabled_MiraInstallation)
        {
            return;
        }

        if (roomName == "SS_T1_S01")
        {
            room.AddObject(new SS_T1_S01(room));
        }

        if (roomName == "SS_T1_CROSS")
        {
            room.AddObject(new SS_T1_CROSS(room));
        }
    }

    public static void AddRoomSpecificScript_T1(Room room, string roomName)
    {
        if (roomName == "T1_S01")
        {
            room.AddObject(new T1_S01(room));
        }

        // Starting room that only sets the correct position + adds food
        if (!room.game.IsPearlcatStory() || ModCompat_Helpers.RainMeadow_IsOnline)
        {
            if (roomName == "T1_START")
            {
                room.AddObject(new T1_START_Alt(room));
            }

            return;
        }

        var miscProg = Utils.MiscProgression;
        var everVisited = room.game.GetStorySession.saveState.regionStates[room.world.region.regionNumber].roomsVisited.Contains(room.abstractRoom.name);

        // Tutorial
        if (!everVisited)
        {
            // Start
            if (roomName == "T1_START")
            {
                room.AddObject(new T1_START(room));
            }

            // Rage (+ Possession)
            if (roomName == "T1_CAR2")
            {
                room.AddObject(new T1_CAR2(room));
            }

            if (!miscProg.HasTrueEnding)
            {
                // Agility
                if (roomName == "T1_CAR0")
                {
                    room.AddObject(new T1_CAR0(room));
                }

                // Shield
                if (roomName == "T1_CAR1")
                {
                    room.AddObject(new T1_CAR1(room));
                }

                // Revive
                if (roomName == "T1_CAR3")
                {
                    room.AddObject(new T1_CAR3(room));
                }
            }
        }
    }
}
