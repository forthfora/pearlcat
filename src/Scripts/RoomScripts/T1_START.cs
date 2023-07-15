namespace Pearlcat;

public class T1_START : UpdatableAndDeletable
{
    public Phase CurrentPhase { get; set; } = Phase.Init;
    public int HardsetPosTimer { get; set; } = 2;

    public enum Phase
    {
        Init,
        End,
    }

    public T1_START(Room room)
    {
        this.room = room;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        
        foreach (var crit in room.game.Players)
        {
            if (crit.realizedCreature is not Player player) continue;

            if (!player.TryGetPearlcatModule(out var playerModule)) return;

            if (CurrentPhase == Phase.Init)
            {
                player.playerState.foodInStomach = 3;
                player.sleepCounter = 100;
                CurrentPhase = Phase.End;

                //// Clear default pearls
                //for (int i = playerModule.Inventory.Count - 1; i >= 0; i--)
                //{
                //    var item = playerModule.Inventory[i];

                //    player.RemoveFromInventory(item);
                //    item.destroyOnAbstraction = true;
                //    item.Abstractize(item.pos);
                //}
            }

            // I think Slugbase is setting the position after us ?
            if (room == player.room && HardsetPosTimer > 0)
            {
                player.SuperHardSetPosition(new(680.0f, 340.0f));
                player.graphicsModule?.Reset();
                HardsetPosTimer--;
            }
        }
    }
}
