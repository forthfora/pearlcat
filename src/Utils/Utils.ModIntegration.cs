using System.Linq;

namespace Pearlcat;

public static partial class Utils
{
    public static bool IsWarpAllowed(this RainWorldGame game) => game.IsStorySession && (!ModManager.MSC || !game.rainWorld.safariMode);

    public static bool IsModEnabled_MiraInstallation => ModManager.ActiveMods.Any(x => x.id == "mira");
    public static bool ShowMiraVersionWarning => IsModEnabled_MiraInstallation; // TODO: warn when relevant version

    public static bool IsModEnabled_ChasingWind => ModManager.ActiveMods.Any(x => x.id == "myr.chasing_wind");
    public static void InitCWIntegration() => CWIntegration.Init();

    public static bool IsModEnabled_ImprovedInputConfig => ModManager.ActiveMods.Any(x => x.id == "improved-input-config");
    public static bool IsIICActive => IsModEnabled_ImprovedInputConfig && !ModOptions.DisableImprovedInputConfig.Value;
}
