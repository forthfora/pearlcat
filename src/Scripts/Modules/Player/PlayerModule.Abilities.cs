using System;
using System.Collections.Generic;
using System.Linq;
using static Pearlcat.PearlEffect;

namespace Pearlcat;

public partial class PlayerModule
{
    public PearlEffect CurrentPearlEffect { get; set; } = PearlEffectManager.None;
    public List<MajorEffectType> DisabledEffects { get; } = [];

    // Possession
    public WeakReference<Creature>? PossessionTarget { get; set; }
    public WeakReference<AbstractCreature>? PossessedCreature { get; set; }
    public bool IsPossessingCreature => PossessedCreature is not null && PossessedCreature.TryGetTarget(out _) && IsAdultPearlpup;

    // Shield
    public bool ShieldActive => (ShieldTimer > 0 || ShieldCount > 0) && !ModOptions.DisableShield && PlayerRef.TryGetTarget(out var player) && !player.dead;
    public int ShieldTimer { get; set; }
    public float ShieldAlpha { get; set; }
    public float ShieldScale { get; set; }

    // Revive
    public int ReviveTimer { get; set; }

    // Agility
    public int AgilityOveruseTimer { get; set; }

    // Rage
    public WeakReference<Creature>? RageTarget { get; set; }
    public int RageAnimTimer { get; set; }


    // Count
    public int RageCount => ModOptions.DisableRage ? 0 : MajorEffectCount(MajorEffectType.Rage);
    public int AgilityCount => ModOptions.DisableAgility ? 0 : MajorEffectCount(MajorEffectType.Agility);
    public int CamoCount => ModOptions.DisableCamoflague ? 0 : MajorEffectCount(MajorEffectType.Camouflage);
    public int ReviveCount => ModOptions.DisableRevive ? 0 : MajorEffectCount(MajorEffectType.Revive);
    public int SpearCount => ModOptions.DisableSpear ? 0 : MajorEffectCount(MajorEffectType.SpearCreation);
    public int ShieldCount => ModOptions.DisableShield ? 0 : MajorEffectCount(MajorEffectType.Shield);

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


    // Cooldown
    public AbstractPhysicalObject? SetCamoCooldown (int cooldown)
    {
        return PutOnCooldown(MajorEffectType.Camouflage, cooldown);
    }

    public AbstractPhysicalObject? SetRageCooldown(int cooldown)
    {
        return PutOnCooldown(MajorEffectType.Rage, cooldown);
    }

    public AbstractPhysicalObject? SetSpearCooldown(int cooldown)
    {
        return PutOnCooldown(MajorEffectType.SpearCreation, cooldown);
    }

    public AbstractPhysicalObject? SetAgilityCooldown(int cooldown)
    {
        return PutOnCooldown(MajorEffectType.Agility, cooldown);
    }
    public AbstractPhysicalObject? SetReviveCooldown(int cooldown)
    {
        var result = PutOnCooldown(MajorEffectType.Revive, cooldown);

        if (result?.TryGetPlayerPearlModule(out var module) == true)
        {
            module.InventoryFlash = true;
        }

        if (ModOptions.InventoryPings)
        {
            ShowHUD(80);
        }

        return result;
    }
    public AbstractPhysicalObject? SetShieldCooldown(int cooldown)
    {
        return PutOnCooldown(MajorEffectType.Shield, cooldown);
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

            if (pearl.GetPearlEffect().MajorEffect != MajorEffectType.Agility)
            {
                continue;
            }

            if (module.CooldownTimer == -1)
            {
                module.CooldownTimer = time;
            }

            module.IsCWDoubleJumpUsed = false;
        }
    }


    // Activates a shield pearl if one is not already active
    public void ActivateVisualShield()
    {
        if (ShieldTimer > 0)
        {
            return;
        }

        ActivateVisualShield_Local();

        if (ModCompat_Helpers.RainMeadow_IsOnline && PlayerRef.TryGetTarget(out var player))
        {
            MeadowCompat.RPC_ActivateVisualShield(player);
        }
    }

    public void ActivateVisualShield_Local()
    {
        var obj = SetShieldCooldown(ModOptions.ShieldRechargeTime);

        ShieldTimer = ModOptions.ShieldDuration;
        ShieldTimer *= (int)(ActivePearl?.GetPearlEffect().MajorEffect == MajorEffectType.Shield ? 2.0f : 1.0f);

        if (PlayerRef.TryGetTarget(out var player))
        {
            player.room?.PlaySound(Enums.Sounds.Pearlcat_ShieldStart, player.firstChunk);
        }

        if (obj?.TryGetPlayerPearlModule(out var module) == true)
        {
            module.InventoryFlash = true;
        }

        if (ModOptions.InventoryPings)
        {
            ShowHUD(60);
        }
    }
}
