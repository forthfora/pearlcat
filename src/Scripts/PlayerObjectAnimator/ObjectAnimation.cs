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

        for (int i = 0; i < playerModule.abstractInventory.Count; i++)
            HaloEffectStackers.Add((1.0f / playerModule.abstractInventory.Count) * i);
    }


    public int animStacker = 0;

    public virtual void Update(Player player)
    {
        if (!player.TryGetPearlcatModule(out var playerModule)) return;


        for (int i = 0; i < playerModule.abstractInventory.Count; i++)
        {
            AbstractPhysicalObject abstractObject = playerModule.abstractInventory[i];

            if (abstractObject.realizedObject == null) continue;
            var realizedObject = abstractObject.realizedObject;

            if (!Hooks.PlayerObjectData.TryGetValue(realizedObject, out var playerObjectModule)) continue;


            if (!ObjectAddon.ObjectsWithAddon.TryGetValue(realizedObject, out _))
                new ObjectAddon(abstractObject);

            playerObjectModule.playCollisionSound = false;
        }

        animStacker++;
    }


    // TODO: the pain
    public virtual void MoveToTargetPos(Player player, AbstractPhysicalObject abstractObject, Vector2 targetPos)
    {
        if (!player.TryGetPearlcatModule(out var playerModule)) return;

        if (abstractObject.realizedObject == null) return;

        if (!Hooks.MinFricSpeed.TryGet(abstractObject.world.game, out var minFricSpeed)) return;
        if (!Hooks.MaxFricSpeed.TryGet(abstractObject.world.game, out var maxFricSpeed)) return;
        if (!Hooks.MinFric.TryGet(abstractObject.world.game, out var minFric)) return;
        if (!Hooks.MaxFric.TryGet(abstractObject.world.game, out var maxFric)) return;

        if (!Hooks.CutoffDist.TryGet(abstractObject.world.game, out var cutoffDist)) return;
        if (!Hooks.CutoffMinSpeed.TryGet(abstractObject.world.game, out var cutoffMinSpeed)) return;
        if (!Hooks.CutoffMaxSpeed.TryGet(abstractObject.world.game, out var cutoffMaxSpeed)) return;
        if (!Hooks.DazeMaxSpeed.TryGet(abstractObject.world.game, out var dazeMaxSpeed)) return;

        if (!Hooks.MaxDist.TryGet(abstractObject.world.game, out var maxDist)) return;
        if (!Hooks.MinSpeed.TryGet(abstractObject.world.game, out var minSpeed)) return;
        if (!Hooks.MaxSpeed.TryGet(abstractObject.world.game, out var maxSpeed)) return;

        var firstChunk = abstractObject.realizedObject.firstChunk;
        var dir = (targetPos - firstChunk.pos).normalized;
        var dist = Custom.Dist(firstChunk.pos, targetPos);

        float speed = dist < cutoffDist ? Custom.LerpMap(dist, 0.0f, cutoffDist, cutoffMinSpeed, playerModule.IsDazed ? dazeMaxSpeed : cutoffMaxSpeed) : Custom.LerpMap(dist, cutoffDist, maxDist, minSpeed, maxSpeed);

        firstChunk.vel *= Custom.LerpMap(firstChunk.vel.magnitude, minFricSpeed, maxFricSpeed, minFric, maxFric);
        firstChunk.vel += dir * speed;
    }

    public virtual Vector2 GetActiveObjectPos(Player player)
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

            if (!ObjectAddon.ObjectsWithAddon.TryGetValue(abstractObject.realizedObject, out var effect)) continue;

            
            effect.drawHalo = true;
            float haloEffectStacker = HaloEffectStackers[i];

            if (i == playerModule.activeObjectIndex)
            {
                effect.haloColor = Hooks.GetObjectColor(abstractObject) * new Color(1.0f, 0.25f, 0.25f);
                effect.haloScale = 1.0f + 0.45f * haloEffectStacker;
                effect.haloAlpha = 0.8f;
            }
            else
            {
                effect.haloColor = Hooks.GetObjectColor(abstractObject) * new Color(0.25f, 0.25f, 1.0f);
                effect.haloScale = 0.3f + 0.45f * haloEffectStacker;
                effect.haloAlpha = 0.6f;
            }



            if (haloEffectStacker < 0.0f)
                HaloEffectDir = 1;

            else if (haloEffectStacker > 1.0f)
                HaloEffectDir = -1;

            HaloEffectStackers[i] += HaloEffectDir * HaloEffectFrameAddition;
        }
    }
}
