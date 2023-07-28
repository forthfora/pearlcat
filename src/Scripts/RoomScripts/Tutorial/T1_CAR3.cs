
using MoreSlugcats;
using UnityEngine;

namespace Pearlcat;

public class T1_CAR3 : UpdatableAndDeletable
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

    public T1_CAR3(Room room)
    {
        this.room = room;
    }

    public Vector2 TutorialSlugpupPos { get; } = new(500.0f, 280.0f);

    public Vector2 TutorialPearlPos { get; } = new(730.0f, 190.0f);
    public DataPearl.AbstractDataPearl.DataPearlType TutorialPearlType { get; } = Enums.Pearls.AS_PearlGreen;

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
                if (room.BeingViewed)
                {
                    room.LockAndHideShortcuts();
                    
                    var abstractPearl = new DataPearl.AbstractDataPearl(room.world, AbstractPhysicalObject.AbstractObjectType.DataPearl, null,
                        new(room.abstractRoom.index, -1, -1, 0), room.game.GetNewID(), -1, -1, null, TutorialPearlType);

                    room.abstractRoom.entities.Add(abstractPearl);
                    abstractPearl.RealizeInRoom();

                    var pearl = abstractPearl.realizedObject;
                    pearl.firstChunk.HardSetPosition(TutorialPearlPos);

                    if (ModManager.MSC)
                    {
                        var abstractSlugpup = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC), null, new(room.abstractRoom.index, -1, -1, 0), room.game.GetNewID());

                        room.abstractRoom.entities.Add(abstractSlugpup);
                        abstractSlugpup.RealizeInRoom();

                        var slugpup = (Player)abstractSlugpup.realizedObject;
                        slugpup.SuperHardSetPosition(TutorialSlugpupPos);
                        slugpup.graphicsModule.Reset();
                        slugpup.playerState.foodInStomach = 3;

                        slugpup.Stun(40);
                        slugpup.Die();

                        slugpup.MakePearlpup();
                    }


                    TutorialPearl = abstractPearl;
                    CurrentPhase = ModOptions.DisableTutorials.Value || room.game.GetStorySession.saveStateNumber != Enums.Pearlcat ? Phase.End : Phase.PreTutorial;
                }
            }
            else if (CurrentPhase == Phase.PreTutorial)
            {
                if (TutorialPearl != null && TutorialPearl.IsPlayerObject())
                    CurrentPhase = Phase.Tutorial;
            }
            else if (CurrentPhase == Phase.Tutorial)
            {
                game.AddTextPrompt($"GREEN symbolizes vitality. Each green pearl stored may revive one creature per cycle, including you", 0, 800);
                
                game.AddTextPrompt($"Grab a creature and hold (GRAB) with an active green pearl to revive them", 0, 800);


                PhaseTimer = 1300;
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
