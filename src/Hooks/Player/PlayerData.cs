namespace Pearlcat;

public static partial class Hooks
{
    public const float ShortcutColorIncrement = 0.003f;

    public static bool IsPearlcat(this Player player) => player.SlugCatClass == Enums.Pearlcat;

    public static bool IsFirstPearlcat(this Player player) => player.playerState.playerNumber == GetFirstPearlcatIndex(player.room?.game);

    public static bool IsPearlcatStory(this RainWorldGame? game) => game?.StoryCharacter == Enums.Pearlcat;

    public static bool IsSingleplayer(this Player player) => player.abstractCreature.world.game.Players.Count == 1;
    
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


    // BACKWARDS COMPAT
    public static bool TryGetPearlcatModule(Player player, out PlayerModule playerModule) => player.TryGetPearlcatModule(out playerModule);
}
