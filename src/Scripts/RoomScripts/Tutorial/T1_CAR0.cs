using UnityEngine;

namespace Pearlcat;

public class T1_CAR0 : UpdatableAndDeletable
{
    public Phase CurrentPhase { get; set; } = Phase.Init;
    public int PhaseTimer { get; set; }

    public enum Phase
    {
        Init,

        PreTutorial,
        Tutorial,
        
        End,
    }

    public T1_CAR0(Room room)
    {
        this.room = room;
    }

    public Vector2 TutorialPearlPos { get; } = new(820.0f, 290.0f);
    public DataPearl.AbstractDataPearl.DataPearlType TutorialPearlType { get; } = Enums.Pearls.AS_PearlBlue;

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
                    CurrentPhase = ModOptions.DisableTutorials.Value || room.game.GetStorySession.saveStateNumber != Enums.Pearlcat ? Phase.End : Phase.PreTutorial;
                }
            }
            else if (CurrentPhase == Phase.PreTutorial)
            {
                if (TutorialPearl != null && TutorialPearl.IsPlayerPearl())
                {
                    CurrentPhase = Phase.Tutorial;
                }
            }
            else if (CurrentPhase == Phase.Tutorial)
            {
                game.AddTextPrompt("BLUE symbolizes agility. Each blue pearl stored will provide an additional double jump", 0, 800);

                var t = Utils.Translator;

                if (ModOptions.CustomAgilityKeybind.Value)
                {
                    game.AddTextPrompt(t.Translate("Press (") + Input_Helpers.GetAbilityKeybindDisplayName(false) + t.Translate(") or (") + Input_Helpers.GetAbilityKeybindDisplayName(true) + t.Translate(") while in the air to perform a double jump"), 0, 600);
                }

                game.AddTextPrompt("Press (JUMP + GRAB) while in the air to perform a double jump", 0, 600);

                PhaseTimer = 1000;
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
