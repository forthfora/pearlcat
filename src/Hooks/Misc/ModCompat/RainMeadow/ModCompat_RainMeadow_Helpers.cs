using System;
using RainMeadow;

namespace Pearlcat;

public static class ModCompat_RainMeadow_Helpers
{
    public static bool IsMeadowInput(Player player)
    {
        if (!OnlinePhysicalObject.map.TryGetValue(player.abstractPhysicalObject, out var opo))
        {
            return false;
        }

        return !opo.isMine;
    }

    public static void RealizeInventory(Player player)
    {
        if (!OnlinePhysicalObject.map.TryGetValue(player.abstractPhysicalObject, out var playerOpo))
        {
            return;
        }

        if (playerOpo.isMine)
        {
            return;
        }

        foreach (var onlinePlayer in OnlineManager.players)
        {
            if (onlinePlayer.isMe)
            {
                continue;
            }

            onlinePlayer.InvokeRPC(typeof(MeadowRPCs).GetMethod(nameof(MeadowRPCs.RealizeInventory))!.CreateDelegate(typeof(Action<RPCEvent, OnlinePhysicalObject>)), playerOpo);
        }
    }

    public static void AbstractizeInventory(Player player)
    {
        if (!OnlinePhysicalObject.map.TryGetValue(player.abstractPhysicalObject, out var playerOpo))
        {
            return;
        }

        if (playerOpo.isMine)
        {
            return;
        }

        foreach (var onlinePlayer in OnlineManager.players)
        {
            if (onlinePlayer.isMe)
            {
                continue;
            }

            onlinePlayer.InvokeRPC(typeof(MeadowRPCs).GetMethod(nameof(MeadowRPCs.AbstractInventory))!.CreateDelegate(typeof(Action<RPCEvent, OnlinePhysicalObject>)), playerOpo);
        }
    }

    public static void AddPearlToInventory(Player player, AbstractPhysicalObject dataPearl)
    {
        if (!OnlinePhysicalObject.map.TryGetValue(player.abstractPhysicalObject, out var playerOpo))
        {
            return;
        }

        if (!OnlinePhysicalObject.map.TryGetValue(dataPearl, out var pearlOpo))
        {
            return;
        }

        if (playerOpo.isMine)
        {
            return;
        }

        foreach (var onlinePlayer in OnlineManager.players)
        {
            if (onlinePlayer.isMe)
            {
                continue;
            }

            onlinePlayer.InvokeRPC(typeof(MeadowRPCs).GetMethod(nameof(MeadowRPCs.AddPearlToInventory))!.CreateDelegate(typeof(Action<RPCEvent, OnlinePhysicalObject, OnlinePhysicalObject>)), playerOpo, pearlOpo);
        }
    }

    public static void RemovePearlFromInventory(Player player, AbstractPhysicalObject dataPearl)
    {
        if (!OnlinePhysicalObject.map.TryGetValue(player.abstractPhysicalObject, out var playerOpo))
        {
            return;
        }

        if (!OnlinePhysicalObject.map.TryGetValue(dataPearl, out var pearlOpo))
        {
            return;
        }

        if (playerOpo.isMine)
        {
            return;
        }

        foreach (var onlinePlayer in OnlineManager.players)
        {
            if (onlinePlayer.isMe)
            {
                continue;
            }

            onlinePlayer.InvokeRPC(typeof(MeadowRPCs).GetMethod(nameof(MeadowRPCs.RemovePearlFromInventory))!.CreateDelegate(typeof(Action<RPCEvent, OnlinePhysicalObject, OnlinePhysicalObject>)), playerOpo, pearlOpo);
        }
    }

    public static bool IsObjectMine(AbstractPhysicalObject abstractObject)
    {
        if (!OnlinePhysicalObject.map.TryGetValue(abstractObject, out var opo))
        {
            return true;
        }

        return opo.isMine;
    }
}
