
namespace Pearlcat;

public class T1_CAR1 : UpdatableAndDeletable
{
    public Phase CurrentPhase { get; set; } = Phase.Init;
    public int PhaseTimer { get; set; }

    public enum Phase
    {
        Init,

        PreTutorial,
        ShieldTutorial,
        
        End,
    }

    public T1_CAR1(Room room)
    {
        this.room = room;
    }

    public DataPearl.AbstractDataPearl? ShieldPearl { get; set; }

    public override void Update(bool eu)
    {
        base.Update(eu);

        var game = room.game;

        if (PhaseTimer == 0)
        {
            if (CurrentPhase == Phase.Init)
            {
                if (room.fullyLoaded && room.BeingViewed)
                {
                    room.LockAndHideShortcuts();
                    
                    var abstractPearl = new DataPearl.AbstractDataPearl(room.world, AbstractPhysicalObject.AbstractObjectType.DataPearl, null,
                        new(room.abstractRoom.index, -1, -1, 0), room.game.GetNewID(), -1, -1, null, Enums.Pearls.AS_PearlYellow);

                    room.abstractRoom.entities.Add(abstractPearl);
                    abstractPearl.RealizeInRoom();

                    var pearl = abstractPearl.realizedObject;
                    pearl.firstChunk.HardSetPosition(new(680.0f, 230.0f));

                    ShieldPearl = abstractPearl;
                    CurrentPhase = Phase.PreTutorial;
                }
            }
            else if (CurrentPhase == Phase.PreTutorial)
            {
                if (ShieldPearl != null && ShieldPearl.IsPlayerObject())
                    CurrentPhase = Phase.ShieldTutorial;
            }
            else if (CurrentPhase == Phase.ShieldTutorial)
            {
                game.AddTextPrompt($"YELLOW signifies protection. Each yellow pearl stored will provide a shield charge", 0, 400);
                
                game.AddTextPrompt($"Charges are consumed to provide protection. Each pearl individually replenishes its charge after some time", 0, 400);


                PhaseTimer = 400;
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
