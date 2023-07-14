namespace Pearlcat;

public class T1_S01 : UpdatableAndDeletable
{
    public Phase CurrentPhase { get; set; } = Phase.Init;

    public enum Phase
    {
        Init,
        End,
    }

    public T1_S01(Room room)
    {
        this.room = room;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        
        if (room.shelterDoor.IsClosing && room.lockedShortcuts.Count == 0)
            for (int i = 0; i < room.shortcutsIndex.Length; i++)
                room.lockedShortcuts.Add(room.shortcutsIndex[i]);
    }
}
