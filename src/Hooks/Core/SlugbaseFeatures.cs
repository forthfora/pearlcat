using SlugBase;
using SlugBase.Features;
using System.Collections.Generic;
using UnityEngine;

namespace Pearlcat;

public static partial class Hooks
{
    public static readonly GameFeature<float> TrainViewYShift = FeatureTypes.GameFloat("train_view_yshift");

    public static readonly PlayerFeature<Vector2> ActiveObjectOffset = new("active_object_offset", Vector2Feature);
    public static readonly PlayerFeature<int> DazeDuration = FeatureTypes.PlayerInt("daze_duration");

    public static readonly PlayerFeature<int> MinOATime = FeatureTypes.PlayerInt("oa_min_time");
    public static readonly PlayerFeature<int> MaxOATime = FeatureTypes.PlayerInt("oa_max_time");

    static readonly PlayerFeature<float> MinEffectiveOffset = FeatureTypes.PlayerFloat("min_tail_offset");
    static readonly PlayerFeature<float> MaxEffectiveOffset = FeatureTypes.PlayerFloat("max_tail_offset");

    static readonly PlayerFeature<Dictionary<int, Vector2>> TailSegmentVelocities = new("tail_segment_velocities", IntVector2Feature);

    static readonly PlayerFeature<Vector2> EarLOffset = new("ear_l_offset", Vector2Feature);
    static readonly PlayerFeature<Vector2> EarROffset = new("ear_r_offset", Vector2Feature);

    public static readonly PlayerFeature<float> MinFricSpeed = FeatureTypes.PlayerFloat("oa_min_fric_speed");
    public static readonly PlayerFeature<float> MaxFricSpeed = FeatureTypes.PlayerFloat("oa_max_fric_speed");
    public static readonly PlayerFeature<float> MinFric = FeatureTypes.PlayerFloat("oa_min_fric_mult");
    public static readonly PlayerFeature<float> MaxFric = FeatureTypes.PlayerFloat("oa_max_fric_mult");

    public static readonly PlayerFeature<float> CutoffDist = FeatureTypes.PlayerFloat("oa_cutoff_dist");
    public static readonly PlayerFeature<float> CutoffMinSpeed = FeatureTypes.PlayerFloat("oa_cutoff_min_speed");
    public static readonly PlayerFeature<float> CutoffMaxSpeed = FeatureTypes.PlayerFloat("oa_cutoff_max_speed");
    public static readonly PlayerFeature<float> DazeMaxSpeed = FeatureTypes.PlayerFloat("oa_daze_max_speed");

    public static readonly PlayerFeature<float> MaxDist = FeatureTypes.PlayerFloat("oa_max_dist");
    public static readonly PlayerFeature<float> MinSpeed = FeatureTypes.PlayerFloat("oa_min_speed");
    public static readonly PlayerFeature<float> MaxSpeed = FeatureTypes.PlayerFloat("oa_max_speed");

    public static readonly PlayerFeature<Vector2> InventoryUIOffset = new("inventory_ui_offset", Vector2Feature);

    public static readonly PlayerFeature<int> StoreObjectDelay = FeatureTypes.PlayerInt("store_object_delay");


    public static Vector2 Vector2Feature(JsonAny json)
    {
        var jsonList = json.AsList();
        return new(jsonList[0].AsFloat(), jsonList[1].AsFloat());
    }

    public static Dictionary<int, Vector2> IntVector2Feature(JsonAny json)
    {
        var result = new Dictionary<int, Vector2>();

        foreach (var segmentVelocityPair in json.AsObject())
        {
            var velocities = segmentVelocityPair.Value.AsList();

            if (velocities.Count < 2) continue;

            var velocity = new Vector2(velocities[0].AsFloat(), velocities[1].AsFloat());

            if (!int.TryParse(segmentVelocityPair.Key, out var segmentIndex)) continue;

            result[segmentIndex] = velocity;
        }

        return result;
    }

}
