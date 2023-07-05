using static DataPearl.AbstractDataPearl;

namespace Pearlcat;

public static class Enums
{
    public static class General
    {
        public static SlugcatStats.Name Pearlcat = new(nameof(Pearlcat), false);

        public static SSOracleBehavior.Action Pearlcat_SSOracleGeneral = new(nameof(Pearlcat_SSOracleGeneral), true);
    }

    public static class Pearls
    {
        public static DataPearlType AS_PearlBlue = new(nameof(AS_PearlBlue), false);
        public static DataPearlType AS_PearlYellow = new(nameof(AS_PearlYellow), false);
        public static DataPearlType AS_PearlGreen = new(nameof(AS_PearlGreen), false);
        public static DataPearlType AS_PearlRed = new(nameof(AS_PearlRed), false);
        public static DataPearlType AS_PearlBlack = new(nameof(AS_PearlBlack), false);
    }

    public static class Sounds
    {
        public static SoundID Pearlcat_MenuCrackle = new(nameof(Pearlcat_MenuCrackle), true);
        public static SoundID Pearlcat_PearlScroll = new(nameof(Pearlcat_PearlScroll), true);
        public static SoundID Pearlcat_PearlStore = new(nameof(Pearlcat_PearlStore), true);
        public static SoundID Pearlcat_PearlRetrieve = new(nameof(Pearlcat_PearlRetrieve), true);
    }
}
