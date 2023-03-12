using MoreSlugcats;

namespace TheSacrifice
{
    internal class Enums
    {
        public static void RegisterEnums()
        {
            Sounds.RegisterValues();
        }

        public static void UnregisterEnums()
        {
            Sounds.UnregisterValues();
        }

        public class Sounds
        {
            public static SoundID? StoringObject;
            public static SoundID? ObjectStored;

            public static SoundID? RetrievingObject;
            public static SoundID? ObjectRetrieved;

            public static void RegisterValues()
            {
                StoringObject = new SoundID("StoringObject", true);
                ObjectStored = new SoundID("ObjectStored", true);

                RetrievingObject = new SoundID("RetrievingObject", true);
                ObjectRetrieved = new SoundID("ObjectRetrieved", true);
            }

            public static void UnregisterValues()
            {
                StoringObject?.Unregister();
                ObjectStored?.Unregister();

                RetrievingObject?.Unregister();
                ObjectRetrieved?.Unregister();
            }
        }
    }
}
