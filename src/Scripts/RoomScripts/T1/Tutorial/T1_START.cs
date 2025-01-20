
namespace Pearlcat;

public class T1_START : UpdatableAndDeletable
{
    public Phase CurrentPhase { get; set; } = Phase.Init;
    public int HardsetPosTimer { get; set; } = 8;

    public int PhaseTimer { get; set; }

    public enum Phase
    {
        Init,
        SwapTutorial,

        StoreTutorial,
        SentryTutorial,

        End,

        Sleep,
    }

    public T1_START(Room room)
    {
        this.room = room;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        if (!room.fullyLoaded)
        {
            return;
        }

        var game = room.game;
        var save = Utils.MiscProgression;

        // Per player
        foreach (var crit in game.Players)
        {
            if (crit.realizedCreature is not Player player)
            {
                continue;
            }

            if (CurrentPhase == Phase.Init)
            {
                player.playerState.foodInStomach = SlugcatStats.SlugcatFoodMeter(player.SlugCatClass).y;
                room.game.cameras[0].hud.foodMeter.NewShowCount(player.FoodInStomach);

                player.controller = new PearlcatController(new(this, player.playerState.playerNumber));

                if (!save.HasTrueEnding)
                {
                    // Remove Default Inventory
                    if (player.TryGetPearlcatModule(out var playerModule) && !ModOptions.StartingInventoryOverride && !ModOptions.InventoryOverride && player.playerState.playerNumber == 0)
                    {
                        // Clear default pearls
                        for (var i = playerModule.Inventory.Count - 1; i >= 0; i--)
                        {
                            var item = playerModule.Inventory[i];

                            if (item is DataPearl.AbstractDataPearl dataPearl)
                            {
                                if (dataPearl.IsHalcyonPearl() || dataPearl.dataPearlType == Enums.Pearls.AS_PearlBlack)
                                {
                                    continue;
                                }
                            }

                            player.RemoveFromInventory(item);

                            item.realizedObject?.Destroy();
                            item.Destroy();
                        }

                        player.UpdateInventorySaveData();
                    }
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
            {
                pearlcatController.Owner.Update();
            }
        }


        if (save.HasTrueEnding)
        {
            if (PhaseTimer == 0)
            {
                if (CurrentPhase == Phase.Init)
                {
                    if (room.BeingViewed)
                    {
                        room.LockAndHideShortcuts();

                        room.game.cameras[0].hud.foodMeter.visibleCounter = 0;
                        room.game.cameras[0].hud.foodMeter.fade = 0f;
                        room.game.cameras[0].hud.foodMeter.lastFade = 0f;

                        CurrentPhase = Phase.Sleep;
                        PhaseTimer = 300;
                    }
                }
                else if (CurrentPhase == Phase.Sleep)
                {
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
        else
        {
            if (PhaseTimer == 0)
            {
                if (CurrentPhase == Phase.Init)
                {
                    if (room.BeingViewed)
                    {
                        room.LockAndHideShortcuts();

                        room.game.cameras[0].hud.foodMeter.visibleCounter = 0;
                        room.game.cameras[0].hud.foodMeter.fade = 0f;
                        room.game.cameras[0].hud.foodMeter.lastFade = 0f;

                        if (ModOptions.DisableTutorials)
                        {
                            CurrentPhase = Phase.End;
                        }
                        else
                        {
                            CurrentPhase = Phase.SwapTutorial;
                            PhaseTimer = 300;
                        }
                    }
                }
                else if (CurrentPhase == Phase.SwapTutorial)
                {
                    var t = Utils.Translator;

                    game.AddTextPrompt(t.Translate("To cycle between pearls, use (") + Input_Helpers.GetSwapLeftKeybindDisplayName(false) + t.Translate(") & (") + Input_Helpers.GetSwapRightKeybindDisplayName(false) + t.Translate("), or the triggers on controller"), 0, 600);

                    game.AddTextPrompt(
                        t.Translate("Alternatively, hold (") + Input_Helpers.GetSwapKeybindDisplayName(false) + t.Translate(") or (") + Input_Helpers.GetSwapKeybindDisplayName(true) + t.Translate(") & use the (LEFT) & (RIGHT) directional inputs"), 50, 500);

                    PhaseTimer = 1100;
                    CurrentPhase = Phase.StoreTutorial;
                }
                else if (CurrentPhase == Phase.StoreTutorial)
                {
                    var t = Utils.Translator;

                    if (ModOptions.UsesCustomStoreKeybind)
                    {
                        game.AddTextPrompt(t.Translate("To retrieve pearls, have an empty main hand, and hold (") + Input_Helpers.GetStoreKeybindDisplayName(false) + t.Translate(") or (") + Input_Helpers.GetStoreKeybindDisplayName(true) + t.Translate(")"), 0, 800);
                    }

                    else
                    {
                        game.AddTextPrompt("To retrieve pearls, have an empty main hand, and hold (GRAB + UP)", 0, 600);
                    }

                    game.AddTextPrompt("To store, hold the same keybind with a pearl in your main hand", 0, 400);

                    PhaseTimer = 800;
                    CurrentPhase = Phase.SentryTutorial;
                }
                else if (CurrentPhase == Phase.SentryTutorial)
                {
                    var t = Utils.Translator;

                    if (ModOptions.CustomSentryKeybind)
                    {
                        game.AddTextPrompt(t.Translate("Pearls may also be deployed as temporary sentries. Press (") + Input_Helpers.GetSentryKeybindDisplayName(false) + t.Translate(") or (")
                            + Input_Helpers.GetSentryKeybindDisplayName(true) + t.Translate(") to deploy, and again to return."), 0, 600);
                    }
                    else
                    {
                        game.AddTextPrompt("Pearls may also be deployed as temporary sentries. Press (GRAB + JUMP + DOWN) to deploy, and again to return.", 0, 800);
                    }

                    game.AddTextPrompt("Play around with sentries to see what they do!", 0, 200);

                    PhaseTimer = 700;
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
    }

    public class PearlcatController(PearlcatPlayer owner) : Player.PlayerController
    {
        public PearlcatPlayer Owner { get; } = owner;

        public override Player.InputPackage GetInput()
        {
            return Owner.GetInput();
        }
    }

    public class PearlcatPlayer(T1_START owner, int playerNumber)
    {
        public T1_START Owner { get; } = owner;
        public int PlayerNumber { get; } = playerNumber;

        public Player? Player => (Owner.room?.game.Players[PlayerNumber].realizedCreature) as Player;

        public void Update()
        {
            if (Player is null)
            {
                return;
            }

            Player.sleepCounter = 90;
        }

        public Player.InputPackage GetInput()
        {
            if (Player is null)
            {
                return new Player.InputPackage(false, Options.ControlSetup.Preset.None, 0, 0, false, false, false, false, false);
            }

            return new(false, Options.ControlSetup.Preset.KeyboardSinglePlayer, 0, 0, false, false, false, false, false);
        }
    }
}
