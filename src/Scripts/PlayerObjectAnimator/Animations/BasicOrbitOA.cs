using UnityEngine;

namespace Pearlcat;

public class BasicOrbitOA : ObjectAnimation
{
    public BasicOrbitOA(Player player) : base(player) { }


    public const float angleFrameAddition = 0.02f;
    public const float radius = 25.0f;

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
            {
                MoveToTargetPos(abstractObject, GetActiveObjectPos(player));
                continue;
            }

            float angle = (nonActiveIndex * Mathf.PI * 2.0f / (playerModule.abstractInventory.Count - 1)) + angleFrameAddition * animStacker;
            Vector2 origin = ((PlayerGraphics)player.graphicsModule).head.pos;

            Vector2 targetPos = new(origin.x + Mathf.Cos(angle) * radius, origin.y + Mathf.Sin(angle) * radius);
            MoveToTargetPos(abstractObject, targetPos);

            nonActiveIndex++;
        }
    }
}
