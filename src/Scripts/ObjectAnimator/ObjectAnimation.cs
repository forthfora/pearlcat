using RWCustom;
using SlugBase.Features;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Color = UnityEngine.Color;

namespace TheSacrifice
{
    internal abstract class ObjectAnimation
    {
        public virtual void Update(Player self)
        {
            if (!Hooks.PlayerData.TryGetValue(self, out var playerModule)) return;

            for (int i = 0; i < playerModule.abstractInventory.Count; i++)
            {
                AbstractPhysicalObject abstractObject = playerModule.abstractInventory[i];

                if (!ObjectAddon.ObjectsWithAddons.TryGetValue(abstractObject.realizedObject, out _))
                    new ObjectAddon(abstractObject);
            }
        }



        protected const float MaxLockDistance = 3.0f;

        protected virtual void MoveToTargetPos(AbstractPhysicalObject abstractObject, Vector2 targetPos)
        {
            if (abstractObject.realizedObject == null) return;

            if (Custom.Dist(abstractObject.realizedObject.firstChunk.pos, targetPos) <= MaxLockDistance)
                abstractObject.realizedObject.firstChunk.pos = targetPos;

            abstractObject.realizedObject.firstChunk.pos = Vector2.Lerp(abstractObject.realizedObject.firstChunk.pos, targetPos, 0.5f);
        }

        protected virtual Vector2 GetActiveObjectPos(Player self)
        {
            if (!Hooks.ActiveObjectOffset.TryGet(self, out var activeObjectOffset))
                activeObjectOffset = Vector2.zero;

            if (self.gravity == 0.0f)
                return self.graphicsModule.bodyParts[6].pos + (activeObjectOffset.magnitude * self.bodyChunks[0].Rotation);


            Vector2 pos = self.graphicsModule.bodyParts[6].pos + activeObjectOffset;
            pos.x += self.mainBodyChunk.vel.x * 1.0f;

            return pos;
        }

        protected virtual void UpdateHaloEffects(Player self)
        {
            if (!Hooks.PlayerData.TryGetValue(self, out var playerModule)) return;

            for (int i = 0; i < playerModule.abstractInventory.Count; i++)
            {
                AbstractPhysicalObject abstractObject = playerModule.abstractInventory[i];

                if (!ObjectAddon.ObjectsWithAddons.TryGetValue(abstractObject.realizedObject, out var effect)) continue;


                effect.drawHalo = true;
                effect.haloColor = Hooks.GetObjectFirstColor(abstractObject);

                if (i == playerModule.activeObjectIndex)
                {
                    effect.haloScale = 0.85f;
                    effect.haloAlpha = 0.6f;
                }
            }
        }
    }
}
