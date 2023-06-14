using Music;

namespace Pearlcat;

public partial class Hooks
{
    public static void ApplyMusicHooks() => On.Music.MusicPlayer.NewRegion += MusicPlayer_NewRegion;

    public static void MusicPlayer_NewRegion(On.Music.MusicPlayer.orig_NewRegion orig, MusicPlayer self, string newRegion) => orig(self, "AS");
}
