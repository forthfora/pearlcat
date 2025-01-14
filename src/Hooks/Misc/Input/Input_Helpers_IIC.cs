using UnityEngine;

namespace Pearlcat;

// Improved Input Config integration - need the buffer methods so it's not a hard dependency
public static class Input_Helpers_IIC
{
    public static bool IsStorePressedIIC(this Player player)
    {
        return IICCompat.IsStorePressed(player);
    }

    public static bool IsSwapPressedIIC(this Player player)
    {
        return IICCompat.IsSwapPressed(player);
    }

    public static bool IsSwapLeftPressedIIC(this Player player)
    {
        return IICCompat.IsSwapLeftPressed(player);
    }

    public static bool IsSwapRightPressedIIC(this Player player)
    {
        return IICCompat.IsSwapRightPressed(player);
    }

    public static bool IsSentryPressedIIC(this Player player)
    {
        return IICCompat.IsSentryPressed(player);
    }

    public static bool IsAbilityPressedIIC(this Player player)
    {
        return IICCompat.IsAbilityPressed(player);
    }

    public static KeyCode GetStoreKeybindIIC(bool controller)
    {
        return IICCompat.GetStoreKeybind(controller);
    }

    public static KeyCode GetSwapKeybindIIC(bool controller)
    {
        return IICCompat.GetSwapKeybind(controller);
    }

    public static KeyCode GetSwapLeftKeybindIIC(bool controller)
    {
        return IICCompat.GetSwapLeftKeybind(controller);
    }

    public static KeyCode GetSwapRightKeybindIIC(bool controller)
    {
        return IICCompat.GetSwapRightKeybind(controller);
    }

    public static KeyCode GetSentryKeybindIIC(bool controller)
    {
        return IICCompat.GetSentryKeybind(controller);
    }

    public static KeyCode GetAbilityKeybindIIC(bool controller)
    {
        return IICCompat.GetAbilityKeybind(controller);
    }
}
