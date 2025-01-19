using MoreSlugcats;
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
            if (abstractObject.realizedObject is null)
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

            if (abstractObject.realizedObject is null)
            {
                continue;
            }

            if (!abstractObject.TryGetPearlGraphicsModule(out var pearlGraphics))
            {
                continue;
            }

            if (i >= HaloEffectStackers.Count) // Happens in meadow, not sure why though
            {
                break;
            }

            pearlGraphics.DrawHalo = true;
            var haloEffectTimer = HaloEffectStackers[i];

            pearlGraphics.ActiveHaloColor = abstractObject.GetObjectColor() * new Color(1.0f, 0.25f, 0.25f);

            // CW Pearl unique halo color
            if (abstractObject is DataPearl.AbstractDataPearl dataPearl &&
                dataPearl.dataPearlType == Enums.Pearls.CW_Pearlcat)
            {
                pearlGraphics.ActiveHaloColor = Custom.hexToColor("bf934d");
            }

            if (i == playerModule.ActivePearlIndex)
            {
                pearlGraphics.HaloColor = pearlGraphics.ActiveHaloColor;
                pearlGraphics.HaloScale = 1.0f + 0.45f * haloEffectTimer;
                pearlGraphics.HaloAlpha = 0.8f;
            }
            else
            {
                pearlGraphics.HaloColor = abstractObject.GetObjectColor() * new Color(0.25f, 0.25f, 1.0f);
                pearlGraphics.HaloScale = 0.3f + 0.45f * haloEffectTimer;
                pearlGraphics.HaloAlpha = ModOptions.HidePearls && !abstractObject.IsHeartPearl() ? 0.0f : 0.6f;
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

            if (abstractObject.realizedObject is null)
            {
                continue;
            }

            if (!abstractObject.TryGetPearlGraphicsModule(out var pearlGraphics))
            {
                continue;
            }

            if (!abstractObject.TryGetPlayerPearlModule(out var pearlModule))
            {
                continue;
            }

            if (player.room is null || pearlGraphics.Pos == Vector2.zero)
            {
                pearlGraphics.IsVisible = false;
                continue;
            }

            pearlGraphics.IsVisible = true;


            var effect = abstractObject.GetPearlEffect();

            pearlGraphics.IsActivePearl = i == playerModule.ActivePearlIndex;
            pearlGraphics.SymbolColor = effect.MajorEffect == PearlEffect.MajorEffectType.Camouflage ? Color.white : abstractObject.GetObjectColor();

            if (pearlModule.CooldownTimer != 0)
            {
                if (pearlModule.CooldownTimer == 1)
                {
                    abstractObject.realizedObject.room.AddObject(new ShockWave(abstractObject.realizedObject.firstChunk.pos, 10.0f, 1.0f, 5, true));
                }

                pearlGraphics.DrawSymbolCooldown = true;

                var cooldownLerp = pearlModule.CooldownTimer < 0 ? 1.0f : Custom.LerpMap(pearlModule.CooldownTimer, pearlModule.CurrentCooldownTime / 2.0f, 0.0f, 1.0f, 0.0f);
                var cooldownColor = effect.MajorEffect == PearlEffect.MajorEffectType.Rage ? Color.white : (Color)new Color32(189, 13, 0, 255);

                pearlGraphics.SymbolColor = Color.Lerp(Color.Lerp(pearlGraphics.SymbolColor, cooldownColor, 0.4f), cooldownColor, cooldownLerp);
            }
            else
            {
                pearlGraphics.DrawSymbolCooldown = false;
            }

            if (playerModule.DisabledEffects.Contains(effect.MajorEffect))
            {
                pearlGraphics.DrawSymbolCooldown = true;
                pearlGraphics.SymbolColor = Color.white;
            }

            if (ModOptions.HidePearls && !pearlGraphics.IsActivePearl)
            {
                pearlGraphics.DrawSymbolCooldown = false;
            }


            pearlGraphics.Symbol = PearlGraphics.SpriteFromPearl(abstractObject);
            pearlGraphics.SymbolAlpha = pearlGraphics.IsActivePearl ? Mathf.Lerp(pearlGraphics.SymbolAlpha, 1.0f, 0.05f) : Mathf.Lerp(pearlGraphics.SymbolAlpha, 0.0f, 0.05f);

            pearlGraphics.CamoLerp = ModOptions.HidePearls && !pearlGraphics.IsActivePearl && !abstractObject.IsHeartPearl() && !pearlGraphics.IsActiveRagePearl ? 1.0f : playerModule.CamoLerp;

            if ((!ModOptions.HidePearls || pearlGraphics.IsActivePearl) && effect.MajorEffect == PearlEffect.MajorEffectType.Camouflage)
            {
                pearlGraphics.CamoLerp = 0.0f;
            }

            pearlGraphics.DrawSpearLerp = playerModule.SpearLerp;

            pearlGraphics.ShieldCounter = playerModule.ShieldCount;
            pearlGraphics.ReviveCounter = playerModule.ReviveCount;



            pearlGraphics.IsSentry = pearlModule.IsSentry || pearlModule.IsReturningSentry;

            if (pearlGraphics.IsSentry || pearlGraphics.IsActiveRagePearl)
            {
                if (pearlGraphics.IsSentry)
                {
                    pearlGraphics.CamoLerp = 0.0f;
                    pearlGraphics.Symbol = "pearlcat_glyphsentry";
                }

                // gross code
                if (PlayerPearl_Helpers_Data.TargetPositions.TryGetValue(abstractObject, out var targetPos))
                {
                    pearlGraphics.OverridePos ??= abstractObject.realizedObject.firstChunk.pos;
                    pearlGraphics.OverrideLastPos = pearlGraphics.OverridePos;

                    pearlGraphics.OverridePos = Vector2.Lerp((Vector2)pearlGraphics.OverridePos, pearlGraphics.IsActivePearl ? player.GetActivePearlPos(timeStacker: 1.0f) : targetPos.Value, 0.7f);

                    if (pearlGraphics.OverrideLastPos is not null && !Custom.DistLess((Vector2)pearlGraphics.OverridePos, (Vector2)pearlGraphics.OverrideLastPos, 100.0f))
                    {
                        pearlGraphics.OverrideLastPos = pearlGraphics.OverridePos;
                    }
                }
            }
            else
            {
                pearlGraphics.OverridePos = null;
                pearlGraphics.OverrideLastPos = null;
            }

            var returnPos = PlayerPearl_Helpers_Data.TargetPositions.TryGetValue(abstractObject, out var pos) ? pos.Value : player.GetActivePearlPos();
            var returned = Custom.DistLess(abstractObject.realizedObject.firstChunk.pos, returnPos, 8.0f);


            if (pearlGraphics.IsActiveRagePearl)
            {
                returnPos = player.firstChunk.pos;
                returned = Custom.DistLess(abstractObject.realizedObject.firstChunk.pos, returnPos, 80.0f);
            }

            if (pearlModule.IsReturningSentry && returned)
            {
                abstractObject.realizedObject.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, abstractObject.realizedObject.firstChunk.pos, 0.5f, 3.0f);
                abstractObject.realizedObject.room.AddObject(new LightningMachine.Impact(abstractObject.realizedObject.firstChunk.pos, 0.1f, pearlGraphics.SymbolColor, true));

                pearlModule.IsReturningSentry = false;
            }

            var hasTarget = false;

            if (!pearlGraphics.IsSentry && playerModule.RageTarget?.TryGetTarget(out var target) == true)
            {
                hasTarget = true;
                pearlGraphics.LaserTarget = target.mainBodyChunk.pos;
            }

            if (ModOptions.OldRedPearlAbility)
            {
                pearlGraphics.IsLaserVisible = hasTarget && effect.MajorEffect == PearlEffect.MajorEffectType.Rage && playerModule.ActivePearl?.GetPearlEffect().MajorEffect == PearlEffect.MajorEffectType.Rage;
                pearlGraphics.LaserLerp = pearlModule.LaserLerp;
            }
            else
            {
                pearlGraphics.LaserLerp = Custom.LerpAndTick(pearlGraphics.LaserLerp, 0.0f, 0.025f, 0.005f);
                pearlGraphics.IsLaserVisible = pearlGraphics.LaserLerp > 0.02f;
            }

            // CW Pearl is on second double jump
            if (pearlModule.IsCWDoubleJumpUsed && pearlModule.CooldownTimer == 0)
            {
                pearlGraphics.SymbolColor = Custom.hexToColor("ffc800");
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
