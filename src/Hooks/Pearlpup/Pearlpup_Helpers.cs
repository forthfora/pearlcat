namespace Pearlcat;

public static class Pearlpup_Helpers
{
    public static bool IsPearlpup(this AbstractCreature crit)
    {
        return crit.Room?.world?.game?.IsPearlcatStory() == true &&
               crit.Room.world.game.GetMiscWorld()?.PearlpupID == crit.ID.number;
    }

    public static bool IsPearlpup(this Player player)
    {
        return player.abstractCreature.IsPearlpup();
    }

    public static void ConvertIntoPearlpupIfIdMatch(Player self)
    {
        var miscWorld = self.abstractCreature?.world?.game?.GetMiscWorld();

        if (miscWorld is null)
        {
            return;
        }

        if (miscWorld.HasPearlpupWithPlayerDeadOrAlive)
        {
            return;
        }

        if (self.IsPearlpup())
        {
            return;
        }

        if (self.abstractCreature?.ID.number != miscWorld.PearlpupID)
        {
            return;
        }

        self.abstractCreature?.TryMakePearlpup();
    }
}
