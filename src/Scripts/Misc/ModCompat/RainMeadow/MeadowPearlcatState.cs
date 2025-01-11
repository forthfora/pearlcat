using System;
using System.Linq;
using JetBrains.Annotations;
using RainMeadow;

namespace Pearlcat;

public class MeadowPearlcatState : OnlineEntity.EntityData.EntityDataState
{
    [OnlineField]
    public RainMeadow.Generics.DynamicOrderedEntityIDs playerPearls = null!;

    [OnlineField]
    public int activeObjectIndex;

    [OnlineField]
    public byte remoteInput;

    [UsedImplicitly]
    public MeadowPearlcatState()
    {
    }

    public MeadowPearlcatState(MeadowPearlcatData data, OnlineEntity onlineEntity)
    {
        var player = (Player)((OnlinePhysicalObject)onlineEntity).apo.realizedObject;

        if (!player.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        playerPearls = new(playerModule.Inventory.Select(x => x?.GetOnlineObject()?.id).OfType<OnlineEntity.EntityId>().ToList());

        activeObjectIndex = playerModule.ActiveObjectIndex ?? -1;

        remoteInput = playerModule.RemoteInput.ToByte();
    }

    public override void ReadTo(OnlineEntity.EntityData data, OnlineEntity onlineEntity)
    {
        var player = (Player)((OnlinePhysicalObject)onlineEntity).apo.realizedObject;

        if (!player.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        // Compare local and remote inventory, call AddToInventory / RemoveFromInventory where appropriate to sync local to remote
        var remoteInventory = playerPearls.list
            .Where(x => x.FindEntity() is OnlinePhysicalObject)
            .Select(x => ((OnlinePhysicalObject)x.FindEntity()).apo)
            .ToList();

        var localInventory = playerModule.Inventory;

        var pearlsToAdd = remoteInventory.Where(x => localInventory.Contains(x));
        var pearlsToRemove = localInventory.Where(x => remoteInventory.Contains(x));

        foreach (var pearl in pearlsToAdd)
        {
            player.AddToInventory(pearl);
        }

        foreach (var pearl in pearlsToRemove)
        {
            player.RemoveFromInventory(pearl);
        }

        playerModule.Inventory = remoteInventory;

        playerModule.ActiveObjectIndex = activeObjectIndex == -1 ? null : activeObjectIndex;

        playerModule.RemoteInput.FromByte(remoteInput);
    }

    public override Type GetDataType()
    {
        return typeof(MeadowPearlcatData);
    }
}
