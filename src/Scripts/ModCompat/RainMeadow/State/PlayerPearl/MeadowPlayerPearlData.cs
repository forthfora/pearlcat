using JetBrains.Annotations;
using RainMeadow;

namespace Pearlcat;

public class MeadowPlayerPearlData : OnlineEntity.EntityData
{
    [UsedImplicitly]
    public MeadowPlayerPearlData()
    {
    }

    public override EntityDataState MakeState(OnlineEntity entity, OnlineResource inResource)
    {
        return new MeadowPlayerPearlState(this, entity);
    }
}
