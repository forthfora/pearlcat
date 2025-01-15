using JetBrains.Annotations;
using System;
using RainMeadow;

namespace Pearlcat;

public class MeadowOptionsData : OnlineResource.ResourceData
{
    [UsedImplicitly]
    public MeadowOptionsData()
    {
    }

    public override ResourceDataState MakeState(OnlineResource inResource)
    {
        return new State();
    }


    public class State : ResourceDataState
    {
        [OnlineField]
        public bool pearlpupRespawn = ModOptions.PearlpupRespawn;


        [OnlineField]
        public int maxPearlCount = ModOptions.MaxPearlCount;

        [OnlineField]
        public int visibilityMultiplier = ModOptions.VisibilityMultiplier;

        [OnlineField]
        public bool enableBackSpear = ModOptions.EnableBackSpear;


        [OnlineField]
        public bool disableMinorEffects = ModOptions.DisableMinorEffects;

        [OnlineField]
        public bool disableSpear = ModOptions.DisableSpear;

        [OnlineField]
        public bool disableRevive = ModOptions.DisableRevive;

        [OnlineField]
        public bool disableAgility = ModOptions.DisableAgility;

        [OnlineField]
        public bool disableRage = ModOptions.DisableRage;

        [OnlineField]
        public bool disableShield = ModOptions.DisableShield;

        [OnlineField]
        public bool disableCamoflague = ModOptions.DisableCamoflague;


        [OnlineField]
        public bool inventoryOverride = ModOptions.InventoryOverride;

        [OnlineField]
        public bool startingInventoryOverride = ModOptions.StartingInventoryOverride;


        [OnlineField]
        public int shieldRechargeTime = ModOptions.ShieldRechargeTime;

        [OnlineField]
        public int shieldDuration = ModOptions.ShieldDuration;


        [OnlineField]
        public float laserDamage = ModOptions.LaserDamage;

        [OnlineField]
        public int laserWindupTime = ModOptions.LaserWindupTime;

        [OnlineField]
        public int laserRechargeTime = ModOptions.LaserRechargeTime;


        [OnlineField]
        public static bool oldRedPearlAbility = ModOptions.OldRedPearlAbility;


        [UsedImplicitly]
        public State()
        {
        }

        public override void ReadTo(OnlineResource.ResourceData data, OnlineResource resource)
        {
            MeadowOnlineOptions.PearlpupRespawn = pearlpupRespawn;

            MeadowOnlineOptions.MaxPearlCount = maxPearlCount;
            MeadowOnlineOptions.VisibilityMultiplier = visibilityMultiplier;
            MeadowOnlineOptions.EnableBackSpear = enableBackSpear;

            MeadowOnlineOptions.DisableMinorEffects = disableMinorEffects;
            MeadowOnlineOptions.DisableSpear = disableSpear;
            MeadowOnlineOptions.DisableRevive = disableRevive;
            MeadowOnlineOptions.DisableAgility = disableAgility;
            MeadowOnlineOptions.DisableRage = disableRage;
            MeadowOnlineOptions.DisableShield = disableShield;
            MeadowOnlineOptions.DisableCamoflague = disableCamoflague;

            MeadowOnlineOptions.InventoryOverride = inventoryOverride;
            MeadowOnlineOptions.StartingInventoryOverride = startingInventoryOverride;

            MeadowOnlineOptions.ShieldRechargeTime = shieldRechargeTime;
            MeadowOnlineOptions.ShieldDuration = shieldDuration;

            MeadowOnlineOptions.LaserDamage = laserDamage;
            MeadowOnlineOptions.LaserWindupTime = laserWindupTime;
            MeadowOnlineOptions.LaserRechargeTime = laserRechargeTime;

            MeadowOnlineOptions.OldRedPearlAbility = oldRedPearlAbility;
        }

        public override Type GetDataType()
        {
            return typeof(MeadowOptionsData);
        }
    }
}
