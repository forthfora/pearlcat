using Music;
using System.Runtime.CompilerServices;

namespace Pearlcat;

public partial class Hooks
{
    public static void ApplySoundHooks()
    {
        //On.Music.MusicPlayer.Update += MusicPlayer_Update;
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

    private static void MusicPlayer_Update(On.Music.MusicPlayer.orig_Update orig, MusicPlayer self)
    {
        if (self.manager.currentMainLoop is RainWorldGame game)
        {
            bool hasThreatMusicPearl = false;

            foreach (var abstractCreature in game.Players)
            {
                if (abstractCreature?.realizedCreature is not Player player) continue;

                if (!player.TryGetPearlcatModule(out var playerModule)) continue;

                if (playerModule.ActiveObject == null) continue;

                var effect = playerModule.ActiveObject.GetPOEffect();

                if (effect.threatMusic != null && (PearlcatOptions.pearlThreatMusic.Value || effect.threatMusic == "AS"))
                {
                    if (self.proceduralMusic == null || self.proceduralMusic.name != effect.threatMusic)
                       self.NewRegion(effect.threatMusic);
                     
                    hasThreatMusicPearl = true;
                    break;
                }
            }

            // Stop New Threat Music
            var region = self.threatTracker?.region;

            if (!hasThreatMusicPearl && region != null && (self.proceduralMusic == null || self.proceduralMusic.name != region))
                self.NewRegion(region);
        }

        orig(self);
    }
}
