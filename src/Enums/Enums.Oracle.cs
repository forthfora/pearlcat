namespace Pearlcat;

public static partial class Enums
{
    public static class Oracle
    {
        // Used for Pebbles reading pearls, it takes in a slugcat ID and the normal ID is already used for Moon, making a new ID is just easier than IL hooking the method
        public static SlugcatStats.Name PearlcatPebbles { get; } = new(nameof(PearlcatPebbles), true);

        // Pebbles sub bh
        public static SSOracleBehavior.Action Pearlcat_SSActionGeneral { get; } = new(nameof(Pearlcat_SSActionGeneral), true);
        public static SSOracleBehavior.SubBehavior.SubBehavID Pearlcat_SSSubBehavGeneral { get; } = new(nameof(Pearlcat_SSSubBehavGeneral), true);

        // Pebbles Conversations
        public static Conversation.ID Pearlcat_SSConvoFirstMeet { get; } = new(nameof(Pearlcat_SSConvoFirstMeet), true);
        public static Conversation.ID Pearlcat_SSConvoFirstLeave { get; } = new(nameof(Pearlcat_SSConvoFirstLeave), true);

        public static Conversation.ID Pearlcat_SSConvoRMPearlInspect { get; } = new(nameof(Pearlcat_SSConvoRMPearlInspect), true);
        public static Conversation.ID Pearlcat_SSConvoTakeRMPearl { get; } = new(nameof(Pearlcat_SSConvoTakeRMPearl), true);

        public static Conversation.ID Pearlcat_SSConvoSickPup { get; } = new(nameof(Pearlcat_SSConvoSickPup), true);
        public static Conversation.ID Pearlcat_SSConvoUnlockWellGate { get; } = new(nameof(Pearlcat_SSConvoUnlockWellGate), true);

        public static Conversation.ID Pearlcat_SSConvoFirstMeetTrueEnd { get; } = new(nameof(Pearlcat_SSConvoFirstMeetTrueEnd), true);


        // Moon Conversations
        public static Conversation.ID Pearlcat_SLConvoMeeting { get; } = new(nameof(Pearlcat_SLConvoMeeting), true);
    }
}
