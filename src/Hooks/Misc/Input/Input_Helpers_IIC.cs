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

    public static string GetStoreBindingNameIIC()
    {
        return IICCompat.GetStoreBindingName();
    }

    public static string GetSwapBindingNameIIC()
    {
        return IICCompat.GetSwapBindingName();
    }

    public static string GetSwapLeftKeybindNameIIC()
    {
        return IICCompat.GetSwapLeftBindingName();
    }

    public static string GetSwapRightKeybindNameIIC()
    {
        return IICCompat.GetSwapRightBindingName();
    }

    public static string GetSentryKeybindNameIIC()
    {
        return IICCompat.GetSentryBindingName();
    }

    public static string GetAbilityKeybindNameIIC()
    {
        return IICCompat.GetAbilityBindingName();
    }
}
