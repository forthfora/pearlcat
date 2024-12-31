﻿using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace Pearlcat;

public class PearlAnimation_LayerOrbit : PearlAnimation
{
    // num = amount
    public readonly List<AbstractPhysicalObject> OrbitPearls_5 = [];
    public readonly List<AbstractPhysicalObject> OrbitPearls_3 = [];
    public readonly List<AbstractPhysicalObject> OrbitPearls_2 = [];

    public const float RADIUS_5 = 30.0f;
    public const float RADIUS_3 = 20.0f;
    public const float RADIUS_2 = 10.0f;

    public const float F_ADDITION_5 = -0.03f;
    public const float F_ADDITION_3 = 0.06f;
    public const float F_ADDITION_2 = -0.1f;

    public AbstractPhysicalObject? prevActiveObject;

    public PearlAnimation_LayerOrbit(Player player) : base(player)
    {
        if (!player.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        foreach (var randIndex in Enumerable.Range(0, playerModule.Inventory.Count).OrderBy(_ => Random.value))
        {
            var item = playerModule.Inventory[randIndex];

            if (item == playerModule.ActiveObject)
            {
                continue;
            }

            if (OrbitPearls_2.Count < 2)
            {
                OrbitPearls_2.Add(item);
            }
            else if (OrbitPearls_3.Count < 3)
            {
                OrbitPearls_3.Add(item);
            }
            else
            {
                OrbitPearls_5.Add(item);
            }
        }
    }

    public override void Update(Player player)
    {
        base.Update(player);

        if (!player.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        if (playerModule.ActiveObject != prevActiveObject)
        {
            ReplaceActive(playerModule);
        }

        var headPos = ((PlayerGraphics)player.graphicsModule).head.pos;

        AnimateOrbit(player, headPos, RADIUS_5, F_ADDITION_5, OrbitPearls_5);
        AnimateOrbit(player, headPos, RADIUS_3, F_ADDITION_3, OrbitPearls_3);
        AnimateOrbit(player, headPos, RADIUS_2, F_ADDITION_2, OrbitPearls_2);

        playerModule.ActiveObject?.TryToAnimateToTargetPos(player, player.GetActivePearlPos());
        prevActiveObject = playerModule.ActiveObject;
    }

    public void ReplaceActive(PlayerModule playerModule)
    {
        if (ReplaceActiveForOrbit(OrbitPearls_5, playerModule.ActiveObject, prevActiveObject))
        {
            return;
        }

        if (ReplaceActiveForOrbit(OrbitPearls_3, playerModule.ActiveObject, prevActiveObject))
        {
            return;
        }

        _ = ReplaceActiveForOrbit(OrbitPearls_2, playerModule.ActiveObject, prevActiveObject);
    }

    public bool ReplaceActiveForOrbit(List<AbstractPhysicalObject> orbitPearls, AbstractPhysicalObject? activeObject, AbstractPhysicalObject? objToReplace)
    {
        if (activeObject is null || objToReplace is null)
        {
            return false;
        }

        if (orbitPearls.Contains(activeObject))
        {
            orbitPearls.Remove(activeObject);
            orbitPearls.Add(objToReplace);

            return true;
        }

        return false;
    }
}