using System;
using System.Linq;
using JetBrains.Annotations;
using RainMeadow;
using UnityEngine;

namespace Pearlcat;

public class MeadowPearlcatState : OnlineEntity.EntityData.EntityDataState
{
    // Inventory & Input
    [OnlineField(nullable = true)]
    public RainMeadow.Generics.DynamicOrderedEntityIDs inventory = null!;

    [OnlineField]
    public int activeObjectIndex;

    [OnlineField]
    public int currentPearlAnimation;

    [OnlineField]
    public byte remoteInput;


    // Colors
    [OnlineField]
    public Color baseBodyColor;

    [OnlineField]
    public Color baseAccentColor;

    [OnlineField]
    public Color baseCloakColor;

    [OnlineField]
    public Color baseFaceColor;


    // Abilities
    [OnlineField]
    public int flyTimer;

    [OnlineField]
    public int groundedTimer;

    [OnlineField]
    public int dazeTimer;

    [OnlineField]
    public int reviveTimer;

    [OnlineField]
    public int shieldTimer;

    [OnlineField]
    public int spearTimer;

    [OnlineField]
    public int agilityOveruseTimer;


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

        inventory = new(playerModule.Inventory.Select(x => x?.GetOnlineObject()?.id).OfType<OnlineEntity.EntityId>().ToList());

        activeObjectIndex = playerModule.ActiveObjectIndex ?? -1;

        currentPearlAnimation = playerModule.CurrentPearlAnimation is null ? 0 : playerModule.PearlAnimationMap.IndexOf(playerModule.CurrentPearlAnimation.GetType());

        remoteInput = playerModule.RemoteInput.ToByte();


        baseBodyColor = playerModule.BaseBodyColor;
        baseAccentColor = playerModule.BaseAccentColor;
        baseCloakColor = playerModule.BaseCloakColor;
        baseFaceColor = playerModule.BaseFaceColor;


        flyTimer = playerModule.FlyTimer;
        groundedTimer = playerModule.GroundedTimer;
        dazeTimer = playerModule.DazeTimer;
        reviveTimer = playerModule.ReviveTimer;
        shieldTimer = playerModule.ShieldTimer;
        spearTimer = playerModule.SpearTimer;
        agilityOveruseTimer = playerModule.AgilityOveruseTimer;

        // TODO: try and avoid this mess
        // Ownership goes to the player who's storing the pearl
        // foreach (var pearl in playerModule.Inventory)
        // {
        //     var onlinePearl = pearl.GetOnlineObject();
        //
        //     if (onlinePearl is null)
        //     {
        //         continue;
        //     }
        //
        //     // Pearlcat who's storing the pearl
        //     var shouldBeOwner = onlineEntity.owner;
        //
        //     if (onlinePearl.owner == onlineEntity.owner)
        //     {
        //         continue;
        //     }
        //
        //     onlinePearl.NewOwner(shouldBeOwner);
        // }


        // Plugin.Logger.LogWarning("MEADOW PEARLCAT STATE SENDER: ");
        //
        // Plugin.Logger.LogWarning("Inventory: ");
        // foreach (var item in playerModule.Inventory)
        // {
        //     Plugin.Logger.LogWarning((item as DataPearl.AbstractDataPearl)?.dataPearlType.value ?? "[Invalid Pearl]");
        // }
        //
        // Plugin.Logger.LogWarning($"Input: {string.Join(" ", remoteInput.ByteToBools().Select(x => x ? 1 : 0))}");
        // Plugin.Logger.LogWarning($"Active Object Index: {activeObjectIndex}");
        // Plugin.Logger.LogWarning($"Current Pearl Animation: {currentPearlAnimation}");
    }

    public override void ReadTo(OnlineEntity.EntityData data, OnlineEntity onlineEntity)
    {
        var player = (Player)((OnlinePhysicalObject)onlineEntity).apo.realizedObject;

        if (!player.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        // Compare local and remote inventory, call AddToInventory / RemoveFromInventory where appropriate to sync local to remote
        var remoteInventory = inventory.list
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


        // Pearl Animation
        if (playerModule.CurrentPearlAnimation?.GetType() != playerModule.PearlAnimationMap[currentPearlAnimation])
        {
            playerModule.CurrentPearlAnimation = (PearlAnimation)Activator.CreateInstance(playerModule.PearlAnimationMap[currentPearlAnimation], player);
        }


        // Input
        playerModule.RemoteInput.FromByte(remoteInput);


        playerModule.BaseBodyColor = baseBodyColor;
        playerModule.BaseAccentColor = baseAccentColor;
        playerModule.BaseCloakColor = baseCloakColor;
        playerModule.BaseFaceColor = baseFaceColor;


        playerModule.FlyTimer = flyTimer;
        playerModule.GroundedTimer = groundedTimer;
        playerModule.DazeTimer = dazeTimer;
        playerModule.ReviveTimer = reviveTimer;
        playerModule.ShieldTimer = shieldTimer;
        playerModule.SpearTimer = spearTimer;
        playerModule.AgilityOveruseTimer = agilityOveruseTimer;


        // Plugin.Logger.LogWarning("MEADOW PEARLCAT STATE RECEIVER: ");
        //
        // Plugin.Logger.LogWarning("Inventory: ");
        // foreach (var item in remoteInventory)
        // {
        //     Plugin.Logger.LogWarning((item as DataPearl.AbstractDataPearl)?.dataPearlType.value ?? "[Invalid Pearl]");
        // }
        //
        // Plugin.Logger.LogWarning($"Input: {string.Join(" ", remoteInput.ByteToBools().Select(x => x ? 1 : 0))}");
        // Plugin.Logger.LogWarning($"Active Object Index: {activeObjectIndex}");
        // Plugin.Logger.LogWarning($"Current Pearl Animation: {currentPearlAnimation}");
    }

    public override Type GetDataType()
    {
        return typeof(MeadowPearlcatData);
    }
}
