using Music;

namespace TheSacrifice;

public partial class Hooks
{
    private static void ApplyMusicHooks() => On.Music.MusicPlayer.NewRegion += MusicPlayer_NewRegion;

    private static void MusicPlayer_NewRegion(On.Music.MusicPlayer.orig_NewRegion orig, MusicPlayer self, string newRegion) => orig(self, "AS");
}
