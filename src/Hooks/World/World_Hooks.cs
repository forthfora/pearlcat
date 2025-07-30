using MoreSlugcats;
using RWCustom;
using static Pearlcat.World_Helpers;

namespace Pearlcat;

public static class World_Hooks
{
    public static void ApplyHooks()
    {
        On.HUD.Map.GetItemInShelterFromWorld += Map_GetItemInShelterFromWorld;

        On.RegionState.AdaptRegionStateToWorld += RegionState_AdaptRegionStateToWorld;

        On.Spear.DrawSprites += Spear_DrawSprites_PearlSpear;
        On.Spear.Update += Spear_Update_PearlSpear;

        On.SlugcatStats.HiddenOrUnplayableSlugcat += SlugcatStats_HiddenOrUnplayableSlugcat;

        On.GateKarmaGlyph.ctor += GateKarmaGlyph_ctor;

        On.RegionGate.customOEGateRequirements += RegionGate_customOEGateRequirements;

        On.VultureMask.DrawSprites += VultureMask_DrawSprites;

        On.MoreSlugcats.MSCRoomSpecificScript.OE_GourmandEnding.Update += OE_GourmandEnding_Update;

        On.PlacedObject.FilterData.FromString += FilterData_FromString;

        On.RainWorldGame.ctor += RainWorldGame_ctor;

        On.DataPearl.UniquePearlHighLightColor += DataPearl_UniquePearlHighLightColor;
        On.Room.PlaySound_SoundID_BodyChunk += Room_PlaySound_SoundID_BodyChunk;

        On.SaveState.GetSaveStateDenToUse += SaveState_GetSaveStateDenToUse;
        On.OverWorld.WorldLoaded += OverWorldOnWorldLoaded;
    }

    // Meadow Gate Fix (inform meadow that the pearls are changing world)
    private static void OverWorldOnWorldLoaded(On.OverWorld.orig_WorldLoaded orig, OverWorld self, bool warpused)
    {
        if (ModCompat_Helpers.RainMeadow_IsOnline)
        {
            foreach (var playerModule in self.game.GetAllPearlcatModules())
            {
                playerModule.PlayerRef?.TryAbstractInventory(true);
            }
        }

        var newWorld = self.worldLoader?.world;

        orig(self, warpused);

        if (!ModCompat_Helpers.RainMeadow_IsOnline)
        {
            return;
        }

        if (newWorld is null)
        {
            return;
        }

        foreach (var playerModule in self.game.GetAllPearlcatModules())
        {
            foreach (var item in playerModule.Inventory)
            {
                MeadowCompat.ApoEnteringWorld(item, newWorld);
            }
        }
    }

    // Override shelter for trains and skips
    private static string SaveState_GetSaveStateDenToUse(On.SaveState.orig_GetSaveStateDenToUse orig, SaveState self)
    {
        var result = orig(self);

        var miscProg = Utils.MiscProgression;
        var miscWorld = self.miscWorldSaveData?.GetMiscWorld();

        if (self.saveStateNumber == Enums.Pearlcat && miscProg.IsNewPearlcatSave)
        {
            if (!string.IsNullOrEmpty(ModOptions.StartShelterOverride) && RainWorld.roomNameToIndex.ContainsKey(ModOptions.StartShelterOverride))
            {
                return ModOptions.StartShelterOverride;
            }
        }

        if (miscWorld?.JustStorySkipped == true)
        {
            return "SS_AI";
        }

        if (result == "T1_S01")
        {
            return "SS_T1_S01";
        }

        return result;
    }


    // Manage dreams
    private static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
    {
        orig(self, manager);

        // Reset for Meadow
        if (ModCompat_Helpers.RainMeadow_IsOnline)
        {
            MeadowCompat.SetWasSaveDataSynced(ModCompat_Helpers.RainMeadow_IsHost);
        }


        if (!self.IsPearlcatStory())
        {
            return;
        }

        var miscWorld = self.GetMiscWorld();

        if (miscWorld is null)
        {
            return;
        }


        var dreamPool = GetDreamPool(self, miscWorld);

        if (dreamPool.Count == 0)
        {
            return;
        }

        var randState = Random.state;
        Random.InitState((int)DateTime.Now.Ticks);

        self.GetStorySession.TryDream(dreamPool[Random.Range(0, dreamPool.Count)], false);

        Random.state = randState;
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
            var miscProg = Utils.MiscProgression;

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

        if (wasPlayer is not null && wasEatCounter is not null)
        {
            wasPlayer.eatCounter = (int)wasEatCounter;
        }
    }


    // Unlock OE Gate
    private static bool RegionGate_customOEGateRequirements(On.RegionGate.orig_customOEGateRequirements orig, RegionGate self)
    {
        var result = orig(self);

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

        if (i == Enums.Oracle.PearlcatPebbles)
        {
            return true;
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


                if ((player.GraspsHasType(AbstractPhysicalObject.AbstractObjectType.Spear) != -1 || freeHand == -1) && playerModule.Inventory.Count < ModOptions.MaxPearlCount)
                {
                    if (self.room is not null)
                    {
                        self.room.AddObject(new ShockWave(prevPos, 50.0f, 0.8f, 10));
                        self.room.AddObject(new ExplosionSpikes(self.room, prevPos, 10, 10.0f, 10, 10.0f, 80.0f, color));

                        self.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, player.firstChunk.pos, 0.8f, 3.5f);
                        self.room.PlaySound(Enums.Sounds.Pearlcat_PearlStore, player.firstChunk.pos, 1.0f, 1.0f);
                    }

                    self.AllGraspsLetGoOfThisObject(true);
                    player.StorePearl(self.abstractSpear, fromPearlSpear: true);
                }
                else if (freeHand != -1 && player.GraspsHasType(AbstractPhysicalObject.AbstractObjectType.Spear) == -1)
                {
                    if (self.room is not null && player.graphicsModule is not null)
                    {
                        self.room.AddObject(new ShockWave(prevPos, 50.0f, 0.8f, 10));
                        self.room.AddObject(new ExplosionSpikes(self.room, prevPos, 10, 10.0f, 10, 10.0f, 80.0f, color));

                        var handPos = ((PlayerGraphics)player.graphicsModule).hands[freeHand].pos;

                        self.room.AddObject(new ShockWave(handPos, 15.0f, 0.8f, 10));
                        self.room.AddObject(new ExplosionSpikes(self.room, handPos, 10, 5.0f, 10, 10.0f, 40.0f, color));

                        self.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, player.firstChunk.pos, 0.8f, 3.5f);
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

    
    // Stop player pearls being saved in the shelter (duplicating)
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

                    if (!abstractObject.IsPlayerPearl())
                    {
                        continue;
                    }

                    if (abstractObject.world.game.IsStorySession)
                    {
                        abstractObject.world.game.GetStorySession.RemovePersistentTracker(abstractObject);
                    }

                    abstractRoom.entities.Remove(entity);
                }
            }
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"Error removing player pearls from the world state: \n{e}");
        }

        orig(self, playerShelter, activeGate);
    }


    // Prevent player pearls from being displayed on the map when in a shelter
    private static HUD.Map.ShelterMarker.ItemInShelterMarker.ItemInShelterData? Map_GetItemInShelterFromWorld(On.HUD.Map.orig_GetItemInShelterFromWorld orig, World world, int room, int index)
    {
        var result = orig(world, room, index);

        var abstractRoom = world.GetAbstractRoom(room);

        if (index < abstractRoom.entities.Count && abstractRoom.entities[index] is AbstractPhysicalObject abstractObject)
        {
            if (abstractObject.IsPlayerPearl())
            {
                return null;
            }
        }

        return result;
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
    private static ChunkSoundEmitter Room_PlaySound_SoundID_BodyChunk(On.Room.orig_PlaySound_SoundID_BodyChunk orig, Room self, SoundID soundId, BodyChunk? chunk)
    {
        if (chunk?.owner is Spear spear && spear.abstractSpear.TryGetSpearModule(out var spearModule) && spearModule.DecayTimer == 0)
        {
            if (soundId == SoundID.Spear_Bounce_Off_Creauture_Shell)
            {
                self.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, chunk, false, 0.5f, Random.Range(1.2f, 1.5f));

                return self.PlaySound(soundId, chunk, false, 1f, 1.5f);
            }

            if (soundId == SoundID.Spear_Stick_In_Creature)
            {
                self.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, chunk, false, 0.5f, Random.Range(1.1f, 1.25f));

                return self.PlaySound(soundId, chunk, false, 1f, 1.5f);
            }

            if (soundId == SoundID.Spear_Stick_In_Ground)
            {
                return self.PlaySound(soundId, chunk, false, 1f, 1.5f);
            }

            if (soundId == SoundID.Spear_Stick_In_Wall)
            {
                self.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, chunk, false, 0.5f, Random.Range(1.2f, 1.5f));

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
