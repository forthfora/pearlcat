using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pearlcat;

public partial class PlayerModule
{
    public WeakReference<AbstractCreature> AbstractPlayerRef { get; }
    public Player? PlayerRef => AbstractPlayerRef.TryGetTarget(out var player) ? player.realizedCreature as Player : null;

    public WeakReference<AbstractCreature>? AbstractPearlpupRef { get; set; }
    public Player? PearlpupRef => AbstractPearlpupRef?.TryGetTarget(out var player) == true ? player.realizedCreature as Player : null;

    public bool IsAdultPearlpup =>
        PlayerRef is not null &&
        PlayerRef.abstractCreature.Room.world.game.IsPearlcatStory() &&
        Utils.MiscProgression.HasTrueEnding;

    public SlugcatStats BaseStats { get; set; }
    public SlugcatStats NormalStats { get; } = new(Enums.Pearlcat, false);
    public SlugcatStats MalnourishedStats { get; private set; } = new(Enums.Pearlcat, true);


    public bool GivenPearlsThisCycle { get; set; }
    public AbstractRoom? LastRoom { get; set; }


    public bool ForceLockSpearOnBack { get; set; }
    public bool WasSpearOnBack { get; set; }


    public int SpearTimer { get; set; }
    public int SpearDelay { get; set; }
    public float SpearLerp { get; set; }


    public float HoloLightAlpha { get; set; } = 1.0f;
    public float HoloLightScale { get; set; }


    public Vector2 PrevHeadRotation { get; set; }
    public Vector2 LastGroundedPos { get; set; }
    public int GroundedTimer { get; set; }
    public int FlyTimer { get; set; }


    public bool IsDazed => DazeTimer > 0;
    public int DazeTimer { get; set; }


    public int MaskCounter { get; set; }


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
    public float HudFade { get; set; }
    public float HudFadeTimer { get; set; }


    public PlayerModule(Player self)
    {
        AbstractPlayerRef = new(self.abstractCreature);

        BaseStats = NormalStats;

        if (ModCompat_Helpers.RainMeadow_IsOnline)
        {
            MeadowCompat.AddMeadowPlayerData(self);
        }
    }

    public void ShowHUD(int duration)
    {
        if (ModCompat_Helpers.RainMeadow_IsOnline)
        {
            if (PlayerRef is null)
            {
                return;
            }

            // No need to show the HUD for other players in meadow
            if (!ModCompat_Helpers.RainMeadow_IsMine(PlayerRef.abstractCreature))
            {
                return;
            }
        }

        HudFadeTimer = duration;
    }

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

        var id = self.playerState.playerNumber;

        if (ModCompat_Helpers.RainMeadow_IsOnline)
        {
            id = ModCompat_Helpers.GetOwnerId(self.abstractPhysicalObject);
        }


        if (!ModOptions.InventoryOverride)
        {
            Inventory.Clear();

            if (save.Inventory.TryGetValue(id, out var inventory))
            {
                foreach (var item in inventory)
                {
                    self.AddToInventory(SaveState.AbstractPhysicalObjectFromString(world, item), addToEnd: true);
                }
            }
        }

        if (Inventory.Any())
        {
            if (save.ActiveObjectIndex.TryGetValue(id, out var activePearlIndex) && activePearlIndex < Inventory.Count)
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
