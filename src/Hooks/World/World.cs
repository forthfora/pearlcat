using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MoreSlugcats;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Pearlcat;

public partial class Hooks
{
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

        On.Spear.DrawSprites += Spear_DrawSprites_PearlSpear;
        On.Spear.Update += Spear_Update_PearlSpear;

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

        On.Spear.Update += Spear_Update_RageSpear;
        On.Spear.DrawSprites += Spear_DrawSprites_RageSpear;
    }



    // Manage dreams
    private static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
    {
        orig(self, manager);

        if (!self.IsStorySession) return;

        if (self.StoryCharacter != Enums.Pearlcat) return;

        var save = self.GetStorySession.saveState;
        var miscWorld = self.GetMiscWorld();
        var miscProg = Utils.GetMiscProgression();

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


    // Remove transit system from the Regions menu
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


    // Fix for DevTools timeline object filters, make it copy hunters
    private static void FilterData_FromString(On.PlacedObject.FilterData.orig_FromString orig, PlacedObject.FilterData self, string s)
    {
        orig(self, s);

        if (!self.availableToPlayers.Contains(SlugcatStats.Name.Red) && self.availableToPlayers.Contains(Enums.Pearlcat))
        {
            self.availableToPlayers.Remove(Enums.Pearlcat);
        }
    }


    // Outer Expanse Ending
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
                
                var miscProg = Utils.GetMiscProgression();

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


    // Block OE ending under some conditions
    private static void OE_GourmandEnding_Update(On.MoreSlugcats.MSCRoomSpecificScript.OE_GourmandEnding.orig_Update orig, MSCRoomSpecificScript.OE_GourmandEnding self, bool eu)
    {
        if (self.room.world.game.IsPearlcatStory())
        {
            var miscWorld = self.room.world.game.GetMiscWorld();
            var miscProg = Utils.GetMiscProgression();

            if (miscWorld?.HasPearlpupWithPlayer == false) return;

            if (miscProg.HasOEEnding) return;

            if (miscProg.HasTrueEnding) return;
        }

        orig(self, eu);
    }


    // Make pup spawns guaranteed when the Pearlpup respawn cheat is enabled and pearlpup is missing
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


    // Make vulture masks raised when pearlcat isn't moving (the only reason i did this is because I found them ugly lol)
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


    // Unlock OE Gate
    private static bool RegionGate_customOEGateRequirements(On.RegionGate.orig_customOEGateRequirements orig, RegionGate self)
    {
        var result = orig(self);

        //if (self.room.game.IsPearlcatStory() && self.room.game.IsStorySession && self.room.game.GetStorySession.saveState.denPosition.Contains("OE_"))
        //    return true;

        if (self.room.game.IsPearlcatStory())
            return true;

        return result;
    }



    // Only let pups spawn if pearlpup is missing
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



    // Unlock certain gates (Bitter Aerie, Metropolis)
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



    // Hide the ID used for pebbles pearl readings
    private static bool SlugcatStats_HiddenOrUnplayableSlugcat(On.SlugcatStats.orig_HiddenOrUnplayableSlugcat orig, SlugcatStats.Name i)
    {
        var result = orig(i);

        if (i == Enums.SSOracle.PearlcatPebbles)
            return true;

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

        if (!self.abstractSpear.TryGetSpearModule(out var module)) return;

        var returnTime = 60;
        var minSparkTime = 40;
        var maxSparkTime = 350;

        if (self.mode == Weapon.Mode.Thrown && module.DecayTimer == 0)
        {
            if (!module.WasThrown)
            {
                self.firstChunk.vel *= 1.25f;
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

                    player.SlugcatGrab(self, freeHand);
                }
            }
        }
    }

    private static void Spear_DrawSprites_PearlSpear(On.Spear.orig_DrawSprites orig, Spear self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (!self.abstractSpear.TryGetSpearModule(out var module)) return;

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


    private static void Spear_DrawSprites_RageSpear(On.Spear.orig_DrawSprites orig, Spear self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (!self.TryGetRageSpearModule(out var module)) return;

        sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("pearlcat_ragespear");
        sLeaser.sprites[0].color = module.Color;
    }

    private static void Spear_Update_RageSpear(On.Spear.orig_Update orig, Spear self, bool eu)
    {
        orig(self, eu);

        if (!self.TryGetRageSpearModule(out var module)) return;
        
        if (self.mode != Weapon.Mode.Thrown && self.mode != Weapon.Mode.Carried)
        {
            self.room.AddObject(new ShockWave(self.firstChunk.pos, 10.0f, 0.5f, 8));

            self.Destroy();
        }
    }



    // Room & World Loading
    private static void RoomSpecificScript_AddRoomSpecificScript(On.RoomSpecificScript.orig_AddRoomSpecificScript orig, Room room)
    {
        orig(room);

        if (!room.game.IsPearlcatStory()) return;

        var miscProg = Utils.GetMiscProgression();

        if (room.roomSettings.name == "T1_S01")
            room.AddObject(new T1_S01(room));

        if (room.roomSettings.name == "SS_T1_S01")
            room.AddObject(new SS_T1_S01(room));

        if (room.roomSettings.name == "SS_T1_CROSS" && !Utils.IsMiraActive)
            room.AddObject(new SS_T1_CROSS(room));


        if (!room.abstractRoom.firstTimeRealized) return;


        // Tutorial

        // Start
        if (room.roomSettings.name == "T1_START")
            room.AddObject(new T1_START(room));

        // Rage (+ Possession)
        if (room.roomSettings.name == "T1_CAR2")
            room.AddObject(new T1_CAR2(room));



        if (miscProg.HasTrueEnding) return;

        // Agility
        if (room.roomSettings.name == "T1_CAR0")
            room.AddObject(new T1_CAR0(room));

        // Shield
        if (room.roomSettings.name == "T1_CAR1")
            room.AddObject(new T1_CAR1(room));

        // Revive
        if (room.roomSettings.name == "T1_CAR3")
            room.AddObject(new T1_CAR3(room));
    }

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
                    if (camera.paletteA != 301)
                    {
                        camera.ChangeMainPalette(301);
                    }
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

    

    // Shelter
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
