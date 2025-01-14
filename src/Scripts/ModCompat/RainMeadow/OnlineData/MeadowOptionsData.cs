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

    [method: UsedImplicitly]
    public class State() : ResourceDataState
    {
        [OnlineField]
        public bool pearlpupRespawn = ModOptions_TEMPNAME.PearlpupRespawn;


        public override void ReadTo(OnlineResource.ResourceData data, OnlineResource resource)
        {
            MeadowOnlineOptions.PearlpupRespawn = pearlpupRespawn;
        }

        public override Type GetDataType()
        {
            return typeof(MeadowOptionsData);
        }
    }
}
