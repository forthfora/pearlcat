using System.Linq;
using Action = SSOracleBehavior.Action;

namespace Pearlcat;

// the base was referenced from Dronemaster: https://github.com/HarvieSorroway/TheDroneMaster
public static class SSOracle_Hooks
{
    public static void ApplyHooks()
    {
        On.SSOracleBehavior.UpdateStoryPearlCollection += SSOracleBehavior_UpdateStoryPearlCollection;

        On.SSOracleBehavior.SeePlayer += SSOracleBehavior_SeePlayer;
        On.SSOracleBehavior.NewAction += SSOracleBehavior_NewAction;
    }


    private static void SSOracleBehavior_UpdateStoryPearlCollection(On.SSOracleBehavior.orig_UpdateStoryPearlCollection orig, SSOracleBehavior self)
    {
        if (self.oracle.room.game.IsPearlcatStory() && self.IsPebbles())
        {
            return;
        }

        orig(self);
    }

    private static void SSOracleBehavior_SeePlayer(On.SSOracleBehavior.orig_SeePlayer orig, SSOracleBehavior self)
    {
        if (self.oracle.room.game.IsPearlcatStory() && self.IsPebbles() && self.action != Enums.Oracle.Pearlcat_SSActionGeneral)
        {
            if (self.timeSinceSeenPlayer < 0)
            {
                self.timeSinceSeenPlayer = 0;
            }

            self.SlugcatEnterRoomReaction();
            self.NewAction(Enums.Oracle.Pearlcat_SSActionGeneral);
            return;
        }

        orig(self);
    }

    private static void SSOracleBehavior_NewAction(On.SSOracleBehavior.orig_NewAction orig, SSOracleBehavior self, Action nextAction)
    {
        if (self.oracle.room.game.IsPearlcatStory() && self.IsPebbles() && self.action != Enums.Oracle.Pearlcat_SSActionGeneral && self.action != Action.ThrowOut_KillOnSight)
        {
            if (self.currSubBehavior.ID == Enums.Oracle.Pearlcat_SSSubBehavGeneral)
            {
                return;
            }

            self.inActionCounter = 0;
            self.action = nextAction;

            var subBehavior = self.allSubBehaviors.FirstOrDefault(x => x.ID == Enums.Oracle.Pearlcat_SSSubBehavGeneral);
            
            if (subBehavior is null)
            {
                self.allSubBehaviors.Add(subBehavior = new SSOracleMeetPearlcat(self));
            }

            self.currSubBehavior.Deactivate();

            subBehavior.Activate(self.action, nextAction);
            self.currSubBehavior = subBehavior;
            return;
        }
        
        orig(self, nextAction);
    }
}
