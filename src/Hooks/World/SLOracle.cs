using UnityEngine;
using Random = UnityEngine.Random;

namespace Pearlcat;

// adapted from NoirCatto: https://github.com/NoirCatto/NoirCatto/blob/master/Oracles.cs
public static partial class Hooks
{
    public static void ApplySLOracleHooks()
    {
        On.SLOracleBehaviorHasMark.MoonConversation.AddEvents += MoonConversation_AddEvents;
        On.SLOracleBehaviorHasMark.NameForPlayer += SLOracleBehaviorHasMark_NameForPlayer;

        On.SLOracleBehaviorHasMark.ThirdAndUpGreeting += SLOracleBehaviorHasMark_ThirdAndUpGreeting;
    }

    private static void SLOracleBehaviorHasMark_ThirdAndUpGreeting(On.SLOracleBehaviorHasMark.orig_ThirdAndUpGreeting orig, SLOracleBehaviorHasMark self)
    {
        if (!self.oracle.room.game.IsPearlcatStory())
        {
            orig(self);
            return;
        }

        var save = self.oracle.room.game.GetMiscWorld();
        var miscProg = self.oracle.room.game.GetMiscProgression();

        var t = self.oracle.room.game.rainWorld.inGameTranslator;

        void SayStart(string text) => self.dialogBox.Interrupt(t.Translate(text), 10);

        void Say(string text) => self.dialogBox.NewMessage(t.Translate(text), 10);

        if (save?.HasPearlpupWithPlayer == true && miscProg.IsPearlpupSick && self.State.GetOpinion != SLOrcacleState.PlayerOpinion.Dislikes && !self.DamagedMode)
        {
            self.oracle.room.game.GetStorySession.TryDream(Enums.Dreams.Dream_Pearlcat_Moon_Sick);

            if (save.MoonSickPupMeetCount == 0)
            {
                SayStart("Oh! It is good to see you two again!");

                Say("Is my memory so bad to forget how your little one looks? They seem paler...");

                Say("Oh... oh no...");

                Say("They are unwell, <PlayerName>, very unwell indeed.".Replace("<PlayerName>", self.NameForPlayer(false)));

                Say("I... wish there was more I could do... but even... nevermind in my current state.");

                Say("I am so sorry.");


                Say(". . .");

                Say("My neighbour, Five Pebbles, is a little temperamental, but means well.");

                Say("He is much better equipped than me at present - I would recommend paying him a visit, if you can.<LINE>Although he was not designed as a medical facility, he may be able to aid you, in some way.");

                Say("In any case, you are welcome to stay as long as you like. Anything to ease the pain.");

                save.MoonSickPupMeetCount++;
                return;
            }
            else if (save.MoonSickPupMeetCount == 1)
            {
                SayStart("Welcome back, <PlayerName>, and your little one too.".Replace("<PlayerName>", self.NameForPlayer(false)));

                Say("I hope the cycles have been treating you well... it must be hard to take care of eachother out there.");

                Say(". . .");

                Say("...I am not sure if this is comforting, but...");

                Say("Death is not the end... even death that seems permanent.");

                Say("I know that quite well.");

                save.MoonSickPupMeetCount++;
                return;
            }
            else
            {
                SayStart("Welcome back, you two!");

                Say("I hope you are staying safe out there...");
            }
        }

        orig(self);
    }

    private static string SLOracleBehaviorHasMark_NameForPlayer(On.SLOracleBehaviorHasMark.orig_NameForPlayer orig, SLOracleBehaviorHasMark self, bool capitalized)
    {
        var result = orig(self, capitalized);

        if (!self.oracle.room.game.IsPearlcatStory())
        {
            return result;
        }

        var t = self.oracle.room.game.rainWorld.inGameTranslator;
        var miscProg = Utils.GetMiscProgression();

        string name = t.Translate("scholar");
        string prefix = miscProg.HasTrueEnding ? t.Translate("miraculous") : t.Translate("strange");

        if (self.State.GetOpinion == SLOrcacleState.PlayerOpinion.Dislikes)
        {
            name = miscProg.HasTrueEnding ? t.Translate("savage") : t.Translate("ruffian");
            prefix = t.Translate("terrible");
        }

        bool damagedSpeech = self.DamagedMode && Random.value < 0.5f;


        if (capitalized)
            prefix = string.Concat(prefix[0].ToString().ToUpper(), prefix.Substring(1));

        return prefix + (damagedSpeech ? "... " : " ") + name;
    }

    private static void MoonConversation_AddEvents(On.SLOracleBehaviorHasMark.MoonConversation.orig_AddEvents orig, SLOracleBehaviorHasMark.MoonConversation self)
    {
        if (self.currentSaveFile != Enums.Pearlcat)
        {
            orig(self);
            return;
        }

        var save = self.myBehavior.oracle.room.game.GetMiscWorld();
        var miscProg = self.myBehavior.oracle.room.game.GetMiscProgression();

        var t = self.myBehavior.oracle.room.game.rainWorld.inGameTranslator;

        void SayNoLinger(string text) => self.events.Add(new Conversation.TextEvent(self, 0, t.Translate(text), 0));

        void Say(string text, int initialWait, int textLinger) => self.events.Add(new Conversation.TextEvent(self, initialWait, t.Translate(text), textLinger));

        void Wait(int initialWait) => self.events.Add(new Conversation.WaitEvent(self, initialWait));

        if (miscProg.HasTrueEnding)
        {
            // PEARLADULT DIALOGUE
        }
        else
        {
            if (self.id == Conversation.ID.MoonFirstPostMarkConversation)
            {
                switch (Mathf.Clamp(self.State.neuronsLeft, 0, 5))
                {
                    case 2:
                        Say("Get... get away... strange... thing.", 30, 10);
                        Say("Please... this all I have left... you have so much... ...why take mine?", 0, 10);
                        return;

                    case 3:
                        Say("YOU!", 30, 10);
                        Say("...You ate... me. Please go away. I won't speak... to you.<LINE>I... CAN'T speak to you... because... you ate...me... ...and why?", 60, 0);
                        return;

                    case 5:
                        SayNoLinger("Oh, hello! Hello!");

                        SayNoLinger("You gave me quite the fright! I do not get visitors often these days;<LINE>much less visitors quite like you, who I can talk to...");


                        SayNoLinger("...you have the gift of communication!<LINE>Perhaps you paid a visit to my neighbour, Five Pebbles...?");

                        SayNoLinger("But... the mark you possess, it is very unusual.<LINE>Not something anyone in the local group could bestow...");

        
                        SayNoLinger("You must be very brave to have made it all the way here. Very brave indeed...");
          
                        Wait(10);

                        if (save?.HasPearlpupWithPlayer == true)
                        {
                            if (miscProg.IsPearlpupSick)
                            {
                                SayNoLinger("And who is your little friend? They are...");

                                SayNoLinger("Oh... oh no...");

                                SayNoLinger("They are unwell, <PlayerName>, very unwell indeed.");

                                SayNoLinger("I... wish there was more I could do... but even... nevermind in my current state.");

                                SayNoLinger("I am so sorry.");

                                SayNoLinger("You are welcome to stay as long as you like. Anything to ease the pain.");
                            
                                save.MoonSickPupMeetCount++;
                                return;
                            }
                            else
                            {
                                SayNoLinger("And who is your little friend? They are quite adorable!");

                                SayNoLinger("Ah, I hope you both stay safe out there...");
                            }
                        }
                        else
                        {
                            SayNoLinger("The power you possess, it is fascinating. I have never seen anything quite like it from your kind.");

                            SayNoLinger("Your biology... it is both familiar and foreign to me. How curious.");
                        
                            Say("If only I had my memories, I might have a clue...", 0, 5);

                            Say("But it's best not to dwell on the past, don't you agree <PlayerName>?", 0, 5);
                        }

                        Wait(10);

                        Say("Given your unique abilities, if you are curious as to the contents of those pearls -", 0, 5);

                        Say("I think I may still have some use left after all!~", 0, 5);
                        return;

                    default:
                        orig(self);
                        return;
                }
            }

            if (save?.HasPearlpupWithPlayer == true && miscProg.IsPearlpupSick && self.State.GetOpinion != SLOrcacleState.PlayerOpinion.Dislikes && self.myBehavior is SLOracleBehaviorHasMark mark && !mark.DamagedMode)
            {
                self.myBehavior.oracle.room.game.GetStorySession.TryDream(Enums.Dreams.Dream_Pearlcat_Moon_Sick);

                if (save.MoonSickPupMeetCount == 0)
                {
                    SayNoLinger("Oh! It is good to see you two again!");

                    SayNoLinger("Is my memory so bad to forget how your little one looks? They seem paler...");

                    SayNoLinger("Oh... oh no...");

                    SayNoLinger("They are unwell, <PlayerName>, very unwell indeed.");

                    SayNoLinger("I... wish there was more I could do... but even... nevermind in my current state.");

                    SayNoLinger("I am so sorry.");


                    SayNoLinger(". . .");

                    SayNoLinger("My neighbour, Five Pebbles, is a little temperamental, but means well.");

                    SayNoLinger("He is much better equipped than me at present - I would recommend paying him a visit, if you can.<LINE>Although he was not designed as a medical facility, he may be able to aid you, in some way.");

                    SayNoLinger("In any case, you are welcome to stay as long as you like. Anything to ease the pain.");
                
                    save.MoonSickPupMeetCount++;
                    return;
                }
                else if (save.MoonSickPupMeetCount == 1)
                {
                    SayNoLinger("Welcome back, <PlayerName>, and your little one too.");

                    SayNoLinger("I hope the cycles have been treating you well... it must be hard to take care of eachother out there.");

                    SayNoLinger(". . .");

                    SayNoLinger("...I am not sure if this is comforting, but...");

                    SayNoLinger("Death is not the end... even death that seems permanent.");

                    SayNoLinger("I know that quite well.");
                
                    save.MoonSickPupMeetCount++;
                    return;
                }
                else
                {
                    SayNoLinger("Welcome back, you two!");

                    SayNoLinger("I hope you are staying safe out there...");
                }
            }

        
            if (self.id == Conversation.ID.MoonSecondPostMarkConversation)
            {
                switch (Mathf.Clamp(self.State.neuronsLeft, 0, 5))
                {
                    case 4:
                        if (self.State.GetOpinion == SLOrcacleState.PlayerOpinion.Likes)
                        {
                            Say("Hello! I remember you! I remember...", 30, 0);

                            SayNoLinger("You would be a hard one to forget after all, <PlayerName>!");
                        
                            SayNoLinger("I'm still more than happy to read any pearls you bring me;<LINE>especially given your unique abilities.");

                            SayNoLinger("I do not have much else to occupy my time with these days...");
                            return;
                        }

                        if (self.State.GetOpinion == SLOrcacleState.PlayerOpinion.Dislikes)
                        {

                            Say("You. I didn't forget...", 30, 0);

                            SayNoLinger("What do you want? To take away more of my life...?");

                            SayNoLinger("Nevermind... this anger is useless...");

                            SayNoLinger("There is nothing I can do to stop you, so here is my request:<LINE>Leave me be, or end it quickly.");
                            return;
                        }

                        Say("You... I didn't forget...", 30, 0);

                        Say("I can only hope you have not come... to hurt me more.", 30, 0);
                        return;

                    case 5:
                        if (self.State.GetOpinion == SLOrcacleState.PlayerOpinion.Dislikes)
                        {
                            Say("You. Again. I still remember what you did...", 0, 10);
                            return;
                        }

                        if (self.State.GetOpinion == SLOrcacleState.PlayerOpinion.Likes)
                        {
                            Say("Hello again, <PlayerName>!", 0, 10);

                            SayNoLinger("So curious... I wonder what it is you're searching for?");

                            SayNoLinger("I have nothing for you, I'm afraid... but I hope you find the answers you seek.");

                            if (ModManager.MSC && self.myBehavior.CheckSlugpupsInRoom())
                            {
                                Say("I very much enjoy the company though... you and your little friend are always welcome here.", 0, 5);

                                Say("I would guess they will grow up into quite the scholar themself, someday...", 0, 5);
                                return;
                            }

                            if (ModManager.MMF && self.myBehavior.CheckStrayCreatureInRoom() != CreatureTemplate.Type.StandardGroundCreature)
                            {
                                Say("I do enjoy the company of you and your friend though, <PlayerName>.", 0, 5);

                                Say("You're welcome to stay a while... your ability is fascinating.", 0, 5);
                                return;
                            }

                            Say("I do enjoy the company though... it gets lonely out here.", 0, 5);

                            SayNoLinger("If you happen to have a moment to spare, I'd be more than happy to read those pearls...<LINE>There is not much else to do to ppass the time.");
                            return;
                        }

                        Say("Oh, hello <PlayerName>!", 0, 10);
                        return;

                    default:
                        orig(self);
                        return;
                }
            }
        }


        orig(self);
    }
}
