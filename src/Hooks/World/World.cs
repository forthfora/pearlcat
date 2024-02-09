using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MoreSlugcats;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;

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


        On.KingTusks.Tusk.ShootUpdate += Tusk_ShootUpdate;
        On.KingTusks.Tusk.Update += Tusk_Update;

        On.Spear.DrawSprites += Spear_DrawSprites;
        On.Spear.Update += Spear_Update;

        On.BigNeedleWorm.Swish += BigNeedleWorm_Swish;

        On.SaveState.GetSaveStateDenToUse += SaveState_GetSaveStateDenToUse;

        On.SlugcatStats.HiddenOrUnplayableSlugcat += SlugcatStats_HiddenOrUnplayableSlugcat;
        
        On.GateKarmaGlyph.ctor += GateKarmaGlyph_ctor;

        new Hook(
            typeof(RegionGate).GetProperty(nameof(RegionGate.MeetRequirement), BindingFlags.Instance | BindingFlags.Public).GetGetMethod(),
            typeof(Hooks).GetMethod(nameof(GetRegionGateMeetRequirement), BindingFlags.Static | BindingFlags.Public)
        );

        new Hook(
            typeof(StoryGameSession).GetProperty(nameof(StoryGameSession.slugPupMaxCount), BindingFlags.Instance | BindingFlags.Public).GetGetMethod(),
            typeof(Hooks).GetMethod(nameof(GetStoryGameSessionSlugPupMaxCount), BindingFlags.Static | BindingFlags.Public)
        );

        On.SporePlant.AttachedBee.Update += AttachedBee_Update;

        On.WormGrass.WormGrassPatch.Update += WormGrassPatch_Update;
        On.WormGrass.Worm.Attached += Worm_Attached;

        On.RegionGate.customOEGateRequirements += RegionGate_customOEGateRequirements;

        On.VultureMask.DrawSprites += VultureMask_DrawSprites;

        IL.AbstractRoom.RealizeRoom += AbstractRoom_RealizeRoom;

        On.MoreSlugcats.MSCRoomSpecificScript.OE_GourmandEnding.Update += OE_GourmandEnding_Update;

        IL.RainWorldGame.BeatGameMode += RainWorldGame_BeatGameMode;

        On.RegionState.AdaptWorldToRegionState += RegionState_AdaptWorldToRegionState;

        On.DaddyTentacle.Update += DaddyTentacle_Update;
        On.DaddyLongLegs.Update += DaddyLongLegs_Update;

        On.ScavengerAI.CollectScore_PhysicalObject_bool += ScavengerAI_CollectScore_PhysicalObject_bool1;

        IL.BigEel.JawsSnap += BigEel_JawsSnap;

        On.TempleGuardAI.ThrowOutScore += TempleGuardAI_ThrowOutScore;

        On.Leech.Attached += Leech_Attached;

        On.PlacedObject.FilterData.FromString += FilterData_FromString;

        IL.Region.GetFullRegionOrder += Region_GetFullRegionOrder;

        On.DreamsState.StaticEndOfCycleProgress += DreamsState_StaticEndOfCycleProgress;
        On.RainWorldGame.ctor += RainWorldGame_ctor;

        On.AboveCloudsView.ctor += AboveCloudsView_ctor;
    }

    private static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
    {
        orig(self, manager);

        if (!self.IsStorySession) return;

        if (self.StoryCharacter != Enums.Pearlcat) return;

        var save = self.GetStorySession.saveState;
        var miscWorld = self.GetMiscWorld();
        var miscProg = self.GetMiscProgression();

        if (miscWorld == null) return;


        if (miscWorld.HasPearlpupWithPlayer)
        {
            var canDream = save.cycleNumber > 4 && Random.Range(0.0f, 1.0f) < 0.2f;

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
        else if (miscProg.HasTrueEnding)
        {
            var canDream = Random.Range(0.0f, 1.0f) < 0.1f;

            if (canDream)
            {
                var dreamPool = new List<DreamsState.DreamID>()
                {
                    Enums.Dreams.Dream_Pearlcat_Sick,
                    Enums.Dreams.Dream_Pearlcat_Pearlpup,
                    Enums.Dreams.Dream_Pearlcat_Pebbles,
                    Enums.Dreams.Dream_Pearlcat_Moon_Sick,
                };

                var randState = Random.state;
                Random.InitState((int)DateTime.Now.Ticks);

                self.GetStorySession.TryDream(dreamPool[Random.Range(0, dreamPool.Count)], true);

                Random.state = randState;
            }
        }
    }

    private static void DreamsState_StaticEndOfCycleProgress(On.DreamsState.orig_StaticEndOfCycleProgress orig, SaveState saveState, string currentRegion, string denPosition, ref int cyclesSinceLastDream, ref int cyclesSinceLastFamilyDream, ref int cyclesSinceLastGuideDream, ref int inGWOrSHCounter, ref DreamsState.DreamID upcomingDream, ref DreamsState.DreamID eventDream, ref bool everSleptInSB, ref bool everSleptInSB_S01, ref bool guideHasShownHimselfToPlayer, ref int guideThread, ref bool guideHasShownMoonThisRound, ref int familyThread)
    {
        if (saveState.saveStateNumber == Enums.Pearlcat && (eventDream == null || !eventDream.value.Contains("Pearlcat"))) return;

        orig(saveState, currentRegion, denPosition, ref cyclesSinceLastDream, ref cyclesSinceLastFamilyDream, ref cyclesSinceLastGuideDream, ref inGWOrSHCounter, ref upcomingDream, ref eventDream, ref everSleptInSB, ref everSleptInSB_S01, ref guideHasShownHimselfToPlayer, ref guideThread, ref guideHasShownMoonThisRound, ref familyThread);
    }

    private static void Region_GetFullRegionOrder(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(MoveType.Before,
            x => x.MatchRet());

        c.EmitDelegate<Func<List<string>, List<string>>>((list) =>
        {
            list.Remove("T1");

            return list;
        });
    }

    private static void FilterData_FromString(On.PlacedObject.FilterData.orig_FromString orig, PlacedObject.FilterData self, string s)
    {
        orig(self, s);

        if (!self.availableToPlayers.Contains(SlugcatStats.Name.Red) && self.availableToPlayers.Contains(Enums.Pearlcat))
        {
            self.availableToPlayers.Remove(Enums.Pearlcat);
        }
    }

    private static void RegionState_AdaptWorldToRegionState(On.RegionState.orig_AdaptWorldToRegionState orig, RegionState self)
    {
        //var save = self.world.game.GetMiscWorld();
 
        //if (save != null && save.JustBeatAltEnd)
        //{
        //    save.JustBeatAltEnd = false;

        //    foreach (var item in self.unrecognizedPopulation)
        //    {
        //        Plugin.Logger.LogWarning(item);
        //        //var obj = SaveState.AbstractPhysicalObjectFromString(self.world, item);

        //        //if (obj.ID.number != save.PearlpupID) continue;
                
        //        //self.savedPopulation.Remove(item);
        //        //Plugin.Logger.LogWarning("ACK");
        //        //break;
        //    }

        //    Plugin.Logger.LogWarning("-------------------");
        //}

        orig(self);
    }

    private static void RainWorldGame_BeatGameMode(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(MoveType.After,
            x => x.MatchLdstr("OE_SEXTRA"),
            x => x.MatchStloc(0));

        c.GotoNext(MoveType.After,
            x => x.MatchStloc(1));


        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldloc_0);
        c.EmitDelegate<Func<RainWorldGame, string, string>>((game, roomName) =>
        {
            if (game.GetStorySession.saveStateNumber == Enums.Pearlcat)
            {   
                var deathSave = game.GetStorySession.saveState.deathPersistentSaveData;
                deathSave.karma = deathSave.karmaCap;
                
                var miscProg = game.GetMiscProgression();

                miscProg.IsPearlpupSick = true;
                miscProg.HasOEEnding = true;

                var miscWorld = game.GetMiscWorld();

                if (miscWorld != null)
                    miscWorld.JustBeatAltEnd = true;

                Plugin.Logger.LogInfo("PEARLCAT OE ENDING");

                return "OE_SEXTRA";
            }

            return roomName;
        });

        c.Emit(OpCodes.Stloc_0);
    }

    private static void OE_GourmandEnding_Update(On.MoreSlugcats.MSCRoomSpecificScript.OE_GourmandEnding.orig_Update orig, MSCRoomSpecificScript.OE_GourmandEnding self, bool eu)
    {
        if (self.room.world.game.IsPearlcatStory())
        {
            var miscWorld = self.room.world.game.GetMiscWorld();
            var miscProg = self.room.world.game.GetMiscProgression();

            if (miscWorld?.HasPearlpupWithPlayer == false) return;

            if (miscProg.HasOEEnding) return;

            if (miscProg.HasTrueEnding) return;
        }

        orig(self, eu);
    }

    private static void AbstractRoom_RealizeRoom(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(MoveType.After,
            x => x.MatchStloc(0));

        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldarg_2);
        c.Emit(OpCodes.Ldloc_0);
        c.EmitDelegate<Func<AbstractRoom, RainWorldGame, int, int>>((self, game, num) =>
        {
            if (game.IsStorySession && game.StoryCharacter == Enums.Pearlcat)
            {
                var save = game.GetMiscWorld();
                var miscProg = game.GetMiscProgression();

                if (save?.PearlpupID == null && ModOptions.PearlpupRespawn.Value && !miscProg.HasTrueEnding)
                    return 0;

                return int.MaxValue;
            }

            return num;
        });

        c.Emit(OpCodes.Stloc_0);
    }


    private static void VultureMask_DrawSprites(On.VultureMask.orig_DrawSprites orig, VultureMask self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        //Vector2 vector = Vector2.Lerp(self.firstChunk.lastPos, self.firstChunk.pos, timeStacker);
        //Vector2 vector2 = Vector3.Slerp(self.lastRotationA, self.rotationA, timeStacker);
        //Vector2 vector3 = Vector3.Slerp(self.lastRotationB, self.rotationB, timeStacker);
        
        float donnedLerp = Mathf.Lerp(self.lastDonned, self.donned, timeStacker);

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
            wasPlayer.eatCounter = (int)wasEatCounter;
    }

    private static bool RegionGate_customOEGateRequirements(On.RegionGate.orig_customOEGateRequirements orig, RegionGate self)
    {
        var result = orig(self);

        //if (self.room.game.IsPearlcatStory() && self.room.game.IsStorySession && self.room.game.GetStorySession.saveState.denPosition.Contains("OE_"))
        //    return true;

        if (self.room.game.IsPearlcatStory())
            return true;

        return result;
    }


    public delegate int orig_StoryGameSessionSlugPupMaxCount(StoryGameSession self);
    public static int GetStoryGameSessionSlugPupMaxCount(orig_StoryGameSessionSlugPupMaxCount orig, StoryGameSession self)
    {
        var result = orig(self);

        if (self.saveStateNumber == Enums.Pearlcat)
        {
            var save = self.saveState.miscWorldSaveData.GetMiscWorld();

            if (save != null && save.PearlpupID == null)
                return 1;

            return 0;
        }

        return result;
    }

    
    public delegate bool orig_RegionGateMeetRequirement(RegionGate self);
    public static bool GetRegionGateMeetRequirement(orig_RegionGateMeetRequirement orig, RegionGate self)
    {
        var result = orig(self);

        if (self.IsGateOpenForPearlcat())
            return true;

        return result;
    }

    private static void GateKarmaGlyph_ctor(On.GateKarmaGlyph.orig_ctor orig, GateKarmaGlyph self, bool side, RegionGate gate, RegionGate.GateRequirement requirement)
    {
        orig(self, side, gate, requirement);

        if (!gate.IsGateOpenForPearlcat()) return;

        self.requirement = RegionGate.GateRequirement.OneKarma;
    }

    public static bool IsGateOpenForPearlcat(this RegionGate gate)
    {
        var roomName = gate.room?.roomSettings?.name;

        if (gate.room == null || roomName == null)
            return false;

        if (!gate.room.game.IsPearlcatStory())
            return false;


        if (roomName == "GATE_UW_LC")
            return true;

        if (roomName == "GATE_SL_MS")
            return true;


        return false;
    }


    private static bool SlugcatStats_HiddenOrUnplayableSlugcat(On.SlugcatStats.orig_HiddenOrUnplayableSlugcat orig, SlugcatStats.Name i)
    {
        var result = orig(i);

        if (i == Enums.SSOracle.PearlcatPebbles)
            return true;

        return result;
    }

    private static string SaveState_GetSaveStateDenToUse(On.SaveState.orig_GetSaveStateDenToUse orig, SaveState self)
    {
        var result = orig(self);

        var miscProg = self.progression.miscProgressionData.GetMiscProgression();

        if (self.saveStateNumber == Enums.Pearlcat && miscProg.IsNewPearlcatSave)
        {
            if (!string.IsNullOrEmpty(ModOptions.StartShelterOverride.Value) && RainWorld.roomNameToIndex.ContainsKey(ModOptions.StartShelterOverride.Value))
            {
                return ModOptions.StartShelterOverride.Value;
            }
        }

        if (miscProg.IsMiraSkipEnabled)
        {
            return "SS_AI";
        }

        if (result == "T1_S01")
        {
            return ModManager.MSC ? "LC_T1_S01" : "SS_S04";
        }

        return result;
    }

    private static void Spear_Update(On.Spear.orig_Update orig, Spear self, bool eu)
    {
        orig(self, eu);

        if (!self.abstractSpear.TryGetSpearModule(out var module)) return;

        if (self.mode == Weapon.Mode.Thrown)
        {
            if (!module.WasThrown)
                self.firstChunk.vel *= 1.5f;
            
            module.WasThrown = true;
        }
        else
        {
            module.WasThrown = false;
        }


        if (module.SparkTimer <= 0 && module.DecayTimer < 800)
            module.SparkTimer = Random.Range(40, 350);

        else
            module.SparkTimer--;


        if (self.mode == Weapon.Mode.StuckInCreature && module.ExplodeTimer == -1)
        {
            self.room.PlaySound(SoundID.Fire_Spear_Ignite, self.firstChunk, false, 0.7f, 2.5f);
            module.ExplodeTimer = 40;
        }
    
        if (module.ExplodeTimer == 0)
        {
            if (self.stuckInObject != null)
            {
                if (self.stuckInObject is Creature creature)
                    creature.Violence(self.firstChunk, new Vector2?(self.rotation * 12f), self.stuckInChunk, null, Creature.DamageType.Explosion, (self.stuckInAppendage != null) ? 2.2f : 5.0f, 120f);

                self.stuckInChunk.vel += self.rotation * 12f / self.stuckInChunk.mass;
            }

            for (int i = 0; i < 3; i++)
                self.room.AddObject(new ExplosiveSpear.SpearFragment(self.firstChunk.pos, Custom.RNV() * Mathf.Lerp(20f, 40f, Random.value)));

            self.room.AddObject(new ShockWave(self.firstChunk.pos, 120.0f, 1.0f, 40));

            var pos = self.firstChunk.pos;
            self.room.AddObject(new Explosion.ExplosionLight(pos, 160f, 1f, 3, module.Color));
            self.room.AddObject(new ExplosionSpikes(self.room, pos, 9, 4f, 5f, 5f, 90f, module.Color));

            self.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, self.firstChunk.pos, Random.Range(1.2f, 1.6f), Random.Range(0.6f, 0.8f));
            self.room.PlaySound(SoundID.Bomb_Explode, self.firstChunk.pos, Random.Range(0.8f, 1.2f), Random.Range(0.8f, 1.2f));

            self.room.AddObject(new Explosion(self.room, self, pos, 5, 110f, 5f, 1.1f, 60f, 0.3f, self.thrownBy, 0.8f, 0f, 0.7f));

            self.Destroy();
        }
        else if (module.ExplodeTimer > 0)
        {
            if (module.ExplodeTimer % 3 == 0)
                self.ConnectEffect(self.firstChunk.pos + Custom.RNV() * 100.0f, module.Color);
            
            module.ExplodeTimer--;   
        }


        if (self.mode == Weapon.Mode.StuckInWall)
            module.DecayTimer++;
    }

    public static bool TryGetSpearModule(this AbstractSpear spear, out SpearModule module)
    {
        var save = spear.Room.world.game.GetMiscWorld();

        if (save == null)
        {
            module = null!;
            return false;
        }

        if (save.PearlSpears.TryGetValue(spear.ID.number, out module))
            return true;

        return false;
    }

    private static void Spear_DrawSprites(On.Spear.orig_DrawSprites orig, Spear self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (!self.abstractSpear.TryGetSpearModule(out var module)) return;

        var color =  module.Color * Custom.HSL2RGB(1.0f, Custom.LerpMap(module.DecayTimer, 0, 1200, 1.0f, 0.5f), Custom.LerpMap(module.DecayTimer, 0, 1200, 1.0f, 0.2f));

        sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("pearlcat_spear");
        sLeaser.sprites[0].color = color;

        var randOffset = Custom.DegToVec(Random.value * 360f) * Custom.LerpMap(module.DecayTimer, 400, 1200, 0.25f, 0.0f) * Random.value;
        sLeaser.sprites[0].x += randOffset.x;
        sLeaser.sprites[0].y += randOffset.y;

        var thrown = self.mode == Weapon.Mode.Thrown;

        if (module.SparkTimer == 0 || thrown && module.DecayTimer < 800)
        {
            var startPos = self.firstChunk.pos + Custom.DegToVec(sLeaser.sprites[0].rotation) * -30.0f;
            var endPos = self.firstChunk.pos + Custom.DegToVec(sLeaser.sprites[0].rotation) * 30.0f;
            self.room.ConnectEffect(startPos, endPos, module.Color, thrown ? 0.5f : 0.75f, thrown ? 6 : 12);
        }
    }

    private static void RoomSpecificScript_AddRoomSpecificScript(On.RoomSpecificScript.orig_AddRoomSpecificScript orig, Room room)
    {
        orig(room);

        if (!room.game.IsPearlcatStory()) return;

        var save = room.abstractRoom.world.game.GetMiscProgression();


        if (room.roomSettings.name == "T1_S01")
            room.AddObject(new T1_S01(room));

        if (room.roomSettings.name == "LC_T1_S01")
            room.AddObject(new LC_T1_S01(room));


        if (!room.abstractRoom.firstTimeRealized) return;


        // Tutorial

        // Start
        if (room.roomSettings.name == "T1_START")
            room.AddObject(new T1_START(room));



        if (save.HasTrueEnding) return;

        // Agility
        if (room.roomSettings.name == "T1_CAR0")
            room.AddObject(new T1_CAR0(room));

        // Shield
        if (room.roomSettings.name == "T1_CAR1")
            room.AddObject(new T1_CAR1(room));

        // Rage
        if (room.roomSettings.name == "T1_CAR2")
            room.AddObject(new T1_CAR2(room));

        // Revive
        if (room.roomSettings.name == "T1_CAR3")
            room.AddObject(new T1_CAR3(room));
    }


    public static List<string> TrainViewRooms { get; } = new()
    {
        "T1_START",
        "T1_CAR0",
        "T1_CAR1",
        "T1_CAR2",
        "T1_CAR3",
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

        var save = Utils.GetMiscProgression();

        if (TrainViewRooms.Contains(self.roomSettings.name))
        {
            var intensity = self.roomSettings.name == "T1_END" ? 0.15f : 0.1f;
            self.ScreenMovement(null, Vector2.right * 3.0f, intensity);

            if (save.HasTrueEnding)
            {
                foreach (var camera in self.game.cameras)
                {
                    camera.ChangeMainPalette(301);
                }
            }
        }

        // Outside train wind effect
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
        try
        {
            for (int i = 0; i < self.world.NumberOfRooms; i++)
            {
                var abstractRoom = self.world.GetAbstractRoom(self.world.firstRoomIndex + i);

                for (int j = abstractRoom.entities.Count - 1; j >= 0; j--)
                {
                    var entity = abstractRoom.entities[j];

                    if (entity is not AbstractPhysicalObject abstractObject) continue;

                    if (abstractObject.IsPlayerObject())
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

    // Reset this here instead, better for compat
    private static void AboveCloudsView_ctor(On.AboveCloudsView.orig_ctor orig, AboveCloudsView self, Room room, RoomSettings.RoomEffect effect)
    {
        if (Shader.GetGlobalFloat("_windDir") == TrainView.TRAIN_WIND_DIR)
        {
            Shader.SetGlobalFloat("_windDir", ModManager.MSC ? -1.0f : 1.0f);
        }

        orig(self, room, effect);
    }
}
