
namespace Pearlcat;

public class PlayerObjectModule
{
    // Attributes to restore
    public float Gravity { get; set; } = 1.0f;

    public bool CollideWithObjects { get; set; } = true;
    public bool CollideWithSlopes { get; set; } = true;
    public bool CollideWithTerrain { get; set; } = true;

    public int PearlGlimmerWait { get; set; }
    public float WeaponRotationSpeed { get; set; }

    public bool PlayCollisionSound { get; set; } = true;
}
