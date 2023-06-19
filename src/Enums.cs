namespace Pearlcat;

public class Enums
{
    public static void RegisterEnums()
    {
        General.RegisterValues();
        Pearls.RegisterValues();
        Sounds.RegisterValues();
        Oracles.RegisterValues();
    }

    public static void UnregisterEnums()
    {
        General.RegisterValues();
        Pearls.RegisterValues();
        Sounds.UnregisterValues();
        Oracles.UnregisterValues();
    }

    public class General
    {
        public static SlugcatStats.Name Pearlcat = null!;
        public static void RegisterValues()
        {
            Pearlcat = new(nameof(Pearlcat), false);
        }

        public static void UnregisterValues()
        {
            Pearlcat?.Unregister();
        }
    }

    public class Pearls
    {
        public static DataPearl.AbstractDataPearl.DataPearlType AS_Pearl_ThreatMusic = null!;

        public static void RegisterValues()
        {
            AS_Pearl_ThreatMusic = new(nameof(AS_Pearl_ThreatMusic), false);
        }

        public static void UnregisterValues()
        {
            AS_Pearl_ThreatMusic?.Unregister();
        }
    }

    public class Sounds
    {
        public static SoundID StoringObject = null!;
        public static SoundID ObjectStored = null!;

        public static SoundID RetrievingObject = null!;
        public static SoundID ObjectRetrieved = null!;

        public static void RegisterValues()
        {
            StoringObject = new(nameof(StoringObject), true);
            ObjectStored = new(nameof(ObjectStored), true);

            RetrievingObject = new(nameof(RetrievingObject), true);
            ObjectRetrieved = new(nameof(ObjectRetrieved), true);
        }

        public static void UnregisterValues()
        {
            StoringObject?.Unregister();
            ObjectStored?.Unregister();

            RetrievingObject?.Unregister();
            ObjectRetrieved?.Unregister();
        }
    }

    public class Oracles
    {
        public static SSOracleBehavior.Action Pearlcat_General = null!;

        public static void RegisterValues()
        {
            Pearlcat_General = new(nameof(Pearlcat_General), true);
        }

        public static void UnregisterValues()
        {
            Pearlcat_General?.Unregister();
        }
    }
}
