﻿using System;
using RWCustom;
using UnityEngine;

namespace Pearlcat;

public class PearlpupPearlModule
{
    public WeakReference<DataPearl.AbstractDataPearl> DataPearlRef { get; }
    public WeakReference<Player>? OwnerRef { get; } 

    public PearlpupPearlModule(DataPearl.AbstractDataPearl self)
    {
        DataPearlRef = new(self);

        var owner = self.TryGetPlayerPearlOwner();

        if (owner != null)
        {
            OwnerRef = new(owner);
        }

        CurrentMainColor = AliveMainColor;
    }

    public bool IsPlayerAlive => OwnerRef?.TryGetTarget(out var owner) == true && !owner.dead;

    public float HeartRateMult { get; set; } = 1.0f;

    public int HeartFirstBeatTime { get; set; } = 80;
    public int HeartSecondBeatTime { get; set; } = 10;

    public float HeartFirstBeatTimer { get; set; }
    public float HeartSecondBeatTimer { get; set; } = -1.0f;


    public Color CurrentMainColor { get; set; }
    public Color AliveMainColor => Custom.hexToColor("8f1800");
    public Color DeadMainColor => Custom.hexToColor("785757");


    // Umbilical
    public int UmbilicalSprite { get; set; }
    public UmbilicalGraphics? Umbilical { get; set; }


    // Possession
    public int PossessLaserSprite { get; set; }
    public int PossessProgressSprite { get; internal set; }
    public int PossessCircleSprite { get; internal set; }
}
