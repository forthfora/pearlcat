using JetBrains.Annotations;
using RainMeadow;
using UnityEngine;
using System;
using System.Linq;

namespace Pearlcat;

public class MeadowPearlcatData : OnlineEntity.EntityData
{
    [UsedImplicitly]
    public MeadowPearlcatData()
    {
    }

    public override EntityDataState MakeState(OnlineEntity entity, OnlineResource inResource)
    {
        return new State(entity);
    }

    public class State : EntityDataState
    {
        // Inventory
        [OnlineField(nullable = true)]
        public RainMeadow.Generics.DynamicOrderedEntityIDs inventory = null!;

        [OnlineField(nullable = true)]
        public RainMeadow.Generics.DynamicOrderedEntityIDs postDeathInventory = null!;

        [OnlineField]
        public int postDeathActivePearlIndex;

        [OnlineField]
        public int activePearlIndex;

        [OnlineField]
        public int currentPearlAnimation;


        // Graphics
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


        // Misc
        [OnlineField]
        public int blink;


        [UsedImplicitly]
        public State()
        {
        }

        public State(OnlineEntity onlineEntity)
        {
            var player = (Player)((OnlinePhysicalObject)onlineEntity).apo.realizedObject;

            if (!player.TryGetPearlcatModule(out var playerModule))
            {
                return;
            }

            inventory = new(playerModule.Inventory.Select(x => x?.GetOnlineObject()?.id).OfType<OnlineEntity.EntityId>().ToList());
            postDeathInventory = new(playerModule.PostDeathInventory.Select(x => x?.GetOnlineObject()?.id).OfType<OnlineEntity.EntityId>().ToList());

            activePearlIndex = playerModule.ActivePearlIndex ?? -1;
            postDeathActivePearlIndex = playerModule.PostDeathActivePearlIndex ?? -1;

            currentPearlAnimation = playerModule.CurrentPearlAnimation is null ? 0 : playerModule.PearlAnimationMap.IndexOf(playerModule.CurrentPearlAnimation.GetType());


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


            if (player.graphicsModule is PlayerGraphics graphics)
            {
                blink = graphics.blink;
            }
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


            var remotePostDeathInventory = postDeathInventory.list
                .Where(x => x.FindEntity() is OnlinePhysicalObject)
                .Select(x => ((OnlinePhysicalObject)x.FindEntity()).apo)
                .ToList();

            playerModule.PostDeathInventory = remotePostDeathInventory;


            if (activePearlIndex == -1)
            {
                playerModule.ActivePearlIndex = null;
            }
            else if (activePearlIndex != playerModule.ActivePearlIndex)
            {
                player.SetActivePearl(activePearlIndex);
            }
            playerModule.PostDeathActivePearlIndex = postDeathActivePearlIndex == -1 ? null : postDeathActivePearlIndex;


            // Pearl Animation
            if (playerModule.CurrentPearlAnimation?.GetType() != playerModule.PearlAnimationMap[currentPearlAnimation])
            {
                playerModule.CurrentPearlAnimation = (PearlAnimation)Activator.CreateInstance(playerModule.PearlAnimationMap[currentPearlAnimation], player);
            }


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


            if (player.graphicsModule is PlayerGraphics graphics)
            {
                graphics.blink = blink;
            }
        }

        public override Type GetDataType()
        {
            return typeof(MeadowPearlcatData);
        }
    }
}
