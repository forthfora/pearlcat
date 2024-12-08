using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Pearlcat;

public static class PlayerAbilities_Helpers_Agility
{
    public static void Update(Player self, PlayerModule playerModule, PearlEffect effect)
    {
        if (playerModule.AgilityOveruseTimer > 0)
        {
            playerModule.AgilityOveruseTimer--;
        }

        if (ModOptions.DisableAgility.Value || self.inVoidSea || playerModule.PossessedCreature != null)
        {
            playerModule.DisabledEffects.Add(PearlEffect.MajorEffectType.Agility);
            return;
        }

        var maxOveruse = playerModule.ActiveObject?.GetPearlEffect().MajorEffect == PearlEffect.MajorEffectType.Agility
            ? 180
            : 120;

        var velocityMult = Custom.LerpMap(playerModule.AgilityCount, 1, 5, 1.0f, 0.75f);
        velocityMult *= Custom.LerpMap(playerModule.AgilityOveruseTimer, 40, maxOveruse, 1.0f, 0.7f);
        //velocityMult *= playerModule.ActiveObject?.GetPearlEffect().MajorEffect == MajorEffectType.AGILITY ? 1.25f : 1.0f;

        var abilityInput = self.IsAgilityKeybindPressed(playerModule);
        var wasAbilityInput = playerModule.WasAgilityInput;

        var canUseAbility = playerModule.AgilityCount > 0 && playerModule.AgilityOveruseTimer < maxOveruse &&
                            self.canJump <= 0 && !(self.eatMeat >= 20 || self.maulTimer >= 15) && self.Consious &&
                            self.bodyMode != Player.BodyModeIndex.Crawl &&
                            self.bodyMode != Player.BodyModeIndex.CorridorClimb &&
                            self.bodyMode != Player.BodyModeIndex.ClimbIntoShortCut &&
                            self.animation != Player.AnimationIndex.HangFromBeam &&
                            self.animation != Player.AnimationIndex.ClimbOnBeam &&
                            self.bodyMode != Player.BodyModeIndex.WallClimb &&
                            self.animation != Player.AnimationIndex.AntlerClimb &&
                            self.animation != Player.AnimationIndex.VineGrab &&
                            self.animation != Player.AnimationIndex.ZeroGPoleGrab && self.onBack == null;

        if (abilityInput && !wasAbilityInput && canUseAbility)
        {
            var agilityObject = playerModule.SetAgilityCooldown(-1);
            var isCWPearl = agilityObject is DataPearl.AbstractDataPearl dataPearl && dataPearl.dataPearlType == Enums.Pearls.CW_Pearlcat;

            if (isCWPearl)
            {
                if (agilityObject?.TryGetPlayerPearlModule(out var playerPearlModule) == true && !playerPearlModule.IsCWDoubleJumpUsed)
                {
                    playerPearlModule.CooldownTimer = 0;
                    playerPearlModule.IsCWDoubleJumpUsed = true;
                }
            }

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
                if (isCWPearl && agilityObject.TryGetPlayerPearlModule(out var playerPearlModule) && playerPearlModule.CooldownTimer == -1)
                {
                    var sulfurColor = Custom.hexToColor("ffc800");

                    self.ConnectEffect(targetPos, sulfurColor);

                    if (self.room is not null)
                    {
                        for (var i = 0; i < 5; i++)
                        {
                            self.room.AddObject(new SingularityBomb.SparkFlash(Vector2.Lerp(self.firstChunk.pos, targetPos, 0.5f) + Random.Range(10.0f, 60.0f) * Custom.RNV(), Random.Range(0.05f, 0.2f), sulfurColor) { lifeTime = Random.Range(3, 12) });
                        }

                        self.room.PlaySound(SoundID.Firecracker_Burn, self.firstChunk.pos, 0.2f, Random.Range(1.5f, 2.0f));
                    }
                }
                else
                {
                    self.ConnectEffect(targetPos, agilityObject.GetObjectColor());
                }
            }

            playerModule.AgilityOveruseTimer += (int)Custom.LerpMap(playerModule.AgilityOveruseTimer, 0, 80, 40, 60);
        }

        var animWhichResetsCooldown = self.animation == Player.AnimationIndex.HangFromBeam || self.animation == Player.AnimationIndex.ClimbOnBeam
                                                                          || self.bodyMode == Player.BodyModeIndex.WallClimb ||
                                                                          self.animation == Player.AnimationIndex.AntlerClimb
                                                                          || self.animation == Player.AnimationIndex.VineGrab ||
                                                                          self.animation == Player.AnimationIndex.ZeroGPoleGrab
                                                                          || self.bodyMode == Player.BodyModeIndex.Swimming;

        // FREAKING NULL REF
        // (self.bodyMode == Player.BodyModeIndex.ZeroG) && (self.wantToJump == 0 || !self.input[0].pckp) <- gives unlimited jumps in zero-G, but not sure that's actually a good idea...
        if (animWhichResetsCooldown || self.canJump > 0 || !self.Consious || self.Stunned)
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
}
