using System.Collections.Generic;
using UnityEngine;

namespace Pearlcat;

public sealed class PearlAnimation_SineWaveWeave(Player player) : PearlAnimation_SineBase(player)
{
    protected override float YPeriod => 90.0f;
}
