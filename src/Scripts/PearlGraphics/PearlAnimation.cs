﻿using MoreSlugcats;
using RWCustom;
using System.Collections.Generic;
using UnityEngine;
using Color = UnityEngine.Color;

namespace Pearlcat;

public abstract class PearlAnimation
{
    public int AnimTimer { get; set; }

    public List<float> HaloEffectStackers { get; set; } = [];
    public float HaloEffectFrameAddition { get; set; } = 0.02f;
    public float HaloEffectDir { get; set; } = 1;

    public PearlAnimation(Player player)
    {
        InitAnimation(player);
    }

    public void InitAnimation(Player self)
    {
        if (!self.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        HaloEffectStackers.Clear();

        for (var i = 0; i < playerModule.Inventory.Count; i++)
        {
            HaloEffectStackers.Add((1.0f / playerModule.Inventory.Count) * i);
        }
    }


    public virtual void Update(Player player)
    {
        if (!player.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }


        foreach (var abstractObject in playerModule.Inventory)
        {
            if (abstractObject.realizedObject == null)
            {
                continue;
            }

            if (!abstractObject.TryGetPlayerPearlModule(out var module))
            {
                continue;
            }

            if (!abstractObject.TryGetPearlGraphicsModule(out _))
            {
                _ = new PearlGraphics(abstractObject);
            }

            module.PlayCollisionSound = false;
        }

        AnimTimer++;

        UpdateHaloEffects(player);
        UpdateSymbolEffects(player);
    }

    public void UpdateHaloEffects(Player player)
    {
        if (!player.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }


        for (var i = 0; i < playerModule.Inventory.Count; i++)
        {
            var abstractObject = playerModule.Inventory[i];

            if (abstractObject.realizedObject == null)
            {
                continue;
            }

            if (!abstractObject.TryGetPearlGraphicsModule(out var addon))
            {
                continue;
            }


            addon.DrawHalo = true;
            var haloEffectTimer = HaloEffectStackers[i];

            addon.ActiveHaloColor = abstractObject.GetObjectColor() * new Color(1.0f, 0.25f, 0.25f);

            // CW Pearl unique halo color
            if (abstractObject is DataPearl.AbstractDataPearl dataPearl &&
                dataPearl.dataPearlType == Enums.Pearls.CW_Pearlcat)
            {
                addon.ActiveHaloColor = Custom.hexToColor("bf934d");
            }

            if (i == playerModule.ActiveObjectIndex)
            {
                addon.HaloColor = addon.ActiveHaloColor;
                addon.HaloScale = 1.0f + 0.45f * haloEffectTimer;
                addon.HaloAlpha = 0.8f;
            }
            else
            {
                addon.HaloColor = abstractObject.GetObjectColor() * new Color(0.25f, 0.25f, 1.0f);
                addon.HaloScale = 0.3f + 0.45f * haloEffectTimer;
                addon.HaloAlpha = ModOptions.HidePearls.Value && !abstractObject.IsHeartPearl() ? 0.0f : 0.6f;
            }



            if (haloEffectTimer < 0.0f)
            {
                HaloEffectDir = 1;
            }
            else if (haloEffectTimer > 1.0f)
            {
                HaloEffectDir = -1;
            }

            HaloEffectStackers[i] += HaloEffectDir * HaloEffectFrameAddition;
        }
    }

    public void UpdateSymbolEffects(Player player)
    {
        if (!player.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        for (var i = 0; i < playerModule.Inventory.Count; i++)
        {
            var abstractObject = playerModule.Inventory[i];

            if (abstractObject.realizedObject == null)
            {
                continue;
            }

            if (!abstractObject.TryGetPearlGraphicsModule(out var addon))
            {
                continue;
            }

            if (!abstractObject.TryGetPlayerPearlModule(out var pearlModule))
            {
                continue;
            }

            if (player.room == null || addon.Pos == Vector2.zero)
            {
                addon.IsVisible = false;
                continue;
            }

            addon.IsVisible = true;


            var effect = abstractObject.GetPearlEffect();

            addon.IsActiveObject = i == playerModule.ActiveObjectIndex;
            addon.SymbolColor = effect.MajorEffect == PearlEffect.MajorEffectType.CAMOFLAGUE ? Color.white : abstractObject.GetObjectColor();

            if (pearlModule.CooldownTimer != 0)
            {
                if (pearlModule.CooldownTimer == 1)
                {
                    abstractObject.realizedObject.room.AddObject(new ShockWave(abstractObject.realizedObject.firstChunk.pos, 10.0f, 1.0f, 5, true));
                }

                addon.DrawSymbolCooldown = true;

                var cooldownLerp = pearlModule.CooldownTimer < 0 ? 1.0f : Custom.LerpMap(pearlModule.CooldownTimer, pearlModule.CurrentCooldownTime / 2.0f, 0.0f, 1.0f, 0.0f);
                var cooldownColor = effect.MajorEffect == PearlEffect.MajorEffectType.RAGE ? Color.white : (Color)new Color32(189, 13, 0, 255);

                addon.SymbolColor = Color.Lerp(Color.Lerp(addon.SymbolColor, cooldownColor, 0.4f), cooldownColor, cooldownLerp);
            }
            else
            {
                addon.DrawSymbolCooldown = false;
            }

            if (playerModule.DisabledEffects.Contains(effect.MajorEffect))
            {
                addon.DrawSymbolCooldown = true;
                addon.SymbolColor = Color.white;
            }

            if (ModOptions.HidePearls.Value && !addon.IsActiveObject)
            {
                addon.DrawSymbolCooldown = false;
            }


            addon.Symbol = PearlGraphics.SpriteFromPearl(abstractObject);
            addon.SymbolAlpha = addon.IsActiveObject ? Mathf.Lerp(addon.SymbolAlpha, 1.0f, 0.05f) : Mathf.Lerp(addon.SymbolAlpha, 0.0f, 0.05f);

            addon.CamoLerp = ModOptions.HidePearls.Value && !addon.IsActiveObject && !abstractObject.IsHeartPearl() && !addon.IsActiveRagePearl ? 1.0f : playerModule.CamoLerp;

            if ((!ModOptions.HidePearls.Value || addon.IsActiveObject) && effect.MajorEffect == PearlEffect.MajorEffectType.CAMOFLAGUE)
            {
                addon.CamoLerp = 0.0f;
            }

            addon.DrawSpearLerp = playerModule.SpearLerp;

            addon.ShieldCounter = playerModule.ShieldCount;
            addon.ReviveCounter = playerModule.ReviveCount;



            addon.IsSentry = pearlModule.IsSentry || pearlModule.IsReturningSentry;

            if (addon.IsSentry || addon.IsActiveRagePearl)
            {
                if (addon.IsSentry)
                {
                    addon.CamoLerp = 0.0f;
                    addon.Symbol = "pearlcat_glyphsentry";
                }

                // gross code
                if (PlayerPearl_Helpers.TargetPositions.TryGetValue(abstractObject, out var targetPos))
                {
                    addon.OverridePos ??= abstractObject.realizedObject.firstChunk.pos;
                    addon.OverrideLastPos = addon.OverridePos;

                    addon.OverridePos = Vector2.Lerp((Vector2)addon.OverridePos, addon.IsActiveObject ? player.GetActivePearlPos(timeStacker: 1.0f) : targetPos.Value, 0.7f);

                    if (addon.OverrideLastPos != null && !Custom.DistLess((Vector2)addon.OverridePos, (Vector2)addon.OverrideLastPos, 100.0f))
                    {
                        addon.OverrideLastPos = addon.OverridePos;
                    }
                }
            }
            else
            {
                addon.OverridePos = null;
                addon.OverrideLastPos = null;
            }

            var returnPos = PlayerPearl_Helpers.TargetPositions.TryGetValue(abstractObject, out var pos) ? pos.Value : player.GetActivePearlPos();
            var returned = Custom.DistLess(abstractObject.realizedObject.firstChunk.pos, returnPos, 8.0f);


            if (addon.IsActiveRagePearl)
            {
                returnPos = player.firstChunk.pos;
                returned = Custom.DistLess(abstractObject.realizedObject.firstChunk.pos, returnPos, 80.0f);
            }

            if (pearlModule.IsReturningSentry && returned)
            {
                abstractObject.realizedObject.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, abstractObject.realizedObject.firstChunk.pos, 0.5f, 3.0f);
                abstractObject.realizedObject.room.AddObject(new LightningMachine.Impact(abstractObject.realizedObject.firstChunk.pos, 0.1f, addon.SymbolColor, true));

                pearlModule.IsReturningSentry = false;
            }

            var hasTarget = false;

            if (!addon.IsSentry && playerModule.RageTarget?.TryGetTarget(out var target) == true)
            {
                hasTarget = true;
                addon.LaserTarget = target.mainBodyChunk.pos;
            }

            if (ModOptions.OldRedPearlAbility.Value)
            {
                addon.IsLaserVisible = hasTarget && effect.MajorEffect == PearlEffect.MajorEffectType.RAGE && playerModule.ActiveObject?.GetPearlEffect().MajorEffect == PearlEffect.MajorEffectType.RAGE;
                addon.LaserLerp = pearlModule.LaserLerp;
            }
            else
            {
                addon.LaserLerp = Custom.LerpAndTick(addon.LaserLerp, 0.0f, 0.025f, 0.005f);
                addon.IsLaserVisible = addon.LaserLerp > 0.02f;
            }

            // CW Pearl is on second double jump
            if (pearlModule.IsCWDoubleJumpUsed && pearlModule.CooldownTimer == 0)
            {
                addon.SymbolColor = Custom.hexToColor("ffc800");
            }
        }
    }


    public void AnimateOrbit(Player player, Vector2 origin, float radius, float angleFrameAddition, List<AbstractPhysicalObject> abstractObjects)
    {
        for (var i = 0; i < abstractObjects.Count; i++)
        {
            var abstractObject = abstractObjects[i];

            var angle = (i * Mathf.PI * 2.0f / abstractObjects.Count) + angleFrameAddition * AnimTimer;

            Vector2 targetPos = new(origin.x + Mathf.Cos(angle) * radius, origin.y + Mathf.Sin(angle) * radius);
            abstractObject.TryToAnimateToTargetPos(player, targetPos);
        }
    }
}
