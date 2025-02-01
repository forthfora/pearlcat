using RainMeadow;
using UnityEngine;

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

        meadowPearlcatData.InventorySaveDataNeedsUpdate = true;

        // Can try update it immediately anyways, can't hurt
        MeadowCompat.UpdateOnlineInventorySaveData(playerOpo);
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

    [RPCMethod]
    public static void ObjectConnectEffect(RPCEvent rpcEvent, OnlinePhysicalObject opo, Vector2 pos, Color color)
    {
        if (opo.apo?.realizedObject is not PhysicalObject physicalObject)
        {
            return;
        }

        physicalObject.ConnectEffect(pos, color);
    }

    [RPCMethod]
    public static void RoomConnectEffect(RPCEvent rpcEvent, RoomSession roomSession, Vector2 startPos, Vector2 targetPos, Color color, float intensity, float lifeTime)
    {
        if (roomSession.absroom?.realizedRoom is not Room room)
        {
            return;
        }

        room.ConnectEffect(startPos, targetPos, color, intensity, lifeTime);
    }
}
