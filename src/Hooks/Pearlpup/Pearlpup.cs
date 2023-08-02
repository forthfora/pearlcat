
using System.Linq;
using UnityEngine;

namespace Pearlcat;

public static partial class Hooks
{
    public static void ApplyPearlpupHooks()
    {
        On.Weapon.HitThisObject += Weapon_HitThisObject;

        On.Player.ctor += Player_ctor;
        On.Player.Update += Player_UpdatePearlpup;

        On.Player.CanIPickThisUp += Player_CanIPickThisUp;
    }

    private static bool Player_CanIPickThisUp(On.Player.orig_CanIPickThisUp orig, Player self, PhysicalObject obj)
    {
        var result = orig(self, obj);

        if (self.room.updateList.FirstOrDefault(x => x is Oracle) is Oracle oracle && oracle.oracleBehavior is SLOracleBehavior behavior)
            if (self.IsPearlpup() && obj == behavior.holdingObject)
                return false;

        return result;
    }

    private static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);

        if (self.SlugCatClass != MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Slugpup) return;

        var save = world.game.GetMiscWorld();

        if (!world.game.IsStorySession || save?.PearlpupID != null) return;

        self.MakePearlpup();
    }

    private static void Player_UpdatePearlpup(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);

        if (!self.IsPearlpup()) return;

        var stats = self.slugcatStats;

        stats.throwingSkill = 2;
        stats.runspeedFac = self.grabbedBy.Count > 0 ? 0.9f : 1.1f;
        stats.corridorClimbSpeedFac = 1.1f;
        stats.poleClimbSpeedFac = 1.15f;
        stats.lungsFac = 0.5f;
        
        stats.generalVisibilityBonus = 0.1f;
        stats.visualStealthInSneakMode = 0.3f;
        stats.loudnessFac = 1.35f;

        if (self.abstractCreature.Room.world.game.devToolsActive && Input.GetKey("q"))
            self.AddFood(1);
    }

    private static bool Weapon_HitThisObject(On.Weapon.orig_HitThisObject orig, Weapon self, PhysicalObject obj)
    {
        var result = orig(self, obj);

        bool hitPlayerByPlayer = obj is Player && self is Spear && self.thrownBy != null && self.thrownBy is Player;

        if (hitPlayerByPlayer && obj is Player player && player.IsPearlpup())
            return false;

        return result;
    }
}
