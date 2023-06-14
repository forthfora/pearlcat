using UnityEngine;

namespace Pearlcat
{
    public class BasicObjectAnimation : ObjectAnimation
    {
        public BasicObjectAnimation(Player player) : base(player) { }



        public override void Update(Player player)
        {
            base.Update(player);

            if (!player.TryGetPearlcatModule(out var playerModule)) return;


            UpdateHaloEffects(player);


            int nonActiveIndex = 0;

            for (int i = 0; i < playerModule.abstractInventory.Count; i++)
            {
                AbstractPhysicalObject abstractObject = playerModule.abstractInventory[i];

                if (i == playerModule.activeObjectIndex)
                    MoveToTargetPos(abstractObject, GetActiveObjectPos(player));

                float radius = 20.0f;

                float angle = nonActiveIndex * Mathf.PI * 2.0f / playerModule.abstractInventory.Count - 1;
                Vector2 origin = GetActiveObjectPos(player);

                Vector2 pos = new(origin.x + Mathf.Cos(angle) * radius, origin.y + Mathf.Sin(angle) * radius);

                MoveToTargetPos(abstractObject, pos);

                nonActiveIndex++;
            }
        }
    }
}
