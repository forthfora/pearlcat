using UnityEngine;

namespace Pearlcat;

public static partial class Hooks
{
    public static bool IsStoreKeybindPressed(this Player player, PlayerModule playerModule)
    {
        var input = playerModule.UnblockedInput;

        if (!PearlcatOptions.UsesCustomStoreKeybind.Value)
            return input.y == 1.0f && input.pckp && !input.jmp;

        return player.playerState.playerNumber switch
        {
            0 => Input.GetKey(PearlcatOptions.StoreKeybindPlayer1.Value) || Input.GetKey(PearlcatOptions.StoreKeybindKeyboard.Value),
            1 => Input.GetKey(PearlcatOptions.StoreKeybindPlayer2.Value),
            2 => Input.GetKey(PearlcatOptions.StoreKeybindPlayer3.Value),
            3 => Input.GetKey(PearlcatOptions.StoreKeybindPlayer4.Value),

            _ => false
        };
    }

    public static bool IsAbilityKeybindPressed(this Player player, PlayerModule playerModule)
    {
        var input = playerModule.UnblockedInput;

        if (!PearlcatOptions.UsesCustomAbilityKeybind.Value)
            return input.jmp && input.pckp;

        return player.playerState.playerNumber switch
        {
            0 => Input.GetKey(PearlcatOptions.AbilityKeybindPlayer1.Value) || Input.GetKey(PearlcatOptions.AbilityKeybindKeyboard.Value),
            1 => Input.GetKey(PearlcatOptions.AbilityKeybindPlayer2.Value),
            2 => Input.GetKey(PearlcatOptions.AbilityKeybindPlayer3.Value),
            3 => Input.GetKey(PearlcatOptions.AbilityKeybindPlayer4.Value),

            _ => false
        };
    }

    public static bool IsSwapKeybindPressed(this Player player)
    {
        return player.playerState.playerNumber switch
        {
            0 => Input.GetKey(PearlcatOptions.SwapKeybindPlayer1.Value) || Input.GetKey(PearlcatOptions.SwapKeybindKeyboard.Value),
            1 => Input.GetKey(PearlcatOptions.SwapKeybindPlayer2.Value),
            2 => Input.GetKey(PearlcatOptions.SwapKeybindPlayer3.Value),
            3 => Input.GetKey(PearlcatOptions.SwapKeybindPlayer4.Value),

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
        => Input.GetKey(PearlcatOptions.SwapLeftKeybind.Value)
        || (Input.GetAxis("DschockHorizontalLeft") > 0.5f && player.playerState.playerNumber == PearlcatOptions.SwapTriggerPlayer.Value);

    public static bool IsSwapRightInput(this Player player)
        => Input.GetKey(PearlcatOptions.SwapRightKeybind.Value)
        || (Input.GetAxis("DschockHorizontalRight") > 0.5f && player.playerState.playerNumber == PearlcatOptions.SwapTriggerPlayer.Value);
}
