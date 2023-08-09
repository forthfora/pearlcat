
namespace Pearlcat;

public class PlayerObjectModule
{
    public bool IsCurrentlyStored { get; set; } = false;

    public float Gravity { get; set; } = 1.0f;
    public bool CollideWithObjects { get; set; } = true;
    public bool CollideWithSlopes { get; set; } = true;
    public bool CollideWithTerrain { get; set; } = true;
    public int PearlGlimmerWait { get; set; }
    public float WeaponRotationSpeed { get; set; }
    public bool PlayCollisionSound { get; set; } = true;

    private int cooldownTimer;
    public int CooldownTimer
    {
        get => cooldownTimer;
        set
        {
            if (cooldownTimer <= 0)
                CurrentCooldownTime = value;

            cooldownTimer = value;
        }
    }
    public int CurrentCooldownTime { get; set; }

    public int LaserTimer { get; set; }
    public float LaserLerp { get; set; }

    public bool InventoryFlash { get; set; }

    public bool IsSentry { get; set; }
    public void RemoveSentry(AbstractPhysicalObject obj)
    {
        if (IsSentry)
            IsReturningSentry = true;

        IsSentry = false;

        if (POSentry.SentryData.TryGetValue(obj, out var sentry))
            sentry.Destroy();
    }

    public bool IsReturningSentry { get; set; }
}
