using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Pearlcat;

public static partial class Hooks
{
    public static bool IsImprovedInputActive => ModManager.ActiveMods.Any(x => x.id == "improved-input-config");

    public static void InitIICKeybinds() => IICKeybinds.InitKeybinds();

    public static bool IsStorePressedIIC(this Player player) => IICKeybinds.IsStorePressed(player);
    public static bool IsSwapPressedIIC(this Player player) => IICKeybinds.IsSwapPressed(player);
    public static bool IsSwapLeftPressedIIC(this Player player) => IICKeybinds.IsSwapLeftPressed(player);
    public static bool IsSwapRightPressedIIC(this Player player) => IICKeybinds.IsSwapRightPressed(player);
    public static bool IsSentryPressedIIC(this Player player) => IICKeybinds.IsSentryPressed(player);
    public static bool IsAbilityPressedIIC(this Player player) => IICKeybinds.IsAbilityPressed(player);


    // Inventory
    public static bool IsStoreKeybindPressed(this Player player, PlayerModule playerModule)
    {
        if (player.bodyMode != Player.BodyModeIndex.Stand && player.bodyMode != Player.BodyModeIndex.ZeroG && player.bodyMode != Player.BodyModeIndex.Swimming
            && player.animation != Player.AnimationIndex.StandOnBeam && player.animation != Player.AnimationIndex.BeamTip)
            return false;

        var input = playerModule.UnblockedInput;

        if (!ModOptions.UsesCustomStoreKeybind.Value)
        {
            return input.y == 1.0f && input.pckp && !input.jmp;
        }

        if (IsImprovedInputActive)
        {
            return player.IsStorePressedIIC();
        }

        return player.playerState.playerNumber switch
        {
            0 => Input.GetKey(ModOptions.StoreKeybindPlayer1.Value) || Input.GetKey(ModOptions.StoreKeybindKeyboard.Value),
            1 => Input.GetKey(ModOptions.StoreKeybindPlayer2.Value),
            2 => Input.GetKey(ModOptions.StoreKeybindPlayer3.Value),
            3 => Input.GetKey(ModOptions.StoreKeybindPlayer4.Value),

            _ => false
        };
    }


    public static bool IsSwapKeybindPressed(this Player player)
    {
        if (IsImprovedInputActive)
        {
            return player.IsSwapPressedIIC();
        }

        return player.playerState.playerNumber switch
        {
            0 => Input.GetKey(ModOptions.SwapKeybindPlayer1.Value) || Input.GetKey(ModOptions.SwapKeybindKeyboard.Value),
            1 => Input.GetKey(ModOptions.SwapKeybindPlayer2.Value),
            2 => Input.GetKey(ModOptions.SwapKeybindPlayer3.Value),
            3 => Input.GetKey(ModOptions.SwapKeybindPlayer4.Value),

            _ => false
        };
    }

    public static bool IsSwapLeftInput(this Player player)
    {
        if (Input.GetAxis("DschockHorizontalRight") < -0.25f && ModOptions.SwapTriggerPlayer.Value != 0 && (player.playerState.playerNumber == ModOptions.SwapTriggerPlayer.Value - 1 || player.IsSingleplayer()))
        {
            return true;
        }

        if (IsImprovedInputActive)
        {
            return player.IsSwapLeftPressedIIC();
        }

        return player.input[0].controllerType == Options.ControlSetup.Preset.KeyboardSinglePlayer && Input.GetKey(ModOptions.SwapLeftKeybind.Value);
    }

    public static bool IsSwapRightInput(this Player player)
    {
        if (Input.GetAxis("DschockHorizontalRight") > 0.25f && ModOptions.SwapTriggerPlayer.Value != 0 && (player.playerState.playerNumber == ModOptions.SwapTriggerPlayer.Value - 1 || player.IsSingleplayer()))
        {
            return true;
        }

        if (IsImprovedInputActive)
        {
            return player.IsSwapRightPressedIIC();
        }

        return player.input[0].controllerType == Options.ControlSetup.Preset.KeyboardSinglePlayer && Input.GetKey(ModOptions.SwapRightKeybind.Value);
    }
    
    // DEPRECATED
    public static int GetNumberPressed(this Player player)
    {
        //if (player.input[0].controllerType != Options.ControlSetup.Preset.KeyboardSinglePlayer)
        //    return -1;

        //for (int number = 0; number <= 9; number++)
        //    if (Input.GetKey(number.ToString()))
        //        return number;

        return -1;
    }



    // Ability
    public static bool IsCustomAbilityKeybindPressed(this Player player, PlayerModule playerModule)
    {
        if (IsImprovedInputActive)
        {
            return player.IsAbilityPressedIIC();
        }

        return player.playerState.playerNumber switch
        {
            0 => Input.GetKey(ModOptions.AbilityKeybindPlayer1.Value) || Input.GetKey(ModOptions.AbilityKeybindKeyboard.Value),
            1 => Input.GetKey(ModOptions.AbilityKeybindPlayer2.Value),
            2 => Input.GetKey(ModOptions.AbilityKeybindPlayer3.Value),
            3 => Input.GetKey(ModOptions.AbilityKeybindPlayer4.Value),

            _ => false
        };
    }
    
    public static bool IsSentryKeybindPressed(this Player player, PlayerModule playerModule)
    {
        if (ModOptions.CustomSentryKeybind.Value)
        {
            if (IsImprovedInputActive)
            {
                return player.IsSentryPressedIIC();
            }

            return player.playerState.playerNumber switch
            {
                0 => Input.GetKey(ModOptions.SentryKeybindPlayer1.Value) || Input.GetKey(ModOptions.SentryKeybindKeyboard.Value),
                1 => Input.GetKey(ModOptions.SentryKeybindPlayer2.Value),
                2 => Input.GetKey(ModOptions.SentryKeybindPlayer3.Value),
                3 => Input.GetKey(ModOptions.SentryKeybindPlayer4.Value),

                _ => false
            };
        }
        else
        {
            var input = playerModule.UnblockedInput;
            return input.jmp && input.pckp && input.y == -1;
        }
    }



    // Custom Ability
    public static bool IsAgilityKeybindPressed(this Player player, PlayerModule playerModule)
    {
        if (ModOptions.CustomAgilityKeybind.Value)
            return IsCustomAbilityKeybindPressed(player, playerModule);

        var input = playerModule.UnblockedInput;
        return input.jmp && input.pckp;
    }

    public static bool IsSpearCreationKeybindPressed(this Player player, PlayerModule playerModule)
    {
        if (ModOptions.CustomSpearKeybind.Value)
            return IsCustomAbilityKeybindPressed(player, playerModule);

        var input = playerModule.UnblockedInput;
        return input.pckp;
    }

    // DEPRECATED
    public static bool IsReviveKeybindPressed(this Player player, PlayerModule playerModule)
    {
        var input = playerModule.UnblockedInput;
        return input.pckp;
    }



    public static string GetDisplayName(this KeyCode keyCode)
    {
        var keyCodeChar = Regex.Replace(keyCode.ToString(), "Joystick[0-9]Button", "");

        if (!int.TryParse(keyCodeChar, out var buttonNum))
            return keyCode.ToString();

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
}
