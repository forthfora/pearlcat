using System;
using System.Linq;
using JetBrains.Annotations;
using RainMeadow;
using UnityEngine;

namespace Pearlcat;

public class MeadowPlayerPearlState : OnlineEntity.EntityData.EntityDataState
{
    // Pearl module
    [OnlineField]
    public int laserTimer;

    [OnlineField]
    public int _cooldownTimer;

    [OnlineField]
    public int currentCooldownTime;

    [OnlineField]
    public bool isCWDoubleJumpUsed;


    // Graphics
    [OnlineField(nullable = true)]
    public Vector2? overridePos;

    [OnlineField(nullable = true)]
    public Vector2? overrideLastPos;


    // Sentry
    [OnlineField]
    public bool isSentry;

    [OnlineField]
    public bool isReturningSentry;

    [OnlineField]
    public float sentryShieldTimer;

    [OnlineField]
    public int sentryRageCounter;


    [UsedImplicitly]
    public MeadowPlayerPearlState()
    {
    }

    public MeadowPlayerPearlState(MeadowPlayerPearlData data, OnlineEntity onlineEntity)
    {
        var pearl = (DataPearl.AbstractDataPearl)((OnlinePhysicalObject)onlineEntity).apo;

        if (pearl.TryGetPlayerPearlModule(out var pearlModule))
        {
            currentCooldownTime = pearlModule.CurrentCooldownTime;
            _cooldownTimer = pearlModule._cooldownTimer;
            laserTimer = pearlModule.LaserTimer;
            isCWDoubleJumpUsed = pearlModule.IsCWDoubleJumpUsed;

            isSentry = pearlModule.IsSentry;
            isReturningSentry = pearlModule.IsReturningSentry;
        }

        if (pearl.TryGetPearlGraphicsModule(out var graphics))
        {
            overridePos = graphics.OverridePos;
            overrideLastPos = graphics.OverrideLastPos;
        }

        if (pearl.TryGetSentry(out var sentry))
        {
            sentryShieldTimer = sentry.ShieldTimer;
            sentryRageCounter = sentry.RageCounter;
        }
    }

    public override void ReadTo(OnlineEntity.EntityData data, OnlineEntity onlineEntity)
    {
        var pearl = (DataPearl.AbstractDataPearl)((OnlinePhysicalObject)onlineEntity).apo;

        if (pearl.TryGetPlayerPearlModule(out var pearlModule))
        {
            pearlModule.CurrentCooldownTime = currentCooldownTime;
            pearlModule._cooldownTimer = _cooldownTimer;
            pearlModule.LaserTimer = laserTimer;
            pearlModule.IsCWDoubleJumpUsed = isCWDoubleJumpUsed;

            pearlModule.IsReturningSentry = isReturningSentry;

            // Sentry is deployed when it shouldn't be (idk when this would happen tbh)
            if (pearlModule.IsSentry && !isSentry)
            {
                pearlModule.RemoveSentry_Local(pearl);
            }
        }

        if (pearl.TryGetPearlGraphicsModule(out var graphics))
        {
            graphics.OverridePos = overridePos;
            graphics.OverrideLastPos = overrideLastPos;
        }

        if (pearl.TryGetSentry(out var sentry))
        {
            sentry.ShieldTimer = sentryShieldTimer;
            sentry.RageCounter = sentryRageCounter;
        }
        else
        {
            // Sync sentry deployment for newly joined players
            if (isSentry && pearl.TryGetPlayerPearlOwner(out var player))
            {
                PlayerAbilities_Helpers.DeploySentry_Local(player, pearl);
            }
        }
    }

    public override Type GetDataType()
    {
        return typeof(MeadowPlayerPearlData);
    }
}
