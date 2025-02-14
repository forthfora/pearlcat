using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RainMeadow;

namespace Pearlcat;

public class MeadowSaveData : OnlineResource.ResourceData
{
    // Whether this state has been ReadTo, so we don't try and load save data too early - IsHost check as host is always 'in sync' with itself
    public bool WasSynced { get; set; } = ModCompat_Helpers.RainMeadow_IsHost;


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
