﻿using System;
using System.Linq;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Pearlcat;

public static class Player_Helpers
{
    public const int POSSESSION_DELAY = 80;
    public const int REMOVE_HEART_DELAY = 320;


    // Player Check
    public static bool IsPearlcat(this Player player)
    {
        return player.SlugCatClass == Enums.Pearlcat;
    }

    public static bool IsFirstPearlcat(this Player player)
    {
        return player.playerState.playerNumber == (player.room?.game).GetFirstPearlcatIndex();
    }


    // Misc
    public static int GraspsHasType(this Player self, AbstractPhysicalObject.AbstractObjectType type)
    {
        for (var i = 0; i < self.grasps.Length; i++)
        {
            var grasp = self.grasps[i];

            if (grasp == null)
            {
                continue;
            }

            if (grasp.grabbed.abstractPhysicalObject.type == type)
            {
                return i;
            }
        }

        return -1;
    }

    public static bool IsHostileToMe(this Creature self, Creature creature)
    {
        // trust no one, not even yourself?
        if (creature == self)
        {
            return false;
        }

        if (creature is Player pup && pup.IsPearlpup())
        {
            return false;
        }

        // Possessed Creature
        if (self is Player && creature.abstractCreature.controlled)
        {
            return false;
        }

        var AI = creature.abstractCreature.abstractAI?.RealAI;

        // Player vs Aggressive Creature
        if (self is Player && AI is LizardAI or ScavengerAI or BigNeedleWormAI or DropBugAI or CicadaAI or InspectorAI)
        {
            var aggression = AI.CurrentPlayerAggression(self.abstractCreature);

            var rep = AI.tracker.RepresentationForCreature(self.abstractCreature, false);

            if (rep?.dynamicRelationship == null)
            {
                return false;
            }

            return AI switch
            {
                LizardAI => aggression > 0.0f,
                ScavengerAI => aggression > 0.5f,
                BigNeedleWormAI => aggression > 0.0f,
                CicadaAI => aggression > 0.0f,
                DropBugAI => true,
                InspectorAI => aggression > 0.0f,
                _ => false
            };
        }

        // Player vs Player
        if (self is Player && creature is Player player2 && !player2.isSlugpup)
        {
            var game = self.abstractCreature.world.game;

            if (game.IsArenaSession && game.GetArenaGameSession.GameTypeSetup.spearsHitPlayers)
            {
                return true;
            }
        }

        var myRelationship =
            self.abstractCreature.creatureTemplate.CreatureRelationship(self.abstractCreature.creatureTemplate);
        var creatureRelationship =
            creature.abstractCreature.creatureTemplate.CreatureRelationship(self.abstractCreature.creatureTemplate);

        return myRelationship.GoForKill || creatureRelationship.GoForKill;
    }

    public static bool InDeathPit(this Player self)
    {
        var belowRoom = self.mainBodyChunk.pos.y < -300.0f;

        var roomHasWater = self.room.water && !self.room.waterInverted && self.room.defaultWaterLevel >= -10;

        var deadOrStunned = self.dead || self.Stunned;

        var canFly = self.Template.canFly;

        var roomHasDeathPit = self.room.deathFallGraphic is not null;

        return roomHasDeathPit && belowRoom && !roomHasWater && (deadOrStunned || !canFly);
    }


    // Revive
    public static void TryRevivePlayer(this Player self, PlayerModule playerModule)
    {
        if (playerModule.ReviveCount <= 0)
        {
            return;
        }

        if (self.room == null)
        {
            return;
        }

        self.AllGraspsLetGoOfThisObject(true);

        self.room.DeflectEffect(self.firstChunk.pos);
        playerModule.ShieldTimer = 200;

        if (self.dead)
        {
            self.RevivePlayer();
        }

        else
        {
            self.room.ReviveEffect(self.firstChunk.pos);
        }

        playerModule.SetReviveCooldown(-1);
    }

    public static void RevivePlayer(this Player self)
    {
        self.Revive();

        self.abstractCreature.Room.world.game.cameras.First().hud.textPrompt.gameOverMode = false;
        self.playerState.permaDead = false;
        self.playerState.alive = true;

        self.exhausted = false;
        self.airInLungs = 1.0f;
        self.aerobicLevel = 0.0f;

        self.bodyMode = Player.BodyModeIndex.Default;
        self.animation = Player.AnimationIndex.None;

        if (!self.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        playerModule.PickObjectAnimation(self);
    }

    public static void Revive(this Creature self)
    {
        if (self.State is HealthState healthState)
        {
            healthState.health = 1.0f;
        }

        self.State.alive = true;

        self.dead = false;
        self.killTag = null;
        self.killTagCounter = 0;
        self.abstractCreature.abstractAI?.SetDestination(self.abstractCreature.pos);

        if (self is not Player)
        {
            self.Stun(100);
        }

        self.room.ReviveEffect(self.mainBodyChunk.pos);
    }


    // Possession (Adult Pearlpup)
    public static void TryToRemoveHeart(Player self, PlayerModule playerModule, DataPearl.AbstractDataPearl dataPearl)
    {
        if (self.room == null)
        {
            return;
        }

        if (playerModule.PossessionTarget?.TryGetTarget(out var target) == true)
        {
            self.PossessCreature(playerModule, target);
        }
        else
        {
            var room = self.room;
            var pos = self.firstChunk.pos;

            self.firstChunk.vel.y += 20.0f;

            room.AddObject(new Explosion.ExplosionLight(pos, 100.0f, 1.0f, 3, Color.red));
            room.AddObject(new ShockWave(pos, 250.0f, 0.07f, 6));

            room.AddObject(new ExplosionSpikes(room, pos, 5, 100.0f, 20.0f, 25.0f, 100.0f, Color.red));
            room.AddObject(new LightningMachine.Impact(pos, 2.0f, Color.red, true));

            for (var i = 0; i < 4; i++)
            {
                var randVec = Custom.RNV() * 150.0f;
                room.ConnectEffect(pos, pos + randVec, Color.red, 1.5f, 80);
            }

            room.PlaySound(SoundID.Fire_Spear_Explode, pos, 1.2f, 0.8f);
            room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, pos, 1.0f, 1.0f);

            self.Die();
        }
    }

    public static void PossessCreature(this Player self, PlayerModule playerModule, Creature target)
    {
        playerModule.PossessionTarget = null;
        playerModule.PossessedCreature = new(target.abstractCreature);

        self.LoseAllGrasps();
        self.Stun(10);

        var room = self.room;
        var pos = self.firstChunk.pos;

        for (var i = 0; i < 5; i++)
        {
            room.AddObject(new NeuronSpark(pos));
        }

        room.AddObject(new Explosion.ExplosionLight(pos, 150f, 1f, 8, Color.white));
        room.AddObject(new ShockWave(pos, 60f, 0.1f, 8));

        room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, pos, 0.6f, 2.5f + Random.value * 0.5f);
    }

    public static void ReleasePossession(Player self, PlayerModule playerModule)
    {
        if (playerModule.PossessedCreature?.TryGetTarget(out var creature) == true)
        {
            creature.controlled = false;
        }
        else
        {
            return;
        }

        playerModule.PossessedCreature = null;
        playerModule.PossessionTarget = null;

        var room = self.room;
        var pos = self.firstChunk.pos;

        for (var i = 0; i < 5; i++)
        {
            room.AddObject(new Spark(pos, Custom.RNV(), Color.white, null, 16, 24));
        }

        room.AddObject(new Explosion.ExplosionSmoke(pos, Custom.RNV() * 2f * Random.value, 1f));

        room.AddObject(new Explosion.ExplosionLight(pos, 150f, 1f, 8, Color.white));
        room.AddObject(new ShockWave(pos, 60f, 0.3f, 16));

        room.PlaySound(SoundID.Bomb_Explode, pos, 0.5f, 1.2f);
        room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, pos, 0.6f, 0.3f + Random.value * 0.2f);

        self.InitiateGraphicsModule();
        self.graphicsModule?.Reset();
    }


    // Update
    public static void UpdateAll(Player self, PlayerModule playerModule)
    {
        // Warp Fix
        if (self.room != null && playerModule.JustWarped)
        {
            self.GivePearls(playerModule);
            playerModule.LoadSaveData(self);

            Plugin.Logger.LogInfo("PEARLCAT WARP END");
            playerModule.JustWarped = false;
        }

        self.TryRealizeInventory(playerModule);

        UpdatePlayerPearlAnimation(self, playerModule);
        UpdatePlayerDaze(self, playerModule);
        UpdatePostDeathInventory(self, playerModule);

        PlayerAbilities_Helpers.UpdateCombinedPOEffect(self, playerModule);
        PlayerAbilities_Helpers.ApplyCombinedPOEffect(self, playerModule);

        UpdateHUD(self, playerModule);
        UpdateSFX(self, playerModule);

        UpdateStoreRetrieveObject(self, playerModule);

        UpdateTryRevive(self, playerModule);

        if (self.room != null)
        {
            self.GivePearls(playerModule);
        }

        RefreshPearlpup(self, playerModule);

        UpdateAdultPearlpup(self, playerModule);
    }

    public static void UpdateStoreRetrieveObject(Player self, PlayerModule playerModule)
    {
        if (self.inVoidSea)
        {
            return;
        }

        var storeObjectDelay = 30.0f;

        var storeInput = self.IsStoreKeybindPressed(playerModule);
        var toStore = self.grasps[0]?.grabbed;
        var isStoring = self.grasps[0]?.grabbed.abstractPhysicalObject.IsObjectStorable() ?? false;

        if (isStoring && toStore == null)
        {
            return;
        }

        if (!isStoring && self.FreeHand() == -1)
        {
            return;
        }

        if (!isStoring && playerModule.ActiveObject == null)
        {
            return;
        }


        // Longer delay removing heart
        if (playerModule.ActiveObject.IsHeartPearl() && !isStoring)
        {
            storeObjectDelay = playerModule.PossessionTarget == null ? REMOVE_HEART_DELAY : POSSESSION_DELAY;
        }


        if (playerModule.StoreObjectTimer > storeObjectDelay)
        {
            if (isStoring && toStore != null)
            {
                self.StoreObject(toStore.abstractPhysicalObject, true);
            }
            else if (playerModule.ActiveObject != null)
            {
                if (playerModule.ActiveObject.IsHeartPearl())
                {
                    TryToRemoveHeart(self, playerModule, (DataPearl.AbstractDataPearl)playerModule.ActiveObject);
                }
                else
                {
                    self.room.PlaySound(Enums.Sounds.Pearlcat_PearlRetrieve, playerModule.ActiveObject.realizedObject.firstChunk);
                    self.RetrieveActiveObject();
                }
            }

            playerModule.StoreObjectTimer = -1;
        }


        if (storeInput)
        {
            if (playerModule.StoreObjectTimer >= 0)
            {
                if (isStoring || (playerModule.ActiveObject != null && playerModule.ActiveObject.TryGetPlayerPearlModule(out var module) && !module.IsReturningSentry))
                {
                    playerModule.StoreObjectTimer++;

                    // every 5 frames
                    if (playerModule.StoreObjectTimer % 5 == 0)
                    {
                        if (isStoring)
                        {
                            var activeObjPos = self.GetActivePearlPos();
                            toStore?.ConnectEffect(activeObjPos);
                        }
                        else
                        {
                            var activeObj = playerModule.ActiveObject?.realizedObject;

                            if (playerModule.ActiveObject?.TryGetPlayerPearlModule(out module) == true)
                            {
                                if (!module.IsReturningSentry)
                                {
                                    activeObj.ConnectEffect(self.firstChunk.pos);
                                }

                                module.RemoveSentry(playerModule.ActiveObject);
                            }
                            else
                            {
                                activeObj.ConnectEffect(self.firstChunk.pos);
                            }
                        }
                    }


                    var heartRemovalStart = 40;

                    // trying to remove heart
                    if (playerModule.ActiveObject is DataPearl.AbstractDataPearl abstractHeart && abstractHeart.IsHeartPearl() && !isStoring)
                    {

                        // Removing the heart without any possessable creatures nearby
                        if (playerModule.PossessionTarget == null && playerModule.StoreObjectTimer > heartRemovalStart)
                        {
                            var heart = (DataPearl)abstractHeart.realizedObject;
                            var bigSparkFreq = (int)Custom.LerpMap(playerModule.StoreObjectTimer, heartRemovalStart, storeObjectDelay, 35, 1);
                            var heartBeatFreq = (int)Custom.LerpMap(playerModule.StoreObjectTimer, heartRemovalStart, storeObjectDelay, 35, 4);

                            if (playerModule.StoreObjectTimer % bigSparkFreq == 0)
                            {
                                var randVec = Custom.RNV() * Random.Range(150.0f, 250.0f);
                                self.room.ConnectEffect(heart.firstChunk.pos, heart.firstChunk.pos + randVec, Color.red, 8.0f, 40);
                                self.room.PlaySound(SoundID.Zapper_Zap, heart.firstChunk.pos, 0.8f, Random.Range(0.6f, 1.4f));
                            }

                            if (playerModule.StoreObjectTimer % heartBeatFreq == 0)
                            {
                                self.room.PlaySound(Enums.Sounds.Pearlcat_Heartbeat , heart.firstChunk.pos, Custom.LerpMap(playerModule.StoreObjectTimer, heartRemovalStart, storeObjectDelay, 0.45f, 1.0f), 1.0f);
                            }

                            if (playerModule.StoreObjectTimer % 10 == 0)
                            {
                                self.room.AddObject(new LightningMachine.Impact(heart.firstChunk.pos, 0.4f, Color.red));
                            }

                            if (playerModule.StoreObjectTimer % 30 == 0)
                            {
                                self.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, heart.firstChunk.pos, 0.8f, 5.0f);
                                self.room.AddObject(new ExplosionSpikes(self.room, heart.firstChunk.pos, 5, 100.0f, 20.0f, 25.0f, 100.0f, Color.red));
                            }
                        }
                    }
                }

                playerModule.BlockInput = true;
                playerModule.ShowHUD(10);

                self.Blink(5);
            }
        }
        else
        {
            playerModule.StoreObjectTimer = 0;
        }
    }

    public static void UpdatePostDeathInventory(Player self, PlayerModule playerModule)
    {
        if (self.dead || playerModule.PostDeathInventory.Count == 0)
        {
            return;
        }

        for (var i = playerModule.PostDeathInventory.Count - 1; i >= 0; i--)
        {
            var item = playerModule.PostDeathInventory[i];
            playerModule.PostDeathInventory.RemoveAt(i);

            if (item.realizedObject == null)
            {
                continue;
            }

            if (item.realizedObject.room != self.room)
            {
                continue;
            }

            if (item.realizedObject.grabbedBy.Count > 0)
            {
                continue;
            }

            if (item.IsPlayerPearl())
            {
                return;
            }

            if (ModuleManager.PlayerPearlGraphicsData.TryGetValue(item, out var _))
            {
                ModuleManager.PlayerPearlGraphicsData.Remove(item);
            }

            self.StoreObject(item);
        }

        if (playerModule.PostDeathActiveObjectIndex != null)
        {
            self.ActivateObjectInStorage((int)playerModule.PostDeathActiveObjectIndex);
        }

        playerModule.PostDeathActiveObjectIndex = null;
    }

    public static void UpdatePlayerPearlAnimation(Player self, PlayerModule playerModule)
    {
        if (self.bodyMode == Player.BodyModeIndex.Stunned || self.bodyMode == Player.BodyModeIndex.Dead)
        {
            playerModule.CurrentObjectAnimation = new PearlAnimation_FreeFall(self);
        }
        else if (self.Sleeping || self.sleepCurlUp > 0.0f)
        {
            playerModule.CurrentObjectAnimation = new PearlAnimation_Sleeping(self);
        }
        else if (playerModule.CurrentObjectAnimation is PearlAnimation_Sleeping or PearlAnimation_FreeFall)
        {
            for (var i = 0; i < playerModule.Inventory.Count; i++)
            {
                var abstractObject = playerModule.Inventory[i];

                // just handle this before it gets out of hand
                if (i >= PlayerPearl_Helpers.MaxPearlsWithEffects)
                {
                    break;
                }

                if (abstractObject.TryGetSentry(out _))
                {
                    continue;
                }

                if (ModOptions.HidePearls.Value)
                {
                    if (playerModule.ActiveObject != abstractObject)
                    {
                        continue;
                    }
                }

                abstractObject.realizedObject.ConnectEffect(((PlayerGraphics)self.graphicsModule).head.pos);
            }

            playerModule.PickObjectAnimation(self);
        }
        else
        {
            if (playerModule.CurrentObjectAnimation is PearlAnimation_SineWaveWeave or PearlAnimation_SineWave && self.firstChunk.vel.magnitude > 4.0f)
            {
                playerModule.PickObjectAnimation(self);
            }
        }

        if (playerModule.ObjectAnimationTimer > playerModule.ObjectAnimationDuration)
        {
            playerModule.PickObjectAnimation(self);
        }

        playerModule.CurrentObjectAnimation?.Update(self);
        playerModule.ObjectAnimationTimer++;
    }

    public static void UpdateSFX(Player self, PlayerModule playerModule)
    {
        // Outsider breaks looping SFX sometimes, this is safety
        try
        {
            playerModule.MenuCrackleLoop.Update();
            playerModule.MenuCrackleLoop.Volume = playerModule.HudFade;

            playerModule.ShieldHoldLoop.Update();
            playerModule.ShieldHoldLoop.Volume = playerModule.ShieldTimer > 0 ? 1.0f : 0.0f;
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("Looping SFX Error, Outsider often likes to cause this:\n" + e);
        }
    }

    public static void UpdateHUD(Player self, PlayerModule playerModule)
    {
        if (playerModule.HudFadeTimer > 0)
        {
            playerModule.HudFadeTimer--;
            playerModule.HudFade = Mathf.Lerp(playerModule.HudFade, 1.0f, 0.1f);
        }
        else
        {
            playerModule.HudFadeTimer = 0;
            playerModule.HudFade = Mathf.Lerp(playerModule.HudFade, 0.0f, 0.05f);
        }
    }

    public static void UpdatePlayerDaze(Player self, PlayerModule playerModule)
    {
        var dazeDuration = 40;

        if (self.dead || self.bodyMode == Player.BodyModeIndex.Stunned || self.Sleeping)
        {
            playerModule.DazeTimer = dazeDuration;
        }

        if (playerModule.DazeTimer > 0)
        {
            playerModule.DazeTimer--;
        }
    }

    public static void UpdateTryRevive(Player self, PlayerModule playerModule)
    {
        var shouldTryRevive = self.dead || (self.dangerGraspTime >= 60 && self.AI == null);

        if (!shouldTryRevive)
        {
            return;
        }

        self.TryRevivePlayer(playerModule);
    }

    public static void UpdateAdultPearlpup(Player self, PlayerModule playerModule)
    {
        if (!playerModule.IsAdultPearlpup)
        {
            return;
        }

        var hasHeart =
            playerModule.Inventory.Any(x => x is DataPearl.AbstractDataPearl dataPearl && dataPearl.IsHeartPearl()) ||
            playerModule.PostDeathInventory.Any(x =>
                x is DataPearl.AbstractDataPearl dataPearl && dataPearl.IsHeartPearl());
        var room = self.room;

        if (room != null && !self.dead && !hasHeart)
        {
            var pearl = new DataPearl.AbstractDataPearl(self.room.world,
                AbstractPhysicalObject.AbstractObjectType.DataPearl, null, self.abstractPhysicalObject.pos,
                room.game.GetNewID(), -1, -1, null, Enums.Pearls.Heart_Pearlpup);
            self.StoreObject(pearl, overrideLimit: true);
        }


        if (playerModule.IsPossessingCreature)
        {
            playerModule.PossessionTarget = null;

            AbstractCreature? possessedCreature = null;
            var shouldReleasePossession = playerModule.PossessedCreature == null ||
                                          !playerModule.PossessedCreature.TryGetTarget(out possessedCreature);

            if (possessedCreature != null)
            {
                if (possessedCreature.realizedCreature == null || possessedCreature.realizedCreature.dead)
                {
                    shouldReleasePossession = true;
                }
            }

            if (shouldReleasePossession || possessedCreature == null)
            {
                ReleasePossession(self, playerModule);
                return;
            }


            playerModule.BlockInput = true;
            possessedCreature.controlled = true;

            if (possessedCreature.realizedCreature?.room == null)
            {
                self.SuckedIntoShortCut(possessedCreature.pos.Tile, false);
            }
            else
            {
                if (room == null)
                {
                    self.abstractCreature.ChangeRooms(possessedCreature.pos);
                    self.abstractCreature.RealizeInRoom();
                }

                self.ChangeCollisionLayer(0);
                self.SuperHardSetPosition(possessedCreature.realizedCreature.firstChunk.pos);

                foreach (var chunk in self.bodyChunks)
                {
                    chunk.vel = Vector2.zero;
                }
            }
        }
        else
        {
            if (room == null)
            {
                return;
            }

            if (!playerModule.ActiveObject.IsHeartPearl())
            {
                playerModule.PossessionTarget = null;
                return;
            }

            const float possessionMaxDist = 400.0f;
            const float possessionLostDist = 400.0f;

            // search for target
            if (playerModule.PossessionTarget == null || !playerModule.PossessionTarget.TryGetTarget(out var target))
            {
                Creature? bestTarget = null;
                var shortestDist = float.MaxValue;

                foreach (var roomObject in room.physicalObjects)
                {
                    foreach (var physicalObject in roomObject)
                    {
                        if (physicalObject is Player)
                        {
                            continue;
                        }

                        if (physicalObject is not Creature creature)
                        {
                            continue;
                        }

                        if (creature.dead)
                        {
                            continue;
                        }

                        if (creature.abstractCreature.controlled)
                        {
                            continue;
                        }


                        var dist = Custom.Dist(creature.mainBodyChunk.pos, self.firstChunk.pos);

                        if (dist > possessionMaxDist)
                        {
                            continue;
                        }

                        if (dist > shortestDist)
                        {
                            continue;
                        }

                        if (!self.room.VisualContact(self.mainBodyChunk.pos, creature.mainBodyChunk.pos))
                        {
                            continue;
                        }

                        shortestDist = dist;
                        bestTarget = creature;
                    }
                }

                if (bestTarget != null)
                {
                    playerModule.PossessionTarget = new(bestTarget);
                }
            }
            else
            {
                // ensure target is still valid
                var invalidTarget =
                    !Custom.DistLess(target.mainBodyChunk.pos, self.mainBodyChunk.pos, possessionLostDist);

                if (target.room != self.room)
                {
                    invalidTarget = true;
                }

                if (target.dead)
                {
                    invalidTarget = true;
                }

                if (!self.room.VisualContact(self.mainBodyChunk.pos, target.mainBodyChunk.pos))
                {
                    invalidTarget = true;
                }

                if (invalidTarget)
                {
                    playerModule.PossessionTarget = null;
                    playerModule.StoreObjectTimer = 0;
                }
            }
        }
    }


    public static void RefreshPearlpup(Player self, PlayerModule playerModule)
    {
        if (!self.IsFirstPearlcat())
        {
            return;
        }

        var save = self.abstractCreature.Room.world.game.GetMiscWorld();
        var miscProg = Utils.GetMiscProgression();

        miscProg.HasPearlpup = false;

        if (save != null)
        {
            save.HasPearlpupWithPlayer = false;
            save.HasPearlpupWithPlayerDeadOrAlive = false;
        }


        // Can get a reference to pearlpup (i.e. they're in the world somewhere)
        if (playerModule.PearlpupRef != null && playerModule.PearlpupRef.TryGetTarget(out var pup))
        {
            miscProg.HasPearlpup = !pup.dead && pup.abstractCreature.Room == self.abstractCreature.Room;

            if (save is null)
            {
                return;
            }

            save.HasPearlpupWithPlayer = miscProg.HasPearlpup;

            save.HasPearlpupWithPlayerDeadOrAlive = pup.abstractCreature.Room == self.abstractCreature.Room;

            return;
        }


        if (self.room == null)
        {
            return;
        }

        foreach (var roomObject in self.room.physicalObjects)
        {
            foreach (var physicalobject in roomObject)
            {
                if (physicalobject is not Player player)
                {
                    continue;
                }

                if (player.IsPearlpup())
                {
                    playerModule.PearlpupRef = new(player);
                    return;
                }
            }
        }

        playerModule.PearlpupRef = null;
    }
}