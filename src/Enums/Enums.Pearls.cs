namespace Pearlcat;

public static partial class Enums
{
    public static class Pearls
    {
        public static DataPearl.AbstractDataPearl.DataPearlType RM_Pearlcat { get; } = new(nameof(RM_Pearlcat), true);
        public static DataPearl.AbstractDataPearl.DataPearlType SS_Pearlcat { get; } = new(nameof(SS_Pearlcat), true);

        public static DataPearl.AbstractDataPearl.DataPearlType AS_PearlBlue { get; } = new(nameof(AS_PearlBlue), true);
        public static DataPearl.AbstractDataPearl.DataPearlType AS_PearlYellow { get; } = new(nameof(AS_PearlYellow), true);
        public static DataPearl.AbstractDataPearl.DataPearlType AS_PearlGreen { get; } = new(nameof(AS_PearlGreen), true);
        public static DataPearl.AbstractDataPearl.DataPearlType AS_PearlRed { get; } = new(nameof(AS_PearlRed), true);
        public static DataPearl.AbstractDataPearl.DataPearlType AS_PearlBlack { get; } = new(nameof(AS_PearlBlack), true);

        public static DataPearl.AbstractDataPearl.DataPearlType Heart_Pearlpup { get; } = new(nameof(Heart_Pearlpup), true);

        // Chasing Wind
        public static DataPearl.AbstractDataPearl.DataPearlType CW_Pearlcat { get; } = new(nameof(CW_Pearlcat), true);

        // Shrouded Assembly - don't register, it's not part of Pearlcat
        public static DataPearl.AbstractDataPearl.DataPearlType BigGoldenPearl { get; } = new(nameof(BigGoldenPearl));
    }
}
