using SlugBase;
using SlugBase.Features;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vector2 = UnityEngine.Vector2;

namespace Pearlcat;

public static partial class Hooks
{
    public static readonly ConditionalWeakTable<Player, PlayerModule> PearlcatData = new();


    private const float FrameShortcutColorAddition = 0.003f;

    public static readonly PlayerFeature<Vector2> ActiveObjectOffset = new("active_object_offset", Vector2Feature);


    private static bool IsPearlcat(this Player player) => player.SlugCatClass == Enums.Slugcat.Pearlcat;

    private static List<PlayerModule> GetAllPlayerData(this RainWorldGame game)
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


    // Feature Factories
    public static Vector2 Vector2Feature(JsonAny json)
    {
        JsonList jsonList = json.AsList();
        return new Vector2(jsonList[0].AsFloat(), jsonList[1].AsFloat());
    }
}
