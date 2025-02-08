using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MonoMod.RuntimeDetour;
using RainMeadow;
using UnityEngine;

namespace Pearlcat;

public static class MeadowCompat
{
    public static void InitCompat()
    {
        try
        {
            _ = new Hook(
                typeof(OnlineResource).GetMethod("Available", BindingFlags.Instance | BindingFlags.NonPublic),
                typeof(MeadowCompat).GetMethod(nameof(OnLobbyAvailable), BindingFlags.Static | BindingFlags.NonPublic)
            );

            // TODO: use this event instead when it's been pushed
            // Lobby.ResourceAvailable
        }
        catch (Exception e)
        {
            e.LogHookException();
        }

        try
        {
            _ = new Hook(
                typeof(PlayerSpecificOnlineHud).GetMethod(nameof(PlayerSpecificOnlineHud.Update), BindingFlags.Instance | BindingFlags.Public),
                typeof(MeadowCompat).GetMethod(nameof(OnPlayerSpecificOnlineHudUpdate), BindingFlags.Static | BindingFlags.NonPublic)
            );
        }
        catch (Exception e)
        {
            e.LogHookException();
        }

        try
        {
            _ = new Hook(
                typeof(StoryGameMode).GetMethod(nameof(StoryGameMode.LoadWorldAs), BindingFlags.Instance | BindingFlags.Public),
                typeof(MeadowCompat).GetMethod(nameof(OnLoadWorldAs), BindingFlags.Static | BindingFlags.NonPublic)
            );
        }
        catch (Exception e)
        {
            e.LogHookException();
        }

        On.SlugcatStats.ctor += SlugcatStatsOnctor;
    }


    public static bool IsLobbyOwner => !IsOnline || OnlineManager.lobby.isOwner;
    public static bool IsOnline => OnlineManager.lobby is not null;
    public static bool FriendlyFire => RainMeadow.RainMeadow.isStoryMode(out var story) && story.friendlyFire;

    public static bool IsLocal(AbstractPhysicalObject abstractPhysicalObject)
    {
        return abstractPhysicalObject.IsLocal();
    }

    public static bool IsPosSynced(AbstractPhysicalObject abstractPhysicalObject)
    {
        return !abstractPhysicalObject.GetOnlineObject()?.lenientPos ?? false;
    }

    public static int? GetOwnerId(AbstractPhysicalObject abstractPhysicalObject)
    {
        var opo = abstractPhysicalObject.GetOnlineObject();

        return opo?.owner.id.GetHashCode();
    }

    public static List<AbstractCreature> GetAllPlayers()
    {
        return OnlineManager.lobby.playerAvatars.Select(kvp => (kvp.Value.FindEntity(true) as OnlinePhysicalObject)?.apo).OfType<AbstractCreature>().ToList();
    }


    public static void SetRealized(AbstractPhysicalObject abstractPhysicalObject, bool realized)
    {
        var opo = abstractPhysicalObject.GetOnlineObject();

        if (opo is null)
        {
            return;
        }

        opo.realized = realized;
    }

    public static void SetPosSynced(AbstractPhysicalObject abstractPhysicalObject, bool isPosSynced)
    {
        var opo = abstractPhysicalObject.GetOnlineObject();

        if (opo is null)
        {
            return;
        }

        opo.lenientPos = !isPosSynced;
    }

    public static void ApoEnteringWorld(AbstractPhysicalObject abstractPhysicalObject, World world)
    {
        var worldSession = world.GetResource();

        worldSession?.ApoEnteringWorld(abstractPhysicalObject);
    }


    // Meadow SlugBase food fix (TODO: remove it if it ever gets fixed)
    private static void SlugcatStatsOnctor(On.SlugcatStats.orig_ctor orig, SlugcatStats self, SlugcatStats.Name slugcat, bool malnourished)
    {
        orig(self, slugcat, malnourished);

        if (slugcat != Enums.Pearlcat)
        {
            return;
        }

        if (!RainMeadow.RainMeadow.isStoryMode(out var storyGameMode))
        {
            return;
        }

        var onlineFood = SlugcatStats.SlugcatFoodMeter(storyGameMode.currentCampaign);

        self.maxFood = onlineFood.x;
        self.foodToHibernate = onlineFood.y;
    }

    private static void OnPlayerSpecificOnlineHudUpdate(Action<PlayerSpecificOnlineHud> orig, PlayerSpecificOnlineHud self)
    {
        orig(self);

        if (self.abstractPlayer is null) // just sidestep a warning
        {
            return;
        }

        if (self.abstractPlayer.realizedCreature is not Player player)
        {
            return;
        }

        if (!player.TryGetPearlcatModule(out var playerModule))
        {
            return;
        }

        if (playerModule.ActivePearl is null)
        {
            return;
        }

        if (player.abstractCreature.Room != self.camera.room?.abstractRoom)
        {
            return;
        }

        if (!self.camera.PositionCurrentlyVisible(player.firstChunk.pos, 10.0f, true))
        {
            return;
        }

        if (!self.found)
        {
            return;
        }

        // Raise the HUD so it doesn't obscure the active pearl
        self.drawpos.y += 50.0f;
    }

    // Meadow world state fix
    private static SlugcatStats.Name OnLoadWorldAs(Func<StoryGameMode, RainWorldGame, SlugcatStats.Name> orig, StoryGameMode self, RainWorldGame game)
    {
        if (game.IsPearlcatStory())
        {
            return SlugcatStats.Name.Red;
        }

        return orig(self, game);
    }

    public static void UpdateOnlineInventorySaveData(OnlinePhysicalObject playerOpo)
    {
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

        var id = playerOpo.owner.id.GetHashCode();

        if (!ModOptions.InventoryOverride)
        {
            save.Inventory[id] = playerModule.Inventory.Select(x => x.ToString()).ToList();
        }

        save.ActiveObjectIndex[id] = playerModule.ActivePearlIndex;
    }


    // Add Online Data
    private static void OnLobbyAvailable(Action<OnlineResource> orig, OnlineResource self)
    {
        orig(self);

        self.AddData(new MeadowOptionsData());
        self.AddData(new MeadowSaveData());
    }

    public static void AddMeadowPlayerData(Player player)
    {
        var playerOpo = player.abstractPhysicalObject.GetOnlineObject();

        if (playerOpo is null)
        {
            return;
        }

        if (playerOpo.TryGetData<MeadowPearlcatData>(out _))
        {
            return;
        }

        playerOpo.AddData(new MeadowPearlcatData());
    }

    public static void AddMeadowPlayerPearlData(AbstractPhysicalObject pearl)
    {
        var pearlOpo = pearl.GetOnlineObject();

        if (pearlOpo is null)
        {
            return;
        }

        if (pearlOpo.TryGetData<MeadowPlayerPearlData>(out _))
        {
            return;
        }

        pearlOpo.AddData(new MeadowPlayerPearlData());
    }

    public static void AddMeadowPearlSpearData(AbstractPhysicalObject pearl)
    {
        var spearOpo = pearl.GetOnlineObject();

        if (spearOpo is null)
        {
            return;
        }

        if (spearOpo.TryGetData<MeadowPearlSpearData>(out _))
        {
            return;
        }

        spearOpo.AddData(new MeadowPearlSpearData());
    }


    // Remote Calls
    public static void RPC_RevivePlayer(Player player)
    {
        var playerOpo = player.abstractPhysicalObject.GetOnlineObject();

        if (playerOpo is null)
        {
            return;
        }

        foreach (var onlinePlayer in OnlineManager.players)
        {
            if (onlinePlayer.isMe)
            {
                continue;
            }

            onlinePlayer.InvokeRPC(typeof(MeadowRPCs).GetMethod(nameof(MeadowRPCs.RevivePlayer))!.CreateDelegate(typeof(Action<RPCEvent, OnlinePhysicalObject>)), playerOpo);
        }
    }

    public static void RPC_ActivateVisualShield(Player player)
    {
        var playerOpo = player.abstractPhysicalObject.GetOnlineObject();

        if (playerOpo is null)
        {
            return;
        }

        foreach (var onlinePlayer in OnlineManager.players)
        {
            if (onlinePlayer.isMe)
            {
                continue;
            }

            onlinePlayer.InvokeRPC(typeof(MeadowRPCs).GetMethod(nameof(MeadowRPCs.ActivateVisualShield))!.CreateDelegate(typeof(Action<RPCEvent, OnlinePhysicalObject>)), playerOpo);
        }
    }

    public static void RPC_UpdateInventorySaveData(Player player)
    {
        var playerOpo = player.abstractPhysicalObject.GetOnlineObject();

        if (playerOpo is null)
        {
            return;
        }

        if (IsLobbyOwner)
        {
            UpdateOnlineInventorySaveData(playerOpo);
        }
        else
        {
            var owner = OnlineManager.lobby.owner;

            owner.InvokeRPC(typeof(MeadowRPCs).GetMethod(nameof(MeadowRPCs.UpdateInventorySaveData))!.CreateDelegate(typeof(Action<RPCEvent, OnlinePhysicalObject>)), playerOpo);
        }
    }

    public static void RPC_UpdateGivenPearlsSaveData(Player player)
    {
        if (IsLobbyOwner)
        {
            return;
        }

        var playerOpo = player.abstractPhysicalObject.GetOnlineObject();

        if (playerOpo is null)
        {
            return;
        }

        var owner = OnlineManager.lobby.owner;

        owner.InvokeRPC(typeof(MeadowRPCs).GetMethod(nameof(MeadowRPCs.UpdateGivenPearlsSaveData))!.CreateDelegate(typeof(Action<RPCEvent, OnlinePhysicalObject>)), playerOpo);
    }

    public static void RPC_ObjectConnectEffect(PhysicalObject physicalObject, Vector2 pos, Color color)
    {
        var opo = physicalObject.abstractPhysicalObject.GetOnlineObject();

        if (opo is null)
        {
            return;
        }

        foreach (var onlinePlayer in OnlineManager.players)
        {
            if (onlinePlayer.isMe)
            {
                continue;
            }

            onlinePlayer.InvokeRPC(typeof(MeadowRPCs).GetMethod(nameof(MeadowRPCs.ObjectConnectEffect))!.CreateDelegate(typeof(Action<RPCEvent, OnlinePhysicalObject, Vector2, Color>)), opo, pos, color);
        }
    }

    public static void RPC_RoomConnectEffect(Room room, Vector2 startPos, Vector2 targetPos, Color color, float intensity, float lifeTime)
    {
        var roomSession = room.abstractRoom.GetResource();

        if (roomSession is null)
        {
            return;
        }

        foreach (var onlinePlayer in OnlineManager.players)
        {
            if (onlinePlayer.isMe)
            {
                continue;
            }

            onlinePlayer.InvokeRPC(typeof(MeadowRPCs).GetMethod(nameof(MeadowRPCs.RoomConnectEffect))!.CreateDelegate(typeof(Action<RPCEvent, RoomSession, Vector2, Vector2, Color, float, float>)), roomSession, startPos, targetPos, color, intensity, lifeTime, lifeTime);
        }
    }
}
