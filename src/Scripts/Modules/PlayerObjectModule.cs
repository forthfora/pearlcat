
namespace Pearlcat;

public class PlayerObjectModule
{
    private int cooldownTimer;

    public bool IsCurrentlyStored { get; set; } = false;

    public float Gravity { get; set; } = 1.0f;
    public bool CollideWithObjects { get; set; } = true;
    public bool CollideWithSlopes { get; set; } = true;
    public bool CollideWithTerrain { get; set; } = true;
    public int PearlGlimmerWait { get; set; }
    public float WeaponRotationSpeed { get; set; }
    public bool PlayCollisionSound { get; set; } = true;

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

    public bool UsedAgility { get; set; }

    public int LaserTimer { get; set; }
    public float LaserLerp { get; set; }
}
