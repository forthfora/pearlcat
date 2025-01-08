using System.Linq;

namespace Pearlcat;

public static class ModCompat_Helpers
{
    public static void InitModCompat()
    {
        if (IsModEnabled_ImprovedInputConfig)
        {
            Input_Helpers_IIC.InitIICKeybinds();
        }

        if (IsModEnabled_ChasingWind)
        {
            InitCWIntegration();
        }

        if (IsModEnabled_RainMeadow)
        {
            ModCompat_RainMeadow_Hooks.ApplyHooks();
        }
    }


    // Warp
    public static bool IsWarpAllowed(this RainWorldGame game)
    {
        return game.IsStorySession && (!ModManager.MSC || !game.rainWorld.safariMode);
    }


    // Mira Installation
    public static bool IsModEnabled_MiraInstallation => ModManager.ActiveMods.Any(x => x.id == "mira");
    public static bool ShowMiraVersionWarning => false; // TODO


    // Chasing Wind
    public static bool IsModEnabled_ChasingWind => ModManager.ActiveMods.Any(x => x.id == "myr.chasing_wind");
    public static void InitCWIntegration()
    {
        CWIntegration.Init();
    }


    // Improved Input Config
    public static bool IsModEnabled_ImprovedInputConfig => ModManager.ActiveMods.Any(x => x.id == "improved-input-config");
    public static bool IsIICActive => IsModEnabled_ImprovedInputConfig && !ModOptions.DisableImprovedInputConfig.Value;


    // Rain Meadow
    public static bool IsModEnabled_RainMeadow => ModManager.ActiveMods.Any(x => x.id == "henpemaz_rainmeadow");
    public static bool RainMeadow_IsOwner => !IsModEnabled_RainMeadow || ModCompat_RainMeadow_Helpers.IsOwner;
}
