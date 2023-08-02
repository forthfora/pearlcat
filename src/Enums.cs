using static DataPearl.AbstractDataPearl;
using static Menu.MenuScene;
using static Menu.SlideShow;

namespace Pearlcat;

public static class Enums
{
    public static SlugcatStats.Name Pearlcat = new(nameof(Pearlcat), false);
    
    public static class SSOracle
    {
        public static Conversation.ID Pearlcat_SSConvoFirstMeet = new(nameof(Pearlcat_SSConvoFirstMeet), true);
        public static Conversation.ID Pearlcat_SSConvoFirstLeave = new(nameof(Pearlcat_SSConvoFirstLeave), true);

        public static Conversation.ID Pearlcat_SSConvoRMPearlInspect = new(nameof(Pearlcat_SSConvoRMPearlInspect), true);
        public static Conversation.ID Pearlcat_SSConvoTakeRMPearl = new(nameof(Pearlcat_SSConvoTakeRMPearl), true);

        public static Conversation.ID Pearlcat_SSConvoSSPearl = new(nameof(Pearlcat_SSConvoSSPearl), true);
        public static Conversation.ID Pearlcat_SSConvoASPearlBlue = new(nameof(Pearlcat_SSConvoASPearlBlue), true);
        public static Conversation.ID Pearlcat_SSConvoASPearlRed = new(nameof(Pearlcat_SSConvoASPearlRed), true);
        public static Conversation.ID Pearlcat_SSConvoASPearlGreen = new(nameof(Pearlcat_SSConvoASPearlGreen), true);
        public static Conversation.ID Pearlcat_SSConvoASPearlBlack = new(nameof(Pearlcat_SSConvoASPearlBlack), true);
        public static Conversation.ID Pearlcat_SSConvoASPearlYellow = new(nameof(Pearlcat_SSConvoASPearlYellow), true);
        
        public static SSOracleBehavior.Action Pearlcat_SSActionGeneral = new(nameof(Pearlcat_SSActionGeneral), true);
        public static SSOracleBehavior.SubBehavior.SubBehavID Pearlcat_SSSubBehavGeneral = new(nameof(Pearlcat_SSSubBehavGeneral), true);

        public static SlugcatStats.Name PearlcatPebbles = new(nameof(PearlcatPebbles), true);
    }

    public static class Sounds
    {
        public static SoundID Pearlcat_MenuCrackle = new(nameof(Pearlcat_MenuCrackle), true);
        public static SoundID Pearlcat_PearlScroll = new(nameof(Pearlcat_PearlScroll), true);
        
        public static SoundID Pearlcat_PearlStore = new(nameof(Pearlcat_PearlStore), true);
        public static SoundID Pearlcat_PearlRetrieve = new(nameof(Pearlcat_PearlRetrieve), true);

        public static SoundID Pearlcat_PearlRealize = new(nameof(Pearlcat_PearlRealize), true);
        public static SoundID Pearlcat_PearlAbstract = new(nameof(Pearlcat_PearlAbstract), true);

        public static SoundID Pearlcat_SpearEquip = new(nameof(Pearlcat_SpearEquip), true);
        public static SoundID Pearlcat_ShieldStart = new(nameof(Pearlcat_ShieldStart), true);

        public static SoundID Pearlcat_ShieldRecharge = new(nameof(Pearlcat_ShieldRecharge), true);
        public static SoundID Pearlcat_ShieldOff = new(nameof(Pearlcat_ShieldOff), true);

        public static SoundID Pearlcat_ShieldHold = new(nameof(Pearlcat_ShieldHold), true);
        public static SoundID Pearlcat_CamoFade = new(nameof(Pearlcat_CamoFade), true);
    }

    public static class Pearls
    {
        public static DataPearlType RM_Pearlcat = new(nameof(RM_Pearlcat), false);
        public static DataPearlType SS_Pearlcat = new(nameof(SS_Pearlcat), false);

        public static DataPearlType AS_PearlBlue = new(nameof(AS_PearlBlue), false);
        public static DataPearlType AS_PearlYellow = new(nameof(AS_PearlYellow), false);
        public static DataPearlType AS_PearlGreen = new(nameof(AS_PearlGreen), false);
        public static DataPearlType AS_PearlRed = new(nameof(AS_PearlRed), false);
        public static DataPearlType AS_PearlBlack = new(nameof(AS_PearlBlack), false);
    }

    public static class SlideShows
    {
        public static SlideShowID PearlcatIntro = new(nameof(PearlcatIntro), true);
        public static SlideShowID PearlcatOutro = new(nameof(PearlcatOutro), true);
        public static SlideShowID PearlcatAltOutro = new(nameof(PearlcatAltOutro), true);

        public static SceneID Pearlcat_Intro_1 = new(nameof(Pearlcat_Intro_1), false);
        public static SceneID Pearlcat_Intro_2 = new(nameof(Pearlcat_Intro_2), false);

        public static SceneID Pearlcat_Outro_1 = new(nameof(Pearlcat_Outro_1), false);
        public static SceneID Pearlcat_Outro_2 = new(nameof(Pearlcat_Outro_2), false);

        public static SceneID Pearlcat_AltOutro_1 = new(nameof(Pearlcat_AltOutro_1), false);
        public static SceneID Pearlcat_AltOutro_2 = new(nameof(Pearlcat_AltOutro_2), false);
    }
}
