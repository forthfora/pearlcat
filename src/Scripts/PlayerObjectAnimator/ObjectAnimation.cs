using RWCustom;
using System.Collections.Generic;
using UnityEngine;
using Color = UnityEngine.Color;

namespace Pearlcat
{
    public abstract class ObjectAnimation
    {
        public ObjectAnimation(Player player) => InitAnimation(player);

        public virtual void InitAnimation(Player self)
        {
            if (!self.TryGetPearlcatModule(out var playerModule, createIfMissing: false)) return;


            HaloEffectStackers.Clear();

            for (int i = 0; i < playerModule.abstractInventory.Count; i++)
                HaloEffectStackers.Add((1.0f / playerModule.abstractInventory.Count) * i);
        }


        public virtual void Update(Player player)
        {
            if (!player.TryGetPearlcatModule(out var playerModule)) return;


            for (int i = 0; i < playerModule.abstractInventory.Count; i++)
            {
                AbstractPhysicalObject abstractObject = playerModule.abstractInventory[i];

                if (abstractObject.realizedObject == null) continue;

                if (!ObjectAddon.ObjectsWithAddon.TryGetValue(abstractObject.realizedObject, out _))
                    new ObjectAddon(abstractObject);
            }
        }



        public const float MaxLockDistance = 0.1f;

        public virtual void MoveToTargetPos(AbstractPhysicalObject abstractObject, Vector2 targetPos)
        {
            if (abstractObject.realizedObject == null) return;

            if (Custom.Dist(abstractObject.realizedObject.firstChunk.pos, targetPos) <= MaxLockDistance)
            {
                abstractObject.realizedObject.firstChunk.pos = targetPos;
                return;
            }

            abstractObject.realizedObject.firstChunk.pos = Vector2.Lerp(abstractObject.realizedObject.firstChunk.pos, targetPos, 0.5f);
        }

        public virtual Vector2 GetActiveObjectPos(Player player)
        {
            if (!Hooks.ActiveObjectOffset.TryGet(player, out var activeObjectOffset))
                activeObjectOffset = Vector2.zero;

            if (player.bodyMode == Player.BodyModeIndex.ZeroG)
                return player.graphicsModule.bodyParts[6].pos + (activeObjectOffset.magnitude * player.bodyChunks[0].Rotation);


            Vector2 pos = player.graphicsModule.bodyParts[6].pos + activeObjectOffset;
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
                    effect.haloColor = Hooks.GetObjectFirstColor(abstractObject) * new Color(1.0f, 0.25f, 0.25f);
                    effect.haloScale = 0.4f + 0.45f * haloEffectStacker;
                    effect.haloAlpha = 0.6f;
                }
                else
                {
                    effect.haloColor = Hooks.GetObjectFirstColor(abstractObject) * new Color(0.25f, 0.25f, 1.0f);
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
}
