using System;

namespace Pearlcat;

public class PearlpupPearlModule
{
    public WeakReference<DataPearl.AbstractDataPearl> DataPearlRef { get; }
    public WeakReference<Player>? OwnerRef { get; } 

    public PearlpupPearlModule(DataPearl.AbstractDataPearl dataPearl)
    {
        DataPearlRef = new(dataPearl);

        var owner = dataPearl.TryGetPlayerObjectOwner();

        if (owner != null)
        {
            OwnerRef = new(owner);
        }
    }

    public float HeartRateMult { get; set; } = 1.0f;

    public int HeartBeatTime => (int)(80 * HeartRateMult);
    public int HeartBeatTimer1 { get; set; } = 0;
    public int HeartBeatTimer2 { get; set; } = 10;
}
