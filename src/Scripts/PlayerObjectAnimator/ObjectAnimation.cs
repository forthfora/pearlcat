using RWCustom;
using System.Collections.Generic;
using UnityEngine;
using Color = UnityEngine.Color;

namespace Pearlcat;

public abstract class ObjectAnimation
{
    public ObjectAnimation(Player player) => InitAnimation(player);

    public virtual void InitAnimation(Player self)
    {
        if (!self.TryGetPearlcatModule(out var playerModule)) return;

        HaloEffectStackers.Clear();

        for (int i = 0; i < playerModule.Inventory.Count; i++)
            HaloEffectStackers.Add((1.0f / playerModule.Inventory.Count) * i);
    }


    public int animTimer = 0;

    public virtual void Update(Player player)
    {
        if (!player.TryGetPearlcatModule(out var playerModule)) return;


        for (int i = 0; i < playerModule.Inventory.Count; i++)
        {
            var abstractObject = playerModule.Inventory[i];

            if (abstractObject.realizedObject == null) continue;

            if (!Hooks.PlayerObjectData.TryGetValue(abstractObject, out var playerObjectModule)) continue;

            if (!ObjectAddon.ObjectsWithAddon.TryGetValue(abstractObject, out _))
                new ObjectAddon(abstractObject);

            playerObjectModule.PlayCollisionSound = false;
        }

        animTimer++;

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
            AbstractPhysicalObject abstractObject = playerModule.Inventory[i];

            if (abstractObject.realizedObject == null) continue;

            if (!ObjectAddon.ObjectsWithAddon.TryGetValue(abstractObject, out var addon)) continue;
            
            addon.drawHalo = true;
            float haloEffectTimer = HaloEffectStackers[i];

            if (i == playerModule.ActiveObjectIndex)
            {
                addon.haloColor = Hooks.GetObjectColor(abstractObject) * new Color(1.0f, 0.25f, 0.25f);
                addon.haloScale = 1.0f + 0.45f * haloEffectTimer;
                addon.haloAlpha = 0.8f;
            }
            else
            {
                addon.haloColor = Hooks.GetObjectColor(abstractObject) * new Color(0.25f, 0.25f, 1.0f);
                addon.haloScale = 0.3f + 0.45f * haloEffectTimer;
                addon.haloAlpha = 0.6f;
            }


            if (haloEffectTimer < 0.0f)
                HaloEffectDir = 1;

            else if (haloEffectTimer > 1.0f)
                HaloEffectDir = -1;

            HaloEffectStackers[i] += HaloEffectDir * HaloEffectFrameAddition;
        }
    }

    public virtual void UpdateSymbolEffects(Player player)
    {
        if (!player.TryGetPearlcatModule(out var playerModule)) return;

        for (int i = 0; i < playerModule.Inventory.Count; i++)
        {
            AbstractPhysicalObject abstractObject = playerModule.Inventory[i];
            if (abstractObject.realizedObject == null) continue;

            if (!ObjectAddon.ObjectsWithAddon.TryGetValue(abstractObject, out var addon)) continue;

            if (!Hooks.PlayerObjectData.TryGetValue(abstractObject, out var poModule)) continue;
            
            var effect = abstractObject.GetPOEffect();
            var majorEffect = effect.MajorEffect;

            addon.symbolColor = Hooks.GetObjectColor(abstractObject);
            
            if (i == playerModule.ActiveObjectIndex)
            {
                addon.isActive = true;
            }
            else
            {
                addon.isActive = false;
                majorEffect = POEffect.MajorEffectType.NONE;
            }    

            if (poModule.CooldownTimer != 0)
            {
                if (poModule.CooldownTimer == 1)
                    abstractObject.realizedObject.room.AddObject(new ShockWave(abstractObject.realizedObject.firstChunk.pos, 10.0f, 1.0f, 5, true));
                
                majorEffect = POEffect.MajorEffectType.NONE;
                addon.drawSymbolCooldown = true;

                var cooldownLerp = poModule.CooldownTimer < 0 ? 1.0f : Custom.LerpMap(poModule.CooldownTimer, 40, 0, 1.0f, 0.0f);
                addon.symbolColor = Color.Lerp(addon.symbolColor, new Color(189 / 255.0f, 13 / 255.0f, 0.0f), cooldownLerp);
            }
            else
            {
                addon.drawSymbolCooldown = false;
            }
            
            addon.drawSymbolSpear = majorEffect == POEffect.MajorEffectType.SPEAR_CREATION;
            addon.drawSymbolRage = majorEffect == POEffect.MajorEffectType.RAGE;
            addon.drawSymbolRevive = majorEffect == POEffect.MajorEffectType.REVIVE;
            addon.drawSymbolShield = majorEffect == POEffect.MajorEffectType.SHIELD;
            addon.drawSymbolAgility = majorEffect == POEffect.MajorEffectType.AGILITY;
            addon.drawSymbolCamo = majorEffect == POEffect.MajorEffectType.CAMOFLAGUE;

            addon.camoLerp = playerModule.CamoLerp;
        }
    }

    public void AnimateOrbit(Player player, Vector2 origin, float radius, float angleFrameAddition, List<AbstractPhysicalObject> abstractObjects)
    {
        for (int i = 0; i < abstractObjects.Count; i++)
        {
            var abstractObject = abstractObjects[i];

            var angle = (i * Mathf.PI * 2.0f / abstractObjects.Count) + angleFrameAddition * animTimer;

            Vector2 targetPos = new(origin.x + Mathf.Cos(angle) * radius, origin.y + Mathf.Sin(angle) * radius);
            abstractObject.MoveToTargetPos(player, targetPos);
        }
    }
}
