
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

    public Vector2 TutorialSlugpupPos { get; } = new(600.0f, 230.0f);

    public Vector2 TutorialPearlPos { get; } = new(750.0f, 230.0f);
    public DataPearl.AbstractDataPearl.DataPearlType TutorialPearlType { get; } = Enums.Pearls.AS_PearlGreen;

    public DataPearl.AbstractDataPearl? TutorialPearl { get; set; }


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
                        new(room.abstractRoom.index, -1, -1, 0), room.game.GetNewID(), -1, -1, null, TutorialPearlType);

                    room.abstractRoom.entities.Add(abstractPearl);
                    abstractPearl.RealizeInRoom();

                    var pearl = abstractPearl.realizedObject;
                    pearl.firstChunk.HardSetPosition(TutorialPearlPos);


                    var abstractSlugpup = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC), null, new(room.abstractRoom.index, -1, -1, 0), room.game.GetNewID());

                    room.abstractRoom.entities.Add(abstractSlugpup);
                    abstractSlugpup.RealizeInRoom();

                    var slugpup = (Player)abstractSlugpup.realizedObject;
                    slugpup.firstChunk.pos = TutorialSlugpupPos;
                    slugpup.graphicsModule.Reset();

                    slugpup.Die();


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
                game.AddTextPrompt($"GREEN symbolizes vitality. Each green pearl stored may revive one creature per cycle, including you", 0, 400);
                
                game.AddTextPrompt($"Grab a creature and hold (GRAB) with an active green pearl to revive them", 0, 400);


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
