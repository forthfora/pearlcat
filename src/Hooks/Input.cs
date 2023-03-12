using IL.Menu.Remix.MixedUI;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using SlugBase.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace TheSacrifice
{
    internal static partial class Hooks
    {
        private static bool IsStoreKeybindPressed(Player player)
        {
            if (!Options.usesCustomStoreKeybind.Value && player.input[0].y == 1.0f && player.input[0].pckp) return true;

            return player.playerState.playerNumber switch
            {
                0 => Input.GetKey(Options.storeKeybindPlayer1.Value) || Input.GetKey(Options.storeKeybindKeyboard.Value),
                1 => Input.GetKey(Options.storeKeybindPlayer2.Value),
                2 => Input.GetKey(Options.storeKeybindPlayer3.Value),
                3 => Input.GetKey(Options.storeKeybindPlayer4.Value),

                _ => false
            };
        }

        private static bool IsDashKeybindPressed(Player player)
        {
            if (!Options.usesCustomDashKeybind.Value && player.input[0].jmp && player.input[0].pckp) return true;

            return player.playerState.playerNumber switch
            {
                0 => Input.GetKey(Options.dashKeybindPlayer1.Value) || Input.GetKey(Options.dashKeybindKeyboard.Value),
                1 => Input.GetKey(Options.dashKeybindPlayer2.Value),
                2 => Input.GetKey(Options.dashKeybindPlayer3.Value),
                3 => Input.GetKey(Options.dashKeybindPlayer4.Value),

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
}
