using System.Runtime.CompilerServices;

namespace Pearlcat;

public class SSOracleModule
{
    public DataPearl? PearlToRead { get; set; }
    public DataPearl? PearlBeingRead { get; set; }
    public DataPearl? PearlToReturn { get; set; }
    public WeakReference<Player>? PlayerToReturnTo { get; set; }

    public int Rand { get; set; }

    public bool WasPearlAlreadyRead { get; set; }
    public ConditionalWeakTable<DataPearl, Player> PearlsHeldByPlayer { get; } = new();

    public int TakeRMTimer { get; set; } = 120;
    public int GiveSSTimer { get; set; } = 60;
}
