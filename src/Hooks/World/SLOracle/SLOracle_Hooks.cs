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
        On.SLOracleBehaviorHasMark.InitateConversation += SLOracleBehaviorHasMarkOnInitateConversation;
        On.SLOracleBehaviorHasMark.MoonConversation.PearlIntro += MoonConversationOnPearlIntro;
    }


    private static void SLOracleBehaviorHasMarkOnInitateConversation(On.SLOracleBehaviorHasMark.orig_InitateConversation orig, SLOracleBehaviorHasMark self)
    {
        orig(self);

        if (self.oracle.room.game.IsPearlcatStory() && self.IsMoon())
        {
            if (self.currentConversation?.id == Enums.Oracle.Pearlcat_SLConvoMeeting)
            {
                return;
            }

            // Only override these, rest can be handled by the game normally
            if (self.currentConversation?.id == Conversation.ID.MoonFirstPostMarkConversation || self.currentConversation?.id == Conversation.ID.MoonSecondPostMarkConversation
                || self.State.playerEncountersWithMark >= 2) // >= 2 = ThirdAndUpGreeting
            {
                self.currentConversation = new SLOracleBehaviorHasMark.MoonConversation(Enums.Oracle.Pearlcat_SLConvoMeeting, self, SLOracleBehaviorHasMark.MiscItemType.NA);
            }
        }
    }

    private static void MoonConversation_AddEvents(On.SLOracleBehaviorHasMark.MoonConversation.orig_AddEvents orig, SLOracleBehaviorHasMark.MoonConversation self)
    {
        if (self.myBehavior?.oracle?.room?.game?.IsPearlcatStory() == false || self.myBehavior?.oracle?.IsMoon() == false)
        {
            orig(self);
            return;
        }

        var miscProg = Utils.MiscProgression;
        var miscWorld = self.myBehavior?.oracle?.abstractPhysicalObject.world.game.GetMiscWorld();

        if (miscWorld is null || self.myBehavior is not SLOracleBehaviorHasMark behavior)
        {
            orig(self);
            return;
        }

        // Custom Meeting Dialog
        if (self.id == Enums.Oracle.Pearlcat_SLConvoMeeting)
        {
            var timesMetBefore = self.State.playerEncountersWithMark;

            Plugin.Logger.LogWarning(timesMetBefore);

            // First & Second Meeting Dialog
            if (miscProg.HasTrueEnding)
            {
                if (TryHandleMoonDialog_TrueEnd(self, miscWorld.MoonTrueEndMeetCount))
                {
                    miscWorld.MoonTrueEndMeetCount++;
                    return;
                }

                // Fallback
                if (timesMetBefore < 2)
                {
                    self.id = timesMetBefore == 0 ? Conversation.ID.MoonFirstPostMarkConversation : Conversation.ID.MoonSecondPostMarkConversation;
                    orig(self);
                    return;
                }

                if (MoonTrueEndDialog_ThirdAndUpGreeting(behavior))
                {
                    miscWorld.MoonTrueEndMeetCount++;
                    return;
                }
            }
            else
            {
                if (TryHandleMoonDialog(self, timesMetBefore))
                {
                    return;
                }

                // Fallback
                if (timesMetBefore < 2)
                {
                    self.id = timesMetBefore == 0 ? Conversation.ID.MoonFirstPostMarkConversation : Conversation.ID.MoonSecondPostMarkConversation;
                    orig(self);
                    return;
                }

                if (MoonDialog_ThirdAndUpGreeting(behavior))
                {
                    return;
                }
            }

            // Fallback if wasn't handled by the custom dialog
            behavior.ThirdAndUpGreeting();
            return;
        }

        // Custom Pearl Reading (only the pearls Pearlcat adds)
        if (self.id.IsCustomPearlConvo())
        {
            self.PearlIntro();
            self.LoadCustomEventsFromFile("PearlcatMoon_" + self.id.value);
            return;
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


    // Dream after moon reads pearls
    private static void MoonConversationOnPearlIntro(On.SLOracleBehaviorHasMark.MoonConversation.orig_PearlIntro orig, SLOracleBehaviorHasMark.MoonConversation self)
    {
        orig(self);

        if (self.myBehavior?.oracle?.room?.game?.IsPearlcatStory() == true && self.myBehavior.oracle.IsMoon())
        {
            self.myBehavior.oracle.room.game.GetStorySession.TryDream(Enums.Dreams.Dream_Pearlcat_Moon, false);
        }
    }
}
