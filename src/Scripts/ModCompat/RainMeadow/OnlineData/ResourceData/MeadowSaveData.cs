using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RainMeadow;

namespace Pearlcat;

public class MeadowSaveData : OnlineResource.ResourceData
{
    [UsedImplicitly]
    public MeadowSaveData()
    {
    }

    public override ResourceDataState MakeState(OnlineResource inResource)
    {
        return new State();
    }

    public class State : ResourceDataState
    {
        [OnlineField]
        public string inventory = "";


        [UsedImplicitly]
        public State()
        {
            var game = Utils.RainWorld.processManager.currentMainLoop as RainWorldGame;

            var miscWorld = game?.GetMiscWorld();

            if (miscWorld is null)
            {
                return;
            }
        }

        public override void ReadTo(OnlineResource.ResourceData data, OnlineResource resource)
        {
            var game = Utils.RainWorld.processManager.currentMainLoop as RainWorldGame;
        }

        public override Type GetDataType()
        {
            return typeof(MeadowSaveData);
        }
    }
}
