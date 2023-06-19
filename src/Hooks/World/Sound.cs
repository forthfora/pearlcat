using Music;

namespace Pearlcat;

public partial class Hooks
{
    public static void ApplySoundHooks()
    {
        On.Music.MusicPlayer.Update += MusicPlayer_Update;

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
        orig(self);

        if (self.manager.currentMainLoop is not RainWorldGame game) return;

        bool customThreatTheme = false;

        foreach (var abstractCreature in game.Players)
        {
            if (abstractCreature?.realizedCreature is not Player player) continue;

            if (!player.TryGetPearlcatModule(out var playerModule)) continue;

            if (playerModule.ActiveObject == null) continue;

            if (self.proceduralMusic == null) continue;


            if (playerModule.ActiveObject.GetPOEffect().ASThreatMusic && self.proceduralMusic.instruction.name != "AS")
            {
                self.NewRegion("AS");
                customThreatTheme = true;
                break;
            }
        }

        if (self.proceduralMusic != null && self.proceduralMusic.instruction.name == "AS" && !customThreatTheme)
            self.threatTracker.region = "";

        Plugin.Logger.LogWarning(customThreatTheme);
        Plugin.Logger.LogWarning(self.threatTracker.region);
    }
}
