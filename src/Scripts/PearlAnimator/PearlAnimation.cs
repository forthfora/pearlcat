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
    internal abstract class PearlAnimation
    {
        protected const float MaxLockDistance = 0.1f;



        public virtual void Update(Player self)
        {
            if (!Hooks.PlayerData.TryGetValue(self, out var playerModule)) return;

            List<int> implictIndexes = new List<int>();

            for (int i = 0; i < playerModule.abstractInventory.Count; i++)
            {
                Vector2? targetPos = GetObjectTargetPos(self, i);

                if (targetPos == null)
                {
                    implictIndexes.Add(i);
                    continue;
                }

                MoveToTargetPos(playerModule.abstractInventory[i], (Vector2)targetPos);
            }

            UpdateImplictObjects(self, implictIndexes);
        }

        private bool MoveToTargetPos(AbstractPhysicalObject abstractObject, Vector2 targetPos)
        {
            if (Custom.Dist(abstractObject.realizedObject.firstChunk.pos, targetPos) <= MaxLockDistance)
            {
                abstractObject.realizedObject.firstChunk.pos = targetPos;
                return true;
            }

            abstractObject.realizedObject.firstChunk.pos = Vector2.Lerp(abstractObject.realizedObject.firstChunk.pos, targetPos, 0.1f);
            return false;
        }

        private Vector2? GetObjectTargetPos(Player self, int index)
        {
            if (!Hooks.PlayerData.TryGetValue(self, out var playerModule)) return Vector2.zero;

            AbstractPhysicalObject abstractObject = playerModule.abstractInventory[index];

            if (index == playerModule.selectedObjectIndex)
                return UpdateActiveObject(self, abstractObject);

            return index switch
            {
                0 => UpdateObject1(self, abstractObject),
                1 => UpdateObject2(self, abstractObject),
                2 => UpdateObject3(self, abstractObject),
                3 => UpdateObject4(self, abstractObject),
                4 => UpdateObject5(self, abstractObject),
                6 => UpdateObject6(self, abstractObject),
                7 => UpdateObject7(self, abstractObject),
                8 => UpdateObject8(self, abstractObject),
                9 => UpdateObject9(self, abstractObject),
                10 => UpdateObject10(self, abstractObject),
               
                _ => null,
            };
        }



        protected virtual void UpdateImplictObjects(Player self, List<int> implictIndexes)
        {
            if (!Hooks.PlayerData.TryGetValue(self, out var playerModule)) return;

            foreach (int index in implictIndexes)
            {

            }
        }

        static readonly PlayerFeature<Vector2> ActiveObjectOffset = new("active_object_offset", Hooks.Vector2Feature);

        protected virtual Vector2? UpdateActiveObject(Player self, AbstractPhysicalObject physicalObject)
        {
            if (!ActiveObjectOffset.TryGet(self, out var activeObjectOffset))
                return null;

            if (self.gravity == 0.0f)
                return self.graphicsModule.bodyParts[6].pos + (activeObjectOffset.magnitude * self.bodyChunks[0].Rotation);


            Vector2 pos = self.graphicsModule.bodyParts[6].pos + activeObjectOffset;
            pos.x += self.mainBodyChunk.vel.x * 1.0f;

            return pos;
        }



        protected virtual Vector2? UpdateObject1(Player self, AbstractPhysicalObject physicalObject) => null;

        protected virtual Vector2? UpdateObject2(Player self, AbstractPhysicalObject physicalObject) => null;

        protected virtual Vector2? UpdateObject3(Player self, AbstractPhysicalObject physicalObject) => null;

        protected virtual Vector2? UpdateObject4(Player self, AbstractPhysicalObject physicalObject) => null;

        protected virtual Vector2? UpdateObject5(Player self, AbstractPhysicalObject physicalObject) => null;

        protected virtual Vector2? UpdateObject6(Player self, AbstractPhysicalObject physicalObject) => null;

        protected virtual Vector2? UpdateObject7(Player self, AbstractPhysicalObject physicalObject) => null;

        protected virtual Vector2? UpdateObject8(Player self, AbstractPhysicalObject physicalObject) => null;

        protected virtual Vector2? UpdateObject9(Player self, AbstractPhysicalObject physicalObject) => null;

        protected virtual Vector2? UpdateObject10(Player self, AbstractPhysicalObject physicalObject) => null;
    }
}
