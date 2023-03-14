
namespace TheSacrifice
{
    internal static partial class Hooks
    {
        private static void ApplyOracleHooks()
        {
            On.SSOracleBehavior.ctor += SSOracleBehavior_ctor;
            On.SSOracleBehavior.Update += SSOracleBehavior_Update;
        }

        private const string SS_ORACLE_ROOM = "SS_AI";
        private const string DM_ORACLE_ROOM = "DM_AI";

        private static SSCustomBehavior? ssCustomBehavior;

        private static void SSOracleBehavior_ctor(On.SSOracleBehavior.orig_ctor orig, SSOracleBehavior self, Oracle oracle)
        {
            orig(self, oracle);

            ssCustomBehavior = null;
        }
        private static void SSOracleBehavior_Update(On.SSOracleBehavior.orig_Update orig, SSOracleBehavior self, bool eu)
        {
            orig(self, eu);

            if (self.player?.room?.roomSettings != null
                && (self.player.room.roomSettings.name.Contains(SS_ORACLE_ROOM)
                || self.player.room.roomSettings.name.Contains(DM_ORACLE_ROOM)))
            {
                ssCustomBehavior ??= new();
            }

            if ((self.player?.room?.roomSettings == null
                || !self.player.room.roomSettings.name.Contains(SS_ORACLE_ROOM)
                || !self.player.room.roomSettings.name.Contains(DM_ORACLE_ROOM))
                && ssCustomBehavior != null
                && ssCustomBehavior.superState == SSCustomBehavior.SuperState.Stopped)
            {
                ssCustomBehavior = null;
            }

            ssCustomBehavior?.Update(self);
        }
    }
}
