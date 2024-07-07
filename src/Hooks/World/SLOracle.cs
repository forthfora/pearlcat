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
        if (!self.oracle.room.game.IsPearlcatStory() || !self.IsMoon())
        {
            orig(self);
            return;
        }

        var save = self.oracle.room.game.GetMiscWorld();
        var miscProg = Utils.GetMiscProgression();

        var t = self;

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

                Say("They are unwell, <PlayerName>, very unwell indeed.");

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
                SayStart("Welcome back, <PlayerName>, and your little one too.");

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

        else if (miscProg.HasTrueEnding && self.State.GetOpinion != SLOrcacleState.PlayerOpinion.Dislikes && !self.DamagedMode)
        {
            SayStart("Welcome back, <PlayerName>!");

            Say("Ah, you didn't happen to find a way to tell me about your travels, did you?");

            Say("I'm kidding, of course! My imagination will suffice, though the curiosity does burn me up inside...");

            Say("For now, my only lens into the outside is those pearls you carry.");

            Say("So please, bring me more, as long as it isn't too dangerous for you... these visits really are the highlight of my days here...");
        }

        orig(self);
    }

    private static string SLOracleBehaviorHasMark_NameForPlayer(On.SLOracleBehaviorHasMark.orig_NameForPlayer orig, SLOracleBehaviorHasMark self, bool capitalized)
    {
        if (!self.oracle.room.game.IsPearlcatStory() || !self.IsMoon())
        {
            return orig(self, capitalized);
        }

        var t = Utils.Translator;

        var prefix = t.Translate("strange");
        var name = t.Translate("scholar");

        if (self.State.GetOpinion == SLOrcacleState.PlayerOpinion.Dislikes)
        {
            prefix = t.Translate("acursed");
            name = t.Translate("thing");
        }

        var damagedSpeech = self.DamagedMode && Random.value < 0.5f;

        if (capitalized)
        {
            prefix = string.Concat(prefix[0].ToString().ToUpper(), prefix.Substring(1));
        }

        return prefix + (damagedSpeech ? t.Translate("... ") : " ") + name;
    }

    private static void MoonConversation_AddEvents(On.SLOracleBehaviorHasMark.MoonConversation.orig_AddEvents orig, SLOracleBehaviorHasMark.MoonConversation self)
    {
        var save = self.myBehavior.oracle.room.game.GetMiscWorld();
        var miscProg = Utils.GetMiscProgression();

        if (self.currentSaveFile != Enums.Pearlcat || self.myBehavior.oracle.IsMoon() || save == null)
        {
            orig(self);
            return;
        }

        var metMoon = self.myBehavior.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.EverMetMoon;

        var t = self.myBehavior;

        void SayNoLinger(string text) => self.events.Add(new Conversation.TextEvent(self, 0, t.Translate(text), 0));

        void Say(string text, int initialWait, int textLinger) => self.events.Add(new Conversation.TextEvent(self, initialWait, t.Translate(text), textLinger));

        void Wait(int initialWait) => self.events.Add(new Conversation.WaitEvent(self, initialWait));


        // PEARLPUP
        if (miscProg.HasTrueEnding)
        {
            // Moon never mmet Pearlcat
            if (!metMoon)
            {
                if (self.id == Conversation.ID.MoonFirstPostMarkConversation)
                {
                    switch (Mathf.Clamp(self.State.neuronsLeft, 0, 5))
                    {
                        case 2:
                            Say("...hello?", 30, 10);
                            Say("...help... please...", 0, 10);
                            return;

                        case 3:
                            Say("Who... who is there?", 30, 10);
                            Say("I have so little... please don't take... this... this is all I have...", 0, 10);
                            return;

                        case 5:
                            SayNoLinger("Oh...? Hello! Hello...");

                            Wait(10);

                            SayNoLinger("...are you alright, little one? The modifications to your body are quite... extreme.");

                            Wait(5);

                            SayNoLinger("Sorry, I shouldn't be so intrusive, I am in quite the state myself after all.");

                            Say("You are just a lot to take in for little old me! A fascinating power, the gift of communication;", 0, 5);

                            Say("And a nasty scar, poor thing... I can't imagine where you got that, or even where you came from...?", 0, 5);

                            Say("You seem to be doing well for yourself nonetheless, your tenacity from whatever occurred is admirable!", 0, 5);

                            Wait(5);

                            Say("...well, given your unique set of skills... if you ever need something read...", 0, 5);

                            Say("I'll be waiting right here - not like I can go far, anyways~", 0, 5);
                            return;

                        default:
                            orig(self);
                            return;
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

                                SayNoLinger("You would be a hard one to forget after all, strange scholar!");

                                SayNoLinger("I'm still more than happy to read any pearls you bring me;<LINE>especially given your unique abilities.");

                                SayNoLinger("I do not have much else to occupy my time with these days...");
                                return;
                            }

                            if (self.State.GetOpinion == SLOrcacleState.PlayerOpinion.Dislikes)
                            {

                                Say("You. I didn't forget...", 30, 0);

                                SayNoLinger("What do you want? To take away more of my life...?");

                                SayNoLinger("Look at yourself! You know how it feels, to be within an inch of death...");

                                SayNoLinger("There is nothing I can do to stop you, so here is my request:<LINE>Leave me be, or end it quickly.");
                                return;
                            }

                            Say("Hello again, strange scholar...", 30, 0);

                            Say("Please try not to push me too hard; with the number of neurons I have... let's just say my mind gets a little foggy.", 30, 0);
                            return;

                        case 5:
                            if (self.State.GetOpinion == SLOrcacleState.PlayerOpinion.Dislikes)
                            {
                                Say("You... I still remember what you did.", 0, 10);

                                Say("It would be pointless to ask why...", 0, 10);

                                Say("I've accepted my fate - and one day, you will meet an end too...", 0, 10);
                                return;
                            }

                            if (self.State.GetOpinion == SLOrcacleState.PlayerOpinion.Likes)
                            {
                                Say("Hello again, strange scholar!", 0, 10);

                                SayNoLinger("So curious... I wonder what it is you're searching for?");

                                SayNoLinger("I have nothing for you, I'm afraid... but I hope you find the answers you seek.");

                                if (ModManager.MSC && self.myBehavior.CheckSlugpupsInRoom())
                                {
                                    Say("I very much enjoy the company though... you and your family are always welcome here.", 0, 5);
                                    return;
                                }

                                if (ModManager.MMF && self.myBehavior.CheckStrayCreatureInRoom() != CreatureTemplate.Type.StandardGroundCreature)
                                {
                                    Say("I do enjoy the company of you and your friend though, strange scholar.", 0, 5);

                                    Say("You're welcome to stay a while... your ability is fascinating.", 0, 5);
                                    return;
                                }

                                Say("I do enjoy the company though... it gets lonely out here.", 0, 5);

                                SayNoLinger("If you happen to have a moment to spare, I'd be more than happy to read those pearls...<LINE>There is not much else to do to pass the time.");
                                return;
                            }

                            Say("Oh, hello strange scholar!", 0, 10);
                            return;

                        default:
                            orig(self);
                            return;
                    }
                }
            }

            // Moon met Pearlcat but never met sick Pearlpup
            else if (save.MoonSickPupMeetCount == 0)
            {
                if (self.id == Conversation.ID.MoonFirstPostMarkConversation)
                {
                    switch (Mathf.Clamp(self.State.neuronsLeft, 0, 5))
                    {
                        case 2:
                            Say("...hello?", 30, 10);

                            Say("...familiar ...help... please...", 0, 10);
                            return;

                        case 3:
                            Say("Who... who is there?", 30, 10);

                            Say("I have so little... please don't take... this... this is all I have...", 0, 10);

                            Wait(10);

                            Say("You are... so familiar...", 0, 10);
                            return;

                        case 5:
                            SayNoLinger("Oh, hello there!");

                            Wait(10);

                            SayNoLinger("...are you alright, little one? The modifications to your body are quite... unusual.");

                            Wait(5);

                            SayNoLinger("Sorry, I shouldn't be so intrusive, I am in quite the state myself after all.");

                            Wait(5);

                            Say("Ah... you seem, familiar...? ...but in such a strange way...", 0, 5);

                            Say("A scholar with the ability to manipulate pearls...? Oh... yes! I remember!", 0, 5);

                            Say("...but, you are not like I remember them? Am I misremembering so badly?", 0, 5);

                            Wait(10);

                            Say("...did they have a child? ...could you be?", 0, 5);

                            Wait(10);

                            Say("Well, if my memory is serving me correctly, it is nice to see you again... strange scholar.", 0, 5);

                            Say("Has it really been so long...? And considering your current state, you've been through a lot...", 0, 5);

                            SayNoLinger("Ah, I won't lie... I am curious as to how you ended up back here, and what happened on your travels...");

                            SayNoLinger("...and what happened to your parent...");

                            Wait(5);

                            Say("Ah, I'm sorry... it's wrong to be so invasive, and I shouldn't be so sentimental anyways.", 0, 5);

                            Say("I won't be going anywhere any time soon... if you need company, you know who to visit!", 0, 5);
                            return;

                        default:
                            orig(self);
                            return;
                    }
                }

                if (self.id == Conversation.ID.MoonSecondPostMarkConversation)
                {
                    switch (Mathf.Clamp(self.State.neuronsLeft, 0, 5))
                    {
                        case 4:
                            if (self.State.GetOpinion == SLOrcacleState.PlayerOpinion.Likes)
                            {
                                Say("Hello! Strange scholar, it is good to see you!", 30, 0);

                                SayNoLinger("Was it really so long ago that you were only half my height...?");

                                SayNoLinger("I'm still more than happy to read any pearls you bring me;<LINE>especially given your unique abilities.");

                                SayNoLinger("Sorry if I seem a little slow by the way, we are designed to operate with millions of neurons, as opposed to, well...");
                                return;
                            }

                            if (self.State.GetOpinion == SLOrcacleState.PlayerOpinion.Dislikes)
                            {
                                Say("You...", 30, 0);

                                SayNoLinger("What do you want? To take away more of my life...?");

                                SayNoLinger("Look at yourself! You know how it feels, to be within an inch of death...");

                                SayNoLinger("I... I have nothing more to say...");
                                return;
                            }

                            Say("Hello again, strange scholar...", 30, 0);

                            Say("Please try not to push me too hard; with the number of neurons I have... let's just say my mind gets a little foggy.", 30, 0);
                            return;

                        case 5:
                            if (self.State.GetOpinion == SLOrcacleState.PlayerOpinion.Dislikes)
                            {
                                Say("You... I still remember what you did.", 0, 10);

                                Say("It would be pointless to ask why...", 0, 10);

                                Say("I've accepted my fate - and one day, you will meet an end too...", 0, 10);
                                return;
                            }

                            if (self.State.GetOpinion == SLOrcacleState.PlayerOpinion.Likes)
                            {
                                Say("Hello again, strange scholar!", 0, 10);

                                SayNoLinger("I do so wish you could tell me what happened on your travels... I hardly remember the last time I saw beyond this chamber...");

                                SayNoLinger("Oh... the memories can hurt a little... but I shouldn't well on them.");

                                if (ModManager.MSC && self.myBehavior.CheckSlugpupsInRoom())
                                {
                                    Say("You and your family are always welcome here - please visit often!", 0, 5);
                                    return;
                                }

                                else if (ModManager.MMF && self.myBehavior.CheckStrayCreatureInRoom() != CreatureTemplate.Type.StandardGroundCreature)
                                {
                                    Say("The company of you and your friend makes my day, strange scholar.", 0, 5);

                                    Say("You're more than welcome to stay a while... your ability will always be a miracle to me...", 0, 5);
                                    return;
                                }

                                Say("I'll always enjoy your company... it gets lonely out here.", 0, 5);

                                SayNoLinger("If you happen to have a moment to spare, I'd be more than happy to read those pearls...<LINE>There is not much else to do to pass the time.");
                                return;
                            }

                            Say("Oh, hello strange scholar!", 0, 10);

                            SayNoLinger("You really do remind me so much of your mother...");
                            return;

                        default:
                            orig(self);
                            return;
                    }
                }
            }

            // Moon met Pearlcat & sick Pearlpup
            else
            {
                if (self.id == Conversation.ID.MoonFirstPostMarkConversation)
                {
                    switch (Mathf.Clamp(self.State.neuronsLeft, 0, 5))
                    {
                        case 2:
                            Say("...hello?", 30, 10);

                            Say("...familiar ...help... please...", 0, 10);
                            return;

                        case 3:
                            Say("Who... who is there?", 30, 10);

                            Say("I have so little... please don't take... this... this is all I have...", 0, 10);

                            Wait(10);

                            Say("You are... so familiar...", 0, 10);

                            Say("...scholar?", 0, 10);
                            return;

                        case 5:
                            SayNoLinger("Oh, hello! Hello...");

                            Wait(10);

                            SayNoLinger("...are you alright, little one? The modifications to your body are quite... extreme.");

                            Wait(5);

                            SayNoLinger("Sorry, I shouldn't be so intrusive, I am in quite the state myself after all.");

                            Wait(5);

                            Say("Ah... you seem, familiar...? ...but in such a strange way...", 0, 5);

                            Say("A scholar with the ability to manipulate pearls...? Oh... yes! I remember!", 0, 5);

                            Say("...but, you are not like I remember them? Am I misremembering so badly?", 0, 5);

                            Wait(10);

                            Say("...did they have a child? ...could you be?", 0, 5);

                            Wait(10);

                            Say("Is that you, little scholar? I'm speechless...", 0, 5);

                            Say("Has it really been so long...? And considering your current state, you've been through a lot...", 0, 5);

                            SayNoLinger("I won't lie, I'm so curious as to how you... survived, and how you ended up back here...?");

                            SayNoLinger("...and what happened to your parent...");

                            Wait(5);

                            Say("I'm sorry... it's wrong to be so invasive, it's none of my business really.", 0, 5);

                            Say("I'm just so glad you're safe, too many things have come into my memory, only to be washed away...", 0, 5);

                            Say("Please, stay as long as you'd like, I can't begin to imagine what you've been through.", 0, 5);

                            Say("I still don't have much to offer, sadly... aside from my pearl readings, and company of course! ~", 0, 5);
                            return;

                        default:
                            orig(self);
                            return;
                    }
                }

                if (self.id == Conversation.ID.MoonSecondPostMarkConversation)
                {
                    switch (Mathf.Clamp(self.State.neuronsLeft, 0, 5))
                    {
                        case 4:
                            if (self.State.GetOpinion == SLOrcacleState.PlayerOpinion.Likes)
                            {
                                Say("Hello! Strange scholar, it is good to see you!", 30, 0);

                                SayNoLinger("Was it really so long ago that you were only half my height...?");

                                SayNoLinger("I'm still more than happy to read any pearls you bring me;<LINE>especially given your unique abilities.");

                                SayNoLinger("Sorry if I seem a little slow by the way, we are designed to operate with millions of neurons, as opposed to, well...");
                                return;
                            }

                            if (self.State.GetOpinion == SLOrcacleState.PlayerOpinion.Dislikes)
                            {
                                Say("You...", 30, 0);

                                SayNoLinger("What do you want? To take away more of my life...?");

                                SayNoLinger("Look at yourself! You know how it feels, to be within an inch of death...");

                                SayNoLinger("I... I have nothing more to say...");
                                return;
                            }

                            Say("Hello again, strange scholar...", 30, 0);

                            Say("Please try not to push me too hard; with the number of neurons I have... let's just say my mind gets a little foggy.", 30, 0);
                            return;

                        case 5:
                            if (self.State.GetOpinion == SLOrcacleState.PlayerOpinion.Dislikes)
                            {
                                Say("You... I still remember what you did.", 0, 10);

                                Say("It would be pointless to ask why...", 0, 10);

                                Say("I've accepted my fate - and one day, you will meet an end too...", 0, 10);
                                return;
                            }

                            if (self.State.GetOpinion == SLOrcacleState.PlayerOpinion.Likes)
                            {
                                Say("Hello again, strange scholar!", 0, 10);

                                SayNoLinger("I do so wish you could tell me what happened on your travels... I hardly remember the last time I saw beyond this chamber...");

                                SayNoLinger("Oh... the memories can hurt a little... but I shouldn't well on them.");

                                if (ModManager.MSC && self.myBehavior.CheckSlugpupsInRoom())
                                {
                                    Say("You and your family are always welcome here - please visit often!", 0, 5);
                                    return;
                                }

                                else if (ModManager.MMF && self.myBehavior.CheckStrayCreatureInRoom() != CreatureTemplate.Type.StandardGroundCreature)
                                {
                                    Say("The company of you and your friend makes my day, strange scholar.", 0, 5);

                                    Say("You're more than welcome to stay a while... your ability will always be a little miracle to me...", 0, 5);
                                    return;
                                }

                                Say("I'll always enjoy your company... it gets lonely out here.", 0, 5);

                                SayNoLinger("If you happen to have a moment to spare, I'd be more than happy to read those pearls...<LINE>There is not much else to do to pass the time.");
                                return;
                            }

                            Say("Oh, hello strange scholar!", 0, 10);

                            SayNoLinger("You really do remind me so much of your mother...");
                            return;

                        default:
                            orig(self);
                            return;
                    }
                }
            }
        }


        // PEARLCAT
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

                            SayNoLinger("If you happen to have a moment to spare, I'd be more than happy to read those pearls...<LINE>There is not much else to do to pass the time.");
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