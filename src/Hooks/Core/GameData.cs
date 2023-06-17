using SlugBase.Features;

namespace Pearlcat;

public partial class Hooks
{
    // OA MoveToTargetPos
    // Slow Down
    public static readonly GameFeature<float> MinFricSpeed = FeatureTypes.GameFloat("oa_min_fric_speed");
    public static readonly GameFeature<float> MaxFricSpeed = FeatureTypes.GameFloat("oa_max_fric_speed");
    public static readonly GameFeature<float> MinFric = FeatureTypes.GameFloat("oa_min_fric_mult");
    public static readonly GameFeature<float> MaxFric = FeatureTypes.GameFloat("oa_max_fric_mult");

    // Move to Target
    public static readonly GameFeature<float> CutoffDist = FeatureTypes.GameFloat("oa_cutoff_dist");
    public static readonly GameFeature<float> CutoffMinSpeed = FeatureTypes.GameFloat("oa_cutoff_min_speed");
    public static readonly GameFeature<float> CutoffMaxSpeed = FeatureTypes.GameFloat("oa_cutoff_max_speed");

    public static readonly GameFeature<float> MaxDist = FeatureTypes.GameFloat("oa_max_dist");
    public static readonly GameFeature<float> MinSpeed = FeatureTypes.GameFloat("oa_min_speed");
    public static readonly GameFeature<float> MaxSpeed = FeatureTypes.GameFloat("oa_max_speed");
}
