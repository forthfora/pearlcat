using Newtonsoft.Json;
using RainMeadow;

namespace Pearlcat;

public static class MeadowRPCs
{
    [RPCMethod]
    public static void RealizePlayerPearl(RPCEvent rpcEvent, OnlinePhysicalObject playerOpo, OnlinePhysicalObject pearlOpo, bool hasEffect)
    {
        if (playerOpo.apo is not AbstractPhysicalObject absPlayer)
        {
            return;
        }

        if (absPlayer.realizedObject is not Player player)
        {
            return;
        }

        if (pearlOpo.apo is not AbstractPhysicalObject pearl)
        {
            return;
        }

        PlayerPearl_Helpers.RealizePlayerPearl_Local(player, pearl, hasEffect);
    }

    [RPCMethod]
    public static void AbstractPlayerPearl(RPCEvent rpcEvent, OnlinePhysicalObject pearlOpo, bool hasEffect)
    {
        if (pearlOpo.apo is not AbstractPhysicalObject pearl)
        {
            return;
        }

        PlayerPearl_Helpers.AbstractPlayerPearl_Local(pearl, hasEffect);
    }
}
