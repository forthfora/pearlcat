using System.Text.RegularExpressions;
using UnityEngine;

namespace Pearlcat;

public static class Input_Helpers
{
    // Unity doesn't allow us to change the Input Map in a built project... luckily the PS axis overlaps with the Xbox triggers, discovery courtesy of Andrew (thanks Andrew)
    // Only works for Xbox controllers though, big sad
    public static string TriggerAxisId => "DschockHorizontalRight";


    // Inventory
    public static bool IsStoreKeybindPressed(this Player player, PlayerModule playerModule)
    {
        if (!ModCompat_Helpers.RainMeadow_IsLocal(player.abstractPhysicalObject))
        {
            return playerModule.MeadowInput.Store;
        }

        if (player.bodyMode != Player.BodyModeIndex.Stand
            && player.bodyMode != Player.BodyModeIndex.ZeroG
            && player.bodyMode != Player.BodyModeIndex.Swimming
            && player.animation != Player.AnimationIndex.StandOnBeam
            && player.animation != Player.AnimationIndex.BeamTip)
        {
            return false;
        }

        var input = playerModule.UnblockedInput;

        if (!ModOptions.UsesCustomStoreKeybind.Value)
        {
            return input.y == 1.0f && input.pckp && !input.jmp;
        }

        if (ModCompat_Helpers.IsIICActive)
        {
            return player.IsStorePressedIIC();
        }

        return player.playerState.playerNumber switch
        {
            0 => Input.GetKey(ModOptions.StoreKeybindPlayer1.Value) || Input.GetKey(ModOptions.StoreKeybindKeyboard.Value),
            1 => Input.GetKey(ModOptions.StoreKeybindPlayer2.Value),
            2 => Input.GetKey(ModOptions.StoreKeybindPlayer3.Value),
            3 => Input.GetKey(ModOptions.StoreKeybindPlayer4.Value),

            _ => false,
        };
    }

    public static bool IsSwapKeybindPressed(this Player player)
    {
        if (!player.TryGetPearlcatModule(out var playerModule))
        {
            return false;
        }

        if (!ModCompat_Helpers.RainMeadow_IsLocal(player.abstractPhysicalObject))
        {
            return playerModule.MeadowInput.Swap;
        }

        if (ModCompat_Helpers.IsIICActive)
        {
            return player.IsSwapPressedIIC();
        }

        return player.playerState.playerNumber switch
        {
            0 => Input.GetKey(ModOptions.SwapKeybindPlayer1.Value) || Input.GetKey(ModOptions.SwapKeybindKeyboard.Value),
            1 => Input.GetKey(ModOptions.SwapKeybindPlayer2.Value),
            2 => Input.GetKey(ModOptions.SwapKeybindPlayer3.Value),
            3 => Input.GetKey(ModOptions.SwapKeybindPlayer4.Value),

            _ => false,
        };
    }

    public static bool IsSwapLeftInput(this Player player)
    {
        if (!player.TryGetPearlcatModule(out var playerModule))
        {
            return false;
        }

        if (!ModCompat_Helpers.RainMeadow_IsLocal(player.abstractPhysicalObject))
        {
            return playerModule.MeadowInput.SwapLeft;
        }

        if (ModOptions.SwapTriggerPlayer.Value != 0)
        {
            // Normal
            if (ModOptions.SwapTriggerPlayer.Value > 0)
            {
                if (player.playerState.playerNumber == ModOptions.SwapTriggerPlayer.Value - 1 || player.IsSingleplayer())
                {
                    if (Input.GetAxis(TriggerAxisId) < -0.25f)
                    {
                        return true;
                    }
                }
            }
            // Inverted
            else
            {
                if (player.playerState.playerNumber == -ModOptions.SwapTriggerPlayer.Value + 1 || player.IsSingleplayer())
                {
                    if (Input.GetAxis(TriggerAxisId) > 0.25f)
                    {
                        return true;
                    }
                }
            }
        }

        if (ModCompat_Helpers.IsIICActive)
        {
            return player.IsSwapLeftPressedIIC();
        }

        return player.input[0].controllerType == Options.ControlSetup.Preset.KeyboardSinglePlayer && Input.GetKey(ModOptions.SwapLeftKeybind.Value);
    }

    public static bool IsSwapRightInput(this Player player)
    {
        if (!player.TryGetPearlcatModule(out var playerModule))
        {
            return false;
        }

        if (!ModCompat_Helpers.RainMeadow_IsLocal(player.abstractPhysicalObject))
        {
            return playerModule.MeadowInput.SwapRight;
        }

        if (ModOptions.SwapTriggerPlayer.Value != 0)
        {
            // Normal
            if (ModOptions.SwapTriggerPlayer.Value > 0)
            {
                if (player.playerState.playerNumber == ModOptions.SwapTriggerPlayer.Value - 1 || player.IsSingleplayer())
                {
                    if (Input.GetAxis(TriggerAxisId) > 0.25f)
                    {
                        return true;
                    }
                }
            }
            // Inverted
            else
            {
                if (player.playerState.playerNumber == -ModOptions.SwapTriggerPlayer.Value + 1 || player.IsSingleplayer())
                {
                    if (Input.GetAxis(TriggerAxisId) < -0.25f)
                    {
                        return true;
                    }
                }
            }
        }

        if (ModCompat_Helpers.IsIICActive)
        {
            return player.IsSwapRightPressedIIC();
        }

        return player.input[0].controllerType == Options.ControlSetup.Preset.KeyboardSinglePlayer && Input.GetKey(ModOptions.SwapRightKeybind.Value);
    }


    // Ability
    public static bool IsCustomAbilityKeybindPressed(this Player player)
    {
        if (!player.TryGetPearlcatModule(out var playerModule))
        {
            return false;
        }

        if (!ModCompat_Helpers.RainMeadow_IsLocal(player.abstractPhysicalObject))
        {
            return playerModule.MeadowInput.Ability;
        }


        if (ModCompat_Helpers.IsIICActive)
        {
            return player.IsAbilityPressedIIC();
        }

        return player.playerState.playerNumber switch
        {
            0 => Input.GetKey(ModOptions.AbilityKeybindPlayer1.Value) || Input.GetKey(ModOptions.AbilityKeybindKeyboard.Value),
            1 => Input.GetKey(ModOptions.AbilityKeybindPlayer2.Value),
            2 => Input.GetKey(ModOptions.AbilityKeybindPlayer3.Value),
            3 => Input.GetKey(ModOptions.AbilityKeybindPlayer4.Value),

            _ => false,
        };
    }
    
    public static bool IsSentryKeybindPressed(this Player player, PlayerModule playerModule)
    {
        if (!ModCompat_Helpers.RainMeadow_IsLocal(player.abstractPhysicalObject))
        {
            return playerModule.MeadowInput.Semtry;
        }

        if (ModOptions.CustomSentryKeybind.Value)
        {
            if (ModCompat_Helpers.IsIICActive)
            {
                return player.IsSentryPressedIIC();
            }

            return player.playerState.playerNumber switch
            {
                0 => Input.GetKey(ModOptions.SentryKeybindPlayer1.Value) || Input.GetKey(ModOptions.SentryKeybindKeyboard.Value),
                1 => Input.GetKey(ModOptions.SentryKeybindPlayer2.Value),
                2 => Input.GetKey(ModOptions.SentryKeybindPlayer3.Value),
                3 => Input.GetKey(ModOptions.SentryKeybindPlayer4.Value),

                _ => false,
            };
        }

        var input = playerModule.UnblockedInput;

        return input.jmp && input.pckp && input.y == -1;
    }


    // Custom Ability
    public static bool IsAgilityKeybindPressed(this Player player, PlayerModule playerModule)
    {
        if (!ModCompat_Helpers.RainMeadow_IsLocal(player.abstractPhysicalObject))
        {
            return playerModule.MeadowInput.Agility;
        }

        if (ModOptions.CustomAgilityKeybind.Value)
        {
            return IsCustomAbilityKeybindPressed(player);
        }

        var input = playerModule.UnblockedInput;
        return input.jmp && input.pckp;
    }

    public static bool IsSpearCreationKeybindPressed(this Player player, PlayerModule playerModule)
    {
        if (!ModCompat_Helpers.RainMeadow_IsLocal(player.abstractPhysicalObject))
        {
            return playerModule.MeadowInput.SpearCreation;
        }

        if (ModOptions.CustomSpearKeybind.Value)
        {
            return IsCustomAbilityKeybindPressed(player);
        }

        var input = playerModule.UnblockedInput;
        return input.pckp;
    }

    public static bool IsReviveKeybindPressed(this Player player, PlayerModule playerModule)
    {
        if (!ModCompat_Helpers.RainMeadow_IsLocal(player.abstractPhysicalObject))
        {
            return playerModule.MeadowInput.Revive;
        }

        var input = playerModule.UnblockedInput;
        return input.pckp;
    }


    // Display
    public static string GetDisplayName(this KeyCode keyCode)
    {
        var keyCodeChar = Regex.Replace(keyCode.ToString(), "Joystick[0-9]Button", "");

        if (!int.TryParse(keyCodeChar, out var buttonNum))
        {
            keyCodeChar = keyCode.ToString().Replace("JoystickButton", "");

            if (!int.TryParse(keyCodeChar, out buttonNum))
            {
                return keyCode.ToString();
            }
        }

        var t = Utils.Translator;

        return buttonNum switch
        {
            0 => t.Translate("Button South"),
            1 => t.Translate("Button East"),
            2 => t.Translate("Button West"),
            3 => t.Translate("Button North"),
            4 => t.Translate("Left Bumper"),
            5 => t.Translate("Right Bumper"),
            6 => t.Translate("Menu"),
            7 => t.Translate("View"),
            8 => t.Translate("L-Stick"),
            9 => t.Translate("R-Stick"),

            _ => keyCode.ToString(),
        };
    }


    public static string GetStoreKeybindDisplayName(bool controller)
    {
        if (ModCompat_Helpers.IsIICActive)
        {
            return Input_Helpers_IIC.GetStoreKeybindIIC(controller).GetDisplayName();
        }

        return (controller ? ModOptions.StoreKeybindPlayer1.Value : ModOptions.StoreKeybindKeyboard.Value).GetDisplayName();
    }

    public static string GetSwapKeybindDisplayName(bool controller)
    {
        if (ModCompat_Helpers.IsIICActive)
        {
            return Input_Helpers_IIC.GetSwapKeybindIIC(controller).GetDisplayName();
        }

        return (controller ? ModOptions.SwapKeybindPlayer1.Value : ModOptions.SwapKeybindKeyboard.Value).GetDisplayName();
    }

    public static string GetSentryKeybindDisplayName(bool controller)
    {
        if (ModCompat_Helpers.IsIICActive)
        {
            return Input_Helpers_IIC.GetSentryKeybindIIC(controller).GetDisplayName();
        }

        return (controller ? ModOptions.SentryKeybindPlayer1.Value : ModOptions.SentryKeybindKeyboard.Value).GetDisplayName();
    }

    public static string GetAbilityKeybindDisplayName(bool controller)
    {
        if (ModCompat_Helpers.IsIICActive)
        {
            return Input_Helpers_IIC.GetAbilityKeybindIIC(controller).GetDisplayName();
        }

        return (controller ? ModOptions.AbilityKeybindPlayer1.Value : ModOptions.AbilityKeybindKeyboard.Value).GetDisplayName();
    }


    public static string GetSwapLeftKeybindDisplayName(bool controller)
    {
        if (ModCompat_Helpers.IsIICActive)
        {
            return Input_Helpers_IIC.GetSwapLeftKeybindIIC(controller).GetDisplayName();
        }

        return ModOptions.SwapLeftKeybind.Value.GetDisplayName();
    }

    public static string GetSwapRightKeybindDisplayName(bool controller)
    {
        if (ModCompat_Helpers.IsIICActive)
        {
            return Input_Helpers_IIC.GetSwapRightKeybindIIC(controller).GetDisplayName();
        }

        return ModOptions.SwapRightKeybind.Value.GetDisplayName();
    }
}
