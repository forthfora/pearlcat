using MoreSlugcats;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Pearlcat.World_Helpers;
using Random = UnityEngine.Random;

namespace Pearlcat;

public static class World_Hooks
{
    public static void ApplyHooks()
    {
        On.HUD.Map.GetItemInShelterFromWorld += Map_GetItemInShelterFromWorld;

        On.RegionState.AdaptRegionStateToWorld += RegionState_AdaptRegionStateToWorld;

        On.Room.Loaded += Room_Loaded;
        On.Room.Update += Room_Update;

        On.RoomSpecificScript.AddRoomSpecificScript += RoomSpecificScript_AddRoomSpecificScript;

        On.ShelterDoor.DrawSprites += ShelterDoor_DrawSprites;
        On.ShelterDoor.DoorGraphic.DrawSprites += DoorGraphic_DrawSprites;

        On.Spear.DrawSprites += Spear_DrawSprites_PearlSpear;
        On.Spear.Update += Spear_Update_PearlSpear;

        On.SaveState.GetSaveStateDenToUse += SaveState_GetSaveStateDenToUse;

        On.SlugcatStats.HiddenOrUnplayableSlugcat += SlugcatStats_HiddenOrUnplayableSlugcat;
        
        On.GateKarmaGlyph.ctor += GateKarmaGlyph_ctor;

        On.RegionGate.customOEGateRequirements += RegionGate_customOEGateRequirements;

        On.VultureMask.DrawSprites += VultureMask_DrawSprites;

        On.MoreSlugcats.MSCRoomSpecificScript.OE_GourmandEnding.Update += OE_GourmandEnding_Update;

        On.PlacedObject.FilterData.FromString += FilterData_FromString;

        On.DreamsState.StaticEndOfCycleProgress += DreamsState_StaticEndOfCycleProgress;
        On.RainWorldGame.ctor += RainWorldGame_ctor;

        On.AboveCloudsView.ctor += AboveCloudsView_ctor;

        On.DataPearl.UniquePearlHighLightColor += DataPearl_UniquePearlHighLightColor;
        On.Room.PlaySound_SoundID_BodyChunk += Room_PlaySound_SoundID_BodyChunk;
    }


    // Manage dreams
    private static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
    {
        orig(self, manager);

        if (!self.IsStorySession)
        {
            return;
        }

        if (self.StoryCharacter != Enums.Pearlcat)
        {
            return;
        }

        var save = self.GetStorySession.saveState;
        var miscWorld = self.GetMiscWorld();
        var miscProg = Utils.GetMiscProgression();

        if (miscWorld == null)
        {
            return;
        }


        if (miscWorld.HasPearlpupWithPlayerDeadOrAlive)
        {
            var canDream = save.cycleNumber > 4 && Random.Range(0.0f, 1.0f) < 0.35f;

            if (canDream)
            {
                if (miscProg.IsPearlpupSick)
                {
                    self.GetStorySession.TryDream(Enums.Dreams.Dream_Pearlcat_Sick);
                }
                else
                {
                    self.GetStorySession.TryDream(Enums.Dreams.Dream_Pearlcat_Pearlpup);
                }
            }
        }
        else
        {
            var canDream = Random.Range(0.0f, 1.0f) < 0.2f;

            if (canDream)
            {
                var dreamPool = new List<DreamsState.DreamID>();

                if (miscProg.HasTrueEnding)
                {
                    dreamPool.Add(Enums.Dreams.Dream_Pearlcat_Sick);
                    dreamPool.Add(Enums.Dreams.Dream_Pearlcat_Pearlpup);
                    dreamPool.Add(Enums.Dreams.Dream_Pearlcat_Pebbles);
                    dreamPool.Add(Enums.Dreams.Dream_Pearlcat_Moon_Sick);
                }
                else if (!miscWorld.HasPearlpupWithPlayerDeadOrAlive && (miscProg.AscendedWithPup || miscProg.DidHavePearlpup))
                {
                    dreamPool.Add(Enums.Dreams.Dream_Pearlcat_Pearlpup);
                    dreamPool.Add(Enums.Dreams.Dream_Pearlcat_Sick);
                }

                if (dreamPool.Count > 0)
                {
                    var randState = Random.state;
                    Random.InitState((int)DateTime.Now.Ticks);

                    self.GetStorySession.TryDream(dreamPool[Random.Range(0, dreamPool.Count)], true);

                    Random.state = randState;
                }
            }
        }
    }

    private static void DreamsState_StaticEndOfCycleProgress(On.DreamsState.orig_StaticEndOfCycleProgress orig, SaveState saveState, string currentRegion, string denPosition, ref int cyclesSinceLastDream, ref int cyclesSinceLastFamilyDream, ref int cyclesSinceLastGuideDream, ref int inGWOrSHCounter, ref DreamsState.DreamID upcomingDream, ref DreamsState.DreamID eventDream, ref bool everSleptInSB, ref bool everSleptInSB_S01, ref bool guideHasShownHimselfToPlayer, ref int guideThread, ref bool guideHasShownMoonThisRound, ref int familyThread)
    {
        if (saveState.saveStateNumber == Enums.Pearlcat && (eventDream == null || !eventDream.value.Contains("Pearlcat")))
        {
            return;
        }

        orig(saveState, currentRegion, denPosition, ref cyclesSinceLastDream, ref cyclesSinceLastFamilyDream, ref cyclesSinceLastGuideDream, ref inGWOrSHCounter, ref upcomingDream, ref eventDream, ref everSleptInSB, ref everSleptInSB_S01, ref guideHasShownHimselfToPlayer, ref guideThread, ref guideHasShownMoonThisRound, ref familyThread);
    }


    // Fix for DevTools timeline object filters, make it copy hunters
    private static void FilterData_FromString(On.PlacedObject.FilterData.orig_FromString orig, PlacedObject.FilterData self, string s)
    {
        orig(self, s);

        if (!self.availableToPlayers.Contains(SlugcatStats.Name.Red) && self.availableToPlayers.Contains(Enums.Pearlcat))
        {
            self.availableToPlayers.Remove(Enums.Pearlcat);
        }
    }


    // Block OE ending under some conditions
    private static void OE_GourmandEnding_Update(On.MoreSlugcats.MSCRoomSpecificScript.OE_GourmandEnding.orig_Update orig, MSCRoomSpecificScript.OE_GourmandEnding self, bool eu)
    {
        if (self.room.world.game.IsPearlcatStory())
        {
            var miscWorld = self.room.world.game.GetMiscWorld();
            var miscProg = Utils.GetMiscProgression();

            if (miscWorld?.HasPearlpupWithPlayer == false)
            {
                return;
            }

            if (miscProg.HasOEEnding)
            {
                return;
            }

            if (miscProg.HasTrueEnding)
            {
                return;
            }
        }

        orig(self, eu);
    }


    // Make vulture masks raised when pearlcat isn't moving (the only reason i did this is because I found them ugly lol)
    private static void VultureMask_DrawSprites(On.VultureMask.orig_DrawSprites orig, VultureMask self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        //Vector2 vector = Vector2.Lerp(self.firstChunk.lastPos, self.firstChunk.pos, timeStacker);
        //Vector2 vector2 = Vector3.Slerp(self.lastRotationA, self.rotationA, timeStacker);
        //Vector2 vector3 = Vector3.Slerp(self.lastRotationB, self.rotationB, timeStacker);

        var donnedLerp = Mathf.Lerp(self.lastDonned, self.donned, timeStacker);

        Player? wasPlayer = null;
        int? wasEatCounter = null;

        if (donnedLerp > 0f && self.grabbedBy.Count > 0 && self.grabbedBy[0].grabber is Player player && player.TryGetPearlcatModule(out var module))
        {
            wasPlayer = player;
            wasEatCounter = player.eatCounter;

            var isMoving = player.firstChunk.vel.magnitude > 3.0f;
            var targetCounter = isMoving ? 35 : 0;

            module.MaskCounter = (int)Custom.LerpAndTick(module.MaskCounter, targetCounter, 0.1f, 1.0f);
            player.eatCounter = module.MaskCounter;
        }

        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (wasPlayer != null && wasEatCounter != null)
        {
            wasPlayer.eatCounter = (int)wasEatCounter;
        }
    }


    // Unlock OE Gate
    private static bool RegionGate_customOEGateRequirements(On.RegionGate.orig_customOEGateRequirements orig, RegionGate self)
    {
        var result = orig(self);

        //if (self.room.game.IsPearlcatStory() && self.room.game.IsStorySession && self.room.game.GetStorySession.saveState.denPosition.Contains("OE_"))
        //    return true;

        if (self.room.game.IsPearlcatStory())
        {
            return true;
        }

        return result;
    }

    private static void GateKarmaGlyph_ctor(On.GateKarmaGlyph.orig_ctor orig, GateKarmaGlyph self, bool side, RegionGate gate, RegionGate.GateRequirement requirement)
    {
        orig(self, side, gate, requirement);

        if (!gate.IsGateOpenForPearlcat())
        {
            return;
        }

        self.requirement = RegionGate.GateRequirement.OneKarma;
    }


    // Hide the ID used for pebbles pearl readings
    private static bool SlugcatStats_HiddenOrUnplayableSlugcat(On.SlugcatStats.orig_HiddenOrUnplayableSlugcat orig, SlugcatStats.Name i)
    {
        var result = orig(i);

        if (i == Enums.SSOracle.PearlcatPebbles)
        {
            return true;
        }

        return result;
    }

    // Override shelter for trains and skips
    private static string SaveState_GetSaveStateDenToUse(On.SaveState.orig_GetSaveStateDenToUse orig, SaveState self)
    {
        var result = orig(self);

        var miscProg = Utils.GetMiscProgression();
        var miscWorld = self.miscWorldSaveData.GetMiscWorld();

        if (self.saveStateNumber == Enums.Pearlcat && miscProg.IsNewPearlcatSave)
        {
            if (!string.IsNullOrEmpty(ModOptions.StartShelterOverride.Value) && RainWorld.roomNameToIndex.ContainsKey(ModOptions.StartShelterOverride.Value))
            {
                return ModOptions.StartShelterOverride.Value;
            }
        }

        if (miscWorld?.JustMiraSkipped == true)
        {
            return "SS_AI";
        }

        if (result == "T1_S01")
        {
            return "SS_T1_S01";
        }

        return result;
    }



    // Custom Spears
    private static void Spear_Update_PearlSpear(On.Spear.orig_Update orig, Spear self, bool eu)
    {
        orig(self, eu);

        if (!self.abstractSpear.TryGetSpearModule(out var module))
        {
            return;
        }

        var returnTime = 60;
        var minSparkTime = 40;
        var maxSparkTime = 350;

        if (self.mode == Weapon.Mode.Thrown && module.DecayTimer == 0)
        {
            if (!module.WasThrown)
            {
                self.firstChunk.vel *= 2.0f;
            }

            module.WasThrown = true;
        }
        else
        {
            module.WasThrown = false;
        }


        if (module.SparkTimer <= 0 && module.DecayTimer < 200)
        {
            module.SparkTimer = Random.Range(minSparkTime, maxSparkTime);
        }
        else
        {
            module.SparkTimer--;
        }


        if (self.mode != Weapon.Mode.Thrown && module.ReturnTimer == -1)
        {
            if (self.mode == Weapon.Mode.StuckInWall)
            {
                module.DecayTimer++;
            }
            else
            {
                module.ReturnTimer = returnTime;
            }
        }

        if (module.ReturnTimer > 0)
        {
            module.ReturnTimer--;

            if (self.onPlayerBack || self.grabbedBy.Any(x => x.grabber is Player) || module.DecayTimer > 0)
            {
                module.ReturnTimer = -2;
            }
        }

        if (module.ReturnTimer == 0)
        {
            module.ReturnTimer = -2;

            if (module.ThrownByPlayer?.TryGetTarget(out var player) == true && player.TryGetPearlcatModule(out var playerModule))
            {
                var color = module.Color;
                var prevPos = self.firstChunk.pos;

                var freeHand = player.FreeHand();


                if ((player.GraspsHasType(AbstractPhysicalObject.AbstractObjectType.Spear) != -1 || freeHand == -1) && playerModule.Inventory.Count < ModOptions.MaxPearlCount.Value)
                {
                    if (self.room != null)
                    {
                        self.room.AddObject(new ShockWave(prevPos, 50.0f, 0.8f, 10));
                        self.room.AddObject(new ExplosionSpikes(self.room, prevPos, 10, 10.0f, 10, 10.0f, 80.0f, color));

                        self.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, prevPos, 1.0f, 3.5f);
                        self.room.PlaySound(Enums.Sounds.Pearlcat_PearlStore, player.firstChunk.pos, 1.0f, 1.0f);
                    }

                    self.AllGraspsLetGoOfThisObject(true);
                    player.StoreObject(self.abstractSpear, noSound: true, storeBeforeActive: true);
                }
                else if (freeHand != -1 && player.GraspsHasType(AbstractPhysicalObject.AbstractObjectType.Spear) == -1)
                {
                    if (self.room != null && player.graphicsModule != null)
                    {
                        self.room.AddObject(new ShockWave(prevPos, 50.0f, 0.8f, 10));
                        self.room.AddObject(new ExplosionSpikes(self.room, prevPos, 10, 10.0f, 10, 10.0f, 80.0f, color));

                        var handPos = ((PlayerGraphics)player.graphicsModule).hands[freeHand].pos;

                        self.room.AddObject(new ShockWave(handPos, 15.0f, 0.8f, 10));
                        self.room.AddObject(new ExplosionSpikes(self.room, handPos, 10, 5.0f, 10, 10.0f, 40.0f, color));

                        self.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, prevPos, 1.0f, 3.5f);
                    }

                    self.AllGraspsLetGoOfThisObject(true);
                    player.SlugcatGrab(self, freeHand);
                }
            }
        }
    }

    private static void Spear_DrawSprites_PearlSpear(On.Spear.orig_DrawSprites orig, Spear self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (!self.abstractSpear.TryGetSpearModule(out var module))
        {
            return;
        }

        var color =  module.Color
                     * Custom.HSL2RGB(1.0f, Custom.LerpMap(module.DecayTimer, 0, 200, 1.0f, 0.1f), Custom.LerpMap(module.DecayTimer, 0, 200, 1.0f, 0.05f));

        color = color.RWColorSafety();

        sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("pearlcat_spear");
        sLeaser.sprites[0].color = color;

        var randOffset = Custom.DegToVec(Random.value * 360f) * Custom.LerpMap(module.DecayTimer, 0, 150, 0.25f, 0.0f) * Random.value;
        sLeaser.sprites[0].x += randOffset.x;
        sLeaser.sprites[0].y += randOffset.y;

        var thrown = self.mode == Weapon.Mode.Thrown;

        if (module.SparkTimer == 0 || thrown && module.DecayTimer < 150)
        {
            var startPos = self.firstChunk.pos + Custom.DegToVec(sLeaser.sprites[0].rotation) * -30.0f;
            var endPos = self.firstChunk.pos + Custom.DegToVec(sLeaser.sprites[0].rotation) * 30.0f;
            self.room.ConnectEffect(startPos, endPos, module.Color, thrown ? 0.5f : 0.75f, thrown ? 6 : 12);
        }
    }


    // Room & World Loading
    private static void RoomSpecificScript_AddRoomSpecificScript(On.RoomSpecificScript.orig_AddRoomSpecificScript orig, Room room)
    {
        orig(room);


        if (room.roomSettings.name == "T1_S01")
        {
            room.AddObject(new T1_S01(room));
        }

        if (room.roomSettings.name == "SS_T1_S01")
        {
            room.AddObject(new SS_T1_S01(room));
        }

        if (room.roomSettings.name == "SS_T1_CROSS" && !ModCompat_Helpers.IsModEnabled_MiraInstallation)
        {
            room.AddObject(new SS_T1_CROSS(room));
        }


        if (!room.game.IsPearlcatStory())
        {
            return;
        }

        var miscProg = Utils.GetMiscProgression();
        var everVisited = room.game.GetStorySession.saveState.regionStates[room.world.region.regionNumber].roomsVisited.Contains(room.abstractRoom.name);

        // Tutorial
        if (!everVisited)
        {
            // Start
            if (room.roomSettings.name == "T1_START")
            {
                room.AddObject(new T1_START(room));
            }

            // Rage (+ Possession)
            if (room.roomSettings.name == "T1_CAR2")
            {
                room.AddObject(new T1_CAR2(room));
            }

            if (!miscProg.HasTrueEnding)
            {
                // Agility
                if (room.roomSettings.name == "T1_CAR0")
                {
                    room.AddObject(new T1_CAR0(room));
                }

                // Shield
                if (room.roomSettings.name == "T1_CAR1")
                {
                    room.AddObject(new T1_CAR1(room));
                }

                // Revive
                if (room.roomSettings.name == "T1_CAR3")
                {
                    room.AddObject(new T1_CAR3(room));
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

    private static void Room_Update(On.Room.orig_Update orig, Room self)
    {
        orig(self);

        var save = Utils.GetMiscProgression();

        if (TrainViewRooms.Contains(self.roomSettings.name))
        {
            var intensity = self.roomSettings.name == "T1_END" ? 0.15f : 0.1f;
            self.ScreenMovement(null, Vector2.right * 3.0f, intensity);

            if (save.HasTrueEnding)
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

        // Outside train wind effect
        if (self.roomSettings.name == "T1_END")
        {
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

                List<Player.BodyModeIndex> exemptBodyModes = new()
                {
                    Player.BodyModeIndex.Crawl,
                    Player.BodyModeIndex.ClimbIntoShortCut,
                    Player.BodyModeIndex.CorridorClimb,
                };

                var target = player.canJump == 0 ? 1.0f : 0.85f;

                if (!player.TryGetPearlcatModule(out var playerModule))
                {
                    continue;
                }

                if (playerModule.EarL == null || playerModule.EarR == null)
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
        }
    }

    private static void RegionState_AdaptRegionStateToWorld(On.RegionState.orig_AdaptRegionStateToWorld orig, RegionState self, int playerShelter, int activeGate)
    {
        try
        {
            for (var i = 0; i < self.world.NumberOfRooms; i++)
            {
                var abstractRoom = self.world.GetAbstractRoom(self.world.firstRoomIndex + i);

                for (var j = abstractRoom.entities.Count - 1; j >= 0; j--)
                {
                    var entity = abstractRoom.entities[j];

                    if (entity is not AbstractPhysicalObject abstractObject)
                    {
                        continue;
                    }

                    if (abstractObject.IsPlayerPearl())
                    {
                        if (abstractObject.world.game.IsStorySession)
                        {
                            abstractObject.world.game.GetStorySession.RemovePersistentTracker(abstractObject);
                        }

                        abstractRoom.RemoveEntity(entity);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Plugin.Logger.LogWarning("ERROR REMOVING PERSISTENT TRACKERS FROM STORED OBJECTS: \n" + e);
        }

        orig(self, playerShelter, activeGate);
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


    // Prevent PlayerPearls being saved in the shelter
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

    // Reset this here instead, better for compat
    private static void AboveCloudsView_ctor(On.AboveCloudsView.orig_ctor orig, AboveCloudsView self, Room room, RoomSettings.RoomEffect effect)
    {
        if (Shader.GetGlobalFloat("_windDir") == TrainView.TRAIN_WIND_DIR)
        {
            Shader.SetGlobalFloat("_windDir", ModManager.MSC ? -1.0f : 1.0f);
        }

        orig(self, room, effect);
    }


    // CW Unique Pearl Colour
    private static Color? DataPearl_UniquePearlHighLightColor(On.DataPearl.orig_UniquePearlHighLightColor orig, DataPearl.AbstractDataPearl.DataPearlType pearlType)
    {
        if (pearlType == Enums.Pearls.CW_Pearlcat)
        {
            return Custom.hexToColor("0077ff");
        }

        return orig(pearlType);
    }


    // Pearl Spears unique sounds
    private static ChunkSoundEmitter Room_PlaySound_SoundID_BodyChunk(On.Room.orig_PlaySound_SoundID_BodyChunk orig, Room self, SoundID soundId, BodyChunk chunk)
    {
        if (chunk?.owner is Spear spear && spear.abstractSpear.TryGetSpearModule(out var spearModule) && spearModule.DecayTimer == 0)
        {
            if (soundId == SoundID.Spear_Bounce_Off_Creauture_Shell)
            {
                return self.PlaySound(soundId, chunk, false, 1f, 1.5f);
            }

            if (soundId == SoundID.Spear_Stick_In_Creature)
            {
                return self.PlaySound(soundId, chunk, false, 1f, 1.5f);
            }

            if (soundId == SoundID.Spear_Stick_In_Ground)
            {
                return self.PlaySound(soundId, chunk, false, 1f, 1.5f);
            }

            if (soundId == SoundID.Spear_Stick_In_Wall)
            {
                return self.PlaySound(soundId, chunk, false, 1f, 1.5f);
            }

            if (soundId == SoundID.Spear_Dislodged_From_Creature)
            {
                return self.PlaySound(soundId, chunk, false, 1f, 1.5f);
            }

            if (soundId == SoundID.Slugcat_Throw_Spear)
            {
                return self.PlaySound(soundId, chunk, false, 1f, 1.2f);
            }

            if (soundId == SoundID.Spear_Bounce_Off_Wall)
            {
                self.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, chunk, false, 0.5f, Random.Range(1.5f, 2.0f));

                return self.PlaySound(soundId, chunk, false, 1f, 1.2f);
            }
        }

        return orig(self, soundId, chunk);
    }

}
