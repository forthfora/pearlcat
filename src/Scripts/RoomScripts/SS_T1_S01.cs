namespace Pearlcat;

public class SS_T1_S01 : UpdatableAndDeletable
{
    public Phase CurrentPhase { get; set; } = Phase.Init;
    
    public enum Phase
    {
        Init,
        End,
    }

    public SS_T1_S01(Room room)
    {
        this.room = room;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        if (!room.fullyLoaded) return;

        if (CurrentPhase == Phase.Init)
        {
            foreach (var player in room.PlayersInRoom)
            {
                if (player == null) continue;

                player.SuperHardSetPosition(new(550.0f, 222.0f));
                player.graphicsModule?.Reset();
            }

            CurrentPhase = Phase.End;
        }
    }
}
