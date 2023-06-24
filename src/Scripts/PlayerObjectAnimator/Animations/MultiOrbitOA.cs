using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Pearlcat;

public class MultiOrbitOA : ObjectAnimation
{
    // num = amount
    public readonly List<AbstractPhysicalObject> OrbitPearls_5 = new();
    public readonly List<AbstractPhysicalObject> OrbitPearls_3 = new();
    public readonly List<AbstractPhysicalObject> OrbitPearls_2 = new();

    public readonly Vector2 OrbitPearls_5_Offset = new(-25.0f, 7.0f);
    public readonly Vector2 OrbitPearls_3_Offset = new(25.0f, 25.0f);
    public readonly Vector2 OrbitPearls_2_Offset = new(23.0f, -10.0f);


    public const float RADIUS_5 = 15.0f;
    public const float RADIUS_3 = 10.0f;
    public const float RADIUS_2 = 7.5f;

    public const float F_ADDITION_5 = 0.04f;
    public const float F_ADDITION_3 = -0.08f;
    public const float F_ADDITION_2 = 0.16f;

    public AbstractPhysicalObject? prevActiveObject;

    public MultiOrbitOA(Player player) : base(player)
    {
        if (!player.TryGetPearlcatModule(out var playerModule)) return;

        foreach (int randIndex in Enumerable.Range(0, playerModule.abstractInventory.Count).OrderBy(x => Random.value))
        {
            var item = playerModule.abstractInventory[randIndex];
            if (item == playerModule.ActiveObject) continue;

            if (OrbitPearls_2.Count < 2)
                OrbitPearls_2.Add(item);

            else if (OrbitPearls_3.Count < 3)
                OrbitPearls_3.Add(item);

            else
                OrbitPearls_5.Add(item);
        }
    }

    public override void Update(Player player)
    {
        base.Update(player);

        if (!player.TryGetPearlcatModule(out var playerModule)) return;

        if (playerModule.ActiveObject != prevActiveObject)
            ReplaceActive(playerModule);

        var headPos = ((PlayerGraphics)player.graphicsModule).head.pos;

        AnimateOrbit(player, headPos + OrbitPearls_5_Offset, RADIUS_5, F_ADDITION_5, OrbitPearls_5);
        AnimateOrbit(player, headPos + OrbitPearls_3_Offset, RADIUS_3, F_ADDITION_3, OrbitPearls_3);
        AnimateOrbit(player, headPos + OrbitPearls_2_Offset, RADIUS_2, F_ADDITION_2, OrbitPearls_2);

        playerModule.ActiveObject?.MoveToTargetPos(player, GetActiveObjectPos(player));
        prevActiveObject = playerModule.ActiveObject;
    }

    public void ReplaceActive(PearlcatModule playerModule)
    {
        if (ReplaceActiveForOrbit(OrbitPearls_5, playerModule.ActiveObject, prevActiveObject)) return;

        if (ReplaceActiveForOrbit(OrbitPearls_3, playerModule.ActiveObject, prevActiveObject)) return;

        if (ReplaceActiveForOrbit(OrbitPearls_2, playerModule.ActiveObject, prevActiveObject)) return;
    }

    public bool ReplaceActiveForOrbit(List<AbstractPhysicalObject> orbitPearls, AbstractPhysicalObject? activeObject, AbstractPhysicalObject? prevActiveObject)
    {
        if (activeObject == null || prevActiveObject == null)
            return false;

        if (orbitPearls.Contains(activeObject))
        {
            orbitPearls.Remove(activeObject);
            orbitPearls.Add(prevActiveObject);
            return true;
        }

        return false;
    }
}
