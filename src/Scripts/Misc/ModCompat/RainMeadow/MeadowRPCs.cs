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

    [RPCMethod]
    public static void AddPearlToInventory(RPCEvent rpcEvent, OnlinePhysicalObject playerOpo, OnlinePhysicalObject pearlOpo, bool addToEnd, bool storeBeforeActive)
    {
        if (playerOpo.apo is not AbstractPhysicalObject absPlayer)
        {
            return;
        }

        if (pearlOpo.apo is not AbstractPhysicalObject pearl)
        {
            return;
        }

        if (absPlayer.realizedObject is not Player player)
        {
            return;
        }

        PlayerPearl_Helpers.AddToInventory_Local(player, pearl, addToEnd, storeBeforeActive);
    }

    [RPCMethod]
    public static void RemovePearlFromInventory(RPCEvent rpcEvent, OnlinePhysicalObject playerOpo, OnlinePhysicalObject pearlOpo)
    {
        if (playerOpo.apo is not AbstractPhysicalObject absPlayer)
        {
            return;
        }

        if (pearlOpo.apo is not DataPearl.AbstractDataPearl pearl)
        {
            return;
        }

        if (absPlayer.realizedObject is not Player player)
        {
            return;
        }

        PlayerPearl_Helpers.RemoveFromInventory_Local(player, pearl);
    }

    [RPCMethod]
    public static void ReceivePearlcatInput(RPCEvent rpcEvent, OnlinePhysicalObject playerOpo, string inputString)
    {
        if (playerOpo.apo is not AbstractPhysicalObject absPlayer)
        {
            return;
        }

        if (absPlayer.realizedObject is not Player player)
        {
            return;
        }

        if (!player.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        var meadowInput = JsonConvert.DeserializeObject<PlayerModule.MeadowInputData>(inputString);

        playerModule.MeadowInput = meadowInput!;
    }

    [RPCMethod]
    public static void PickObjectAnimation(RPCEvent rpcEvent, OnlinePhysicalObject playerOpo, int randomSeed)
    {
        if (playerOpo.apo is not AbstractPhysicalObject absPlayer)
        {
            return;
        }

        if (absPlayer.realizedObject is not Player player)
        {
            return;
        }

        if (!player.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        playerModule.PickObjectAnimation_Local(player, randomSeed);
    }
}
