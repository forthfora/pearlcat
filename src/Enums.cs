namespace Pearlcat;

public class Enums
{
    public static void RegisterEnums()
    {
        General.RegisterValues();
        Pearls.RegisterValues();
        Sounds.RegisterValues();
    }

    public static void UnregisterEnums()
    {
        General.UnregisterValues();
        Pearls.UnregisterValues();
        Sounds.UnregisterValues();
    }


    public class General
    {
        public static SlugcatStats.Name Pearlcat = null!;

        public static SSOracleBehavior.Action Pearlcat_SSOracleGeneral = null!;

        public static void RegisterValues()
        {
            Pearlcat = new(nameof(Pearlcat), false);
            Pearlcat_SSOracleGeneral = new(nameof(Pearlcat_SSOracleGeneral), true);
        }

        public static void UnregisterValues()
        {
            Pearlcat?.Unregister();
            Pearlcat_SSOracleGeneral?.Unregister();
        }
    }


    public class Pearls
    {
        public static DataPearl.AbstractDataPearl.DataPearlType AS_PearlBlue = null!;
        public static DataPearl.AbstractDataPearl.DataPearlType AS_PearlYellow = null!;
        public static DataPearl.AbstractDataPearl.DataPearlType AS_PearlGreen = null!;
        public static DataPearl.AbstractDataPearl.DataPearlType AS_PearlRed = null!;
        public static DataPearl.AbstractDataPearl.DataPearlType AS_PearlBlack = null!;

        public static void RegisterValues()
        {
            AS_PearlBlue = new(nameof(AS_PearlBlue), false);
            AS_PearlYellow = new(nameof(AS_PearlYellow), false);
            AS_PearlGreen = new(nameof(AS_PearlGreen), false);
            AS_PearlRed = new(nameof(AS_PearlRed), false);
            AS_PearlBlack = new(nameof(AS_PearlBlack), false);
        }

        public static void UnregisterValues()
        {
            AS_PearlBlue?.Unregister();
            AS_PearlYellow?.Unregister();
            AS_PearlGreen?.Unregister();
            AS_PearlRed?.Unregister();
            AS_PearlBlack?.Unregister();
        }
    }


    public class Sounds
    {
        public static SoundID Pearlcat_MenuCrackle = null!;
        public static SoundID Pearlcat_PearlScroll = null!;
        public static SoundID Pearlcat_PearlEquip = null!;

        public static void RegisterValues()
        {
            Pearlcat_MenuCrackle = new(nameof(Pearlcat_MenuCrackle), false);
            Pearlcat_PearlScroll = new(nameof(Pearlcat_PearlScroll), false);
            Pearlcat_PearlEquip = new(nameof(Pearlcat_PearlEquip), false);
        }

        public static void UnregisterValues()
        {
            Pearlcat_MenuCrackle?.Unregister();
            Pearlcat_PearlScroll?.Unregister();
            Pearlcat_PearlEquip?.Unregister();
        }
    }
}
