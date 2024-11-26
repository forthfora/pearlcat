namespace Pearlcat;

public static partial class Enums
{
    public static SlugcatStats.Name Pearlcat { get; } = new(nameof(Pearlcat));

    public static void InitEnums()
    {
        _ = Pearlcat;

        _ = Oracle.PearlcatPebbles;
        _ = Pearls.RM_Pearlcat;
        _ = Sounds.Pearlcat_MenuCrackle;
        _ = Scenes.Slugcat_Pearlcat;

        Dreams.RegisterDreams();
    }
}
