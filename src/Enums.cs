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

        public class OracleEnums
        {
            public static SSOracleBehavior.Action? TheSacrifice_General;

            public static SSOracleBehavior.MovementBehavior? TheSacrifice_SSMovement;
            public static SLOracleBehavior.MovementBehavior? TheSacrifice_SLMovement;

            public static void RegisterValues()
            {
                TheSacrifice_General = new SSOracleBehavior.Action("TheSacrifice_General", true);

                TheSacrifice_SSMovement = new SSOracleBehavior.MovementBehavior("TheSacrifice_SSMovement", true);
                TheSacrifice_SLMovement = new SLOracleBehavior.MovementBehavior("TheSacrifice_SLMovement", true);
            }

            public static void UnregisterValues()
            {
                TheSacrifice_General?.Unregister();

                TheSacrifice_SSMovement?.Unregister();
                TheSacrifice_SLMovement?.Unregister();
            }
        }
    }
}
