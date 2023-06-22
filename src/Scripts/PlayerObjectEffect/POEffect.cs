
namespace Pearlcat;

public struct POEffect
{
    public POEffect() { }

    public MajorEffect majorEffect = MajorEffect.NONE;

    public enum MajorEffect
    {
        NONE,
        SPEAR_CREATION, // White (Very Common)
        AGILITY, // Blue (8 + 2)
        REVIVE, // Green (5 + 1)
        SHIELD, // Yellow (3 + 3, common at iterators)
        RAGE, // Red (8 + 1)
        CAMOFLAGUE, // Black (iterators only, but common)
    }

    // When the PO is active, percentages are multiplied by this value before being applied
    public float activeMultiplier = 2.0f;


    // Slugcat Stats
    public float lungsFac = 0.0f;
    public float throwingSkill = 0.0f;
    public float runSpeedFac = 0.0f;

    public float corridorClimbSpeedFac = 0.0f;
    public float poleClimbSpeedFac = 0.0f;
    public float bodyWeightFac = 0.0f;

    public float loudnessFac = 0.0f;
    public float generalVisibilityBonus = 0.0f;
    public float visualStealthInSneakMode = 0.0f;

    // Non Standard
    public float jumpHeightFac = 0.0f;
    public float slideSpeedFac = 0.0f; // Includes pounce distance
    public float rollSpeedFac = 0.0f;

    public float survivalFac = 0.0f;

    public float maulFac = 0.0f;
    public float spearPullFac = 0.0f;
    public float backSpearFac = 0.0f;

    public string? threatMusic;
}
