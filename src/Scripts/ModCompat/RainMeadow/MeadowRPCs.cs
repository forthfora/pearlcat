using System.Collections.Generic;
using Newtonsoft.Json;
using RainMeadow;
using UnityEngine;
// ReSharper disable ConditionalAccessQualifierIsNonNullableAccordingToAPIContract

namespace Pearlcat;

public static class MeadowRPCs
{
    [RPCMethod]
    public static void RevivePlayer(RPCEvent _, OnlinePhysicalObject? playerOpo)
    {
        if (playerOpo?.apo?.realizedObject is not Player player)
        {
            return;
        }

        Player_Helpers.RevivePlayer_Local(player);
    }

    [RPCMethod]
    public static void ActivateVisualShield(RPCEvent _, OnlinePhysicalObject? playerOpo)
    {
        if (playerOpo?.apo?.realizedObject is not Player player)
        {
            return;
        }

        if (!player.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        playerModule.ActivateVisualShield_Local();
    }

    [RPCMethod]
    public static void ObjectConnectEffect(RPCEvent _, OnlinePhysicalObject? opo, Vector2 pos, Color color)
    {
        if (opo?.apo?.realizedObject is not PhysicalObject physicalObject)
        {
            return;
        }

        physicalObject.ConnectEffect(pos, color);
    }

    [RPCMethod]
    public static void RoomConnectEffect(RPCEvent _, RoomSession? roomSession, Vector2 startPos, Vector2 targetPos, Color color, float intensity, float lifeTime)
    {
        if (roomSession?.absroom?.realizedRoom is not Room room)
        {
            return;
        }

        room.ConnectEffect(startPos, targetPos, color, intensity, lifeTime);
    }

    [RPCMethod]
    public static void ExplodeSentry(RPCEvent _, OnlinePhysicalObject? opo)
    {
        if (opo?.apo is not AbstractPhysicalObject apo)
        {
            return;
        }

        if (!apo.TryGetSentry(out var sentry))
        {
            return;
        }

        sentry.ExplodeSentry();
    }
}
