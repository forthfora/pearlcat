using System.Linq;
using RainMeadow;

namespace Pearlcat;

public static class MeadowRPCs
{
    [RPCMethod]
    public static void RevivePlayer(RPCEvent rpcEvent, OnlinePhysicalObject playerOpo)
    {
        if (playerOpo.apo.realizedObject is not Player player)
        {
            return;
        }

        Player_Helpers.RevivePlayer_Local(player);
    }

    [RPCMethod]
    public static void ActivateVisualShield(RPCEvent rpcEvent, OnlinePhysicalObject playerOpo)
    {
        if (playerOpo.apo.realizedObject is not Player player)
        {
            return;
        }

        if (!player.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        playerModule.ActivateVisualShield_Local();
    }

    [RPCMethod]
    public static void UpdateInventorySaveData(RPCEvent rpcEvent, OnlinePhysicalObject playerOpo)
    {
        if (!playerOpo.TryGetData<MeadowPearlcatData>(out var meadowPearlcatData))
        {
            return;
        }

        //
        meadowPearlcatData.InventorySaveDataNeedsUpdate = true;

        if (playerOpo.apo.realizedObject is not Player player)
        {
            return;
        }

        if (!player.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        var save = player.abstractPhysicalObject.world.game.GetMiscWorld();

        if (save is null)
        {
            return;
        }

        // Can try update it immediately anyways, can't hurt
        var id = playerOpo.owner.id.GetHashCode();

        save.Inventory[id] = playerModule.Inventory.Select(x => x.ToString()).ToList();
        save.ActivePearlIndex[id] = playerModule.ActivePearlIndex;
    }

    [RPCMethod]
    public static void UpdateGivenPearlsSaveData(RPCEvent rpcEvent, OnlinePhysicalObject playerOpo)
    {
        if (playerOpo.apo.realizedObject is not Player player)
        {
            return;
        }

        var save = player.abstractPhysicalObject.world.game.GetMiscWorld();

        if (save is null)
        {
            return;
        }

        var id = playerOpo.owner.id.GetHashCode();

        if (!save.PlayersGivenPearls.Contains(id))
        {
            save.PlayersGivenPearls.Add(id);
        }
    }
}
