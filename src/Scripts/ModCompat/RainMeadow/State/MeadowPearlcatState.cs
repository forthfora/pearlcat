using System;
using System.Linq;
using JetBrains.Annotations;
using RainMeadow;

namespace Pearlcat;

public class MeadowPearlcatState : OnlineEntity.EntityData.EntityDataState
{
    [OnlineField(nullable = true)]
    public RainMeadow.Generics.DynamicOrderedEntityIDs playerPearls = null!;

    [OnlineField]
    public int activeObjectIndex;

    [OnlineField]
    public byte remoteInput;

    [OnlineField]
    public int currentPearlAnimation;


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

        // Ownership goes to the player who's storing the pearl
        foreach (var pearl in playerModule.Inventory)
        {
            var onlinePearl = pearl.GetOnlineObject();

            if (onlinePearl?.owner is null)
            {
                continue;
            }

            if (onlinePearl.owner != onlineEntity.owner)
            {
                onlinePearl.NewOwner(onlineEntity.owner);
            }

        }

        activeObjectIndex = playerModule.ActiveObjectIndex ?? -1;

        currentPearlAnimation = playerModule.CurrentPearlAnimation is null ? -1 : playerModule.PearlAnimationMap.IndexOf(playerModule.CurrentPearlAnimation.GetType());

        remoteInput = playerModule.RemoteInput.ToByte();

        // Plugin.Logger.LogWarning($"Owner ID: {onlineEntity.owner.id}");
        // Plugin.Logger.LogWarning($"Active Object Index: {activeObjectIndex}");
        // Plugin.Logger.LogWarning($"Pearl Animation: {activeObjectIndex}");
        // Plugin.Logger.LogWarning("Remote Input: " + string.Join(" ", remoteInput.ByteToBools().Select(x => x ? "1" : "0")));
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

        var pearlsToAdd = remoteInventory.Where(x => !localInventory.Contains(x));
        var pearlsToRemove = localInventory.Where(x => !remoteInventory.Contains(x));

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

        if (currentPearlAnimation == -1)
        {
            playerModule.CurrentPearlAnimation = null;
        }
        else if (playerModule.CurrentPearlAnimation?.GetType() != playerModule.PearlAnimationMap[currentPearlAnimation])
        {
            playerModule.CurrentPearlAnimation = (PearlAnimation)Activator.CreateInstance(playerModule.PearlAnimationMap[currentPearlAnimation], player);
        }
    }

    public override Type GetDataType()
    {
        return typeof(MeadowPearlcatData);
    }
}
