using JetBrains.Annotations;
using RainMeadow;

namespace Pearlcat;

public class MeadowPearlSpearData : OnlineEntity.EntityData
{
    [UsedImplicitly]
    public MeadowPearlSpearData()
    {
    }

    public override EntityDataState MakeState(OnlineEntity entity, OnlineResource inResource)
    {
        return new State(entity);
    }

    public class State : EntityDataState
    {
        [OnlineField]
        public Color color;

        [OnlineField]
        public string pearlType = "";

        [OnlineField]
        public int pebblesColor;

        [OnlineField]
        public bool wasThrown;

        [OnlineField]
        public int sparkTimer;

        [OnlineField]
        public int returnTimer;

        [OnlineField]
        public int decayTimer;


        [UsedImplicitly]
        public State()
        {
        }

        public State(OnlineEntity onlineEntity)
        {
            if ((onlineEntity as OnlinePhysicalObject)?.apo is not AbstractSpear spear)
            {
                return;
            }

            if (!spear.TryGetSpearModule(out var spearModule))
            {
                return;
            }

            color = spearModule.Color;
            pearlType = spearModule.PearlType;
            pebblesColor = spearModule.PebblesColor;
            wasThrown = spearModule.WasThrown;
            sparkTimer = spearModule.SparkTimer;
            returnTimer = spearModule.ReturnTimer;
            decayTimer = spearModule.DecayTimer;
        }

        public override void ReadTo(OnlineEntity.EntityData data, OnlineEntity onlineEntity)
        {
            if ((onlineEntity as OnlinePhysicalObject)?.apo is not AbstractSpear spear)
            {
                return;
            }

            if (!spear.TryGetSpearModule(out var spearModule))
            {
                var miscWorld = spear.world.game.GetMiscWorld();

                spearModule = new SpearModule(color, pearlType);

                if (miscWorld is null)
                {
                    ModuleManager.TempPearlSpearData.Add(spear, spearModule);
                }
                else
                {
                    miscWorld.PearlSpears[spear.ID.number] = spearModule;
                }
            }

            spearModule.Color = color;
            spearModule.PearlType = pearlType;
            spearModule.PebblesColor = pebblesColor;
            spearModule.WasThrown = wasThrown;
            spearModule.SparkTimer = sparkTimer;
            spearModule.ReturnTimer = returnTimer;
            spearModule.DecayTimer = decayTimer;
        }

        public override Type GetDataType()
        {
            return typeof(MeadowPearlSpearData);
        }
    }
}
