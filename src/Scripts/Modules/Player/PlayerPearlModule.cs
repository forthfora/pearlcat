
using System.Runtime.CompilerServices;

namespace Pearlcat;

public class PlayerPearlModule
{
    public bool IsCurrentlyStored { get; set; }
    
    public bool CollideWithObjects { get; set; } = true;
    public bool CollideWithSlopes { get; set; } = true;
    public bool CollideWithTerrain { get; set; } = true;
    public bool PlayCollisionSound { get; set; } = true;

    public int PearlGlimmerWait { get; set; }
    public float WeaponRotationSpeed { get; set; }    
    
    public float Gravity { get; set; } = 1.0f;
    
    public int CurrentCooldownTime { get; set; }
   
    public int LaserTimer { get; set; }
    public float LaserLerp { get; set; }

    public bool InventoryFlash { get; set; }

    public bool IsSentry { get; set; }
    public bool IsReturningSentry { get; set; }

    public ConditionalWeakTable<PhysicalObject, StrongBox<bool>> VisitedObjects = new();


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


    // Special flag for CW Pearl - allows 2 double jumps
    public bool IsCWDoubleJumpUsed { get; set; }
}
