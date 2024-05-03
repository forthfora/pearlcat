using CWStuff;
using UnityEngine;

namespace Pearlcat;

public static class CWIntegration
{
    public static void Init()
    {
        On.SSOracleBehavior.SpecialEvent += SSOracleBehavior_SpecialEvent;

        CWConversation.OnAddEvents += CWConversation_OnAddEvents;
    }

    private static void CWConversation_OnAddEvents(CWConversation self, ref bool runOriginalCode)
    {
        var room = self.owner?.oracle?.room;

        if (room == null || room.game.IsPearlcatStory()) return;

        var miscProg = Utils.GetMiscProgression();
        var miscWorld = room.game.GetMiscWorld();

        if (miscWorld == null) return;


        runOriginalCode = false;

        var rand = Random.Range(0, 100000);

        
        if (miscProg.HasTrueEnding)
        {
            CWConversation.CWEventsFromFile(self, "Pearlcat_FirstEncounter_TrueEnd");

            CWConversation.CWEventsFromFile(self, "Pearlcat_SecondEncounter_TrueEnd");


            CWConversation.CWEventsFromFile(self, "Pearlcat_RandomGreeting_TrueEnd", false, null, true, rand);
        }
        else
        {
            if (miscWorld.HasPearlpupWithPlayer)
            {
                if (miscProg.IsPearlpupSick)
                {
                    CWConversation.CWEventsFromFile(self, "Pearlcat_FirstEncounter_SickPup");

                    CWConversation.CWEventsFromFile(self, "Pearlcat_SecondEncounter_SickPup");


                    CWConversation.CWEventsFromFile(self, "Pearlcat_RandomGreeting_SickPup", false, null, true, rand);
                }
                else
                {
                    CWConversation.CWEventsFromFile(self, "Pearlcat_FirstEncounter_HasPup");

                    CWConversation.CWEventsFromFile(self, "Pearlcat_SecondEncounter_HasPup");


                    CWConversation.CWEventsFromFile(self, "Pearlcat_RandomGreeting_HasPup", false, null, true, rand);
                }
            }
            else
            {
                CWConversation.CWEventsFromFile(self, "Pearlcat_FirstEncounter_NoPup");

                CWConversation.CWEventsFromFile(self, "Pearlcat_SecondEncounter");


                CWConversation.CWEventsFromFile(self, "Pearlcat_RandomGreeting_NoPup", false, null, true, rand);
            }
        }
    }

    private static void SSOracleBehavior_SpecialEvent(On.SSOracleBehavior.orig_SpecialEvent orig, SSOracleBehavior self, string eventName)
    {
        orig(self, eventName);
    }
}
