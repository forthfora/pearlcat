using System.Collections.Generic;
using UnityEngine;

namespace Pearlcat;

public class SineWaveOA : ObjectAnimation
{
    public readonly List<float> floatOffsets = new(); 

    public SineWaveOA(Player player) : base(player)
    {
        if (!player.TryGetPearlcatModule(out var playerModule)) return;
    }

    public override void Update(Player player)
    {
        base.Update(player);

        if (!player.TryGetPearlcatModule(out var playerModule)) return;

        var floatingObjects = new List<AbstractPhysicalObject>();
        floatingObjects.AddRange(playerModule.Inventory);

        var activeObject = playerModule.ActiveObject;

        if (activeObject != null)
        {
            floatingObjects.Remove(activeObject);
            activeObject.MoveToTargetPos(player, player.GetActiveObjectPos());
        }

        for (int i = 0; i < floatingObjects.Count; i++)
        {
            var abstractObject = floatingObjects[i];

            Vector2 targetPos = Vector2.zero;

            float spacing = 10.0f;

            targetPos.x = player.firstChunk.pos.x + spacing * i - floatingObjects.Count / 2.0f * spacing + (spacing / 2.0f);
            targetPos.y = player.firstChunk.pos.y + 20.0f * Mathf.Sin(animTimer / 30.0f + i * (180.0f / floatingObjects.Count));

            abstractObject.MoveToTargetPos(player, targetPos);
        }
    }
}
