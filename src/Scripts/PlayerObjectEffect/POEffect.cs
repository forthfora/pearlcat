
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
    public float lungsFac;
    public float throwingSkill;
    public float runSpeedFac;

    public float corridorClimbSpeedFac;
    public float poleClimbSpeedFac;
    public float bodyWeightFac;

    public float loudnessFac;
    public float generalVisibilityBonus;
    public float visualStealthInSneakMode;

    // Non Standard
    public float jumpHeightFac;
    public float slideSpeedFac; // Includes pounce distance
    public float rollSpeedFac;

    public float survivalFac;

    public float maulFac;
    public float spearPullFac;
    public float backSpearFac;

    public string? threatMusic;
}
