using System;
using System.Collections.Generic;
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

        if (ModCompat_Helpers.IsModEnabled_RainMeadow)
        {
            ModCompat_RainMeadow_Helpers.InitMeadowPearlcatData(self);
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


    public bool IsDazed => DazeTimer > 0;
    public int DazeTimer { get; set; }


    // Inventory
    public List<AbstractPhysicalObject> Inventory { get; set; } = [];
    public List<AbstractPhysicalObject> PostDeathInventory { get; } = [];
    public int? PostDeathActiveObjectIndex { get; set; }
    public AbstractPhysicalObject? ActiveObject =>
        ActiveObjectIndex is not null && ActiveObjectIndex < Inventory.Count
            ? Inventory[(int)ActiveObjectIndex]
            : null;

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
        if (!ModCompat_Helpers.RainMeadow_IsOwner)
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


    // Rain Meadow
    public MeadowRemoteInput RemoteInput { get; set; } = new();

    public void UpdateRemoteInput(Player self)
    {
        // we're only in charge of these if we're the owner
        if (!ModCompat_Helpers.RainMeadow_IsLocal(self.abstractCreature))
        {
            return;
        }

        RemoteInput.Store = self.IsStoreKeybindPressed(this);

        RemoteInput.Swap = self.IsSwapKeybindPressed();
        RemoteInput.SwapLeft = self.IsSwapLeftInput();
        RemoteInput.SwapRight = self.IsSwapRightInput();

        RemoteInput.Ability = self.IsCustomAbilityKeybindPressed();
        RemoteInput.Sentry = self.IsSentryKeybindPressed(this);

        RemoteInput.Agility = self.IsAgilityKeybindPressed(this);
        RemoteInput.SpearCreation = self.IsSpearCreationKeybindPressed(this);
    }

    public class MeadowRemoteInput
    {
        public bool Store { get; set; }

        public bool Swap { get; set; }
        public bool SwapLeft { get; set; }
        public bool SwapRight { get; set; }

        public bool Ability { get; set; }
        public bool Sentry { get; set; }

        public bool Agility { get; set; }
        public bool SpearCreation { get; set; }

        public byte ToByte()
        {
            bool[] values =
            [
                Store,
                Swap,
                SwapLeft,
                SwapRight,
                Ability,
                Sentry,
                Agility,
                SpearCreation,
            ];

            var result = values.BoolsToByte();

            return result;
        }

        public void FromByte(byte source)
        {
            var bools = source.ByteToBools();

            Store = bools[0];
            Swap = bools[1];
            SwapLeft = bools[2];
            SwapRight = bools[3];
            Ability = bools[4];
            Sentry = bools[5];
            Agility = bools[6];
            SpearCreation = bools[7];
        }
    }
}
