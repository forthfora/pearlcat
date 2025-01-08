using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RainMeadow;

namespace Pearlcat;

public static class MeadowRPCs
{
    [RPCMethod]
    public static void RealizeInventory(RPCEvent rpcEvent, OnlinePhysicalObject playerOpo)
    {
        if (playerOpo.apo is not AbstractPhysicalObject apo)
        {
            return;
        }

        if (apo.realizedObject is not Player player)
        {
            return;
        }

        if (!player.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        player.TryRealizeInventory(playerModule);

        Plugin.Logger.LogInfo("Meadow: Realized pearlcat inventory");
    }

    [RPCMethod]
    public static void AbstractInventory(RPCEvent rpcEvent, OnlinePhysicalObject playerOpo)
    {
        if (playerOpo.apo is not AbstractPhysicalObject apo)
        {
            return;
        }

        if (apo.realizedObject is not Player player)
        {
            return;
        }

        player.AbstractizeInventory();

        Plugin.Logger.LogInfo("Meadow: Abstracted pearlcat inventory");
    }

    [RPCMethod]
    public static void AddPearlToInventory(RPCEvent rpcEvent, OnlinePhysicalObject playerOpo, OnlinePhysicalObject pearlOpo)
    {
        if (playerOpo.apo is not AbstractPhysicalObject apo)
        {
            return;
        }

        if (pearlOpo.apo is not DataPearl.AbstractDataPearl adp)
        {
            return;
        }

        if (apo.realizedObject is not Player player)
        {
            return;
        }

        player.AddToInventory(adp);

        Plugin.Logger.LogInfo("Meadow: Added pearl to inventory");
    }

    [RPCMethod]
    public static void RemovePearlFromInventory(RPCEvent rpcEvent, OnlinePhysicalObject playerOpo, OnlinePhysicalObject pearlOpo)
    {
        if (playerOpo.apo is not AbstractPhysicalObject apo)
        {
            return;
        }

        if (pearlOpo.apo is not DataPearl.AbstractDataPearl adp)
        {
            return;
        }

        if (apo.realizedObject is not Player player)
        {
            return;
        }

        player.RemoveFromInventory(adp);

        Plugin.Logger.LogInfo("Meadow: Removed pearl from inventory");
    }

    [RPCMethod]
    public static void ReceivePearlcatInput(RPCEvent rpcEvent, OnlinePhysicalObject playerOpo, string inputString)
    {
        if (playerOpo.apo is not AbstractPhysicalObject apo)
        {
            return;
        }

        if (apo.realizedObject is not Player player)
        {
            return;
        }

        if (!player.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        var inputData = JsonConvert.DeserializeObject<PlayerModule.MeadowInputData>(inputString);

        playerModule.MeadowInput = inputData!;
    }
}
