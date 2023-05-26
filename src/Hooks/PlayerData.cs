using SlugBase;
using SlugBase.Features;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vector2 = UnityEngine.Vector2;

namespace TheSacrifice;

public static partial class Hooks
{
    public static readonly ConditionalWeakTable<Player, PlayerModule> PlayerData = new();


    // Constant Features
    private const float FrameShortcutColorAddition = 0.003f;

    private const int FramesToStoreObject = 80;
    private const int FramesToRetrieveObject = 80;


    // Generates a unique texture ID so that the atlases don't overlap
    private static int _textureID = 0;
    public static int TextureID => _textureID++;



    private static bool IsCustomSlugcat(Player player) => player.SlugCatClass == Enums.Slugcat.Sacrifice;

    private static List<PlayerModule> GetAllPlayerData(RainWorldGame game)
    {
        List<PlayerModule> allPlayerData = new();
        List<AbstractCreature> players = game.Players;

        if (players == null) return allPlayerData;

        foreach (AbstractCreature creature in players)
        {
            if (creature.realizedCreature == null) continue;

            if (creature.realizedCreature is not Player player) continue;

            if (!PlayerData.TryGetValue(player, out PlayerModule playerModule)) continue;

            allPlayerData.Add(playerModule);
        }

        return allPlayerData;
    }



    // SlugBase Features
    public static readonly PlayerFeature<Vector2> ActiveObjectOffset = new("active_object_offset", Hooks.Vector2Feature);


    // Feature Factories
    public static Vector2 Vector2Feature(JsonAny json)
    {
        JsonList jsonList = json.AsList();
        return new Vector2(jsonList[0].AsFloat(), jsonList[1].AsFloat());
    }
}
