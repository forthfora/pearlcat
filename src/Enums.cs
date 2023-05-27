namespace Pearlcat;

public class Enums
{
    public static void RegisterEnums()
    {
        Slugcat.RegisterValues();
        Sounds.RegisterValues();
        Oracles.RegisterValues();
    }

    public static void UnregisterEnums()
    {
        Slugcat.RegisterValues();
        Sounds.UnregisterValues();
        Oracles.UnregisterValues();
    }

    public class Slugcat
    {
        public static SlugcatStats.Name Pearlcat = null!;

        public static void RegisterValues()
        {
            Pearlcat = new SlugcatStats.Name(nameof(Pearlcat), false);
        }

        public static void UnregisterValues()
        {
            Pearlcat?.Unregister();
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
            StoringObject = new SoundID(nameof(StoringObject), true);
            ObjectStored = new SoundID(nameof(ObjectStored), true);

            RetrievingObject = new SoundID(nameof(RetrievingObject), true);
            ObjectRetrieved = new SoundID(nameof(ObjectRetrieved), true);
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
            Pearlcat_General = new SSOracleBehavior.Action(nameof(Pearlcat_General), true);
        }

        public static void UnregisterValues()
        {
            Pearlcat_General?.Unregister();
        }
    }
}
