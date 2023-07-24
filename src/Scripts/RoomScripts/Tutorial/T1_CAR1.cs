using System.Linq;
using UnityEngine;

namespace Pearlcat;

public class T1_CAR1 : UpdatableAndDeletable
{
    public Phase CurrentPhase { get; set; } = Phase.Init;
    public int PhaseTimer { get; set; }

    public enum Phase
    {
        Init,

        PreTutorial,
        Tutorial,
        Demo,
        
        End,
    }

    public T1_CAR1(Room room)
    {
        this.room = room;
    }

    public Vector2 TutorialPearlPos { get; } = new(680.0f, 230.0f);
    public DataPearl.AbstractDataPearl.DataPearlType TutorialPearlType { get; } = Enums.Pearls.AS_PearlYellow;

    public DataPearl.AbstractDataPearl? TutorialPearl { get; set; }


    public override void Update(bool eu)
    {
        base.Update(eu);

        if (!room.fullyLoaded) return;

        var game = room.game;

        if (PhaseTimer == 0)
        {
            if (CurrentPhase == Phase.Init)
            {
                if (room.fullyLoaded && room.BeingViewed)
                {
                    room.LockAndHideShortcuts();
                    
                    var abstractPearl = new DataPearl.AbstractDataPearl(room.world, AbstractPhysicalObject.AbstractObjectType.DataPearl, null,
                        new(room.abstractRoom.index, -1, -1, 0), room.game.GetNewID(), -1, -1, null, TutorialPearlType);

                    room.abstractRoom.entities.Add(abstractPearl);
                    abstractPearl.RealizeInRoom();

                    var pearl = abstractPearl.realizedObject;
                    pearl.firstChunk.HardSetPosition(TutorialPearlPos);


                    TutorialPearl = abstractPearl;
                    CurrentPhase = ModOptions.DisableTutorials.Value ? Phase.End : Phase.PreTutorial;
                }
            }
            else if (CurrentPhase == Phase.PreTutorial)
            {
                if (TutorialPearl != null && TutorialPearl.IsPlayerObject())
                    CurrentPhase = Phase.Tutorial;
            }
            else if (CurrentPhase == Phase.Tutorial)
            {
                game.AddTextPrompt($"YELLOW symbolizes protection. Each yellow pearl stored will provide a shield charge", 0, 400);
                
                game.AddTextPrompt($"Charges are consumed to provide protection. Each pearl individually replenishes its charge after some time", 100, 400);


                PhaseTimer = 300;
                CurrentPhase = Phase.Demo;
            }
            else if (CurrentPhase == Phase.Demo)
            {
                foreach (var player in room.PlayersInRoom)
                {
                    player.firstChunk.vel.y += 80.0f;

                    if (player.TryGetPearlcatModule(out var playerModule))
                        playerModule.ActivateVisualShield();
                }

                room.PlaySound(SoundID.Bomb_Explode, room.PlayersInRoom.First().firstChunk, false, 1.5f, 0.4f);
                room.ScreenMovement(null, Vector2.right * 3.0f, 7.0f);

                PhaseTimer = 300;
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
