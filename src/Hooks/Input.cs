using UnityEngine;

namespace TheSacrifice;

public static partial class Hooks
{
    private static bool IsStoreKeybindPressed(Player player)
    {
        if (!Options.usesCustomStoreKeybind.Value)
            return player.input[0].y == 1.0f && player.input[0].pckp;

        return player.playerState.playerNumber switch
        {
            0 => Input.GetKey(Options.storeKeybindPlayer1.Value) || Input.GetKey(Options.storeKeybindKeyboard.Value),
            1 => Input.GetKey(Options.storeKeybindPlayer2.Value),
            2 => Input.GetKey(Options.storeKeybindPlayer3.Value),
            3 => Input.GetKey(Options.storeKeybindPlayer4.Value),

            _ => false
        };
    }

    private static bool IsAbilityKeybindPressed(Player player)
    {
        if (!Options.usesCustomDashKeybind.Value)
            return player.input[0].jmp && player.input[0].pckp;

        return player.playerState.playerNumber switch
        {
            0 => Input.GetKey(Options.abilityKeybindPlayer1.Value) || Input.GetKey(Options.abilityKeybindKeyboard.Value),
            1 => Input.GetKey(Options.abilityKeybindPlayer2.Value),
            2 => Input.GetKey(Options.abilityKeybindPlayer3.Value),
            3 => Input.GetKey(Options.abilityKeybindPlayer4.Value),

            _ => false
        };
    }

    private static bool IsSwapKeybindPressed(Player player)
    {
        return player.playerState.playerNumber switch
        {
            0 => Input.GetKey(Options.swapKeybindPlayer1.Value) || Input.GetKey(Options.swapKeybindKeyboard.Value),
            1 => Input.GetKey(Options.swapKeybindPlayer2.Value),
            2 => Input.GetKey(Options.swapKeybindPlayer3.Value),
            3 => Input.GetKey(Options.swapKeybindPlayer4.Value),

            _ => false
        };
    }
}
