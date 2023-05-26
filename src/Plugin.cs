using BepInEx;
using BepInEx.Logging;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Security;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: IgnoresAccessChecksTo("Assembly-CSharp")]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[module: UnverifiableCode]
#pragma warning restore CS0618 // Type or member is obsolete


namespace TheSacrifice;

[BepInPlugin(AUTHOR + "." + MOD_ID, MOD_NAME, VERSION)]
public class Plugin : BaseUnityPlugin
{
    public const string VERSION = "0.0.0";
    public const string MOD_ID = "thesacrifice";
    public const string AUTHOR = "forthbridge";
    public const string MOD_NAME = "The Sacrifice";

    public const string SLUGCAT_ID = "Sacrifice";

    public static new ManualLogSource Logger { get; private set; } = null!;

    public void OnEnable()
    {
        Logger = base.Logger;
        Hooks.ApplyHooks();
    }
}
