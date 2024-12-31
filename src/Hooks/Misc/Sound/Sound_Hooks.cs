using Music;
using System.Linq;
using Random = UnityEngine.Random;

namespace Pearlcat;

public static class Sound_Hooks
{
    public static void ApplyHooks()
    {
        On.Music.MusicPlayer.Update += MusicPlayer_Update;
        On.Music.MusicPlayer.NewRegion += MusicPlayer_NewRegion;

        On.Room.PlaySound_SoundID_BodyChunk_bool_float_float_bool += Room_PlaySound_SoundID_BodyChunk_bool_float_float_bool;
    }


    private static ChunkSoundEmitter Room_PlaySound_SoundID_BodyChunk_bool_float_float_bool(On.Room.orig_PlaySound_SoundID_BodyChunk_bool_float_float_bool orig,
        Room self, SoundID soundId, BodyChunk chunk, bool loop, float vol, float pitch, bool randomStartPosition)
    {
        if (chunk.owner is not null && chunk.owner.abstractPhysicalObject.IsPlayerPearl())
        {
            if (soundId == SoundID.SS_AI_Marble_Hit_Floor && chunk.owner.abstractPhysicalObject.TryGetPlayerPearlModule(out var module) && !module.PlayCollisionSound)
            {
                vol = 0.0f;
            }
        }

        return orig(self, soundId, chunk, loop, vol, pitch, randomStartPosition);
    }


    // Fix subregion specific tracks
    private static void MusicPlayer_NewRegion(On.Music.MusicPlayer.orig_NewRegion orig, MusicPlayer self, string newRegion)
    {
        var module = self.GetModule();

        module.Subregion = null;

        if (module.IsPearlPlaying)
        {
            module.Subregion = newRegion switch
            {
                "CC" => Random.value > 0.5f ? "Chimney Canopy" : "The Gutter",
                _ => null,
            };
        }

        module.IsPearlPlaying = false;

        orig(self, newRegion);
    }

    private static void MusicPlayer_Update(On.Music.MusicPlayer.orig_Update orig, MusicPlayer self)
    {
        if (!ModOptions.PearlThreatMusic.Value)
        {
            orig(self);
            return;
        }

        var module = self.GetModule();

        if (self.manager.currentMainLoop is RainWorldGame game && game.Players.Any(x => x.realizedCreature is Player player && player.IsPearlcat()))
        {
            var region = self.threatTracker?.region;
            var hasThreatMusicPearl = false;
            
            foreach (var abstractCreature in game.Players)
            {
                if (abstractCreature?.realizedCreature is not Player player)
                {
                    continue;
                }

                if (!player.TryGetPearlcatModule(out var playerModule))
                {
                    continue;
                }

                if (playerModule.ActiveObject is null)
                {
                    continue;
                }

                var effect = playerModule.ActiveObject.GetPearlEffect();

                if (effect.ThreatMusic is not null)
                {
                    if (self.proceduralMusic is null || (self.nextProcedural != effect.ThreatMusic && self.proceduralMusic.instruction.name != effect.ThreatMusic))
                    {
                        module.WasThreatPearlActive = true;

                        if (self.proceduralMusic?.instruction?.name == region)
                        {
                            module.IsPearlPlaying = true;
                            self.NewRegion(effect.ThreatMusic);
                            //Plugin.Logger.LogWarning("START PEARL THREAT " + effect.ThreatMusic);
                        }

                    }

                    hasThreatMusicPearl = true;
                    break;
                }
            }

            // Stop New Threat Music
            if (!hasThreatMusicPearl && module.WasThreatPearlActive)
            {
                if (region is not null)
                {
                    self.NewRegion(region);
                    //Plugin.Logger.LogWarning("STOP PEARL THREAT");
                }

                module.WasThreatPearlActive = false;
            }
        }

        orig(self);
    }
}
