using MoreSlugcats;
using System.Linq;
using UnityEngine;
using static Pearlcat.Pearlpup_Helpers;

namespace Pearlcat;

public static class Pearlpup_Hooks
{
    public static void ApplyHooks()
    {
        On.Weapon.HitThisObject += Weapon_HitThisObject;

        On.Player.ctor += Player_ctor;
        On.Player.Update += Player_Update;

        On.Player.CanIPickThisUp += Player_CanIPickThisUp;
    }


    // Stop pearlpup stealing pearls Moon is reading
    private static bool Player_CanIPickThisUp(On.Player.orig_CanIPickThisUp orig, Player self, PhysicalObject obj)
    {
        var result = orig(self, obj);

        if (!self.IsPearlpup())
        {
            return result;
        }

        // Check room for Moon
        if (self.room?.updateList?.FirstOrDefault(x => x is Oracle { oracleBehavior: SSOracleBehavior }) is not Oracle { oracleBehavior: SLOracleBehavior slOracleBehavior })
        {
            return result;
        }

        if (obj == slOracleBehavior.holdingObject)
        {
            return false;
        }

        return result;
    }

    // If the respawn pearlpup option is enabled, make the next pup that spawns a pearlpup automatically
    private static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);

        if (!ModOptions.PearlpupRespawn.Value)
        {
            return;
        }

        if (self.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Slugpup)
        {
            return;
        }


        var miscWorld = world.game.GetMiscWorld();

        // Don't want pearlpups to spawn outside of story mode
        if (!world.game.IsStorySession)
        {
            return;
        }

        // Don't want to spawn them if the player already has a pup
        if (miscWorld?.HasPearlpupWithPlayerDeadOrAlive == true)
        {
            return;
        }

        if (miscWorld?.PearlpupID is not null)
        {
            return;
        }


        self.abstractCreature.TryMakePearlpup();
    }

    private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);

        // If pearlpup's ID and the pup with that ID fall out of sync for some reason, this should automatically convert them back into a pearlpup
        ConvertIntoPearlpupIfIdMatch(self);


        if (!self.TryGetPearlpupModule(out var module))
        {
            return;
        }

        var stats = self.slugcatStats;
        var miscProg = Utils.MiscProgression;
        

        if (self.Malnourished || miscProg.IsPearlpupSick)
        {
            stats.throwingSkill = 0;
            stats.runspeedFac = 0.9f;
            stats.corridorClimbSpeedFac = 0.9f;
            stats.poleClimbSpeedFac = 0.9f;
            stats.lungsFac = 0.1f;
        }
        else
        {
            stats.throwingSkill = 2;
            stats.runspeedFac = self.grabbedBy.Count > 0 ? 0.9f : 1.1f;
            stats.corridorClimbSpeedFac = 1.1f;
            stats.poleClimbSpeedFac = 1.15f;
            stats.lungsFac = 0.1f;
        }

        stats.generalVisibilityBonus = 0.1f;
        stats.visualStealthInSneakMode = 0.3f;
        stats.loudnessFac = 1.35f;


        if (miscProg.IsPearlpupSick)
        {
            self.redsIllness ??= new(self, 10);
            self.redsIllness.Update();
        }

        // DEBUG: for convenience
        if (self.abstractCreature.Room.world.game.devToolsActive && Input.GetKey("q"))
        {
            self.AddFood(1);
        }
    }

    // Prevent players from attacking pearlpup
    private static bool Weapon_HitThisObject(On.Weapon.orig_HitThisObject orig, Weapon self, PhysicalObject obj)
    {
        var result = orig(self, obj);

        var hitPlayerByPlayer = obj is Player && self is Spear && self.thrownBy != null && self.thrownBy is Player;

        if (hitPlayerByPlayer && obj is Player player && player.IsPearlpup())
        {
            return false;
        }

        return result;
    }
}
