using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pearlcat;

public partial class PlayerModule
{
    public WeakReference<Player> PlayerRef { get; }
    public WeakReference<Player>? PearlpupRef { get; set; }

    public PlayerModule(Player self)
    {
        PlayerRef = new(self);

        PlayerNumber = self.playerState.playerNumber;
        BaseStats = NormalStats;

        if (ModCompat_Helpers.RainMeadow_IsOnline)
        {
            MeadowCompat.AddMeadowPlayerData(self);
        }
    }


    public bool IsAdultPearlpup =>
        PlayerRef.TryGetTarget(out var player) &&
        player.abstractCreature.Room.world.game.IsPearlcatStory() &&
        Utils.MiscProgression.HasTrueEnding;

    public int PlayerNumber { get; }

    public SlugcatStats BaseStats { get; set; }
    public SlugcatStats NormalStats { get; } = new(Enums.Pearlcat, false);
    public SlugcatStats MalnourishedStats { get; private set; } = new(Enums.Pearlcat, true);


    public bool JustWarped { get; set; }
    public AbstractRoom? LastRoom { get; set; }
    public int MaskCounter { get; set; }
    public bool GivenPearlsThisCycle { get; set; }


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


    public bool IsDazed => DazeTimer > 0;
    public int DazeTimer { get; set; }


    // Inventory
    public List<AbstractPhysicalObject> Inventory { get; set; } = [];
    public int? ActivePearlIndex { get; set; }

    public List<AbstractPhysicalObject> PostDeathInventory { get; set; } = [];
    public int? PostDeathActivePearlIndex { get; set; }

    public AbstractPhysicalObject? ActivePearl => ActivePearlIndex is not null && ActivePearlIndex < Inventory.Count ? Inventory[(int)ActivePearlIndex] : null;


    // Input
    public bool WasSwapLeftInput { get; set; }
    public bool WasSwapRightInput { get; set; }
    public bool WasSwapped { get; set; }
    public bool WasStoreInput { get; set; }
    public bool WasAgilityInput { get; set; }
    public bool WasSentryInput { get; set; }

    public Player.InputPackage UnblockedInput { get; set; }
    public bool BlockInput { get; set; }

    public int StoreObjectTimer { get; set; }

    
    // HUD
    public void ShowHUD(int duration)
    {
        if (ModCompat_Helpers.RainMeadow_IsOnline)
        {
            if (!PlayerRef.TryGetTarget(out var player))
            {
                return;
            }

            // No need to show the HUD for other players in meadow
            if (!ModCompat_Helpers.RainMeadow_IsMine(player.abstractCreature))
            {
                return;
            }
        }

        HudFadeTimer = duration;
    }

    public float HudFade { get; set; }
    public float HudFadeTimer { get; set; }


    public void LoadInventorySaveData(Player self)
    {
        if (!ModCompat_Helpers.RainMeadow_IsMine(self.abstractPhysicalObject))
        {
            return;
        }

        var world = self.abstractCreature.world;
        var save = world.game.GetMiscWorld();

        if (save is null)
        {
            return;
        }

        var playerNumber = self.playerState.playerNumber;

        if (!ModOptions.InventoryOverride)
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

        if (Inventory.Any())
        {
            if (save.ActivePearlIndex.TryGetValue(playerNumber, out var activePearlIndex) && activePearlIndex < Inventory.Count)
            {
                ActivePearlIndex = activePearlIndex;
            }
            else
            {
                // Just in case
                ActivePearlIndex = 0;
            }
        }
        else
        {
            ActivePearlIndex = null;
        }

        PickPearlAnimation(self);
    }
}
