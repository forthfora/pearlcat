using System;
using RainMeadow;

namespace Pearlcat;

public static class MeadowCompat
{
    public static bool IsMeadowGameMode => OnlineManager.lobby is not null && OnlineManager.lobby.gameMode is MeadowGameMode;

    public static bool IsLocal(AbstractPhysicalObject abstractPhysicalObject)
    {
        return abstractPhysicalObject.IsLocal();
    }

    public static void InitMeadowPearlcatData(Player player)
    {
        var playerOpo = player.abstractPhysicalObject.GetOnlineObject();

        playerOpo?.AddData(new MeadowPearlcatData());
    }

    public static void InitMeadowPlayerPearlData(AbstractPhysicalObject pearl)
    {
        var pearlOpo = pearl.GetOnlineObject();

        pearlOpo?.AddData(new MeadowPlayerPearlData());
    }

    public static void SetMeadowColors(Player player, PlayerModule playerModule)
    {
        var playerOpo = player.abstractPhysicalObject.GetOnlineObject();

        if (playerOpo is null)
        {
            return;
        }

        if (!playerOpo.TryGetData<SlugcatCustomization>(out var customization))
        {
            return;
        }

        // TODO
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

    public static void RPC_DeploySentry(Player player, AbstractPhysicalObject pearl)
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

            onlinePlayer.InvokeRPC(typeof(MeadowRPCs).GetMethod(nameof(MeadowRPCs.DeploySentry))!.CreateDelegate(typeof(Action<RPCEvent, OnlinePhysicalObject, OnlinePhysicalObject>)), playerOpo, pearlOpo);
        }
    }

    public static void RPC_RemoveSentry(AbstractPhysicalObject pearl)
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

            onlinePlayer.InvokeRPC(typeof(MeadowRPCs).GetMethod(nameof(MeadowRPCs.RemoveSentry))!.CreateDelegate(typeof(Action<RPCEvent, OnlinePhysicalObject>)), pearlOpo);
        }
    }
}
