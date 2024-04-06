
namespace Pearlcat;

public static partial class Hooks
{
    // DEPEDENT: Pups+
    public static bool IsPearlpup(this Player player) => player.abstractCreature.IsPearlpup();

    public static bool IsPearlpup(this AbstractCreature crit) => crit.Room?.world?.game?.IsPearlcatStory() == true && crit.Room.world.game.GetMiscWorld()?.PearlpupID == crit.ID.number;
}
