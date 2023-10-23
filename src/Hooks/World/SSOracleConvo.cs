using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using System;
using static Conversation;
using static SSOracleBehavior;
using Random = UnityEngine.Random;

namespace Pearlcat;

public static partial class Hooks
{
    public static void ApplySSOracleConvoHooks()
    {
        On.SSOracleBehavior.PebblesConversation.AddEvents += PebblesConversation_AddEvents;

        try
        {
            IL.SSOracleBehavior.Update += SSOracleBehavior_UpdateIL;
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("Oracle Hooks Error:\n" + e);
        }
    }


    private static void SSOracleBehavior_UpdateIL(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(MoveType.After,
            x => x.MatchLdstr("Yes, help yourself. They are not edible."));

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Func<string, SSOracleBehavior, string>>((origText, self) =>
        {
            if (self.oracle.room.game.IsPearlcatStory())
            {
                var miscProg = self.oracle.room.game.GetMiscProgression();

                if (miscProg.HasTrueEnding)
                {
                    return self.Translate("...ah... are those still a fascination to you? You really are no different from your mother...");
                }
                else
                {
                    return self.Translate("...oh? Take them, the data they contain is worthless to me. I suppose they'd be far more useful to you...");
                }
            }

            return origText;
        });
    }

    private static void PebblesPearlIntro(this PebblesConversation self)
    {
        var module = self.owner.GetModule();
        var save = self.owner.oracle.room.game.GetMiscWorld();

        if (save == null) return;


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

    
    private static void PebblesConversation_AddEvents(On.SSOracleBehavior.PebblesConversation.orig_AddEvents orig, PebblesConversation self)
    {
        if (!self.owner.oracle.room.game.IsPearlcatStory())
        {
            orig(self);
            return;
        }

        var save = self.owner.oracle.room.game.GetMiscWorld();
        var miscProg = self.owner.oracle.room.game.GetMiscProgression();

        var module = self.owner.GetModule();
        var currentLang = self.owner.rainWorld.inGameTranslator.currentLanguage;
        var l = currentLang == InGameTranslator.LanguageID.Chinese || currentLang == InGameTranslator.LanguageID.Japanese || currentLang == InGameTranslator.LanguageID.Korean ? 8 : 0;

        var id = self.id;
        var e = self.events;

        var rand = module.Rand;

        if (id == Enums.SSOracle.Pearlcat_SSConvoFirstMeet)
        {
            if (save?.HasPearlpupWithPlayer == true)
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

            if (save?.HasPearlpupWithPlayer == true && miscProg.IsPearlpupSick)
            {
                e.Add(new WaitEvent(self, 40));
            }
            else
            {
                if (save?.HasPearlpupWithPlayer == true)
                {
                    e.Add(new WaitEvent(self, 40));

                    e.Add(new TextEvent(self, 0,
                        self.Translate("Oh, and for direction - there is a gate to the far west of here, where the ground fissues..."), l * 80));

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
        }

        else if (id == Enums.SSOracle.Pearlcat_SSConvoRMPearlInspect)
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
        }

        else if (id == Enums.SSOracle.Pearlcat_SSConvoTakeRMPearl)
        {
            e.Add(new WaitEvent(self, 200));

            e.Add(new TextEvent(self, 0,
                self.Translate("...there."), l * 80));
        }

        else if (id == Enums.SSOracle.Pearlcat_SSConvoSickPup)
        {
            if (save != null)
            {
                save.PebblesMetSickPup = true;
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
        }

        #region Vanilla Pearls

        else if (self.id == ID.Moon_Pearl_Misc || self.id == ID.Moon_Pearl_Misc2)
        {
            self.PebblesPearlIntro();
            self.LoadEventsFromFile(38, Enums.SSOracle.PearlcatPebbles, true, rand);
        }

        else if (self.id == ID.Moon_Pebbles_Pearl)
        {
            self.PebblesPearlIntro();
            self.LoadEventsFromFile(40, Enums.SSOracle.PearlcatPebbles, true, rand);
        }
        else if (self.id == ID.Moon_Pearl_CC)
        {
            self.PebblesPearlIntro();
            self.LoadEventsFromFile(7, Enums.SSOracle.PearlcatPebbles, false, rand);
        }
        else if (self.id == ID.Moon_Pearl_LF_west)
        {
            self.PebblesPearlIntro();
            self.LoadEventsFromFile(10, Enums.SSOracle.PearlcatPebbles, false, rand);
        }
        else if (self.id == ID.Moon_Pearl_LF_bottom)
        {
            self.PebblesPearlIntro();
            self.LoadEventsFromFile(11, Enums.SSOracle.PearlcatPebbles, false, rand);
        }
        else if (self.id == ID.Moon_Pearl_HI)
        {
            self.PebblesPearlIntro();
            self.LoadEventsFromFile(12, Enums.SSOracle.PearlcatPebbles, false, rand);
        }
        else if (self.id == ID.Moon_Pearl_SH)
        {
            self.PebblesPearlIntro();
            self.LoadEventsFromFile(13, Enums.SSOracle.PearlcatPebbles, false, rand);
        }
        else if (self.id == ID.Moon_Pearl_DS)
        {
            self.PebblesPearlIntro();
            self.LoadEventsFromFile(14, Enums.SSOracle.PearlcatPebbles, false, rand);
        }
        else if (self.id == ID.Moon_Pearl_SB_filtration)
        {
            self.PebblesPearlIntro();
            self.LoadEventsFromFile(15, Enums.SSOracle.PearlcatPebbles, false, rand);
        }
        else if (self.id == ID.Moon_Pearl_GW)
        {
            self.PebblesPearlIntro();
            self.LoadEventsFromFile(16, Enums.SSOracle.PearlcatPebbles, false, rand);
        }
        else if (self.id == ID.Moon_Pearl_SL_bridge)
        {
            self.PebblesPearlIntro();
            self.LoadEventsFromFile(17, Enums.SSOracle.PearlcatPebbles, false, rand);
        }
        else if (self.id == ID.Moon_Pearl_SL_moon)
        {
            self.PebblesPearlIntro();
            self.LoadEventsFromFile(18, Enums.SSOracle.PearlcatPebbles, false, rand);
        }
        else if (self.id == ID.Moon_Pearl_SU)
        {
            self.PebblesPearlIntro();
            self.LoadEventsFromFile(41, Enums.SSOracle.PearlcatPebbles, false, rand);
        }
        else if (self.id == ID.Moon_Pearl_UW)
        {
            self.PebblesPearlIntro();
            self.LoadEventsFromFile(42, Enums.SSOracle.PearlcatPebbles, false, rand);
        }
        else if (self.id == ID.Moon_Pearl_SB_ravine)
        {
            self.PebblesPearlIntro();
            self.LoadEventsFromFile(43, Enums.SSOracle.PearlcatPebbles, false, rand);
        }
        else if (self.id == ID.Moon_Pearl_SL_chimney)
        {
            self.PebblesPearlIntro();
            self.LoadEventsFromFile(54, Enums.SSOracle.PearlcatPebbles, false, rand);
        }
        else if (self.id == ID.Moon_Pearl_Red_stomach)
        {
            self.PebblesPearlIntro();
            self.LoadEventsFromFile(51, Enums.SSOracle.PearlcatPebbles, false, rand);
        }
        #endregion

        #region MSC Pearls
        else if (self.id == ID.Moon_Pearl_SI_west)
        {
            self.PebblesPearlIntro();
            self.LoadEventsFromFile(20, Enums.SSOracle.PearlcatPebbles, false, rand);
        }
        else if (self.id == ID.Moon_Pearl_SI_top)
        {
            self.PebblesPearlIntro();
            self.LoadEventsFromFile(21, Enums.SSOracle.PearlcatPebbles, false, rand);
        }
        else if (self.id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_SI_chat3)
        {
            self.PebblesPearlIntro();
            self.LoadEventsFromFile(22, Enums.SSOracle.PearlcatPebbles, false, rand);
        }
        else if (self.id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_SI_chat4)
        {
            self.PebblesPearlIntro();
            self.LoadEventsFromFile(23, Enums.SSOracle.PearlcatPebbles, false, rand);
        }
        else if (self.id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_SI_chat5)
        {
            self.PebblesPearlIntro();
            self.LoadEventsFromFile(24, Enums.SSOracle.PearlcatPebbles, false, rand);
        }
        else if (self.id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_SU_filt)
        {
            self.PebblesPearlIntro();
            self.LoadEventsFromFile(101, Enums.SSOracle.PearlcatPebbles, false, rand);
        }
        else if (self.id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_DM)
        {
            self.PebblesPearlIntro();
            self.LoadEventsFromFile(102, Enums.SSOracle.PearlcatPebbles, false, rand);
        }
        else if (self.id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_LC)
        {
            self.PebblesPearlIntro();
            self.LoadEventsFromFile(103, Enums.SSOracle.PearlcatPebbles, false, rand);
        }
        else if (self.id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_OE)
        {
            self.PebblesPearlIntro();
            self.LoadEventsFromFile(104, Enums.SSOracle.PearlcatPebbles, false, rand);
        }
        else if (self.id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_MS)
        {
            self.PebblesPearlIntro();
            self.LoadEventsFromFile(105, Enums.SSOracle.PearlcatPebbles, false, rand);
        }
        else if (self.id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_Rivulet_stomach)
        {
            self.PebblesPearlIntro();
            self.LoadEventsFromFile(119, Enums.SSOracle.PearlcatPebbles, false, rand);
        }
        else if (self.id == MoreSlugcatsEnums.ConversationID.Moon_Pearl_LC_second)
        {
            self.PebblesPearlIntro();
            self.LoadEventsFromFile(121, Enums.SSOracle.PearlcatPebbles, false, rand);
        }
        #endregion

        else if (self.id == Enums.SSOracle.Pearlcat_SSConvoSSPearl)
        {
            self.PebblesPearlIntro();
            self.LoadCustomEventsFromFile("Pebbles_SS_Pearlcat");
        }
        else if (self.id == Enums.SSOracle.Pearlcat_SSConvoASPearlBlue)
        {
            self.PebblesPearlIntro();
            self.LoadCustomEventsFromFile("Pebbles_AS_PearlBlue");
        }
        else if (self.id == Enums.SSOracle.Pearlcat_SSConvoASPearlRed)
        {
            self.PebblesPearlIntro();
            self.LoadCustomEventsFromFile("Pebbles_AS_PearlRed");
        }
        else if (self.id == Enums.SSOracle.Pearlcat_SSConvoASPearlYellow)
        {
            self.PebblesPearlIntro();
            self.LoadCustomEventsFromFile("Pebbles_AS_PearlYellow");
        }
        else if (self.id == Enums.SSOracle.Pearlcat_SSConvoASPearlGreen)
        {
            self.PebblesPearlIntro();
            self.LoadCustomEventsFromFile("Pebbles_AS_PearlGreen");
        }
        else if (self.id == Enums.SSOracle.Pearlcat_SSConvoASPearlBlack)
        {
            self.PebblesPearlIntro();
            self.LoadCustomEventsFromFile("Pebbles_AS_PearlBlack");
        }

        else
        {
            orig(self); // HACK
        }
    }
}
