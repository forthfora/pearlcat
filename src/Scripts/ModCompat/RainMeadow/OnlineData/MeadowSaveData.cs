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
        public List<int> playersGivenPearls = [];

        [OnlineField]
        public string inventory = "";

        [OnlineField]
        public string activePearlIndex = "";


        [UsedImplicitly]
        public State()
        {
            var game = Utils.RainWorld.processManager.currentMainLoop as RainWorldGame;

            var miscWorld = game?.GetMiscWorld();

            if (miscWorld is null)
            {
                return;
            }

            playersGivenPearls = miscWorld.PlayersGivenPearls;

            inventory = JsonConvert.SerializeObject(miscWorld.Inventory);
            activePearlIndex = JsonConvert.SerializeObject(miscWorld.ActivePearlIndex);
        }

        public override void ReadTo(OnlineResource.ResourceData data, OnlineResource resource)
        {
            var game = Utils.RainWorld.processManager.currentMainLoop as RainWorldGame;

            var miscWorld = game?.GetMiscWorld();

            if (miscWorld is null)
            {
                return;
            }

            miscWorld.PlayersGivenPearls = playersGivenPearls;

            miscWorld.Inventory = JsonConvert.DeserializeObject<Dictionary<int, List<string>>>(inventory)!;
            miscWorld.ActivePearlIndex = JsonConvert.DeserializeObject<Dictionary<int, int?>>(activePearlIndex)!;
        }

        public override Type GetDataType()
        {
            return typeof(MeadowSaveData);
        }
    }
}
