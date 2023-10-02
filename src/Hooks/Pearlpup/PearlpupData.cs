
namespace Pearlcat;

public static partial class Hooks
{
    public static bool IsPearlpup(this Player player) => player.abstractCreature.IsPearlpup();

    public static bool IsPearlpup(this AbstractCreature crit) => crit.Room.world.game.IsPearlcatStory() && crit.Room.world.game.GetMiscWorld()?.PearlpupID == crit.ID.number;
}
