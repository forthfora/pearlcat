
namespace Pearlcat;

public record struct PearlEffect()
{
    public MajorEffectType MajorEffect { get; set; } = MajorEffectType.None;

    public enum MajorEffectType
    {
        None,
        SpearCreation, // White (Very Common)
        Agility, // Blue (8 + 2)
        Revive, // Green (5 + 1)
        Shield, // Yellow (3 + 3, common at iterators)
        Rage, // Red (8 + 1)
        Camouflage, // Black (iterators only, but common)
    }

    // When the PO is active, percentages are multiplied by this value before being applied
    public float ActiveMultiplier { get; set; } = 1.0f;


    // Slugcat Stats
    public float LungsFac { get; set; }
    public float ThrowingSkill { get; set; }
    public float RunSpeedFac { get; set; }

    public float CorridorClimbSpeedFac { get; set; }
    public float PoleClimbSpeedFac { get; set; }
    public float BodyWeightFac { get; set; }

    public string? ThreatMusic { get; set; }
}
