using ImprovedInput;

namespace Pearlcat;


public static class IICCompat
{
    public static PlayerKeybind StoreKeybind { get; } = PlayerKeybind.Register($"{Plugin.MOD_ID}:store_pearl", "Pearlcat", "Store Pearl", KeyCode.None, KeyCode.None);

    public static PlayerKeybind SwapKeybind { get; } = PlayerKeybind.Register($"{Plugin.MOD_ID}:swap_pearl", "Pearlcat", "Swap Pearl", KeyCode.LeftAlt, KeyCode.JoystickButton3);
    public static PlayerKeybind SwapLeftKeybind { get; } = PlayerKeybind.Register($"{Plugin.MOD_ID}:swap_left", "Pearlcat", "Swap Left", KeyCode.A, KeyCode.None);
    public static PlayerKeybind SwapRightKeybind { get; } = PlayerKeybind.Register($"{Plugin.MOD_ID}:swap_right", "Pearlcat", "Swap Right", KeyCode.D, KeyCode.None);

    public static PlayerKeybind SentryKeybind { get; } = PlayerKeybind.Register($"{Plugin.MOD_ID}:sentry_pearl", "Pearlcat", "Sentry Pearl", KeyCode.C, KeyCode.JoystickButton4);
    public static PlayerKeybind AbilityKeybind { get; } = PlayerKeybind.Register($"{Plugin.MOD_ID}:ability", "Pearlcat", "Pearl Ability", KeyCode.None, KeyCode.None);

    public static void RefreshConfigs()
    {
        var t = Utils.Translator;

        StoreKeybind.Description = t.Translate("Stores the pearl in your main hand, or retrieves the current active pearl if your main hand is empty.");
        SwapKeybind.Description = t.Translate("Opens the inventory UI when held, allowing the active pearl to be swapped with the movement keys.");
        SwapLeftKeybind.Description = t.Translate("Swaps the active pearl to the left.");
        SwapRightKeybind.Description = t.Translate("Swaps the active pearl to the right.");
        SentryKeybind.Description = t.Translate("Deploys the active pearl as a sentry, or returns it if it is already deployed.");
        AbilityKeybind.Description = t.Translate("Custom keybind to perform certain pearl abilities, configure which use the custom bind in Pearlcat's Remix config.");

        var hide = ModOptions.DisableImprovedInputConfig;

        StoreKeybind.HideConfig = hide;
        SwapKeybind.HideConfig = hide;
        SwapLeftKeybind.HideConfig = hide;
        SwapRightKeybind.HideConfig = hide;
        SentryKeybind.HideConfig = hide;
        AbilityKeybind.HideConfig = hide;
    }

    
    public static bool IsStorePressed(Player player)
    {
        return player.IsPressed(StoreKeybind);
    }
    
    public static string GetStoreBindingName()
    {
        var firstPearlcat = (Utils.RainWorld.processManager.currentMainLoop as RainWorldGame)?.GetFirstPearlcat();
        var playerNumber = (firstPearlcat?.realizedCreature as Player)?.playerState.playerNumber ?? 0;
        return StoreKeybind.CurrentBindingName(playerNumber);
    }

    
    public static bool IsSwapPressed(Player player)
    {
        return player.IsPressed(SwapKeybind);
    }
    
    public static string GetSwapBindingName()
    {
        var firstPearlcat = (Utils.RainWorld.processManager.currentMainLoop as RainWorldGame)?.GetFirstPearlcat();
        var playerNumber = (firstPearlcat?.realizedCreature as Player)?.playerState.playerNumber ?? 0;
        return SwapKeybind.CurrentBindingName(playerNumber);
    }

    
    public static bool IsSwapLeftPressed(Player player)
    {
        return player.IsPressed(SwapLeftKeybind);
    }
    
    public static string GetSwapLeftBindingName()
    {
        var firstPearlcat = (Utils.RainWorld.processManager.currentMainLoop as RainWorldGame)?.GetFirstPearlcat();
        var playerNumber = (firstPearlcat?.realizedCreature as Player)?.playerState.playerNumber ?? 0;
        return SwapLeftKeybind.CurrentBindingName(playerNumber);
    }

    
    public static bool IsSwapRightPressed(Player player)
    {
        return player.IsPressed(SwapRightKeybind);
    }
    
    public static string GetSwapRightBindingName()
    {
        var firstPearlcat = (Utils.RainWorld.processManager.currentMainLoop as RainWorldGame)?.GetFirstPearlcat();
        var playerNumber = (firstPearlcat?.realizedCreature as Player)?.playerState.playerNumber ?? 0;
        return SwapRightKeybind.CurrentBindingName(playerNumber);
    }

    
    public static bool IsSentryPressed(Player player)
    {
        return player.IsPressed(SentryKeybind);
    }
    
    public static string GetSentryBindingName()
    {
        var firstPearlcat = (Utils.RainWorld.processManager.currentMainLoop as RainWorldGame)?.GetFirstPearlcat();
        var playerNumber = (firstPearlcat?.realizedCreature as Player)?.playerState.playerNumber ?? 0;
        return SentryKeybind.CurrentBindingName(playerNumber);
    }

    
    public static bool IsAbilityPressed(Player player)
    {
        return player.IsPressed(AbilityKeybind);
    }
    
    public static string GetAbilityBindingName()
    {
        var firstPearlcat = (Utils.RainWorld.processManager.currentMainLoop as RainWorldGame)?.GetFirstPearlcat();
        var playerNumber = (firstPearlcat?.realizedCreature as Player)?.playerState.playerNumber ?? 0;
        return AbilityKeybind.CurrentBindingName(playerNumber);
    }
}
