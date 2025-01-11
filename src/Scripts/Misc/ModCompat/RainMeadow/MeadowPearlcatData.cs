using JetBrains.Annotations;
using RainMeadow;

namespace Pearlcat;

public class MeadowPearlcatData : OnlineEntity.EntityData
{
    public override EntityDataState MakeState(OnlineEntity entity, OnlineResource inResource)
    {
        return new MeadowPearlcatState(this, entity);
    }
}
