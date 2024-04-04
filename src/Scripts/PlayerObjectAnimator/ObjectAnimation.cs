using MoreSlugcats;
using RWCustom;
using System.Collections.Generic;
using UnityEngine;
using Color = UnityEngine.Color;

namespace Pearlcat;

public abstract class ObjectAnimation
{
    public int AnimTimer { get; set; } = 0;


    public ObjectAnimation(Player player) => InitAnimation(player);

    public virtual void InitAnimation(Player self)
    {
        if (!self.TryGetPearlcatModule(out var playerModule)) return;

        HaloEffectStackers.Clear();

        for (int i = 0; i < playerModule.Inventory.Count; i++)
            HaloEffectStackers.Add((1.0f / playerModule.Inventory.Count) * i);
    }

    public virtual void Update(Player player)
    {
        if (!player.TryGetPearlcatModule(out var playerModule)) return;


        for (int i = 0; i < playerModule.Inventory.Count; i++)
        {
            var abstractObject = playerModule.Inventory[i];

            if (abstractObject.realizedObject == null) continue;

            if (!abstractObject.TryGetModule(out var module)) continue;

            if (!abstractObject.TryGetAddon(out _))
                new ObjectAddon(abstractObject);

            module.PlayCollisionSound = false;
        }

        AnimTimer++;

        UpdateHaloEffects(player);
        UpdateSymbolEffects(player);
    }



    public List<float> HaloEffectStackers { get; set; } = new();
    public float HaloEffectFrameAddition { get; set; } = 0.02f;
    public float HaloEffectDir { get; set; } = 1;   

    public virtual void UpdateHaloEffects(Player player)
    {
        if (!player.TryGetPearlcatModule(out var playerModule)) return;


        for (int i = 0; i < playerModule.Inventory.Count; i++)
        {
            var abstractObject = playerModule.Inventory[i];

            if (abstractObject.realizedObject == null) continue;

            if (!abstractObject.TryGetAddon(out var addon)) continue;
            

            addon.DrawHalo = true;
            var haloEffectTimer = HaloEffectStackers[i];

            addon.ActiveHaloColor = Hooks.GetObjectColor(abstractObject) * new Color(1.0f, 0.25f, 0.25f);

            if (i == playerModule.ActiveObjectIndex)
            {
                addon.HaloColor = addon.ActiveHaloColor;
                addon.HaloScale = 1.0f + 0.45f * haloEffectTimer;
                addon.HaloAlpha = 0.8f;
            }
            else
            {
                addon.HaloColor = Hooks.GetObjectColor(abstractObject) * new Color(0.25f, 0.25f, 1.0f);
                addon.HaloScale = 0.3f + 0.45f * haloEffectTimer;
                addon.HaloAlpha = ModOptions.HidePearls.Value ? 0.0f : 0.6f; 
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


    public virtual void UpdateSymbolEffects(Player player)
    {
        if (!player.TryGetPearlcatModule(out var playerModule)) return;

        for (int i = 0; i < playerModule.Inventory.Count; i++)
        {
            var abstractObject = playerModule.Inventory[i];

            if (abstractObject.realizedObject == null) continue;

            if (!abstractObject.TryGetAddon(out var addon)) continue;

            if (!abstractObject.TryGetModule(out var poModule)) continue;

            if (player.room == null || addon.Pos == Vector2.zero)
            {
                addon.AllVisible = false;
                continue;
            }

            addon.AllVisible = true;


            var effect = abstractObject.GetPOEffect();

            addon.IsActiveObject = i == playerModule.ActiveObjectIndex;
            addon.SymbolColor = effect.MajorEffect == POEffect.MajorEffectType.CAMOFLAGUE ? Color.white : Hooks.GetObjectColor(abstractObject);

            if (poModule.CooldownTimer != 0)
            {
                if (poModule.CooldownTimer == 1)
                    abstractObject.realizedObject.room.AddObject(new ShockWave(abstractObject.realizedObject.firstChunk.pos, 10.0f, 1.0f, 5, true));

                addon.DrawSymbolCooldown = true;

                var cooldownLerp = poModule.CooldownTimer < 0 ? 1.0f : Custom.LerpMap(poModule.CooldownTimer, poModule.CurrentCooldownTime / 2.0f, 0.0f, 1.0f, 0.0f);
                var cooldownColor = effect.MajorEffect == POEffect.MajorEffectType.RAGE ? Color.white : (Color)new Color32(189, 13, 0, 255);

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

            addon.Symbol = ObjectAddon.SpriteFromPearl(abstractObject);
            addon.SymbolAlpha = addon.IsActiveObject ? Mathf.Lerp(addon.SymbolAlpha, 1.0f, 0.05f) : Mathf.Lerp(addon.SymbolAlpha, 0.0f, 0.05f);

            addon.CamoLerp = ModOptions.HidePearls.Value && !addon.IsActiveObject ? 1.0f : playerModule.CamoLerp;

            if ((!ModOptions.HidePearls.Value || addon.IsActiveObject) && effect.MajorEffect == POEffect.MajorEffectType.CAMOFLAGUE)
                addon.CamoLerp = 0.0f;

            addon.DrawSpearLerp = playerModule.SpearLerp;

            addon.ShieldCounter = playerModule.ShieldCount;
            addon.ReviveCounter = playerModule.ReviveCount;


            
            addon.IsSentry = poModule.IsSentry || poModule.IsReturningSentry;

            if (addon.IsSentry || addon.IsActiveRagePearl)
            {
                if (addon.IsSentry)
                {
                    addon.CamoLerp = 0.0f;
                    addon.Symbol = "pearlcat_glyphsentry";
                }

                // ew
                if (Hooks.TargetPositions.TryGetValue(abstractObject, out var targetPos))
                {
                    addon.OverridePos ??= abstractObject.realizedObject.firstChunk.pos;
                    addon.OverrideLastPos = addon.OverridePos;
                    
                    addon.OverridePos = Vector2.Lerp((Vector2)addon.OverridePos, addon.IsActiveObject ? player.GetActiveObjectPos(timeStacker: 1.0f) : targetPos.Value, 0.7f);
                    
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

            var returnPos = Hooks.TargetPositions.TryGetValue(abstractObject, out var pos) ? pos.Value : player.GetActiveObjectPos();
            var returned = Custom.DistLess(abstractObject.realizedObject.firstChunk.pos, returnPos, 8.0f);


            if (addon.IsActiveRagePearl)
            {
                returnPos = player.firstChunk.pos;
                returned = Custom.DistLess(abstractObject.realizedObject.firstChunk.pos, returnPos, 80.0f);
            }

            if (poModule.IsReturningSentry && returned)
            {
                abstractObject.realizedObject.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, abstractObject.realizedObject.firstChunk.pos, 0.5f, 3.0f);
                abstractObject.realizedObject.room.AddObject(new LightningMachine.Impact(abstractObject.realizedObject.firstChunk.pos, 0.1f, addon.SymbolColor, true));

                poModule.IsReturningSentry = false;
            }
            
            var hasTarget = false;
            
            if (!addon.IsSentry && playerModule.RageTarget?.TryGetTarget(out var target) == true)
            {
                hasTarget = true;
                addon.LaserTarget = target.mainBodyChunk.pos;
            }

            addon.IsLaserVisible = hasTarget && effect.MajorEffect == POEffect.MajorEffectType.RAGE && playerModule.ActiveObject?.GetPOEffect().MajorEffect == POEffect.MajorEffectType.RAGE;
            addon.LaserLerp = poModule.LaserLerp;

        }
    }


    public void AnimateOrbit(Player player, Vector2 origin, float radius, float angleFrameAddition, List<AbstractPhysicalObject> abstractObjects)
    {
        for (int i = 0; i < abstractObjects.Count; i++)
        {
            var abstractObject = abstractObjects[i];

            var angle = (i * Mathf.PI * 2.0f / abstractObjects.Count) + angleFrameAddition * AnimTimer;

            Vector2 targetPos = new(origin.x + Mathf.Cos(angle) * radius, origin.y + Mathf.Sin(angle) * radius);
            abstractObject.TryToAnimateToTargetPos(player, targetPos);
        }
    }
}
