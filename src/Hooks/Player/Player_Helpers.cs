using System;
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
    public static bool IsPearlcat(this Player? player)
    {
        return player?.SlugCatClass == Enums.Pearlcat;
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

            if (grasp is null)
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

            if (rep?.dynamicRelationship is null)
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
                _ => false,
            };
        }

        // Player vs Player
        if (self is Player && creature is Player otherPlayer && !otherPlayer.isSlugpup)
        {
            var game = self.abstractCreature.world.game;

            if (game.IsArenaSession && game.GetArenaGameSession.GameTypeSetup.spearsHitPlayers)
            {
                return true;
            }

            if (ModCompat_Helpers.RainMeadow_IsOnline)
            {
                if (ModCompat_Helpers.RainMeadow_FriendlyFire)
                {
                    return true;
                }
            }
            else
            {
                if (ModManager.CoopAvailable && Utils.RainWorld.options.friendlyFire)
                {
                    return true;
                }
            }
        }

        var myRelationship = self.abstractCreature.creatureTemplate.CreatureRelationship(self.abstractCreature.creatureTemplate);
        var creatureRelationship = creature.abstractCreature.creatureTemplate.CreatureRelationship(self.abstractCreature.creatureTemplate);

        return myRelationship.GoForKill || creatureRelationship.GoForKill;
    }

    public static bool InDeathPit(this Player self)
    {
        var belowRoom = self.mainBodyChunk.pos.y < -300.0f;

        var roomHasWater = self.room is not null && self.room.water && !self.room.waterInverted && self.room.defaultWaterLevel >= -10;

        var deadOrStunned = self.dead || self.Stunned;

        var canFly = self.Template.canFly;

        var roomHasDeathPit = self.room?.deathFallGraphic is not null;

        return roomHasDeathPit && belowRoom && !roomHasWater && (deadOrStunned || !canFly);
    }


    // Revive
    public static void TryRevivePlayer(this Player self, PlayerModule playerModule)
    {
        if (playerModule.ReviveCount <= 0)
        {
            return;
        }

        if (self.room is null)
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
        RevivePlayer_Local(self);

        if (ModCompat_Helpers.RainMeadow_IsOnline)
        {
            MeadowCompat.RPC_RevivePlayer(self);
        }
    }

    public static void RevivePlayer_Local(Player self)
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

        playerModule.PickPearlAnimation(self);
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
    public static void TryToRemoveHeart(Player self, PlayerModule playerModule)
    {
        if (self.room is null)
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

        self.CollideWithObjects = false;
        self.CollideWithSlopes = false;
        self.CollideWithTerrain = false;

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

        self.CollideWithObjects = true;
        self.CollideWithSlopes = true;
        self.CollideWithTerrain = true;

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
    public static void UpdatePearlcat(Player self, PlayerModule playerModule)
    {
        playerModule.BaseStats = self.Malnourished ? playerModule.MalnourishedStats : playerModule.NormalStats;

        if (self.room is not null)
        {
            self.GivePearls(playerModule);
        }

        // If a pearl is in the wrong room abstract to reset it to the player's room
        foreach (var pearl in playerModule.Inventory)
        {
            if (pearl.Room != self.abstractCreature.Room)
            {
                PlayerPearl_Helpers.AbstractPlayerPearl(pearl);
            }
        }

        // Inventory
        if (self.room is null || self.inVoidSea)
        {
            self.TryAbstractInventory();
        }
        else
        {
            self.TryRealizeInventory(playerModule);
        }


        // Input
        var unblockedInput = playerModule.UnblockedInput;
        var allowInput = self.Consious && !self.inVoidSea && !self.Sleeping && (self.controller is null || !ModCompat_Helpers.RainMeadow_IsMine(self.abstractPhysicalObject)); // meadow remote control

        var swapLeftInput = self.IsSwapLeftInput() && allowInput;
        var swapRightInput = self.IsSwapRightInput() && allowInput;

        var swapInput = self.IsSwapKeybindPressed() && allowInput;
        var storeInput = self.IsStoreKeybindPressed(playerModule) && allowInput;

        var agilityInput = self.IsAgilityKeybindPressed(playerModule) && allowInput;
        var sentryInput = self.IsSentryKeybindPressed(playerModule) && allowInput;


        playerModule.BlockInput = false;

        if (swapLeftInput && !playerModule.WasSwapLeftInput)
        {
            self.SelectPreviousPearl();
        }
        else if (swapRightInput && !playerModule.WasSwapRightInput)
        {
            self.SelectNextPearl();
        }
        else if (swapInput)
        {
            playerModule.BlockInput = true;
            playerModule.ShowHUD(10);

            if (!playerModule.WasSwapped)
            {
                if (unblockedInput.x < -0.5f)
                {
                    self.SelectPreviousPearl();
                    playerModule.WasSwapped = true;
                }
                else if (unblockedInput.x > 0.5f)
                {
                    self.SelectNextPearl();
                    playerModule.WasSwapped = true;
                }
            }
            else if (Mathf.Abs(unblockedInput.x) < 0.5f)
            {
                playerModule.WasSwapped = false;
            }
        }


        // Main Methods
        UpdatePlayerDaze(self, playerModule);

        UpdatePostDeathInventory(self, playerModule);
        UpdateStoreAndRetrieve(self, playerModule);

        UpdatePlayerPearlAnimation(self, playerModule);
        PlayerAbilities_Helpers.UpdatePearlEffects(self, playerModule);

        UpdatePearlpup(self, playerModule);
        UpdateAdultPearlpup(self, playerModule);

        UpdateRevive(self, playerModule);
        UpdateDeathpitRevive(self, playerModule);

        UpdateHUD(self, playerModule);
        UpdateSFX(self, playerModule);


        // Post Update
        playerModule.WasSwapLeftInput = swapLeftInput;
        playerModule.WasSwapRightInput = swapRightInput;
        playerModule.WasStoreInput = storeInput;
        playerModule.WasAgilityInput = agilityInput;
        playerModule.WasSentryInput = sentryInput;

        playerModule.LastRoom = self.abstractCreature.Room;
    }

    private static void UpdateDeathpitRevive(Player self, PlayerModule playerModule)
    {
        // Tries to store the last solid ground the player was stood on (for deathpit revive)
        if (self.canJump >= 5)
        {
            if (playerModule.GroundedTimer > 15)
            {
                playerModule.LastGroundedPos = self.firstChunk.pos;
            }
            else
            {
                playerModule.GroundedTimer++;
            }
        }
        else
        {
            playerModule.GroundedTimer = 0;
        }

        // Revive from deathpit
        if (playerModule.ReviveCount > 0 && self.InDeathPit())
        {
            self.Die();
            self.SuperHardSetPosition(playerModule.LastGroundedPos);

            self.graphicsModule?.Reset();
            playerModule.FlyTimer = 60;

            var slugOnBack = self.slugOnBack?.slugcat;

            if (slugOnBack is not null)
            {
                slugOnBack.SuperHardSetPosition(playerModule.LastGroundedPos);
                slugOnBack.graphicsModule?.Reset();
            }
        }

        // For making the player float for a few seconds after being revived from a deathpit
        if (playerModule.FlyTimer > 0)
        {
            playerModule.FlyTimer--;

            self.firstChunk.vel.x = self.input[0].x * 5.0f;
            self.firstChunk.vel.y = 6.0f;
        }

        // Revive pearlpup if they fall into a death pit (consumes a revive)
        if (playerModule.PearlpupRef?.room is not null && playerModule.PearlpupRef.InDeathPit() && playerModule.ReviveCount > 0)
        {
            var pup = playerModule.PearlpupRef;

            pup.SuperHardSetPosition(self.firstChunk.pos);

            pup.Die();
            pup.RevivePlayer();

            pup.graphicsModule.Reset();
            pup.Stun(40);

            playerModule.SetReviveCooldown(-1);
        }
    }

    public static void UpdateStoreAndRetrieve(Player self, PlayerModule playerModule)
    {
        if (self.inVoidSea)
        {
            return;
        }

        var storeObjectDelay = 30.0f;

        var storeInput = self.IsStoreKeybindPressed(playerModule);
        var toStore = self.grasps[0]?.grabbed;
        var isStoring = self.grasps[0]?.grabbed.abstractPhysicalObject.IsObjectStorable() ?? false;

        if (isStoring && toStore is null)
        {
            return;
        }

        if (!isStoring && self.FreeHand() == -1)
        {
            return;
        }

        if (!isStoring && playerModule.ActivePearl is null)
        {
            return;
        }


        // Longer delay removing heart
        if (playerModule.ActivePearl.IsHeartPearl() && !isStoring)
        {
            storeObjectDelay = playerModule.PossessionTarget is null ? REMOVE_HEART_DELAY : POSSESSION_DELAY;
        }


        if (playerModule.StoreObjectTimer > storeObjectDelay)
        {
            if (isStoring && toStore is not null)
            {
                self.StorePearl(toStore.abstractPhysicalObject, true);
            }
            else if (playerModule.ActivePearl is not null)
            {
                if (playerModule.ActivePearl.IsHeartPearl())
                {
                    TryToRemoveHeart(self, playerModule);
                }
                else
                {
                    self.room.PlaySound(Enums.Sounds.Pearlcat_PearlRetrieve, playerModule.ActivePearl.realizedObject.firstChunk);
                    self.RetrieveActivePearl();
                }
            }

            playerModule.StoreObjectTimer = -1;
        }


        if (storeInput && !playerModule.IsPossessingCreature)
        {
            if (playerModule.StoreObjectTimer >= 0)
            {
                if (isStoring || (playerModule.ActivePearl is not null && playerModule.ActivePearl.TryGetPlayerPearlModule(out var module) && !module.IsReturningSentry))
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
                            var activeObj = playerModule.ActivePearl?.realizedObject;

                            if (playerModule.ActivePearl?.TryGetPlayerPearlModule(out module) == true)
                            {
                                if (!module.IsReturningSentry)
                                {
                                    activeObj.ConnectEffect(self.firstChunk.pos);
                                }

                                module.ReturnSentry(playerModule.ActivePearl);
                            }
                            else
                            {
                                activeObj.ConnectEffect(self.firstChunk.pos);
                            }
                        }
                    }


                    var heartRemovalStart = 40;

                    // trying to remove heart
                    if (playerModule.ActivePearl is DataPearl.AbstractDataPearl abstractHeart && abstractHeart.IsHeartPearl() && !isStoring)
                    {

                        // Removing the heart without any possessable creatures nearby
                        if (playerModule.PossessionTarget is null && playerModule.StoreObjectTimer > heartRemovalStart)
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
        if (!ModCompat_Helpers.RainMeadow_IsMine(self.abstractPhysicalObject))
        {
            return;
        }

        if (playerModule.PostDeathInventory.Count == 0)
        {
            return;
        }

        if (self.dead)
        {
            return;
        }

        foreach (var item in playerModule.PostDeathInventory)
        {
            if (item.realizedObject is null)
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
                continue;
            }

            if (ModuleManager.PlayerPearlGraphicsData.TryGetValue(item, out _))
            {
                ModuleManager.PlayerPearlGraphicsData.Remove(item);
            }

            self.StorePearl(item);
        }

        if (playerModule.PostDeathActivePearlIndex is not null)
        {
            self.SetActivePearl((int)playerModule.PostDeathActivePearlIndex);
        }

        playerModule.PostDeathInventory.Clear();
        playerModule.PostDeathActivePearlIndex = null;
    }

    public static void UpdatePlayerPearlAnimation(Player self, PlayerModule playerModule)
    {
        if (self.graphicsModule is null)
        {
            return;
        }

        if (self.bodyMode == Player.BodyModeIndex.Stunned || self.bodyMode == Player.BodyModeIndex.Dead)
        {
            playerModule.CurrentPearlAnimation = new PearlAnimation_FreeFall(self);
        }
        else if (self.Sleeping || self.sleepCurlUp > 0.0f)
        {
            playerModule.CurrentPearlAnimation = new PearlAnimation_Sleeping(self);
        }
        else if (playerModule.CurrentPearlAnimation is PearlAnimation_Sleeping or PearlAnimation_FreeFall)
        {
            for (var i = 0; i < playerModule.Inventory.Count; i++)
            {
                var abstractObject = playerModule.Inventory[i];

                // just handle this before it gets out of hand
                if (i >= PlayerPearl_Helpers_Graphics.MaxPearlsWithEffects)
                {
                    break;
                }

                if (abstractObject.TryGetSentry(out _))
                {
                    continue;
                }

                if (ModOptions.HidePearls)
                {
                    if (playerModule.ActivePearl != abstractObject && !abstractObject.IsHeartPearl())
                    {
                        continue;
                    }
                }

                abstractObject.realizedObject.ConnectEffect(((PlayerGraphics)self.graphicsModule).head.pos);
            }

            playerModule.PickPearlAnimation(self);
        }
        else
        {
            if (playerModule.CurrentPearlAnimation is PearlAnimation_SineWaveWeave or PearlAnimation_SineWave && self.firstChunk.vel.magnitude > 4.0f)
            {
                playerModule.PickPearlAnimation(self);
            }
        }

        if (playerModule.PearlAnimationTimer > playerModule.PearlAnimationDuration)
        {
            playerModule.PickPearlAnimation(self);
        }

        playerModule.CurrentPearlAnimation?.Update(self);
        playerModule.PearlAnimationTimer++;
    }

    public static void UpdateSFX(Player self, PlayerModule playerModule)
    {
        if (self.room is null)
        {
            return;
        }

        // Outsider breaks looping SFX sometimes, this is safety
        try
        {
            if (playerModule.MenuCrackleLoop is not null)
            {
                playerModule.MenuCrackleLoop.Update();
                playerModule.MenuCrackleLoop.Volume = playerModule.HudFade;
            }

            if (playerModule.ShieldHoldLoop is not null)
            {
                playerModule.ShieldHoldLoop.Update();
                playerModule.ShieldHoldLoop.Volume = playerModule.ShieldTimer > 0 ? 1.0f : 0.0f;
            }
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("Handled exception updating player SFX:\n" + e + "\n" + e.StackTrace);
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

    public static void UpdateRevive(Player self, PlayerModule playerModule)
    {
        var shouldTryRevive = self.dead || (self.dangerGraspTime >= 60 && self.AI is null);

        if (!shouldTryRevive)
        {
            return;
        }

        self.TryRevivePlayer(playerModule);
    }


    // Update Adult Pearlpup
    public static void UpdateAdultPearlpup(Player self, PlayerModule playerModule)
    {
        if (!playerModule.IsAdultPearlpup)
        {
            return;
        }


        var hasHeart =
            playerModule.Inventory.Any(x => x is DataPearl.AbstractDataPearl dataPearl && dataPearl.IsHeartPearl()) ||
            playerModule.PostDeathInventory.Any(x => x is DataPearl.AbstractDataPearl dataPearl && dataPearl.IsHeartPearl());

        // Give heart if it is missing
        if (self.room is not null && !self.dead && !hasHeart)
        {
            var pearl = new DataPearl.AbstractDataPearl(self.room.world,
                AbstractPhysicalObject.AbstractObjectType.DataPearl, null, self.abstractPhysicalObject.pos,
                self.room.game.GetNewID(), -1, -1, null, Enums.Pearls.Heart_Pearlpup);

            self.StorePearl(pearl, overrideLimit: true);
        }

        if (playerModule.IsPossessingCreature)
        {
            UpdateAdultPearlpup_Possessing(self, playerModule);
        }
        else
        {
            UpdateAdultPearlpup_NotPossessing(self, playerModule);
        }
    }

    private static void UpdateAdultPearlpup_Possessing(Player self, PlayerModule playerModule)
    {
        playerModule.PossessionTarget = null;

        AbstractCreature? possessedCreature = null;
        var releasePossession = playerModule.PossessedCreature is null || !playerModule.PossessedCreature.TryGetTarget(out possessedCreature);

        if (possessedCreature is not null)
        {
            if (possessedCreature.realizedCreature is null)
            {
                releasePossession = true;
            }
            else if (possessedCreature.realizedCreature.dead)
            {
                releasePossession = true;
            }
            else
            {
                // Mainly for flying creatures, release possession if outside bounds of the room
                var widthBuffer = 10;
                var bottomBuffer = 10;
                var topBuffer = 50;

                var insideRoom = Custom.InsideRect(self.abstractCreature.pos.Tile, new IntRect(-widthBuffer, -bottomBuffer, self.room.TileWidth + widthBuffer, self.room.TileHeight + topBuffer));

                if (!insideRoom)
                {
                    releasePossession = true;
                }
            }
        }

        if (releasePossession || possessedCreature is null)
        {
            ReleasePossession(self, playerModule);
            return;
        }


        playerModule.BlockInput = true;
        possessedCreature.controlled = true;

        if (possessedCreature.realizedCreature?.enteringShortCut is not null)
        {
            var enteringShortcut = possessedCreature.realizedCreature.enteringShortCut.Value;

            var shortcutRad = possessedCreature.realizedCreature.room.MiddleOfTile(enteringShortcut) + Custom.IntVector2ToVector2(possessedCreature.realizedCreature.room.ShorcutEntranceHoleDirection(enteringShortcut)) * -5f;
            var allConnectedObjects = possessedCreature.GetAllConnectedObjects();

            var chunksNotInShortcut = 0;

            foreach (var connectedObj in allConnectedObjects)
            {
                if (connectedObj.realizedObject is null)
                {
                    continue;
                }

                foreach (var bodyChunk in connectedObj.realizedObject.bodyChunks)
                {
                    if (Custom.DistLess(bodyChunk.pos, shortcutRad, Mathf.Max(10f, 0.7f)))
                    {
                        continue;
                    }

                    if (connectedObj == possessedCreature)
                    {
                        chunksNotInShortcut++;
                    }
                }
            }

            if (chunksNotInShortcut == 0)
            {
                self.SuckedIntoShortCut(possessedCreature.realizedCreature.enteringShortCut.Value, false);
            }
        }
        else if (possessedCreature.realizedCreature?.room is null)
        {
            self.SuckedIntoShortCut(possessedCreature.pos.Tile, false);
        }
        else
        {
            self.SuperHardSetPosition(possessedCreature.realizedCreature.firstChunk.pos);

            foreach (var chunk in self.bodyChunks)
            {
                chunk.vel = Vector2.zero;
            }
        }
    }

    private static void UpdateAdultPearlpup_NotPossessing(Player self, PlayerModule playerModule)
    {
        if (self.room is null)
        {
            return;
        }

        if (!playerModule.ActivePearl.IsHeartPearl())
        {
            playerModule.PossessionTarget = null;
            return;
        }

        const float possessionMaxDist = 400.0f;
        const float possessionLostDist = 400.0f;

        // search for target
        if (playerModule.PossessionTarget is null || !playerModule.PossessionTarget.TryGetTarget(out var target))
        {
            Creature? bestTarget = null;
            var shortestDist = float.MaxValue;

            foreach (var roomObject in self.room.physicalObjects)
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

                    if (self.abstractCreature.world.game.GetAllPearlcatModules().Any(x => x.PossessedCreature?.TryGetTarget(out var p) == true && p == creature.abstractCreature))
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

            if (bestTarget is not null)
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

            if (target.abstractCreature.controlled)
            {
                invalidTarget = true;
            }

            if (self.abstractCreature.world.game.GetAllPearlcatModules().Any(x => x.PossessedCreature?.TryGetTarget(out var p) == true && p == target.abstractCreature))
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


    public static void UpdatePearlpup(Player self, PlayerModule playerModule)
    {
        if (!self.IsFirstPearlcat())
        {
            return;
        }

        var save = self.abstractCreature.Room.world.game.GetMiscWorld();
        var miscProg = Utils.MiscProgression;

        miscProg.HasPearlpup = false;
        miscProg.HasDeadPearlpup = false;

        if (save is not null)
        {
            save.HasPearlpupWithPlayer = false;
            save.HasPearlpupWithPlayerDeadOrAlive = false;
        }


        // Can get a reference to pearlpup (i.e. they're in the world somewhere)
        if (playerModule.PearlpupRef is not null)
        {
            var pup = playerModule.PearlpupRef;

            var sameRoom = pup.abstractCreature.Room == self.abstractCreature.Room;

            miscProg.HasPearlpup = !pup.dead && sameRoom;
            miscProg.HasDeadPearlpup = pup.dead && sameRoom;

            if (save is null)
            {
                return;
            }

            save.HasPearlpupWithPlayer = miscProg.HasPearlpup;
            save.HasPearlpupWithPlayerDeadOrAlive = sameRoom;
            return;
        }


        if (self.room is null)
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
                    playerModule.AbstractPearlpupRef = new(player.abstractCreature);
                    return;
                }
            }
        }

        playerModule.AbstractPearlpupRef = null;
    }
}
