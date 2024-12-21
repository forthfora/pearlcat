using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Pearlcat;

public static class PlayerAbilities_Helpers_Camouflage
{
    public static void Update(Player self, PlayerModule playerModule, PearlEffect effect)
    {
        var possessingNightVisionCreature = false;

        if (effect.MajorEffect != PearlEffect.MajorEffectType.Camouflage || playerModule.ActiveObject == null ||
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
                };

                if (ModManager.MSC)
                {
                    nightVisionCreatures.AddRange(new List<CreatureTemplate.Type>
                    {
                        MoreSlugcatsEnums.CreatureTemplateType.AquaCenti,
                        MoreSlugcatsEnums.CreatureTemplateType.Inspector,
                        MoreSlugcatsEnums.CreatureTemplateType.MotherSpider,
                        MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs,
                        MoreSlugcatsEnums.CreatureTemplateType.MirosVulture,
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

        if (ModOptions.DisableCamoflague.Value || self.inVoidSea)
        {
            playerModule.DisabledEffects.Add(PearlEffect.MajorEffectType.Camouflage);
            return;
        }

        var camera = self.abstractCreature.world.game.cameras[0];

        var camoSpeed = Custom.LerpMap(playerModule.CamoCount, 1, 5, 0.001f, 0.01f);
        var camoMaxMoveSpeed = Custom.LerpMap(playerModule.CamoCount, 1, 5, 2.0f, 10.0f);

        var shouldCamo = (((self.canJump > 0 || self.bodyMode == Player.BodyModeIndex.ClimbingOnBeam ||
                            self.bodyMode == Player.BodyModeIndex.CorridorClimb)
                           && self.firstChunk.vel.magnitude < camoMaxMoveSpeed) ||
                          self.bodyMode == Player.BodyModeIndex.Crawl)
                         && effect.MajorEffect == PearlEffect.MajorEffectType.Camouflage &&
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
