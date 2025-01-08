using System;
using RainMeadow;

namespace Pearlcat;

public static class ModCompat_RainMeadow_Helpers
{
    public static bool IsOwner => OnlineManager.lobby.isOwner;

    public static bool IsMeadowInput(Player player)
    {
        if (!OnlinePhysicalObject.map.TryGetValue(player.abstractPhysicalObject, out var opo))
        {
            return false;
        }

        return !opo.isMine;
    }


    // Remote Calls
    public static void RPC_RealizeInventory(Player player)
    {
        if (!OnlinePhysicalObject.map.TryGetValue(player.abstractPhysicalObject, out var playerOpo))
        {
            return;
        }

        if (!player.abstractPhysicalObject.IsLocal())
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

    public static void RPC_AbstractizeInventory(Player player)
    {
        if (!OnlinePhysicalObject.map.TryGetValue(player.abstractPhysicalObject, out var playerOpo))
        {
            return;
        }

        if (!player.abstractPhysicalObject.IsLocal())
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

    public static void RPC_AddPearlToInventory(Player player, AbstractPhysicalObject dataPearl)
    {
        if (!OnlinePhysicalObject.map.TryGetValue(player.abstractPhysicalObject, out var playerOpo))
        {
            return;
        }

        if (!player.abstractPhysicalObject.IsLocal())
        {
            return;
        }

        if (!OnlinePhysicalObject.map.TryGetValue(dataPearl, out var pearlOpo))
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

    public static void RPC_RemovePearlFromInventory(Player player, AbstractPhysicalObject dataPearl)
    {
        if (!OnlinePhysicalObject.map.TryGetValue(player.abstractPhysicalObject, out var playerOpo))
        {
            return;
        }

        if (!player.abstractPhysicalObject.IsLocal())
        {
            return;
        }

        if (!OnlinePhysicalObject.map.TryGetValue(dataPearl, out var pearlOpo))
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

    public static void RPC_LoadSaveData(Player player)
    {
        if (!OnlinePhysicalObject.map.TryGetValue(player.abstractPhysicalObject, out var playerOpo))
        {
            return;
        }

        if (!player.abstractPhysicalObject.IsLocal())
        {
            return;
        }

        foreach (var onlinePlayer in OnlineManager.players)
        {
            if (onlinePlayer.isMe)
            {
                continue;
            }

            onlinePlayer.InvokeRPC(typeof(MeadowRPCs).GetMethod(nameof(MeadowRPCs.LoadSaveData))!.CreateDelegate(typeof(Action<RPCEvent, OnlinePhysicalObject>)), playerOpo);
        }
    }
}
