using RWCustom;
using System.Collections.Generic;
using UnityEngine;
using Color = UnityEngine.Color;

namespace Pearlcat
{
    public abstract class ObjectAnimation
    {
        public ObjectAnimation(Player self) => InitAnimation(self);

        public virtual void InitAnimation(Player self)
        {
            if (!Hooks.PlayerData.TryGetValue(self, out var playerModule)) return;

            HaloEffectStackers.Clear();
            for (int i = 0; i < playerModule.abstractInventory.Count; i++)
                HaloEffectStackers.Add((1.0f / playerModule.abstractInventory.Count) * i);
        }


        public virtual void Update(Player self)
        {
            if (!Hooks.PlayerData.TryGetValue(self, out var playerModule)) return;

            for (int i = 0; i < playerModule.abstractInventory.Count; i++)
            {
                AbstractPhysicalObject abstractObject = playerModule.abstractInventory[i];

                if (abstractObject.realizedObject == null) continue;

                if (!ObjectAddon.ObjectsWithAddon.TryGetValue(abstractObject.realizedObject, out _))
                    new ObjectAddon(abstractObject);
            }
        }



        protected const float MaxLockDistance = 0.1f;

        protected virtual void MoveToTargetPos(AbstractPhysicalObject abstractObject, Vector2 targetPos)
        {
            if (abstractObject.realizedObject == null) return;

            if (Custom.Dist(abstractObject.realizedObject.firstChunk.pos, targetPos) <= MaxLockDistance)
            {
                abstractObject.realizedObject.firstChunk.pos = targetPos;
                return;
            }

            abstractObject.realizedObject.firstChunk.pos = Vector2.Lerp(abstractObject.realizedObject.firstChunk.pos, targetPos, 0.5f);
        }

        protected virtual Vector2 GetActiveObjectPos(Player self)
        {
            if (!Hooks.ActiveObjectOffset.TryGet(self, out var activeObjectOffset))
                activeObjectOffset = Vector2.zero;

            if (self.bodyMode == Player.BodyModeIndex.ZeroG)
                return self.graphicsModule.bodyParts[6].pos + (activeObjectOffset.magnitude * self.bodyChunks[0].Rotation);


            Vector2 pos = self.graphicsModule.bodyParts[6].pos + activeObjectOffset;
            pos.x += self.mainBodyChunk.vel.x * 1.0f;

            return pos;
        }



        private List<float> HaloEffectStackers = new();
        private float HaloEffectFrameAddition = 0.02f;
        private float HaloEffectDir = 1;

        protected virtual void UpdateHaloEffects(Player self)
        {
            if (!Hooks.PlayerData.TryGetValue(self, out var playerModule)) return;

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
