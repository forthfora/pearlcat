
namespace Pearlcat;

public class T1_CAR1 : UpdatableAndDeletable
{
    public Phase CurrentPhase { get; set; } = Phase.Init;
    public int PhaseTimer { get; set; }

    public enum Phase
    {
        Init,

        SwapTutorial,
        StoreTutorial,

        End,
    }

    public T1_CAR1(Room room)
    {
        this.room = room;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        var game = room.game;

        if (PhaseTimer == 0)
        {
            if (CurrentPhase == Phase.Init)
            {
                if (room.fullyLoaded)
                {
                    room.LockAndHideShortcuts();
                    PhaseTimer = 50;

                }
            }
            else if (CurrentPhase == Phase.SwapTutorial)
            {
                game.AddTextPrompt($"To cycle between pearls, use ({ModOptions.SwapLeftKeybind.Value}) & ({ModOptions.SwapRightKeybind.Value}), or the triggers on controller.", 0, 400);

                PhaseTimer = 800;
                CurrentPhase = Phase.StoreTutorial;
            }
            else if (CurrentPhase == Phase.StoreTutorial)
            {
                game.AddTextPrompt($"To store, hold the same keybind with a pearl in your right hand.", 0, 400);

                PhaseTimer = 800;
                CurrentPhase = Phase.End;
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
