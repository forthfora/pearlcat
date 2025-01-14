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

    [RPCMethod]
    public static void ActivateVisualShield(RPCEvent rpcEvent, OnlinePhysicalObject playerOpo)
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
}
