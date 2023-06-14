using SlugBase;
using SlugBase.Features;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vector2 = UnityEngine.Vector2;

namespace Pearlcat;

public static partial class Hooks
{
    public static readonly ConditionalWeakTable<Player, PearlcatModule> PearlcatData = new();


    public const float ShortcutColorIncrement = 0.003f;

    public static readonly PlayerFeature<Vector2> ActiveObjectOffset = new("active_object_offset", Vector2Feature);



    public static bool IsPearlcat(this Player player) => player.SlugCatClass == Enums.Slugcat.Pearlcat;

    // Only pearlcats get this module
    public static bool TryGetPearlcatModule(this Player player, out PearlcatModule pearlcatModule)
    {
        if (!player.IsPearlcat())
        {
            pearlcatModule = null!;
            return false;
        }

        if (!PearlcatData.TryGetValue(player, out pearlcatModule))
        {
            pearlcatModule = new PearlcatModule(player);
            PearlcatData.Add(player, pearlcatModule);
        }

        return true;
    }

    public static List<PearlcatModule> GetAllPlayerData(this RainWorldGame game)
    {
        List<PearlcatModule> allPlayerData = new();
        List<AbstractCreature> players = game.Players;

        if (players == null)
            return allPlayerData;

        foreach (AbstractCreature creature in players)
        {
            if (creature.realizedCreature == null) continue;

            if (creature.realizedCreature is not Player player) continue;

            if (!PearlcatData.TryGetValue(player, out PearlcatModule playerModule)) continue;

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
