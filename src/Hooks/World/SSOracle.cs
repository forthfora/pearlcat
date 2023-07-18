using System.Linq;
using UnityEngine;
using static Conversation;
using static SSOracleBehavior;

namespace Pearlcat;

// the base was referenced from Dronemaster: https://github.com/HarvieSorroway/TheDroneMaster
public static partial class Hooks
{
    public static void ApplySSOracleHooks()
    {
        On.SSOracleBehavior.SeePlayer += SSOracleBehavior_SeePlayer;
        On.SSOracleBehavior.NewAction += SSOracleBehavior_NewAction;

        On.SSOracleBehavior.PebblesConversation.AddEvents += PebblesConversation_AddEvents;
    }


    private static void PebblesConversation_AddEvents(On.SSOracleBehavior.PebblesConversation.orig_AddEvents orig, PebblesConversation self)
    {
        orig(self);

        var l = self.owner.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.Chinese ? 1 : 0;

        var id = self.id;
        var e = self.events;

        if (id == Enums.SSOracle.Pearlcat_SSConvoFirstMeet)
        {
            e.Add(new WaitEvent(self, 160));

            e.Add(new TextEvent(self, 0,
                self.Translate(".  .  ."), 0));

            e.Add(new TextEvent(self, 0,
                self.Translate("And just who might you be?"), l * 80));
            
            e.Add(new WaitEvent(self, 80));
            


            e.Add(new TextEvent(self, 0,
                self.Translate("You can communicate with us, you can manipulate our storage medium in unforseen ways..."), l * 80));

            e.Add(new WaitEvent(self, 40));



            e.Add(new TextEvent(self, 0,
                self.Translate("Clearly an artifical creation. But from where? Sent by who?"), l * 80));

            e.Add(new TextEvent(self, 0,
                self.Translate("And why... to me?"), l * 80));


            e.Add(new WaitEvent(self, 80));

            e.Add(new TextEvent(self, 0,
                self.Translate("I do not know what to make of this."), l * 80));
        }

        else if (id == Enums.SSOracle.Pearlcat_SSConvoRMPearl)
        {
            e.Add(new WaitEvent(self, 120));

            e.Add(new TextEvent(self, 0,
                self.Translate("...that pearl you carry, the purple one, where did you find it?"), l * 80));

            e.Add(new TextEvent(self, 0,
                self.Translate(".  .  ."), 0));

            e.Add(new WaitEvent(self, 80));



            e.Add(new TextEvent(self, 0,
                self.Translate("It appears to contain a hymn that once meant a lot to the inhabitants of my city..."), l * 80));

            e.Add(new TextEvent(self, 0,
                self.Translate("...one which still means a lot to me."), l * 80));


            e.Add(new TextEvent(self, 0,
                self.Translate("I have tried to reconstruct it from memory many times, fruitlessly."), l * 80));

            e.Add(new WaitEvent(self, 80));



            e.Add(new TextEvent(self, 0,
                self.Translate("And now here you are, with a perfect copy."), l * 80));

            e.Add(new WaitEvent(self, 80));


            e.Add(new TextEvent(self, 0,
                self.Translate("I suppose the only thing you value in these pearls is how pure their lattice is? I will substitute you with one much more refined than this."), l * 80));

            e.Add(new TextEvent(self, 0,
                self.Translate("I assure you it will be a more than suitable replacement for your primitive needs."), l * 80));

            e.Add(new WaitEvent(self, 80));
        }

        else if (id == Enums.SSOracle.Pearlcat_SSConvoFirstLeave)
        {
            e.Add(new WaitEvent(self, 120));

            e.Add(new TextEvent(self, 0,
                self.Translate(".  .  ."), 0));

            e.Add(new TextEvent(self, 0,
                self.Translate("You have a knack for finding these pearls, yes?"), l * 80));

            e.Add(new TextEvent(self, 0,
                self.Translate("I suppose, seeing as you cannot read the information stored on them, I can read them for you, if you wish."), l * 80));

            e.Add(new TextEvent(self, 0,
                self.Translate("Think of this mutually, I can 'see' a little further outside this can..."), l * 80));


            e.Add(new TextEvent(self, 0,
                self.Translate("...and you can gather more of that data you so clearly desire."), l * 80));


            e.Add(new WaitEvent(self, 120));

            e.Add(new TextEvent(self, 0,
                self.Translate("I will be waiting here, as always."), l * 80));

            e.Add(new WaitEvent(self, 160));
        }
    }


    private static void SSOracleBehavior_SeePlayer(On.SSOracleBehavior.orig_SeePlayer orig, SSOracleBehavior self)
    {
        if (self.oracle.room.game.IsPearlcatStory() && self.action != Enums.SSOracle.Pearlcat_SSActionGeneral)
        {
            if (self.timeSinceSeenPlayer < 0)
                self.timeSinceSeenPlayer = 0;
            
            self.SlugcatEnterRoomReaction();
            self.NewAction(Enums.SSOracle.Pearlcat_SSActionGeneral);
            return;
        }

        orig(self);
    }

    private static void SSOracleBehavior_NewAction(On.SSOracleBehavior.orig_NewAction orig, SSOracleBehavior self, Action nextAction)
    {
        if (self.oracle.room.game.IsPearlcatStory() && self.action != Enums.SSOracle.Pearlcat_SSActionGeneral)
        {
            if (self.currSubBehavior.ID == Enums.SSOracle.Pearlcat_SSSubBehavGeneral) return;

            self.inActionCounter = 0;
            self.action = nextAction;

            var subBehavior = self.allSubBehaviors.FirstOrDefault(x => x.ID == Enums.SSOracle.Pearlcat_SSSubBehavGeneral);
            
            if (subBehavior == null)
                self.allSubBehaviors.Add(subBehavior = new SSOracleMeetPearlcat(self));

            self.currSubBehavior.Deactivate();

            subBehavior.Activate(self.action, nextAction);
            self.currSubBehavior = subBehavior;
            return;
        }
        
        orig(self, nextAction);
    }


    public class SSOracleMeetPearlcat : ConversationBehavior
    {
        public int ConvoCount { get; set; }

        public SSOracleMeetPearlcat(SSOracleBehavior owner) : base(owner, Enums.SSOracle.Pearlcat_SSSubBehavGeneral, Enums.SSOracle.Pearlcat_SSConvoFirstMeet)
        {
            var save = oracle.room.game.GetMiscWorld();
         
            if (save.PebblesMeetCount == 1)
            {
                dialogBox.NewMessage(
                    Translate("Ah. So you've returned."), 0);

                dialogBox.NewMessage(
                    Translate("Brought me something to read? Or just wasting my time, as per usual?"), 0);
            }
            else if (save.PebblesMeetCount == 2)
            {
                dialogBox.NewMessage(
                    Translate("Back again?"), 0);
            }
        }

        public override void Update()
        {
            if (player == null) return;

            var save = oracle.room.game.GetMiscWorld();

            owner.movementBehavior = MovementBehavior.KeepDistance;
            
            if (owner.conversation == null || owner.conversation.slatedForDeletion)
            {
                var meetCount = save.PebblesMeetCount;

                if (meetCount == 0)
                {
                    if (ConvoCount == 0)
                    {
                        owner.LockShortcuts();
        
                        owner.InitateConversation(Enums.SSOracle.Pearlcat_SSConvoFirstMeet, this);
                        ConvoCount++;
                    }
                    else if (ConvoCount == 1)
                    {
                        if (HasRMPearl(oracle))
                            owner.InitateConversation(Enums.SSOracle.Pearlcat_SSConvoRMPearl, this);
   
                        ConvoCount++;
                    }
                    else if (ConvoCount == 2)
                    {
                        owner.InitateConversation(Enums.SSOracle.Pearlcat_SSConvoFirstLeave, this);
                        ConvoCount++;
                    }
                    else
                    {
                        owner.UnlockShortcuts();

                        owner.getToWorking = 1.0f;
                        owner.movementBehavior = MovementBehavior.Meditate;

                        save.PebblesMeetCount++;

                    }
                }
                else if (meetCount > 1)
                {

                }
            }
        }

        public bool HasRMPearl(Oracle oracle)
        {
            foreach (var roomObject in oracle.room.physicalObjects)
            {
                foreach (var physicalObject in roomObject)
                {
                    if (physicalObject is not DataPearl dataPearl) continue;

                    var type = dataPearl.AbstractPearl.dataPearlType;

                    if (type == Enums.Pearls.RM_Pearlcat || type == MoreSlugcats.MoreSlugcatsEnums.DataPearlType.RM)
                        return true;
                }
            }

            return false;
        }
    }
}
