using System.Collections.Generic;
using UnityEngine;

namespace Pearlcat;

using StoredPearlData = SaveMiscProgression.StoredPearlData;

public class MenuSceneModule(List<StoredPearlData> nonActivePearls, StoredPearlData? activePearl)
{
    public List<StoredPearlData> NonActivePearls { get; set; } = nonActivePearls;
    public StoredPearlData? ActivePearl { get; set; } = activePearl;
    public Vector2 ActivePearlPos { get; set; }
}
