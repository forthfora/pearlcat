using Music;

namespace Pearlcat;

public partial class Hooks
{
    public static void ApplySoundHooks()
    {
        On.Music.MusicPlayer.NewRegion += MusicPlayer_NewRegion;

        On.Room.PlaySound_SoundID_BodyChunk_bool_float_float_bool += Room_PlaySound_SoundID_BodyChunk_bool_float_float_bool;
    }

    private static ChunkSoundEmitter Room_PlaySound_SoundID_BodyChunk_bool_float_float_bool(On.Room.orig_PlaySound_SoundID_BodyChunk_bool_float_float_bool orig,
        Room self, SoundID soundId, BodyChunk chunk, bool loop, float vol, float pitch, bool randomStartPosition)
    {
        if (chunk.owner != null && IsPlayerObject(chunk.owner))
        {
            if (soundId == SoundID.SS_AI_Marble_Hit_Floor && PlayerObjectData.TryGetValue(chunk.owner, out var playerObjectModule) && !playerObjectModule.playCollisionSound)
                vol = 0.0f;
        }

        return orig(self, soundId, chunk, loop, vol, pitch, randomStartPosition);
    }

    public static void MusicPlayer_NewRegion(On.Music.MusicPlayer.orig_NewRegion orig, MusicPlayer self, string newRegion) => orig(self, "AS");
}
