
using RWCustom;
using System;
using UnityEngine;

namespace Pearlcat;

public class PearlpupPearlModule
{
    public WeakReference<DataPearl.AbstractDataPearl> DataPearlRef { get; }

    public PearlpupPearlModule(DataPearl.AbstractDataPearl dataPearl)
    {
        DataPearlRef = new(dataPearl);
    }

    public int TimeCounter = 0;

    public Color MainColor { get; set; }
    public Color HighlightColor { get; set; } = Custom.HSL2RGB(0.0f, 1.0f, 0.5f);
}
