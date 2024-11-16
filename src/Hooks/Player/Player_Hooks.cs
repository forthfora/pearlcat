using RWCustom;
using System.Linq;
using UnityEngine;
using VoidSea;
using static Pearlcat.Player_Helpers;

namespace Pearlcat;

public static class Player_Hooks
{
    public static void ApplyHooks()
    {
        On.Player.Update += Player_Update;
        On.Player.checkInput += Player_checkInput;

        On.Player.Grabability += Player_Grabability;

        On.Player.Die += Player_Die;

        On.Creature.SuckedIntoShortCut += Creature_SuckedIntoShortCut;
        On.Creature.SpitOutOfShortCut += Creature_SpitOutOfShortCut;
        On.Creature.Violence += Creature_Violence;

        On.Player.SpearOnBack.Update += SpearOnBack_Update;

        On.Player.ctor += Player_ctor;
        On.Creature.Grasp.Release += Grasp_Release;

        On.VoidSea.VoidSeaScene.Update += VoidSeaScene_Update;

        On.Player.ThrownSpear += Player_ThrownSpear;
        On.Player.Stun += PlayerOnStun;
    }


    private static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);

        if (!self.TryGetPearlcatModule(out _))
        {
            return;
        }

        if (ModOptions.EnableBackSpear.Value)
        {
            self.spearOnBack ??= new Player.SpearOnBack(self);
        }
    }

    private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        if (self.TryGetPearlcatModule(out var playerModule) && self.spearOnBack != null)
        {
            playerModule.WasSpearOnBack = self.spearOnBack.HasASpear;
        }

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

        if (playerModule == null)
        {
            return;
        }

        playerModule.BaseStats = self.Malnourished ? playerModule.MalnourishedStats : playerModule.NormalStats;

        var unblockedInput = playerModule.UnblockedInput;
        var allowInput = self.Consious && !self.inVoidSea && !self.Sleeping && self.controller == null;

        var swapLeftInput = self.IsSwapLeftInput() && allowInput;
        var swapRightInput = self.IsSwapRightInput() && allowInput;

        var swapInput = self.IsSwapKeybindPressed() && allowInput;
        var storeInput = self.IsStoreKeybindPressed(playerModule) && allowInput;

        var agilityInput = self.IsAgilityKeybindPressed(playerModule) && allowInput;
        var sentryInput = self.IsSentryKeybindPressed(playerModule) && allowInput;


        playerModule.BlockInput = false;

        if (swapLeftInput && !playerModule.WasSwapLeftInput)
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
                playerModule.LoadTailTexture(playerModule.IsPearlpupAppearance ? "pearlpup_adulttail" : "tail");
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

        if (playerModule.ReviveCount > 0 && self.InDeathPit())
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

        if (playerModule.PearlpupRef != null && playerModule.PearlpupRef.TryGetTarget(out var pup) && pup.room != null && pup.InDeathPit() && playerModule.ReviveCount > 0)
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


    private static void Player_checkInput(On.Player.orig_checkInput orig, Player self)
    {
        orig(self);
        
        if (!self.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

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

        if (wasDead)
        {
            return;
        }

        if (!self.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        playerModule.ReviveTimer = 0;
        playerModule.ShieldTimer = 0;
        playerModule.SpearTimer = 0;

        playerModule.PostDeathActiveObjectIndex = playerModule.ActiveObjectIndex;

        self.room?.PlaySound(SoundID.Zapper_Zap, self.firstChunk.pos, 0.4f, 0.6f);
        self.room?.PlaySound(SoundID.Fire_Spear_Explode, self.firstChunk.pos, 0.7f, 0.6f);

        if (playerModule.ReviveCount <= 0 && playerModule.Inventory.Count > 0)
        {
            self.room?.AddObject(new ShockWave(self.firstChunk.pos, 30.0f, 0.4f, 5));
            self.room?.AddObject(new ExplosionSpikes(self.room, self.firstChunk.pos, 5, 20.0f, 10, 20.0f, 20.0f, Color.red));
        }

        for (var i = playerModule.Inventory.Count - 1; i >= 0; i--)
        {
            var abstractObject = playerModule.Inventory[i];

            self.RemoveFromInventory(abstractObject);

            playerModule.PostDeathInventory.Add(abstractObject);


            if (i < PlayerPearl_Helpers.MaxPearlsWithEffects)
            {
                if (playerModule.ReviveCount <= 0)
                {
                    var randVec = Custom.RNV() * 150.0f;

                    self.room?.ConnectEffect(self.firstChunk.pos, self.firstChunk.pos + randVec, abstractObject.GetObjectColor(), 1.5f, 80);
                    abstractObject.realizedObject.DeathEffect();
                }
            }
        }
    }

    private static void PlayerOnStun(On.Player.orig_Stun orig, Player self, int st)
    {
        if (self.TryGetPearlcatModule(out var playerModule))
        {
            if (playerModule.IsPossessingCreature)
            {
                return;
            }
        }

        orig(self, st);
    }


    private static void Player_ThrownSpear(On.Player.orig_ThrownSpear orig, Player self, Spear spear)
    {
        orig(self, spear);

        if (spear.abstractSpear.TryGetSpearModule(out var spearModule))
        {
            spearModule.ReturnTimer = -1;
            spearModule.ThrownByPlayer = new(self);
        }
    }

    private static void SpearOnBack_Update(On.Player.SpearOnBack.orig_Update orig, Player.SpearOnBack self, bool eu)
    {
        orig(self, eu);

        if (!self.owner.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        if (playerModule.ForceLockSpearOnBack)
        {
            self.interactionLocked = true;
        }
    }


    private static Player.ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
    {
        var result = orig(self, obj);

        if (obj != null && obj.abstractPhysicalObject.IsPlayerPearl())
        {
            return Player.ObjectGrabability.CantGrab;
        }

        return result;
    }

    private static void Grasp_Release(On.Creature.Grasp.orig_Release orig, Creature.Grasp self)
    {
        if (self.grabber is not Player player)
        {
            orig(self);
            return;
        }

        var inVoid = (player.inVoidSea || player.room?.roomSettings?.name == "SB_L01");

        if (inVoid && player.IsPearlcat() && self.grabbed?.firstChunk?.owner is Player pup && pup.IsPearlpup())
        {
            return;
        }

        if (inVoid && player.IsPearlpup())
        {
            return;
        }

        orig(self);
    }


    private static void Creature_SpitOutOfShortCut(On.Creature.orig_SpitOutOfShortCut orig, Creature self, IntVector2 pos, Room newRoom, bool spitOutAllSticks)
    {
        orig(self, pos, newRoom, spitOutAllSticks);

        if (self is Player thisPlayer && thisPlayer.TryGetPearlcatModule(out var mod))
        {
            mod.LastGroundedPos = thisPlayer.firstChunk.pos;
        }

        foreach (var playerModule in self.abstractCreature.Room.world.game.GetAllPlayerData())
        {
            foreach (var item in playerModule.Inventory)
            {
                if (!item.TryGetSentry(out var sentry))
                {
                    continue;
                }

                if (!item.TryGetPlayerPearlModule(out var module))
                {
                    continue;
                }

                if (module.CooldownTimer != 0 && sentry.ShieldTimer <= 0)
                {
                    continue;
                }

                var effect = item.GetPearlEffect();
                if (effect.MajorEffect != PearlEffect.MajorEffectType.SHIELD)
                {
                    continue;
                }

                if (!sentry.OwnerRef.TryGetTarget(out var owner))
                {
                    continue;
                }

                if (owner.realizedObject == null)
                {
                    continue;
                }

                if (!Custom.DistLess(owner.realizedObject.firstChunk.pos, newRoom.MiddleOfTile(pos), 75.0f))
                {
                    continue;
                }

                if (sentry.ShieldTimer <= 0)
                {
                    sentry.ShieldTimer = ModOptions.ShieldDuration.Value * 3.0f;
                }

                owner.realizedObject.room?.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, owner.realizedObject.firstChunk, false, 1.0f, 0.7f);
                owner.realizedObject.room?.DeflectEffect(newRoom.MiddleOfTile(pos));

                self.SuckedIntoShortCut(pos, false);
            }
        }
    }

    private static void Creature_SuckedIntoShortCut(On.Creature.orig_SuckedIntoShortCut orig, Creature self, IntVector2 entrancePos, bool carriedByOther)
    {
        if (self is Player player)
        {
            var sameRoom = false;
            
            if (player.TryGetPearlcatModule(out var playerModule) || player.slugOnBack?.slugcat?.TryGetPearlcatModule(out playerModule) == true)
            {
                sameRoom = player.abstractCreature.Room == playerModule.LastRoom;
            }

            player.AbstractizeInventory(sameRoom);
            player.slugOnBack?.slugcat?.AbstractizeInventory(sameRoom);
        }

        orig(self, entrancePos, carriedByOther);
    }

    private static void Creature_Violence(On.Creature.orig_Violence orig, Creature self, BodyChunk? source, Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus)
    {
        // sin number 2
        if (self is Player player && player.TryGetPearlcatModule(out var playerModule))
        {
            var shouldShield = playerModule.ShieldActive;
            var attacker = source?.owner;

            if (attacker is JetFish)
            {
                shouldShield = false;
            }

            if (attacker is Cicada)
            {
                shouldShield = false;
            }

            if (attacker is Centipede centipede && centipede.Small)
            {
                shouldShield = false;
            }

            if (damage <= 0.1f)
            {
                shouldShield = false;
            }

            if (shouldShield)
            {
                playerModule.ActivateVisualShield();
                return;
            }
        }

        orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
    }
    
    
    private static void VoidSeaScene_Update(On.VoidSea.VoidSeaScene.orig_Update orig, VoidSeaScene self, bool eu)
    {
        orig(self, eu);

        if (!self.room.game.IsPearlcatStory())
        {
            return;
        }

        foreach (var obj in self.room.updateList)
        {
            if (obj is not Player player)
            {
                continue;
            }

            if (player.inVoidSea)
            {
                continue;
            }

            if (!player.IsPearlpup())
            {
                continue;
            }

            player.inVoidSea = true;
            self.UpdatePlayerInVoidSea(player);
        }

        if (self.deepDivePhase == VoidSeaScene.DeepDivePhase.EggScenario)
        {
            var miscProg = Utils.GetMiscProgression();
            var miscWorld = self.room.abstractRoom.world.game.GetMiscWorld();

            miscProg.JustAscended = true;
            
            if (miscWorld?.HasPearlpupWithPlayer == true)
            {
                miscProg.AscendedWithPup = true;
            }

            SlugBase.Assets.CustomScene.SetSelectMenuScene(self.room.game.GetStorySession.saveState, Enums.Scenes.Slugcat_Pearlcat_Ascended);
        }
    }
}
