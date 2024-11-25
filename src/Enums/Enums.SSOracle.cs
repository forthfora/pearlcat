namespace Pearlcat;

public static partial class Enums
{
    public static class SSOracle
    {
        // Used for custom pearls, it takes in a slugcat ID, this is just easier than IL hooking the method
        public static SlugcatStats.Name PearlcatPebbles { get; } = new(nameof(PearlcatPebbles), true);

        public static SSOracleBehavior.Action Pearlcat_SSActionGeneral { get; } = new(nameof(Pearlcat_SSActionGeneral), true);
        public static SSOracleBehavior.SubBehavior.SubBehavID Pearlcat_SSSubBehavGeneral { get; } = new(nameof(Pearlcat_SSSubBehavGeneral), true);

        // Conversations
        public static Conversation.ID Pearlcat_SSConvoFirstMeet { get; } = new(nameof(Pearlcat_SSConvoFirstMeet), true);
        public static Conversation.ID Pearlcat_SSConvoFirstLeave { get; } = new(nameof(Pearlcat_SSConvoFirstLeave), true);

        public static Conversation.ID Pearlcat_SSConvoRMPearlInspect { get; } = new(nameof(Pearlcat_SSConvoRMPearlInspect), true);
        public static Conversation.ID Pearlcat_SSConvoTakeRMPearl { get; } = new(nameof(Pearlcat_SSConvoTakeRMPearl), true);

        public static Conversation.ID Pearlcat_SSConvoSickPup { get; } = new(nameof(Pearlcat_SSConvoSickPup), true);
        public static Conversation.ID Pearlcat_SSConvoUnlockMira { get; } = new(nameof(Pearlcat_SSConvoUnlockMira), true);

        public static Conversation.ID Pearlcat_SSConvoFirstMeetTrueEnd { get; } = new(nameof(Pearlcat_SSConvoFirstMeetTrueEnd), true);
    }
}
