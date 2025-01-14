using RainMeadow;

namespace Pearlcat;

public static class MeadowRPCs
{
    [RPCMethod]
    public static void RevivePlayer(RPCEvent rpcEvent, OnlinePhysicalObject playerOpo)
    {
        if (playerOpo?.apo?.realizedObject is not Player player)
        {
            return;
        }

        Player_Helpers.RevivePlayer_Local(player);
    }
}
