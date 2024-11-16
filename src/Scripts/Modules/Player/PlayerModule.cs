using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pearlcat;

public partial class PlayerModule
{
    public WeakReference<Player> PlayerRef { get; private set; }
    public WeakReference<Player>? PearlpupRef { get; set; }

    public PlayerModule(Player self)
    {
        PlayerRef = new(self);

        PlayerNumber = self.playerState.playerNumber;
        UniqueID = IDCounter++;
        BaseStats = NormalStats;
    }
    
    
    public bool IsAdultPearlpup
    {
        get
        {
            return PlayerRef.TryGetTarget(out var player) &&
                   player.abstractCreature.Room.world.game.IsPearlcatStory() &&
                   Utils.GetMiscProgression().HasTrueEnding;
        }
    }

    public int PlayerNumber { get; }
    public int UniqueID { get; }
    public static int IDCounter { get; set; }

    public SlugcatStats BaseStats { get; set; }
    public SlugcatStats NormalStats { get; private set; } = new(Enums.Pearlcat, false);
    public SlugcatStats MalnourishedStats { get; private set; } = new(Enums.Pearlcat, true);


    public bool JustWarped { get; set; }
    public AbstractRoom? LastRoom { get; set; }
    public int MaskCounter { get; set; }
    public bool GivenPearls { get; set; }


    public int SpearTimer { get; set; }
    public int SpearDelay { get; set; }
    public bool ForceLockSpearOnBack { get; set; }
    public float SpearLerp { get; set; }
    public bool WasSpearOnBack { get; set; }


    public float HoloLightAlpha { get; set; } = 1.0f;
    public float HoloLightScale { get; set; }


    public Vector2 PrevHeadRotation { get; set; }
    public Vector2 LastGroundedPos { get; set; }
    public int GroundedTimer { get; set; }
    public int FlyTimer { get; set; }


    public bool IsDazed
    {
        get { return DazeTimer > 0; }
    }

    public int DazeTimer { get; set; }


    // Inventory
    public List<AbstractPhysicalObject> Inventory { get; } = [];
    public List<AbstractPhysicalObject> PostDeathInventory { get; } = [];
    public int? PostDeathActiveObjectIndex { get; set; }
    public AbstractPhysicalObject? ActiveObject
    {
        get
        {
            return ActiveObjectIndex != null && ActiveObjectIndex < Inventory.Count
                ? Inventory[(int)ActiveObjectIndex]
                : null;
        }
    }

    public int? ActiveObjectIndex { get; set; }


    // Input
    public bool WasSwapLeftInput { get; set; }
    public bool WasSwapRightInput { get; set; }
    public bool WasSwapped { get; set; }
    public bool WasStoreInput { get; set; }
    public bool WasAgilityInput { get; set; }
    public bool WasSentryInput { get; set; }

    public Player.InputPackage UnblockedInput { get; set; }
    public bool BlockInput { get; set; }

    public int SwapIntervalTimer { get; set; }
    public int StoreObjectTimer { get; set; }

    
    // HUD
    public void ShowHUD(int duration)
    {
        HudFadeTimer = duration;
    }

    public float HudFade { get; set; }
    public float HudFadeTimer { get; set; }


    public void LoadSaveData(Player self)
    {
        var world = self.abstractCreature.world;
        var save = world.game.GetMiscWorld();

        if (save == null)
        {
            return;
        }

        var playerNumber = self.playerState.playerNumber;

        if (!ModOptions.InventoryOverride.Value)
        {
            Inventory.Clear();
            
            if (save.Inventory.TryGetValue(playerNumber, out var inventory))
            {
                foreach (var item in inventory)
                {
                    self.AddToInventory(SaveState.AbstractPhysicalObjectFromString(world, item), addToEnd: true);
                }
            }
        }

        ActiveObjectIndex = null;

        if (save.ActiveObjectIndex.TryGetValue(playerNumber, out var activeObjectIndex) && Inventory.Count > 0)
        {
            ActiveObjectIndex = activeObjectIndex < Inventory.Count ? activeObjectIndex : 0;
        }

        PickObjectAnimation(self);
    }
}
