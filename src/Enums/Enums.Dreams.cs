namespace Pearlcat;

public static partial class Enums
{
    public static class Dreams
    {
        public static DreamsState.DreamID Dream_Pearlcat_Pearlpup { get; } = new(nameof(Dream_Pearlcat_Pearlpup), true);
        public static DreamsState.DreamID Dream_Pearlcat_Scholar { get; } = new(nameof(Dream_Pearlcat_Scholar), true);

        public static DreamsState.DreamID Dream_Pearlcat_Pebbles { get; } = new(nameof(Dream_Pearlcat_Pebbles), true);
        public static DreamsState.DreamID Dream_Pearlcat_Moon { get; } = new(nameof(Dream_Pearlcat_Moon), true);

        public static DreamsState.DreamID Dream_Pearlcat_Sick { get; } = new(nameof(Dream_Pearlcat_Sick), true);


        public static void RegisterDreams()
        {
            SlugBase.Assets.CustomDreams.SetDreamScene(Dream_Pearlcat_Pearlpup, Scenes.Dream_Pearlcat_Pearlpup);
            SlugBase.Assets.CustomDreams.SetDreamScene(Dream_Pearlcat_Scholar, Scenes.Dream_Pearlcat_Scholar);

            SlugBase.Assets.CustomDreams.SetDreamScene(Dream_Pearlcat_Pebbles, Scenes.Dream_Pearlcat_Pebbles);
            SlugBase.Assets.CustomDreams.SetDreamScene(Dream_Pearlcat_Moon, Scenes.Dream_Pearlcat_Moon);

            SlugBase.Assets.CustomDreams.SetDreamScene(Dream_Pearlcat_Sick, Scenes.Dream_Pearlcat_Sick);
        }
    }
}
