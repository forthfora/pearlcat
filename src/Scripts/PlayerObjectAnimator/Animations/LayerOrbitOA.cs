using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Pearlcat;

public class LayerOrbitOA : ObjectAnimation
{
    // num = amount
    public readonly List<AbstractPhysicalObject> OrbitPearls_5 = new();
    public readonly List<AbstractPhysicalObject> OrbitPearls_3 = new();
    public readonly List<AbstractPhysicalObject> OrbitPearls_2 = new();

    public readonly Vector2 OrbitPearls_5_Offset = new(-25.0f, 7.0f);
    public readonly Vector2 OrbitPearls_3_Offset = new(25.0f, 25.0f);
    public readonly Vector2 OrbitPearls_2_Offset = new(23.0f, -10.0f);


    public const float RADIUS_5 = 30.0f;
    public const float RADIUS_3 = 20.0f;
    public const float RADIUS_2 = 10.0f;

    public const float F_ADDITION_5 = -0.03f;
    public const float F_ADDITION_3 = 0.06f;
    public const float F_ADDITION_2 = -0.1f;

    public AbstractPhysicalObject? prevActiveObject;

    public LayerOrbitOA(Player player) : base(player)
    {
        if (!player.TryGetPearlcatModule(out var playerModule)) return;

        foreach (int randIndex in Enumerable.Range(0, playerModule.Inventory.Count).OrderBy(x => Random.value))
        {
            var item = playerModule.Inventory[randIndex];
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

        AnimateOrbit(player, headPos, RADIUS_5, F_ADDITION_5, OrbitPearls_5);
        AnimateOrbit(player, headPos, RADIUS_3, F_ADDITION_3, OrbitPearls_3);
        AnimateOrbit(player, headPos, RADIUS_2, F_ADDITION_2, OrbitPearls_2);

        playerModule.ActiveObject?.MoveToTargetPos(player, GetActiveObjectPos(player));
        prevActiveObject = playerModule.ActiveObject;
    }

    public void ReplaceActive(PlayerModule playerModule)
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
