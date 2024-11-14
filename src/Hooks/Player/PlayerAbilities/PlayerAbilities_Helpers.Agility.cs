using RWCustom;
using UnityEngine;

namespace Pearlcat;

public static partial class PlayerAbilities_Helpers
{
    public static void UpdateAgility(Player self, PlayerModule playerModule, PearlEffect effect)
    {
        if (playerModule.AgilityOveruseTimer > 0)
            playerModule.AgilityOveruseTimer--;

        if (ModOptions.DisableAgility.Value || self.inVoidSea || playerModule.PossessedCreature != null)
        {
            playerModule.DisabledEffects.Add(PearlEffect.MajorEffectType.AGILITY);
            return;
        }

        var maxOveruse = playerModule.ActiveObject?.GetPearlEffect().MajorEffect == PearlEffect.MajorEffectType.AGILITY
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
        if (isAnim || self.canJump > 0 || !self.Consious || self.Stunned ||
            ((self.bodyMode == Player.BodyModeIndex.ZeroG) && (self.wantToJump == 0 || !self.input[0].pckp)))
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
