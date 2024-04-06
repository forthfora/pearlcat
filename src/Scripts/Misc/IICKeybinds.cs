using ImprovedInput;
using UnityEngine;

namespace Pearlcat;


public static class IICKeybinds
{
    public static void InitKeybinds()
    {
        Plugin.Logger.LogInfo("Pearlcat + Improved Input Config integration init!");

        StoreKeybind.Description = "Stores the pearl in your main hand, or retrieves the current active pearl if your main hand is empty.";
        SwapKeybind.Description = "Opens the inventory UI when held, allowing the active pearl to be swapped with the movement keys.";
        SwapLeftKeybind.Description = "Swaps the active pearl to the left.";
        SwapRightKeybind.Description = "Swaps the active pearl to the right.";
        SentryKeybind.Description = "Deploys the active pearl as a sentry, or returns it if it is already deployed.";
        AbilityKeybind.Description = "Custom keybind to perform certain pearl abilities, configure which use the custom bind in Pearlcat's Remix config.";
    }

    public static readonly PlayerKeybind StoreKeybind = PlayerKeybind.Register($"{Plugin.MOD_ID}:store_pearl", Plugin.MOD_NAME, "Store Pearl", KeyCode.None, KeyCode.None);
    public static bool IsStorePressed(Player player) => player.IsPressed(StoreKeybind);


    public static readonly PlayerKeybind SwapKeybind = PlayerKeybind.Register($"{Plugin.MOD_ID}:swap_pearl", Plugin.MOD_NAME, "Swap Pearl", KeyCode.LeftAlt, KeyCode.JoystickButton3);
    public static bool IsSwapPressed(Player player) => player.IsPressed(SwapKeybind);


    public static readonly PlayerKeybind SwapLeftKeybind = PlayerKeybind.Register($"{Plugin.MOD_ID}:swap_left", Plugin.MOD_NAME, "Swap Left", KeyCode.A, KeyCode.None);
    public static bool IsSwapLeftPressed(Player player) => player.IsPressed(SwapLeftKeybind);


    public static readonly PlayerKeybind SwapRightKeybind = PlayerKeybind.Register($"{Plugin.MOD_ID}:swap_right", Plugin.MOD_NAME, "Swap Right", KeyCode.D, KeyCode.None);
    public static bool IsSwapRightPressed(Player player) => player.IsPressed(SwapRightKeybind);


    public static readonly PlayerKeybind SentryKeybind = PlayerKeybind.Register($"{Plugin.MOD_ID}:sentry_pearl", Plugin.MOD_NAME, "Sentry Pearl", KeyCode.C, KeyCode.JoystickButton4);
    public static bool IsSentryPressed(Player player) => player.IsPressed(SentryKeybind);


    public static readonly PlayerKeybind AbilityKeybind = PlayerKeybind.Register($"{Plugin.MOD_ID}:ability", Plugin.MOD_NAME, "Pearl Ability", KeyCode.None, KeyCode.None);
    public static bool IsAbilityPressed(Player player) => player.IsPressed(AbilityKeybind);
}
