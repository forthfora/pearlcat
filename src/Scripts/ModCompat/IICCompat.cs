using ImprovedInput;
using UnityEngine;

namespace Pearlcat;


public static class IICCompat
{
    public static void InitCompat()
    {
        var t = Utils.Translator;

        StoreKeybind.Description = t.Translate("Stores the pearl in your main hand, or retrieves the current active pearl if your main hand is empty.");
        SwapKeybind.Description = t.Translate("Opens the inventory UI when held, allowing the active pearl to be swapped with the movement keys.");
        SwapLeftKeybind.Description = t.Translate("Swaps the active pearl to the left.");
        SwapRightKeybind.Description = t.Translate("Swaps the active pearl to the right.");
        SentryKeybind.Description = t.Translate("Deploys the active pearl as a sentry, or returns it if it is already deployed.");
        AbilityKeybind.Description = t.Translate("Custom keybind to perform certain pearl abilities, configure which use the custom bind in Pearlcat's Remix config.");

        var hide = ModOptions.DisableImprovedInputConfig.Value;

        StoreKeybind.HideConfig = hide;
        SwapKeybind.HideConfig = hide;
        SwapLeftKeybind.HideConfig = hide;
        SwapRightKeybind.HideConfig = hide;
        SentryKeybind.HideConfig = hide;
        AbilityKeybind.HideConfig = hide;
    }

    public static PlayerKeybind StoreKeybind { get; } = PlayerKeybind.Register($"{Plugin.MOD_ID}:store_pearl", Plugin.MOD_NAME, "Store Pearl", KeyCode.None, KeyCode.None);
    public static bool IsStorePressed(Player player)
    {
        return player.IsPressed(StoreKeybind);
    }

    public static KeyCode GetStoreKeybind(bool controller)
    {
        return controller ? StoreKeybind.GamepadPreset : StoreKeybind.CurrentBinding(0);
    }


    public static PlayerKeybind SwapKeybind { get; } = PlayerKeybind.Register($"{Plugin.MOD_ID}:swap_pearl", Plugin.MOD_NAME, "Swap Pearl", KeyCode.LeftAlt, KeyCode.JoystickButton3);
    public static bool IsSwapPressed(Player player)
    {
        return player.IsPressed(SwapKeybind);
    }

    public static KeyCode GetSwapKeybind(bool controller)
    {
        return controller ? SwapKeybind.GamepadPreset : SwapKeybind.CurrentBinding(0);
    }


    public static PlayerKeybind SwapLeftKeybind { get; } = PlayerKeybind.Register($"{Plugin.MOD_ID}:swap_left", Plugin.MOD_NAME, "Swap Left", KeyCode.A, KeyCode.None);
    public static bool IsSwapLeftPressed(Player player)
    {
        return player.IsPressed(SwapLeftKeybind);
    }

    public static KeyCode GetSwapLeftKeybind(bool controller)
    {
        return controller ? SwapLeftKeybind.GamepadPreset : SwapLeftKeybind.CurrentBinding(0);
    }


    public static PlayerKeybind SwapRightKeybind { get; } = PlayerKeybind.Register($"{Plugin.MOD_ID}:swap_right", Plugin.MOD_NAME, "Swap Right", KeyCode.D, KeyCode.None);
    public static bool IsSwapRightPressed(Player player)
    {
        return player.IsPressed(SwapRightKeybind);
    }

    public static KeyCode GetSwapRightKeybind(bool controller)
    {
        return controller ? SwapRightKeybind.GamepadPreset : SwapRightKeybind.CurrentBinding(0);
    }


    public static PlayerKeybind SentryKeybind { get; } = PlayerKeybind.Register($"{Plugin.MOD_ID}:sentry_pearl", Plugin.MOD_NAME, "Sentry Pearl", KeyCode.C, KeyCode.JoystickButton4);
    public static bool IsSentryPressed(Player player)
    {
        return player.IsPressed(SentryKeybind);
    }

    public static KeyCode GetSentryKeybind(bool controller)
    {
        return controller ? SentryKeybind.GamepadPreset : SentryKeybind.CurrentBinding(0);
    }


    public static PlayerKeybind AbilityKeybind { get; } = PlayerKeybind.Register($"{Plugin.MOD_ID}:ability", Plugin.MOD_NAME, "Pearl Ability", KeyCode.None, KeyCode.None);
    public static bool IsAbilityPressed(Player player)
    {
        return player.IsPressed(AbilityKeybind);
    }

    public static KeyCode GetAbilityKeybind(bool controller)
    {
        return controller ? AbilityKeybind.GamepadPreset : AbilityKeybind.CurrentBinding(0);
    }
}
