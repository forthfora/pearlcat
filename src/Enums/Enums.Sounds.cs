namespace Pearlcat;

public static partial class Enums
{
    public static class Sounds
    {
        public static SoundID Pearlcat_MenuCrackle { get; } = new(nameof(Pearlcat_MenuCrackle), true);
        public static SoundID Pearlcat_PearlScroll { get; } = new(nameof(Pearlcat_PearlScroll), true);

        public static SoundID Pearlcat_PearlStore { get; } = new(nameof(Pearlcat_PearlStore), true);
        public static SoundID Pearlcat_PearlRetrieve { get; } = new(nameof(Pearlcat_PearlRetrieve), true);

        public static SoundID Pearlcat_ShieldStart { get; } = new(nameof(Pearlcat_ShieldStart), true);
        public static SoundID Pearlcat_ShieldOff { get; } = new(nameof(Pearlcat_ShieldOff), true);
        public static SoundID Pearlcat_ShieldHold { get; }= new(nameof(Pearlcat_ShieldHold), true);
        
        public static SoundID Pearlcat_Heartbeat { get; } = new(nameof(Pearlcat_Heartbeat), true);
    }
}
