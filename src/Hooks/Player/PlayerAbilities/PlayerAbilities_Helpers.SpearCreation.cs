using System.Collections.Generic;
using System.Linq;
using MoreSlugcats;
using RWCustom;
using SlugBase.Features;
using UnityEngine;

namespace Pearlcat;

public static partial class PlayerAbilities_Helpers
{
    public static void UpdateSpearCreation(Player self, PlayerModule playerModule, PearlEffect effect)
    {
        if (ModOptions.DisableSpear.Value || self.inVoidSea || playerModule.PossessedCreature != null)
        {
            playerModule.DisabledEffects.Add(PearlEffect.MajorEffectType.SPEAR_CREATION);
            return;
        }

        var spearCreationTime = 30;
        playerModule.SpearLerp = Custom.LerpMap(playerModule.SpearTimer, 5, spearCreationTime, 0.0f, 1.0f);

        playerModule.ForceLockSpearOnBack = false;

        if (effect.MajorEffect != PearlEffect.MajorEffectType.SPEAR_CREATION)
        {
            playerModule.SpearTimer = 0;
            playerModule.SpearDelay = 0;
            return;
        }

        if (playerModule.SpearCount <= 0) return;

        playerModule.ForceLockSpearOnBack = self.spearOnBack != null &&
                                            (self.spearOnBack.HasASpear != playerModule.WasSpearOnBack ||
                                             spearCreationTime < 20);

        bool IsHoldingFoodOrPlayer(Player player)
        {
            var grasps = player.grasps;

            foreach (var grasp in grasps)
            {
                if (grasp == null) continue;

                if (grasp.grabbed is Player)
                {
                    return true;
                }


                // not hungry
                if (self.CurrentFood == self.slugcatStats.maxFood) continue;

                if (grasp.grabbed is Creature creature && creature.dead &&
                    PlayerFeatures.Diet.TryGet(self, out var diet) && diet.GetFoodMultiplier(creature) > 0)
                {
                    return true;
                }


                // not a consumable object
                if (grasp.grabbed?.abstractPhysicalObject is not AbstractConsumable) continue;

                if (grasp.grabbed?.abstractPhysicalObject is AbstractConsumable consumable
                    && consumable.realizedObject != null
                    && PlayerFeatures.Diet.TryGet(self, out diet)
                    && diet.GetFoodMultiplier(consumable.realizedObject) > 0)
                {
                    return true;
                }
            }

            return false;
        }

        var abilityInput = self.IsSpearCreationKeybindPressed(playerModule) &&
                           !self.IsStoreKeybindPressed(playerModule) && !IsHoldingFoodOrPlayer(self);

        var holdingSpear = self.GraspsHasType(AbstractPhysicalObject.AbstractObjectType.Spear) >= 0;

        //Plugin.Logger.LogWarning(self.eatCounter);

        if (abilityInput && ((self.spearOnBack == null && !holdingSpear) ||
                             (self.spearOnBack != null &&
                              (self.spearOnBack.interactionLocked || (!holdingSpear && !self.spearOnBack.HasASpear)) &&
                              !(holdingSpear && self.spearOnBack.HasASpear) &&
                              !(self.spearOnBack.HasASpear && self.onBack != null))))
        {
            playerModule.ForceLockSpearOnBack = true;

            if (playerModule.SpearDelay > 10)
            {
                playerModule.BlockInput = true;
                playerModule.SpearTimer++;
                self.Blink(5);

                if (playerModule.SpearTimer > spearCreationTime)
                {
                    playerModule.SpearTimer = 0;

                    var abstractSpear = new AbstractSpear(self.room.world, null,
                        self.room.GetWorldCoordinate(self.mainBodyChunk.pos), self.room.game.GetNewID(), false);
                    self.room.abstractRoom.AddEntity(abstractSpear);
                    abstractSpear.pos = self.abstractCreature.pos;
                    abstractSpear.RealizeInRoom();

                    var dataPearlType = (playerModule.ActiveObject as DataPearl.AbstractDataPearl)?.dataPearlType.value;


                    var spearModule = new SpearModule(playerModule.ActiveColor, dataPearlType ?? "");

                    if (playerModule.ActiveObject is PebblesPearl.AbstractPebblesPearl pebblesPearl)
                    {
                        spearModule.PebblesColor = pebblesPearl.color;
                    }


                    var save = self.abstractCreature.Room.world.game.GetMiscWorld();

                    // Story
                    if (save != null)
                    {
                        save.PearlSpears.Add(abstractSpear.ID.number, spearModule);
                    }
                    // Non-Story (e.g. Arena / Sandbox)
                    else
                    {
                        ModuleManager.TempPearlSpearData.Add(abstractSpear, spearModule);
                    }


                    if (self.spearOnBack != null && (holdingSpear || self.onBack != null))
                    {
                        self.spearOnBack.SpearToBack((Spear)abstractSpear.realizedObject);
                    }
                    else
                    {
                        self.SlugcatGrab(abstractSpear.realizedObject, self.FreeHand());
                    }

                    (playerModule.ActiveObject?.realizedObject).ConnectEffect(abstractSpear.realizedObject.firstChunk.pos);

                    self.room?.PlaySound(Enums.Sounds.Pearlcat_PearlStore, self.firstChunk, false, 0.5f,
                        Random.Range(2.2f, 2.5f));

                    if (playerModule.ActiveObject != null)
                    {
                        var activeObj = playerModule.ActiveObject;
                        self.RemoveFromInventory(playerModule.ActiveObject);

                        activeObj.destroyOnAbstraction = true;
                        activeObj.Abstractize(activeObj.pos);

                        self.UpdateInventorySaveData(playerModule);
                    }
                }
            }
            else
            {
                playerModule.SpearDelay++;
            }
        }
        else
        {
            if (playerModule.SpearTimer > spearCreationTime / 2.0f)
                self.room?.AddObject(new ShockWave(playerModule.ActiveObject!.realizedObject.firstChunk.pos, 30.0f,
                    0.5f, 6));

            playerModule.SpearTimer = 0;
            playerModule.SpearDelay = 0;
        }
    }

    public static void UpdateAgility(Player self, PlayerModule playerModule, PearlEffect effect)
    {
        if (playerModule.AgilityOveruseTimer > 0)
            playerModule.AgilityOveruseTimer--;

        if (ModOptions.DisableAgility.Value || self.inVoidSea || playerModule.PossessedCreature != null)
        {
            playerModule.DisabledEffects.Add(PearlEffect.MajorEffectType.AGILITY);
            return;
        }

        var maxOveruse = playerModule.ActiveObject?.GetPOEffect().MajorEffect == PearlEffect.MajorEffectType.AGILITY
            ? 180
            : 120;

        var velocityMult = Custom.LerpMap(playerModule.AgilityCount, 1, 5, 1.0f, 0.75f);
        velocityMult *= Custom.LerpMap(playerModule.AgilityOveruseTimer, 40, maxOveruse, 1.0f, 0.7f);
        //velocityMult *= playerModule.ActiveObject?.GetPOEffect().MajorEffect == MajorEffectType.AGILITY ? 1.25f : 1.0f;

        var abilityInput = self.IsAgilityKeybindPressed(playerModule);
        var wasAbilityInput = playerModule.WasAgilityInput;

        var canUseAbility = playerModule.AgilityCount > 0 && playerModule.AgilityOveruseTimer < maxOveruse
                                                          && self.canJump <= 0 &&
                                                          !(self.eatMeat >= 20 || self.maulTimer >= 15)
                                                          && self.Consious &&
                                                          self.bodyMode != Player.BodyModeIndex.Crawl
                                                          && self.bodyMode != Player.BodyModeIndex.CorridorClimb &&
                                                          self.bodyMode != Player.BodyModeIndex.ClimbIntoShortCut
                                                          && self.animation != Player.AnimationIndex.HangFromBeam &&
                                                          self.animation != Player.AnimationIndex.ClimbOnBeam
                                                          && self.bodyMode != Player.BodyModeIndex.WallClimb
                                                          && self.animation != Player.AnimationIndex.AntlerClimb &&
                                                          self.animation != Player.AnimationIndex.VineGrab
                                                          && self.animation != Player.AnimationIndex.ZeroGPoleGrab &&
                                                          self.onBack == null;

        if (abilityInput && !wasAbilityInput && canUseAbility)
        {
            var agilityObject = playerModule.SetAgilityCooldown(-1);

            self.noGrabCounter = 5;
            var pos = self.firstChunk.pos;

            self.room?.AddObject(new Explosion.ExplosionLight(pos, 160f, 1f, 3, Color.white));

            for (var j = 0; j < 10; j++)
            {
                var randVec = Custom.RNV();
                self.room?.AddObject(new Spark(pos + randVec * Random.value * 40f,
                    randVec * Mathf.Lerp(4f, 30f, Random.value), Color.white, null, 4, 18));
            }

            self.room?.PlaySound(SoundID.Fire_Spear_Explode, pos, 0.15f + Random.value * 0.15f, 0.5f + Random.value * 2f);


            if (self.bodyMode == Player.BodyModeIndex.ZeroG || self.room?.gravity == 0f || self.gravity == 0f ||
                self.bodyMode == Player.BodyModeIndex.Swimming)
            {
                float inputX = self.input[0].x;
                float randVariation = self.input[0].y;

                while (inputX == 0f && randVariation == 0f)
                {
                    inputX = (Random.value <= 0.33) ? 0 : ((Random.value <= 0.5) ? 1 : -1);
                    randVariation = (Random.value <= 0.33) ? 0 : ((Random.value <= 0.5) ? 1 : -1);
                }

                self.bodyChunks[0].vel.x = 9f * inputX * velocityMult;
                self.bodyChunks[0].vel.y = 9f * randVariation * velocityMult;
                self.bodyChunks[1].vel.x = 8f * inputX * velocityMult;
                self.bodyChunks[1].vel.y = 8f * randVariation * velocityMult;
            }
            else
            {
                if (self.input[0].x != 0)
                {
                    self.bodyChunks[0].vel.y = Mathf.Min(self.bodyChunks[0].vel.y, 0f) + 8f * velocityMult;
                    self.bodyChunks[1].vel.y = Mathf.Min(self.bodyChunks[1].vel.y, 0f) + 7f * velocityMult;
                    self.jumpBoost = 6f;
                }

                if (self.input[0].x == 0 || self.input[0].y == 1)
                {
                    self.bodyChunks[0].vel.y = 16f * velocityMult;
                    self.bodyChunks[1].vel.y = 15f * velocityMult;
                    self.jumpBoost = 8f;
                }

                if (self.input[0].y == 1)
                {
                    self.bodyChunks[0].vel.x = 10f * self.input[0].x * velocityMult;
                    self.bodyChunks[1].vel.x = 8f * self.input[0].x * velocityMult;
                }
                else
                {
                    self.bodyChunks[0].vel.x = 15f * self.input[0].x;
                    self.bodyChunks[1].vel.x = 13f * self.input[0].x;
                }

                self.animation = Player.AnimationIndex.Flip;
                self.bodyMode = Player.BodyModeIndex.Default;
            }

            var targetPos = self.firstChunk.pos + self.firstChunk.vel * -10.0f;

            if (agilityObject != null)
            {
                self.ConnectEffect(targetPos, agilityObject.GetObjectColor());
            }

            playerModule.AgilityOveruseTimer += (int)Custom.LerpMap(playerModule.AgilityOveruseTimer, 0, 80, 40, 60);
        }

        var isAnim =
            self.animation == Player.AnimationIndex.HangFromBeam || self.animation == Player.AnimationIndex.ClimbOnBeam
                                                                 || self.bodyMode == Player.BodyModeIndex.WallClimb ||
                                                                 self.animation == Player.AnimationIndex.AntlerClimb
                                                                 || self.animation == Player.AnimationIndex.VineGrab ||
                                                                 self.animation == Player.AnimationIndex.ZeroGPoleGrab
                                                                 || self.bodyMode == Player.BodyModeIndex.Swimming;

        // FREAKING NULL REF
        if (isAnim || self.canJump > 0 || !self.Consious || self.Stunned
            || ((self.bodyMode == Player.BodyModeIndex.ZeroG)
                && (self.wantToJump == 0 || !self.input[0].pckp)))
        {
            playerModule.ResetAgilityCooldown(30);
        }

        var overuse = playerModule.AgilityOveruseTimer;
        var overuseDisplayCount = overuse < 20 ? 0 : (int)Custom.LerpMap(overuse, 20, maxOveruse, 1, 5, 1.5f);

        for (var i = 0; i < overuseDisplayCount; i++)
        {
            if (Random.value < 0.25f)
            {
                self.room?.AddObject(new Explosion.ExplosionSmoke(self.mainBodyChunk.pos, Custom.RNV() * 2f * Random.value, 1f));
            }

            if (Random.value < 0.5f)
            {
                self.room?.AddObject(new Spark(self.mainBodyChunk.pos, Custom.RNV(), Color.white, null, 4, 8));
            }

            if (overuse > 90 && Random.value < 0.03f)
            {
                self.ConnectEffect(self.mainBodyChunk.pos + Custom.RNV() * 80.0f, playerModule.ActiveColor);
            }
        }

        if (overuse > maxOveruse && !self.Stunned)
        {
            self.room?.PlaySound(SoundID.Fire_Spear_Explode, self.mainBodyChunk.pos, 0.3f + Random.value * 0.15f, 0.25f + Random.value * 1.5f);
            self.Stun(60);
        }
    }

    public static void UpdateRevive(Player self, PlayerModule playerModule, PearlEffect effect)
    {
        if (ModOptions.DisableRevive.Value || self.inVoidSea)
        {
            playerModule.DisabledEffects.Add(PearlEffect.MajorEffectType.REVIVE);
            return;
        }

        if (playerModule.ActiveObject == null ||
            !playerModule.ActiveObject.TryGetPlayerPearlModule(out var poModule)) return;

        var abilityInput = self.IsReviveKeybindPressed(playerModule);

        if (effect.MajorEffect != PearlEffect.MajorEffectType.REVIVE || !abilityInput)
        {
            playerModule.ReviveTimer = 0;
            return;
        }

        if (poModule.CooldownTimer != 0) return;

        var shouldResetRevive = true;

        foreach (var grasp in self.grasps)
        {
            if (grasp?.grabbed is not Creature creature) continue;

            // maybe i dunno
            if (!creature.dead && !creature.State.dead && !(creature is Player deadPlayer &&
                                                            (deadPlayer.playerState.dead ||
                                                             deadPlayer.playerState.permaDead))) continue;

            self.Blink(5);

            if (playerModule.ReviveTimer % 3 == 0 && !poModule.IsReturningSentry)
            {
                playerModule.ActiveObject.realizedObject.ConnectEffect(creature.firstChunk.pos);
            }

            if (playerModule.ReviveTimer > 100)
            {
                playerModule.SetReviveCooldown(-1);

                if (creature is Player player)
                {
                    player.RevivePlayer();
                }
                else
                {
                    creature.Revive();

                    if (playerModule.PlayerRef.TryGetTarget(out player) && creature.killTag != player.abstractCreature)
                    {
                        creature.abstractCreature.world.game.session.creatureCommunities.InfluenceLikeOfPlayer(
                            creature.abstractCreature.creatureTemplate.communityID,
                            creature.abstractCreature.world.RegionNumber,
                            playerModule.PlayerNumber,
                            1.0f, 0.0f, 0.0f);
                    }
                }
            }

            shouldResetRevive = false;
            playerModule.BlockInput = true;
            break;
        }

        if (shouldResetRevive)
        {
            playerModule.ReviveTimer = 0;
        }
        else
        {
            poModule.RemoveSentry(playerModule.ActiveObject);

            if (!poModule.IsReturningSentry)
            {
                playerModule.ReviveTimer++;
            }
        }
    }

    public static void UpdateShield(Player self, PlayerModule playerModule, PearlEffect effect)
    {
        if (playerModule.ShieldTimer > 0)
        {
            self.AllGraspsLetGoOfThisObject(false);
            playerModule.ShieldTimer--;

            self.airInLungs = 1.0f;

            playerModule.ShieldAlpha = Mathf.Lerp(playerModule.ShieldAlpha, 1.0f, 0.25f);
            playerModule.ShieldScale = Mathf.Lerp(playerModule.ShieldScale, 6.0f, 0.4f);

            if (playerModule.ShieldTimer % 3 == 0)
            {
                for (var i = 0; i < playerModule.Inventory.Count; i++)
                {
                    var item = playerModule.Inventory[i];

                    if (i >= PlayerPearl_Helpers.MaxPearlsWithEffects) break;

                    if (ModOptions.HidePearls.Value)
                    {
                        if (item != playerModule.ActiveObject) continue;
                    }

                    var itemEffect = item.GetPOEffect();

                    if (!item.TryGetPlayerPearlModule(out var module)) continue;

                    if (module.CooldownTimer != 0) continue;

                    if (itemEffect.MajorEffect == PearlEffect.MajorEffectType.SHIELD && !item.TryGetSentry(out _))
                    {
                        item.realizedObject.ConnectEffect(self.firstChunk.pos);
                    }
                }
            }

            if (playerModule.ShieldTimer == 0)
                self.room?.PlaySound(Enums.Sounds.Pearlcat_ShieldOff, self.firstChunk);
        }
        else
        {
            playerModule.ShieldAlpha = Mathf.Lerp(playerModule.ShieldAlpha, 0.0f, 0.25f);
            playerModule.ShieldScale = Mathf.Lerp(playerModule.ShieldScale, 0.0f, 0.4f);
        }

        if (self.airInLungs < 0.1f && playerModule.ShieldActive)
        {
            playerModule.ActivateVisualShield();
        }

        if (self.room == null) return;

        var roomObjects = self.room.updateList;
        var shouldActivate = false;

        if (ModOptions.DisableShield.Value || self.inVoidSea)
        {
            playerModule.DisabledEffects.Add(PearlEffect.MajorEffectType.SHIELD);
            return;
        }

        if (playerModule.ShieldActive)
        {
            for (var i = roomObjects.Count - 1; i >= 0; i--)
            {
                var obj = roomObjects[i];

                if (obj is Weapon weapon)
                {
                    if (weapon.thrownBy == self) continue;

                    // Thrown by another player
                    if (weapon.thrownBy is Player playerThrownBy)
                    {
                        // Thrown by a player we are on the back of
                        if (playerThrownBy.onBack == self) continue;

                        // Jolly FF is off, doesn't apply to arena sessions
                        if (!self.abstractCreature.world.game.IsArenaSession && !Utils.RainWorld.options.friendlyFire)
                            continue;

                        // Arena FF is off, only applies to arena sessions
                        if (self.abstractCreature.world.game.IsArenaSession && !self.abstractCreature.world.game
                                .GetArenaGameSession.GameTypeSetup.spearsHitPlayers) continue;
                    }


                    // When posessing a creature don't let the spears activate our shield (only relevant for posessing scavs really)
                    if (playerModule.PossessedCreature?.TryGetTarget(out var possessed) == true &&
                        possessed.realizedCreature == weapon.thrownBy) continue;


                    if (weapon.mode == Weapon.Mode.Thrown &&
                        Custom.DistLess(weapon.firstChunk.pos, self.firstChunk.pos, 75.0f))
                    {
                        weapon.ChangeMode(Weapon.Mode.Free);
                        weapon.SetRandomSpin();
                        weapon.firstChunk.vel *= -0.2f;

                        weapon.room.DeflectEffect(weapon.firstChunk.pos);
                        shouldActivate = true;
                    }
                }
                else if (obj is LizardSpit spit)
                {
                    if (playerModule.ShieldTimer > 0 && Custom.DistLess(spit.pos, self.firstChunk.pos, 75.0f))
                    {
                        spit.vel = Vector2.zero;

                        if (playerModule.ShieldTimer <= 0)
                        {
                            spit.room.DeflectEffect(spit.pos);
                        }
                    }
                }
                else if (obj is DartMaggot dart)
                {
                    if (dart.mode != DartMaggot.Mode.Free)
                    {
                        if (Custom.DistLess(dart.firstChunk.pos, self.firstChunk.pos, 75.0f))
                        {
                            dart.mode = DartMaggot.Mode.Free;
                            dart.firstChunk.vel = Vector2.zero;

                            dart.room.DeflectEffect(dart.firstChunk.pos);
                            shouldActivate = true;
                        }
                    }
                }
            }
        }

        if (shouldActivate)
        {
            playerModule.ActivateVisualShield();
        }
    }

    public static void UpdateCamoflague(Player self, PlayerModule playerModule, PearlEffect effect)
    {
        if (effect.MajorEffect != PearlEffect.MajorEffectType.CAMOFLAGUE || playerModule.ActiveObject == null ||
            playerModule.ActiveObject.TryGetSentry(out _))
        {
            // Give these creatures night vision by default
            if (playerModule.PossessedCreature?.TryGetTarget(out var creature) == true &&
                self.room?.Darkness(self.mainBodyChunk.pos) >= 0.75f)
            {
                var nightVisionCreatures = new List<CreatureTemplate.Type>()
                {
                    CreatureTemplate.Type.BlackLizard,
                    CreatureTemplate.Type.LanternMouse,
                    CreatureTemplate.Type.Spider,
                    CreatureTemplate.Type.BigSpider,
                    CreatureTemplate.Type.SpitterSpider,
                    CreatureTemplate.Type.DaddyLongLegs,
                    CreatureTemplate.Type.BrotherLongLegs,
                    CreatureTemplate.Type.Centipede,
                    CreatureTemplate.Type.Centiwing,
                    CreatureTemplate.Type.RedCentipede,
                    CreatureTemplate.Type.SmallCentipede,
                    CreatureTemplate.Type.Overseer,
                    CreatureTemplate.Type.MirosBird,

                    MoreSlugcatsEnums.CreatureTemplateType.AquaCenti,
                    MoreSlugcatsEnums.CreatureTemplateType.Inspector,
                    MoreSlugcatsEnums.CreatureTemplateType.MotherSpider,
                    MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs,
                    MoreSlugcatsEnums.CreatureTemplateType.MirosVulture,
                };

                if (nightVisionCreatures.Contains(creature.creatureTemplate.type))
                {
                    playerModule.HoloLightScale = Mathf.Lerp(playerModule.HoloLightScale, 100.0f, 0.1f);
                }
            }
            else
            {
                playerModule.HoloLightScale = Mathf.Lerp(playerModule.HoloLightScale, 0.0f, 0.2f);
            }
        }

        if (ModOptions.DisableCamoflague.Value || self.inVoidSea)
        {
            playerModule.DisabledEffects.Add(PearlEffect.MajorEffectType.CAMOFLAGUE);
            return;
        }

        var camera = self.abstractCreature.world.game.cameras[0];

        var camoSpeed = Custom.LerpMap(playerModule.CamoCount, 1, 5, 0.001f, 0.01f);
        var camoMaxMoveSpeed = Custom.LerpMap(playerModule.CamoCount, 1, 5, 2.0f, 10.0f);

        var shouldCamo = (((self.canJump > 0 || self.bodyMode == Player.BodyModeIndex.ClimbingOnBeam ||
                            self.bodyMode == Player.BodyModeIndex.CorridorClimb)
                           && self.firstChunk.vel.magnitude < camoMaxMoveSpeed) ||
                          self.bodyMode == Player.BodyModeIndex.Crawl)
                         && effect.MajorEffect == PearlEffect.MajorEffectType.CAMOFLAGUE &&
                         playerModule.StoreObjectTimer <= 0 && playerModule.CamoCount > 0;

        // LAG CAUSER
        if (shouldCamo || playerModule.BodyColor != playerModule.BaseBodyColor)
        {
            var samples = new List<Color>()
            {
                camera.PixelColorAtCoordinate(self.firstChunk.pos),

                camera.PixelColorAtCoordinate(self.firstChunk.pos + new Vector2(-10.0f, 0.0f)),
                camera.PixelColorAtCoordinate(self.firstChunk.pos + new Vector2(10.0f, 0.0f)),
            };

            var totalColor = Color.black;

            foreach (var color in samples)
            {
                totalColor += color;
            }

            playerModule.CamoColor = totalColor / samples.Count;
        }


        playerModule.CamoLerp = shouldCamo
            ? Custom.LerpAndTick(playerModule.CamoLerp, 1.0f, 0.1f, camoSpeed)
            : Custom.LerpAndTick(playerModule.CamoLerp, 0.0f, 0.1f, camoSpeed);

        if (effect.MajorEffect == PearlEffect.MajorEffectType.CAMOFLAGUE && playerModule.CamoCount > 0 &&
            self.room?.Darkness(self.mainBodyChunk.pos) >= 0.75f)
        {
            var targetScale = Custom.LerpMap(playerModule.CamoCount, 1, 5, 40.0f, 150.0f);
            playerModule.HoloLightScale = Mathf.Lerp(playerModule.HoloLightScale, targetScale, 0.1f);
        }
    }
}
