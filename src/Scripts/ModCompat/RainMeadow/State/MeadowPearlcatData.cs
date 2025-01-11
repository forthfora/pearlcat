using JetBrains.Annotations;
using RainMeadow;

namespace Pearlcat;

public class MeadowPearlcatData : OnlineEntity.EntityData
{
    [UsedImplicitly]
    public MeadowPearlcatData()
    {
    }

    public override EntityDataState MakeState(OnlineEntity entity, OnlineResource inResource)
    {
        return new MeadowPearlcatState(this, entity);
    }
}
