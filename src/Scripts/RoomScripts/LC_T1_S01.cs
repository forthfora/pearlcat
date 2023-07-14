namespace Pearlcat;

public class LC_T1_S01 : UpdatableAndDeletable
{
    public Phase CurrentPhase { get; set; } = Phase.Init;

    public enum Phase
    {
        Init,
        End,
    }

    public LC_T1_S01(Room room)
    {
        this.room = room;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        foreach (var crit in room.game.Players)
        {
            if (crit.realizedCreature is not Player player) continue;

            if (CurrentPhase == Phase.Init)
            {
                player.SuperHardSetPosition(new(550.0f, 222.0f));
                player.graphicsModule?.Reset();

                CurrentPhase = Phase.End;
            }
        }
    }
}
