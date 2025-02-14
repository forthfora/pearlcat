using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RainMeadow;

namespace Pearlcat;

public class MeadowSaveData : OnlineResource.ResourceData
{
    public bool WasSynced { get; set; }

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
        public string inventorySaveString = "";

        [OnlineField]
        public string activePearlIndexSaveString = "";


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

            inventorySaveString = JsonConvert.SerializeObject(miscWorld.Inventory);
            activePearlIndexSaveString = JsonConvert.SerializeObject(miscWorld.ActiveObjectIndex);
        }

        public override void ReadTo(OnlineResource.ResourceData data, OnlineResource resource)
        {
            var game = Utils.RainWorld.processManager.currentMainLoop as RainWorldGame;

            var miscWorld = game?.GetMiscWorld();

            if (miscWorld is null)
            {
                return;
            }

            if (data is not MeadowSaveData saveData)
            {
                return;
            }

            miscWorld.PlayersGivenPearls = playersGivenPearls;

            var inventory = JsonConvert.DeserializeObject<Dictionary<int, List<string>>>(inventorySaveString);

            if (inventory is not null)
            {
                miscWorld.Inventory = inventory;
            }

            var activePearlIndex = JsonConvert.DeserializeObject<Dictionary<int, int?>>(activePearlIndexSaveString);

            if (activePearlIndex is not null)
            {
                miscWorld.ActiveObjectIndex = activePearlIndex;
            }

            saveData.WasSynced = true;
        }

        public override Type GetDataType()
        {
            return typeof(MeadowSaveData);
        }
    }
}
