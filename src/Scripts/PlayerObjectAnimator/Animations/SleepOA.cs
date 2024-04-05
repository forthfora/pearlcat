using System.Collections.Generic;
using UnityEngine;

namespace Pearlcat;

public class SleepOA : ObjectAnimation
{
    public SleepOA(Player player) : base(player) { }

    public override void Update(Player player)
    {   
        base.Update(player);

        if (!player.TryGetPearlcatModule(out var playerModule)) return;

        var fallingObjects = new List<AbstractPhysicalObject>();
        fallingObjects.AddRange(playerModule.Inventory);

        var activeObject = playerModule.ActiveObject;

        if (activeObject != null)
        {
            fallingObjects.Remove(activeObject);
            activeObject.TryToAnimateToTargetPos(player, player.GetActiveObjectPos(new Vector2(0.0f, 30.0f)));
        }


        foreach (var abstractObject in fallingObjects)
        {
            if (abstractObject.realizedObject == null) continue;
            var realizedObject = abstractObject.realizedObject;

            if (!realizedObject.abstractPhysicalObject.TryGetPOModule(out var playerObjectModule)) continue;

            realizedObject.gravity = 0.05f;
            realizedObject.CollideWithTerrain = true;

            playerObjectModule.PlayCollisionSound = false;
        }
    }
}
