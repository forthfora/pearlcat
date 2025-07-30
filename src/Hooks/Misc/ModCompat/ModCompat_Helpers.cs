namespace Pearlcat;

public static class ModCompat_Helpers
{
    public static void InitModCompat()
    {
        if (IsModEnabled_ImprovedInputConfig)
        {
            try
            {
                // Needs a buffer method as there are statics in the IICCompat class which reference the DLL
                RefreshIICConfigs();
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError($"Error initializing Improved Input Config compat:\n{e}");
            }
        }

        if (IsModEnabled_ChasingWind)
        {
            try
            {
                CWCompat.InitCompat();
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError($"Error initializing Chasing Wind compat:\n{e}");
            }
        }

        if (IsModEnabled_RainMeadow)
        {
            try
            {
                MeadowCompat.InitCompat();
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError($"Error initializing Rain Meadow compat:\n{e}");
            }
        }
    }


    // The Well
    public static bool IsModEnabled_TheWell => ModManager.ActiveMods.Any(x => x.id == "thewell");
    public static bool ShowTheWellVersionWarning => IsModEnabled_TheWell; // TODO


    // Chasing Wind
    public static bool IsModEnabled_ChasingWind => ModManager.ActiveMods.Any(x => x.id == "myr.chasing_wind");


    // Improved Input Config
    public static bool IsModEnabled_ImprovedInputConfig => ModManager.ActiveMods.Any(x => x.id == "improved-input-config");
    public static bool IsIICActive => IsModEnabled_ImprovedInputConfig && !ModOptions.DisableImprovedInputConfig;

    public static void InitIICCompat()
    {
        _ = IICCompat.StoreKeybind;
    }

    public static void RefreshIICConfigs()
    {
        IICCompat.RefreshConfigs();
    }


    // Rain Meadow
    public static bool IsModEnabled_RainMeadow => ModManager.ActiveMods.Any(x => x.id == "henpemaz_rainmeadow");

    public static bool RainMeadow_IsHost => !IsModEnabled_RainMeadow || MeadowCompat.IsHost;
    public static bool RainMeadow_IsOnline => IsModEnabled_RainMeadow && MeadowCompat.IsOnline;

    public static bool RainMeadow_IsMine(AbstractPhysicalObject obj)
    {
        return !RainMeadow_IsOnline || MeadowCompat.IsMine(obj);
    }

    public static bool RainMeadow_IsPosSynced(AbstractPhysicalObject obj)
    {
        return RainMeadow_IsOnline && MeadowCompat.IsPosSynced(obj);
    }

    public static int? RainMeadow_GetOwnerIdOrNull(AbstractPhysicalObject obj)
    {
        return RainMeadow_IsOnline ? MeadowCompat.GetOwnerId(obj) : null;
    }
}
