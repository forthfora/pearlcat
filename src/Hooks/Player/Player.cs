using IL.MoreSlugcats;
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
using VoidSea;
using static AbstractPhysicalObject;
using static DataPearl.AbstractDataPearl;
using MoreSlugcatsEnums = MoreSlugcats.MoreSlugcatsEnums;

namespace Pearlcat;

public static partial class Hooks
{
    public static void ApplyPlayerHooks()
    {
        On.Player.Update += Player_Update;
        On.Player.checkInput += Player_checkInput;

        On.Player.Grabability += Player_Grabability;

        On.Player.Die += Player_Die;

        On.Creature.SuckedIntoShortCut += Creature_SuckedIntoShortCut;
        On.Creature.SpitOutOfShortCut += Creature_SpitOutOfShortCut;
        On.Creature.Violence += Creature_Violence;

        On.Player.SpearOnBack.Update += SpearOnBack_Update;

        //On.Player.Jump += Player_Jump;
        //IL.Player.Jump += Player_JumpIL;

        new Hook(
            typeof(Player).GetProperty(nameof(Player.VisibilityBonus), BindingFlags.Instance | BindingFlags.Public).GetGetMethod(),
            typeof(Hooks).GetMethod(nameof(GetPlayerVisibilityBonus), BindingFlags.Static | BindingFlags.Public)
        );

        On.Player.ctor += Player_ctor;
        On.Creature.Grasp.Release += Grasp_Release;

        On.VoidSea.VoidSeaScene.Update += VoidSeaScene_Update;
       
        try
        {
            IL.Creature.Update += Creature_Update;
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("Player Hooks IL Exception: \n" + e);
        }
    }

    private static void VoidSeaScene_Update(On.VoidSea.VoidSeaScene.orig_Update orig, VoidSeaScene self, bool eu)
    {
        orig(self, eu);

        if (!self.room.game.IsPearlcatStory()) return;

        foreach (var obj in self.room.updateList)
        {
            if (obj is not Player player) continue;

            if (player.inVoidSea) continue;

            if (!player.IsPearlpup()) continue;

            player.inVoidSea = true;
            self.UpdatePlayerInVoidSea(player);
        }

        if (self.deepDivePhase == VoidSeaScene.DeepDivePhase.EggScenario)
        {
            var save = self.room.abstractRoom.world.game.GetMiscProgression();
            save.JustAscended = true;
        }
    }

    // void sea limits (150 - 160) (4260 - 4275)
    // tiles (-9) (214)
    // only the first player does not have this issue at all


    private static void Grasp_Release(On.Creature.Grasp.orig_Release orig, Creature.Grasp self)
    {
        if (self.grabber is not Player player)
        {
            orig(self);
            return;
        }

        var inVoid = (player.inVoidSea || player.room?.roomSettings?.name == "SB_L01");

        if (inVoid && player.IsPearlcat() && self.grabbed?.firstChunk?.owner is Player pup && pup.IsPearlpup()) return;

        if (inVoid && player.IsPearlpup()) return;
        
        orig(self);
    }

    private static void Creature_Update(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(MoveType.After,
            x => x.MatchLdstr("FORCE CREATURE RELEASE UNDER ROOM"));

        var dest = c.DefineLabel();

        c.GotoPrev(MoveType.After,
            x => x.MatchBle(out dest));

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Func<Creature, bool>>((self) =>
        {
            if (self is not Player player)
            {
                return false;
            }

            var inVoid = (player.inVoidSea || player.room?.roomSettings?.name == "SB_L01");

            if (inVoid && player.IsPearlpup())
            {
                // Plugin.Logger.LogWarning("PREVENTED PEARLPUP GRASP RELEASE");
                return true;
            }

            // Plugin.Logger.LogWarning("DID NOT PREVENT RELEASE");
            return false;
        });

        c.Emit(OpCodes.Brtrue, dest);

        // Plugin.Logger.LogWarning(c.Context);
    }


    private static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);

        if (!self.TryGetPearlcatModule(out _)) return;

        if (ModOptions.EnableBackSpear.Value)
        {
            self.spearOnBack ??= new Player.SpearOnBack(self);
        }
    }

    // yucky
    private static void Player_JumpIL(ILContext il)
    {
        var c = new ILCursor(il);

        // Jump from Roll
        c.GotoNext(MoveType.After,
            x => x.MatchLdsfld<Player.AnimationIndex>(nameof(Player.AnimationIndex.RocketJump)),
            x => x.MatchStfld<Player>(nameof(Player.animation))
        );

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Action<Player>>((player) =>
        {
            if (!player.TryGetPearlcatModule(out var playerModule))
                return;

            var effect = playerModule.CurrentPOEffect;

            player.bodyChunks[0].vel.x *= effect.RollSpeedFac;
            player.bodyChunks[0].vel.y *= effect.RollSpeedFac;

            player.bodyChunks[1].vel.x *= effect.RollSpeedFac;
            player.bodyChunks[1].vel.y *= effect.RollSpeedFac;
        });


        // Jump from Slide
        c.GotoNext(MoveType.After,
            x => x.MatchStfld<Player>(nameof(Player.rocketJumpFromBellySlide))
        );

        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldloc_0);        
        c.EmitDelegate<Action<Player, float>>((player, origValue) =>
        {
            if (!player.TryGetPearlcatModule(out var playerModule))
                return;

            var effect = playerModule.CurrentPOEffect;

            player.bodyChunks[1].vel = new Vector2(player.rollDirection * effect.SlideSpeedFac, effect.SlideSpeedFac) * origValue * (player.longBellySlide ? 1.2f : 1.0f);
            player.bodyChunks[0].vel = new Vector2(player.rollDirection * effect.SlideSpeedFac, effect.SlideSpeedFac) * origValue * (player.longBellySlide ? 1.2f : 1.0f);
        });


        // Backflip
        //c.GotoNext(MoveType.After,
        //    x => x.MatchLdsfld<Player.AnimationIndex>(nameof(Player.AnimationIndex.Flip)),
        //    x => x.MatchStfld<Player>(nameof(Player.jumpBoost))
        //);

        //c.Emit(OpCodes.Ldarg_0);
        //c.Emit(OpCodes.Ldloc_0);
        //c.EmitDelegate<Action<Player, float>>((player, origValue) =>
        //{
        //    if (!player.TryGetPearlcatModule(out var playerModule))
        //        return;

        //    var effect = playerModule.CurrentPOEffect;

        //    player.bodyChunks[0].vel.y = (effect.JumpHeightFac + 2) * origValue;
        //    player.bodyChunks[1].vel.y = effect.JumpHeightFac * origValue;
        //});
    }
    private static void Player_Jump(On.Player.orig_Jump orig, Player self)
    {
        orig(self);

        if (!self.TryGetPearlcatModule(out var playerModule)) return;

        self.jumpBoost *= playerModule.CurrentPOEffect.JumpHeightFac;
    }


    private static void Creature_SpitOutOfShortCut(On.Creature.orig_SpitOutOfShortCut orig, Creature self, IntVector2 pos, Room newRoom, bool spitOutAllSticks)
    {
        orig(self, pos, newRoom, spitOutAllSticks);

        if (self is Player p && p.TryGetPearlcatModule(out var mod))
            mod.LastGroundedPos = p.firstChunk.pos;

        foreach (var playerModule in self.abstractCreature.Room.world.game.GetAllPlayerData())
        {
            foreach (var item in playerModule.Inventory) 
            {
                if (!item.TryGetSentry(out var sentry)) continue;

                if (!item.TryGetModule(out var module)) continue;

                if (module.CooldownTimer != 0 && sentry.ShieldTimer <= 0) continue;

                var effect = item.GetPOEffect();
                if (effect.MajorEffect != POEffect.MajorEffectType.SHIELD) continue;
                
                if (!sentry.OwnerRef.TryGetTarget(out var owner)) continue;
                
                if (owner.realizedObject == null) continue;

                if (!Custom.DistLess(owner.realizedObject.firstChunk.pos, newRoom.MiddleOfTile(pos), 75.0f)) continue;

                if (sentry.ShieldTimer <= 0)
                    sentry.ShieldTimer = ModOptions.ShieldDuration.Value * 3.0f;
                
                owner.realizedObject.room?.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, owner.realizedObject.firstChunk, false, 1.0f, 0.7f);
                owner.realizedObject.room?.DeflectEffect(newRoom.MiddleOfTile(pos));
                
                self.SuckedIntoShortCut(pos, false);
            }
        }
    }


    public delegate float orig_PlayerVisibilityBonus(Player self);
    public static float GetPlayerVisibilityBonus(orig_PlayerVisibilityBonus orig, Player self)
    {
        if (self.TryGetPearlcatModule(out var playerModule) || self.onBack?.TryGetPearlcatModule(out playerModule) == true
            || (self.grabbedBy.FirstOrDefault(x => x.grabber is Player)?.grabber as Player)?.TryGetPearlcatModule(out playerModule) == true)
            if (playerModule.CamoLerp > 0.5f)
                return -playerModule.CamoLerp;

        return orig(self);
    }


    private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        if (self.TryGetPearlcatModule(out var playerModule) && self.spearOnBack != null)
            playerModule.WasSpearOnBack = self.spearOnBack.HasASpear;


        orig(self, eu);

        // zero G movement assist
        if (self.room != null && self.room.game.IsPearlcatStory() && self.room.roomSettings.name == "SS_AI" && self.room.gravity == 0.0f)
        {
            if (self.firstChunk.vel.magnitude < 7.5f)
            {
                self.firstChunk.vel += self.input[0].analogueDir * 0.7f;
            }

            if (self.input[0].analogueDir.magnitude < 0.05f)
            {
                self.firstChunk.vel *= 0.8f;
            }
        }

        if (playerModule == null) return;

        playerModule.BaseStats = self.Malnourished ? playerModule.MalnourishedStats : playerModule.NormalStats;

        var unblockedInput = playerModule.UnblockedInput;
        var allowInput = self.Consious && !self.inVoidSea && !self.Sleeping;

        bool swapLeftInput = self.IsSwapLeftInput() && allowInput;
        bool swapRightInput = self.IsSwapRightInput() && allowInput;

        bool swapInput = self.IsSwapKeybindPressed() && allowInput;
        bool storeInput = self.IsStoreKeybindPressed(playerModule) && allowInput;

        bool agilityInput = self.IsAgilityKeybindPressed(playerModule) && allowInput;
        bool sentryInput = self.IsSentryKeybindPressed(playerModule) && allowInput;

        int numPressed = self.IsFirstPearlcat() && allowInput ? self.GetNumberPressed() : -1;

        playerModule.BlockInput = false;

        if (numPressed >= 0)
        {
            self.ActivateObjectInStorage(numPressed - 1);
        }
        else if (swapLeftInput && !playerModule.WasSwapLeftInput)
        {
            self.SelectPreviousObject();
        }
        else if (swapRightInput && !playerModule.WasSwapRightInput)
        {
            self.SelectNextObject();
        }
        else if (swapInput)
        {
            playerModule.BlockInput = true;
            playerModule.ShowHUD(10);

            if (!playerModule.WasSwapped)
            {
                if (unblockedInput.x < -0.5f)
                {
                    self.SelectPreviousObject();
                    playerModule.WasSwapped = true;
                }
                else if (unblockedInput.x > 0.5f)
                {
                    self.SelectNextObject();
                    playerModule.WasSwapped = true;
                }
            }
            else if (Mathf.Abs(unblockedInput.x) < 0.5f)
            {
                playerModule.WasSwapped = false;
            }
        }

        UpdateAll(self, playerModule);

        playerModule.WasSwapLeftInput = swapLeftInput;
        playerModule.WasSwapRightInput = swapRightInput;
        playerModule.WasStoreInput = storeInput;
        playerModule.WasAgilityInput = agilityInput;
        playerModule.WasSentryInput = sentryInput;

        // LAG CAUSER
        if (playerModule.TextureUpdateTimer > self.TexUpdateInterval() && !ModOptions.DisableCosmetics.Value)
        {
            if ((playerModule.LastBodyColor != playerModule.BodyColor || playerModule.LastAccentColor != playerModule.AccentColor || playerModule.SetInvertTailColors != playerModule.CurrentlyInvertedTailColors))
            {
                playerModule.LoadTailTexture("tail");
                playerModule.LoadEarLTexture("ear_l");
                playerModule.LoadEarRTexture("ear_r");
            }

            playerModule.LastBodyColor = playerModule.BodyColor;
            playerModule.LastAccentColor = playerModule.AccentColor;

            playerModule.TextureUpdateTimer = 0;
        }
        else
        {
            playerModule.TextureUpdateTimer++;
        }



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

        if (playerModule.ReviveCount > 0 && self.InDeathpit())
        {
            self.Die();
            self.SuperHardSetPosition(playerModule.LastGroundedPos);

            self.graphicsModule?.Reset();
            playerModule.FlyTimer = 60;

            var slugOnBack = self.slugOnBack?.slugcat;

            if (slugOnBack != null)
            {
                slugOnBack.SuperHardSetPosition(playerModule.LastGroundedPos);
                slugOnBack.graphicsModule?.Reset();
            }
        }
        if (playerModule.FlyTimer > 0)
        {
            playerModule.FlyTimer--;

            self.firstChunk.vel.x = self.input[0].x * 5.0f;
            self.firstChunk.vel.y = 6.0f;
        }

        if (self.inVoidSea || playerModule.Inventory.Any(x => x.Room != self.abstractCreature.Room))
        {
            self.AbstractizeInventory();
        }

        if (playerModule.PearlpupRef != null && playerModule.PearlpupRef.TryGetTarget(out var pup) && pup.room != null && pup.InDeathpit() && playerModule.ReviveCount > 0)
        {
            pup.SuperHardSetPosition(self.firstChunk.pos);
            
            pup.Die();
            pup.RevivePlayer();

            pup.graphicsModule.Reset();
            pup.Stun(40);

            playerModule.SetReviveCooldown(-1);
        }

        playerModule.LastRoom = self.abstractCreature.Room;

        if (playerModule.GraphicsResetCounter > 0)
        {
            playerModule.GraphicsResetCounter--;
            self.graphicsModule?.Reset();
        }
    }

    private static void UpdateAll(Player self, PlayerModule playerModule)
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

        UpdatePlayerOA(self, playerModule);
        UpdatePlayerDaze(self, playerModule);
        UpdatePostDeathInventory(self, playerModule);

        UpdateCombinedPOEffect(self, playerModule);
        ApplyCombinedPOEffect(self, playerModule);

        UpdateHUD(self, playerModule);
        UpdateSFX(self, playerModule);

        UpdateStoreRetrieveObject(self, playerModule);

        UpdateTryRevive(self, playerModule);

        if (self.room != null)
        {
            self.GivePearls(playerModule);
        }

        RefreshPearlpup(self, playerModule);
    }

    private static void RefreshPearlpup(Player self, PlayerModule playerModule)
    {
        if (!self.IsFirstPearlcat()) return;
        
        var save = self.abstractCreature.Room.world.game.GetMiscWorld();
        var miscProg = self.abstractCreature.Room.world.game.GetMiscProgression();

        miscProg.HasPearlpup = false;
        
        if (save != null)
        {
            save.HasPearlpupWithPlayer = false;
        }

        if (playerModule.PearlpupRef != null && playerModule.PearlpupRef.TryGetTarget(out var pup))
        {
            miscProg.HasPearlpup = !pup.dead && pup.abstractCreature.Room == self.abstractCreature.Room;

            if (save != null)
            {
                save.HasPearlpupWithPlayer = miscProg.HasPearlpup;
            }

            return;
        }

        if (self.room == null) return;

        foreach (var roomObject in self.room.physicalObjects)
        {
            foreach (var physicalobject in roomObject)
            {
                if (physicalobject is not Player player) continue;

                if (!player.IsPearlpup()) continue;

                playerModule.PearlpupRef = new(player);
                return;
            }
        }

        playerModule.PearlpupRef = null;

        if (save != null)
        {
            save.PearlpupID = null;
        }
    }

    private static void UpdateTryRevive(Player self, PlayerModule playerModule)
    {
        bool shouldTryRevive = false;

        if (self.dead)
            shouldTryRevive = true;

        if (self.dangerGraspTime >= 60 && self.AI == null)
            shouldTryRevive = true;


        if (!shouldTryRevive) return;

        TryRevivePlayer(self, playerModule);
    }

    private static void UpdateStoreRetrieveObject(Player self, PlayerModule playerModule)
    {
        if (self.inVoidSea) return;

        if (!StoreObjectDelay.TryGet(self, out var storeObjectDelay)) return;

        var storeInput = self.IsStoreKeybindPressed(playerModule);
        var toStore = self.grasps[0]?.grabbed;
        var isStoring = self.grasps[0]?.grabbed.abstractPhysicalObject.IsStorable() ?? false;

        if (isStoring && toStore == null) return;

        if (!isStoring && self.FreeHand() == -1) return;

        if (!isStoring && playerModule.ActiveObject == null) return;


        if (playerModule.StoreObjectTimer > storeObjectDelay)
        {
            if (isStoring && toStore != null)
            {
                self.StoreObject(toStore.abstractPhysicalObject, true);
            }
            else if (playerModule.ActiveObject != null)
            {
                self.room.PlaySound(Enums.Sounds.Pearlcat_PearlRetrieve, playerModule.ActiveObject.realizedObject.firstChunk);
                self.RetrieveActiveObject();
            }

            playerModule.StoreObjectTimer = -1;
        }


        if (storeInput)
        {
            if (playerModule.StoreObjectTimer >= 0)
            {
                if (isStoring || (playerModule.ActiveObject != null && playerModule.ActiveObject.TryGetModule(out var module) && !module.IsReturningSentry))
                {
                    playerModule.StoreObjectTimer++;
                }
                
                playerModule.BlockInput = true;
                playerModule.ShowHUD(10);

                self.Blink(5);

                //var pGraphics = (PlayerGraphics)self.graphicsModule;
                //pGraphics.hands[self.FreeHand()].absoluteHuntPos = self.firstChunk.pos + new Vector2(50.0f, 0.0f);

                // every 5 frames
                if (playerModule.StoreObjectTimer % 5 == 0)
                {
                    if (isStoring)
                    {
                        var activeObjPos = self.GetActiveObjectPos();
                        toStore?.ConnectEffect(activeObjPos);                
                    }
                    else
                    {
                        var activeObj = playerModule.ActiveObject?.realizedObject;
                    
                        if (playerModule.ActiveObject?.TryGetModule(out module) == true)
                        {
                            if (!module.IsReturningSentry)
                                activeObj.ConnectEffect(self.firstChunk.pos);

                            module.RemoveSentry(playerModule.ActiveObject);
                        }
                        else
                        {
                            activeObj.ConnectEffect(self.firstChunk.pos);
                        }
                    }
                }
            }
        }
        else
        {
            playerModule.StoreObjectTimer = 0;
        }
    }

    private static void UpdateSFX(Player self, PlayerModule playerModule)
    {
        // Outsider breaks looping SFX sometimes, this is safety
        try
        {
            playerModule.MenuCrackleLoop.Update();
            playerModule.MenuCrackleLoop.Volume = playerModule.HudFade;

            playerModule.ShieldHoldLoop.Update();
            playerModule.ShieldHoldLoop.Volume = playerModule.ShieldTimer > 0 ? 1.0f : 0.0f;

            // forced to fade out for some reason
            //if (playerModule.ActiveObject?.GetPOEffect().RMSong == true && self.firstChunk.vel.magnitude < 3.0f)
            //{
            //    if (self.room.game.manager.musicPlayer.song == null || self.room.game.manager.musicPlayer.song is not HalcyonSong)
            //        self.room.game.manager.musicPlayer.RequestHalcyonSong("NA_19 - Halcyon Memories");
            //}
            //else
            //{
            //    if (self.room.game.manager.musicPlayer != null && self.room.game.manager.musicPlayer.song != null && self.room.game.manager.musicPlayer.song is HalcyonSong)
            //        self.room.game.manager.musicPlayer.song.FadeOut(20f);
            //}
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("Looping SFX Error, Outsider often likes to cause this:\n" + e);
        }
    }

    private static void UpdateHUD(Player self, PlayerModule playerModule)
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

    private static void UpdatePostDeathInventory(Player self, PlayerModule playerModule)
    {
        if (self.dead || playerModule.PostDeathInventory.Count == 0) return;

        for (int i = playerModule.PostDeathInventory.Count - 1; i >= 0; i--)
        {
            var item = playerModule.PostDeathInventory[i];
            playerModule.PostDeathInventory.RemoveAt(i);

            if (item.realizedObject == null) continue;

            if (item.realizedObject.room != self.room) continue;

            if (item.realizedObject.grabbedBy.Count > 0) continue;

            if (item.IsPlayerObject()) return;

            if (ObjectAddon.ObjectsWithAddon.TryGetValue(item, out var _))
                ObjectAddon.ObjectsWithAddon.Remove(item);

            self.StoreObject(item);
        }

        if (playerModule.PostDeathActiveObjectIndex != null)
            ActivateObjectInStorage(self, (int)playerModule.PostDeathActiveObjectIndex);
        
        playerModule.PostDeathActiveObjectIndex = null;
    }

    private static void UpdatePlayerDaze(Player self, PlayerModule playerModule)
    {
        if (!DazeDuration.TryGet(self, out var dazeDuration)) return;

        if (self.dead || self.bodyMode == Player.BodyModeIndex.Stunned || self.Sleeping)
            playerModule.DazeTimer = dazeDuration;

        if (playerModule.DazeTimer > 0)
            playerModule.DazeTimer--;
    }

    private static void UpdatePlayerOA(Player self, PlayerModule playerModule)
    {
        if (self.bodyMode == Player.BodyModeIndex.Stunned || self.bodyMode == Player.BodyModeIndex.Dead)
        {
            playerModule.CurrentObjectAnimation = new FreeFallOA(self);
        }
        else if (self.Sleeping || self.sleepCurlUp > 0.0f)
        {
            playerModule.CurrentObjectAnimation = new SleepOA(self);
        }
        else if (playerModule.CurrentObjectAnimation is SleepOA or FreeFallOA)
        {
            foreach (var abstractObject in playerModule.Inventory)
            {
                if (abstractObject.TryGetSentry(out _)) continue;
                
                abstractObject.realizedObject.ConnectEffect(((PlayerGraphics)self.graphicsModule).head.pos);
            }

            playerModule.PickObjectAnimation(self);
        }
        else
        {
            if (playerModule.CurrentObjectAnimation is SineWaveInterOA or SineWaveOA && self.firstChunk.vel.magnitude > 4.0f)
            {
                playerModule.PickObjectAnimation(self);
            }
        }

        if (playerModule.ObjectAnimationTimer > playerModule.ObjectAnimationDuration)
            playerModule.PickObjectAnimation(self);

        playerModule.CurrentObjectAnimation?.Update(self);
        playerModule.ObjectAnimationTimer++;
    }


    private static void Player_checkInput(On.Player.orig_checkInput orig, Player self)
    {
        orig(self);
        
        if (!self.TryGetPearlcatModule(out var playerModule)) return;
        
        var input = self.input[0];
        playerModule.UnblockedInput = input;

        if (playerModule.BlockInput)
        {
            input.x = 0;
            input.y = 0;
            input.analogueDir *= 0f;

            input.jmp = false;
            input.thrw = false;
            input.pckp = false;
        }

        self.input[0] = input;
    }

    private static void Player_Die(On.Player.orig_Die orig, Player self)
    {
        var wasDead = self.dead;

        orig(self);

        //Plugin.Logger.LogWarning(self.mainBodyChunk.pos);

        if (wasDead) return;

        if (!self.TryGetPearlcatModule(out var playerModule)) return;

        playerModule.ReviveTimer = 0;
        playerModule.ShieldTimer = 0;
        playerModule.SpearTimer = 0;

        playerModule.PostDeathActiveObjectIndex = playerModule.ActiveObjectIndex;

        self.room?.PlaySound(SoundID.Zapper_Zap, self.firstChunk.pos, 0.4f, 0.6f);
        self.room?.PlaySound(SoundID.Fire_Spear_Explode, self.firstChunk.pos, 0.7f, 0.6f);

        if (playerModule.ReviveCount <= 0 && playerModule.Inventory.Count > 0)
        {
            self.room?.AddObject(new ShockWave(self.firstChunk.pos, 30.0f, 0.4f, 5, false));
            self.room?.AddObject(new ExplosionSpikes(self.room, self.firstChunk.pos, 5, 20.0f, 10, 20.0f, 20.0f, Color.red));
        }

        for (int i = playerModule.Inventory.Count - 1; i >= 0; i--)
        {
            var abstractObject = playerModule.Inventory[i];

            RemoveFromInventory(self, abstractObject);

            playerModule.PostDeathInventory.Add(abstractObject);

            if (playerModule.ReviveCount <= 0)
            {
                var randVec = Custom.RNV() * 150.0f;
                self.room?.ConnectEffect(self.firstChunk.pos, self.firstChunk.pos + randVec, abstractObject.GetObjectColor(), 1.5f, 80);

                DeathEffect(abstractObject.realizedObject);
            }
        }
    }

    private static Player.ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
    {
        var result = orig(self, obj);

        if (obj != null && obj.abstractPhysicalObject.IsPlayerObject())
            return Player.ObjectGrabability.CantGrab;

        return result;
    }

    private static void Creature_SuckedIntoShortCut(On.Creature.orig_SuckedIntoShortCut orig, Creature self, IntVector2 entrancePos, bool carriedByOther)
    {
        if (self is Player player && player.TryGetPearlcatModule(out var playerModule))
        {
            var sameRoom = player.abstractCreature.Room == playerModule.LastRoom;
            //var sameRoom = false;

            player.AbstractizeInventory(sameRoom);
            player.slugOnBack?.slugcat?.AbstractizeInventory(sameRoom);
        }

        orig(self, entrancePos, carriedByOther);
    }

    private static void Creature_Violence(On.Creature.orig_Violence orig, Creature self, BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus)
    {
        // sin number 2
        if (self is Player player && player.TryGetPearlcatModule(out var playerModule) && source != null)
        {
            bool shouldShield = playerModule.ShieldActive;
            var attacker = source.owner;

            if (attacker is JetFish)
                shouldShield = false;

            if (attacker is Cicada)
                shouldShield = false;

            if (attacker is Centipede centipede && centipede.Small)
                shouldShield = false;

            if (damage <= 0.1f)
                shouldShield = false;

            if (shouldShield)
            {
                playerModule.ActivateVisualShield();
                return;
            }
        }

        orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
    }


    public static void GivePearls(this Player self, PlayerModule playerModule)
    {
        var save = self.room.game.GetMiscWorld();
        bool shouldGivePearls = true;

        if (save != null)
            shouldGivePearls = !save.PlayersGivenPearls.Contains(self.playerState.playerNumber);

        if (ModOptions.InventoryOverride.Value && playerModule.JustWarped)
            playerModule.GivenPearls = false;

        if (!(shouldGivePearls || ModOptions.InventoryOverride.Value) || playerModule.GivenPearls) return;


        List<DataPearlType> pearls;
        bool overrideLimit = false;

        if (ModOptions.InventoryOverride.Value || ModOptions.StartingInventoryOverride.Value)
        {
            pearls = ModOptions.GetOverridenInventory(self.IsFirstPearlcat() || self.abstractCreature.world.game.IsArenaSession);
        }
        else
        {
            // Defaults
            pearls = new List<DataPearlType>()
            {
                Enums.Pearls.AS_PearlBlue,
                Enums.Pearls.AS_PearlYellow,
                Enums.Pearls.AS_PearlGreen,
                Enums.Pearls.AS_PearlBlack,
                Enums.Pearls.AS_PearlRed,
                self.IsFirstPearlcat() || self.abstractCreature.world.game.IsArenaSession ? Enums.Pearls.RM_Pearlcat : DataPearlType.Misc,
            };

            if (ModOptions.MaxPearlCount.Value <= 1)
                pearls.Remove(Enums.Pearls.AS_PearlBlack);

            overrideLimit = true;
        }

        foreach (var pearlType in pearls)
        {
            var pearl = new DataPearl.AbstractDataPearl(self.room.world, AbstractObjectType.DataPearl, null, self.abstractPhysicalObject.pos, self.room.game.GetNewID(), -1, -1, null, pearlType);
            self.StoreObject(pearl, overrideLimit: overrideLimit);
        }

        playerModule.GivenPearls = true;

        if (save != null && !save.PlayersGivenPearls.Contains(self.playerState.playerNumber))
            save.PlayersGivenPearls.Add(self.playerState.playerNumber);
    }
    
    public static int GraspsHasType(this Player self, AbstractObjectType type)
    {
        for (int i = 0; i < self.grasps.Length; i++)
        {
            Creature.Grasp? grasp = self.grasps[i];
            
            if (grasp == null) continue;

            if (grasp.grabbed.abstractPhysicalObject.type == type)
                return i;
        }

        return -1;
    }

    private static void SpearOnBack_Update(On.Player.SpearOnBack.orig_Update orig, Player.SpearOnBack self, bool eu)
    {
        orig(self, eu);

        if (!self.owner.TryGetPearlcatModule(out var playerModule)) return;

        if (playerModule.ForceLockSpearOnBack)
            self.interactionLocked = true;
    }

    public static void TryRevivePlayer(this Player self, PlayerModule playerModule)
    {
        if (playerModule.ReviveCount <= 0) return;

        if (self.room == null) return;

        //if (self.room == null || self.graphicsModule == null) return;

        //if (self.killTag?.creatureTemplate is CreatureTemplate template
        //    && (template.type == CreatureTemplate.Type.DaddyLongLegs || template.type == CreatureTemplate.Type.BrotherLongLegs
        //    || template.type == CreatureTemplate.Type.BigEel || template.type == MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs)) return;

        self.AllGraspsLetGoOfThisObject(true);

        DeflectEffect(self.room, self.firstChunk.pos);
        playerModule.ShieldTimer = 200;

        if (self.dead)
            self.RevivePlayer();

        else
            self.room.ReviveEffect(self.firstChunk.pos);

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
        
        if (!self.TryGetPearlcatModule(out var playerModule)) return;

        playerModule.PickObjectAnimation(self);
    }
    
    public static void Revive(this Creature self)
    {
        //self.graphicsModule?.ReleaseAllInternallyContainedSprites();

        if (self.State is HealthState healthState)
            healthState.health = 1.0f;

        self.State.alive = true;

        self.dead = false;
        self.killTag = null;
        self.killTagCounter = 0;
        self.abstractCreature.abstractAI?.SetDestination(self.abstractCreature.pos);

        if (self is not Player)
            self.Stun(100);

        self.room.ReviveEffect(self.mainBodyChunk.pos);
    }


    public static bool IsHostileToMe(this Creature self, Creature creature)
    {
        // trust no one, not even yourself?
        if (creature == self)
            return false;

        if (creature is Player pup && pup.IsPearlpup())
            return false;

        var AI = creature.abstractCreature.abstractAI?.RealAI;

        if (self is Player && AI is LizardAI or ScavengerAI or BigNeedleWormAI or DropBugAI or CicadaAI or MoreSlugcats.InspectorAI)
        {
            var aggression = AI.CurrentPlayerAggression(self.abstractCreature);

            var rep = AI.tracker.RepresentationForCreature(self.abstractCreature, false);

            if (rep?.dynamicRelationship == null)
                return false;

            if (AI is LizardAI)
                return aggression > 0.0f;

            if (AI is ScavengerAI)
                return aggression > 0.5f;

            if (AI is BigNeedleWormAI)
                return aggression > 0.0f;

            if (AI is CicadaAI)
                return aggression > 0.0f;

            if (AI is DropBugAI)
                return true;

            if (AI is MoreSlugcats.InspectorAI)
                return aggression > 0.0f;

            return false;
        }

        if (self is Player && creature is Player player && !player.isSlugpup)
        {
            var game = self.abstractCreature.world.game;

            if (game.IsArenaSession && game.GetArenaGameSession.GameTypeSetup.spearsHitPlayers)
                return true;
        }

        var myRelationship = self.abstractCreature.creatureTemplate.CreatureRelationship(self.abstractCreature.creatureTemplate);
        var creatureRelationship = creature.abstractCreature.creatureTemplate.CreatureRelationship(self.abstractCreature.creatureTemplate);

        return myRelationship.GoForKill || creatureRelationship.GoForKill;
    }

    // https://github.com/WondaMegapon/pitrespawn/blob/master/PitRespawn.cs
    public static bool InDeathpit(this Player self) => self.mainBodyChunk.pos.y < -300.0f
        && (!self.room.water || self.room.waterInverted || self.room.defaultWaterLevel < -10) && (!self.Template.canFly || self.Stunned || self.dead) && self.room.deathFallGraphic != null;
}
