using RWCustom;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static DataPearl.AbstractDataPearl;

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

        //On.GlobalRain.Update += GlobalRain_Update;
        On.KingTusks.Tusk.ShootUpdate += Tusk_ShootUpdate;

        On.Spear.DrawSprites += Spear_DrawSprites;
        On.Spear.Update += Spear_Update;

        On.SaveState.GetSaveStateDenToUse += SaveState_GetSaveStateDenToUse;
        On.SaveState.SaveToString += SaveState_SaveToString1;
    }

    private static string SaveState_SaveToString1(On.SaveState.orig_SaveToString orig, SaveState self)
    {
        var miscWorld = self.miscWorldSaveData.GetMiscWorld();
        var miscProg = self.progression.miscProgressionData.GetMiscProgression();

        miscProg.StoredPearlTypes.Clear();
        miscProg.ActivePearlType = null;

        if (miscWorld.Inventory.TryGetValue(0, out var inventory) && miscWorld.ActiveObjectIndex.TryGetValue(0, out var activeIndex))
        {
            for (int i = 0; i < inventory.Count; i++)
            {
                string? item = inventory[i];
                var tryToParse = item.Split('>').Last();

                Plugin.Logger.LogWarning(tryToParse);

                if (!ExtEnumBase.TryParse(typeof(DataPearlType), tryToParse, false, out var type)) continue;

                if (type is not DataPearlType dataPearlType) continue;

                if (i == activeIndex)
                    miscProg.ActivePearlType = dataPearlType;

                else
                    miscProg.StoredPearlTypes.Add(dataPearlType);
            }
        }

        return orig(self);
    }

    private static string SaveState_GetSaveStateDenToUse(On.SaveState.orig_GetSaveStateDenToUse orig, SaveState self)
    {
        var result = orig(self);

        if (result == "T1_S01")
            return ModManager.MSC ? "LC_T1_S01" : "SS_S04";

        return result;
    }

    private static void Spear_Update(On.Spear.orig_Update orig, Spear self, bool eu)
    {
        orig(self, eu);


        if (!self.abstractSpear.TryGetModule(out var module)) return;

        if (module.SparkTimer <= 0)
        {
            module.SparkTimer = Random.Range(40, 350);
        }
        else
        {
            module.SparkTimer--;
        }
    }

    public static bool TryGetModule(this AbstractSpear spear, out SpearModule module)
    {
        var save = spear.Room.world.game.GetMiscWorld();

        if (save.PearlSpears.TryGetValue(spear.ID.number, out module))
            return true;

        return false;
    }

    private static void Spear_DrawSprites(On.Spear.orig_DrawSprites orig, Spear self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (!self.abstractSpear.TryGetModule(out var module)) return;

        sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("pearlcat_spear");
        sLeaser.sprites[0].color = module.Color;

        var randOffset = Custom.DegToVec(Random.value * 360f) * 0.25f * Random.value;
        sLeaser.sprites[0].x += randOffset.x;
        sLeaser.sprites[0].y += randOffset.y;

        var thrown = self.mode == Weapon.Mode.Thrown;

        if (module.SparkTimer == 0 || thrown)
        {
            var startPos = self.firstChunk.pos + Custom.DegToVec(sLeaser.sprites[0].rotation) * -30.0f;
            var endPos = self.firstChunk.pos + Custom.DegToVec(sLeaser.sprites[0].rotation) * 30.0f;
            self.room.ConnectEffect(startPos, endPos, module.Color, thrown ? 0.5f : 0.75f, thrown ? 6 : 12);
        }
    }


    // Shield deflects tusks
    private static void Tusk_ShootUpdate(On.KingTusks.Tusk.orig_ShootUpdate orig, KingTusks.Tusk self, float speed)
    {
        orig(self, speed);

        if (self.mode != KingTusks.Tusk.Mode.ShootingOut) return;

        foreach (var crit in self.vulture.abstractCreature.Room.world.game.Players)
        {
            if (crit.realizedCreature is not Player player) continue;   
            
            if (!player.TryGetPearlcatModule(out var playerModule)) continue;
            
            if (!playerModule.ShieldActive) continue;


            var pos = self.chunkPoints[0, 0] + self.shootDir * (20.0f + speed);

            if (!Custom.DistLess(player.firstChunk.pos, pos, 50.0f)) continue;

            self.mode = KingTusks.Tusk.Mode.Dangling;

            self.room.DeflectEffect(pos);

            self.head.pos += Custom.DirVec(self.head.pos, self.chunkPoints[1, 0]) * 100f;
            self.head.vel += Custom.DirVec(self.head.pos, self.chunkPoints[1, 0]) * 100f;
            
            self.chunkPoints[0, 2] = self.shootDir * speed * 0.4f;
            self.chunkPoints[1, 2] = self.shootDir * speed * 0.6f;

            var rand = Custom.RNV();
            self.chunkPoints[0, 0] += rand * 4f;
            self.chunkPoints[0, 2] += rand * 6f;
            self.chunkPoints[1, 0] -= rand * 4f;
            self.chunkPoints[1, 2] -= rand * 6f;

            playerModule.ActivateVisualShield();
        }
    }

    // nevermind, rain shader looks weird at this angle
    private static void GlobalRain_Update(On.GlobalRain.orig_Update orig, GlobalRain self)
    {
        orig(self);

        foreach (var crit in self.game.Players)
        {
            if (crit.realizedCreature is not Player player) continue;

            if (player.room == null || !player.room.BeingViewed) continue;

            if (player.room.roomSettings.name == "T1_END")
                self.rainDirection = 40.0f;
        }
    }

    private static void RoomSpecificScript_AddRoomSpecificScript(On.RoomSpecificScript.orig_AddRoomSpecificScript orig, Room room)
    {
        orig(room);

        if (room.game.IsStorySession && room.game.GetStorySession.saveState.saveStateNumber == Enums.General.Pearlcat && room.abstractRoom.firstTimeRealized
            && room.game.GetStorySession.saveState.cycleNumber == 0 && room.game.GetStorySession.saveState.denPosition == "T1_START")
            room.AddObject(new T1_START(room));

        if (room.roomSettings.name == "LC_T1_S01")
            room.AddObject(new LC_T1_S01(room));

        if (room.roomSettings.name == "T1_S01")
            room.AddObject(new T1_S01(room));
    }

    public static List<string> TrainViewRooms { get; } = new()
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
                
                var target = player.canJump == 0 ? 1.0f : 0.85f;
               
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
