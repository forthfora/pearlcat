
namespace Pearlcat;

public class T1_START : UpdatableAndDeletable
{
    public Phase CurrentPhase { get; set; } = Phase.Init;
    public int HardsetPosTimer { get; set; } = 2;

    public int PhaseTimer { get; set; }

    public enum Phase
    {
        Init,
        SwapTutorial,

        StoreTutorial,

        End,
    }

    public T1_START(Room room)
    {
        this.room = room;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        var game = room.game;

        // Per player
        foreach (var crit in game.Players)
        {
            if (crit.realizedCreature is not Player player) continue;

            if (!player.TryGetPearlcatModule(out var playerModule)) return;

            if (CurrentPhase == Phase.Init)
            {
                player.playerState.foodInStomach = 3;
                room.game.cameras[0].hud.foodMeter.NewShowCount(player.FoodInStomach);

                player.controller = new PearlcatController(new(this, player.playerState.playerNumber));

                if (!ModOptions.StartingInventoryOverride.Value && !ModOptions.InventoryOverride.Value && player.playerState.playerNumber == 0)
                {
                    // Clear default pearls
                    for (int i = playerModule.Inventory.Count - 1; i >= 0; i--)
                    {
                        var item = playerModule.Inventory[i];

                        if (item is DataPearl.AbstractDataPearl dataPearl)
                            if (dataPearl.dataPearlType == Enums.Pearls.RM_Pearlcat || dataPearl.dataPearlType == Enums.Pearls.AS_PearlBlack)
                                continue;

                        player.RemoveFromInventory(item);
                        item.destroyOnAbstraction = true;
                        item.Abstractize(item.pos);
                    }

                    player.UpdateInventorySaveData(playerModule);
                }
            }
            else if (CurrentPhase == Phase.StoreTutorial || CurrentPhase == Phase.End)
            {
                player.controller = null;
            }

            // I think Slugbase is setting the position after us ?
            if (room == player.room && HardsetPosTimer > 0)
            {
                player.SuperHardSetPosition(new(680.0f, 340.0f));
                player.graphicsModule?.Reset();

                HardsetPosTimer--;
            }

            if (player.controller is PearlcatController pearlcatController)
                pearlcatController.Owner.Update();
        }


        if (PhaseTimer == 0)
        {
            if (CurrentPhase == Phase.Init)
            {
                if (room.fullyLoaded && room.BeingViewed)
                {
                    room.LockAndHideShortcuts();

                    room.game.cameras[0].hud.foodMeter.visibleCounter = 0;
                    room.game.cameras[0].hud.foodMeter.fade = 0f;
                    room.game.cameras[0].hud.foodMeter.lastFade = 0f;
                    
                    if (ModOptions.DisableTutorials.Value)
                    {
                        CurrentPhase = Phase.End;
                    }
                    else
                    {
                        CurrentPhase = Phase.SwapTutorial;
                        PhaseTimer = 150;
                    }
                }
            }
            else if (CurrentPhase == Phase.SwapTutorial)
            {
                game.AddTextPrompt($"To cycle between pearls, use ({ModOptions.SwapLeftKeybind.Value}) & ({ModOptions.SwapRightKeybind.Value}), or the triggers on controller", 0, 400);

                game.AddTextPrompt($"Alternatively, hold ({ModOptions.SwapKeybindKeyboard.Value}) & use the (LEFT) & (RIGHT) directional inputs", 50, 300);

                PhaseTimer = 800;
                CurrentPhase = Phase.StoreTutorial;
            }
            else if (CurrentPhase == Phase.StoreTutorial)
            {
                if (ModOptions.UsesCustomStoreKeybind.Value)
                    game.AddTextPrompt($"To retrieve pearls, have an empty right hand, and hold ({ModOptions.StoreKeybindKeyboard.Value})", 0, 600);
 
                else
                    game.AddTextPrompt($"To retrieve pearls, have an empty right hand, and hold (GRAB + UP)", 0, 600);

                game.AddTextPrompt($"To store, hold the same keybind with a pearl in your right hand", 0, 400);

                PhaseTimer = 800;
                CurrentPhase = Phase.End;
            }
            else if (CurrentPhase == Phase.End)
            {
                room.UnlockAndShowShortcuts();
                PhaseTimer = -1;
            }
        }
        else if (PhaseTimer > 0)
        {
            PhaseTimer--;
        }
    }

    public class PearlcatController : Player.PlayerController
    {
        public PearlcatPlayer Owner { get; }

        public PearlcatController(PearlcatPlayer owner) => Owner = owner;
        public override Player.InputPackage GetInput() => Owner.GetInput();
    }

    public class PearlcatPlayer
    {
        public T1_START Owner { get; }
        public int PlayerNumber { get; }

        public bool MainPlayer => Player != null && Player.playerState.playerNumber == 0;
        public Player? Player => (Owner.room?.game.Players[PlayerNumber].realizedCreature) as Player;

        public PearlcatPlayer(T1_START owner, int playerNumber)
        {
            Owner = owner;
            PlayerNumber = playerNumber;
        }

        public void Update()
        {
            if (Player == null) return;

            Player.sleepCounter = 90;
        }

        public Player.InputPackage GetInput()
        {
            if (Player == null)
                return new Player.InputPackage(false, Options.ControlSetup.Preset.None, 0, 0, false, false, false, false, false);

            int x = 0;
            int y = 0;
            bool jmp = false;

            return new(false, Options.ControlSetup.Preset.KeyboardSinglePlayer, x, y, jmp, false, false, false, false);
        }
    }
}
