using SlugBase;
using SlugBase.Features;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vector2 = UnityEngine.Vector2;

namespace Pearlcat;

public static partial class Hooks
{
    public static readonly ConditionalWeakTable<Player, PlayerModule> PearlcatData = new();


    public const float ShortcutColorIncrement = 0.003f;

    public static readonly PlayerFeature<Vector2> ActiveObjectOffset = new("active_object_offset", Vector2Feature);
    public static readonly PlayerFeature<int> DazeDuration = FeatureTypes.PlayerInt("daze_duration");

    public static readonly PlayerFeature<int> MinOATime = FeatureTypes.PlayerInt("oa_min_time");
    public static readonly PlayerFeature<int> MaxOATime = FeatureTypes.PlayerInt("oa_max_time");


    // OA MoveToTargetPos
    // Slow Down
    public static readonly PlayerFeature<float> MinFricSpeed = FeatureTypes.PlayerFloat("oa_min_fric_speed");
    public static readonly PlayerFeature<float> MaxFricSpeed = FeatureTypes.PlayerFloat("oa_max_fric_speed");
    public static readonly PlayerFeature<float> MinFric = FeatureTypes.PlayerFloat("oa_min_fric_mult");
    public static readonly PlayerFeature<float> MaxFric = FeatureTypes.PlayerFloat("oa_max_fric_mult");

    // Move to Target
    public static readonly PlayerFeature<float> CutoffDist = FeatureTypes.PlayerFloat("oa_cutoff_dist");
    public static readonly PlayerFeature<float> CutoffMinSpeed = FeatureTypes.PlayerFloat("oa_cutoff_min_speed");
    public static readonly PlayerFeature<float> CutoffMaxSpeed = FeatureTypes.PlayerFloat("oa_cutoff_max_speed");
    public static readonly PlayerFeature<float> DazeMaxSpeed = FeatureTypes.PlayerFloat("oa_daze_max_speed");

    public static readonly PlayerFeature<float> MaxDist = FeatureTypes.PlayerFloat("oa_max_dist");
    public static readonly PlayerFeature<float> MinSpeed = FeatureTypes.PlayerFloat("oa_min_speed");
    public static readonly PlayerFeature<float> MaxSpeed = FeatureTypes.PlayerFloat("oa_max_speed");

    public static readonly PlayerFeature<Vector2> InventoryUIOffset = new("inventory_ui_offset", Vector2Feature);
    public static readonly PlayerFeature<int> SwapRepeatInterval = FeatureTypes.PlayerInt("swap_repeat_interval");

    public static readonly PlayerFeature<int> StoreObjectDelay = FeatureTypes.PlayerInt("store_object_delay");


    public static bool IsPearlcat(this Player player) => player.SlugCatClass == Enums.General.Pearlcat;

    public static bool IsFirstPearlcat(this Player player) => player.playerState.playerNumber == GetFirstPearlcatIndex(player.room?.game);

    public static bool IsPearlcatCampaign(this RainWorldGame game) => game.StoryCharacter == Enums.General.Pearlcat;

    // Only pearlcats get this module
    public static bool TryGetPearlcatModule(this Player player, out PlayerModule playerModule)
    {
        if (!player.IsPearlcat())
        {
            playerModule = null!;
            return false;
        }

        if (!PearlcatData.TryGetValue(player, out playerModule))
        {
            playerModule = new PlayerModule(player);
            PearlcatData.Add(player, playerModule);

            playerModule.LoadSaveData(player);
            playerModule.PickObjectAnimation(player);
        }

        return true;
    }

    public static List<PlayerModule> GetAllPlayerData(this RainWorldGame game)
    {
        List<PlayerModule> allPlayerData = new();
        List<AbstractCreature> players = game.Players;

        if (players == null)
            return allPlayerData;

        foreach (AbstractCreature creature in players)
        {
            if (creature.realizedCreature == null) continue;

            if (creature.realizedCreature is not Player player) continue;

            if (!PearlcatData.TryGetValue(player, out PlayerModule playerModule)) continue;

            allPlayerData.Add(playerModule);
        }

        return allPlayerData;
    }

    public static int GetFirstPearlcatIndex(this RainWorldGame? game)
    {
        if (game == null)
            return -1;

        for (int i = 0; i < game.Players.Count; i++)
        {
            AbstractCreature? abstractCreature = game.Players[i];
            if (abstractCreature.realizedCreature is not Player player) continue;

            if (player.IsPearlcat())
                return i; 
        }

        return -1;
    }



    public static Vector2 Vector2Feature(JsonAny json)
    {
        var jsonList = json.AsList();
        return new(jsonList[0].AsFloat(), jsonList[1].AsFloat());
    }
}
