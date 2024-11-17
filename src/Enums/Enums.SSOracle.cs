namespace Pearlcat;

public static partial class Enums
{
    public static class SSOracle
    {
        public static Conversation.ID Pearlcat_SSConvoFirstMeet = new(nameof(Pearlcat_SSConvoFirstMeet), true);
        public static Conversation.ID Pearlcat_SSConvoFirstLeave = new(nameof(Pearlcat_SSConvoFirstLeave), true);

        public static Conversation.ID Pearlcat_SSConvoRMPearlInspect = new(nameof(Pearlcat_SSConvoRMPearlInspect), true);

        public static Conversation.ID Pearlcat_SSConvoTakeRMPearl = new(nameof(Pearlcat_SSConvoTakeRMPearl), true);
        public static Conversation.ID Pearlcat_SSConvoSickPup = new(nameof(Pearlcat_SSConvoSickPup), true);

        public static Conversation.ID Pearlcat_SSConvoUnlockMira = new(nameof(Pearlcat_SSConvoUnlockMira), true);

        public static Conversation.ID Pearlcat_SSConvoFirstMeetTrueEnd = new(nameof(Pearlcat_SSConvoFirstMeetTrueEnd), true);

        public static SSOracleBehavior.Action Pearlcat_SSActionGeneral = new(nameof(Pearlcat_SSActionGeneral), true);

        public static SSOracleBehavior.SubBehavior.SubBehavID Pearlcat_SSSubBehavGeneral = new(nameof(Pearlcat_SSSubBehavGeneral), true);

        public static SlugcatStats.Name PearlcatPebbles = new(nameof(PearlcatPebbles), true);
    }
}
