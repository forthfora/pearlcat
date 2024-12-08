using System.Collections.Generic;
using static Conversation;
using static SSOracleBehavior;
using Random = UnityEngine.Random;

using MSCID = MoreSlugcats.MoreSlugcatsEnums.ConversationID;

namespace Pearlcat;

public static class SSOracleConversation_Helpers
{
    public static Dictionary<ID, int> ConvoIdFileIdMap { get; } = new()
    {
        // Random Line
        { ID.Moon_Pearl_Misc, 38 },
        { ID.Moon_Pearl_Misc2, 38 },
        { ID.Moon_Pebbles_Pearl, 40 },

        // Base Game
        { ID.Moon_Pearl_SL_moon, 18 },
        { ID.Moon_Pearl_SL_chimney, 54 },
        { ID.Moon_Pearl_SL_bridge, 17 },
        { ID.Moon_Pearl_SB_filtration, 15 },
        { ID.Moon_Pearl_SB_ravine, 43 },
        { ID.Moon_Pearl_SU, 41 },
        { ID.Moon_Pearl_HI, 12 },
        { ID.Moon_Pearl_GW, 16 },
        { ID.Moon_Pearl_DS, 14 },
        { ID.Moon_Pearl_SH, 13 },
        { ID.Moon_Pearl_CC, 7 },
        { ID.Moon_Pearl_UW, 42 },
        { ID.Moon_Pearl_LF_bottom, 11 },
        { ID.Moon_Pearl_LF_west, 10 },

        { ID.Moon_Pearl_SI_west, 20 },
        { ID.Moon_Pearl_SI_top, 21 },

        { ID.Moon_Pearl_Red_stomach, 51 },

        // MSC
        { MSCID.Moon_Pearl_SI_chat3, 22 },
        { MSCID.Moon_Pearl_SI_chat4, 23 },
        { MSCID.Moon_Pearl_SI_chat5, 24 },

        { MSCID.Moon_Pearl_VS, 128 },
        { MSCID.Moon_Pearl_SU_filt, 101 },
        { MSCID.Moon_Pearl_OE, 104 },
        { MSCID.Moon_Pearl_LC, 103 },
        { MSCID.Moon_Pearl_LC_second, 121 },
        { MSCID.Moon_Pearl_MS, 105 },
        { MSCID.Moon_Pearl_DM, 102 },

        { MSCID.Moon_Pearl_Rivulet_stomach, 119 },

        { MSCID.Moon_Pearl_RM, 106 },
    };

    public static List<ID> RandomLineConvoIds = [ID.Moon_Pearl_Misc, ID.Moon_Pearl_Misc2, ID.Moon_Pebbles_Pearl];


    // Determines the first lines of dialogue before anything else Pebbles says
    public static void PebblesPearlIntro(this PebblesConversation self)
    {
        var module = self.owner.GetModule();
        var save = self.owner.oracle.room.game.GetMiscWorld();

        if (save == null)
        {
            return;
        }


        if (module.WasPearlAlreadyRead)
        {
            switch (Random.Range(0, 4))
            {
                case 0:
                    self.events.Add(new TextEvent(self, 0, self.owner.Translate("I already read this one. I can read it again, I suppose."), 10));
                    break;

                case 1:
                    self.events.Add(new TextEvent(self, 0, self.owner.Translate("This one again? I have better things to do... but..."), 10));
                    break;

                case 2:
                    self.events.Add(new TextEvent(self, 0, self.owner.Translate("Let us see what- oh, again?"), 10));
                    break;

                default:
                    self.events.Add(new TextEvent(self, 0, self.owner.Translate("I remember this one... don't you? Well..."), 10));
                    break;
            }
        }
        else
        {
            switch (save.UniquePearlsBroughtToPebbles)
            {
                case 1:
                    self.events.Add(new TextEvent(self, 0, self.owner.Translate("Something new to read... it has been too long..."), 10));
                    break;

                case 2:
                    self.events.Add(new TextEvent(self, 0, self.owner.Translate("Another? And I shall read this one to you as well..."), 10));
                    break;

                case 3:
                    self.events.Add(new TextEvent(self, 0, self.owner.Translate("And a third? So it is..."), 10));
                    break;

                case 4:
                    self.events.Add(new TextEvent(self, 0, self.owner.Translate("Four! Well, if you insist..."), 10));
                    break;

                case 5:
                    self.events.Add(new TextEvent(self, 0, self.owner.Translate("So curious..."), 10));
                    break;

                default:
                    switch (Random.Range(0, 4))
                    {
                        case 0:
                            self.events.Add(new TextEvent(self, 0, self.owner.Translate("Another one? I shouldn't be surprised. Let's see..."), 10));
                            break;

                        case 1:
                            self.events.Add(new TextEvent(self, 0, self.owner.Translate("Ah, yet another one? You are even better at finding these than I imagined..."), 10));
                            break;

                        case 2:
                            self.events.Add(new TextEvent(self, 0, self.owner.Translate("Read this one too? Yes, yes, here it is..."), 10));
                            break;

                        default:
                            self.events.Add(new TextEvent(self, 0, self.owner.Translate("Something else new? Allow me to see..."), 10));
                            break;
                    }
                    break;
            }
        }
    }


    // Handles any Pearlcat specific conversation IDs
    public static bool TryHandlePebblesConversation(PebblesConversation self, SSOracleModule module)
    {
        var miscWorld = self.owner.oracle.room.game.GetMiscWorld();
        var miscProg = Utils.MiscProgression;

        var currentLang = Utils.Translator.currentLanguage;

        // Linger - increases for character-based languages as it is determined by character count, not word count
        var l = currentLang == InGameTranslator.LanguageID.Chinese || currentLang == InGameTranslator.LanguageID.Japanese || currentLang == InGameTranslator.LanguageID.Korean ? 1 : 0;

        var id = self.id;
        var e = self.events;

        if (id == Enums.Oracle.Pearlcat_SSConvoFirstMeet)
        {
            if (miscWorld?.HasPearlpupWithPlayer == true)
            {
                e.Add(new WaitEvent(self, 160));

                e.Add(new TextEvent(self, 0,
                    self.Translate(".  .  ."), 0));

                e.Add(new TextEvent(self, 0,
                    self.Translate("And just who might you two be?"), l * 80));

                e.Add(new TextEvent(self, 0,
                    self.Translate("My overseers have made no peace over your arrival - I was under the impression the transit system was mostly inoperable."), l * 80));

                e.Add(new WaitEvent(self, 80));


                e.Add(new TextEvent(self, 0,
                    self.Translate("You come from quite a distance? You can communicate with us, but the mark you possess is foreign to me..."), l * 80));

                e.Add(new TextEvent(self, 0,
                    self.Translate("...you can manipulate our storage medium in unforseen ways..."), l * 80));

                e.Add(new WaitEvent(self, 40));


                e.Add(new TextEvent(self, 0,
                    self.Translate("Clearly artificial creations. But from where? Sent by who?"), l * 80));

                e.Add(new TextEvent(self, 0,
                    self.Translate("And why... to me?"), l * 80));


                e.Add(new WaitEvent(self, 80));

                e.Add(new TextEvent(self, 0,
                    self.Translate("I do not know what to make of this."), l * 80));
            }
            else
            {
                e.Add(new WaitEvent(self, 160));

                e.Add(new TextEvent(self, 0,
                    self.Translate(".  .  ."), 0));

                e.Add(new TextEvent(self, 0,
                    self.Translate("And just who might you be?"), l * 80));

                e.Add(new TextEvent(self, 0,
                    self.Translate("My overseers have made no peace over your arrival - I was under the impression the transit system was mostly inoperable."), l * 80));

                e.Add(new WaitEvent(self, 80));


                e.Add(new TextEvent(self, 0,
                    self.Translate("You come from quite a distance? You can communicate with us, but the mark you possess is foreign to me..."), l * 80));

                e.Add(new TextEvent(self, 0,
                    self.Translate("...you can manipulate our storage medium in unforseen ways..."), l * 80));

                e.Add(new WaitEvent(self, 40));


                e.Add(new TextEvent(self, 0,
                    self.Translate("Clearly an artifical creation. But from where? Sent by who?"), l * 80));

                e.Add(new TextEvent(self, 0,
                    self.Translate("And why... to me?"), l * 80));


                e.Add(new WaitEvent(self, 80));

                e.Add(new TextEvent(self, 0,
                    self.Translate("I do not know what to make of this."), l * 80));
            }

            return true;
        }

        if (id == Enums.Oracle.Pearlcat_SSConvoFirstLeave)
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

            if (miscWorld?.HasPearlpupWithPlayer == true && miscProg.IsPearlpupSick)
            {
                e.Add(new WaitEvent(self, 40));
            }
            else
            {
                if (miscWorld?.HasPearlpupWithPlayer == true)
                {
                    e.Add(new WaitEvent(self, 40));

                    e.Add(new TextEvent(self, 0,
                        self.Translate("Oh, and for direction - there is a gate to the far west of here, where the ground fissures..."), l * 80));

                    e.Add(new TextEvent(self, 0,
                        self.Translate("The expanse beyond would be a safe place for you to raise your family - and to stay far away from my business."), l * 80));

                    e.Add(new TextEvent(self, 0,
                        self.Translate("If you do not plan to provide me with any meaningful data, that is your best course of action;"), l * 80));

                    e.Add(new TextEvent(self, 0,
                        self.Translate("The choice is yours."), l * 80));
                }

                e.Add(new WaitEvent(self, 120));

                e.Add(new TextEvent(self, 0,
                    self.Translate("I must resume my work. I will be waiting here, as always."), l * 80));

                e.Add(new WaitEvent(self, 40));
            }

            return true;
        }

        if (id == Enums.Oracle.Pearlcat_SSConvoRMPearlInspect)
        {
            module.TakeRMTimer = 120;
            module.GiveSSTimer = 60;

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

            return true;
        }

        if (id == Enums.Oracle.Pearlcat_SSConvoTakeRMPearl)
        {
            e.Add(new WaitEvent(self, 200));

            e.Add(new TextEvent(self, 0,
                self.Translate("...there."), l * 80));

            return true;
        }

        if (id == Enums.Oracle.Pearlcat_SSConvoSickPup)
        {
            if (miscWorld != null)
            {
                miscWorld.PebblesMetSickPup = true;
            }

            e.Add(new WaitEvent(self, 40));

            e.Add(new TextEvent(self, 0,
                self.Translate(". . ."), l * 80));

            e.Add(new TextEvent(self, 0,
                self.Translate("I am not sure if you are aware, but your child appears very unwell..."), l * 80));

            e.Add(new TextEvent(self, 0,
                self.Translate("This illness, it is unlike any I have seen before - I can say it is likely fatal."), l * 80));

            e.Add(new TextEvent(self, 0,
                self.Translate("There is nothing I can do - I am not a medical facility, and even if I were, my equipment is far from pristine."), l * 80));

            e.Add(new TextEvent(self, 0,
                self.Translate("I am... sorry."), l * 80));

            e.Add(new WaitEvent(self, 120));

            e.Add(new TextEvent(self, 0,
                self.Translate("...we were built to solve problems. It is a shame not every problem has a solution."), l * 80));

            e.Add(new TextEvent(self, 0,
                self.Translate("...but... I will think on your problem, for now. Perhaps I can find a solution for both of us."), l * 80));

            return true;
        }

        if (id == Enums.Oracle.Pearlcat_SSConvoFirstMeetTrueEnd)
        {
            e.Add(new WaitEvent(self, 240));

            e.Add(new TextEvent(self, 0,
                self.Translate(".  .  ."), 0));

            e.Add(new TextEvent(self, 0,
                self.Translate("Some time ago now, I encountered a scholar and their dying pup..."), l * 80));

            e.Add(new TextEvent(self, 0,
                self.Translate("I believe that child was you."), l * 80));

            e.Add(new TextEvent(self, 0,
                self.Translate("Considering the state you left in, I thought it impossible I would see you again... and yet here you are."), l * 80));

            e.Add(new WaitEvent(self, 80));

            e.Add(new TextEvent(self, 0,
                self.Translate("I noticed the augment keeping you alive as soon as you returned here - I don't say this lightly, but it is beyond me how it works."), l * 80));

            e.Add(new TextEvent(self, 0,
                self.Translate("I would love to study it, to help treat my own affliction, however it would require... removal of the augment."), l * 80));

            e.Add(new WaitEvent(self, 40));

            e.Add(new TextEvent(self, 0,
                self.Translate(". . ."), l * 80));

            e.Add(new WaitEvent(self, 120));

            e.Add(new TextEvent(self, 0,
                self.Translate("No, I am not so selfish as to sacrifice you on a whim for this - a cure is not even guaranteed."), l * 80));

            e.Add(new TextEvent(self, 0,
                self.Translate("I am glad you found your own solution... even if it is an abhorrent perversion of natural law."), l * 80));

            e.Add(new WaitEvent(self, 40));

            e.Add(new TextEvent(self, 0,
                self.Translate("Given you have inherited your mother's traits, if you want to make these visits worth my time, bring me something to read..."), l * 80));

            e.Add(new TextEvent(self, 0,
                self.Translate("Otherwise, I must resume my work, it seems the path to my own cure is unfortunately much longer..."), l * 80));

            e.Add(new WaitEvent(self, 40));

            e.Add(new TextEvent(self, 0,
                self.Translate("Your survival gives me some hope, little one."), l * 80));

            return true;
        }

        if (id == Enums.Oracle.Pearlcat_SSConvoUnlockMira)
        {
            miscProg.UnlockedMira = true;

            e.Add(new WaitEvent(self, 160));

            e.Add(new TextEvent(self, 0,
                self.Translate(".  .  ."), 0));

            e.Add(new TextEvent(self, 0,
                self.Translate("I may not have the facilities to aid you, however..."), l * 80));

            e.Add(new TextEvent(self, 0,
                self.Translate("If you were able to reach me via the transit system, I can only assume that you may be able to reach others far beyond."), l * 80));

            e.Add(new TextEvent(self, 0,
                self.Translate("I have sent a request for the stationary cargo transports in my city to resume operation."), l * 80));

            e.Add(new TextEvent(self, 0,
                self.Translate("I cannot guarantee anything - these locomotives have not been operational since my facility was placed into lockdown..."), l * 80));

            e.Add(new TextEvent(self, 0,
                self.Translate("However, given a lack of other options, this is the best I can do."), l * 80));

            e.Add(new TextEvent(self, 0,
                self.Translate("I assume you may find them nearby the area you arrived; that is via the access shaft, if you have forgotten."), l * 80));

            e.Add(new TextEvent(self, 0,
                self.Translate("Whichever path you take, I hope you find the help that you seek."), l * 80));

            e.Add(new TextEvent(self, 0,
                self.Translate("Good luck, mysterious scholar."), l * 80));

            return true;
        }

        return false;
    }
}
