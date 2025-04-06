using MoreSlugcats;
using RWCustom;

namespace Pearlcat;

public static class PlayerAbilities_Helpers_Camouflage
{
    public static void Update(Player self, PlayerModule playerModule, PearlEffect effect)
    {
        var possessingNightVisionCreature = false;

        if (effect.MajorEffect != PearlEffect.MajorEffectType.Camouflage || playerModule.ActivePearl is null ||
            playerModule.ActivePearl.TryGetSentry(out _))
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
                };

                if (ModManager.MSC)
                {
                    nightVisionCreatures.AddRange(new List<CreatureTemplate.Type>
                    {
                        DLCSharedEnums.CreatureTemplateType.AquaCenti,
                        DLCSharedEnums.CreatureTemplateType.Inspector,
                        DLCSharedEnums.CreatureTemplateType.MotherSpider,
                        DLCSharedEnums.CreatureTemplateType.TerrorLongLegs,
                        DLCSharedEnums.CreatureTemplateType.MirosVulture,
                    });
                }

                if (nightVisionCreatures.Contains(creature.creatureTemplate.type))
                {
                    playerModule.HoloLightScale = Mathf.Lerp(playerModule.HoloLightScale, 100.0f, 0.1f);

                    possessingNightVisionCreature = true;
                }
            }
            else
            {
                playerModule.HoloLightScale = Mathf.Lerp(playerModule.HoloLightScale, 0.0f, 0.2f);
            }
        }

        if (ModOptions.DisableCamoflague || self.inVoidSea)
        {
            if (self.inVoidSea)
            {
                playerModule.HoloLightScale = 0.0f;
            }

            playerModule.DisabledEffects.Add(PearlEffect.MajorEffectType.Camouflage);
            return;
        }

        var camera = self.abstractCreature.world.game.cameras[0];

        var camoSpeed = Custom.LerpMap(playerModule.CamoCount, 1, 5, 0.001f, 0.01f);
        var camoMaxMoveSpeed = Custom.LerpMap(playerModule.CamoCount, 1, 5, 2.0f, 10.0f);

        var redPearlBlock = false;

        if (self.abstractPhysicalObject.world.game.IsFriendlyFireEnabled() && self.room is not null && !ModOptions.OldRedPearlAbility)
        {
            if (playerModule.ActivePearl?.GetPearlEffect().MajorEffect == PearlEffect.MajorEffectType.Rage)
            {
                redPearlBlock = true;
            }
            else
            {
                foreach (var obj in self.room.physicalObjects.SelectMany(x => x))
                {
                    if (!Custom.DistLess(obj.firstChunk.pos, self.firstChunk.pos, 75.0f))
                    {
                        continue;
                    }

                    if (!obj.abstractPhysicalObject.TryGetSentry(out var sentry))
                    {
                        continue;
                    }

                    if (!sentry.OwnerRef.TryGetTarget(out var owner))
                    {
                        continue;
                    }

                    if (owner.GetPearlEffect().MajorEffect != PearlEffect.MajorEffectType.Rage)
                    {
                        continue;
                    }

                    redPearlBlock = true;
                    break;
                }
            }

        }

        var camoMovementFlag = (((self.canJump > 0 || self.bodyMode == Player.BodyModeIndex.ClimbingOnBeam ||
                   self.bodyMode == Player.BodyModeIndex.CorridorClimb)
                  && self.firstChunk.vel.magnitude < camoMaxMoveSpeed) ||
                 self.bodyMode == Player.BodyModeIndex.Crawl);

        var shouldCamo = camoMovementFlag == !self.abstractPhysicalObject.world.game.IsArenaSession
                         && effect.MajorEffect == PearlEffect.MajorEffectType.Camouflage &&
                         playerModule.StoreObjectTimer <= 0 && playerModule.CamoCount > 0
                         && !redPearlBlock;

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

            playerModule.CamoColor = Color.Lerp(playerModule.CamoColor, totalColor / samples.Count, 0.15f);
        }


        playerModule.CamoLerp = shouldCamo
            ? Custom.LerpAndTick(playerModule.CamoLerp, 1.0f, 0.1f, camoSpeed)
            : Custom.LerpAndTick(playerModule.CamoLerp, 0.0f, 0.1f, camoSpeed);

        if (effect.MajorEffect == PearlEffect.MajorEffectType.Camouflage && playerModule.CamoCount > 0 &&
            self.room?.Darkness(self.mainBodyChunk.pos) >= 0.75f && !possessingNightVisionCreature)
        {
            var targetScale = Custom.LerpMap(playerModule.CamoCount, 1, 5, 40.0f, 150.0f);
            playerModule.HoloLightScale = Mathf.Lerp(playerModule.HoloLightScale, targetScale, 0.1f);
        }
    }
}
