using UnityEngine;

namespace Pearlcat;

// Improved Input Config integration - need the buffer methods so it's not a hard dependency
public static partial class Input_Helpers
{
    public static void InitIICKeybinds()
    {
        IICKeybinds.InitKeybinds();
    }

    public static bool IsStorePressedIIC(this Player player)
    {
        return IICKeybinds.IsStorePressed(player);
    }

    public static bool IsSwapPressedIIC(this Player player)
    {
        return IICKeybinds.IsSwapPressed(player);
    }

    public static bool IsSwapLeftPressedIIC(this Player player)
    {
        return IICKeybinds.IsSwapLeftPressed(player);
    }

    public static bool IsSwapRightPressedIIC(this Player player)
    {
        return IICKeybinds.IsSwapRightPressed(player);
    }

    public static bool IsSentryPressedIIC(this Player player)
    {
        return IICKeybinds.IsSentryPressed(player);
    }

    public static bool IsAbilityPressedIIC(this Player player)
    {
        return IICKeybinds.IsAbilityPressed(player);
    }

    public static KeyCode GetStoreKeybindIIC(bool controller)
    {
        return IICKeybinds.GetStoreKeybind(controller);
    }

    public static KeyCode GetSwapKeybindIIC(bool controller)
    {
        return IICKeybinds.GetSwapKeybind(controller);
    }

    public static KeyCode GetSwapLeftKeybindIIC(bool controller)
    {
        return IICKeybinds.GetSwapLeftKeybind(controller);
    }

    public static KeyCode GetSwapRightKeybindIIC(bool controller)
    {
        return IICKeybinds.GetSwapRightKeybind(controller);
    }

    public static KeyCode GetSentryKeybindIIC(bool controller)
    {
        return IICKeybinds.GetSentryKeybind(controller);
    }

    public static KeyCode GetAbilityKeybindIIC(bool controller)
    {
        return IICKeybinds.GetAbilityKeybind(controller);
    }
}
