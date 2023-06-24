using UnityEngine;

namespace Pearlcat;

public static partial class Hooks
{
    public static bool IsStoreKeybindPressed(this Player player)
    {
        if (!PearlcatOptions.usesCustomStoreKeybind.Value)
            return player.input[0].y == 1.0f && player.input[0].pckp;

        return player.playerState.playerNumber switch
        {
            0 => Input.GetKey(PearlcatOptions.storeKeybindPlayer1.Value) || Input.GetKey(PearlcatOptions.storeKeybindKeyboard.Value),
            1 => Input.GetKey(PearlcatOptions.storeKeybindPlayer2.Value),
            2 => Input.GetKey(PearlcatOptions.storeKeybindPlayer3.Value),
            3 => Input.GetKey(PearlcatOptions.storeKeybindPlayer4.Value),

            _ => false
        };
    }

    public static bool IsAbilityKeybindPressed(this Player player)
    {
        if (!PearlcatOptions.usesCustomAbilityKeybind.Value)
            return player.input[0].jmp && player.input[0].pckp;

        return player.playerState.playerNumber switch
        {
            0 => Input.GetKey(PearlcatOptions.abilityKeybindPlayer1.Value) || Input.GetKey(PearlcatOptions.abilityKeybindKeyboard.Value),
            1 => Input.GetKey(PearlcatOptions.abilityKeybindPlayer2.Value),
            2 => Input.GetKey(PearlcatOptions.abilityKeybindPlayer3.Value),
            3 => Input.GetKey(PearlcatOptions.abilityKeybindPlayer4.Value),

            _ => false
        };
    }

    public static bool IsSwapKeybindPressed(this Player player)
    {
        return player.playerState.playerNumber switch
        {
            0 => Input.GetKey(PearlcatOptions.swapKeybindPlayer1.Value) || Input.GetKey(PearlcatOptions.swapKeybindKeyboard.Value),
            1 => Input.GetKey(PearlcatOptions.swapKeybindPlayer2.Value),
            2 => Input.GetKey(PearlcatOptions.swapKeybindPlayer3.Value),
            3 => Input.GetKey(PearlcatOptions.swapKeybindPlayer4.Value),

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
}
