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
        if (player.bodyMode != Player.BodyModeIndex.Stand
            && player.bodyMode != Player.BodyModeIndex.ZeroG
            && player.bodyMode != Player.BodyModeIndex.Swimming
            && player.animation != Player.AnimationIndex.StandOnBeam
            && player.animation != Player.AnimationIndex.BeamTip)
        {
            return false;
        }

        var input = playerModule.UnblockedInput;

        if (!ModOptions.UsesCustomStoreKeybind)
        {
            return input.y == 1.0f && input.pckp && !input.jmp;
        }

        if (ModCompat_Helpers.IsIICActive)
        {
            return player.IsStorePressedIIC();
        }

        return player.playerState.playerNumber switch
        {
            0 => Input.GetKey(ModOptions.StoreKeybindPlayer1) || Input.GetKey(ModOptions.StoreKeybindKeyboard),
            1 => Input.GetKey(ModOptions.StoreKeybindPlayer2),
            2 => Input.GetKey(ModOptions.StoreKeybindPlayer3),
            3 => Input.GetKey(ModOptions.StoreKeybindPlayer4),

            _ => false,
        };
    }

    public static bool IsSwapKeybindPressed(this Player player)
    {
        if (ModCompat_Helpers.IsIICActive)
        {
            return player.IsSwapPressedIIC();
        }

        return player.playerState.playerNumber switch
        {
            0 => Input.GetKey(ModOptions.SwapKeybindPlayer1) || Input.GetKey(ModOptions.SwapKeybindKeyboard),
            1 => Input.GetKey(ModOptions.SwapKeybindPlayer2),
            2 => Input.GetKey(ModOptions.SwapKeybindPlayer3),
            3 => Input.GetKey(ModOptions.SwapKeybindPlayer4),

            _ => false,
        };
    }

    public static bool IsSwapLeftInput(this Player player)
    {
        if (ModOptions.SwapTriggerPlayer != 0)
        {
            // Normal
            if (ModOptions.SwapTriggerPlayer > 0)
            {
                if (player.playerState.playerNumber == ModOptions.SwapTriggerPlayer - 1 || player.IsSingleplayer())
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
                if (player.playerState.playerNumber == -ModOptions.SwapTriggerPlayer + 1 || player.IsSingleplayer())
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

        return player.input[0].controllerType == Options.ControlSetup.Preset.KeyboardSinglePlayer && Input.GetKey(ModOptions.SwapLeftKeybind);
    }

    public static bool IsSwapRightInput(this Player player)
    {
        if (ModOptions.SwapTriggerPlayer != 0)
        {
            // Normal
            if (ModOptions.SwapTriggerPlayer > 0)
            {
                if (player.playerState.playerNumber == ModOptions.SwapTriggerPlayer - 1 || player.IsSingleplayer())
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
                if (player.playerState.playerNumber == -ModOptions.SwapTriggerPlayer + 1 || player.IsSingleplayer())
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

        return player.input[0].controllerType == Options.ControlSetup.Preset.KeyboardSinglePlayer && Input.GetKey(ModOptions.SwapRightKeybind);
    }


    // Ability
    public static bool IsCustomAbilityKeybindPressed(this Player player)
    {
        if (ModCompat_Helpers.IsIICActive)
        {
            return player.IsAbilityPressedIIC();
        }

        return player.playerState.playerNumber switch
        {
            0 => Input.GetKey(ModOptions.AbilityKeybindPlayer1) || Input.GetKey(ModOptions.AbilityKeybindKeyboard),
            1 => Input.GetKey(ModOptions.AbilityKeybindPlayer2),
            2 => Input.GetKey(ModOptions.AbilityKeybindPlayer3),
            3 => Input.GetKey(ModOptions.AbilityKeybindPlayer4),

            _ => false,
        };
    }

    public static bool IsSentryKeybindPressed(this Player player, PlayerModule playerModule)
    {
        if (ModOptions.CustomSentryKeybind)
        {
            if (ModCompat_Helpers.IsIICActive)
            {
                return player.IsSentryPressedIIC();
            }

            return player.playerState.playerNumber switch
            {
                0 => Input.GetKey(ModOptions.SentryKeybindPlayer1) || Input.GetKey(ModOptions.SentryKeybindKeyboard),
                1 => Input.GetKey(ModOptions.SentryKeybindPlayer2),
                2 => Input.GetKey(ModOptions.SentryKeybindPlayer3),
                3 => Input.GetKey(ModOptions.SentryKeybindPlayer4),

                _ => false,
            };
        }

        var input = playerModule.UnblockedInput;

        return input.jmp && input.pckp && input.y == -1;
    }


    // Custom Ability
    public static bool IsAgilityKeybindPressed(this Player player, PlayerModule playerModule)
    {
        if (ModOptions.CustomAgilityKeybind)
        {
            return IsCustomAbilityKeybindPressed(player);
        }

        var input = playerModule.UnblockedInput;
        return input.jmp && input.pckp;
    }

    public static bool IsSpearCreationKeybindPressed(this Player player, PlayerModule playerModule)
    {
        if (ModOptions.CustomSpearKeybind)
        {
            return IsCustomAbilityKeybindPressed(player);
        }

        var input = playerModule.UnblockedInput;
        return input.pckp;
    }

    public static bool IsReviveKeybindPressed(this Player player, PlayerModule playerModule)
    {
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

        return (controller ? ModOptions.StoreKeybindPlayer1 : ModOptions.StoreKeybindKeyboard).GetDisplayName();
    }

    public static string GetSwapKeybindDisplayName(bool controller)
    {
        if (ModCompat_Helpers.IsIICActive)
        {
            return Input_Helpers_IIC.GetSwapKeybindIIC(controller).GetDisplayName();
        }

        return (controller ? ModOptions.SwapKeybindPlayer1 : ModOptions.SwapKeybindKeyboard).GetDisplayName();
    }

    public static string GetSentryKeybindDisplayName(bool controller)
    {
        if (ModCompat_Helpers.IsIICActive)
        {
            return Input_Helpers_IIC.GetSentryKeybindIIC(controller).GetDisplayName();
        }

        return (controller ? ModOptions.SentryKeybindPlayer1 : ModOptions.SentryKeybindKeyboard).GetDisplayName();
    }

    public static string GetAbilityKeybindDisplayName(bool controller)
    {
        if (ModCompat_Helpers.IsIICActive)
        {
            return Input_Helpers_IIC.GetAbilityKeybindIIC(controller).GetDisplayName();
        }

        return (controller ? ModOptions.AbilityKeybindPlayer1 : ModOptions.AbilityKeybindKeyboard).GetDisplayName();
    }


    public static string GetSwapLeftKeybindDisplayName(bool controller)
    {
        if (ModCompat_Helpers.IsIICActive)
        {
            return Input_Helpers_IIC.GetSwapLeftKeybindIIC(controller).GetDisplayName();
        }

        return ModOptions.SwapLeftKeybind.GetDisplayName();
    }

    public static string GetSwapRightKeybindDisplayName(bool controller)
    {
        if (ModCompat_Helpers.IsIICActive)
        {
            return Input_Helpers_IIC.GetSwapRightKeybindIIC(controller).GetDisplayName();
        }

        return ModOptions.SwapRightKeybind.GetDisplayName();
    }
}
