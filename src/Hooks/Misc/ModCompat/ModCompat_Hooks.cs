namespace Pearlcat;

public static class ModCompat_Hooks
{
    public static void ApplyHooks()
    {
        On.DevInterface.TriggersPage.ctor += TriggersPage_ctor;
        Application.quitting += Application_quitting;
    }


    // Debug log whenever the game exits (crash or otherwise)
    private static void Application_quitting()
    {
        Plugin.LogPearlcatDebugInfo();
    }

    // Fix for DevTools not displaying sounds (credit to Bro for the code)
    private static void TriggersPage_ctor(On.DevInterface.TriggersPage.orig_ctor orig, DevInterface.TriggersPage self, DevInterface.DevUI owner, string IDstring, DevInterface.DevUINode parentNode, string name)
    {
        orig(self, owner, IDstring, parentNode, name);

        List<string> songs = [];

        var files = AssetManager.ListDirectory("Music" + Path.DirectorySeparatorChar + "Songs");

        foreach (var file in files)
        {
            var noExtension = Path.GetFileNameWithoutExtension(file);

            if (!songs.Contains(noExtension) && Path.GetExtension(file).ToLower() != ".meta")
            {
                songs.Add(noExtension);
            }
        }

        self.songNames = songs.ToArray();
    }
}
