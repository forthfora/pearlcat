using System;
using Newtonsoft.Json;
using RainMeadow;

namespace Pearlcat;

public static class ModCompat_RainMeadow_Helpers
{
    public static bool IsOwner => OnlineManager.lobby.isOwner;

    public static bool IsLocal(AbstractPhysicalObject abstractPhysicalObject)
    {
        return abstractPhysicalObject.IsLocal();
    }


    // Remote Calls
    public static void RPC_RealizePlayerPearl(Player player, AbstractPhysicalObject pearl, bool hasEffect)
    {
        var playerOpo = player.abstractPhysicalObject.GetOnlineObject();

        if (playerOpo is null)
        {
            return;
        }

        var pearlOpo = pearl.GetOnlineObject();

        if (pearlOpo is null)
        {
            return;
        }

        foreach (var onlinePlayer in OnlineManager.players)
        {
            if (onlinePlayer.isMe)
            {
                continue;
            }

            onlinePlayer.InvokeRPC(typeof(MeadowRPCs).GetMethod(nameof(MeadowRPCs.RealizePlayerPearl))!.CreateDelegate(typeof(Action<RPCEvent, OnlinePhysicalObject, OnlinePhysicalObject, bool>)), playerOpo, pearlOpo, hasEffect);
        }
    }

    public static void RPC_AbstractPlayerPearl(AbstractPhysicalObject pearl, bool hasEffect)
    {
        var pearlOpo = pearl.GetOnlineObject();

        if (pearlOpo is null)
        {
            return;
        }

        foreach (var onlinePlayer in OnlineManager.players)
        {
            if (onlinePlayer.isMe)
            {
                continue;
            }

            onlinePlayer.InvokeRPC(typeof(MeadowRPCs).GetMethod(nameof(MeadowRPCs.AbstractPlayerPearl))!.CreateDelegate(typeof(Action<RPCEvent, OnlinePhysicalObject, bool>)), pearlOpo, hasEffect);
        }
    }

    public static void RPC_AddPearlToInventory(Player player, AbstractPhysicalObject pearl, bool addToEnd, bool storeBeforeActive)
    {
        var playerOpo = player.abstractPhysicalObject.GetOnlineObject();

        if (playerOpo is null)
        {
            Plugin.Logger.LogWarning("C");
            return;
        }

        var pearlOpo = pearl.GetOnlineObject();

        if (pearlOpo is null)
        {
            Plugin.Logger.LogWarning("A");
            var room = player.abstractPhysicalObject.Room;

            room.world.GetResource()?.ApoEnteringWorld(pearl);
            room.GetResource()?.ApoEnteringRoom(pearl, player.abstractPhysicalObject.pos);

            pearlOpo = pearl.GetOnlineObject();

            if (pearlOpo is null)
            {
                Plugin.Logger.LogWarning("B");
                return;
            }
        }

        foreach (var onlinePlayer in OnlineManager.players)
        {
            if (onlinePlayer.isMe)
            {
                continue;
            }

            onlinePlayer.InvokeRPC(typeof(MeadowRPCs).GetMethod(nameof(MeadowRPCs.AddPearlToInventory))!.CreateDelegate(typeof(Action<RPCEvent, OnlinePhysicalObject, OnlinePhysicalObject, bool, bool>)), playerOpo, pearlOpo, addToEnd, storeBeforeActive);
        }
    }

    public static void RPC_RemovePearlFromInventory(Player player, AbstractPhysicalObject pearl)
    {
        var playerOpo = player.abstractPhysicalObject.GetOnlineObject();

        if (playerOpo is null)
        {
            return;
        }

        var pearlOpo = pearl.GetOnlineObject();

        if (pearlOpo is null)
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
    public static void RPC_ReceivePearlcatInput(Player player, PlayerModule playerModule)
    {
        var playerOpo = player.abstractPhysicalObject.GetOnlineObject();

        if (playerOpo is null)
        {
            return;
        }

        foreach (var onlinePlayer in OnlineManager.players)
        {
            if (onlinePlayer.isMe)
            {
                continue;
            }

            var inputString = JsonConvert.SerializeObject(playerModule.MeadowInput);

            onlinePlayer.InvokeRPC(typeof(MeadowRPCs).GetMethod(nameof(MeadowRPCs.ReceivePearlcatInput))!.CreateDelegate(typeof(Action<RPCEvent, OnlinePhysicalObject, string>)), playerOpo, inputString);
        }
    }

    public static void RPC_PickObjectAnimation(Player player, int randomSeed)
    {
        var playerOpo = player.abstractPhysicalObject.GetOnlineObject();

        if (playerOpo is null)
        {
            return;
        }

        foreach (var onlinePlayer in OnlineManager.players)
        {
            if (onlinePlayer.isMe)
            {
                continue;
            }

            onlinePlayer.InvokeRPC(typeof(MeadowRPCs).GetMethod(nameof(MeadowRPCs.PickObjectAnimation))!.CreateDelegate(typeof(Action<RPCEvent, OnlinePhysicalObject, int>)), playerOpo, randomSeed);
        }
    }
}
