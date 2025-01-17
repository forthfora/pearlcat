using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MonoMod.RuntimeDetour;
using RainMeadow;
using SlugBase.SaveData;

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

            // use this event instead when it's been pushed
            // Lobby.ResourceAvailable
        }
        catch (Exception e)
        {
            e.LogHookException();
        }

        try
        {
            _ = new Hook(
                typeof(OnlineResource).GetMethod("ParticipantLeft", BindingFlags.Instance | BindingFlags.NonPublic),
                typeof(MeadowCompat).GetMethod(nameof(OnParticipantLeft), BindingFlags.Static | BindingFlags.NonPublic)
            );
        }
        catch (Exception e)
        {
            e.LogHookException();
        }
    }

    public static bool IsLobbyOwner => !IsOnline || OnlineManager.lobby.isOwner;
    public static bool IsOnline => OnlineManager.lobby is not null;
    public static bool FriendlyFire => RainMeadow.RainMeadow.isStoryMode(out var story) && story.friendlyFire;

    public static bool IsLocal(AbstractPhysicalObject abstractPhysicalObject)
    {
        return abstractPhysicalObject.IsLocal();
    }

    public static int GetOwnerId(AbstractPhysicalObject abstractPhysicalObject)
    {
        var opo = abstractPhysicalObject.GetOnlineObject();

        if (opo is null)
        {
            return 0;
        }

        return opo.owner.id.GetHashCode();
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

    public static List<AbstractCreature> GetAllPlayers()
    {
        return OnlineManager.lobby.playerAvatars.Select(kvp => kvp.Value.FindEntity()).Select(oe => (oe as OnlinePhysicalObject)?.apo).OfType<AbstractCreature>().ToList();
    }


    private delegate void orig_OnParticipantLeft(OnlineResource self, OnlinePlayer onlinePlayer);
    private static void OnParticipantLeft(orig_OnParticipantLeft orig, OnlineResource self, OnlinePlayer onlinePlayer)
    {
        orig(self, onlinePlayer);

        if (self.activeEntities is null)
        {
            return;
        }

        var playerPearls = self.activeEntities.OfType<OnlinePhysicalObject>().Select(x => x.apo).Where(x => x.IsPlayerPearl());

        foreach (var pearl in playerPearls)
        {
            if (pearl.TryGetPlayerPearlOwner(out var player))
            {
                var playerOpo = player.abstractPhysicalObject.GetOnlineObject();

                if (playerOpo is null)
                {
                    continue;
                }

                if (playerOpo.owner != onlinePlayer)
                {
                    continue;
                }
            }

            pearl.realizedObject?.Destroy();
            pearl.Destroy();
        }
    }

    private delegate void orig_OnLobbyAvailable(OnlineResource self);
    private static void OnLobbyAvailable(orig_OnLobbyAvailable orig, OnlineResource self)
    {
        orig(self);

        self.AddData(new MeadowOptionsData());
        self.AddData(new MeadowSaveData());
    }


    // Add Online Data
    public static void AddMeadowPlayerData(Player player)
    {
        var playerOpo = player.abstractPhysicalObject.GetOnlineObject();

        playerOpo?.AddData(new MeadowPearlcatData());
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

        spearOpo?.AddData(new MeadowPearlSpearData());
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

        owner.InvokeRPC(typeof(MeadowRPCs).GetMethod(nameof(MeadowRPCs.UpdateInventorySaveData))!.CreateDelegate(typeof(Action<RPCEvent, OnlinePhysicalObject>)), playerOpo);
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
}
