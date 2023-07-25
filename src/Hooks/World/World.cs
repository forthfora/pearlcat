using MonoMod.RuntimeDetour;
using RWCustom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        //On.GlobalRain.Update += GlobalRain_Update;

        On.KingTusks.Tusk.ShootUpdate += Tusk_ShootUpdate;
        On.KingTusks.Tusk.Update += Tusk_Update;

        On.Spear.DrawSprites += Spear_DrawSprites;
        On.Spear.Update += Spear_Update;

        On.DartMaggot.ShotUpdate += DartMaggot_ShotUpdate;
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
    }

    private static void WormGrassPatch_Update(On.WormGrass.WormGrassPatch.orig_Update orig, WormGrass.WormGrassPatch self)
    {
        orig(self);

        for (int i = self.trackedCreatures.Count - 1; i >= 0; i--)
        {
            var trackedCreature = self.trackedCreatures[i];
            if (trackedCreature.creature is not Player player) continue;

            if (!player.TryGetPearlcatModule(out var playerModule)) continue;

            if (playerModule.ShieldTimer > 0)
                self.trackedCreatures.Remove(trackedCreature);
        }
    }

    private static void Worm_Attached(On.WormGrass.Worm.orig_Attached orig, WormGrass.Worm self)
    {
        orig(self);

        if (self.attachedChunk?.owner is not Player player) return;

        if (!player.TryGetPearlcatModule(out var playerModule)) return;

        if (self.patch?.trackedCreatures == null) return;


        var playerPull = self.patch.trackedCreatures.FirstOrDefault(x => x.creature == self.attachedChunk.owner);

        if (playerPull == null) return;

        DeflectEffect(self.room, self.pos);
        playerModule.ActivateVisualShield();
        
        if (playerModule.ShieldTimer > 0)
            self.attachedChunk = null;
    }

    private static void AttachedBee_Update(On.SporePlant.AttachedBee.orig_Update orig, SporePlant.AttachedBee self, bool eu)
    {
        orig(self, eu);

        if (self.attachedChunk?.owner is not Player player) return;

        if (!player.TryGetPearlcatModule(out var playerModule)) return;

        if (playerModule.ShieldTimer > 0)
        {
            self.BreakStinger();
            self.stingerOut = false;
        }
    }

    public delegate int orig_StoryGameSessionSlugPupMaxCount(StoryGameSession self);
    public static int GetStoryGameSessionSlugPupMaxCount(orig_StoryGameSessionSlugPupMaxCount orig, StoryGameSession self)
    {
        var result = orig(self);

        if (self.saveStateNumber == Enums.Pearlcat)
            if (self.saveState.progression.miscProgressionData.beaten_Gourmand_Full || MoreSlugcats.MoreSlugcats.chtUnlockSlugpups.Value)
                return 2;

        return result;
    }


    private static void BigNeedleWorm_Swish(On.BigNeedleWorm.orig_Swish orig, BigNeedleWorm self)
    {
        orig(self);

        foreach (var crit in self.abstractCreature.Room.world.game.Players)
        {

            if (crit.realizedCreature is not Player player) continue;

            if (!player.TryGetPearlcatModule(out var playerModule)) continue;

            if (!playerModule.ShieldActive) continue;

            if (self.impaleChunk == null || self.impaleChunk.owner != player) continue;

            self.swishCounter = 0;
            self.firstChunk.vel = Vector2.zero;

            self.impaleChunk = null;

            self.Stun(40);

            self.room.DeflectEffect(self.firstChunk.pos);
            playerModule.ActivateVisualShield();
        }
    }

    private static void DartMaggot_ShotUpdate(On.DartMaggot.orig_ShotUpdate orig, DartMaggot self)
    {
        orig(self);

        foreach (var crit in self.room.world.game.Players)
        {
            if (crit.realizedCreature is not Player player) continue;

            if (!player.TryGetPearlcatModule(out var playerModule)) continue;

            if (!playerModule.ShieldActive) continue;

            if (!Custom.DistLess(player.firstChunk.pos, self.firstChunk.pos, 50.0f)) continue;

            self.mode = DartMaggot.Mode.Free;

            self.firstChunk.vel = Vector2.zero;
            
            self.room.DeflectEffect(self.firstChunk.pos);
            playerModule.ActivateVisualShield();
        }
    }

    
    public delegate bool orig_RegionGateMeetRequirement(RegionGate self);
    public static bool GetRegionGateMeetRequirement(orig_RegionGateMeetRequirement orig, RegionGate self)
    {
        var result = orig(self);

        var roomName = self.room.roomSettings.name;

        if (roomName != "GATE_UW_LC")
            return result;

        if (!self.room.game.IsPearlcatStory())
            return result;

        return true;
    }


    private static void GateKarmaGlyph_ctor(On.GateKarmaGlyph.orig_ctor orig, GateKarmaGlyph self, bool side, RegionGate gate, RegionGate.GateRequirement requirement)
    {
        orig(self, side, gate, requirement);

        if (gate.room == null) return;
        
        var roomName = gate.room.roomSettings.name;

        if (roomName != "GATE_UW_LC") return;

        if (!gate.room.game.IsPearlcatStory()) return;

        self.requirement = RegionGate.GateRequirement.OneKarma;
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

        if (self.saveStateNumber == Enums.Pearlcat && self.progression.miscProgressionData.GetMiscProgression().IsNewPearlcatSave)
            if (!string.IsNullOrEmpty(ModOptions.StartShelterOverride.Value) && RainWorld.roomNameToIndex.ContainsKey(ModOptions.StartShelterOverride.Value))
                return ModOptions.StartShelterOverride.Value;

        if (result == "T1_S01")
            return ModManager.MSC ? "LC_T1_S01" : "SS_S04";

        return result;
    }

    private static void Spear_Update(On.Spear.orig_Update orig, Spear self, bool eu)
    {
        orig(self, eu);

        if (!self.abstractSpear.TryGetModule(out var module)) return;

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

    public static bool TryGetModule(this AbstractSpear spear, out SpearModule module)
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

        if (!self.abstractSpear.TryGetModule(out var module)) return;

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

    private static void Tusk_Update(On.KingTusks.Tusk.orig_Update orig, KingTusks.Tusk self)
    {
        orig(self);

        if (self.impaleChunk != null && self.impaleChunk.owner is Player impaledPlayer)
            if (impaledPlayer.TryGetPearlcatModule(out var playerModule) && playerModule.ShieldTimer > 0)
                self.mode = KingTusks.Tusk.Mode.Dangling;
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

        if (room.roomSettings.name == "T1_S01")
            room.AddObject(new T1_S01(room));

        if (room.roomSettings.name == "LC_T1_S01")
            room.AddObject(new LC_T1_S01(room));


        if (!room.abstractRoom.firstTimeRealized) return;

        // Tutorial

        // Start
        if (room.game.GetStorySession.saveState.saveStateNumber == Enums.Pearlcat && room.game.GetStorySession.saveState.cycleNumber == 0 && room.roomSettings.name == "T1_START")
            room.AddObject(new T1_START(room));

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

    public static void AddTextPrompt(this RainWorldGame game, string text, int wait, int time, bool darken = false, bool? hideHud = null)
    {
        hideHud ??= ModManager.MMF;
        game.cameras.First().hud.textPrompt.AddMessage(game.manager.rainWorld.inGameTranslator.Translate(text), wait, time, darken, (bool)hideHud);
    } 


    public static void LockAndHideShortcuts(this Room room)
    {
        room.LockShortcuts();
        room.HideShortcuts();
    }
    public static void UnlockAndShowShortcuts(this Room room)
    {
        room.UnlockShortcuts();
        room.ShowShortcuts();

        room.game.cameras.First().hud.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom);
    }

    public static void LockShortcuts(this Room room)
    {
        foreach (var shortcut in room.shortcutsIndex)
            if (!room.lockedShortcuts.Contains(shortcut))
                room.lockedShortcuts.Add(shortcut);
    }
    public static void UnlockShortcuts(this Room room) => room.lockedShortcuts.Clear();

    public static void HideShortcuts(this Room room)
    {
        var rCam = room.game.cameras.First();

        if (rCam.room != room) return;

        var shortcutGraphics = rCam.shortcutGraphics;

        for (int i = 0; i < room.shortcuts.Length; i++)
            if (shortcutGraphics.entranceSprites[i, 0] != null)
                shortcutGraphics.entranceSprites[i, 0].isVisible = false;
    }
    public static void ShowShortcuts(this Room room)
    {
        var rCam = room.game.cameras.First();

        if (rCam.room != room) return;

        var shortcutGraphics = rCam.shortcutGraphics;

        for (int i = 0; i < room.shortcuts.Length; i++)
            if (shortcutGraphics.entranceSprites[i, 0] != null)
                shortcutGraphics.entranceSprites[i, 0].isVisible = true;
    }
}
