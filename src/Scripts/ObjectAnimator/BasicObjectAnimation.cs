using UnityEngine;

namespace Pearlcat
{
    public class BasicObjectAnimation : ObjectAnimation
    {
        public BasicObjectAnimation(Player self) : base(self) { }



        public override void Update(Player self)
        {
            base.Update(self);

            if (!Hooks.PlayerData.TryGetValue(self, out var playerModule)) return;

            UpdateHaloEffects(self);



            int nonActiveIndex = 0;

            for (int i = 0; i < playerModule.abstractInventory.Count; i++)
            {
                AbstractPhysicalObject abstractObject = playerModule.abstractInventory[i];

                if (i == playerModule.activeObjectIndex)
                    MoveToTargetPos(abstractObject, GetActiveObjectPos(self));

                float radius = 20.0f;

                float angle = nonActiveIndex * Mathf.PI * 2.0f / playerModule.abstractInventory.Count - 1;
                Vector2 origin = GetActiveObjectPos(self);

                Vector2 pos = new(origin.x + Mathf.Cos(angle) * radius, origin.y + Mathf.Sin(angle) * radius);

                MoveToTargetPos(abstractObject, pos);

                nonActiveIndex++;
            }
        }
    }
}
