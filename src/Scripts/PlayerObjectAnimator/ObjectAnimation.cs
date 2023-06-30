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

        for (int i = 0; i < playerModule.abstractInventory.Count; i++)
            HaloEffectStackers.Add((1.0f / playerModule.abstractInventory.Count) * i);
    }


    public int animStacker = 0;

    public virtual void Update(Player player)
    {
        if (!player.TryGetPearlcatModule(out var playerModule)) return;


        for (int i = 0; i < playerModule.abstractInventory.Count; i++)
        {
            var abstractObject = playerModule.abstractInventory[i];

            if (abstractObject.realizedObject == null) continue;
            var realizedObject = abstractObject.realizedObject;

            if (!Hooks.PlayerObjectData.TryGetValue(realizedObject, out var playerObjectModule)) continue;

            if (!ObjectAddon.ObjectsWithAddon.TryGetValue(abstractObject, out _))
                new ObjectAddon(abstractObject);

            playerObjectModule.playCollisionSound = false;
        }

        animStacker++;

        UpdateHaloEffects(player);
        UpdateSymbolEffects(player);
    }

    public static Vector2 GetActiveObjectPos(Player player)
    {
        if (!Hooks.ActiveObjectOffset.TryGet(player, out var activeObjectOffset))
            activeObjectOffset = Vector2.zero;

        PlayerGraphics playerGraphics = (PlayerGraphics)player.graphicsModule;

        Vector2 pos = playerGraphics.head.pos + activeObjectOffset;
        pos.x += player.mainBodyChunk.vel.x * 1.0f;

        return pos;
    }



    public List<float> HaloEffectStackers = new();

    public float HaloEffectFrameAddition { get; set; } = 0.02f;
    
    public float HaloEffectDir { get; set; } = 1;



    public virtual void UpdateHaloEffects(Player player)
    {
        if (!player.TryGetPearlcatModule(out var playerModule)) return;


        for (int i = 0; i < playerModule.abstractInventory.Count; i++)
        {
            AbstractPhysicalObject abstractObject = playerModule.abstractInventory[i];

            if (abstractObject.realizedObject == null) continue;

            if (!ObjectAddon.ObjectsWithAddon.TryGetValue(abstractObject, out var addon)) continue;
            
            addon.drawHalo = true;
            float haloEffectStacker = HaloEffectStackers[i];

            if (i == playerModule.activeObjectIndex)
            {
                addon.haloColor = Hooks.GetObjectColor(abstractObject) * new Color(1.0f, 0.25f, 0.25f);
                addon.haloScale = 1.0f + 0.45f * haloEffectStacker;
                addon.haloAlpha = 0.8f;
            }
            else
            {
                addon.haloColor = Hooks.GetObjectColor(abstractObject) * new Color(0.25f, 0.25f, 1.0f);
                addon.haloScale = 0.3f + 0.45f * haloEffectStacker;
                addon.haloAlpha = 0.6f;
            }


            if (haloEffectStacker < 0.0f)
                HaloEffectDir = 1;

            else if (haloEffectStacker > 1.0f)
                HaloEffectDir = -1;

            HaloEffectStackers[i] += HaloEffectDir * HaloEffectFrameAddition;
        }
    }

    public virtual void UpdateSymbolEffects(Player player)
    {
        if (!player.TryGetPearlcatModule(out var playerModule)) return;

        for (int i = 0; i < playerModule.abstractInventory.Count; i++)
        {
            AbstractPhysicalObject abstractObject = playerModule.abstractInventory[i];
            if (abstractObject.realizedObject == null) continue;

            if (!ObjectAddon.ObjectsWithAddon.TryGetValue(abstractObject, out var addon)) continue;

            var effect = abstractObject.GetPOEffect();
            var majorEffect = effect.majorEffect;

            if (i != playerModule.activeObjectIndex)
                majorEffect = POEffect.MajorEffect.NONE;

            addon.drawSymbolSpear = majorEffect == POEffect.MajorEffect.SPEAR_CREATION;
            addon.drawSymbolRage = majorEffect == POEffect.MajorEffect.RAGE;
            addon.drawSymbolRevive = majorEffect == POEffect.MajorEffect.REVIVE;
            addon.drawSymbolShield = majorEffect == POEffect.MajorEffect.SHIELD;
            addon.drawSymbolAgility = majorEffect == POEffect.MajorEffect.AGILITY;
            addon.drawSymbolCamo = majorEffect == POEffect.MajorEffect.CAMOFLAGUE;

            addon.symbolColor = Hooks.GetObjectColor(abstractObject);
        }
    }

    public void AnimateOrbit(Player player, Vector2 origin, float radius, float angleFrameAddition, List<AbstractPhysicalObject> abstractObjects)
    {
        for (int i = 0; i < abstractObjects.Count; i++)
        {
            var abstractObject = abstractObjects[i];

            var angle = (i * Mathf.PI * 2.0f / abstractObjects.Count) + angleFrameAddition * animStacker;

            Vector2 targetPos = new(origin.x + Mathf.Cos(angle) * radius, origin.y + Mathf.Sin(angle) * radius);
            abstractObject.MoveToTargetPos(player, targetPos);
        }
    }
}
