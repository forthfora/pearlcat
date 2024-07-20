using CWStuff;
using UnityEngine;

namespace Pearlcat;

public static class CWIntegration
{
    public static void Init()
    {
        On.SSOracleBehavior.SpecialEvent += SSOracleBehavior_SpecialEvent;
        On.SSOracleBehavior.StartItemConversation += SSOracleBehaviorOnStartItemConversation;
        On.SSOracleBehavior.SeePlayer += SSOracleBehaviorOnSeePlayer;

        CWConversation.OnAddEvents += CWConversation_OnAddEvents;

    }

    private static void SSOracleBehaviorOnSeePlayer(On.SSOracleBehavior.orig_SeePlayer orig, SSOracleBehavior self)
    {
        if (self.oracle?.ID == NewOracleID.CW)
        {
            // Reset this so OnAddEvents always triggers, we handle all conversation in there
            if (self.oracle?.room?.game?.IsPearlcatStory() == true && CWOracleHooks.WorldSaveData.TryGetValue(self.oracle.room.game.GetStorySession.saveState.miscWorldSaveData, out var cwData))
            {
                cwData.NumberOfConversations = 0;
            }
        }

        orig(self);
    }

    private static void SSOracleBehaviorOnStartItemConversation(On.SSOracleBehavior.orig_StartItemConversation orig, SSOracleBehavior self, DataPearl item)
    {
        if (self.oracle?.ID == NewOracleID.CW)
        {
            if (item.AbstractPearl.IsPlayerObject())
            {
                return;
            }
        }

        orig(self, item);
    }

    private static void CWConversation_OnAddEvents(CWConversation self, ref bool runOriginalCode)
    {
        var room = self.owner?.oracle?.room;

        if (room is null) return;

        if (!room.game.IsPearlcatStory()) return;


        var miscProg = Utils.GetMiscProgression();
        var miscWorld = room.game.GetMiscWorld();

        if (miscWorld == null) return;


        runOriginalCode = false;

        var rand = Random.Range(0, 100000);


        // Puts CW into the correct idle stance, can override this if needed
        self.events.Add(new Conversation.SpecialEvent(self, 0, "GRAV"));
        self.events.Add(new Conversation.SpecialEvent(self, 0, "LOCKPATHS"));


        if (miscProg.HasTrueEnding)
        {
            switch (miscWorld.CWTrueEndMeetCount)
            {
                case 0:
                    if (miscWorld.CWMeetCount > 0)
                    {
                        CWConversation.CWEventsFromFile(self, "Pearlcat_FirstEncounter_TrueEnd_Recognised");
                    }
                    else
                    {
                        CWConversation.CWEventsFromFile(self, "Pearlcat_FirstEncounter_TrueEnd");
                    }
                    break;

                case 1:
                    CWConversation.CWEventsFromFile(self, "Pearlcat_SecondEncounter_TrueEnd");
                    break;

                default:
                    CWConversation.CWEventsFromFile(self, "Pearlcat_RandomGreeting_TrueEnd", false, null, true, rand);
                    break;
            }

            miscWorld.CWTrueEndMeetCount++;
        }
        else
        {
            if (miscWorld.HasPearlpupWithPlayer)
            {
                if (miscProg.IsPearlpupSick)
                {
                    switch (miscWorld.CWMeetSickCount)
                    {
                        case 0:
                            CWConversation.CWEventsFromFile(self, "Pearlcat_FirstEncounter_SickPup");
                            break;

                        case 1:
                            CWConversation.CWEventsFromFile(self, "Pearlcat_SecondEncounter_SickPup");
                            break;

                        default:
                            CWConversation.CWEventsFromFile(self, "Pearlcat_RandomGreeting_SickPup", false, null, true, rand);
                            break;
                    }

                    miscWorld.CWMeetSickCount++;
                }
                else
                {
                    switch (miscWorld.CWMeetCount)
                    {
                        case 0:
                            CWConversation.CWEventsFromFile(self, "Pearlcat_FirstEncounter_HasPup");
                            break;

                        case 1:
                            CWConversation.CWEventsFromFile(self, "Pearlcat_SecondEncounter_HasPup");
                            break;

                        default:
                            CWConversation.CWEventsFromFile(self, "Pearlcat_RandomGreeting_HasPup", false, null, true, rand);
                           break;
                    }
                }
            }
            else
            {
                switch (miscWorld.CWMeetCount)
                {
                    case 0:
                        CWConversation.CWEventsFromFile(self, "Pearlcat_FirstEncounter_NoPup");
                        break;

                    case 1:
                        CWConversation.CWEventsFromFile(self, "Pearlcat_SecondEncounter_NoPup");
                        break;

                    default:
                        CWConversation.CWEventsFromFile(self, "Pearlcat_RandomGreeting_NoPup", false, null, true, rand);
                        break;
                }
            }
        }

        // Puts CW into the correct state after any convo
        self.events.Add(new Conversation.SpecialEvent(self, 0, "PARTIALGRAV"));
        self.events.Add(new Conversation.SpecialEvent(self, 0, "UNLOCKPATHS"));

        miscWorld.CWMeetCount++;
    }

    private static void SSOracleBehavior_SpecialEvent(On.SSOracleBehavior.orig_SpecialEvent orig, SSOracleBehavior self, string eventName)
    {
        if (eventName == "PEARLCAT_GIVE_CW")
        {
            GiveCWPearl(self.oracle);
            return;
        }

        orig(self, eventName);
    }

    private static void GiveCWPearl(Oracle oracle, bool withEffect = true)
    {
        var miscWorld = oracle.room.game.GetMiscWorld();

        if (miscWorld is null) return;


        if (miscWorld.CWGavePearl) return;

        miscWorld.CWGavePearl = true;


        foreach (var roomObject in oracle.room.physicalObjects)
        {
            foreach (var physicalObject in roomObject)
            {
                if (physicalObject is not Player player) continue;

                if (!player.TryGetPearlcatModule(out var playerModule)) continue;


                var abstractPearl = new DataPearl.AbstractDataPearl(oracle.room.world, AbstractPhysicalObject.AbstractObjectType.DataPearl, null, player.abstractCreature.pos, oracle.room.game.GetNewID(), -1, -1, null, Enums.Pearls.CW_Pearlcat);

                if (playerModule.Inventory.Count >= ModOptions.MaxPearlCount.Value)
                {
                    abstractPearl.pos = player.abstractCreature.pos;
                    oracle.room.abstractRoom.AddEntity(abstractPearl);
                    abstractPearl.RealizeInRoom();

                    var freeHand = player.FreeHand();

                    if (freeHand > -1)
                    {
                        player.SlugcatGrab(abstractPearl.realizedObject, freeHand);
                    }
                }
                else
                {
                    player.StoreObject(abstractPearl);

                    if (withEffect)
                    {
                        playerModule.ShowHUD(120);
                    }
                }

                if (withEffect)
                {
                    oracle.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, physicalObject.firstChunk, false, 1.5f, 0.5f);
                }
            }
        }
    }
}
