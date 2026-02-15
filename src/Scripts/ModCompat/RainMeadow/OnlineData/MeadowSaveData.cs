using JetBrains.Annotations;
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
        try
        {
            return new State();
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"Failed to make state: {e}");
            return null!;
        }
    }

    public class State : ResourceDataState
    {
        [OnlineField]
        public List<int> playersGivenPearls;

        [OnlineField]
        public string inventorySaveString;

        [OnlineField]
        public string activePearlIndexSaveString;
        
        [OnlineField]
        public bool isValid; // need to know whether we're sending valid data or a blank (prob a way to pause the state being sent but yeah, been a while)


        [UsedImplicitly]
        public State()
        {
            var game = Utils.RainWorld.processManager.currentMainLoop as RainWorldGame;

            var miscWorld = game?.GetMiscWorld();

            if (miscWorld is null)
            {
                playersGivenPearls = [];
                inventorySaveString = "";
                activePearlIndexSaveString = "";
                isValid = false;
                return;
            }

            playersGivenPearls = miscWorld.PlayersGivenPearls;
            
            inventorySaveString = JsonConvert.SerializeObject(miscWorld.Inventory);
            activePearlIndexSaveString = JsonConvert.SerializeObject(miscWorld.ActiveObjectIndex);
            
            isValid = true;
        }

        public override void ReadTo(OnlineResource.ResourceData data, OnlineResource resource)
        {
            // i.e. miscWorld is null on the sender
            if (!isValid)
            {
                return;
            }
            
            var game = Utils.RainWorld.processManager.currentMainLoop as RainWorldGame;

            var miscWorld = game?.GetMiscWorld();

            if (miscWorld is null)
            {
                // we don't care if miscWorld is null on the receiver we just reject it gracefully
                return;
            }

            if (data is not MeadowSaveData saveData)
            {
                throw new Exception("Data is not MeadowSaveData");
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
