namespace Pearlcat;

public static partial class Enums
{
    public static SlugcatStats.Name Pearlcat = new(nameof(Pearlcat));

    public static void InitEnums()
    {
        _ = Pearlcat;

        _ = SSOracle.Pearlcat_SSActionGeneral;
        _ = Pearls.RM_Pearlcat;
        _ = Sounds.Pearlcat_PearlScroll;

        _ = Scenes.Slugcat_Pearlcat;

        Dreams.RegisterDreams();
    }
}
