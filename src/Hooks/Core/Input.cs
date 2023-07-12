using UnityEngine;

namespace Pearlcat;

public static partial class Hooks
{
    public static bool IsStoreKeybindPressed(this Player player, PlayerModule playerModule)
    {
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

    public static bool IsAbilityKeybindPressed(this Player player, PlayerModule playerModule)
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

    public static bool IsDoubleJumpKeybindPressed(this Player player, PlayerModule playerModule)
    {
        if (ModOptions.PreferCustomAbilityKeybind.Value)
            return IsAbilityKeybindPressed(player, playerModule);

        var input = playerModule.UnblockedInput;
        return input.jmp && input.pckp;
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
        => Input.GetKey(ModOptions.SwapLeftKeybind.Value)
        || (Input.GetAxis("DschockHorizontalRight") < -0.5f && player.playerState.playerNumber == ModOptions.SwapTriggerPlayer.Value - 1);

    public static bool IsSwapRightInput(this Player player)
        => Input.GetKey(ModOptions.SwapRightKeybind.Value)
        || (Input.GetAxis("DschockHorizontalRight") > 0.5f && player.playerState.playerNumber == ModOptions.SwapTriggerPlayer.Value - 1);
}
