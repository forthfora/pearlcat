
using System.Collections.Generic;

namespace Pearlcat;

public class SSOracleModule
{
    public DataPearl? PearlToRead { get; set; }
    public DataPearl? PearlBeingRead { get; set; }
    public DataPearl? PearlBeingReturned { get; set; }

    public int Rand { get; set; }

    public bool WasPearlAlreadyRead { get; set; }
    public List<DataPearl> PearlsHeldByPlayer { get; } = new();
}
