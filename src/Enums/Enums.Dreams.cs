namespace Pearlcat;

public static partial class Enums
{
    public static class Dreams
    {
        public static DreamsState.DreamID Dream_Pearlcat_Pebbles = new(nameof(Dream_Pearlcat_Pebbles), true);
        public static DreamsState.DreamID Dream_Pearlcat_Pearlpup = new(nameof(Dream_Pearlcat_Pearlpup), true);
        public static DreamsState.DreamID Dream_Pearlcat_Moon_Sick = new(nameof(Dream_Pearlcat_Moon_Sick), true);
        public static DreamsState.DreamID Dream_Pearlcat_Sick = new(nameof(Dream_Pearlcat_Sick), true);

        public static void RegisterDreams()
        {
            SlugBase.Assets.CustomDreams.SetDreamScene(Dream_Pearlcat_Pebbles, Scenes.Dream_Pearlcat_Pebbles);
            SlugBase.Assets.CustomDreams.SetDreamScene(Dream_Pearlcat_Pearlpup, Scenes.Dream_Pearlcat_Pearlpup);
            SlugBase.Assets.CustomDreams.SetDreamScene(Dream_Pearlcat_Moon_Sick, Scenes.Dream_Pearlcat_Moon_Sick);
            SlugBase.Assets.CustomDreams.SetDreamScene(Dream_Pearlcat_Sick, Scenes.Dream_Pearlcat_Sick);
        }
    }
}