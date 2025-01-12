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

        pearlOpo.apo.InDen = false;

        PlayerPearl_Helpers.RealizePlayerPearl_Local(player, pearl, hasEffect);
    }

    [RPCMethod]
    public static void AbstractPlayerPearl(RPCEvent rpcEvent, OnlinePhysicalObject pearlOpo, bool hasEffect)
    {
        if (pearlOpo.apo is not AbstractPhysicalObject pearl)
        {
            return;
        }

        pearlOpo.apo.InDen = true;

        PlayerPearl_Helpers.AbstractPlayerPearl_Local(pearl, hasEffect);
    }

    [RPCMethod]
    public static void DeploySentry(RPCEvent rpcEvent, OnlinePhysicalObject playerOpo, OnlinePhysicalObject pearlOpo)
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

        PlayerAbilities_Helpers.DeploySentry_Local(player, pearl);
    }

    [RPCMethod]
    public static void RemoveSentry(RPCEvent rpcEvent, OnlinePhysicalObject pearlOpo)
    {
        if (pearlOpo.apo is not AbstractPhysicalObject pearl)
        {
            return;
        }

        if (!pearl.TryGetPlayerPearlModule(out var module))
        {
            return;
        }

        module.RemoveSentry_Local(pearl);
    }
}
