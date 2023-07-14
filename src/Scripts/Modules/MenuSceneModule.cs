using System.Collections.Generic;
using UnityEngine;
using static DataPearl.AbstractDataPearl;

namespace Pearlcat;

public class MenuSceneModule
{
    public MenuSceneModule(List<DataPearlType> pearlTypes, DataPearlType? activePearlType)
    {
        PearlTypes = pearlTypes;
        ActivePearlType = activePearlType;
    }

    public List<DataPearlType> PearlTypes { get; set; } = new();
    public DataPearlType? ActivePearlType { get; set; }

    public Vector2 ActivePearlPos { get; set; }
}