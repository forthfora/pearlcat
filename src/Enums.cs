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
            public static void RegisterValues() { }

            public static void UnregisterValues() { }
        }
    }
}
