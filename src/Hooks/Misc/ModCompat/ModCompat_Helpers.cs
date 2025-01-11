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
            InitChasingWindCompat();
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
    public static void InitChasingWindCompat()
    {
        CWCompat.Init();
    }


    // Improved Input Config
    public static bool IsModEnabled_ImprovedInputConfig => ModManager.ActiveMods.Any(x => x.id == "improved-input-config");
    public static bool IsIICActive => IsModEnabled_ImprovedInputConfig && !ModOptions.DisableImprovedInputConfig.Value;


    // Rain Meadow
    public static bool IsModEnabled_RainMeadow => ModManager.ActiveMods.Any(x => x.id == "henpemaz_rainmeadow");
    public static bool RainMeadow_IsOwner => !IsModEnabled_RainMeadow || MeadowIntegration.IsOwner;
    public static bool RainMeadow_IsMine(AbstractPhysicalObject obj)
    {
        return !IsModEnabled_RainMeadow || MeadowIntegration.IsLocal(obj);
    }
}
