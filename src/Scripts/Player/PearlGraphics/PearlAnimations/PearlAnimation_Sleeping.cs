using System.Collections.Generic;
using UnityEngine;

namespace Pearlcat;

public sealed class PearlAnimation_Sleeping(Player player) : PearlAnimation(player)
{
    public override void Update(Player player)
    {   
        base.Update(player);

        if (!player.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        var fallingObjects = new List<AbstractPhysicalObject>();
        fallingObjects.AddRange(playerModule.Inventory);

        var activePearl = playerModule.ActivePearl;

        if (activePearl is not null)
        {
            fallingObjects.Remove(activePearl);
            activePearl.TryAnimateToTargetPos(player, player.GetActivePearlPos(new Vector2(0.0f, 30.0f)));
        }


        foreach (var abstractObject in fallingObjects)
        {
            if (abstractObject.realizedObject is null)
            {
                continue;
            }

            var realizedObject = abstractObject.realizedObject;

            if (!realizedObject.abstractPhysicalObject.TryGetPlayerPearlModule(out var playerPearlModule))
            {
                continue;
            }

            realizedObject.gravity = 0.05f;
            realizedObject.CollideWithTerrain = true;

            playerPearlModule.PlayCollisionSound = false;
        }
    }
}
