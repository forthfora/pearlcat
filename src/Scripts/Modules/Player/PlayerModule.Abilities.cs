using System;
using System.Collections.Generic;
using System.Linq;
using static Pearlcat.PearlEffect;

namespace Pearlcat;

public partial class PlayerModule
{
    public PearlEffect CurrentPearlEffect { get; set; } = PearlEffectManager.None;

    public bool ShieldActive
    {
        get
        {
            return (ShieldTimer > 0 || ShieldCount > 0) && !ModOptions.DisableShield.Value &&
                   PlayerRef.TryGetTarget(out var player) && !player.dead;
        }
    }

    public int ReviveTimer { get; set; }
    public int ShieldTimer { get; set; }
    public float ShieldAlpha { get; set; }
    public float ShieldScale { get; set; }

    public int AgilityOveruseTimer { get; set; }

    public WeakReference<Creature>? RageTarget { get; set; }

    public int RageAnimTimer { get; set; } = 0;

    public WeakReference<Creature>? PossessionTarget { get; set; }
    public WeakReference<AbstractCreature>? PossessedCreature { get; set; }
    public bool IsPossessingCreature
    {
        get { return PossessedCreature != null && PossessedCreature.TryGetTarget(out _) && IsAdultPearlpup; }
    }


    public List<MajorEffectType> DisabledEffects { get; } = new();

    public int AgilityCount
    {
        get { return ModOptions.DisableAgility.Value ? 0 : MajorEffectCount(MajorEffectType.AGILITY); }
    }

    public int CamoCount
    {
        get { return ModOptions.DisableCamoflague.Value ? 0 : MajorEffectCount(MajorEffectType.CAMOFLAGUE); }
    }

    public int RageCount
    {
        get { return ModOptions.DisableRage.Value ? 0 : MajorEffectCount(MajorEffectType.RAGE); }
    }

    public int ReviveCount
    {
        get { return ModOptions.DisableRevive.Value ? 0 : MajorEffectCount(MajorEffectType.REVIVE); }
    }

    public int SpearCount
    {
        get { return ModOptions.DisableSpear.Value ? 0 : MajorEffectCount(MajorEffectType.SPEAR_CREATION); }
    }

    public int ShieldCount
    {
        get { return ModOptions.DisableShield.Value ? 0 : MajorEffectCount(MajorEffectType.SHIELD); }
    }

    public int MajorEffectCount(MajorEffectType type)
    {
        var count = -1;

        var inventory = Inventory.Concat(PostDeathInventory);

        foreach (var pearl in inventory)
        {
            if (!pearl.TryGetPlayerPearlModule(out var module))
            {
                continue;
            }

            if (pearl.TryGetSentry(out _))
            {
                continue;
            }

            if (pearl.GetPearlEffect().MajorEffect != type)
            {
                continue;
            }

            if (count < 0)
            {
                count = 0;
            }

            if (module.CooldownTimer == 0)
            {
                count++;
            }
        }

        return count;
    }


    public AbstractPhysicalObject? SetAgilityCooldown(int cooldown)
    {
        return PutOnCooldown(MajorEffectType.AGILITY, cooldown);
    }

    public AbstractPhysicalObject? SetCamoCooldown (int cooldown)
    {
        return PutOnCooldown(MajorEffectType.CAMOFLAGUE, cooldown);
    }

    public AbstractPhysicalObject? SetRageCooldown(int cooldown)
    {
        return PutOnCooldown(MajorEffectType.RAGE, cooldown);
    }

    public AbstractPhysicalObject? SetReviveCooldown(int cooldown)
    {
        var result = PutOnCooldown(MajorEffectType.REVIVE, cooldown);

        if (result?.TryGetPlayerPearlModule(out var module) == true)
        {
            module.InventoryFlash = true;
        }

        if (ModOptions.InventoryPings.Value)
        {
            ShowHUD(80);
        }

        return result;
    }
    public AbstractPhysicalObject? SetSpearCooldown(int cooldown)
    {
        return PutOnCooldown(MajorEffectType.SPEAR_CREATION, cooldown);
    }

    public AbstractPhysicalObject? SetShieldCooldown(int cooldown)
    {
        return PutOnCooldown(MajorEffectType.SHIELD, cooldown);
    }

    public AbstractPhysicalObject? PutOnCooldown(MajorEffectType type, int cooldown)
    {
        var inventory = Inventory.Concat(PostDeathInventory);

        foreach (var pearl in inventory)
        {
            if (!pearl.TryGetPlayerPearlModule(out var module))
            {
                continue;
            }

            if (pearl.TryGetSentry(out _))
            {
                continue;
            }

            if (pearl.GetPearlEffect().MajorEffect != type)
            {
                continue;
            }

            if (module.CooldownTimer == 0)
            {
                module.CooldownTimer = cooldown;
                return pearl;
            }
        }

        return null;
    }


    public void ResetAgilityCooldown(int time)
    {
        foreach (var pearl in Inventory)
        {
            if (!pearl.TryGetPlayerPearlModule(out var module))
            {
                continue;
            }

            if (pearl.GetPearlEffect().MajorEffect != MajorEffectType.AGILITY)
            {
                continue;
            }

            if (module.CooldownTimer == -1)
            {
                module.CooldownTimer = time;
            }
        }
    }
    
    public void ActivateVisualShield()
    {
        if (ShieldTimer > 0)
        {
            return;
        }

        var obj = SetShieldCooldown(ModOptions.ShieldRechargeTime.Value);

        ShieldTimer = ModOptions.ShieldDuration.Value;
        ShieldTimer *= (int)(ActiveObject?.GetPearlEffect().MajorEffect == MajorEffectType.SHIELD ? 2.0f : 1.0f);

        if (PlayerRef.TryGetTarget(out var player))
        {
            player.room?.PlaySound(Enums.Sounds.Pearlcat_ShieldStart, player.firstChunk);
        }
        
        if (obj?.TryGetPlayerPearlModule(out var module) == true)
        {
            module.InventoryFlash = true;
        }

        if (ModOptions.InventoryPings.Value)
        {
            ShowHUD(60);
        }
    }
}
