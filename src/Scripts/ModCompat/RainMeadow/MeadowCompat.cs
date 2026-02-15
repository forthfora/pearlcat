using System.Reflection;
using MonoMod.RuntimeDetour;
using RainMeadow;

namespace Pearlcat;

public static class MeadowCompat
{
    public static void InitCompat()
    {
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
                typeof(StoryGameMode).GetMethod(nameof(StoryGameMode.LoadWorldIn), BindingFlags.Instance | BindingFlags.Public),
                typeof(MeadowCompat).GetMethod(nameof(OnLoadWorldIn), BindingFlags.Static | BindingFlags.NonPublic)
            );
        }
        catch (Exception e)
        {
            e.LogHookException();
        }
        
        OnlineResource.OnAvailable += OnlineResourceOnOnAvailable;
    }


    public static bool IsHost => !IsOnline || OnlineManager.lobby.isOwner;
    public static bool IsOnline => OnlineManager.lobby is not null;
    public static bool IsOnlineFriendlyFire => RainMeadow.RainMeadow.isStoryMode(out var story) && story.friendlyFire;


    public static bool IsMine(AbstractPhysicalObject abstractPhysicalObject)
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


    public static void SetWasSaveDataSynced(bool value)
    {
        var lobby = OnlineManager.lobby;

        if (lobby is null)
        {
            return;
        }

        if (!lobby.TryGetData<MeadowSaveData>(out var saveData))
        {
            return;
        }

        saveData.WasSynced = value;
    }
    
    public static bool WasSaveDataSynced()
    {
        var lobby = OnlineManager.lobby;

        if (lobby is null)
        {
            return false;
        }

        if (!lobby.TryGetData<MeadowSaveData>(out var saveData))
        {
            return false;
        }

        return saveData.WasSynced;
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

    // Meadow world state fix
    // TODO: remove if fixed
    private static SlugcatStats.Timeline OnLoadWorldIn(Func<StoryGameMode, RainWorldGame?, SlugcatStats.Timeline> orig, StoryGameMode self, RainWorldGame? game)
    {
        if (game.IsPearlcatStory())
        {
            return SlugcatStats.Timeline.Red;
        }

        return orig(self, game);
    }

    // Raise the HUD so it doesn't obscure the active pearl
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

        self.drawpos.y += 50.0f;
    }


    // Add Online Data
    private static void OnlineResourceOnOnAvailable(OnlineResource obj)
    {
        obj.AddData(new MeadowOptionsData());
        obj.AddData(new MeadowSaveData());
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

    public static void RPC_SentryExplode(AbstractPhysicalObject sentryOwner)
    {
        var opo = sentryOwner.GetOnlineObject();

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

            onlinePlayer.InvokeRPC(typeof(MeadowRPCs).GetMethod(nameof(MeadowRPCs.SentryExplode))!.CreateDelegate(typeof(Action<RPCEvent, OnlinePhysicalObject>)), opo);
        }
    }

    public static void RPC_SentryDestroyEffect(AbstractPhysicalObject sentryOwner)
    {
        var opo = sentryOwner.GetOnlineObject();

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

            onlinePlayer.InvokeRPC(typeof(MeadowRPCs).GetMethod(nameof(MeadowRPCs.SentryDestroyEffect))!.CreateDelegate(typeof(Action<RPCEvent, OnlinePhysicalObject>)), opo);
        }
    }

    public static void RPC_SetGivenPearls_OnHost(Player player)
    {
        if (IsHost)
        {
            return;
        }

        var playerOpo = player.abstractPhysicalObject.GetOnlineObject();

        if (playerOpo is null)
        {
            return;
        }

        var owner = OnlineManager.lobby.owner;

        owner.InvokeRPC(typeof(MeadowRPCs).GetMethod(nameof(MeadowRPCs.SetGivenPearls))!.CreateDelegate(typeof(Action<RPCEvent, OnlinePhysicalObject>)), playerOpo);
    }
}
