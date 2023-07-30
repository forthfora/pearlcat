using System.Text.RegularExpressions;
using UnityEngine;

namespace Pearlcat;

public static partial class Hooks
{
    public static bool IsStoreKeybindPressed(this Player player, PlayerModule playerModule)
    {
        if (player.bodyMode != Player.BodyModeIndex.Stand && player.bodyMode != Player.BodyModeIndex.ZeroG && player.bodyMode != Player.BodyModeIndex.Swimming)
            return false;

        var input = playerModule.UnblockedInput;

        if (!ModOptions.UsesCustomStoreKeybind.Value)
            return input.y == 1.0f && input.pckp && !input.jmp;

        return player.playerState.playerNumber switch
        {
            0 => Input.GetKey(ModOptions.StoreKeybindPlayer1.Value) || Input.GetKey(ModOptions.StoreKeybindKeyboard.Value),
            1 => Input.GetKey(ModOptions.StoreKeybindPlayer2.Value),
            2 => Input.GetKey(ModOptions.StoreKeybindPlayer3.Value),
            3 => Input.GetKey(ModOptions.StoreKeybindPlayer4.Value),

            _ => false
        };
    }

public static bool IsCustomAbilityKeybindPressed(this Player player, PlayerModule playerModule)
{
    return player.playerState.playerNumber switch
    {
        0 => Input.GetKey(ModOptions.AbilityKeybindPlayer1.Value) || Input.GetKey(ModOptions.AbilityKeybindKeyboard.Value),
        1 => Input.GetKey(ModOptions.AbilityKeybindPlayer2.Value),
        2 => Input.GetKey(ModOptions.AbilityKeybindPlayer3.Value),
        3 => Input.GetKey(ModOptions.AbilityKeybindPlayer4.Value),

        _ => false
    };
}

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

    public static bool IsReviveKeybindPressed(this Player player, PlayerModule playerModule)
    {
        //if (ModOptions.CustomReviveKeybind.Value)
        //    return IsCustomAbilityKeybindPressed(player, playerModule);

        var input = playerModule.UnblockedInput;
        return input.pckp;
    }


    public static bool IsSwapKeybindPressed(this Player player)
    {
        return player.playerState.playerNumber switch
        {
            0 => Input.GetKey(ModOptions.SwapKeybindPlayer1.Value) || Input.GetKey(ModOptions.SwapKeybindKeyboard.Value),
            1 => Input.GetKey(ModOptions.SwapKeybindPlayer2.Value),
            2 => Input.GetKey(ModOptions.SwapKeybindPlayer3.Value),
            3 => Input.GetKey(ModOptions.SwapKeybindPlayer4.Value),

            _ => false
        };
    }

    public static int GetNumberPressed(this Player player)
    {
        if (player.input[0].controllerType != Options.ControlSetup.Preset.KeyboardSinglePlayer)
            return -1;

        for (int number = 0; number <= 9; number++)
            if (Input.GetKey(number.ToString()))
                return number;

        return -1;
    }


    public static bool IsSwapLeftInput(this Player player)
        => (player.input[0].controllerType == Options.ControlSetup.Preset.KeyboardSinglePlayer && Input.GetKey(ModOptions.SwapLeftKeybind.Value))
        || (Input.GetAxis("DschockHorizontalRight") < -0.5f && player.playerState.playerNumber == ModOptions.SwapTriggerPlayer.Value - 1);

    public static bool IsSwapRightInput(this Player player)
        => (player.input[0].controllerType == Options.ControlSetup.Preset.KeyboardSinglePlayer && Input.GetKey(ModOptions.SwapRightKeybind.Value))
        || (Input.GetAxis("DschockHorizontalRight") > 0.5f && player.playerState.playerNumber == ModOptions.SwapTriggerPlayer.Value - 1);


    public static string GetDisplayName(this KeyCode keyCode)
    {
        var keyCodeChar = Regex.Replace(keyCode.ToString(), "Joystick[0-9]Button", "");

        if (!int.TryParse(keyCodeChar, out var buttonNum))
            return keyCode.ToString();

        return buttonNum switch
        {
            0 => "Button South",
            1 => "Button East",
            2 => "Button West",
            3 => "Button North",
            4 => "Left Bumper",
            5 => "Right Bumper",
            6 => "Menu",
            7 => "View",
            8 => "L-Stick",
            9 => "R-Stick",

            _ => keyCode.ToString(),
        };
    }
}
