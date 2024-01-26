
namespace Pearlcat;

public class PlayerObjectModule
{
    public bool IsCurrentlyStored { get; set; } = false;
    
    public bool CollideWithObjects { get; set; } = true;
    public bool CollideWithSlopes { get; set; } = true;
    public bool CollideWithTerrain { get; set; } = true;

    public float Gravity { get; set; } = 1.0f;
    
    public int PearlGlimmerWait { get; set; }
    
    public float WeaponRotationSpeed { get; set; }
    
    public bool PlayCollisionSound { get; set; } = true;

    public int CurrentCooldownTime { get; set; }

    public int LaserTimer { get; set; }
    
    public float LaserLerp { get; set; }

    public bool InventoryFlash { get; set; }

    public bool IsSentry { get; set; }

    public bool IsReturningSentry { get; set; }
    

    private int cooldownTimer;
    public int CooldownTimer
    {
        get => cooldownTimer;
        set
        {
            if (cooldownTimer <= 0)
            {
                CurrentCooldownTime = value;
            }

            cooldownTimer = value;
        }
    }


    public void RemoveSentry(AbstractPhysicalObject obj)
    {
        if (IsSentry)
        {
            IsReturningSentry = true;
        }

        IsSentry = false;

        if (obj.TryGetSentry(out var sentry))
        {
            sentry.Destroy();
        }
    }

}
