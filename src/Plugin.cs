using BepInEx;
using BepInEx.Logging;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Security;
using static Pearlcat.Hooks;
using System.Linq;
using System;
using UnityEngine;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: IgnoresAccessChecksTo("Assembly-CSharp")]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[module: UnverifiableCode]
#pragma warning restore CS0618 // Type or member is obsolete


namespace Pearlcat;

[BepInDependency("slime-cubed.slugbase")] // SlugBase
[BepInDependency("com.rainworldgame.garrakx.crs.mod")] // CRS
[BepInDependency("improved-input-config", BepInDependency.DependencyFlags.SoftDependency)] // Improved Input Config
[BepInDependency("lb-fgf-m4r-ik.chatoyant-waterfalls-but-real", BepInDependency.DependencyFlags.SoftDependency)] // Chasing Wind

[BepInPlugin(MOD_ID, MOD_ID, "1.2.3")]

public class Plugin : BaseUnityPlugin
{
    public const string MOD_ID = "pearlcat";

    public static string MOD_NAME = "";
    public static string VERSION = "";
    public static string AUTHORS = "";

    public new static ManualLogSource Logger { get; private set; } = null!;

    public void OnEnable()
    {
        Logger = base.Logger;
        Hooks.ApplyInit();
    }

    public void Update()
    {
        // Left Control + Left Shift + L
        // Produces a debug log to the console
        var input = Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.L);

        if (input)
        {
            Utils.LogPearlcatDebugInfo();
        }
    }
}
