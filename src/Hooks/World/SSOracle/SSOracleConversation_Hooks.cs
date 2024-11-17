using static Pearlcat.SSOracleConversation_Helpers;
using static SSOracleBehavior;

namespace Pearlcat;

public static class SSOracleConversation_Hooks
{
    public static void ApplyHooks()
    {
        On.SSOracleBehavior.PebblesConversation.AddEvents += PebblesConversation_AddEvents;
    }

    private static void PebblesConversation_AddEvents(On.SSOracleBehavior.PebblesConversation.orig_AddEvents orig, PebblesConversation self)
    {
        if (!self.owner.oracle.room.game.IsPearlcatStory() || !self.owner.IsPebbles())
        {
            orig(self);
            return;
        }

        var module = self.owner.GetModule();
        var rand = module.Rand;

        if (TryHandlePebblesConversation(self, module))
        {
            return;
        }

        if (ConvoIdFileIdMap.TryGetValue(self.id, out var fileName))
        {
            var oneRandomLine = RandomLineConvoIds.Contains(self.id);

            self.PebblesPearlIntro();
            self.LoadEventsFromFile(fileName, Enums.SSOracle.PearlcatPebbles, oneRandomLine, rand);
            return;
        }

        if (self.id.IsCustomPearlConvo())
        {
            self.PebblesPearlIntro();
            self.LoadCustomEventsFromFile("PearlcatPebbles_" + self.id.value);
            return;
        }

        orig(self);
    }
}
