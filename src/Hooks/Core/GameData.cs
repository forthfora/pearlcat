using SlugBase.Features;

namespace Pearlcat;

public partial class Hooks
{
    public static void ApplyGameDataHooks()
    {
        On.HUD.Map.GetItemInShelterFromWorld += Map_GetItemInShelterFromWorld;
        On.RegionState.AdaptRegionStateToWorld += RegionState_AdaptRegionStateToWorld;
    }

    private static void RegionState_AdaptRegionStateToWorld(On.RegionState.orig_AdaptRegionStateToWorld orig, RegionState self, int playerShelter, int activeGate)
    {


        for (int i = 0; i < self.world.NumberOfRooms; i++)
        {
            var abstractRoom = self.world.GetAbstractRoom(self.world.firstRoomIndex + i);

            for (int j = abstractRoom.entities.Count - 1; j >= 0; j--)
            {
                var entity = abstractRoom.entities[j];

                if (entity is not AbstractPhysicalObject abstractObject) continue;

                if (abstractObject.IsPlayerObject())
                    abstractRoom.RemoveEntity(entity);
            }
        }

        orig(self, playerShelter, activeGate);
    }

    // Prevent Player Pearls being saved in the shelter 
    public static HUD.Map.ShelterMarker.ItemInShelterMarker.ItemInShelterData? Map_GetItemInShelterFromWorld(On.HUD.Map.orig_GetItemInShelterFromWorld orig, World world, int room, int index)
    {
        var abstractRoom = world.GetAbstractRoom(room);

        if (index < abstractRoom.entities.Count && abstractRoom.entities[index] is AbstractPhysicalObject abstractObject)
            if (abstractObject.realizedObject != null && abstractObject.IsPlayerObject())
                return null;

        return orig(world, room, index);
    }

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
    public static readonly GameFeature<float> DazeMaxSpeed = FeatureTypes.GameFloat("oa_daze_max_speed");

    public static readonly GameFeature<float> MaxDist = FeatureTypes.GameFloat("oa_max_dist");
    public static readonly GameFeature<float> MinSpeed = FeatureTypes.GameFloat("oa_min_speed");
    public static readonly GameFeature<float> MaxSpeed = FeatureTypes.GameFloat("oa_max_speed");
}
