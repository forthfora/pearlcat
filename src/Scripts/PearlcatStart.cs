
namespace Pearlcat;

public class PearlcatStart : UpdatableAndDeletable
{
    public Phase CurrentPhase { get; set; } = Phase.Init;
    public int HardsetPosTimer { get; set; } = 2;

    public enum Phase
    {
        Init,
        End,
    }

    public PearlcatStart(Room room)
    {
        this.room = room;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        SingleplayerUpdate();
    }

    public void SingleplayerUpdate()
    {
        if (room.game.Players[0].realizedCreature is not Player player) return;

        if (!player.TryGetPearlcatModule(out var playerModule)) return;

        if (CurrentPhase == Phase.Init)
        {
            player.playerState.foodInStomach = 3;
            player.sleepCounter = 100;
            CurrentPhase = Phase.End;

            foreach (var item in playerModule.Inventory)
            {
                player.RemoveFromInventory(item);
                item.destroyOnAbstraction = true;
                item.Abstractize(item.pos);
            }
        }

        // I think Slugbase is setting the position after us ?
        if (HardsetPosTimer > 0)
        {
            player.SuperHardSetPosition(new(680.0f, 340.0f));
            player.graphicsModule?.Reset();
            HardsetPosTimer--;
        }
    }
}
