using static Pearlcat.SLOracle_Helpers;
using Random = UnityEngine.Random;

namespace Pearlcat;

// adapted from NoirCatto: https://github.com/NoirCatto/NoirCatto/blob/master/Oracles.cs
public static class SLOracle_Hooks
{
    public static void ApplyHooks()
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

        if (save?.HasPearlpupWithPlayer == true && miscProg.IsPearlpupSick && self.State.GetOpinion != SLOrcacleState.PlayerOpinion.Dislikes && !self.DamagedMode)
        {
            self.oracle.room.game.GetStorySession.TryDream(Enums.Dreams.Dream_Pearlcat_Moon_Sick);

            if (save.MoonSickPupMeetCount == 0)
            {
                self.Dialog_Start("Oh! It is good to see you two again!");

                self.Dialog("Is my memory so bad to forget how your little one looks? They seem paler...");

                self.Dialog("Oh... oh no...");

                self.Dialog("They are unwell, <PlayerName>, very unwell indeed.");

                self.Dialog("I... wish there was more I could do... but even... nevermind in my current state.");

                self.Dialog("I am so sorry.");


                self.Dialog(". . .");

                self.Dialog("My neighbour, Five Pebbles, is a little temperamental, but means well.");

                self.Dialog("He is much better equipped than me at present - I would recommend paying him a visit, if you can.<LINE>Although he was not designed as a medical facility, he may be able to aid you, in some way.");

                self.Dialog("In any case, you are welcome to stay as long as you like. Anything to ease the pain.");

                save.MoonSickPupMeetCount++;
            }
            else if (save.MoonSickPupMeetCount == 1)
            {
                self.Dialog_Start("Welcome back, <PlayerName>, and your little one too.");

                self.Dialog("I hope the cycles have been treating you well... it must be hard to take care of eachother out there.");

                self.Dialog(". . .");

                self.Dialog("...I am not sure if this is comforting, but...");

                self.Dialog("Death is not the end... even death that seems permanent.");

                self.Dialog("I know that quite well.");

                save.MoonSickPupMeetCount++;
            }
            else
            {
                self.Dialog_Start("Welcome back, you two!");

                self.Dialog("I hope you are staying safe out there...");
            }
        }
        else if (miscProg.HasTrueEnding && self.State.GetOpinion != SLOrcacleState.PlayerOpinion.Dislikes && !self.DamagedMode)
        {
            self.Dialog_Start("Welcome back, <PlayerName>!");

            self.Dialog("Ah, you didn't happen to find a way to tell me about your travels, did you?");

            self.Dialog("I'm kidding, of course! My imagination will suffice, though the curiosity does burn me up inside...");

            self.Dialog("For now, my only lens into the outside is those pearls you carry.");

            self.Dialog("So please, bring me more, as long as it isn't too dangerous for you... these visits really are the highlight of my days here...");
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
        if (self.myBehavior?.oracle?.room?.game?.IsPearlcatStory() == false || self.myBehavior?.oracle?.IsMoon() == false)
        {
            orig(self);
            return;
        }

        var miscProg = Utils.GetMiscProgression();

        if (miscProg.HasTrueEnding)
        {
            if (TryHandleMoonDialog_TrueEnd(self))
            {
                return;
            }
        }
        else
        {
            if (TryHandleMoonDialog(self))
            {
                return;
            }
        }

        if (self.id.IsCustomPearlConvo())
        {
            self.PearlIntro();
            self.LoadCustomEventsFromFile("PearlcatMoon_" + self.id.value);
            return;
        }

        orig(self);
    }
}
