
namespace Pearlcat;

// Version of T1_START for other campaigns / meadow, just sets the position and gives food
public class T1_START_Alt : UpdatableAndDeletable
{
    public Phase CurrentPhase { get; set; } = Phase.Init;
    public int HardsetPosTimer { get; set; } = 8;

    public int PhaseTimer { get; set; }

    public enum Phase
    {
        Init,
        End,
    }

    public T1_START_Alt(Room room)
    {
        this.room = room;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        if (!room.fullyLoaded)
        {
            return;
        }

        var game = room.game;

        // Per player
        foreach (var crit in game.Players)
        {
            if (crit.realizedCreature is not Player player)
            {
                continue;
            }

            if (CurrentPhase == Phase.Init)
            {
                player.playerState.foodInStomach = SlugcatStats.SlugcatFoodMeter(player.SlugCatClass).y;
                room.game.cameras[0].hud.foodMeter.NewShowCount(player.FoodInStomach);
            }
            else if (CurrentPhase == Phase.End)
            {
                player.controller = null;
            }

            if (room == player.room && HardsetPosTimer > 0)
            {
                player.SuperHardSetPosition(new(680.0f, 340.0f));
                player.graphicsModule?.Reset();

                HardsetPosTimer--;
            }
        }

        if (PhaseTimer == 0)
        {
            if (CurrentPhase == Phase.Init)
            {
                if (room.BeingViewed)
                {
                    room.LockAndHideShortcuts();

                    room.game.cameras[0].hud.foodMeter.visibleCounter = 0;
                    room.game.cameras[0].hud.foodMeter.fade = 0f;
                    room.game.cameras[0].hud.foodMeter.lastFade = 0f;

                    CurrentPhase = Phase.End;
                }
            }
            else if (CurrentPhase == Phase.End)
            {
                room.UnlockAndShowShortcuts();
                PhaseTimer = -1;
            }
        }
        else if (PhaseTimer > 0)
        {
            PhaseTimer--;
        }
    }
}
