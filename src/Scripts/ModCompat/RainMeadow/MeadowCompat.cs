using System;
using System.Collections.Generic;
using System.Linq;
using RainMeadow;

namespace Pearlcat;

public static class MeadowCompat
{
    public static bool IsOnline => OnlineManager.lobby is not null;

    public static bool IsLocal(AbstractPhysicalObject abstractPhysicalObject)
    {
        return abstractPhysicalObject.IsLocal();
    }

    public static void SetRealized(AbstractPhysicalObject abstractPhysicalObject, bool realized)
    {
        var opo = abstractPhysicalObject.GetOnlineObject();

        if (opo is null)
        {
            return;
        }

        opo.realized = realized;
    }

    public static List<AbstractCreature> GetAllPlayers()
    {
        return OnlineManager.lobby.playerAvatars.Select(kvp => kvp.Value.FindEntity()).Select(oe => (oe as OnlinePhysicalObject)?.apo).OfType<AbstractCreature>().ToList();
    }

    public static void AddMeadowPlayerData(Player player)
    {
        var playerOpo = player.abstractPhysicalObject.GetOnlineObject();

        playerOpo?.AddData(new MeadowPearlcatData());
    }

    public static void AddMeadowPlayerPearlData(AbstractPhysicalObject pearl)
    {
        var pearlOpo = pearl.GetOnlineObject();

        if (pearlOpo is null)
        {
            return;
        }

        if (pearlOpo.TryGetData<MeadowPlayerPearlData>(out _))
        {
            return;
        }

        pearlOpo.AddData(new MeadowPlayerPearlData());
    }


    // Remote Calls
    public static void RPC_SyncModOptions()
    {
        var modOptionsJson = "";

        foreach (var onlinePlayer in OnlineManager.players)
        {
            if (onlinePlayer.isMe)
            {
                continue;
            }

            onlinePlayer.InvokeRPC(typeof(MeadowRPCs).GetMethod(nameof(MeadowRPCs.ReceiveModOptions))!.CreateDelegate(typeof(Action<RPCEvent, string>)), modOptionsJson);
        }
    }
}
